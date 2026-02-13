using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using MQContract.Attributes;
using MQContract.Helpers;
using MQContract.Interfaces;
using MQContract.Interfaces.Consumers;
using MQContract.Loggers;
using MQContract.Messages;
using MQContract.Middleware;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.Loader;

namespace MQContract.Connections
{
#pragma warning disable S3881 // "IDisposable" should be implemented correctly
    internal abstract partial class AConnection<TContractConnection> : IMetricContractConnection<TContractConnection>
#pragma warning restore S3881 // "IDisposable" should be implemented correctly
        where TContractConnection : IBaseContractConnection
    {
        private const string ConsumerClassNameKey = $"{OpenTelemetryMiddleware.KeyBase}.consumerclass";
        private async ValueTask<TContractConnection> RegisterSubscription(Func<string?, string?, bool, ValueTask<ISubscription>> createSubscription,
            string? channel, string? group, bool ignoreMessageHeader, string consumerName, Type consumerType)
        {
            ISubscription subscription;
            var consumerAttribute = Utility.GetCustomAttribute<ConsumerAttribute>(consumerType);
            try
            {
                subscription = await createSubscription(
                    channel??consumerAttribute?.Channel,
                    group??consumerAttribute?.Group,
                    ignoreMessageHeader||(consumerAttribute?.IgnoreMessageTypeHeader??false)
                );
            }
            catch (Exception err)
            {
                BaseLog.ConsumerRegistrationFailed(Logger, err, consumerName, consumerType);
                throw new ConsumerRegistrationFailedException(consumerName, consumerType, err);
            }
            consumerSubscriptions.Add(subscription);
            return (TContractConnection)(IBaseContractConnection)this;
        }

        async ValueTask<TContractConnection> IMessageContextContractConnection<TContractConnection>.RegisterMessageContextAsync(MQContractMessageContext messageContext)
        {
            this.messageContext.RegisterContext(messageContext);
            await middleware.RegisterMessageContextAsync(messageContext);
            return (TContractConnection)(IBaseContractConnection)this;
        }

        private readonly ConcurrentBag<Assembly> loadedAssemblies = [];

        private async Task LoadConsumersForAssemblyAsync(Assembly assembly, CancellationToken cancellationToken)
        {
            if (!loadedAssemblies.Contains(assembly))
            {
                loadedAssemblies.Add(assembly);
                await ConsumerConnectionHelper.LoadConsumersForAssemblyAsync<TContractConnection>(this, serviceProvider, assembly, cancellationToken);
            }
        }

        [RequiresDynamicCode("Uses reflection, MakeGenericMethod, and MethodInfo.Invoke. Not compatible with NativeAOT.")]
        [RequiresUnreferencedCode("Uses reflection over generic methods and runtime types.")]
        async ValueTask IConsumerContractConnection<TContractConnection>.AutoRegisterAllConsumersAsync(Assembly? assembly, CancellationToken cancellationToken)
        {
            DynamicCodeNotSupportedException.ThrowIfDynamicCodeIsBlocked("Unable Auto Register consumers without reflection");
            if (assembly!=null)
                await LoadConsumersForAssemblyAsync(assembly!, cancellationToken);
            else
                await Task.WhenAll(AssemblyLoadContext.Default.Assemblies.Select(asm => LoadConsumersForAssemblyAsync(asm, cancellationToken)));
        }

        private IHealthCheck? healthCheck;
        protected abstract ConnectionHealthCheck? ProduceConnectionHealthCheck();
        IHealthCheck? IBaseContractConnection.HealthCheck => healthCheck??=ProduceConnectionHealthCheck();

        private static MessageFilters<TMessage>? ExtractFilter<TMessage, TConsumer>(TConsumer consumer)
        {
            Func<MessageHeader, ValueTask<MessageFilterResult>>? headerFilter = null;
            Func<TMessage, MessageHeader, ValueTask<MessageFilterResult>>? messageFilter = null;
            if (consumer is IHeaderFilteredConsumer headerFilteredConsumer)
                headerFilter = headerFilteredConsumer.Filter;
            if (consumer is IMessageFilteredConsumer<TMessage> messageFilteredConsumer)
                messageFilter = messageFilteredConsumer.Filter;
            if (headerFilter!=null || messageFilter!=null)
                return new(headerFilter, messageFilter);
            return null;
        }

        #region PubSubConsumer
        ValueTask<TContractConnection> IConsumerContractConnection<TContractConnection>.RegisterPubSubConsumerAsync<TMessage, TConsumer>(TConsumer consumer, string? channel, string? group, bool ignoreMessageHeader, MessageFilters<TMessage>? messageFilters, CancellationToken cancellationToken)
            => RegisterSubscription((channel, group, ignoreMessageHeader) => CreateSubscriptionAsync<TMessage>(
                    (message) =>
                    {
                        message.Activity?.AddTag(ConsumerClassNameKey, consumer.GetType().Name);
                        consumer.MessageReceived(message);
                        return ValueTask.CompletedTask;
                    },
                    (error) => consumer.ErrorRecieved(error),
                    channel,
                    group,
                    ignoreMessageHeader,
                    messageFilters??AConnection<TContractConnection>.ExtractFilter<TMessage, TConsumer>(consumer),
                    true,
                    cancellationToken
                ),
                channel,
                group,
                ignoreMessageHeader,
                "PubSubConsumer",
                consumer.GetType());

        ValueTask<TContractConnection> IConsumerContractConnection<TContractConnection>.RegisterPubSubConsumerAsync<TMessage, TConsumer>(string? channel, string? group, bool ignoreMessageHeader, MessageFilters<TMessage>? messageFilters, CancellationToken cancellationToken)
            => ((IConsumerContractConnection<TContractConnection>)this).RegisterPubSubConsumerAsync<TMessage, TConsumer>(
                (serviceProvider==null ? Activator.CreateInstance<TConsumer>() : ActivatorUtilities.CreateInstance<TConsumer>(serviceProvider)),
                channel,
                group,
                ignoreMessageHeader,
                messageFilters,
                cancellationToken: cancellationToken
            );

        ValueTask<TContractConnection> IConsumerContractConnection<TContractConnection>.RegisterPubSubConsumerAsync(Type consumerType, string? channel, string? group, bool ignoreMessageHeader, CancellationToken cancellationToken)
            => ConsumerConnectionHelper.RegisterPubSubConsumerAsync<TContractConnection>(this, serviceProvider, consumerType, channel, group, ignoreMessageHeader, cancellationToken);

        #endregion

        #region PubSubAsyncConsumer
        ValueTask<TContractConnection> IConsumerContractConnection<TContractConnection>.RegisterPubSubAsyncConsumerAsync<TMessage, TConsumer>(TConsumer consumer, string? channel, string? group, bool ignoreMessageHeader, MessageFilters<TMessage>? messageFilters, CancellationToken cancellationToken)
            => RegisterSubscription((channel, group, ignoreMessageHeader) => CreateSubscriptionAsync<TMessage>(
                    (message) =>
                    {
                        message.Activity?.AddTag(ConsumerClassNameKey, consumer.GetType().Name);
                        return consumer.MessageReceivedAsync(message);
                    },
                    (error) => consumer.ErrorRecieved(error),
                    channel,
                    group,
                    ignoreMessageHeader,
                    messageFilters??AConnection<TContractConnection>.ExtractFilter<TMessage, TConsumer>(consumer),
                    false,
                    cancellationToken
                ),
                channel,
                group,
                ignoreMessageHeader,
                "PubSubAsyncConsumer",
                consumer.GetType());

        ValueTask<TContractConnection> IConsumerContractConnection<TContractConnection>.RegisterPubSubAsyncConsumerAsync<TMessage, TConsumer>(string? channel, string? group, bool ignoreMessageHeader, MessageFilters<TMessage>? messageFilters, CancellationToken cancellationToken)
            => ((IConsumerContractConnection<TContractConnection>)this).RegisterPubSubAsyncConsumerAsync<TMessage, TConsumer>(
                (serviceProvider==null ? Activator.CreateInstance<TConsumer>() : ActivatorUtilities.CreateInstance<TConsumer>(serviceProvider)),
                channel,
                group,
                ignoreMessageHeader,
                messageFilters,
                cancellationToken: cancellationToken
            );

        ValueTask<TContractConnection> IConsumerContractConnection<TContractConnection>.RegisterPubSubAsyncConsumerAsync(Type consumerType, string? channel, string? group, bool ignoreMessageHeader, CancellationToken cancellationToken)
            => ConsumerConnectionHelper.RegisterPubSubAsyncConsumerAsync<TContractConnection>(this, serviceProvider, consumerType, channel, group, ignoreMessageHeader, cancellationToken);
        #endregion

        #region QueryResponseConsumer
        ValueTask<TContractConnection> IConsumerContractConnection<TContractConnection>.RegisterQueryResponseConsumerAsync<TQuery, TQueryResponse, TConsumer>(TConsumer consumer, string? channel, string? group, bool ignoreMessageHeader, MessageFilters<TQuery>? messageFilters, CancellationToken cancellationToken)
            => RegisterSubscription((channel, group, ignoreMessageHeader) => ProduceSubscribeQueryResponseAsync<TQuery, TQueryResponse>(
                    (message) =>
                    {
                        message.Activity?.AddTag(ConsumerClassNameKey, consumer.GetType().Name);
                        return ValueTask.FromResult(consumer.MessageReceived(message));
                    },
                    (error) => consumer.ErrorRecieved(error),
                    channel,
                    group,
                    ignoreMessageHeader,
                    true,
                    messageFilters??AConnection<TContractConnection>.ExtractFilter<TQuery, TConsumer>(consumer),
                    cancellationToken
                ),
                channel,
                group,
                ignoreMessageHeader,
                "QueryResponseConsumer",
                consumer.GetType());

        ValueTask<TContractConnection> IConsumerContractConnection<TContractConnection>.RegisterQueryResponseConsumerAsync<TQuery, TQueryResponse, TConsumer>(string? channel, string? group, bool ignoreMessageHeader, MessageFilters<TQuery>? messageFilters, CancellationToken cancellationToken)
            => ((IConsumerContractConnection<TContractConnection>)this).RegisterQueryResponseConsumerAsync<TQuery, TQueryResponse, TConsumer>(
                (serviceProvider==null ? Activator.CreateInstance<TConsumer>() : ActivatorUtilities.CreateInstance<TConsumer>(serviceProvider)),
                channel,
                group,
                ignoreMessageHeader,
                messageFilters,
                cancellationToken
            );

        ValueTask<TContractConnection> IConsumerContractConnection<TContractConnection>.RegisterQueryResponseConsumerAsync(Type consumerType, string? channel, string? group, bool ignoreMessageHeader, CancellationToken cancellationToken)
            => ConsumerConnectionHelper.RegisterQueryResponseConsumerAsync<TContractConnection>(this, serviceProvider, consumerType, channel, group, ignoreMessageHeader, cancellationToken);
        #endregion

        #region QueryResponseAsyncConsumer
        ValueTask<TContractConnection> IConsumerContractConnection<TContractConnection>.RegisterQueryResponseAsyncConsumerAsync<TQuery, TQueryResponse, TConsumer>(TConsumer consumer, string? channel, string? group, bool ignoreMessageHeader, MessageFilters<TQuery>? messageFilters, CancellationToken cancellationToken)
            => RegisterSubscription((channel, group, ignoreMessageHeader) => ProduceSubscribeQueryResponseAsync<TQuery, TQueryResponse>(
                    (message) =>
                    {
                        message.Activity?.AddTag(ConsumerClassNameKey, consumer.GetType().Name);
                        return consumer.MessageReceivedAsync(message);
                    },
                    (error) => consumer.ErrorRecieved(error),
                    channel,
                    group,
                    ignoreMessageHeader,
                    false,
                    messageFilters??AConnection<TContractConnection>.ExtractFilter<TQuery, TConsumer>(consumer),
                    cancellationToken
                ),
                channel,
                group,
                ignoreMessageHeader,
                "QueryResponseAsyncConsumer",
                consumer.GetType());

        ValueTask<TContractConnection> IConsumerContractConnection<TContractConnection>.RegisterQueryResponseAsyncConsumerAsync<TQuery, TQueryResponse, TConsumer>(string? channel, string? group, bool ignoreMessageHeader, MessageFilters<TQuery>? messageFilters, CancellationToken cancellationToken)
            => ((IConsumerContractConnection<TContractConnection>)this).RegisterQueryResponseAsyncConsumerAsync<TQuery, TQueryResponse, TConsumer>(
                (serviceProvider==null ? Activator.CreateInstance<TConsumer>() : ActivatorUtilities.CreateInstance<TConsumer>(serviceProvider)),
                channel,
                group,
                ignoreMessageHeader,
                messageFilters,
                cancellationToken
            );

        ValueTask<TContractConnection> IConsumerContractConnection<TContractConnection>.RegisterQueryResponseAsyncConsumerAsync(Type consumerType, string? channel, string? group, bool ignoreMessageHeader, CancellationToken cancellationToken)
            => ConsumerConnectionHelper.RegisterQueryResponseAsyncConsumerAsync<TContractConnection>(this, serviceProvider, consumerType, channel, group, ignoreMessageHeader, cancellationToken);
        #endregion
    }
}

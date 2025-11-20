using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using MQContract.Attributes;
using MQContract.Extensions;
using MQContract.Interfaces;
using MQContract.Interfaces.Consumers;
using MQContract.Messages;
using MQContract.Middleware;
using System.Collections.Concurrent;
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
                logger?.LogErrorChecked(err, "An error occured attempting to register a {ConsumerName} of type {ConsumerType}", consumerName, consumerType);
                throw new ConsumerRegistrationFailedException(consumerName,consumerType,err);
            }
            consumerSubscriptions.Add(subscription);
            return (TContractConnection)(IBaseContractConnection)this;
        }


        private static Type GetConsumerInterfaceType(Type consumerType, Type interfaceType)
            => Array.Find(consumerType.GetInterfaces(), t => t.IsGenericType && t.GetGenericTypeDefinition() == interfaceType)
                ??throw new InvalidConsumerTypeException(consumerType, interfaceType);

        private readonly ConcurrentBag<Assembly> loadedAssemblies = [];

        private static Type[] LoadableTypes => [typeof(IPubSubConsumer<>), typeof(IPubSubAsyncConsumer<>),
                    typeof(IQueryResponseConsumer<,>),typeof(IQueryResponseAsyncConsumer<,>)];

        private async Task LoadConsumersForAssemblyAsync(Assembly assembly, CancellationToken cancellationToken)
        {
            if (!loadedAssemblies.Contains(assembly))
            {
                loadedAssemblies.Add(assembly);
                Type[] types = [];
                try
                {
                    types=assembly.GetTypes()
                        .Where(t => !t.IsInterface && !t.IsAbstract && !(t.FullName?.StartsWith("Castle.Proxies")??false))
                        .ToArray();
                }
                catch
                {
                    //Ignoring the exception as this is just to prevent a loading issue.
                }
                var loadablePairs = types
                    .Select(consumerType => new
                    {
                        ConsumerType = consumerType,
                        InterfaceType = Array.Find(consumerType.GetInterfaces(),
                            t => t.IsGenericType && LoadableTypes.Contains(t.GetGenericTypeDefinition()))
                    })
                    .Where(pair => pair.InterfaceType!=null)
                    .ToArray();
                _ = await Task.WhenAll(
                        loadablePairs
                        .Where(consumerPair => Equals(consumerPair.InterfaceType?.GetGenericTypeDefinition(), typeof(IPubSubConsumer<>)))
                        .Select(consumerPair => ((IConsumerContractConnection<TContractConnection>)this).RegisterPubSubConsumerAsync(consumerPair.ConsumerType, cancellationToken: cancellationToken).AsTask())
                    );
                _ = await Task.WhenAll(
                        loadablePairs
                        .Where(consumerPair => Equals(consumerPair.InterfaceType?.GetGenericTypeDefinition(), typeof(IPubSubAsyncConsumer<>)))
                        .Select(consumerPair => ((IConsumerContractConnection<TContractConnection>)this).RegisterPubSubAsyncConsumerAsync(consumerPair.ConsumerType, cancellationToken: cancellationToken).AsTask())
                    );
                _ = await Task.WhenAll(
                        loadablePairs
                        .Where(consumerPair => Equals(consumerPair.InterfaceType?.GetGenericTypeDefinition(), typeof(IQueryResponseConsumer<,>)))
                        .Select(consumerPair => ((IConsumerContractConnection<TContractConnection>)this).RegisterQueryResponseConsumerAsync(consumerPair.ConsumerType, cancellationToken: cancellationToken).AsTask())
                    );
                _ = await Task.WhenAll(
                        loadablePairs
                        .Where(consumerPair => Equals(consumerPair.InterfaceType?.GetGenericTypeDefinition(), typeof(IQueryResponseAsyncConsumer<,>)))
                        .Select(consumerPair => ((IConsumerContractConnection<TContractConnection>)this).RegisterQueryResponseAsyncConsumerAsync(consumerPair.ConsumerType, cancellationToken: cancellationToken).AsTask())
                    );
            }
        }

        async ValueTask IConsumerContractConnection<TContractConnection>.AutoRegisterAllConsumersAsync(Assembly? assembly, CancellationToken cancellationToken)
        {
            if (assembly!=null)
                await LoadConsumersForAssemblyAsync(assembly!, cancellationToken);
            else
                await Task.WhenAll(AssemblyLoadContext.Default.Assemblies.Select(asm => LoadConsumersForAssemblyAsync(asm, cancellationToken)));
        }

        private IHealthCheck? healthCheck;
        protected abstract ConnectionHealthCheck? ProduceConnectionHealthCheck();
        IHealthCheck? IBaseContractConnection.HealthCheck => healthCheck??=ProduceConnectionHealthCheck();

        private static MessageFilters<TMessage>? ExtractFilter<TMessage,TConsumer>(TConsumer consumer)
        {
            Func<MessageHeader, ValueTask<MessageFilterResult>>? headerFilter=null;
            Func<TMessage, MessageHeader, ValueTask<MessageFilterResult>>? messageFilter=null;
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

        private static readonly MethodInfo RegisterPubSubConsumerMethod = typeof(IConsumerContractConnection<TContractConnection>).GetMethods()
            .First(method => Equals(method.Name, "RegisterPubSubConsumerAsync") && method.GetGenericArguments().Length==2 && method.GetParameters().Length==6);
        ValueTask<TContractConnection> IConsumerContractConnection<TContractConnection>.RegisterPubSubConsumerAsync(Type consumerType, string? channel, string? group, bool ignoreMessageHeader, CancellationToken cancellationToken)
        {
            var ifaceType = GetConsumerInterfaceType(consumerType, typeof(IPubSubConsumer<>));
            var methodInfo = RegisterPubSubConsumerMethod.MakeGenericMethod(ifaceType.GetGenericArguments()[0], consumerType);
            return (ValueTask<TContractConnection>)methodInfo!.Invoke(this, [
                (serviceProvider==null ? Activator.CreateInstance(consumerType) : ActivatorUtilities.CreateInstance(serviceProvider,consumerType)),
                channel,
                group,
                ignoreMessageHeader,
                null,
                cancellationToken
            ])!;
        }

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
                    true,
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

        private static readonly MethodInfo RegisterPubSubAsyncConsumerMethod = typeof(IConsumerContractConnection<TContractConnection>).GetMethods()
            .First(method => Equals(method.Name, "RegisterPubSubAsyncConsumerAsync") && method.GetGenericArguments().Length==2 && method.GetParameters().Length==6);
        ValueTask<TContractConnection> IConsumerContractConnection<TContractConnection>.RegisterPubSubAsyncConsumerAsync(Type consumerType, string? channel, string? group, bool ignoreMessageHeader, CancellationToken cancellationToken)
        {
            var ifaceType = GetConsumerInterfaceType(consumerType, typeof(IPubSubAsyncConsumer<>));
            var methodInfo = RegisterPubSubAsyncConsumerMethod.MakeGenericMethod(ifaceType.GetGenericArguments()[0], consumerType);
            return (ValueTask<TContractConnection>)methodInfo.Invoke(this, [
                (serviceProvider==null ? Activator.CreateInstance(consumerType) : ActivatorUtilities.CreateInstance(serviceProvider,consumerType)),
                channel,
                group,
                ignoreMessageHeader,
                null,
                cancellationToken
            ])!;
        }
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

        private static readonly MethodInfo RegisterQueryResponseConsumerMethod = typeof(IConsumerContractConnection<TContractConnection>).GetMethods()
            .First(method => Equals(method.Name, "RegisterQueryResponseConsumerAsync") && method.GetGenericArguments().Length==3 && method.GetParameters().Length==6);
        ValueTask<TContractConnection> IConsumerContractConnection<TContractConnection>.RegisterQueryResponseConsumerAsync(Type consumerType, string? channel, string? group, bool ignoreMessageHeader, CancellationToken cancellationToken)
        {
            var ifaceType = GetConsumerInterfaceType(consumerType, typeof(IQueryResponseConsumer<,>));
            var methodInfo = RegisterQueryResponseConsumerMethod.MakeGenericMethod(ifaceType.GetGenericArguments()[0], ifaceType.GetGenericArguments()[1], consumerType);
            return (ValueTask<TContractConnection>)methodInfo.Invoke(this, [
                (serviceProvider==null ? Activator.CreateInstance(consumerType) : ActivatorUtilities.CreateInstance(serviceProvider,consumerType)),
                channel,
                group,
                ignoreMessageHeader,
                null,
                cancellationToken
            ])!;
        }
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
                    true,
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

        private static readonly MethodInfo RegisterQueryResponseAsyncConsumerMethod = typeof(IConsumerContractConnection<TContractConnection>).GetMethods()
            .First(method => Equals(method.Name, "RegisterQueryResponseAsyncConsumerAsync") && method.GetGenericArguments().Length==3 && method.GetParameters().Length==6);
        ValueTask<TContractConnection> IConsumerContractConnection<TContractConnection>.RegisterQueryResponseAsyncConsumerAsync(Type consumerType, string? channel, string? group, bool ignoreMessageHeader, CancellationToken cancellationToken)
        {
            var ifaceType = GetConsumerInterfaceType(consumerType, typeof(IQueryResponseAsyncConsumer<,>));
            var methodInfo = RegisterQueryResponseAsyncConsumerMethod.MakeGenericMethod(ifaceType.GetGenericArguments()[0], ifaceType.GetGenericArguments()[1], consumerType);
            return (ValueTask<TContractConnection>)methodInfo.Invoke(this, [
                (serviceProvider==null ? Activator.CreateInstance(consumerType) : ActivatorUtilities.CreateInstance(serviceProvider,consumerType)),
                channel,
                group,
                ignoreMessageHeader,
                null,
                cancellationToken
            ])!;
        }
        #endregion
    }
}

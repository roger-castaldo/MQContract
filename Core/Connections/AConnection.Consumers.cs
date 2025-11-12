using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using MQContract.Attributes;
using MQContract.Interfaces;
using MQContract.Interfaces.Consumers;
using MQContract.Messages;
using MQContract.Middleware;
using System.Reflection;
using System.Runtime.Loader;

namespace MQContract.Connections
{
#pragma warning disable S3881 // "IDisposable" should be implemented correctly
    internal abstract partial class AConnection<CC> : IMetricContractConnection<CC>
#pragma warning restore S3881 // "IDisposable" should be implemented correctly
        where CC : IBaseContractConnection
    {
        private const string ConsumerClassNameKey = $"{OpenTelemetryMiddleware.KeyBase}.consumerclass";
        private async ValueTask<CC> RegisterSubscription(Func<string?, string?, bool, ValueTask<ISubscription>> createSubscription,
            string? channel, string? group, bool ignoreMessageHeader, string consumerName, Type consumerType, CancellationToken cancellationToken)
        {
            ISubscription subscription;
            var consumerAttribute = consumerType.GetCustomAttribute<ConsumerAttribute>();
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
                logger?.LogError(err, "An error occured attempting to register a {ConsumerName} of type {ConsumerType}", consumerName, consumerType);
                throw new ConsumerRegistrationFailedException(consumerName,consumerType,err);
            }
            await inboxSemaphore.WaitAsync(cancellationToken);
            consumerSubscriptions.Add(subscription);
            inboxSemaphore.Release();
            return (CC)(IBaseContractConnection)this;
        }


        private static Type GetConsumerInterfaceType(Type consumerType, Type interfaceType)
            => Array.Find(consumerType.GetInterfaces(), t => t.IsGenericType && t.GetGenericTypeDefinition() == interfaceType)
                ??throw new InvalidConsumerTypeException(consumerType, interfaceType);

        private readonly List<Assembly> loadedAssemblies = [];

        private static Type[] LoadableTypes => [typeof(IPubSubConsumer<>), typeof(IPubSubAsyncConsumer<>),
                    typeof(IQueryResponseConsumer<,>),typeof(IQueryResponseAsyncConsumer<,>)];

        private async Task LoadConsumersForAssemblyAsync(Assembly assembly, CancellationToken cancellationToken)
        {
            var process = false;
            await inboxSemaphore.WaitAsync(cancellationToken);
            if (!loadedAssemblies.Contains(assembly))
            {
                process=true;
                loadedAssemblies.Add(assembly);
            }
            inboxSemaphore.Release();
            if (process)
            {
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
                        .Select(consumerPair => ((IConsumerContractConnection<CC>)this).RegisterPubSubConsumerAsync(consumerPair.ConsumerType, cancellationToken: cancellationToken).AsTask())
                    );
                _ = await Task.WhenAll(
                        loadablePairs
                        .Where(consumerPair => Equals(consumerPair.InterfaceType?.GetGenericTypeDefinition(), typeof(IPubSubAsyncConsumer<>)))
                        .Select(consumerPair => ((IConsumerContractConnection<CC>)this).RegisterPubSubAsyncConsumerAsync(consumerPair.ConsumerType, cancellationToken: cancellationToken).AsTask())
                    );
                _ = await Task.WhenAll(
                        loadablePairs
                        .Where(consumerPair => Equals(consumerPair.InterfaceType?.GetGenericTypeDefinition(), typeof(IQueryResponseConsumer<,>)))
                        .Select(consumerPair => ((IConsumerContractConnection<CC>)this).RegisterQueryResponseConsumerAsync(consumerPair.ConsumerType, cancellationToken: cancellationToken).AsTask())
                    );
                _ = await Task.WhenAll(
                        loadablePairs
                        .Where(consumerPair => Equals(consumerPair.InterfaceType?.GetGenericTypeDefinition(), typeof(IQueryResponseAsyncConsumer<,>)))
                        .Select(consumerPair => ((IConsumerContractConnection<CC>)this).RegisterQueryResponseAsyncConsumerAsync(consumerPair.ConsumerType, cancellationToken: cancellationToken).AsTask())
                    );
            }
        }

        async ValueTask IConsumerContractConnection<CC>.AutoRegisterAllConsumersAsync(Assembly? assembly, CancellationToken cancellationToken)
        {
            if (assembly!=null)
                await LoadConsumersForAssemblyAsync(assembly!, cancellationToken);
            else
                await Task.WhenAll(AssemblyLoadContext.Default.Assemblies.Select(asm => LoadConsumersForAssemblyAsync(asm, cancellationToken)));
        }

        private IHealthCheck? healthCheck;
        protected abstract ConnectionHealthCheck? ProduceConnectionHealthCheck();
        IHealthCheck? IBaseContractConnection.HealthCheck => healthCheck??=ProduceConnectionHealthCheck();

        private MessageFilters<TMessage>? ExtractFilter<TMessage,TConsumer>(TConsumer consumer)
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
        ValueTask<CC> IConsumerContractConnection<CC>.RegisterPubSubConsumerAsync<TMessage, TConsumer>(TConsumer consumer, string? channel, string? group, bool ignoreMessageHeader, MessageFilters<TMessage>? messageFilters, CancellationToken cancellationToken)
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
                    messageFilters??ExtractFilter<TMessage,TConsumer>(consumer),
                    true,
                    cancellationToken
                ),
                channel,
                group,
                ignoreMessageHeader,
                "PubSubConsumer",
                consumer.GetType(),
                cancellationToken
            );

        ValueTask<CC> IConsumerContractConnection<CC>.RegisterPubSubConsumerAsync<TMessage, TConsumer>(string? channel, string? group, bool ignoreMessageHeader, MessageFilters<TMessage>? messageFilters, CancellationToken cancellationToken)
            => ((IConsumerContractConnection<CC>)this).RegisterPubSubConsumerAsync<TMessage, TConsumer>(
                (serviceProvider==null ? Activator.CreateInstance<TConsumer>() : ActivatorUtilities.CreateInstance<TConsumer>(serviceProvider)),
                channel,
                group,
                ignoreMessageHeader,
                messageFilters,
                cancellationToken: cancellationToken
            );

        private static readonly MethodInfo RegisterPubSubConsumerMethod = typeof(IConsumerContractConnection<CC>).GetMethods()
            .First(method => Equals(method.Name, "RegisterPubSubConsumerAsync") && method.GetGenericArguments().Length==2 && method.GetParameters().Length==6);
        ValueTask<CC> IConsumerContractConnection<CC>.RegisterPubSubConsumerAsync(Type consumerType, string? channel, string? group, bool ignoreMessageHeader, CancellationToken cancellationToken)
        {
            var ifaceType = GetConsumerInterfaceType(consumerType, typeof(IPubSubConsumer<>));
            var methodInfo = RegisterPubSubConsumerMethod.MakeGenericMethod(ifaceType.GetGenericArguments()[0], consumerType);
            return (ValueTask<CC>)methodInfo!.Invoke(this, [
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
        ValueTask<CC> IConsumerContractConnection<CC>.RegisterPubSubAsyncConsumerAsync<TMessage, TConsumer>(TConsumer consumer, string? channel, string? group, bool ignoreMessageHeader, MessageFilters<TMessage>? messageFilters, CancellationToken cancellationToken)
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
                    messageFilters??ExtractFilter<TMessage,TConsumer>(consumer),
                    true,
                    cancellationToken
                ),
                channel,
                group,
                ignoreMessageHeader,
                "PubSubAsyncConsumer",
                consumer.GetType(),
                cancellationToken
            );

        ValueTask<CC> IConsumerContractConnection<CC>.RegisterPubSubAsyncConsumerAsync<TMessage, TConsumer>(string? channel, string? group, bool ignoreMessageHeader, MessageFilters<TMessage>? messageFilters, CancellationToken cancellationToken)
            => ((IConsumerContractConnection<CC>)this).RegisterPubSubAsyncConsumerAsync<TMessage, TConsumer>(
                (serviceProvider==null ? Activator.CreateInstance<TConsumer>() : ActivatorUtilities.CreateInstance<TConsumer>(serviceProvider)),
                channel,
                group,
                ignoreMessageHeader,
                messageFilters,
                cancellationToken: cancellationToken
            );

        private static readonly MethodInfo RegisterPubSubAsyncConsumerMethod = typeof(IConsumerContractConnection<CC>).GetMethods()
            .First(method => Equals(method.Name, "RegisterPubSubAsyncConsumerAsync") && method.GetGenericArguments().Length==2 && method.GetParameters().Length==6);
        ValueTask<CC> IConsumerContractConnection<CC>.RegisterPubSubAsyncConsumerAsync(Type consumerType, string? channel, string? group, bool ignoreMessageHeader, CancellationToken cancellationToken)
        {
            var ifaceType = GetConsumerInterfaceType(consumerType, typeof(IPubSubAsyncConsumer<>));
            var methodInfo = RegisterPubSubAsyncConsumerMethod.MakeGenericMethod(ifaceType.GetGenericArguments()[0], consumerType);
            return (ValueTask<CC>)methodInfo.Invoke(this, [
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
        ValueTask<CC> IConsumerContractConnection<CC>.RegisterQueryResponseConsumerAsync<TQuery, TQueryResponse, TConsumer>(TConsumer consumer, string? channel, string? group, bool ignoreMessageHeader, MessageFilters<TQuery>? messageFilters, CancellationToken cancellationToken)
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
                    messageFilters??ExtractFilter<TQuery, TConsumer>(consumer),
                    cancellationToken
                ),
                channel,
                group,
                ignoreMessageHeader,
                "QueryResponseConsumer",
                consumer.GetType(),
                cancellationToken
             );

        ValueTask<CC> IConsumerContractConnection<CC>.RegisterQueryResponseConsumerAsync<TQuery, TQueryResponse, TConsumer>(string? channel, string? group, bool ignoreMessageHeader, MessageFilters<TQuery>? messageFilters, CancellationToken cancellationToken)
            => ((IConsumerContractConnection<CC>)this).RegisterQueryResponseConsumerAsync<TQuery, TQueryResponse, TConsumer>(
                (serviceProvider==null ? Activator.CreateInstance<TConsumer>() : ActivatorUtilities.CreateInstance<TConsumer>(serviceProvider)),
                channel,
                group,
                ignoreMessageHeader,
                messageFilters,
                cancellationToken
            );

        private static readonly MethodInfo RegisterQueryResponseConsumerMethod = typeof(IConsumerContractConnection<CC>).GetMethods()
            .First(method => Equals(method.Name, "RegisterQueryResponseConsumerAsync") && method.GetGenericArguments().Length==3 && method.GetParameters().Length==6);
        ValueTask<CC> IConsumerContractConnection<CC>.RegisterQueryResponseConsumerAsync(Type consumerType, string? channel, string? group, bool ignoreMessageHeader, CancellationToken cancellationToken)
        {
            var ifaceType = GetConsumerInterfaceType(consumerType, typeof(IQueryResponseConsumer<,>));
            var methodInfo = RegisterQueryResponseConsumerMethod.MakeGenericMethod(ifaceType.GetGenericArguments()[0], ifaceType.GetGenericArguments()[1], consumerType);
            return (ValueTask<CC>)methodInfo.Invoke(this, [
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
        ValueTask<CC> IConsumerContractConnection<CC>.RegisterQueryResponseAsyncConsumerAsync<TQuery, TQueryResponse, TConsumer>(TConsumer consumer, string? channel, string? group, bool ignoreMessageHeader, MessageFilters<TQuery>? messageFilters, CancellationToken cancellationToken)
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
                    messageFilters??ExtractFilter<TQuery, TConsumer>(consumer),
                    cancellationToken
                ),
                channel,
                group,
                ignoreMessageHeader,
                "QueryResponseAsyncConsumer",
                consumer.GetType(),
                cancellationToken
            );

        ValueTask<CC> IConsumerContractConnection<CC>.RegisterQueryResponseAsyncConsumerAsync<TQuery, TQueryResponse, TConsumer>(string? channel, string? group, bool ignoreMessageHeader, MessageFilters<TQuery>? messageFilters, CancellationToken cancellationToken)
            => ((IConsumerContractConnection<CC>)this).RegisterQueryResponseAsyncConsumerAsync<TQuery, TQueryResponse, TConsumer>(
                (serviceProvider==null ? Activator.CreateInstance<TConsumer>() : ActivatorUtilities.CreateInstance<TConsumer>(serviceProvider)),
                channel,
                group,
                ignoreMessageHeader,
                messageFilters,
                cancellationToken
            );

        private static readonly MethodInfo RegisterQueryResponseAsyncConsumerMethod = typeof(IConsumerContractConnection<CC>).GetMethods()
            .First(method => Equals(method.Name, "RegisterQueryResponseAsyncConsumerAsync") && method.GetGenericArguments().Length==3 && method.GetParameters().Length==6);
        ValueTask<CC> IConsumerContractConnection<CC>.RegisterQueryResponseAsyncConsumerAsync(Type consumerType, string? channel, string? group, bool ignoreMessageHeader, CancellationToken cancellationToken)
        {
            var ifaceType = GetConsumerInterfaceType(consumerType, typeof(IQueryResponseAsyncConsumer<,>));
            var methodInfo = RegisterQueryResponseAsyncConsumerMethod.MakeGenericMethod(ifaceType.GetGenericArguments()[0], ifaceType.GetGenericArguments()[1], consumerType);
            return (ValueTask<CC>)methodInfo.Invoke(this, [
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

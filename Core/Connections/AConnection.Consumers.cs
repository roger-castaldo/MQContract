using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MQContract.Attributes;
using MQContract.Interfaces;
using MQContract.Interfaces.Consumers;
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
        private async ValueTask<bool> RegisterSubscription(Func<string?, string?, bool, ValueTask<ISubscription>> createSubscription,
            string? channel, string? group, bool ignoreMessageHeader, string consumerName, Type consumerType, CancellationToken cancellationToken)
        {
            ISubscription subscription;
            try
            {
                subscription = await createSubscription(
                    channel??consumerType.GetCustomAttribute<ConsumerMessageChannelAttribute>()?.Name,
                    group??consumerType.GetCustomAttribute<ConsumerGroupAttribute>()?.Name,
                    ignoreMessageHeader||(consumerType.GetCustomAttribute<ConsumerIgnoreMessageHeaderAttribute>()?.IgnoreHeader??false)
                );
            }
            catch (Exception err)
            {
                logger?.LogError(err, "An error occured attempting to register a {ConsumerName} of type {ConsumerType}", consumerName, consumerType);
                return false;
            }
            await inboxSemaphore.WaitAsync(cancellationToken);
            consumerSubscriptions.Add(subscription);
            inboxSemaphore.Release();
            return true;
        }


        private static Type GetConsumerInterfaceType(Type consumerType, Type interfaceType)
            => Array.Find(consumerType.GetInterfaces(), t => t.IsGenericType && t.GetGenericTypeDefinition() == interfaceType)
                ??throw new InvalidConsumerTypeException(consumerType, interfaceType);

        private readonly List<Assembly> loadedAssemblies = [];

        private static Type[] LoadableTypes => [typeof(IPubSubConsumer<>), typeof(IPubSubAsyncConsumer<>),
                    typeof(IQueryResponseConsumer<,>),typeof(IQueryResponseAsyncConsumer<,>)];

        private async Task<bool> LoadConsumersForAssemblyAsync(Assembly assembly, CancellationToken cancellationToken)
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
                foreach (var consumerPair in loadablePairs)
                {
                    if (Equals(consumerPair.InterfaceType?.GetGenericTypeDefinition(), typeof(IPubSubConsumer<>)))
                    {
                        if (!(await ((IConsumerContractConnection)this).RegisterPubSubConsumerAsync(consumerPair.ConsumerType, cancellationToken: cancellationToken)))
                            return false;
                    }
                    else if (Equals(consumerPair.InterfaceType?.GetGenericTypeDefinition(), typeof(IPubSubAsyncConsumer<>)))
                    {
                        if (!(await ((IConsumerContractConnection)this).RegisterPubSubAsyncConsumerAsync(consumerPair.ConsumerType, cancellationToken: cancellationToken)))
                            return false;
                    }
                    else if (Equals(consumerPair.InterfaceType?.GetGenericTypeDefinition(), typeof(IQueryResponseConsumer<,>)))
                    {
                        if (!(await ((IConsumerContractConnection)this).RegisterQueryResponseConsumerAsync(consumerPair.ConsumerType, cancellationToken: cancellationToken)))
                            return false;
                    }
                    else if (Equals(consumerPair.InterfaceType?.GetGenericTypeDefinition(), typeof(IQueryResponseAsyncConsumer<,>))
                        &&!(await ((IConsumerContractConnection)this).RegisterQueryResponseAsyncConsumerAsync(consumerPair.ConsumerType, cancellationToken: cancellationToken)))
                        return false;
                }
            }
            return true;
        }

        async ValueTask<bool> IConsumerContractConnection.AutoRegisterAllConsumersAsync(Assembly? assembly, CancellationToken cancellationToken)
        {
            if (assembly!=null)
                return await LoadConsumersForAssemblyAsync(assembly!, cancellationToken);
            else
            {
                foreach (var asm in AssemblyLoadContext.Default.Assemblies)
                {
                    if (!(await LoadConsumersForAssemblyAsync(asm, cancellationToken)))
                        return false;
                }
                return true;
            }
        }

        #region PubSubConsumer
        ValueTask<bool> IConsumerContractConnection.RegisterPubSubConsumerAsync<T, TConsumer>(TConsumer consumer, string? channel, string? group, bool ignoreMessageHeader, CancellationToken cancellationToken)
            => RegisterSubscription((channel, group, ignoreMessageHeader) => CreateSubscriptionAsync<T>(
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

        ValueTask<bool> IConsumerContractConnection.RegisterPubSubConsumerAsync<T, TConsumer>(string? channel, string? group, bool ignoreMessageHeader, CancellationToken cancellationToken)
            => ((IConsumerContractConnection)this).RegisterPubSubConsumerAsync<T, TConsumer>(
                (serviceProvider==null ? Activator.CreateInstance<TConsumer>() : ActivatorUtilities.CreateInstance<TConsumer>(serviceProvider)),
                channel,
                group,
                ignoreMessageHeader,
                cancellationToken
            );

        private static readonly MethodInfo RegisterPubSubConsumerMethod = typeof(IConsumerContractConnection).GetMethods()
            .First(method => Equals(method.Name, "RegisterPubSubConsumerAsync") && method.GetGenericArguments().Length==2 && method.GetParameters().Length==5);
        ValueTask<bool> IConsumerContractConnection.RegisterPubSubConsumerAsync(Type consumerType, string? channel, string? group, bool ignoreMessageHeader, CancellationToken cancellationToken)
        {
            var ifaceType = GetConsumerInterfaceType(consumerType, typeof(IPubSubConsumer<>));
            var methodInfo = RegisterPubSubConsumerMethod.MakeGenericMethod(ifaceType.GetGenericArguments()[0], consumerType);
            return (ValueTask<bool>)methodInfo!.Invoke(this, [
                (serviceProvider==null ? Activator.CreateInstance(consumerType) : ActivatorUtilities.CreateInstance(serviceProvider,consumerType)),
                channel,
                group,
                ignoreMessageHeader,
                cancellationToken
            ])!;
        }

        #endregion

        #region PubSubAsyncConsumer
        ValueTask<bool> IConsumerContractConnection.RegisterPubSubAsyncConsumerAsync<T, TConsumer>(TConsumer consumer, string? channel, string? group, bool ignoreMessageHeader, CancellationToken cancellationToken)
            => RegisterSubscription((channel, group, ignoreMessageHeader) => CreateSubscriptionAsync<T>(
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
                    cancellationToken
                ),
                channel,
                group,
                ignoreMessageHeader,
                "PubSubAsyncConsumer",
                consumer.GetType(),
                cancellationToken
            );

        ValueTask<bool> IConsumerContractConnection.RegisterPubSubAsyncConsumerAsync<T, TConsumer>(string? channel, string? group, bool ignoreMessageHeader, CancellationToken cancellationToken)
            => ((IConsumerContractConnection)this).RegisterPubSubAsyncConsumerAsync<T, TConsumer>(
                (serviceProvider==null ? Activator.CreateInstance<TConsumer>() : ActivatorUtilities.CreateInstance<TConsumer>(serviceProvider)),
                channel,
                group,
                ignoreMessageHeader,
                cancellationToken
            );

        private static readonly MethodInfo RegisterPubSubAsyncConsumerMethod = typeof(IConsumerContractConnection).GetMethods()
            .First(method => Equals(method.Name, "RegisterPubSubAsyncConsumerAsync") && method.GetGenericArguments().Length==2 && method.GetParameters().Length==5);
        ValueTask<bool> IConsumerContractConnection.RegisterPubSubAsyncConsumerAsync(Type consumerType, string? channel, string? group, bool ignoreMessageHeader, CancellationToken cancellationToken)
        {
            var ifaceType = GetConsumerInterfaceType(consumerType, typeof(IPubSubAsyncConsumer<>));
            var methodInfo = RegisterPubSubAsyncConsumerMethod.MakeGenericMethod(ifaceType.GetGenericArguments()[0], consumerType);
            return (ValueTask<bool>)methodInfo.Invoke(this, [
                (serviceProvider==null ? Activator.CreateInstance(consumerType) : ActivatorUtilities.CreateInstance(serviceProvider,consumerType)),
                channel,
                group,
                ignoreMessageHeader,
                cancellationToken
            ])!;
        }
        #endregion

        #region QueryResponseConsumer
        ValueTask<bool> IConsumerContractConnection.RegisterQueryResponseConsumerAsync<Q, R, TConsumer>(TConsumer consumer, string? channel, string? group, bool ignoreMessageHeader, CancellationToken cancellationToken)
            => RegisterSubscription((channel, group, ignoreMessageHeader) => ProduceSubscribeQueryResponseAsync<Q, R>(
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
                    cancellationToken
                ),
                channel,
                group,
                ignoreMessageHeader,
                "QueryResponseConsumer",
                consumer.GetType(),
                cancellationToken
             );

        ValueTask<bool> IConsumerContractConnection.RegisterQueryResponseConsumerAsync<Q, R, TConsumer>(string? channel, string? group, bool ignoreMessageHeader, CancellationToken cancellationToken)
            => ((IConsumerContractConnection)this).RegisterQueryResponseConsumerAsync<Q, R, TConsumer>(
                (serviceProvider==null ? Activator.CreateInstance<TConsumer>() : ActivatorUtilities.CreateInstance<TConsumer>(serviceProvider)),
                channel,
                group,
                ignoreMessageHeader,
                cancellationToken
            );

        private static readonly MethodInfo RegisterQueryResponseConsumerMethod = typeof(IConsumerContractConnection).GetMethods()
            .First(method => Equals(method.Name, "RegisterQueryResponseConsumerAsync") && method.GetGenericArguments().Length==3 && method.GetParameters().Length==5);
        ValueTask<bool> IConsumerContractConnection.RegisterQueryResponseConsumerAsync(Type consumerType, string? channel, string? group, bool ignoreMessageHeader, CancellationToken cancellationToken)
        {
            var ifaceType = GetConsumerInterfaceType(consumerType, typeof(IQueryResponseConsumer<,>));
            var methodInfo = RegisterQueryResponseConsumerMethod.MakeGenericMethod(ifaceType.GetGenericArguments()[0], ifaceType.GetGenericArguments()[1], consumerType);
            return (ValueTask<bool>)methodInfo.Invoke(this, [
                (serviceProvider==null ? Activator.CreateInstance(consumerType) : ActivatorUtilities.CreateInstance(serviceProvider,consumerType)),
                channel,
                group,
                ignoreMessageHeader,
                cancellationToken
            ])!;
        }
        #endregion

        #region QueryResponseAsyncConsumer
        ValueTask<bool> IConsumerContractConnection.RegisterQueryResponseAsyncConsumerAsync<Q, R, TConsumer>(TConsumer consumer, string? channel, string? group, bool ignoreMessageHeader, CancellationToken cancellationToken)
            => RegisterSubscription((channel, group, ignoreMessageHeader) => ProduceSubscribeQueryResponseAsync<Q, R>(
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
                    cancellationToken
                ),
                channel,
                group,
                ignoreMessageHeader,
                "QueryResponseAsyncConsumer",
                consumer.GetType(),
                cancellationToken
            );

        ValueTask<bool> IConsumerContractConnection.RegisterQueryResponseAsyncConsumerAsync<Q, R, TConsumer>(string? channel, string? group, bool ignoreMessageHeader, CancellationToken cancellationToken)
            => ((IConsumerContractConnection)this).RegisterQueryResponseAsyncConsumerAsync<Q, R, TConsumer>(
                (serviceProvider==null ? Activator.CreateInstance<TConsumer>() : ActivatorUtilities.CreateInstance<TConsumer>(serviceProvider)),
                channel,
                group,
                ignoreMessageHeader,
                cancellationToken
            );

        private static readonly MethodInfo RegisterQueryResponseAsyncConsumerMethod = typeof(IConsumerContractConnection).GetMethods()
            .First(method => Equals(method.Name, "RegisterQueryResponseAsyncConsumerAsync") && method.GetGenericArguments().Length==3 && method.GetParameters().Length==5);
        ValueTask<bool> IConsumerContractConnection.RegisterQueryResponseAsyncConsumerAsync(Type consumerType, string? channel, string? group, bool ignoreMessageHeader, CancellationToken cancellationToken)
        {
            var ifaceType = GetConsumerInterfaceType(consumerType, typeof(IQueryResponseAsyncConsumer<,>));
            var methodInfo = RegisterQueryResponseAsyncConsumerMethod.MakeGenericMethod(ifaceType.GetGenericArguments()[0], ifaceType.GetGenericArguments()[1], consumerType);
            return (ValueTask<bool>)methodInfo.Invoke(this, [
                (serviceProvider==null ? Activator.CreateInstance(consumerType) : ActivatorUtilities.CreateInstance(serviceProvider,consumerType)),
                channel,
                group,
                ignoreMessageHeader,
                cancellationToken
            ])!;
        }
        #endregion
    }
}

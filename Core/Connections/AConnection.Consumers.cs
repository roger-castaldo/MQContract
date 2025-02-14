using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MQContract.Attributes;
using MQContract.Interfaces;
using MQContract.Interfaces.Consumers;
using System.Reflection;
using System.Runtime.Loader;
using System.Threading.Channels;

namespace MQContract.Connections
{
    internal abstract partial class AConnection<CC> : IMetricContractConnection<CC>
        where CC : IBaseContractConnection
    {
        private async ValueTask<bool> RegisterSubscription(Func<string?, string?, bool, ValueTask<ISubscription>> createSubscription,
            string? channel, string? group, bool ignoreMessageHeader, string consumerName, Type consumerType, CancellationToken cancellationToken)
        {
            await inboxSemaphore.WaitAsync(cancellationToken);
            try
            {
                consumerSubscriptions.Add(await createSubscription(
                    channel??consumerType.GetCustomAttribute<ConsumerMessageChannelAttribute>()?.Name,
                    group??consumerType.GetCustomAttribute<ConsumerGroupAttribute>()?.Name,
                    ignoreMessageHeader||(consumerType.GetCustomAttribute<ConsumerIgnoreMessageHeaderAttribute>()?.IgnoreHeader??false)
                ));
                return true;
            }
            catch (Exception err)
            {
                logger?.LogError(err, "An error occured attempting to register a {ConsumerName} of type {ConsumerType}", consumerName,consumerType);
                return false;
            }
            finally
            {
                inboxSemaphore.Release();
            }
        }

        private static Type GetConsumerInterfaceType(Type consumerType,Type interfaceType)
            => Array.Find(consumerType.GetInterfaces(),t=>t.IsGenericType && t.GetGenericTypeDefinition() == interfaceType)
                ??throw new InvalidConsumerType(consumerType, interfaceType);

        private readonly List<Assembly> loadedAssemblies = [];

        private static readonly Type[] LoadableTypes = [typeof(IPubSubConsumer<>), typeof(IPubSubAsyncConsumer<>),
        typeof(IQueryResponseConsumer<,>),typeof(IQueryResponseAsyncConsumer<,>)];

        private async Task<bool> LoadConsumersForAssemblyAsync(Assembly assembly,CancellationToken cancellationToken)
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
                var loadablePairs = assembly.GetTypes()
                    .Select(consumerType => new { ConsumerType=consumerType,InterfaceType=Array.Find(consumerType.GetInterfaces(), t => t.IsGenericType && LoadableTypes.Contains(t.GetGenericTypeDefinition()))})
                    .Where(pair=>pair.InterfaceType!=null)
                    .ToArray();
                foreach(var consumerPair in loadablePairs)
                {
                    if (consumerPair.InterfaceType==typeof(IPubSubConsumer<>)
                        && !(await ((IConsumerContractConnection)this).RegisterPubSubConsumerAsync(consumerPair.ConsumerType, cancellationToken: cancellationToken)))
                        return false;
                    else if (consumerPair.InterfaceType==typeof(IPubSubAsyncConsumer<>)
                        && !(await ((IConsumerContractConnection)this).RegisterPubSubAsyncConsumerAsync(consumerPair.ConsumerType, cancellationToken: cancellationToken)))
                        return false;
                    else if (consumerPair.InterfaceType==typeof(IQueryResponseConsumer<,>)
                        && !(await ((IConsumerContractConnection)this).RegisterQueryResponseConsumerAsync(consumerPair.ConsumerType, cancellationToken: cancellationToken)))
                        return false;
                    else if (consumerPair.InterfaceType==typeof(IQueryResponseAsyncConsumer<,>)
                        && !(await ((IConsumerContractConnection)this).RegisterQueryResponseAsyncConsumerAsync(consumerPair.ConsumerType, cancellationToken: cancellationToken)))
                        return false;
                    else
                        return false;
                }
            }
            return true;
        }

        async ValueTask<bool> IConsumerContractConnection.AutoRegisterAllConsumersAsync(Assembly? assembly,CancellationToken cancellationToken)
        {
            if (assembly!=null)
                return await LoadConsumersForAssemblyAsync(assembly!,cancellationToken);
            else
            {
                foreach (var asm in AssemblyLoadContext.Default.Assemblies)
                {
                    if (!(await LoadConsumersForAssemblyAsync(asm,cancellationToken)))
                        return false;
                }
                return true;
            }
        }

        #region PubSubConsumer
        ValueTask<bool> IConsumerContractConnection.RegisterPubSubConsumerAsync<T, TConsumer>(TConsumer consumer, string? channel, string? group, bool ignoreMessageHeader, CancellationToken cancellationToken)
            => CreatePubSubConsumerSubscriptionAsync<T>(consumer, channel, group, ignoreMessageHeader, cancellationToken);

        ValueTask<bool> IConsumerContractConnection.RegisterPubSubConsumerAsync<T, TConsumer>(string? channel, string? group, bool ignoreMessageHeader, CancellationToken cancellationToken)
            => CreatePubSubConsumerSubscriptionAsync<T>(
                (serviceProvider==null ? Activator.CreateInstance<TConsumer>() : ActivatorUtilities.CreateInstance<TConsumer>(serviceProvider)),
                channel,
                group,
                ignoreMessageHeader,
                cancellationToken
            );

        ValueTask<bool> IConsumerContractConnection.RegisterPubSubConsumerAsync(Type consumerType, string? channel, string? group, bool ignoreMessageHeader, CancellationToken cancellationToken)
        {
            var ifaceType = GetConsumerInterfaceType(consumerType,typeof(IPubSubConsumer<>));
            var methodinfo = typeof(AConnection<CC>).GetMethod(nameof(AConnection<CC>.CreatePubSubConsumerSubscriptionAsync))?
                .MakeGenericMethod(ifaceType.GetGenericArguments()[0]);
            return (ValueTask<bool>)methodinfo!.Invoke(this, [
                (serviceProvider==null ? Activator.CreateInstance(consumerType) : ActivatorUtilities.CreateInstance(serviceProvider,consumerType)),
                channel,
                group,
                ignoreMessageHeader,
                cancellationToken
            ])!;
        }

        private ValueTask<bool> CreatePubSubConsumerSubscriptionAsync<T>(IPubSubConsumer<T> consumer, string? channel, string? group, bool ignoreMessageHeader, CancellationToken cancellationToken)
            => RegisterSubscription((channel, group, ignoreMessageHeader) => CreateSubscriptionAsync<T>(
                    (message) =>
                    {
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

        #endregion

        #region PubSubAsyncConsumer
        ValueTask<bool> IConsumerContractConnection.RegisterPubSubAsyncConsumerAsync<T, TConsumer>(TConsumer consumer, string? channel, string? group, bool ignoreMessageHeader, CancellationToken cancellationToken)
            => CreatePubSubAsyncConsumerSubscriptionAsync<T>(consumer, channel, group, ignoreMessageHeader, cancellationToken);

        ValueTask<bool> IConsumerContractConnection.RegisterPubSubAsyncConsumerAsync<T, TConsumer>(string? channel, string? group, bool ignoreMessageHeader, CancellationToken cancellationToken)
            => CreatePubSubAsyncConsumerSubscriptionAsync<T>(
                (serviceProvider==null ? Activator.CreateInstance<TConsumer>() : ActivatorUtilities.CreateInstance<TConsumer>(serviceProvider)),
                channel,
                group,
                ignoreMessageHeader,
                cancellationToken
            );

        ValueTask<bool> IConsumerContractConnection.RegisterPubSubAsyncConsumerAsync(Type consumerType, string? channel, string? group, bool ignoreMessageHeader, CancellationToken cancellationToken)
        {
            var ifaceType = GetConsumerInterfaceType(consumerType, typeof(IPubSubAsyncConsumer<>));
            var methodinfo = typeof(AConnection<CC>).GetMethod(nameof(AConnection<CC>.CreatePubSubAsyncConsumerSubscriptionAsync))?
                .MakeGenericMethod(ifaceType.GetGenericArguments()[0]);
            return (ValueTask<bool>)methodinfo!.Invoke(this, [
                (serviceProvider==null ? Activator.CreateInstance(consumerType) : ActivatorUtilities.CreateInstance(serviceProvider,consumerType)),
                channel,
                group,
                ignoreMessageHeader,
                cancellationToken
            ])!;
        }

        private ValueTask<bool> CreatePubSubAsyncConsumerSubscriptionAsync<T>(IPubSubAsyncConsumer<T> consumer, string? channel, string? group, bool ignoreMessageHeader, CancellationToken cancellationToken)
            => RegisterSubscription((channel, group, ignoreMessageHeader) => CreateSubscriptionAsync<T>(
                    (message) => consumer.MessageReceivedAsync(message),
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

        #endregion

        #region QueryResponseConsumer
        ValueTask<bool> IConsumerContractConnection.RegisterQueryResponseConsumerAsync<Q,R, TConsumer>(TConsumer consumer, string? channel, string? group, bool ignoreMessageHeader, CancellationToken cancellationToken)
            => CreateQueryResponseConsumerSubscriptionAsync<Q, R>(consumer, channel, group, ignoreMessageHeader, cancellationToken);

        ValueTask<bool> IConsumerContractConnection.RegisterQueryResponseConsumerAsync<Q, R, TConsumer>(string? channel, string? group, bool ignoreMessageHeader, CancellationToken cancellationToken)
            => CreateQueryResponseConsumerSubscriptionAsync<Q, R>(
                (serviceProvider==null ? Activator.CreateInstance<TConsumer>() : ActivatorUtilities.CreateInstance<TConsumer>(serviceProvider)),
                channel,
                group,
                ignoreMessageHeader,
                cancellationToken
            );

        ValueTask<bool> IConsumerContractConnection.RegisterQueryResponseConsumerAsync(Type consumerType, string? channel, string? group, bool ignoreMessageHeader, CancellationToken cancellationToken)
        {
            var ifaceType = GetConsumerInterfaceType(consumerType, typeof(IQueryResponseConsumer<,>));
            var methodinfo = typeof(AConnection<CC>).GetMethod(nameof(AConnection<CC>.CreateQueryResponseConsumerSubscriptionAsync))?
                .MakeGenericMethod(ifaceType.GetGenericArguments()[0], ifaceType.GetGenericArguments()[1]);
            return (ValueTask<bool>)methodinfo!.Invoke(this, [
                (serviceProvider==null ? Activator.CreateInstance(consumerType) : ActivatorUtilities.CreateInstance(serviceProvider,consumerType)),
                channel,
                group,
                ignoreMessageHeader,
                cancellationToken
            ])!;
        }

        private ValueTask<bool> CreateQueryResponseConsumerSubscriptionAsync<Q, R>(IQueryResponseConsumer<Q, R> consumer, string? channel, string? group, bool ignoreMessageHeader, CancellationToken cancellationToken)
             => RegisterSubscription((channel, group, ignoreMessageHeader) => ProduceSubscribeQueryResponseAsync<Q, R>(
                    (message) => ValueTask.FromResult(consumer.MessageReceived(message)),
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

        #endregion

        #region QueryResponseAsyncConsumer
        ValueTask<bool> IConsumerContractConnection.RegisterQueryResponseAsyncConsumerAsync<Q, R, TConsumer>(TConsumer consumer, string? channel, string? group, bool ignoreMessageHeader, CancellationToken cancellationToken)
            => CreateQueryResponseAsyncConsumerSubscriptionAsync<Q, R>(consumer, channel, group, ignoreMessageHeader, cancellationToken);

        ValueTask<bool> IConsumerContractConnection.RegisterQueryResponseAsyncConsumerAsync<Q, R, TConsumer>(string? channel, string? group, bool ignoreMessageHeader, CancellationToken cancellationToken)
            => CreateQueryResponseAsyncConsumerSubscriptionAsync<Q, R>(
                (serviceProvider==null ? Activator.CreateInstance<TConsumer>() : ActivatorUtilities.CreateInstance<TConsumer>(serviceProvider)),
                channel,
                group,
                ignoreMessageHeader,
                cancellationToken
            );

        ValueTask<bool> IConsumerContractConnection.RegisterQueryResponseAsyncConsumerAsync(Type consumerType, string? channel, string? group, bool ignoreMessageHeader, CancellationToken cancellationToken)
        {
            var ifaceType = GetConsumerInterfaceType(consumerType, typeof(IQueryResponseAsyncConsumer<,>));
            var methodinfo = typeof(AConnection<CC>).GetMethod(nameof(AConnection<CC>.CreateQueryResponseAsyncConsumerSubscriptionAsync))?
                .MakeGenericMethod(ifaceType.GetGenericArguments()[0], ifaceType.GetGenericArguments()[1]);
            return (ValueTask<bool>)methodinfo!.Invoke(this, [
                (serviceProvider==null ? Activator.CreateInstance(consumerType) : ActivatorUtilities.CreateInstance(serviceProvider,consumerType)),
                channel,
                group,
                ignoreMessageHeader,
                cancellationToken
            ])!;
        }

        private ValueTask<bool> CreateQueryResponseAsyncConsumerSubscriptionAsync<Q, R>(IQueryResponseAsyncConsumer<Q, R> consumer, string? channel, string? group, bool ignoreMessageHeader, CancellationToken cancellationToken)
            => RegisterSubscription((channel, group, ignoreMessageHeader) => ProduceSubscribeQueryResponseAsync<Q, R>(
                    (message) => consumer.MessageReceivedAsync(message),
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
        #endregion
    }
}

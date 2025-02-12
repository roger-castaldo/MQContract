using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MQContract.Interfaces;
using MQContract.Interfaces.Consumers;

namespace MQContract.Connections
{
    internal abstract partial class AConnection<CC> : IMetricContractConnection<CC>
        where CC : IBaseContractConnection
    {
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
            var ifaceType = consumerType.GetInterfaces().FirstOrDefault(t => t.IsGenericType && t.GetGenericTypeDefinition() == typeof(IPubSubConsumer<>));
            if (ifaceType==null)
                throw new Exception("Unable to register consumer that does not implement the interface IPubSubConsumer<>");
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

        private async ValueTask<bool> CreatePubSubConsumerSubscriptionAsync<T>(IPubSubConsumer<T> consumer, string? channel, string? group, bool ignoreMessageHeader, CancellationToken cancellationToken)
        {
            await inboxSemaphore.WaitAsync(cancellationToken);
            try
            {
                consumerSubscriptions.Add(await CreateSubscriptionAsync<T>(
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
                ));
                return true;
            }
            catch (Exception err)
            {
                logger?.LogError(err, "An error occured attempting to register a PubSubConsumer of type {Type}", consumer.GetType());
                return false;
            }
            finally
            {
                inboxSemaphore.Release();
            }
        }

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
            var ifaceType = consumerType.GetInterfaces().FirstOrDefault(t => t.IsGenericType && t.GetGenericTypeDefinition() == typeof(IPubSubAsyncConsumer<>));
            if (ifaceType==null)
                throw new Exception("Unable to register consumer that does not implement the interface IPubSubAsyncConsumer<>");
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

        private async ValueTask<bool> CreatePubSubAsyncConsumerSubscriptionAsync<T>(IPubSubAsyncConsumer<T> consumer, string? channel, string? group, bool ignoreMessageHeader, CancellationToken cancellationToken)
        {
            await inboxSemaphore.WaitAsync(cancellationToken);
            try
            {
                consumerSubscriptions.Add(await CreateSubscriptionAsync<T>(
                    (message) => consumer.MessageReceivedAsync(message),
                    (error) => consumer.ErrorRecieved(error),
                    channel,
                    group,
                    ignoreMessageHeader,
                    true,
                    cancellationToken
                ));
                return true;
            }
            catch (Exception err)
            {
                logger?.LogError(err, "An error occured attempting to register a PubSubAsyncConsumer of type {Type}", consumer.GetType());
                return false;
            }
            finally
            {
                inboxSemaphore.Release();
            }
        }

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
            var ifaceType = consumerType.GetInterfaces().FirstOrDefault(t => t.IsGenericType && t.GetGenericTypeDefinition() == typeof(IQueryResponseConsumer<,>));
            if (ifaceType==null)
                throw new Exception("Unable to register consumer that does not implement the interface IQueryResponseConsumer<,>");
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

        private async ValueTask<bool> CreateQueryResponseConsumerSubscriptionAsync<Q, R>(IQueryResponseConsumer<Q, R> consumer, string? channel, string? group, bool ignoreMessageHeader, CancellationToken cancellationToken)
        {
            await inboxSemaphore.WaitAsync(cancellationToken);
            try
            {
                consumerSubscriptions.Add(await ProduceSubscribeQueryResponseAsync<Q,R>(
                    (message) => ValueTask.FromResult(consumer.MessageReceived(message)),
                    (error) => consumer.ErrorRecieved(error),
                    channel,
                    group,
                    ignoreMessageHeader,
                    true,
                    cancellationToken
                ));
                return true;
            }
            catch (Exception err)
            {
                logger?.LogError(err, "An error occured attempting to register a QueryResponseConsumer of type {Type}", consumer.GetType());
                return false;
            }
            finally
            {
                inboxSemaphore.Release();
            }
        }

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
            var ifaceType = consumerType.GetInterfaces().FirstOrDefault(t => t.IsGenericType && t.GetGenericTypeDefinition() == typeof(IQueryResponseAsyncConsumer<,>));
            if (ifaceType==null)
                throw new Exception("Unable to register consumer that does not implement the interface IQueryResponseAsyncConsumer<,>");
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

        private async ValueTask<bool> CreateQueryResponseAsyncConsumerSubscriptionAsync<Q, R>(IQueryResponseAsyncConsumer<Q, R> consumer, string? channel, string? group, bool ignoreMessageHeader, CancellationToken cancellationToken)
        {
            await inboxSemaphore.WaitAsync(cancellationToken);
            try
            {
                consumerSubscriptions.Add(await ProduceSubscribeQueryResponseAsync<Q,R>(
                    (message) =>consumer.MessageReceivedAsync(message),
                    (error) => consumer.ErrorRecieved(error),
                    channel,
                    group,
                    ignoreMessageHeader,
                    true,
                    cancellationToken
                ));
                return true;
            }
            catch (Exception err)
            {
                logger?.LogError(err, "An error occured attempting to register a QueryResponseAsyncConsumer of type {Type}", consumer.GetType());
                return false;
            }
            finally
            {
                inboxSemaphore.Release();
            }
        }

        #endregion
    }
}

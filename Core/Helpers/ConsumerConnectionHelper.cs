using Microsoft.Extensions.DependencyInjection;
using MQContract.Interfaces;
using MQContract.Interfaces.Consumers;
using System.Reflection;

namespace MQContract.Helpers
{
    internal static class ConsumerConnectionHelper
    {
        private static Type[] LoadableTypes => [typeof(IPubSubConsumer<>), typeof(IPubSubAsyncConsumer<>),
                    typeof(IQueryResponseConsumer<,>),typeof(IQueryResponseAsyncConsumer<,>)];
        private static MethodInfo? RegisterPubSubConsumerMethod = null;
        private static MethodInfo? RegisterPubSubAsyncConsumerMethod = null;
        private static MethodInfo? RegisterQueryResponseConsumerMethod = null;
        private static MethodInfo? RegisterQueryResponseAsyncConsumerMethod = null;

        private static Type GetConsumerInterfaceType(Type consumerType, Type interfaceType)
            => Array.Find(consumerType.GetInterfaces(), t => t.IsGenericType && t.GetGenericTypeDefinition() == interfaceType)
                ??throw new InvalidConsumerTypeException(consumerType, interfaceType);

        public static ValueTask<TContractConnection> RegisterPubSubConsumerAsync<TContractConnection>(IConsumerContractConnection<TContractConnection>  consumerConnection, IServiceProvider? serviceProvider, Type consumerType, string? channel, string? group, bool ignoreMessageHeader, CancellationToken cancellationToken)
            where TContractConnection : IBaseContractConnection
        {
            RegisterPubSubConsumerMethod??=typeof(IConsumerContractConnection<TContractConnection>).GetMethods()
            .First(method => Equals(method.Name, nameof(IConsumerContractConnection<TContractConnection>.RegisterPubSubConsumerAsync)) && method.GetGenericArguments().Length==2 && method.GetParameters().Length==6);
            var ifaceType = GetConsumerInterfaceType(consumerType, typeof(IPubSubConsumer<>));
            var methodInfo = RegisterPubSubConsumerMethod.MakeGenericMethod(ifaceType.GetGenericArguments()[0], consumerType);
            return (ValueTask<TContractConnection>)methodInfo!.Invoke(consumerConnection, 
                [
                    (serviceProvider==null ? Activator.CreateInstance(consumerType) : ActivatorUtilities.CreateInstance(serviceProvider,consumerType)),
                    channel,
                    group,
                    ignoreMessageHeader,
                    null,
                    cancellationToken
                ])!;
        }

        public static ValueTask<TContractConnection> RegisterPubSubAsyncConsumerAsync<TContractConnection>(IConsumerContractConnection<TContractConnection> consumerConnection, IServiceProvider? serviceProvider, Type consumerType, string? channel, string? group, bool ignoreMessageHeader, CancellationToken cancellationToken)
            where TContractConnection : IBaseContractConnection
        {
            RegisterPubSubAsyncConsumerMethod??=typeof(IConsumerContractConnection<TContractConnection>).GetMethods()
            .First(method => Equals(method.Name, nameof(IConsumerContractConnection<TContractConnection>.RegisterPubSubAsyncConsumerAsync)) && method.GetGenericArguments().Length==2 && method.GetParameters().Length==6);
            var ifaceType = GetConsumerInterfaceType(consumerType, typeof(IPubSubAsyncConsumer<>));
            var methodInfo = RegisterPubSubAsyncConsumerMethod.MakeGenericMethod(ifaceType.GetGenericArguments()[0], consumerType);
            return (ValueTask<TContractConnection>)methodInfo!.Invoke(consumerConnection,
                [
                    (serviceProvider==null ? Activator.CreateInstance(consumerType) : ActivatorUtilities.CreateInstance(serviceProvider,consumerType)),
                    channel,
                    group,
                    ignoreMessageHeader,
                    null,
                    cancellationToken
                ])!;
        }

        public static ValueTask<TContractConnection> RegisterQueryResponseConsumerAsync<TContractConnection>(IConsumerContractConnection<TContractConnection> consumerConnection, IServiceProvider? serviceProvider, Type consumerType, string? channel, string? group, bool ignoreMessageHeader, CancellationToken cancellationToken)
            where TContractConnection : IBaseContractConnection
        {
            RegisterQueryResponseConsumerMethod??=typeof(IConsumerContractConnection<TContractConnection>).GetMethods()
            .First(method => Equals(method.Name, nameof(IConsumerContractConnection<TContractConnection>.RegisterQueryResponseConsumerAsync)) && method.GetGenericArguments().Length==3 && method.GetParameters().Length==6);
            var ifaceType = GetConsumerInterfaceType(consumerType, typeof(IQueryResponseConsumer<,>));
            var methodInfo = RegisterQueryResponseConsumerMethod.MakeGenericMethod(ifaceType.GetGenericArguments()[0], ifaceType.GetGenericArguments()[1], consumerType);
            return (ValueTask<TContractConnection>)methodInfo!.Invoke(consumerConnection,
                [
                    (serviceProvider==null ? Activator.CreateInstance(consumerType) : ActivatorUtilities.CreateInstance(serviceProvider,consumerType)),
                    channel,
                    group,
                    ignoreMessageHeader,
                    null,
                    cancellationToken
                ])!;
        }

        public static ValueTask<TContractConnection> RegisterQueryResponseAsyncConsumerAsync<TContractConnection>(IConsumerContractConnection<TContractConnection> consumerConnection, IServiceProvider? serviceProvider, Type consumerType, string? channel, string? group, bool ignoreMessageHeader, CancellationToken cancellationToken)
            where TContractConnection : IBaseContractConnection
        {
            RegisterQueryResponseAsyncConsumerMethod??=typeof(IConsumerContractConnection<TContractConnection>).GetMethods()
            .First(method => Equals(method.Name, nameof(IConsumerContractConnection<TContractConnection>.RegisterQueryResponseAsyncConsumerAsync)) && method.GetGenericArguments().Length==3 && method.GetParameters().Length==6);
            var ifaceType = GetConsumerInterfaceType(consumerType, typeof(IQueryResponseAsyncConsumer<,>));
            var methodInfo = RegisterQueryResponseAsyncConsumerMethod.MakeGenericMethod(ifaceType.GetGenericArguments()[0], ifaceType.GetGenericArguments()[1], consumerType);
            return (ValueTask<TContractConnection>)methodInfo!.Invoke(consumerConnection,
                [
                    (serviceProvider==null ? Activator.CreateInstance(consumerType) : ActivatorUtilities.CreateInstance(serviceProvider,consumerType)),
                    channel,
                    group,
                    ignoreMessageHeader,
                    null,
                    cancellationToken
                ])!;
        }

        internal static async Task LoadConsumersForAssemblyAsync<TContractConnection>(IConsumerContractConnection<TContractConnection> consumerConnection, IServiceProvider? serviceProvider, Assembly assembly, CancellationToken cancellationToken)
            where TContractConnection : IBaseContractConnection
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
                    .Select(pair=>
                        ((pair.InterfaceType?.GetGenericTypeDefinition()) switch
                        {
                            (Type t) when t == typeof(IPubSubConsumer<>)=> RegisterPubSubConsumerAsync<TContractConnection>(consumerConnection, serviceProvider, pair.ConsumerType, null, null, false, cancellationToken).AsTask(),
                            (Type t) when t == typeof(IPubSubAsyncConsumer<>) => RegisterPubSubAsyncConsumerAsync<TContractConnection>(consumerConnection, serviceProvider, pair.ConsumerType, null, null, false, cancellationToken).AsTask(),
                            (Type t) when t == typeof(IQueryResponseConsumer<,>) => RegisterQueryResponseConsumerAsync<TContractConnection>(consumerConnection, serviceProvider, pair.ConsumerType, null, null, false, cancellationToken).AsTask(),
                            (Type t) when t == typeof(IQueryResponseAsyncConsumer<,>) => RegisterQueryResponseAsyncConsumerAsync<TContractConnection>(consumerConnection, serviceProvider, pair.ConsumerType, null, null, false, cancellationToken).AsTask(),
                            _ => Task.CompletedTask
                        })
                    )
                );
        }
    }
}

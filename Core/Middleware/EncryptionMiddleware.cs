using Microsoft.Extensions.DependencyInjection;
using MQContract.Defaults;
using MQContract.Interfaces.Encrypting;
using MQContract.Interfaces.Middleware;
using MQContract.Messages;
using System.Runtime.Loader;

namespace MQContract.Middleware
{
    internal class EncryptionMiddleware<T> : IAfterEncodeMiddleware, IBeforeDecodeMiddleware
    {
        private readonly IMessageEncryptor? globalEncryptor;
        private readonly IMessageTypeEncryptor<T>? messageTypeEncryptor;
        public EncryptionMiddleware(IMessageEncryptor? globalEncryptor, IServiceProvider? serviceProvider)
        {
            this.globalEncryptor = globalEncryptor??new NonEncryptor<T>();
            var encryptorType = AssemblyLoadContext.All
                .SelectMany(context => context.Assemblies)
                .SelectMany(assembly =>
                {
                    try
                    {
                        return assembly.GetTypes()
                        .Where(t => !t.IsInterface && !t.IsAbstract
                            && Array.Exists(t.GetInterfaces(), iface => Equals(iface, typeof(IMessageTypeEncryptor<T>))));
                    }
                    catch (Exception)
                    {
                        return [];
                    }
                })
                .FirstOrDefault();
            if (encryptorType!=null)
                messageTypeEncryptor = (IMessageTypeEncryptor<T>)(serviceProvider!=null ? ActivatorUtilities.CreateInstance(serviceProvider!, encryptorType!)! : Activator.CreateInstance(encryptorType!)!);
        }

        async ValueTask<ServiceMessage> IAfterEncodeMiddleware.AfterMessageEncodeAsync(Type messageType, IContext context, ServiceMessage message)
        {
            var (body,messageHeaders) = await (messageTypeEncryptor?.EncryptAsync(message.Data.ToArray())??globalEncryptor!.EncryptAsync(message.Data.ToArray()));
            return new(
                message.ID,
                message.MessageTypeID,
                message.Channel,
                new(message.Header,messageHeaders),
                body
            );
        }

        async ValueTask<(MessageHeader messageHeader, ReadOnlyMemory<byte> data)> IBeforeDecodeMiddleware.BeforeMessageDecodeAsync(IContext context, string id, MessageHeader messageHeader, string messageTypeID, string messageChannel, ReadOnlyMemory<byte> data)
        {
            using var dataStream = await (messageTypeEncryptor?.DecryptAsync(new MemoryStream(data.ToArray()), messageHeader)??globalEncryptor!.DecryptAsync(new MemoryStream(data.ToArray()), messageHeader))!;
            var result = new byte[dataStream.Length];
            await dataStream.ReadAsync(result);
            return (messageHeader, result);
        }
    }
}

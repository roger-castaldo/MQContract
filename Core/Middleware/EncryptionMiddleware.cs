using Microsoft.Extensions.DependencyInjection;
using MQContract.Interfaces.Encrypting;
using MQContract.Interfaces.Middleware;
using MQContract.Messages;
using System.Collections.Concurrent;
using System.Runtime.Loader;

namespace MQContract.Middleware
{
    [MiddlewareInjectionOrder<IAfterEncodeMiddleware>(postIndex: 2)]
    [MiddlewareInjectionOrder<IBeforeDecodeMiddleware>(preIndex: 2)]
    internal class EncryptionMiddleware(IMessageEncryptor? globalEncryptor, IServiceProvider? serviceProvider) : IAfterEncodeMiddleware, IBeforeDecodeMiddleware
    {
        public const string ExpectedTypeKey = "_ExpectedType";
        private readonly ConcurrentDictionary<Type, IMessageEncryptor> encryptors = [];

        private sealed class NonEncryptor : IMessageEncryptor
        {
            ValueTask<Stream> IMessageEncryptor.DecryptAsync(Stream stream, MessageHeader headers)
                => ValueTask.FromResult(stream);

            ValueTask<EncryptionResult> IMessageEncryptor.EncryptAsync(byte[] data)
                => ValueTask.FromResult<EncryptionResult>(new(null,data));
        }

        private IMessageEncryptor GetEncryptor(Type messageType)
        {
            if (!encryptors.TryGetValue(messageType, out var encryptor))
            {
                var ifaceType = typeof(IMessageTypeEncryptor<>).MakeGenericType(messageType);
                var encryptorType = AssemblyLoadContext.All
                    .SelectMany(context => context.Assemblies)
                    .SelectMany(assembly =>
                    {
                        try
                        {
                            return assembly.GetTypes()
                            .Where(t => !t.IsInterface && !t.IsAbstract
                                && Array.Exists(t.GetInterfaces(), iface => Equals(iface, ifaceType)));
                        }
                        catch (Exception)
                        {
                            return [];
                        }
                    })
                    .FirstOrDefault();
                if (encryptorType!=null)
                    encryptor = (IMessageEncryptor)(serviceProvider!=null ? ActivatorUtilities.CreateInstance(serviceProvider!, encryptorType!)! : Activator.CreateInstance(encryptorType!)!);
                else
                    encryptor = globalEncryptor??new NonEncryptor();
                if (encryptor!=null)
                    encryptors.TryAdd(messageType, encryptor!);

            }
            return encryptor!;
        }

        async ValueTask<ServiceMessage> IAfterEncodeMiddleware.AfterMessageEncodeAsync(Type messageType, IContext context, ServiceMessage message)
        {
            var encryptionResult = await GetEncryptor(messageType).EncryptAsync(message.Data.ToArray());
            return new(
                message.ID,
                message.MessageTypeID,
                message.Channel,
                (encryptionResult.Headers==null ? message.Header : new(message.Header,encryptionResult.Headers)),
                encryptionResult.Data
            );
        }

        async ValueTask<DecodableMessage> IBeforeDecodeMiddleware.BeforeMessageDecodeAsync(IContext context, string id, string messageTypeID, string messageChannel, DecodableMessage message)
        {
            using var dataStream = await GetEncryptor((Type)context[ExpectedTypeKey]!).DecryptAsync(new MemoryStream(message.Data.ToArray(), 0, message.Data.Length, false, true), message.MessageHeader);
            using var ms = new MemoryStream();
            await dataStream.CopyToAsync(ms);
            ms.TryGetBuffer(out ArraySegment<byte> buffer);
            return new(message.MessageHeader, buffer.AsMemory(0, (int)ms.Length));
        }
    }
}

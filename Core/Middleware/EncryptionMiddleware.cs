using MQContract.Interfaces.Encrypting;
using MQContract.Interfaces.Middleware;
using MQContract.Messages;
using System.Collections.Concurrent;

namespace MQContract.Middleware
{
    [MiddlewareInjectionOrder<IAfterEncodeMiddleware>(postIndex: 2)]
    [MiddlewareInjectionOrder<IBeforeDecodeMiddleware>(preIndex: 2)]
    internal class EncryptionMiddleware(MessageContext messageContext, IMessageEncryptor? globalEncryptor, IServiceProvider? serviceProvider) : IAfterEncodeMiddleware, IBeforeDecodeMiddleware
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
                encryptor = messageContext.GetMessageEncryptor(messageType, globalEncryptor, serviceProvider) ?? new NonEncryptor();
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

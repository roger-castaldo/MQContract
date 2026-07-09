using MQContract.Interfaces.Middleware;
using MQContract.Messages;
using System.IO.Compression;

namespace MQContract.Middleware;

[MiddlewareInjectionOrder<IAfterEncodeMiddleware>(postIndex: 1)]
[MiddlewareInjectionOrder<IBeforeDecodeMiddleware>(preIndex: 3)]
internal class CompressionMiddleware : IAfterEncodeMiddleware, IBeforeDecodeMiddleware
{
    private const string CompressedHeader = "_isCompressed";

    async ValueTask<ServiceMessage> IAfterEncodeMiddleware.AfterMessageEncodeAsync(Type messageType, IContext context, ServiceMessage message)
    {
        if (context is Context c && message.Data.Length>c.MaxMessageSize)
        {
            using var ms = new MemoryStream();
            var zip = new BrotliStream(ms, CompressionLevel.Optimal, false);
            await zip.WriteAsync(message.Data);
            await zip.FlushAsync();
            if (ms.Length>c.MaxMessageSize)
                throw new ArgumentOutOfRangeException(nameof(message), $"message data exceeds maxmium message size (MaxSize:{c.MaxMessageSize},EncodedSize:{ms.Length})");
            message.Data = ms.ToArray();
            message.Header[CompressedHeader] = "true";
        }
        return message;
    }

    async ValueTask<DecodableMessage> IBeforeDecodeMiddleware.BeforeMessageDecodeAsync(IContext context, string id, string messageTypeID, string messageChannel, DecodableMessage message)
    {
        if (messageTypeID.StartsWith("C-") || bool.Parse(message.MessageHeader[CompressedHeader]??"false"))
        {
            using var ms = new MemoryStream(message.Data.ToArray(), 0, message.Data.Length, false, true);
            using var zip = new BrotliStream(ms, CompressionMode.Decompress);
            using var resultStream = new MemoryStream();
            await zip.CopyToAsync(resultStream);
            resultStream.TryGetBuffer(out ArraySegment<byte> buffer);
            return new(message.MessageHeader, buffer.AsMemory(0, (int)resultStream.Length));
        }
        return message;
    }
}

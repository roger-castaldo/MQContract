using MQContract.Interfaces.Middleware;
using MQContract.Messages;
using System.IO.Compression;

namespace MQContract.Middleware
{
    internal class CompressionMiddleware : IAfterEncodeMiddleware,IBeforeDecodeMiddleware
    {
        private const string CompressedHeader = "_isCompressed";

        async ValueTask<ServiceMessage> IAfterEncodeMiddleware.AfterMessageEncodeAsync(Type messageType, IContext context, ServiceMessage message)
        {
            if (context is Context c && message.Data.Length>c.MaxMessageSize)
            {
                using var ms = new MemoryStream();
                var zip = new GZipStream(ms, System.IO.Compression.CompressionLevel.SmallestSize, false);
                await zip.WriteAsync(message.Data);
                await zip.FlushAsync();
                if (ms.Length>c.MaxMessageSize)
                    throw new ArgumentOutOfRangeException(nameof(message), $"message data exceeds maxmium message size (MaxSize:{c.MaxMessageSize},EncodedSize:{ms.Length})");
                return new(
                    message.ID,
                    message.MessageTypeID,
                    message.Channel,
                    new(message.Header, new Dictionary<string, string?>() { { CompressedHeader, "true" } }),
                    ms.ToArray()
                );
            }
            return message;
        }

        async ValueTask<(MessageHeader messageHeader, ReadOnlyMemory<byte> data)> IBeforeDecodeMiddleware.BeforeMessageDecodeAsync(IContext context, string id, MessageHeader messageHeader, string messageTypeID, string messageChannel, ReadOnlyMemory<byte> data)
        {
            if (messageTypeID.StartsWith("C-") || bool.Parse(messageHeader[CompressedHeader]??"false"))
            {
                using var ms = new MemoryStream(data.ToArray());
                using var zip = new GZipStream(ms, CompressionMode.Decompress);
                using var resultStream = new MemoryStream();
                await zip.CopyToAsync(resultStream);
                return (messageHeader, resultStream.ToArray());
            }
            return (messageHeader, data);
        }
    }
}

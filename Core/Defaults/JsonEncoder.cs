using MQContract.Interfaces.Encoding;
using System.Text.Json;

namespace MQContract.Defaults
{
    internal class JsonEncoder<T> : IMessageTypeEncoder<T>
    {
        private static JsonSerializerOptions JsonOptions => new()
        {
            WriteIndented=false,
            DefaultBufferSize=4096,
            AllowTrailingCommas=true,
            PropertyNameCaseInsensitive=true,
            ReadCommentHandling=JsonCommentHandling.Skip
        };

        public async ValueTask<T?> DecodeAsync(Stream stream)
            => await JsonSerializer.DeserializeAsync<T>(stream, options: JsonOptions);

        public async ValueTask<byte[]> EncodeAsync(T message)
        {
            using var ms = new MemoryStream();
            await JsonSerializer.SerializeAsync<T>(ms, message, options: JsonOptions);
            return ms.ToArray();
        }
    }
}

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

        public T? Decode(Stream stream)
            => JsonSerializer.Deserialize<T>(stream, options: JsonOptions);

        public byte[] Encode(T message)
            => System.Text.UTF8Encoding.UTF8.GetBytes(JsonSerializer.Serialize<T>(message, options: JsonOptions));
    }
}

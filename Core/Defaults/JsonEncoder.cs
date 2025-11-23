using MQContract.Interfaces.Encoding;
using System.Text.Json;

namespace MQContract.Defaults
{
    internal class JsonEncoder<TMessage> : IMessageTypeEncoder<TMessage>
    {
        private static JsonSerializerOptions JsonOptions => new()
        {
            WriteIndented=false,
            AllowTrailingCommas=true,
            PropertyNameCaseInsensitive=true,
            ReadCommentHandling=JsonCommentHandling.Skip
        };

        public async ValueTask<TMessage?> DecodeAsync(Stream stream)
            => await JsonSerializer.DeserializeAsync<TMessage>(stream, options: JsonOptions);

        public ValueTask<byte[]> EncodeAsync(TMessage message)
            => ValueTask.FromResult(JsonSerializer.SerializeToUtf8Bytes<TMessage>(message, JsonOptions));
    }
}

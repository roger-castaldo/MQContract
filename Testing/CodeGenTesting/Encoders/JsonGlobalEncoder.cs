using MQContract.Interfaces.Encoding;
using System.Text.Json;

namespace CodeGenTesting.Encoders
{
    internal class JsonGlobalEncoder : IMessageEncoder
    {
        async ValueTask<TMessage?> IMessageEncoder.DecodeAsync<TMessage>(Stream stream) where TMessage : default
            => await JsonSerializer.DeserializeAsync<TMessage>(stream);

        ValueTask<byte[]> IMessageEncoder.EncodeAsync<TMessage>(TMessage message)
            => ValueTask.FromResult(JsonSerializer.SerializeToUtf8Bytes<TMessage>(message));
    }
}

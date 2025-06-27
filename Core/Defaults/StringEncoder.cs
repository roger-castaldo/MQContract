using MQContract.Interfaces.Encoding;

namespace MQContract.Defaults
{
    internal class StringEncoder : IMessageTypeEncoder<string>
    {
        async ValueTask<string?> IMessageTypeEncoder<string>.DecodeAsync(Stream stream)
        {
            using var reader = new StreamReader(stream);
            return await reader.ReadToEndAsync();
        }

        async ValueTask<byte[]> IMessageTypeEncoder<string>.EncodeAsync(string message)
        {
            using var ms = new MemoryStream();
            using var writer = new StreamWriter(ms);
            await writer.WriteAsync(message);
            await writer.FlushAsync();
            return ms.ToArray();
        }
    }
}

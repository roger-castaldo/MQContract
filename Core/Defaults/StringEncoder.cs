using MQContract.Interfaces.Encoding;
using System.Text;

namespace MQContract.Defaults;

internal class StringEncoder : IMessageTypeEncoder<string>
{
    async ValueTask<string?> IMessageTypeEncoder<string>.DecodeAsync(Stream stream)
    {
        using var reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: false, bufferSize: 8192, leaveOpen: true);
        return await reader.ReadToEndAsync();
    }

    ValueTask<byte[]> IMessageTypeEncoder<string>.EncodeAsync(string message)
    {
        int byteCount = Encoding.UTF8.GetByteCount(message);
        byte[] bytes = new byte[byteCount];
        Encoding.UTF8.GetBytes(message.AsSpan(), bytes.AsSpan());
        return ValueTask.FromResult<byte[]>(bytes);
    }
}

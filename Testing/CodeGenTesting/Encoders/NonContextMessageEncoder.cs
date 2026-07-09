using CodeGenTesting.Messages;
using MQContract.Interfaces.Encoding;

namespace CodeGenTesting.Encoders;

internal class NonContextMessageEncoder : IMessageTypeEncoder<NonContextMessage>
{
    ValueTask<NonContextMessage?> IMessageTypeEncoder<NonContextMessage>.DecodeAsync(Stream stream)
        => throw new NotImplementedException();

    ValueTask<byte[]> IMessageTypeEncoder<NonContextMessage>.EncodeAsync(NonContextMessage message)
        => throw new NotImplementedException();
}

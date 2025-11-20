using BenchMark.Messages;
using MQContract.Interfaces.Encoding;
using System.Text.Json;

namespace BenchMark.Encoders
{
    internal class EncodedAnnouncementEncoder : IMessageTypeEncoder<EncodedAnnouncement>
    {
        async ValueTask<EncodedAnnouncement?> IMessageTypeEncoder<EncodedAnnouncement>.DecodeAsync(Stream stream)
            => await JsonSerializer.DeserializeAsync<EncodedAnnouncement?>(stream);

        ValueTask<byte[]> IMessageTypeEncoder<EncodedAnnouncement>.EncodeAsync(EncodedAnnouncement message)
            => ValueTask.FromResult<byte[]>(JsonSerializer.SerializeToUtf8Bytes<EncodedAnnouncement>(message));
    }
}

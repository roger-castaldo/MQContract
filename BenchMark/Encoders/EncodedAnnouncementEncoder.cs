using BenchMark.Messages;
using MQContract.Interfaces.Encoding;
using System.Text.Json;

namespace BenchMark.Encoders
{
    internal class EncodedAnnouncementEncoder : IMessageTypeEncoder<EncodedAnnouncement>
    {
        async ValueTask<EncodedAnnouncement?> IMessageTypeEncoder<EncodedAnnouncement>.DecodeAsync(Stream stream)
            => await JsonSerializer.DeserializeAsync<EncodedAnnouncement?>(stream);

        async ValueTask<byte[]> IMessageTypeEncoder<EncodedAnnouncement>.EncodeAsync(EncodedAnnouncement message)
        {
            using var ms = new MemoryStream();
            await JsonSerializer.SerializeAsync<EncodedAnnouncement>(ms, message);
            return ms.ToArray();
        }
    }
}

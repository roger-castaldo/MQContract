using BenchMark.Messages;
using MQContract.Interfaces.Encoding;
using System.Text.Json;

namespace BenchMark.Encoders
{
    internal class EncodedAnnouncementEncoder : IMessageTypeEncoder<EncodedAnnouncement>
    {
        private static JsonSerializerOptions JsonOptions => new()
        {
            WriteIndented=false,
            AllowTrailingCommas=true,
            PropertyNameCaseInsensitive=true,
            ReadCommentHandling=JsonCommentHandling.Skip,
            TypeInfoResolver = MyJsonContext.Default
        };

        async ValueTask<EncodedAnnouncement?> IMessageTypeEncoder<EncodedAnnouncement>.DecodeAsync(Stream stream)
            => await JsonSerializer.DeserializeAsync<EncodedAnnouncement?>(stream,options:JsonOptions);

        ValueTask<byte[]> IMessageTypeEncoder<EncodedAnnouncement>.EncodeAsync(EncodedAnnouncement message)
            => ValueTask.FromResult<byte[]>(JsonSerializer.SerializeToUtf8Bytes<EncodedAnnouncement>(message, options: JsonOptions));
    }
}

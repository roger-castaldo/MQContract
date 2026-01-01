using CodeGenTesting.Messages;
using MQContract.Interfaces.Encoding;
using System.Text.Json;

namespace CodeGenTesting.Encoders
{
    internal class PartyAnnouncementEncoder : IMessageTypeEncoder<PartyAnnouncement>
    {
        private static JsonSerializerOptions JsonOptions => new()
        {
            WriteIndented=false,
            AllowTrailingCommas=true,
            PropertyNameCaseInsensitive=true,
            ReadCommentHandling=JsonCommentHandling.Skip,
            TypeInfoResolver = MyJsonContext.Default
        };

        async ValueTask<PartyAnnouncement?> IMessageTypeEncoder<PartyAnnouncement>.DecodeAsync(Stream stream)
            => await JsonSerializer.DeserializeAsync<PartyAnnouncement>(stream, options: JsonOptions);

        ValueTask<byte[]> IMessageTypeEncoder<PartyAnnouncement>.EncodeAsync(PartyAnnouncement message)
            => ValueTask.FromResult<byte[]>(JsonSerializer.SerializeToUtf8Bytes<PartyAnnouncement>(message, options: JsonOptions));
    }
}

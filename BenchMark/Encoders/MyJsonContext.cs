using BenchMark.Messages;
using System.Text.Json.Serialization;

namespace BenchMark.Encoders
{
    [JsonSerializable(typeof(EncodedAnnouncement))]
    public partial class MyJsonContext : JsonSerializerContext { }
}

using BenchMark.Messages;
using System.Text.Json.Serialization;

namespace BenchMark.Encoders
{
    [JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    WriteIndented = false,
    AllowTrailingCommas = true,
    PropertyNameCaseInsensitive = true,
    ReadCommentHandling = System.Text.Json.JsonCommentHandling.Skip)]
    [JsonSerializable(typeof(EncodedAnnouncement))]
    public partial class MyJsonContext : JsonSerializerContext { }
}

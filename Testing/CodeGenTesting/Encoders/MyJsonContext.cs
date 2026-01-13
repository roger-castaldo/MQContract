using CodeGenTesting.Messages;
using System.Text.Json.Serialization;

namespace CodeGenTesting.Encoders
{
    [JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    WriteIndented = false,
    AllowTrailingCommas = true,
    PropertyNameCaseInsensitive = true,
    ReadCommentHandling = System.Text.Json.JsonCommentHandling.Skip)]
    [JsonSerializable(typeof(PartyAnnouncement))]
    public partial class MyJsonContext : JsonSerializerContext { }
}

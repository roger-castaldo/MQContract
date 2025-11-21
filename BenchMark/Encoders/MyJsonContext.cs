using BenchMark.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace BenchMark.Encoders
{
    [JsonSerializable(typeof(EncodedAnnouncement))]
    public partial class MyJsonContext : JsonSerializerContext { }
}

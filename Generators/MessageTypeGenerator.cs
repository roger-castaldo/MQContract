using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System.Linq;
using System.Text;

namespace MQContract.Generators
{
    [Generator]
    public sealed class MessageTypeGenerator : IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            context.RegisterSourceOutput(
                context.CompilationProvider,
                Generate);
        }

        private static void Generate(
            SourceProductionContext context,
            Compilation compilation)
        {
            var (_, messages) = Helper.ExtractMessageTypes(compilation);

            var sb = new StringBuilder();
            sb.Append(@"namespace MQContract.Helpers;

internal static partial class MessageTypeHelper
{
    private static (string channel, string typeName, Version version) TryGetType(Type messageType)
    {
        return (messageType) switch {");
            foreach(var msg in messages)
            {
                var att = msg.GetAttributes().FirstOrDefault(a => a.AttributeClass?.Name == "MessageAttribute");
                if (att!=null)
                {
                    var channel = (string?)att.ConstructorArguments[0].Value;
                    var name = (string?)att.ConstructorArguments[1].Value;
                    var version = (string?)att.ConstructorArguments[2].Value;
                    sb.AppendLine($"        (Type t) when t == typeof({msg.ToDisplayString()}) => ({(channel==null ? "null" : $"\"{channel}\"")}, \"{name??msg.Name}\",new Version(\"{version??"0.0.0.0"}\")),");
                }
            }
sb.AppendLine(@"            _ => (null, null, null)
        };
    }
}");
            context.AddSource(
                "MessageTypeHelper.g.cs",
                SourceText.From(sb.ToString(), Encoding.UTF8));
        }
    }
}

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MQContract.Generators
{
    [Generator]
    public sealed class MqContractGenerator : IIncrementalGenerator
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
            var assemblies = compilation.SourceModule
                .ReferencedAssemblySymbols
                .Append(compilation.Assembly);

            // Collect message types
            var messages = assemblies.SelectMany(assembly =>
                assembly.GetAttributes()
                .Where(attr=> attr.AttributeClass?.Name == "UseMqContractAttribute" &&
                        attr.ConstructorArguments[0].Value is INamedTypeSymbol)
                .Select(attr => (INamedTypeSymbol)attr.ConstructorArguments[0].Value!)
            )
                .Distinct(SymbolEqualityComparer.Default)
                .OfType<INamedTypeSymbol>()
                .ToArray();
            var encoders = new Dictionary<INamedTypeSymbol, INamedTypeSymbol>(
                SymbolEqualityComparer.Default);

            // Collect explicit encoders
            foreach (var assembly in assemblies)
            {
                foreach (var type in assembly.GlobalNamespace.GetNamespaceTypes())
                {
                    if (type.IsAbstract)
                        continue;

                    foreach (var iface in type.AllInterfaces)
                    {
                        if (iface.Name == "IMessageTypeEncoder" &&
                            iface.TypeArguments.Length == 1 &&
                            iface.TypeArguments[0] is INamedTypeSymbol msg)
                        {
                            encoders[msg] = type;
                        }
                    }
                }
            }

            GenerateRegistry(context, messages, encoders);
        }

        private static void GenerateRegistry(
            SourceProductionContext context,
            IEnumerable<INamedTypeSymbol> messages,
            IDictionary<INamedTypeSymbol, INamedTypeSymbol> encoders)
        {
            var sb = new StringBuilder();
            var jsonEncoders = new List<string>();

            sb.Append(@"
using System;
using MQContract.Defaults;
using MQContract.Interfaces.Encoding;
using Microsoft.Extensions.DependencyInjection;

namespace MQContract.Factories;

internal static partial class MessageEncodingFactory
{
    private static object TryGetMessageEncoder<TMessage>(IMessageEncoder globalMessageEncoder, IServiceProvider serviceProvider)
    {
        return ");
            if (messages.Any())
            {
                sb.AppendLine(@"(typeof(TMessage), globalMessageEncoder, serviceProvider) switch
        {");

                foreach (var msg in messages)
                {
                    if (encoders.TryGetValue(msg, out var encoder))
                    {
                        sb.AppendLine($@"            (Type t, _, not null) when t == typeof({msg.ToDisplayString()}) => ActivatorUtilities.CreateInstance<{encoder.ToDisplayString()}>(serviceProvider!),
            (Type t, _, null) when t == typeof({msg.ToDisplayString()}) => Activator.CreateInstance<{encoder.ToDisplayString()}>(),");
                    }
                    else
                    {
                        sb.AppendLine($@"            (Type t, not null, _) when t == typeof({msg.ToDisplayString()}) => globalMessageEncoder,
            (Type t, null, _) when t == typeof({msg.ToDisplayString()}) => new {msg.ToDisplayString().Replace('.', '_')}Encoder(),");
                        jsonEncoders.Add($@"
internal class {msg.ToDisplayString().Replace('.','_')}Encoder : IMessageTypeEncoder<{msg.ToDisplayString()}>{{
    private static JsonSerializerOptions JsonOptions => new()
        {{
            WriteIndented=false,
            AllowTrailingCommas=true,
            PropertyNameCaseInsensitive=true,
            ReadCommentHandling=JsonCommentHandling.Skip
        }};

    public async ValueTask<{msg.ToDisplayString()}> DecodeAsync(Stream stream)
            => await JsonSerializer.DeserializeAsync<{msg.ToDisplayString()}>(stream, options: JsonOptions);

        public ValueTask<byte[]> EncodeAsync({msg.ToDisplayString()} message)
            => ValueTask.FromResult(JsonSerializer.SerializeToUtf8Bytes<{msg.ToDisplayString()}>(message, JsonOptions));
}}");
                    }
                }
                sb.AppendLine(@"            _ => null
         };");
            }
            else
                sb.AppendLine("null;");
                sb.AppendLine(@"    }
}
");

            context.AddSource(
                "MessageTypeRegistry.g.cs",
                SourceText.From(sb.ToString(), Encoding.UTF8));

            if (jsonEncoders.Count>0)
                context.AddSource(
                    "MqContractJsonEncoders.g.cs",
                    SourceText.From($@"using MQContract.Interfaces.Encoding;
            using System.Text.Json;

namespace MQContract.Defaults;
                
{string.Join("\r\n", jsonEncoders)}", Encoding.UTF8));
        }
    }

    internal static class NamespaceExtensions
    {
        public static IEnumerable<INamedTypeSymbol> GetNamespaceTypes(
            this INamespaceSymbol ns)
        {
            foreach (var member in ns.GetMembers())
            {
                if (member is INamespaceSymbol child)
                {
                    foreach (var t in child.GetNamespaceTypes())
                        yield return t;
                }
                else if (member is INamedTypeSymbol type)
                {
                    yield return type;
                }
            }
        }
    }
}

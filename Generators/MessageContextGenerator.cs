using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

namespace MQContract.Generators
{
    [Generator]
    public sealed class MessageContextGenerator : IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            // 1. Find candidate classes
            var candidateClasses = TargetHelper.LocateContexts(context);

            // 2. Find encoders
            var encoders = EncodersHelper.LocateEncoders(context);

            // 3. Find converters
            var converters = ConvertersHelper.LocateConverters(context);

            // 3. Generate code
            context.RegisterSourceOutput(
                candidateClasses
                    .Combine(encoders.Collect())
                    .Combine(converters.Collect()),
                Generate
            );
        }

        private void Generate(SourceProductionContext context, ((ContractContext? Left, ImmutableArray<ContractEncoder?> Right) Left, ImmutableArray<ContractConverter?> Right) candidate)
        {
            if (candidate.Left.Left.HasValue)
            {
                var contractContext = candidate.Left.Left.Value;
                var encoders = candidate.Left.Right.OfType<ContractEncoder>();
                var converters = candidate.Right.OfType<ContractConverter>();

                GenerateDefinitionImplementation(context, contractContext);
                GenerateEncoderImplementation(context, contractContext, encoders);
            }
        }

        private void GenerateDefinitionImplementation(SourceProductionContext context, ContractContext contractContext)
        {
            var sb = new StringBuilder();
            sb.AppendLine($@"using MQContract;

namespace {contractContext.Target.ContainingNamespace};

{contractContext.Target.DeclaredAccessibility.ToString().ToLower()} partial class {contractContext.Target.Name} : MQContractMessageContext {{
    public override MessageTypeDefinition? TryGetMessageType(Type messageType){{
        return (messageType) switch {{");

            foreach (var contract in contractContext.Contracts)
            {
                var att = contract.Contract.GetAttributes().FirstOrDefault(a => a.AttributeClass?.Name == "MessageAttribute");
                if (att!=null)
                {
                    var channel = (string?)att.ConstructorArguments[0].Value;
                    var name = (string?)att.ConstructorArguments[1].Value;
                    var version = (string?)att.ConstructorArguments[2].Value;
                    sb.AppendLine($"            (Type t) when t == typeof({contract.Contract.ToDisplayString()}) => new({(channel==null ? "null" : $"\"{channel}\"")}, \"{name??contract.Contract.Name}\",new Version(\"{version??"0.0.0.0"}\")),");
                }
            }
            sb.AppendLine(@"            _ => null
        };
    }
}");

            context.AddSource(
                $"{contractContext.Target.Name}.Definitions.g.cs",
                SourceText.From(sb.ToString(), Encoding.UTF8));
        }

        private void GenerateEncoderImplementation(SourceProductionContext context, ContractContext contractContext, IEnumerable<ContractEncoder> encoders)
        {
            var sb = new StringBuilder();

            sb.Append($@"using System;
using System.Text.Json;
using MQContract;
using MQContract.Interfaces.Encoding;
using Microsoft.Extensions.DependencyInjection;

namespace {contractContext.Target.ContainingNamespace};

{contractContext.Target.DeclaredAccessibility.ToString().ToLower()} partial class {contractContext.Target.Name} : MQContractMessageContext {{

    private class DefaultJsonEncoder<TMessage>(JsonSerializerOptions jsonOptions) : IMessageTypeEncoder<TMessage> {{
        public async ValueTask<TMessage> DecodeAsync(Stream stream)
            => await JsonSerializer.DeserializeAsync<TMessage>(stream, options: jsonOptions);

        public ValueTask<byte[]> EncodeAsync(TMessage message)
            => ValueTask.FromResult(JsonSerializer.SerializeToUtf8Bytes<TMessage>(message, jsonOptions));
    }}

    public override object TryGetMessageEncoder<TMessage>(IMessageEncoder globalMessageEncoder, IServiceProvider serviceProvider){{
        var jsonOptions = new JsonSerializerOptions()
        {{
            WriteIndented=false,
            AllowTrailingCommas=true,
            PropertyNameCaseInsensitive=true,
            ReadCommentHandling=JsonCommentHandling.Skip
        }};
        return (typeof(TMessage), globalMessageEncoder, serviceProvider) switch
        {{");

            foreach(var contract in contractContext.Contracts)
            {
                if (contract.Encoders!=null)
                {
                    if (contract.Encoders.Count()==1)
                    {
                        var encoder = contract.Encoders.First();
                        sb.AppendLine($@"            (Type t, _, not null) when t == typeof({contract.Contract.ToDisplayString()}) => ActivatorUtilities.CreateInstance<{encoder.ToDisplayString()}>(serviceProvider!),
            (Type t, _, null) when t == typeof({contract.Contract.ToDisplayString()}) => Activator.CreateInstance<{encoder.ToDisplayString()}>(),");
                    }
                    else
                    {
                        //faile here due to unknowns
                    }
                }
                else
                    sb.AppendLine($@"            (Type t, not null, _) when t == typeof({contract.Contract.ToDisplayString()}) => globalMessageEncoder,
            (Type t, null, _) when t == typeof({contract.Contract.ToDisplayString()}) => new DefaultJsonEncoder<{contract.Contract.ToDisplayString()}>(jsonOptions),");
            }

            sb.AppendLine(@"            _ => null
         };
    }
}");

            context.AddSource(
                $"{contractContext.Target.Name}.Encoders.g.cs",
                SourceText.From(sb.ToString(), Encoding.UTF8));
        }
    }
}

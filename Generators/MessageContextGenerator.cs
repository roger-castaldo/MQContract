using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.Contracts;
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

                contractContext = MergeEncodersAndConverters(contractContext, encoders, converters);

                GenerateDefinitionImplementation(context, contractContext);
                GenerateEncoderImplementation(context, contractContext);
                GenerateConverterImplementataion(context, contractContext, converters);
            }
        }

        private ContractContext MergeEncodersAndConverters(ContractContext contractContext, IEnumerable<ContractEncoder> encoders, IEnumerable<ContractConverter> converters)
        {
            return contractContext;
        }

        private static readonly string[] MessageAttributes = ["MessageAttribute", "QueryMessageAttribute", "CommandAttribute", "QueryAttribute"];

        private AttributeData? GetMessageAttribute(ITypeSymbol contract)
            => contract.GetAttributes().FirstOrDefault(a => MessageAttributes.Contains(a.AttributeClass?.Name));

        private string GetMessageIDUpperInvariant(ITypeSymbol contract)
        {
            var att = GetMessageAttribute(contract);
            return $"{((string?)att?.ConstructorArguments[1].Value)??contract.Name}-{((string?)att?.ConstructorArguments[2].Value)??"0.0.0.0"}".ToUpperInvariant();
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
                var att = GetMessageAttribute(contract.Contract);
                var channel = (string?)att?.ConstructorArguments[0].Value;
                var name = (string?)att?.ConstructorArguments[1].Value;
                var version = (string?)att?.ConstructorArguments[2].Value;
                var responseChannel = (string?)(att?.ConstructorArguments.Length>=4 ? att?.ConstructorArguments[3].Value : null);
                var responseTimeout = (int?)(att?.ConstructorArguments.Length>=5 ? att?.ConstructorArguments[4].Value : null);
                var responseType = (ITypeSymbol?)(att?.ConstructorArguments.Length>=6 ? att?.ConstructorArguments[5].Value : null);
                sb.AppendLine($"            (Type t) when t == typeof({contract.Contract.ToDisplayString()}) => new({(channel==null ? "null" : $"\"{channel}\"")}, \"{name??contract.Contract.Name}\",new Version(\"{version??"0.0.0.0"}\"), {(responseChannel==null ? "null" : $"\"{responseChannel}\"")},{(responseTimeout==null ? "null" : $"TimeSpan.FromMilliseconds({responseTimeout})")}, {(responseType == null ? "null" : $"typeof({responseType.ToDisplayString()})")}),");
            }
            sb.AppendLine(@"            _ => null
        };
    }
}");

            context.AddSource(
                $"{contractContext.Target.Name}.Definitions.g.cs",
                SourceText.From(sb.ToString(), Encoding.UTF8));
        }

        private void GenerateEncoderImplementation(SourceProductionContext context, ContractContext contractContext)
        {
            var typeSwitches = new List<string>();
            var idSwitches = new List<string>();

            foreach(var contract in contractContext.Contracts)
            {
                var messageId = GetMessageIDUpperInvariant(contract.Contract);
                if (contract.Encoders!=null)
                {
                    if (contract.Encoders.Count()==1)
                    {
                        var encoder = contract.Encoders.First();
                        typeSwitches.Add($@"            (Type t, _, not null) when t == typeof({contract.Contract.ToDisplayString()}) => ActivatorUtilities.CreateInstance<{encoder.ToDisplayString()}>(serviceProvider!),
            (Type t, _, null) when t == typeof({contract.Contract.ToDisplayString()}) => Activator.CreateInstance<{encoder.ToDisplayString()}>(),");
                        idSwitches.Add($@"            (""{messageId}"", not null, _) => ActivatorUtilities.CreateInstance<{encoder.ToDisplayString()}>(serviceProvider!),
            (""{messageId}"", null, _) => Activator.CreateInstance<{encoder.ToDisplayString()}>(),");
                    }
                    else
                    {
                        //faile here due to unknowns
                    }
                }
                else
                {
                    typeSwitches.Add($@"            (Type t, not null, _) when t == typeof({contract.Contract.ToDisplayString()}) => globalMessageEncoder,
            (Type t, null, _) when t == typeof({contract.Contract.ToDisplayString()}) => new DefaultJsonEncoder<{contract.Contract.ToDisplayString()}>(jsonOptions),");
                    idSwitches.Add($@"            (""{messageId}"", not null, _) => globalMessageEncoder,
            (""{messageId}"", null, _) => new DefaultJsonEncoder<{contract.Contract.ToDisplayString()}>(jsonOptions),");
                }
            }

            context.AddSource(
                $"{contractContext.Target.Name}.Encoders.g.cs",
                SourceText.From($@"using System;
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
        var jsonOptions = new JsonSerializerOptions(){{
            WriteIndented=false,
            AllowTrailingCommas=true,
            PropertyNameCaseInsensitive=true,
            ReadCommentHandling=JsonCommentHandling.Skip
        }};
        return (typeof(TMessage), globalMessageEncoder, serviceProvider) switch
        {{
{string.Join("\r\n",typeSwitches)}
            _ => null
        }};
    }}

    public override object TryGetMessageEncoder(string messageID, IMessageEncoder globalMessageEncoder, IServiceProvider serviceProvider){{
        var jsonOptions = new JsonSerializerOptions(){{
            WriteIndented=false,
            AllowTrailingCommas=true,
            PropertyNameCaseInsensitive=true,
            ReadCommentHandling=JsonCommentHandling.Skip
        }};
        return (messageID.ToUpperInvariant(), globalMessageEncoder, serviceProvider) switch
        {{
{string.Join("\r\n", idSwitches)}
            _ => null
        }};
    }}
}}", Encoding.UTF8));
        }

        private void GenerateConverterImplementataion(SourceProductionContext context, ContractContext contractContext, IEnumerable<ContractConverter> converters)
        {
            var encoderCalls = new Dictionary<string,string>();

            foreach(var contract in contractContext.Contracts.Where(c=>(c.Converters?.Any()??false)))
            {
                var converterDefinitions = contract.Converters.Select(converterType => converters.First(c => SymbolEqualityComparer.Default.Equals(c.Converter, converterType)));
                GeneratePrimaryConverters(encoderCalls, contract.Contract, converterDefinitions);
                foreach (var converter in converters.Where(con => con.Contracts.Any(c => SymbolEqualityComparer.Default.Equals(c.to, contract.Contract))))
                {
                    foreach (var pair in converter.Contracts.Where(c => SymbolEqualityComparer.Default.Equals(c.to, contract.Contract)))
                        GenerateChainedConverter(encoderCalls,pair.from, contract.Contract, [new(converter.Converter,pair.from,pair.to)], converterDefinitions);
                }
            }

            context.AddSource(
                $"{contractContext.Target.Name}.Converters.g.cs",
                SourceText.From($@"using System;
using MQContract;
using MQContract.Interfaces.Encoding;
using Microsoft.Extensions.DependencyInjection;
using MQContract.Interfaces.Messages;
using MQContract.Interfaces.Conversion;

namespace {contractContext.Target.ContainingNamespace};

{contractContext.Target.DeclaredAccessibility.ToString().ToLower()} partial class {contractContext.Target.Name} : MQContractMessageContext {{

    public override Func<IEncodedMessage, ValueTask<object>> TryGetMessageConverter<TMessage>(string messageID, Func<IEncodedMessage,ValueTask<object>> messageDecode, IServiceProvider serviceProvider){{
        Func<IServiceProvider,Func<IEncodedMessage, ValueTask<object>>> callback = (messageID.ToUpperInvariant(), typeof(TMessage)) switch
        {{
{string.Join("\r\n", encoderCalls.Select(pair=>$"           {pair.Key} => {pair.Value},"))}
            _ => null
        }};
        return (callback == null ? null : callback(serviceProvider));
    }}
}}", Encoding.UTF8));
        }

        private void GeneratePrimaryConverters(Dictionary<string, string> encoderCalls, INamedTypeSymbol contract, IEnumerable<ContractConverter> converters)
        {
            foreach (var converter in converters.Where(con=>con.Contracts.Any(c=> SymbolEqualityComparer.Default.Equals(c.to, contract))))
            {
                foreach (var conversion in converter.Contracts.Where(c => SymbolEqualityComparer.Default.Equals(c.to, contract))) {
                    var switchStatement = $"(\"{GetMessageIDUpperInvariant(conversion.from)}\", Type t) when t == typeof({contract.ToDisplayString()})";
                    encoderCalls.Remove(switchStatement);
                    encoderCalls.Add(switchStatement, BuildEncoderChain(converter, conversion, []));
                }
            }
        }

        private void GenerateChainedConverter(Dictionary<string, string> encoderCalls, ITypeSymbol from, INamedTypeSymbol contract, IEnumerable<ContractConverterPair> previousSteps, IEnumerable<ContractConverter> converterDefinitions)
        {
            foreach (var con in converterDefinitions.Where(con => con.Contracts.Any(c => SymbolEqualityComparer.Default.Equals(c.to, from))))
            {
                foreach (var conversion in con.Contracts.Where(c => SymbolEqualityComparer.Default.Equals(c.to, from)))
                {
                    var switchStatement = $"(\"{GetMessageIDUpperInvariant(conversion.from)}\", Type t) when t == typeof({contract.ToDisplayString()})";
                    if (!encoderCalls.ContainsKey(switchStatement))
                    {
                        encoderCalls.Add(switchStatement, BuildEncoderChain(con, conversion, previousSteps));
                        GenerateChainedConverter(encoderCalls, conversion.from, contract, new ContractConverterPair[] { new(con.Converter, conversion.from, conversion.to) }.Concat(previousSteps),converterDefinitions);
                    }
                }
            }
        }

        private string BuildEncoderChain(ContractConverter con, (ITypeSymbol from, ITypeSymbol to) conversion, IEnumerable<ContractConverterPair> previousSteps)
        {
            var sb = new StringBuilder();
            sb.AppendLine($@"(sp)=>
                {{
                    IMessageConverter<{conversion.from.ToDisplayString()},{conversion.to.ToDisplayString()}> step0 = (sp == null ? Activator.CreateInstance<{con.Converter.ToDisplayString()}>() : ActivatorUtilities.CreateInstance<{con.Converter.ToDisplayString()}>(sp!))!;");
            var idx = 1;
            foreach(var pair in previousSteps)
            {
                sb.AppendLine($"                    IMessageConverter<{pair.From.ToDisplayString()},{pair.To.ToDisplayString()}> step{idx} = (sp == null ? Activator.CreateInstance<{pair.Converter.ToDisplayString()}>() : ActivatorUtilities.CreateInstance<{pair.Converter.ToDisplayString()}>(sp!))!;");
                idx++;
            }
            sb.AppendLine($@"                    Func<IEncodedMessage, ValueTask<object>> func = async (encodedMessage) => {{
                        var msg = ({conversion.from.ToDisplayString()})(await messageDecode(encodedMessage));
                        if (msg==null) throw new Exception(""Unable to convert message"");
                        var msg0 = await step0.ConvertAsync(msg);");
            idx=0;
            foreach(var pair in previousSteps)
            {
                sb.AppendLine($@"                        var msg{idx+1} = await step{idx+1}.ConvertAsync(msg{idx});
                        if (msg{idx+1}==null) throw new Exception(""Unable to convert message"");");
                idx++;
            }
                        
sb.Append(@$"                        return msg{idx};
                    }};
                    return func;
                }}");
            return sb.ToString();
        }
    }
}

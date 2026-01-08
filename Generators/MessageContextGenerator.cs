using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System;
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
            //if (!Debugger.IsAttached)
            //{
            //    Debugger.Launch();
            //}
            // 1. Find candidate classes
            var candidateClasses = TargetHelper.LocateContexts(context);

            // 2. Find encoders
            var encoders = EncodersHelper.LocateEncoders(context);

            // 3. Find converters
            var converters = ConvertersHelper.LocateConverters(context);

            // 4. Find Encryptors
            var encryptors = EncryptorsHelper.LocateEncryptors(context);

            // 6. Generate code
            context.RegisterSourceOutput(
                candidateClasses
                    .Combine(encoders.Collect())
                    .Combine(converters.Collect())
                    .Combine(encryptors.Collect()),
                Generate
            );
        }

        private void Generate(SourceProductionContext context, (((ContractContext? Left, ImmutableArray<ContractEncoder?> Right) Left, ImmutableArray<ContractConverter?> Right) Left, ImmutableArray<ContractEncryptor?> Right) candidate)
        {
            if (candidate.Left.Left.Left.HasValue)
            {
                var contractContext = candidate.Left.Left.Left.Value;
                var encoders = candidate.Left.Left.Right.OfType<ContractEncoder>();
                var converters = candidate.Left.Right.OfType<ContractConverter>();
                var encryptors = candidate.Right.OfType<ContractEncryptor>();

                contractContext = MergeEncodersConvertersAndEncryptors(contractContext, encoders, converters, encryptors);

                GenerateCodeGeneratedImplementation(context, contractContext);
                GenerateDefinitionImplementation(context, contractContext);
                GenerateEncoderImplementation(context, contractContext);
                GenerateConverterImplementataion(context, contractContext, converters);
                GenerateEncryptorImplementation(context, contractContext);
                GenerateQueryResponseImplementation(context, contractContext);
            }
        }

        private ContractContext MergeEncodersConvertersAndEncryptors(ContractContext contractContext, IEnumerable<ContractEncoder> encoders, IEnumerable<ContractConverter> converters, IEnumerable<ContractEncryptor> encryptors)
            => new(
                contractContext.Target,
                contractContext.Settings,
                contractContext.Contracts.Select(contract =>
                {
                    var conEncoders = contract.Encoders;
                    var conConverters = contract.Converters;
                    var conEncryptors = contract.Encryptors;
                    if (conEncoders == null  && contractContext.Settings.LocateEncoders)
                        conEncoders = encoders.Where(enc => enc.Contracts.Any(c => SymbolEqualityComparer.Default.Equals(c, contract.Contract))).Select(enc => enc.Encoder).ToArray();
                    if (conEncryptors == null && contractContext.Settings.LocateEncryptors)
                        conEncryptors = encryptors.Where(enc => enc.Contracts.Any(c => SymbolEqualityComparer.Default.Equals(c, contract.Contract))).Select(enc => enc.Encryptor).ToArray();
                    if (conConverters==null && contractContext.Settings.LocateConverters)
                        conConverters = RecursivelyLocateConverters(contract.Contract, converters, new());
                    return new ContractType(
                        contract.Contract,
                        (conEncoders?.Count()==0 ? null : conEncoders),
                        conConverters,
                        (conEncryptors?.Count()==0 ? null : conEncryptors)
                    );
                }).ToImmutableArray()
            );

        private IEnumerable<ITypeSymbol>? RecursivelyLocateConverters(ITypeSymbol destination, IEnumerable<ContractConverter> converters, List<ITypeSymbol> currentList)
        {
            foreach(var converter in converters.Where(con=>con.Contracts.Any(pair=>SymbolEqualityComparer.Default.Equals(pair.to, destination))))
            {
                if (!currentList.Contains(converter.Converter))
                {
                    currentList.Add(converter.Converter);
                    foreach(var source in converter.Contracts.Select(c=>c.from))
                        RecursivelyLocateConverters(source, converters, currentList);
                }
            }
            return currentList;
        }

        private void GenerateCodeGeneratedImplementation(SourceProductionContext context, ContractContext contractContext)
        {
            context.AddSource(
                $"{contractContext.Target.Name}.CodeGenerated.g.cs",
                SourceText.From($@"#nullable enable
using MQContract;

namespace {contractContext.Target.ContainingNamespace};

{contractContext.Target.DeclaredAccessibility.ToString().ToLower()} partial class {contractContext.Target.Name} : MQContractMessageContext {{

    public override sealed bool IsMessageCodeGenerated<TMessage>() 
        => (typeof(TMessage)) switch {{
{string.Join("\r\n",contractContext.Contracts.Select(contract=>$"            (Type t) when t == typeof({contract.Contract.ToDisplayString()}) => true,"))}
            _ => false
        }};
}}", Encoding.UTF8));
        }

        private static readonly string[] MessageAttributes = ["MessageAttribute", "QueryMessageAttribute", "CommandAttribute", "QueryAttribute"];

        private AttributeData? GetMessageAttribute(ITypeSymbol contract)
            => contract.GetAttributes().FirstOrDefault(a => MessageAttributes.Contains(a.AttributeClass?.Name));

        private string GetMessageName(AttributeData? att, ITypeSymbol contract)
        {
            var name = (string?)att?.ConstructorArguments[1].Value??contract.Name;
            if (att==null && (!contract.ToDisplayString().EndsWith(name, StringComparison.InvariantCultureIgnoreCase) || string.IsNullOrWhiteSpace(contract.Name)))
            {
                name = contract.ToDisplayString();
                if (!string.IsNullOrWhiteSpace(contract.Name) && name.Contains(contract.Name, StringComparison.InvariantCultureIgnoreCase))
                    name = name.Substring(name.IndexOf(contract.Name, StringComparison.InvariantCultureIgnoreCase));
                name = FixInternalBrackets(name);
            }
            return name;
        }

        private string FixInternalBrackets(string name)
        {
            if (name.Contains('<'))
            {
                var preBracket = name.Substring(0, name.IndexOf("<")+1);
                var betweenBrackets = name.Substring(preBracket.Length, name.Length-1-preBracket.Length);
                return $"{preBracket}{FixInternalBrackets(betweenBrackets)}>";
            }else if (name.Contains(","))
            {
                var splt = name.Split(',');
                for (var x = 0; x<splt.Length; x++)
                    splt[x]=FixInternalBrackets(splt[x]);
                return string.Join(",", splt);
            }else if (name.Contains('.'))
                return name.Substring(name.LastIndexOf('.')+1);
            return name;
        }

        private string GetMessageIDUpperInvariant(ITypeSymbol contract)
        {
            var att = GetMessageAttribute(contract);
            return $"{GetMessageName(att,contract)}-{((string?)att?.ConstructorArguments[2].Value)??"0.0.0.0"}".ToUpperInvariant();
        }

        private void GenerateDefinitionImplementation(SourceProductionContext context, ContractContext contractContext)
        {
            var sb = new StringBuilder();
            sb.AppendLine($@"#nullable enable
using MQContract;

namespace {contractContext.Target.ContainingNamespace};

{contractContext.Target.DeclaredAccessibility.ToString().ToLower()} partial class {contractContext.Target.Name} : MQContractMessageContext {{
    public override sealed MessageTypeDefinition? TryGetMessageType(Type messageType){{
        return (messageType) switch {{");

            foreach (var contract in contractContext.Contracts)
            {
                var att = GetMessageAttribute(contract.Contract);
                var channel = (string?)att?.ConstructorArguments[0].Value;
                var name = GetMessageName(att, contract.Contract);
                var version = (string?)att?.ConstructorArguments[2].Value;
                var responseChannel = (string?)(att?.ConstructorArguments.Length>=4 ? att?.ConstructorArguments[3].Value : null);
                var responseTimeout = (int?)(att?.ConstructorArguments.Length>=5 ? att?.ConstructorArguments[4].Value : null);
                var responseType = (ITypeSymbol?)(att?.ConstructorArguments.Length>=6 ? att?.ConstructorArguments[5].Value : null);
                sb.AppendLine($"            (Type t) when t == typeof({contract.Contract.ToDisplayString()}) => new({(channel==null ? "null" : $"\"{channel}\"")}, \"{name}\",new Version(\"{version??"0.0.0.0"}\"), {(responseChannel==null ? "null" : $"\"{responseChannel}\"")},{(responseTimeout==null ? "null" : $"TimeSpan.FromMilliseconds({responseTimeout})")}, {(responseType == null ? "null" : $"typeof({responseType.ToDisplayString()})")}),");
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
                        idSwitches.Add($@"            (""{messageId}"", _, not null) => () => {{
                IMessageTypeEncoder<{contract.Contract.ToDisplayString()}> encoder = ActivatorUtilities.CreateInstance<{encoder.ToDisplayString()}>(serviceProvider!);
                Func<IEncodedMessage, ValueTask<object?>> callback = async (IEncodedMessage message) => {{
                    using var ms = new MemoryStream(message.Data.ToArray(), 0, message.Data.Length, false, true);
                    return (object?)(await encoder.DecodeAsync(ms));
                }};
                return callback;
            }},
            (""{messageId}"", _, null) => () => {{
                IMessageTypeEncoder<{contract.Contract.ToDisplayString()}> encoder = Activator.CreateInstance<{encoder.ToDisplayString()}>();
                Func<IEncodedMessage, ValueTask<object?>> callback = async (IEncodedMessage message) => {{
                    using var ms = new MemoryStream(message.Data.ToArray(), 0, message.Data.Length, false, true);
                    return (object?)(await encoder.DecodeAsync(ms));
                }};
                return callback;
            }},");
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
                    idSwitches.Add($@"            (""{messageId}"", not null, _) => () => {{
                Func<IEncodedMessage, ValueTask<object?>> callback = async (IEncodedMessage message) => {{
                    using var ms = new MemoryStream(message.Data.ToArray(), 0, message.Data.Length, false, true);
                    return (object?)(await globalMessageEncoder.DecodeAsync<{contract.Contract.ToDisplayString()}>(ms));
                }};
                return callback;
            }},
            (""{messageId}"", null, _) => () => {{
                IMessageTypeEncoder<{contract.Contract.ToDisplayString()}> encoder = new DefaultJsonEncoder<{contract.Contract.ToDisplayString()}>(jsonOptions);
                Func<IEncodedMessage, ValueTask<object?>> callback = async (IEncodedMessage message) => {{
                    using var ms = new MemoryStream(message.Data.ToArray(), 0, message.Data.Length, false, true);
                    return (object?)(await encoder.DecodeAsync(ms));
                }};
                return callback;
            }},");
                }
            }

            context.AddSource(
                $"{contractContext.Target.Name}.Encoders.g.cs",
                SourceText.From($@"#nullable enable
using System;
using System.Text.Json;
using MQContract;
using MQContract.Interfaces.Encoding;
using Microsoft.Extensions.DependencyInjection;
using MQContract.Interfaces.Messages;

namespace {contractContext.Target.ContainingNamespace};

{contractContext.Target.DeclaredAccessibility.ToString().ToLower()} partial class {contractContext.Target.Name} : MQContractMessageContext {{

    private class DefaultJsonEncoder<TMessage>(JsonSerializerOptions jsonOptions) : IMessageTypeEncoder<TMessage> {{
        public async ValueTask<TMessage?> DecodeAsync(Stream stream)
            => await JsonSerializer.DeserializeAsync<TMessage>(stream, options: jsonOptions);

        public ValueTask<byte[]> EncodeAsync(TMessage message)
            => ValueTask.FromResult(JsonSerializer.SerializeToUtf8Bytes<TMessage>(message, jsonOptions));
    }}

    public override sealed object? TryGetMessageEncoder<TMessage>(IMessageEncoder? globalMessageEncoder, IServiceProvider? serviceProvider){{
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

    public override sealed Func<IEncodedMessage, ValueTask<object?>>? TryGetDecodingCallback(string messageID, IMessageEncoder? globalMessageEncoder, IServiceProvider? serviceProvider){{
        var jsonOptions = new JsonSerializerOptions(){{
            WriteIndented=false,
            AllowTrailingCommas=true,
            PropertyNameCaseInsensitive=true,
            ReadCommentHandling=JsonCommentHandling.Skip
        }};
        Func<Func<IEncodedMessage, ValueTask<object?>>>? result = (messageID.ToUpperInvariant(), globalMessageEncoder, serviceProvider) switch
        {{
{string.Join("\r\n", idSwitches)}
            _ => null
        }};
        return (result == null ? null : result());
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
                SourceText.From($@"#nullable enable
using System;
using MQContract;
using MQContract.Interfaces.Encoding;
using Microsoft.Extensions.DependencyInjection;
using MQContract.Interfaces.Messages;
using MQContract.Interfaces.Conversion;

namespace {contractContext.Target.ContainingNamespace};

{contractContext.Target.DeclaredAccessibility.ToString().ToLower()} partial class {contractContext.Target.Name} : MQContractMessageContext {{

    public override sealed Func<IEncodedMessage, ValueTask<object?>>? TryGetMessageConverter<TMessage>(string messageID, Func<IEncodedMessage,ValueTask<object?>> messageDecode, IServiceProvider? serviceProvider){{
        Func<IServiceProvider?,Func<IEncodedMessage, ValueTask<object?>>>? callback = (messageID.ToUpperInvariant(), typeof(TMessage)) switch
        {{
{string.Join("\r\n", encoderCalls.Select(pair=>$"           {pair.Key} => {pair.Value},"))}
            _ => null
        }};
        return (callback == null ? null : callback(serviceProvider));
    }}
}}", Encoding.UTF8));
        }

        private void GenerateEncryptorImplementation(SourceProductionContext context, ContractContext contractContext)
        {
            var typeSwitches = new List<string>();
            foreach (var contract in contractContext.Contracts)
            {
                var messageId = GetMessageIDUpperInvariant(contract.Contract);
                if (contract.Encryptors!=null)
                {
                    if (contract.Encryptors.Count()==1)
                    {
                        var encryptor = contract.Encryptors.First();
                        typeSwitches.Add($@"            (Type t, _, not null) when t == typeof({contract.Contract.ToDisplayString()}) => (IMessageEncryptor)ActivatorUtilities.CreateInstance<{encryptor.ToDisplayString()}>(serviceProvider!),
            (Type t, _, null) when t == typeof({contract.Contract.ToDisplayString()}) => (IMessageEncryptor)Activator.CreateInstance<{encryptor.ToDisplayString()}>(),");
                    }
                    else
                    {
                        //throw error here
                    }
                }
                else
                    typeSwitches.Add($@"            (Type t, not null, _) when t == typeof({contract.Contract.ToDisplayString()}) => globalEncryptor,
            (Type t, null, _) when t == typeof({contract.Contract.ToDisplayString()}) => new NonEncryptor(),");
            }

            context.AddSource(
                $"{contractContext.Target.Name}.Encryptors.g.cs",
                SourceText.From($@"#nullable enable
using System;
using MQContract;
using MQContract.Messages;
using MQContract.Interfaces.Encrypting;
using Microsoft.Extensions.DependencyInjection;

namespace {contractContext.Target.ContainingNamespace};

{contractContext.Target.DeclaredAccessibility.ToString().ToLower()} partial class {contractContext.Target.Name} : MQContractMessageContext {{

    private sealed class NonEncryptor : IMessageEncryptor
    {{
        ValueTask<Stream> IMessageEncryptor.DecryptAsync(Stream stream, MessageHeader headers)
            => ValueTask.FromResult(stream);

        ValueTask<EncryptionResult> IMessageEncryptor.EncryptAsync(byte[] data)
            => ValueTask.FromResult<EncryptionResult>(new(null,data));
    }}

    public override sealed IMessageEncryptor? TryGetMessageEncryptor(Type messageType, IMessageEncryptor? globalEncryptor, IServiceProvider? serviceProvider){{
        return (messageType, globalEncryptor, serviceProvider) switch
        {{
{string.Join("\r\n", typeSwitches)}
            _ => null
        }};
    }}
}}", Encoding.UTF8));
        }

        private void GenerateQueryResponseImplementation(SourceProductionContext context, ContractContext contractContext)
        {
            var connectionSwitches = new List<string>();
            var multiConnectionSwitches = new List<string>();

            foreach(var contract in contractContext.Contracts)
            {
                var att = GetMessageAttribute(contract.Contract);
                if (att?.ConstructorArguments.Length>=6)
                {
                    var responseType = (ITypeSymbol)att.ConstructorArguments[5].Value;
                    connectionSwitches.Add($@"          (Type t) when t == typeof({contract.Contract.ToDisplayString()}) => async () => {{
                    var result = await contractConnection.QueryAsync<{contract.Contract.ToDisplayString()}, {responseType.ToDisplayString()}>(({contract.Contract.ToDisplayString()})message, timeout, channel, responseChannel, messageHeader, cancellationToken);
                    return new QueryResult<object>(result.ID, result.Header, result.Result, result.Error);
                }},");
                    multiConnectionSwitches.Add($@"          (Type t) when t == typeof({contract.Contract.ToDisplayString()}) => async () => {{
                    var result = await contractConnection.QueryAsync<{contract.Contract.ToDisplayString()}, {responseType.ToDisplayString()}>(({contract.Contract.ToDisplayString()})message, timeout, channel, responseChannel, messageHeader, cancellationToken);
                    return result.Select(r=>new QueryResult<object>(r.ID, r.Header, r.Result, r.Error));
                }},");
                }
            }

            context.AddSource(
                $"{contractContext.Target.Name}.QueryResponse.g.cs",
                SourceText.From($@"#nullable enable
using System;
using MQContract;
using MQContract.Messages;
using MQContract.Interfaces;

namespace {contractContext.Target.ContainingNamespace};

{contractContext.Target.DeclaredAccessibility.ToString().ToLower()} partial class {contractContext.Target.Name} : MQContractMessageContext {{

    public override sealed ValueTask<QueryResult<object>>? TryExecuteQuery<TQuery>(IContractConnection contractConnection, object message, TimeSpan? timeout, string? channel, string? responseChannel, MessageHeader? messageHeader, CancellationToken cancellationToken){{
        Func<ValueTask<QueryResult<object>>>? callback =  (typeof(TQuery)) switch
        {{
{string.Join("\r\n", connectionSwitches)}
            _ => null
        }};
        if (callback!=null)
            return callback();
        return null;
    }}

    public override sealed ValueTask<IEnumerable<QueryResult<object>>>? TryExecuteQuery<TQuery>(IMultiServiceContractConnection contractConnection, object message, TimeSpan? timeout, string? channel, string? responseChannel, MessageHeader? messageHeader, CancellationToken cancellationToken){{
        Func<ValueTask<IEnumerable<QueryResult<object>>>>? callback =  (typeof(TQuery)) switch
        {{
{string.Join("\r\n", multiConnectionSwitches)}
            _ => null
        }};
        if (callback!=null)
            return callback();
        return null;
    }}
}}", Encoding.UTF8));
        }

        private void GeneratePrimaryConverters(Dictionary<string, string> encoderCalls, ITypeSymbol contract, IEnumerable<ContractConverter> converters)
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

        private void GenerateChainedConverter(Dictionary<string, string> encoderCalls, ITypeSymbol from, ITypeSymbol contract, IEnumerable<ContractConverterPair> previousSteps, IEnumerable<ContractConverter> converterDefinitions)
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
            sb.AppendLine($@"                    Func<IEncodedMessage, ValueTask<object?>> func = async (encodedMessage) => {{
                        var msg = ({conversion.from.ToDisplayString()}?)(await messageDecode(encodedMessage));
                        if (msg==null) return null;
                        var msg0 = await step0.ConvertAsync(msg);");
            idx=0;
            foreach(var pair in previousSteps)
            {
                sb.AppendLine($@"                        var msg{idx+1} = await step{idx+1}.ConvertAsync(msg{idx});
                        if (msg{idx+1}==null) return null;");
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

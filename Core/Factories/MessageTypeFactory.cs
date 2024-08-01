﻿using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MQContract.Attributes;
using MQContract.Defaults;
using MQContract.Interfaces.Conversion;
using MQContract.Interfaces.Encoding;
using MQContract.Interfaces.Encrypting;
using MQContract.Interfaces.Factories;
using MQContract.Interfaces.Messages;
using MQContract.Messages;
using System.IO.Compression;
using System.Reflection;
using System.Runtime.Loader;
using System.Text.RegularExpressions;

namespace MQContract.Factories
{
    internal class MessageTypeFactory<T>
        : IMessageFactory<T> where T : class
    {
        private static Regex RegMetaData => new(@"^(U|C)-(.+)-((\d+\.)*(\d+))$", RegexOptions.Compiled, TimeSpan.FromMilliseconds(200));

        private readonly IMessageEncryptor? globalMessageEncryptor;
        private readonly IMessageEncoder? globalMessageEncoder;
        private readonly IMessageTypeEncoder<T>? messageEncoder;
        private readonly IMessageTypeEncryptor<T>? messageEncryptor;
        private readonly IEnumerable<IConversionPath<T>> converters;
        private readonly int maxMessageSize;
        public bool IgnoreMessageHeader { get; private init; }

        private readonly string messageName = typeof(T).GetCustomAttributes<MessageNameAttribute>().Select(mn => mn.Value).FirstOrDefault(Utility.TypeName<T>());
        private readonly string messageVersion = typeof(T).GetCustomAttributes<MessageVersionAttribute>().Select(mc => mc.Version.ToString()).FirstOrDefault("0.0.0.0");
        private readonly string messageChannel = typeof(T).GetCustomAttributes<MessageChannelAttribute>().Select(mc => mc.Name).FirstOrDefault(string.Empty);

        public MessageTypeFactory(IMessageEncoder? globalMessageEncoder, IMessageEncryptor? globalMessageEncryptor, IServiceProvider? serviceProvider, bool ignoreMessageHeader, int? maxMessageSize)
        {
            this.maxMessageSize = maxMessageSize??int.MaxValue;
            this.globalMessageEncryptor = globalMessageEncryptor;
            this.globalMessageEncoder = globalMessageEncoder;
            IgnoreMessageHeader = ignoreMessageHeader;
            var types = AssemblyLoadContext.All
                .SelectMany(context => context.Assemblies)
                .SelectMany(assembly =>
                {
                    try
                    {
                        return assembly.GetTypes()
                        .Where(t => !t.IsInterface && !t.IsAbstract
                            && Array.Exists(t.GetInterfaces(), iface => iface == typeof(IMessageTypeEncoder<T>)
                                || iface == typeof(IMessageTypeEncryptor<T>)
                                || iface.IsGenericType && iface.GetGenericTypeDefinition() == typeof(IMessageConverter<,>)));
                    }
                    catch (Exception)
                    {
                        return [];
                    }
                });
            var encoderType = types
                .FirstOrDefault(type => type.GetInterfaces().Contains(typeof(IMessageTypeEncoder<T>)));
            var encryptorType = types
                .FirstOrDefault(type => type.GetInterfaces().Contains(typeof(IMessageTypeEncryptor<T>)));
            messageEncoder = (IMessageTypeEncoder<T>?)((serviceProvider, encoderType, globalMessageEncoder) switch
            {
                (not null, not null, _) => ActivatorUtilities.CreateInstance(serviceProvider!, encoderType!),
                (null, not null, _) => Activator.CreateInstance(encoderType)!,
                (_, null, null) => new JsonEncoder<T>(),
                _ => null
            });
            messageEncryptor = (IMessageTypeEncryptor<T>?)((serviceProvider, encryptorType, globalMessageEncryptor) switch
            {
                (not null, not null, _) => ActivatorUtilities.CreateInstance(serviceProvider, encryptorType),
                (null, not null, _) => Activator.CreateInstance(encryptorType)!,
                (_, null, null) => new NonEncryptor<T>(),
                _ => null
            });
            converters = IgnoreMessageHeader
                ? []
                : ProduceConverters<T>(types,globalMessageEncoder, globalMessageEncryptor, serviceProvider);
        }

        private static IEnumerable<IConversionPath<M>> ProduceConverters<M>(IEnumerable<Type> types, IMessageEncoder? globalMessageEncoder, IMessageEncryptor? globalMessageEncryptor, IServiceProvider? serviceProvider)
            where M : class
        {
            var paths = types
                .Where(t => Array.Exists(t.GetInterfaces(), iface => iface.IsGenericType &&
                    iface.GetGenericTypeDefinition() == typeof(IMessageConverter<,>)
                    && iface.GetGenericArguments()[1] == typeof(M)
                ))
                .Select(t => (IEnumerable<object>)[
                    (serviceProvider == null ?
                        Activator.CreateInstance(t)! :
                        ActivatorUtilities.CreateInstance(serviceProvider, t)
                    )
                ])
                .ToList();

            for (var x = 0; x<paths.Count; x++)
            {
                var conv = paths[x];
                var destType = ExtractGenericArguements(conv.First().GetType())[0];
                paths.AddRange(
                    types
                    .Where(t => Array.Exists(t.GetInterfaces(), iface => iface.IsGenericType &&
                        iface.GetGenericTypeDefinition() == typeof(IMessageConverter<,>)
                        && iface.GetGenericArguments()[1] == destType
                        && !paths.Exists(path => Equals(ExtractGenericArguements(path.First().GetType())[0],iface.GetGenericArguments()[0]))
                    ))
                    .Select(t => conv.Prepend((serviceProvider == null ?
                        Activator.CreateInstance(t)! :
                        ActivatorUtilities.CreateInstance(serviceProvider, t)
                    )))
                    .ToArray()
                );
            }

            return paths
                .Select(path =>
                {
#pragma warning disable CS8601 // Possible null reference assignment.
                    var args = new object[] { path, types, globalMessageEncoder, globalMessageEncryptor, serviceProvider };
#pragma warning restore CS8601 // Possible null reference assignment.
                    var type = typeof(ConversionPath<,>).MakeGenericType(
                        ExtractGenericArguements(path.First().GetType())[0],
                        typeof(M)
                    );
                    return (IConversionPath<M>)(serviceProvider==null ? Activator.CreateInstance(type, args)! : ActivatorUtilities.CreateInstance(serviceProvider, type, args)!);
                });
        }

        private static Type[] ExtractGenericArguements(Type t) => t.GetInterfaces().First(iface => iface.IsGenericType && iface.GetGenericTypeDefinition()==typeof(IMessageConverter<,>)).GetGenericArguments();

        private static bool IsMessageTypeMatch(string metaData, Type t, out bool isCompressed)
        {
            isCompressed=false;
            var match = RegMetaData.Match(metaData);
            if (match.Success)
            {
                isCompressed=match.Groups[1].Value=="C";
                if (match.Groups[2].Value==t.GetCustomAttributes<MessageNameAttribute>().Select(mn => mn.Value).FirstOrDefault(Utility.TypeName(t))
                    && new Version(match.Groups[3].Value)==new Version(t.GetCustomAttributes<MessageVersionAttribute>().Select(mc => mc.Version.ToString()).FirstOrDefault("0.0.0.0")))
                    return true;

            }
            else
                throw new InvalidDataException("MetaData is not valid");
            return false;
        }

        public async Task<ServiceMessage> ConvertMessageAsync(T message, string? channel, MessageHeader? messageHeader, Func<string, Task<string>>? mapChannel=null)
        {
            channel ??= messageChannel;
            if (string.IsNullOrWhiteSpace(channel))
                throw new MessageChannelNullException();

            var encodedData = messageEncoder?.Encode(message)??globalMessageEncoder!.Encode<T>(message);
            var body = messageEncryptor?.Encrypt(encodedData, out var messageHeaders)??globalMessageEncryptor!.Encrypt(encodedData, out messageHeaders);

            var metaData = string.Empty;
            if (body.Length>maxMessageSize)
            {
                using var ms = new MemoryStream();
                var zip = new GZipStream(ms, System.IO.Compression.CompressionLevel.SmallestSize, false);
                await zip.WriteAsync(body, 0, body.Length);
                await zip.FlushAsync();
                body = ms.ToArray();
                metaData = "C";

                if (body.Length > maxMessageSize)
                    throw new ArgumentOutOfRangeException(nameof(message), $"message data exceeds maxmium message size (MaxSize:{maxMessageSize},EncodedSize:{body.Length})");
            }
            else
                metaData="U";
            metaData+=$"-{messageName}-{messageVersion}";
            if (mapChannel!=null)
                channel=await mapChannel(channel);

            return new ServiceMessage(Guid.NewGuid().ToString(), metaData, channel, new MessageHeader(messageHeader, messageHeaders), body);
        }

        T? IConversionPath<T>.ConvertMessage(ILogger? logger, IEncodedMessage message, Stream? dataStream)
        {
            if (!IgnoreMessageHeader)
#pragma warning disable S3236 // Caller information arguments should not be provided explicitly
                ArgumentNullException.ThrowIfNullOrWhiteSpace(message.MessageTypeID, nameof(message.MessageTypeID));
#pragma warning restore S3236 // Caller information arguments should not be provided explicitly
            if (Equals(ErrorServiceMessage.MessageTypeID, message.MessageTypeID))
                throw ErrorServiceMessage.DecodeError(message.Data);
            IConversionPath<T>? converter = null;
            T? result;
            var compressed = false;
            if (IgnoreMessageHeader || IsMessageTypeMatch(message.MessageTypeID, typeof(T), out compressed))
            {
                dataStream = (compressed ? new GZipStream(new MemoryStream(message.Data.ToArray()), System.IO.Compression.CompressionMode.Decompress) : new MemoryStream(message.Data.ToArray()));
                dataStream = messageEncryptor?.Decrypt(dataStream, message.Header)??globalMessageEncryptor!.Decrypt(dataStream, message.Header);
                if (messageEncoder!=null)
                    result = messageEncoder.Decode(dataStream);
                else
                    result = globalMessageEncoder!.Decode<T>(dataStream);
            }
            else
            {
                foreach (var conv in converters)
                {
                    if (IsMessageTypeMatch(message.MessageTypeID, conv.GetType().GetGenericArguments()[0], out compressed))
                    {
                        converter=conv;
                        break;
                    }
                }
                if (converter==null)
                    throw new InvalidCastException();
                dataStream = (compressed ? new GZipStream(new MemoryStream(message.Data.ToArray()), System.IO.Compression.CompressionMode.Decompress) : new MemoryStream(message.Data.ToArray()));
                result = converter.ConvertMessage(logger, message, dataStream: dataStream);
            }
            if (Equals(result, default(T?)))
                throw new MessageConversionException(typeof(T), converter?.GetType()??GetType());
            return result;
        }
    }
}

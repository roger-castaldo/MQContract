using Confluent.SchemaRegistry;
using Microsoft.Extensions.Caching.Memory;
using MQContract.Interfaces.Middleware;
using MQContract.Messages;
using NJsonSchema;
using System.Buffers.Binary;


namespace MQContract.Kafka.Middleware
{
    /// <summary>
    /// Used to inject a confluent schema registry validation middleware that will use the confluent style schema registry to validate message types and 
    /// attach schema information to each message
    /// </summary>
    /// <param name="schemaRegistryClient">A schema registry client used to validate messages</param>
    /// <param name="cache">A memory cache to be used to cache resolved schemas if desired</param>
    /// <param name="failOnMissingSchema">Indicates if the message should fail when no schema is available</param>
    /// <param name="autoRegisterSchema">Indicates if the system should attempt to register a schema when one is not found on the publish side</param>
    /// <param name="registerSchemaType">The default schema registration type to use when registering a schema</param>
    /// <param name="mapMessageSchemaName">A callback used to change the schema name for a given message, by default it will use the MessageTypeID.  The arguments pass will be the type (class) of the message, the topic and the message type id and is expecting a string with for the schema identifier.</param>
    /// <param name="extractSchemaAsync">An alternative call to generate the schema for a given message type, otherwise NJsonSchema will be used</param>
    /// <param name="validateSchemaAsync">An alternative call to validate the schema and the incoming message content, otherwise it will default through NJsonSchema</param>
    public class SchemaValidationMiddleware(ISchemaRegistryClient schemaRegistryClient,
        IMemoryCache? cache = null,
        bool failOnMissingSchema = true,
        bool autoRegisterSchema = true,
        Confluent.SchemaRegistry.SchemaType registerSchemaType = Confluent.SchemaRegistry.SchemaType.Json,
        Func<Type, string, string, ValueTask<string>>? mapMessageSchemaName = null,
        Func<Type, ValueTask<string>>? extractSchemaAsync = null,
        Func<Schema, Stream, ValueTask<bool>>? validateSchemaAsync = null
    ) : IAfterEncodeMiddleware, IBeforeDecodeMiddleware
    {
        private const string SchemaIdHeader = "_kafkaSchemaId";
        private const byte MagicByte = 0x00;
        private readonly static string[] IgnoredMessageTypes = [
            "byte[]-0.0.0.0",
            "Byte-0.0.0.0",
            "bool-0.0.0.0",
            "bool[]-0.0.0.0",
            "IEnumerable<bool>-0.0.0.0",
            "Char-0.0.0.0",
            "Decimal-0.0.0.0",
            "decimal[]-0.0.0.0",
            "IEnumerable<decimal>-0.0.0.0",
            "Double-0.0.0.0",
            "double[]-0.0.0.0",
            "IEnumerable<double>-0.0.0.0",
            "float-0.0.0.0",
            "float[]-0.0.0.0",
            "IEnumerable<float>-0.0.0.0",
            "Half-0.0.0.0",
            "Half[]-0.0.0.0",
            "IEnumerable<Half>-0.0.0.0",
            "int-0.0.0.0",
            "int[]-0.0.0.0",
            "IEnumerable<int>-0.0.0.0",
            "long-0.0.0.0",
            "long[]-0.0.0.0",
            "IEnumerable<long>-0.0.0.0",
            "short-0.0.0.0",
            "short[]-0.0.0.0",
            "IEnumerable<short>-0.0.0.0",
            "String-0.0.0.0",
            "uint-0.0.0.0",
            "uint[]-0.0.0.0",
            "IEnumerable<uint>-0.0.0.0",
            "ulong-0.0.0.0",
            "ulong[]-0.0.0.0",
            "IEnumerable<ulong>-0.0.0.0",
            "ushort-0.0.0.0",
            "ushort[]-0.0.0.0",
            "IEnumerable<ushort>-0.0.0.0"
        ];

        private sealed record CachedSchema(int Id, Schema Schema, JsonSchema? CompiledSchema);

        private readonly MemoryCacheEntryOptions cacheOptions = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5),
            SlidingExpiration = TimeSpan.FromMinutes(5)
        };

        private async Task<CachedSchema> CacheSchema(string schemaName, int schemaId, Schema schema)
        {
            var cacheItem = new CachedSchema(schemaId, schema, (schema.SchemaType == Confluent.SchemaRegistry.SchemaType.Json && validateSchemaAsync==null ? await JsonSchema.FromJsonAsync(schema.SchemaString) : null));
            cache?.Set(schemaName, cacheItem, cacheOptions);
            cache?.Set($"SchemaById_{schemaId}", cacheItem, cacheOptions);
            return cacheItem;
        }

        private async Task LoadAndCheckSchemaAsync(int schemaId, string messageTypeID, ReadOnlyMemory<byte> data)
        {
            CachedSchema? cachedSchema = null;
            if (!(cache?.TryGetValue($"SchemaById_{schemaId}", out cachedSchema)??false))
            {
                var schema = await schemaRegistryClient.GetSchemaAsync(schemaId);
                if (schema!=null)
                    cachedSchema = await CacheSchema(messageTypeID, schemaId, schema);
            }
            if (cachedSchema == null && failOnMissingSchema)
                throw new MissingSchemaException(messageTypeID);
            else if (cachedSchema!=null && !(await ValidateSchemaAsync(cachedSchema, new MemoryStream(data.ToArray(), 0, data.Length, false, true))))
                throw new SchemaValidationFailedException(schemaId, messageTypeID);
        }

        async ValueTask<ServiceMessage> IAfterEncodeMiddleware.AfterMessageEncodeAsync(Type messageType, IContext context, ServiceMessage message)
        {
            if (!IgnoredMessageTypes.Contains(message.MessageTypeID))
            {
                var schemaName = (mapMessageSchemaName==null ? message.MessageTypeID : await mapMessageSchemaName(messageType, message.Channel, message.MessageTypeID));
                int? schemaId = await GetSchemaIdFromCacheAsync(schemaName);
                if (schemaId==null && autoRegisterSchema)
                {
                    var builtSchema = new Schema(await ExtractSchemaAsync(messageType), registerSchemaType);
                    schemaId = await schemaRegistryClient.RegisterSchemaAsync(schemaName, builtSchema);
                    await CacheSchema(schemaName, schemaId.Value, builtSchema);
                }
                else if (schemaId!=null)
                    await LoadAndCheckSchemaAsync(schemaId.Value, message.MessageTypeID, message.Data);
                if (schemaId==null && failOnMissingSchema)
                    throw new MissingSchemaException(message.MessageTypeID);
                else if (schemaId!=null)
                {
                    var data = new byte[message.Data.Length+5];
                    data[0] = MagicByte;
                    BinaryPrimitives.WriteInt32BigEndian(data.AsSpan(1, 4), schemaId.Value);
                    message.Data.ToArray().CopyTo(data, 5);
                    message = new(
                        message.ID,
                        message.MessageTypeID,
                        message.Channel,
                        new(message.Header, new Dictionary<string, string?>() { { SchemaIdHeader, schemaId.ToString() } }),
                        data
                    );
                }
            }
            return message;
        }

        private async ValueTask<int?> GetSchemaIdFromCacheAsync(string schemaName)
        {
            int? schemaId = null;
            if (cache?.TryGetValue(schemaName, out CachedSchema? cachedSchema)??false)
                schemaId = cachedSchema!.Id;
            else
            {
                try
                {
                    var schemaResult = await schemaRegistryClient.GetLatestSchemaAsync(schemaName);
                    if (schemaResult!=null)
                    {
                        schemaId = schemaResult.Id;
                        await CacheSchema(schemaName, schemaId.Value, schemaResult.Schema);
                    }
                }
                catch
                {
                    schemaId=null;
                }
            }
            return schemaId;
        }

        private async ValueTask<string> ExtractSchemaAsync(Type messageType)
        {
            if (extractSchemaAsync!=null)
                return await extractSchemaAsync(messageType);
            else if (registerSchemaType == Confluent.SchemaRegistry.SchemaType.Json)
                return JsonSchema.FromType(messageType).ToJson();
            throw new NotImplementedException();
        }

        async ValueTask<DecodableMessage> IBeforeDecodeMiddleware.BeforeMessageDecodeAsync(IContext context, string id, string messageTypeID, string messageChannel, DecodableMessage message)
        {
            if (!IgnoredMessageTypes.Contains(messageTypeID))
            {
                var schemaId = message.MessageHeader[SchemaIdHeader];
                if (message.Data.Span[0]==MagicByte)
                {
                    var otherSchemaId = BinaryPrimitives.ReadInt32BigEndian(message.Data.Slice(1, 4).Span).ToString();
                    message=new(message.MessageHeader, message.Data.Slice(5));
                    if (schemaId!=otherSchemaId)
                        schemaId= otherSchemaId;
                }
                if (string.IsNullOrWhiteSpace(schemaId) && failOnMissingSchema)
                    throw new MissingSchemaException(messageTypeID);
                else if (!string.IsNullOrWhiteSpace(schemaId))
                    await LoadAndCheckSchemaAsync(int.Parse(schemaId), schemaId, message.Data);
            }
            return message;
        }

        private async ValueTask<bool> ValidateSchemaAsync(CachedSchema cachedSchema, Stream dataStream)
        {
            if (validateSchemaAsync!=null)
                return await validateSchemaAsync(cachedSchema.Schema, dataStream);
            else if (cachedSchema.CompiledSchema!=null)
                return cachedSchema.CompiledSchema.Validate(await new StreamReader(dataStream).ReadToEndAsync()).Count==0;
            return false;
        }
    }
}

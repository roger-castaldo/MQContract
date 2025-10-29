using Confluent.SchemaRegistry;
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
    /// <param name="failOnMissingSchema">Indicates if the message should fail when no schema is available</param>
    /// <param name="autoRegisterSchema">Indicates if the system should attempt to register a schema when one is not found on the publish side</param>
    /// <param name="registerSchemaType">The default schema registration type to use when registering a schema</param>
    /// <param name="mapMessageSchemaName">A callback used to change the schema name for a given message, by default it will use the MessageTypeID.  The arguments pass will be the type (class) of the message, the topic and the message type id and is expecting a string with for the schema identifier.</param>
    /// <param name="extractSchemaAsync">An alternative call to generate the schema for a given message type, otherwise NJsonSchema will be used</param>
    /// <param name="validateSchemaAsync">An alternative call to validate the schema and the incoming message content, otherwise it will default through NJsonSchema</param>
    public class SchemaValidationMiddleware(ISchemaRegistryClient schemaRegistryClient,
        bool failOnMissingSchema=true,
        bool autoRegisterSchema=true,
        Confluent.SchemaRegistry.SchemaType registerSchemaType = Confluent.SchemaRegistry.SchemaType.Json,
        Func<Type,string,string,ValueTask<string>>? mapMessageSchemaName = null,
        Func<Type,ValueTask<string>>? extractSchemaAsync = null,
        Func<Schema,Stream,ValueTask<bool>>? validateSchemaAsync = null
    ) : IAfterEncodeMiddleware, IBeforeDecodeMiddleware
    {
        private const string SchemaIdHeader = "_kafkaSchemaId";
        private const byte MagicByte = 0x00;
        private readonly static string[] IgnoredMessageTypes = [
            $"{typeof(ushort).Name}-0.0.0.0",
            $"{typeof(ushort[]).Name}-0.0.0.0",
            $"{typeof(IEnumerable<ushort>).Name}-0.0.0.0",
            $"{typeof(string).Name}-0.0.0.0",
            $"{typeof(char).Name}-0.0.0.0",
            $"{typeof(short).Name}-0.0.0.0",
            $"{typeof(short[]).Name}-0.0.0.0",
            $"{typeof(IEnumerable<short>).Name}-0.0.0.0",
            $"{typeof(long).Name}-0.0.0.0",
            $"{typeof(long[]).Name}-0.0.0.0",
            $"{typeof(IEnumerable<long>).Name}-0.0.0.0",
            $"{typeof(ulong).Name}-0.0.0.0",
            $"{typeof(ulong[]).Name}-0.0.0.0",
            $"{typeof(IEnumerable<ulong>).Name}-0.0.0.0",
            $"{typeof(uint).Name}-0.0.0.0",
            $"{typeof(uint[]).Name}-0.0.0.0",
            $"{typeof(IEnumerable<uint>).Name}-0.0.0.0",
            $"{typeof(int).Name}-0.0.0.0",
            $"{typeof(int[]).Name}-0.0.0.0",
            $"{typeof(IEnumerable<int>).Name}-0.0.0.0",
            $"{typeof(Half).Name}-0.0.0.0",
            $"{typeof(Half[]).Name}-0.0.0.0",
            $"{typeof(IEnumerable<Half>).Name}-0.0.0.0",
            $"{typeof(float).Name}-0.0.0.0",
            $"{typeof(float[]).Name}-0.0.0.0",
            $"{typeof(IEnumerable<float>).Name}-0.0.0.0",
            $"{typeof(double).Name}-0.0.0.0",
            $"{typeof(double[]).Name}-0.0.0.0",
            $"{typeof(IEnumerable<double>).Name}-0.0.0.0",
            $"{typeof(decimal).Name}-0.0.0.0",
            $"{typeof(decimal[]).Name}-0.0.0.0",
            $"{typeof(IEnumerable<decimal>).Name}-0.0.0.0",
            $"{typeof(byte).Name}-0.0.0.0",
            $"{typeof(byte[]).Name}-0.0.0.0",
            $"{typeof(bool).Name}-0.0.0.0",
            $"{typeof(bool[]).Name}-0.0.0.0",
            $"{typeof(IEnumerable<bool>).Name}-0.0.0.0"
        ];

        private async Task LoadAndCheckSchemaAsync(int schemaId, string messageTypeID, ReadOnlyMemory<byte> data)
        {
            var schema = await schemaRegistryClient.GetSchemaAsync(schemaId);
            if (schema == null && failOnMissingSchema)
                throw new MissingSchemaException(messageTypeID);
            else if (schema!=null && !(await ValidateSchemaAsync(schema, new MemoryStream(data.ToArray()))))
                throw new SchemaValidationFailedException(schemaId, messageTypeID);
        }

        async ValueTask<ServiceMessage> IAfterEncodeMiddleware.AfterMessageEncodeAsync(Type messageType, IContext context, ServiceMessage message)
        {
            if (!IgnoredMessageTypes.Contains(message.MessageTypeID))
            {
                int? schemaId;
                var schehmaName = (mapMessageSchemaName==null ? message.MessageTypeID : await mapMessageSchemaName(messageType, message.Channel, message.MessageTypeID));
                try
                {
                    schemaId = (await schemaRegistryClient.GetLatestSchemaAsync(schehmaName))?.Id;
                }
                catch {
                    schemaId=null;
                }
                if (schemaId==null && autoRegisterSchema)
                        schemaId = await schemaRegistryClient.RegisterSchemaAsync(schehmaName, new Schema(await ExtractSchemaAsync(messageType), registerSchemaType));
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
                    return new(
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

        private async ValueTask<string> ExtractSchemaAsync(Type messageType)
        {
            if (extractSchemaAsync!=null)
                return await extractSchemaAsync(messageType);
            else if (registerSchemaType == Confluent.SchemaRegistry.SchemaType.Json)
                return JsonSchema.FromType(messageType).ToJson();
            throw new NotImplementedException();
        }

        async ValueTask<(MessageHeader messageHeader, ReadOnlyMemory<byte> data)> IBeforeDecodeMiddleware.BeforeMessageDecodeAsync(IContext context, string id, MessageHeader messageHeader, string messageTypeID, string messageChannel, ReadOnlyMemory<byte> data)
        {
            if (!IgnoredMessageTypes.Contains(messageTypeID))
            {
                var schemaId = messageHeader[SchemaIdHeader];
                if (data.Span[0]==MagicByte)
                {
                    var otherSchemaId = BinaryPrimitives.ReadInt32BigEndian(data.Slice(1, 4).Span).ToString();
                    data = data.Slice(5);
                    if (schemaId!=otherSchemaId)
                        schemaId= otherSchemaId;
                }
                if (string.IsNullOrWhiteSpace(schemaId) && failOnMissingSchema)
                    throw new MissingSchemaException(messageTypeID);
                else if (!string.IsNullOrWhiteSpace(schemaId))
                    await LoadAndCheckSchemaAsync(int.Parse(schemaId), schemaId, data);
            }
            return (messageHeader,data);
        }

        private async ValueTask<bool> ValidateSchemaAsync(Schema schema, Stream dataStream)
        {
            if (validateSchemaAsync!=null)
                return await validateSchemaAsync(schema, dataStream);
            else if (schema.SchemaType == Confluent.SchemaRegistry.SchemaType.Json)
            {
                var jSchema = await JsonSchema.FromJsonAsync(schema.SchemaString);
                return jSchema.Validate(await new StreamReader(dataStream).ReadToEndAsync()).Count==0;
            }
            return false;
        }
    }
}

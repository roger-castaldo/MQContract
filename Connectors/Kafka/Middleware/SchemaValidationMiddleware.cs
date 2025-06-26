using Confluent.SchemaRegistry;
using MQContract.Interfaces.Middleware;
using MQContract.Messages;
using NJsonSchema;
using System.Buffers.Binary;


namespace MQContract.Kafka.Middleware
{
    public class SchemaValidationMiddleware(ISchemaRegistryClient schemaRegistryClient,
        bool failOnMissingSchema=true,
        bool autoRegisterSchema=true,
        Confluent.SchemaRegistry.SchemaType registerSchemaType = Confluent.SchemaRegistry.SchemaType.Json,
        Func<Type,ValueTask<string>>? extractSchemaAsync = null,
        Func<Schema,Stream,ValueTask<bool>>? validateSchemaAsync = null
    ) : IAfterEncodeMiddleware, IBeforeDecodeMiddleware
    {
        private const string SchemaIdHeader = "_kafkaSchemaId";

        async ValueTask<ServiceMessage> IAfterEncodeMiddleware.AfterMessageEncodeAsync(Type messageType, IContext context, ServiceMessage message)
        {
            var schemaId = (await schemaRegistryClient.GetLatestSchemaAsync(message.MessageTypeID))?.Id;
            if (schemaId==null)
            {
                if (autoRegisterSchema)
                    schemaId = await schemaRegistryClient.RegisterSchemaAsync(message.MessageTypeID, new Schema(await ExtractSchemaAsync(messageType), registerSchemaType));
            }
            if (schemaId==null && failOnMissingSchema)
                throw new Exception("This is a dummy for now");
            else if (schemaId!=null)
                return new(
                    message.ID,
                    message.MessageTypeID,
                    message.Channel,
                    new(message.Header, new Dictionary<string, string?>() { { SchemaIdHeader, schemaId?.ToString() } }),
                    message.Data
                );
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
            var schemaId = messageHeader[SchemaIdHeader];
            if (string.IsNullOrWhiteSpace(schemaId))
            {
                if (data.Span[0]==0)
                {
                    schemaId = BinaryPrimitives.ReadInt32BigEndian(data.Slice(1, 4).Span).ToString();
                    data = data.Slice(5);
                }
            }
            if (string.IsNullOrWhiteSpace(schemaId) && failOnMissingSchema)
                throw new Exception("This is a dummy for now");
            else if (!string.IsNullOrWhiteSpace(schemaId))
            {
                var schema = await schemaRegistryClient.GetSchemaAsync(int.Parse(schemaId));
                if (schema == null && failOnMissingSchema)
                    throw new Exception("This is a dummy for now");
                else if (schema!=null && !(await ValidateSchemaAsync(schema, new MemoryStream(data.ToArray()))))
                    throw new Exception("This is a dummy for now");
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

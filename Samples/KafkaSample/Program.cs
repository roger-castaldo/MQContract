using Confluent.SchemaRegistry;
using Messages;
using Microsoft.Extensions.Caching.Memory;
using MQContract.Kafka;
using MQContract.Kafka.Middleware;

var cacheOptions = new MemoryCacheOptions();

using var cache = new MemoryCache(cacheOptions);


var serviceConnection = new Connection(new Confluent.Kafka.ClientConfig()
{
    ClientId="KafkaSample",
    BootstrapServers="localhost:9092"
});

#pragma warning disable S1075 // URIs should not be hardcoded
//This is a sample program with a localhost connection so this is necessary
var schemaRegistryConfig = new SchemaRegistryConfig
{
    // URL to your Schema Registry (local, dev, or Confluent Cloud)
    Url = "http://localhost:8081",

    // Optional authentication if using Confluent Cloud
    // BasicAuthCredentialsSource = AuthCredentialsSource.UserInfo,
    // BasicAuthUserInfo = "API_KEY:API_SECRET"
};
#pragma warning restore S1075 // URIs should not be hardcoded

using var schemaRegistryClient = new CachedSchemaRegistryClient(schemaRegistryConfig);

await SampleExecution.ExecuteSample(serviceConnection, "Kafka", middlewares: [
    new SchemaValidationMiddleware(
        schemaRegistryClient,
        mapMessageSchemaName:(messageType,messageChannel,messageTypeId)=>ValueTask.FromResult<string>($"{messageChannel}-{messageTypeId}"),
        cache: cache
    )
]);
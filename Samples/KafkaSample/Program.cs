using Confluent.SchemaRegistry;
using Messages;
using MQContract.Kafka;
using MQContract.Kafka.Middleware;

var serviceConnection = new Connection(new Confluent.Kafka.ClientConfig()
{
    ClientId="KafkaSample",
    BootstrapServers="localhost:9092"
});

var schemaRegistryConfig = new SchemaRegistryConfig
{
    // URL to your Schema Registry (local, dev, or Confluent Cloud)
    Url = "http://localhost:8081",

    // Optional authentication if using Confluent Cloud
    // BasicAuthCredentialsSource = AuthCredentialsSource.UserInfo,
    // BasicAuthUserInfo = "API_KEY:API_SECRET"
};

using var schemaRegistryClient = new CachedSchemaRegistryClient(schemaRegistryConfig);

await SampleExecution.ExecuteSample(serviceConnection, "Kafka", middlewares: [
    new SchemaValidationMiddleware(schemaRegistryClient)
]);
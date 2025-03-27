using Google.Api.Gax;
using Google.Cloud.PubSub.V1;
using Grpc.Core;
using Messages;
using MQContract.GooglePubSub;

const string projectId = "sample-project-id";
const string endpoint = "localhost:8085";

var publisherService = await new PublisherServiceApiClientBuilder
{
    EmulatorDetection = EmulatorDetection.EmulatorOrProduction,
    Endpoint=endpoint,
    ChannelCredentials = ChannelCredentials.Insecure
}.BuildAsync();

var subscriberService = await new SubscriberServiceApiClientBuilder
{
    EmulatorDetection = EmulatorDetection.EmulatorOrProduction,
    Endpoint=endpoint,
    ChannelCredentials = ChannelCredentials.Insecure
}.BuildAsync();

foreach (var name in new string[] { "Arrivals", "Greeting", "Greeting.Response", "StoredArrivals" })
{
    var topicName = new TopicName(projectId, name);
    if (await publisherService.GetTopicAsync(topicName)==null)
        await publisherService.CreateTopicAsync(topicName);
}


var serviceConnection = new Connection(projectId, publisherService, subscriberService);

await SampleExecution.ExecuteSample(serviceConnection, "GooglePubSub");
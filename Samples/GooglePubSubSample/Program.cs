using Google.Api.Gax;
using Google.Cloud.PubSub.V1;
using Grpc.Core;
using Messages;
using MQContract.GooglePubSub;

const string projectId = "sample-project-id";
const string endpoint = "localhost:8085";

var publisherServiceBuilder = new PublisherServiceApiClientBuilder
{
    EmulatorDetection = EmulatorDetection.EmulatorOrProduction,
    Endpoint=endpoint,
    ChannelCredentials = ChannelCredentials.Insecure
};

var subscriberServiceBuilder = new SubscriberServiceApiClientBuilder
{
    EmulatorDetection = EmulatorDetection.EmulatorOrProduction,
    Endpoint=endpoint,
    ChannelCredentials = ChannelCredentials.Insecure
};

var serviceConnection = new Connection(projectId, publisherServiceBuilder, subscriberServiceBuilder);

foreach (var name in new string[] { "Arrivals", "Greeting", "Greeting.Response", "StoredArrivals" })
{
    var topicName = new TopicName(projectId, name);
    if (await serviceConnection.PublisherServiceApi.GetTopicAsync(topicName)==null)
        await serviceConnection.PublisherServiceApi.CreateTopicAsync(topicName);
}




await SampleExecution.ExecuteSample(serviceConnection, "GooglePubSub");
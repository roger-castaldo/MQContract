using Amazon.SimpleNotificationService;
using Amazon.SQS;
using Messages;
using MQContract;
using MQContract.AmazonSNQS;

#pragma warning disable S1075 // URIs should not be hardcoded
//This is a sample program with a localhost connection so this is necessary
const string ServiceURL = "http://localhost:4566";
#pragma warning restore S1075 // URIs should not be hardcoded
const string QueueAttributeName = "QueueArn";

var mapper = new ChannelMapper();
mapper.AddQueryResponseMap("Greeting.Response","Greeting_Response");

var credentials = new Amazon.Runtime.BasicAWSCredentials("test", "test");

var config = new AmazonSQSConfig { ServiceURL = ServiceURL };

var snsConfig = new AmazonSimpleNotificationServiceConfig { ServiceURL = ServiceURL };

var serviceConnection = new Connection(snsClientConfiguration:(credentials,snsConfig),sqsClientConfiguration:(credentials,config));

var arrivalsSNSResponse = await serviceConnection.SNSClient!.CreateTopicAsync("Arrivals");
var storedArrivalsSNSResponse = await serviceConnection.SNSClient!.CreateTopicAsync("StoredArrivals");
var greetingSNSResponse = await serviceConnection.SNSClient!.CreateTopicAsync("Greeting");
var greetingResponseSNSResponse = await serviceConnection.SNSClient!.CreateTopicAsync("Greeting_Response");

var arrivalsSQSResponse = await serviceConnection.SQSClient!.CreateQueueAsync("Arrivals");

var queueArn = (await serviceConnection.SQSClient!.GetQueueAttributesAsync(arrivalsSQSResponse.QueueUrl, [QueueAttributeName])).QueueARN;
await serviceConnection.SQSClient!.SetQueueAttributesAsync(arrivalsSQSResponse.QueueUrl, new() {
    { "Policy",$@"{{
            ""Version"": ""2012-10-17"",
            ""Statement"": [
                {{
                    ""Effect"": ""Allow"",
                    ""Principal"": {{ ""Service"": ""sns.amazonaws.com"" }},
                    ""Action"": ""sqs:SendMessage"",
                    ""Resource"": ""{queueArn}"",
                    ""Condition"": {{
                        ""ArnEquals"": {{ ""aws:SourceArn"": ""{arrivalsSNSResponse.TopicArn}"" }}
                    }}
                }}
            ]
        }}"}
});
await serviceConnection.SNSClient.SubscribeAsync(arrivalsSNSResponse.TopicArn, "sqs", queueArn);
var storedArrivalsSQSResponse = await serviceConnection.SQSClient!.CreateQueueAsync("StoredArrivals");
queueArn = (await serviceConnection.SQSClient!.GetQueueAttributesAsync(storedArrivalsSQSResponse.QueueUrl, [QueueAttributeName])).QueueARN;
await serviceConnection.SQSClient!.SetQueueAttributesAsync(storedArrivalsSQSResponse.QueueUrl, new() {
    { "Policy",$@"{{
            ""Version"": ""2012-10-17"",
            ""Statement"": [
                {{
                    ""Effect"": ""Allow"",
                    ""Principal"": {{ ""Service"": ""sns.amazonaws.com"" }},
                    ""Action"": ""sqs:SendMessage"",
                    ""Resource"": ""{queueArn}"",
                    ""Condition"": {{
                        ""ArnEquals"": {{ ""aws:SourceArn"": ""{storedArrivalsSNSResponse.TopicArn}"" }}
                    }}
                }}
            ]
        }}"}
});
await serviceConnection.SNSClient.SubscribeAsync(storedArrivalsSNSResponse.TopicArn, "sqs", queueArn);
var greetingSQSResponse = await serviceConnection.SQSClient!.CreateQueueAsync("Greeting");
queueArn = (await serviceConnection.SQSClient!.GetQueueAttributesAsync(greetingSQSResponse.QueueUrl, [QueueAttributeName])).QueueARN;
await serviceConnection.SQSClient!.SetQueueAttributesAsync(greetingSQSResponse.QueueUrl, new() {
    { "Policy",$@"{{
            ""Version"": ""2012-10-17"",
            ""Statement"": [
                {{
                    ""Effect"": ""Allow"",
                    ""Principal"": {{ ""Service"": ""sns.amazonaws.com"" }},
                    ""Action"": ""sqs:SendMessage"",
                    ""Resource"": ""{queueArn}"",
                    ""Condition"": {{
                        ""ArnEquals"": {{ ""aws:SourceArn"": ""{greetingSNSResponse.TopicArn}"" }}
                    }}
                }}
            ]
        }}"}
});
await serviceConnection.SNSClient.SubscribeAsync(greetingSNSResponse.TopicArn, "sqs", queueArn);
var greetingResponseSQSResponse = await serviceConnection.SQSClient!.CreateQueueAsync("Greeting_Response");
queueArn = (await serviceConnection.SQSClient!.GetQueueAttributesAsync(greetingResponseSQSResponse.QueueUrl, [QueueAttributeName])).QueueARN;
await serviceConnection.SQSClient!.SetQueueAttributesAsync(greetingResponseSQSResponse.QueueUrl, new() {
    { "Policy",$@"{{
            ""Version"": ""2012-10-17"",
            ""Statement"": [
                {{
                    ""Effect"": ""Allow"",
                    ""Principal"": {{ ""Service"": ""sns.amazonaws.com"" }},
                    ""Action"": ""sqs:SendMessage"",
                    ""Resource"": ""{queueArn}"",
                    ""Condition"": {{
                        ""ArnEquals"": {{ ""aws:SourceArn"": ""{greetingResponseSNSResponse.TopicArn}"" }}
                    }}
                }}
            ]
        }}"}
});
await serviceConnection.SNSClient.SubscribeAsync(greetingResponseSNSResponse.TopicArn, "sqs", queueArn);

await SampleExecution.ExecuteSample(serviceConnection, "AmazonSNQS", mapper);

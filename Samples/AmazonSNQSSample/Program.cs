using Amazon.SimpleNotificationService;
using Amazon.SQS;
using Messages;
using MQContract;
using MQContract.AmazonSNQS;

var mapper = new ChannelMapper();
mapper.AddQueryResponseMap("Greeting.Response","Greeting_Response");

var credentials = new Amazon.Runtime.BasicAWSCredentials("test", "test");

var config = new AmazonSQSConfig { ServiceURL = "http://localhost:4566" };
var sqsClient = new AmazonSQSClient(credentials, config);

var snsConfig = new AmazonSimpleNotificationServiceConfig { ServiceURL = "http://localhost:4566" };
var snsClient = new AmazonSimpleNotificationServiceClient(credentials, snsConfig);

var serviceConnection = new Connection(snsClient, sqsClient);

var arrivalsSNSResponse = await serviceConnection.SNSClient!.CreateTopicAsync("Arrivals");
var storedArrivalsSNSResponse = await serviceConnection.SNSClient!.CreateTopicAsync("StoredArrivals");
var greetingSNSResponse = await serviceConnection.SNSClient!.CreateTopicAsync("Greeting");
var greetingResponseSNSResponse = await serviceConnection.SNSClient!.CreateTopicAsync("Greeting_Response");

var arrivalsSQSResponse = await serviceConnection.SQSClient!.CreateQueueAsync("Arrivals");
var queueArn = (await sqsClient.GetQueueAttributesAsync(arrivalsSQSResponse.QueueUrl, ["QueueArn"])).QueueARN;
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
queueArn = (await sqsClient.GetQueueAttributesAsync(storedArrivalsSQSResponse.QueueUrl, ["QueueArn"])).QueueARN;
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
queueArn = (await sqsClient.GetQueueAttributesAsync(greetingSQSResponse.QueueUrl, ["QueueArn"])).QueueARN;
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
queueArn = (await sqsClient.GetQueueAttributesAsync(greetingResponseSQSResponse.QueueUrl, ["QueueArn"])).QueueARN;
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

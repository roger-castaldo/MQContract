using MQContract.Interfaces.Service;
using MQContract.Messages;

namespace MQContract.Subscriptions
{
    internal static class QueryResponseHelper
    {
        private const string QUERY_IDENTIFIER_HEADER = "_QueryClientID";
        private const string REPLY_ID = "_QueryReplyID";
        private const string REPLY_CHANNEL_HEADER = "_QueryReplyChannel";
        private static readonly List<string> REQUIRED_HEADERS = [QUERY_IDENTIFIER_HEADER, REPLY_ID, REPLY_CHANNEL_HEADER];

        public static MessageHeader StripHeaders(ServiceMessage originalMessage,out Guid queryClientID,out Guid replyID,out string? replyChannel)
        {
            queryClientID = new(originalMessage.Header[QUERY_IDENTIFIER_HEADER]!);
            replyID = new(originalMessage.Header[REPLY_ID]!);
            replyChannel = originalMessage.Header[REPLY_CHANNEL_HEADER];
            return new(originalMessage.Header.Keys
                .Where(key=>!Equals(key,QUERY_IDENTIFIER_HEADER)
                && !Equals(key,REPLY_ID)
                && !Equals(key,REPLY_CHANNEL_HEADER)
                ).Select(key =>new KeyValuePair<string,string>(key,originalMessage.Header[key]!)));
        }

        public static ServiceMessage EncodeMessage(ServiceMessage originalMessage, Guid queryClientID, Guid replyID,string? replyChannel,string? channel)
            => new(
                originalMessage.ID,
                originalMessage.MessageTypeID,
                channel??originalMessage.Channel,
                new(originalMessage.Header,new Dictionary<string, string?>([
                    new KeyValuePair<string,string?>(QUERY_IDENTIFIER_HEADER,queryClientID.ToString()),
                    new KeyValuePair<string,string?>(REPLY_ID,replyID.ToString()),
                    new KeyValuePair<string,string?>(REPLY_CHANNEL_HEADER,replyChannel)
                    ])),
                originalMessage.Data
            );

        public static bool IsValidMessage(ReceivedServiceMessage serviceMessage)
            => REQUIRED_HEADERS.All(key=>serviceMessage.Header.Keys.Contains(key));

        public static async Task<Tuple<TaskCompletionSource<ServiceQueryResult>, CancellationTokenSource>> StartResponseListenerAsync(IMessageServiceConnection connection,TimeSpan timeout,Guid identifier,Guid callID,string replyChannel,CancellationToken cancellationToken)
        {
            var token = new CancellationTokenSource();
            var reg = cancellationToken.Register(() => token.Cancel());
            var result = new TaskCompletionSource<ServiceQueryResult>();
            var consumer = await connection.SubscribeAsync(
                async (message) =>
                {
                    if (!result.Task.IsCompleted)
                    {
                        var headers = StripHeaders(message, out var queryClientID, out var replyID, out _);
                        if (Equals(queryClientID, identifier) && Equals(replyID, callID))
                        {
                            if (message.Acknowledge!=null)
                                await message.Acknowledge();
                            result.TrySetResult(new(
                                message.ID,
                                headers,
                                message.MessageTypeID,
                                message.Data
                            ));
                        }
                    }
                },
                error => { },
                replyChannel,
                cancellationToken: token.Token
            )??throw new QueryExecutionFailedException();
            token.Token.Register(async () => {
                await consumer.EndAsync();
                await reg.DisposeAsync();
                if (!result.Task.IsCompleted)
                    result.TrySetException(new QueryTimeoutException());
            });
            token.CancelAfter(timeout);
            return new Tuple<TaskCompletionSource<ServiceQueryResult>, CancellationTokenSource>(result,token);
        }
    }
}

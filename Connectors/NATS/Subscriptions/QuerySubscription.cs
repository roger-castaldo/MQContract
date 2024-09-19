using MQContract.Messages;
using NATS.Client.Core;

namespace MQContract.NATS.Subscriptions
{
    internal class QuerySubscription(IAsyncEnumerable<NatsMsg<byte[]>> asyncEnumerable, 
        Func<ReceivedServiceMessage, ValueTask<ServiceMessage>> messageReceived, Action<Exception> errorReceived) 
        : SubscriptionBase()
    {
        protected override async Task RunAction()
        {
            await foreach (var msg in asyncEnumerable.WithCancellation(CancelToken))
            {
                var receivedMessage = ExtractMessage(msg);
                try
                {
                    var result = await messageReceived(receivedMessage);
                    await msg.ReplyAsync<byte[]>(
                        result.Data.ToArray(),
                        headers: Connection.ExtractHeader(result),
                        replyTo: msg.ReplyTo,
                        cancellationToken: CancelToken
                    );
                }
                catch (Exception ex)
                {
                    errorReceived(ex);
                    var headers = Connection.ProduceQueryError(ex, receivedMessage.ID, out var responseData);
                    await msg.ReplyAsync<byte[]>(
                        responseData,
                        replyTo: msg.ReplyTo,
                        headers:headers,
                        cancellationToken: CancelToken
                    );
                }
            }
        }
    }
}

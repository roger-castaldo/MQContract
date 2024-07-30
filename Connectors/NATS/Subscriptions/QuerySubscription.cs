using MQContract.Messages;
using NATS.Client.Core;

namespace MQContract.NATS.Subscriptions
{
    internal class QuerySubscription(IAsyncEnumerable<NatsMsg<byte[]>> asyncEnumerable, 
        Func<RecievedServiceMessage, Task<ServiceMessage>> messageRecieved, Action<Exception> errorRecieved, 
        CancellationToken cancellationToken) : SubscriptionBase(cancellationToken)
    {
        protected override async Task RunAction()
        {
            await foreach (var msg in asyncEnumerable.WithCancellation(cancelToken.Token))
            {
                var recievedMessage = ExtractMessage(msg);
                try
                {
                    var result = await messageRecieved(recievedMessage);
                    await msg.ReplyAsync<byte[]>(
                        result.Data.ToArray(),
                        headers: Connection.ExtractHeader(result),
                        replyTo: msg.ReplyTo,
                        cancellationToken: cancelToken.Token
                    );
                }
                catch (Exception ex)
                {
                    errorRecieved(ex);
                    var headers = Connection.ProduceQueryError(ex, recievedMessage.ID, out var responseData);
                    await msg.ReplyAsync<byte[]>(
                        responseData,
                        replyTo: msg.ReplyTo,
                        headers:headers,
                        cancellationToken: cancelToken.Token
                    );
                }
            }
        }
    }
}

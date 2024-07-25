using MQContract.Interfaces.Service;
using MQContract.Messages;
using MQContract.NATS.Messages;
using MQContract.NATS.Serialization;
using NATS.Client.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MQContract.NATS.Subscriptions
{
    internal class QuerySubscription(IAsyncEnumerable<NatsMsg<NatsMessage>> asyncEnumerable, 
        Func<IRecievedServiceMessage, Task<IServiceMessage>> messageRecieved, Action<Exception> errorRecieved, 
        CancellationToken cancellationToken) : SubscriptionBase(cancellationToken)
    {
        protected override async Task RunAction()
        {
            await foreach (var msg in asyncEnumerable.WithCancellation(cancelToken.Token))
            {
                try
                {
                    var result = await messageRecieved(new RecievedMessage(new(msg.Headers),msg.Subject,msg.Data));
                    await msg.ReplyAsync<NatsQueryResponseMessage>(
                        new NatsQueryResponseMessage()
                        {
                            ID=result.ID,
                            MessageTypeID=result.MessageTypeID,
                            Data= result.Data
                        },
                        headers: Connection.ExtractHeader(result.Header),
                        replyTo: msg.ReplyTo,
                        serializer: MessageSerializer<NatsQueryResponseMessage>.Default,
                        cancellationToken: cancelToken.Token
                    );
                }
                catch (Exception ex)
                {
                    errorRecieved(ex);
                    await msg.ReplyAsync<NatsQueryResponseMessage>(
                        new NatsQueryResponseMessage()
                        {
                            ID=msg.Data?.ID??string.Empty,
                            MessageTypeID=msg.Data?.MessageTypeID??string.Empty,
                            Error= ex.Message
                        },
                        replyTo: msg.ReplyTo,
                        serializer: MessageSerializer<NatsQueryResponseMessage>.Default,
                        cancellationToken: cancelToken.Token
                    );
                }
            }
        }
    }
}

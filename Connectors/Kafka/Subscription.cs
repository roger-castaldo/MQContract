using MQContract.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MQContract.Kafka;

internal class Subscription(Confluent.Kafka.IConsumer<string, byte[]> consumer, Func<ReceivedServiceMessage, ValueTask> messageReceived, Action<Exception> errorReceived, string channel)
    : BaseLoopSubscription(errorReceived)
{
    protected override async ValueTask RecieveMessageAsync(CancellationToken cancelToken)
    {
        try
        {
            var msg = consumer.Consume(cancellationToken: cancelToken);
            var ackSource = new TaskCompletionSource();
            var headers = Connection.ExtractHeaders(msg.Message.Headers, out var messageTypeID);
            _ = await Task.WhenAny(
                messageReceived(new ReceivedServiceMessage(
                    msg.Message.Key??string.Empty,
                    messageTypeID??string.Empty,
                    channel,
                    headers,
                    msg.Message.Value,
                    acknowledge: async () =>
                    {
                        consumer.Commit(msg);
                        ackSource.TrySetResult();
                    }
                )).AsTask(),
                ackSource.Task
            );
        }
        catch (AccessViolationException)
        {
            //dropped this exception as it can occur when the consumption is stopped
        }
    }

    protected override ValueTask CleanupAsync()
    {
        try
        {
            consumer.Close();
        }
        catch
        {
            //ignoring error here as we are attempting to dispose the resource
        }
        consumer.Dispose();
        return ValueTask.CompletedTask;
    }
}

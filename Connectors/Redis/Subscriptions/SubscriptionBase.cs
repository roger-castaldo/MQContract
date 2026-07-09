using StackExchange.Redis;

namespace MQContract.Redis.Subscriptions;

internal abstract class SubscriptionBase(Action<Exception> errorReceived, IDatabase database, Guid connectionID, string channel, string? group)
    : BaseLoopSubscription(errorReceived)
{
    protected IDatabase Database => database;
    protected string Channel => channel;
    protected string? Group => group;
    private RedisValue minId = "-";

    protected override async ValueTask RecieveMessageAsync(CancellationToken cancelToken)
    {
        var result = await (group==null ? database.StreamRangeAsync(channel, minId, "+", 1) : database.StreamReadGroupAsync(channel, group!, connectionID.ToString(), ">", 1));
        if (result.Length!=0)
        {
            minId = result[0].Id+1;
            await ProcessMessage(result[0], channel, group);
        }
        else
            await Task.Delay(50);
    }

    protected async ValueTask Acknowledge(RedisValue Id)
    {
        if (Group!=null)
            await Database.StreamAcknowledgeAsync(channel, group, Id);
    }

    protected abstract ValueTask ProcessMessage(StreamEntry streamEntry, string channel, string? group);

}

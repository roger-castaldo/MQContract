namespace MQContract.Messages
{
    public record QueryResponseMessage<T>(T Message,Dictionary<string,string?> Headers);
}

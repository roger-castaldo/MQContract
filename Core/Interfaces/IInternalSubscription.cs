namespace MQContract.Interfaces;

internal interface IInternalSubscription : ISubscription
{
    Guid ID { get; }
    ValueTask EndAsyncWithoutRemoval();
}

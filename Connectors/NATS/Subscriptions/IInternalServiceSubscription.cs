using MQContract.Interfaces.Service;

namespace MQContract.NATS.Subscriptions
{
    internal interface IInternalServiceSubscription : IServiceSubscription
    {
        void Run();
    }
}

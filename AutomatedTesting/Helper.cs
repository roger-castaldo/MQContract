using AutomatedTesting.ServiceInjection;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace AutomatedTesting
{
    internal static class Helper
    {
        public static IServiceProvider ProduceServiceProvider(string serviceName)
        {
            var services = new ServiceCollection();
            services.AddSingleton<IInjectableService>(new InjectedService(serviceName));
            return services.BuildServiceProvider();
        }

        public static IRecievedServiceMessage ProduceRecievedServiceMessage(IServiceMessage message,string? messageTypeID=null)
        {
            var timestamp = DateTime.Now;
            var result = new Mock<IRecievedServiceMessage>();
            result.Setup(x => x.Channel).Returns(message.Channel);
            result.Setup(x=>x.Header).Returns(message.Header);
            result.Setup(x=>x.RecievedTimestamp).Returns(timestamp);
            result.Setup(x=>x.ID).Returns(message.ID);
            result.Setup(x=>x.Data).Returns(message.Data);
            result.Setup(x=>x.MessageTypeID).Returns(messageTypeID??message.MessageTypeID);
            return result.Object;
        }

        public static IServiceQueryResult ProduceQueryResult(IServiceMessage message)
        {
            var timestamp = DateTime.Now;
            var result = new Mock<IServiceQueryResult>();
            result.Setup(x => x.Channel).Returns(message.Channel);
            result.Setup(x => x.Header).Returns(message.Header);
            result.Setup(x => x.MessageID).Returns(Guid.NewGuid().ToString());
            result.Setup(x => x.ID).Returns(message.ID);
            result.Setup(x => x.Data).Returns(message.Data);
            result.Setup(x => x.MessageTypeID).Returns(message.MessageTypeID);
            if (message is IErrorServiceMessage error)
            {
                result.Setup(x => x.IsError).Returns(true);
                result.Setup(x => x.Error).Returns(error.Error.Message);
            }
            return result.Object;
        }
    }
}

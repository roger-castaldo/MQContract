using MQContract.Messages;

namespace MQContract.Interfaces.Consumers;

/// <summary>
/// Used to define a consumer that will filter out given messages using a header filter
/// </summary>
public interface IHeaderFilteredConsumer : IBaseConsumer
{
    /// <summary>
    /// The filter callback to be invoked that will be supplied the current headers and expect back a filter type
    /// </summary>
    Func<MessageHeader, ValueTask<MessageFilterResult>> Filter { get; }
}

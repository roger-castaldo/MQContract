﻿using MQContract.Messages;

namespace MQContract.Interfaces.Service
{
    public interface IMessageServiceConnection
    {
        int? MaxMessageBodySize { get; }
        TimeSpan DefaultTimout { get; }
        Task<IPingResult> PingAsync();
        Task<ITransmissionResult> PublishAsync(IServiceMessage message, TimeSpan timeout, IServiceChannelOptions? options = null, CancellationToken cancellationToken = new CancellationToken());
        Task<IServiceSubscription?> SubscribeAsync(Action<IRecievedServiceMessage> messageRecieved, Action<Exception> errorRecieved, string channel, string group, IServiceChannelOptions? options = null, CancellationToken cancellationToken = new CancellationToken());
        Task<IServiceQueryResult> QueryAsync(IServiceMessage message, TimeSpan timeout, IServiceChannelOptions? options = null, CancellationToken cancellationToken = new CancellationToken());
        Task<IServiceSubscription?> SubscribeQueryAsync(Func<IRecievedServiceMessage, Task<IServiceMessage>> messageRecieved, Action<Exception> errorRecieved, string channel, string group, IServiceChannelOptions? options = null, CancellationToken cancellationToken = new CancellationToken());
    }
}
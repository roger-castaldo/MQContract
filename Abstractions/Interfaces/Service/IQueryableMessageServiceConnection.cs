using MQContract.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MQContract.Interfaces.Service
{
    /// <summary>
    /// Used to identify a message service that supports response query style messaging, either through inbox or directly
    /// </summary>
    public interface IQueryableMessageServiceConnection : IMessageServiceConnection
    {
        /// <summary>
        /// The default timeout to use for RPC calls when it's not specified
        /// </summary>
        TimeSpan DefaultTimeout { get; }
        /// <summary>
        /// Implements a call to create a subscription to a given channel as a member of a given group for responding to queries
        /// </summary>
        /// <param name="messageReceived">The callback to be invoked when a message is received, returning the response message</param>
        /// <param name="errorReceived">The callback to invoke when an exception occurs</param>
        /// <param name="channel">The name of the channel to subscribe to</param>
        /// <param name="group">The group to bind a consumer to</param>
        /// <param name="cancellationToken">A cancellation token</param>
        /// <returns>A service subscription object</returns>
        ValueTask<IServiceSubscription?> SubscribeQueryAsync(Func<ReceivedServiceMessage, ValueTask<ServiceMessage>> messageReceived, Action<Exception> errorReceived, string channel, string? group = null, CancellationToken cancellationToken = new CancellationToken());
    }
}

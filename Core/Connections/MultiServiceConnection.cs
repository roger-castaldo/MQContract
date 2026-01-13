using Microsoft.Extensions.Logging;
using MQContract.Extensions;
using MQContract.Interfaces;
using MQContract.Interfaces.Encoding;
using MQContract.Interfaces.Encrypting;
using MQContract.Interfaces.Service;
using MQContract.Messages;
using MQContract.Middleware;
using MQContract.Subscriptions;
using System.Diagnostics;

namespace MQContract.Connections
{
    internal class MultiServiceConnection(IMessageEncoder? defaultMessageEncoder = null,
        IMessageEncryptor? defaultMessageEncryptor = null,
        IServiceProvider? serviceProvider = null,
        ILogger? logger = null,
        ChannelMapper? channelMapper = null) :
        AMappableConnection<IMultiServiceContractConnection>(defaultMessageEncoder, defaultMessageEncryptor, serviceProvider, logger, channelMapper),
        IMultiServiceContractConnection
    {
        async ValueTask<IEnumerable<PingResult>> IMultiServiceContractConnection.PingAsync()
            => await FullList
                .Select(ss => ss.MessageServiceConnection)
                .OfType<IPingableMessageServiceConnection>()
                .WhenAll(pmc => pmc.PingAsync());
            
        IMultiServiceContractConnection IMultiServiceContractConnection.RegisterServiceConnection(string serviceConnectionName, IMessageServiceConnection messageServiceConnection)
            => RegisterServiceConnection(pars => true, serviceConnectionName, messageServiceConnection);

        #region PubSub
        private static async ValueTask<ChildTransmissionResult> AwaitTransmission(string connectionName, Func<ValueTask<TransmissionResult>> transmit)
        {
            var result = await transmit();
            return new(connectionName, result.Error);
        }

        async ValueTask<MultiTransmissionResult> IMultiServiceContractConnection.PublishAsync<TMessage>(TMessage message, string? channel, MessageHeader? messageHeader, CancellationToken cancellationToken)
        {
            using var scope = SetScope();
            Logger?.LogDebugChecked("Publishing message {TMessage} on {Channel}", typeof(TMessage), channel);
            using var activity = StartActivity(Constants.PublishActivityName);
            var serviceMessage = await ProduceServiceMessageAsync<TMessage>(
                ChannelMapper.MapTypes.Publish, 
                GetMessageFactory<TMessage>(), 
                message, 
                false, 
                activity,
                maxMessageSize: MaxMessageBodySize,
                channel: channel, 
                messageHeader: messageHeader
            );
            var connections = await GetConnectionsAsync(serviceMessage.Channel, typeof(TMessage), serviceMessage.Header);
            var results = await connections
                .WhenAll(c => AwaitTransmission(c.ServiceConnectionName, async () =>
                {
                    OpenTelemetryMiddleware.AssignConnectionType(activity, c.MessageServiceConnection, c.ServiceConnectionName);
                    var result = await PublishMessageAsync<TMessage>(c.PublishLock, serviceMessage, c.MessageServiceConnection, activity, c.ServiceConnectionName, cancellationToken);
                    return result;
                }));
            activity?.SetStatus(results.Any(r => r.IsError) ? ActivityStatusCode.Error : ActivityStatusCode.Ok);
            activity?.Stop();
            return new(serviceMessage.ID, results);
        }

        async ValueTask<IEnumerable<MultiTransmissionResult>> IMultiServiceContractConnection.BulkPublishAsync<TMessage>(IEnumerable<(TMessage message, MessageHeader? messageHeader)> messages, string? channel, CancellationToken cancellationToken)
        {
            using var scope = SetScope();
            Logger?.LogDebugChecked("Bulk Publishing messages {TMessage} on {Channel}", typeof(TMessage), channel);
            using var activity = StartActivity(Constants.BulkPublishActivityName);
            activity?.SetTag(Constants.BulkPublishCountTag, messages.Count());
            var serviceMessages = await
            messages.WhenAll(m =>
                    ProduceServiceMessageAsync<TMessage>(
                        ChannelMapper.MapTypes.Publish, 
                        GetMessageFactory<TMessage>(), 
                        m.message, 
                        false, 
                        activity, 
                        maxMessageSize: MaxMessageBodySize,
                        channel: channel, 
                        messageHeader: m.messageHeader
                    )
            );
            var connections = await GetConnectionsAsync(serviceMessages.First().Channel, typeof(TMessage), serviceMessages.First().Header);
            var transmissionResults = await Task.WhenAll(connections.Select(c => Task<MultiTransmissionResult>.Run(async () =>
            {
                OpenTelemetryMiddleware.AssignConnectionType(activity, c.MessageServiceConnection, c.ServiceConnectionName);
                var result = await BulkPublishAsync<TMessage>(c.PublishLock, serviceMessages, c.MessageServiceConnection, activity, cancellationToken, connectionName: c.ServiceConnectionName);
                return result.Select((res, index) => new MultiTransmissionResult(serviceMessages.ElementAt(index).ID, [new(c.ServiceConnectionName, res.Error)]));
            })));
            activity?.SetStatus(Array.Exists(transmissionResults, mtr => mtr.Any(r => r.HasError)) ? ActivityStatusCode.Error : ActivityStatusCode.Ok);
            activity?.Stop();
            return transmissionResults
                .SelectMany(mtr => mtr)
                .GroupBy(mtr => mtr.ID)
                .Select(grp => new MultiTransmissionResult(grp.Key, grp.SelectMany(g => g.Results)));
        }

        protected override async ValueTask<ISubscription> CreateSubscriptionAsync<TMessage>(Func<IReceivedMessage<TMessage>, ValueTask> messageReceived, Action<Exception> errorReceived, string? channel, string? group, bool ignoreMessageHeader, MessageFilters<TMessage>? messageFilters, bool synchronous, CancellationToken cancellationToken)
        {
            var messageFactory = GetMessageFactory<TMessage>(ignoreMessageHeader);
            (var connections, channel) = await GetConnectionsAsync<TMessage>(channel, ChannelMapper.MapTypes.PublishSubscription);
            return new SubscriptionCollection(await connections.WhenAll(conn =>
                CreateSubscriptionAsync<TMessage>(
                    messageFactory,
                    conn.MessageServiceConnection,
                    messageReceived,
                    errorReceived,
                    channel,
                    group,
                    synchronous,
                    conn.ServiceConnectionName,
                    messageFilters,
                    cancellationToken
                ))
            );
        }
        #endregion

        #region QueryResponse
        async ValueTask<IEnumerable<QueryResult<TQueryResponse>>> IMultiServiceContractConnection.QueryAsync<TQuery, TQueryResponse>(TQuery message, TimeSpan? timeout, string? channel, string? responseChannel, MessageHeader? messageHeader, CancellationToken cancellationToken)
        {
            using var scope = SetScope();
            Logger?.LogDebugChecked("Executing QueryResponse of {TQuery}, expecting {TQueryResponse} on {Channel} with {ResponseChannel}", typeof(TQuery), typeof(TQueryResponse), channel, responseChannel);
            using var activity = StartActivity(Constants.PublishQueryActivityName);
            var serviceMessage = await ProduceServiceMessageAsync<TQuery>(
                ChannelMapper.MapTypes.Query, 
                GetMessageFactory<TQuery>(), 
                message, 
                false, 
                activity, 
                maxMessageSize: MaxMessageBodySize,
                channel: channel, 
                messageHeader: messageHeader
            );
            var connections = await GetConnectionsAsync(serviceMessage.Channel, typeof(TQuery), serviceMessage.Header);
            return await connections
                .WhenAll(conn =>
                {
                    OpenTelemetryMiddleware.AssignConnectionType(activity, conn.MessageServiceConnection, conn.ServiceConnectionName);
                    return ExecuteQueryAsync<TQuery, TQueryResponse>(conn.MessageServiceConnection, serviceMessage, activity, timeout: timeout, responseChannel: responseChannel, connectionName: conn.ServiceConnectionName, cancellationToken: cancellationToken);
                });
        }

        async ValueTask<IEnumerable<QueryResult<object>>> IMultiServiceContractConnection.QueryAsync<TQuery>(TQuery message, TimeSpan? timeout, string? channel, string? responseChannel, MessageHeader? messageHeader, CancellationToken cancellationToken)
        {
            using var scope = SetScope();
            Logger?.LogDebugChecked("Attempting to get response type for QueryResponse for {TQuery} on {Channel} with {ResponseChannel}", typeof(TQuery), channel, responseChannel);
            return await messageContext.ExecuteQuery<TQuery>(this, message, timeout, channel, responseChannel, messageHeader, cancellationToken);
        }

        protected override async ValueTask<ISubscription> ProduceSubscribeQueryResponseAsync<TQuery, TQueryResponse>(Func<IReceivedMessage<TQuery>, ValueTask<QueryResponseMessage<TQueryResponse>>> messageReceived, Action<Exception> errorReceived, string? channel, string? group, bool ignoreMessageHeader, bool synchronous, MessageFilters<TQuery>? messageFilter, CancellationToken cancellationToken)
        {
            using var scope = SetScope();
            Logger?.LogDebugChecked("Producing QueryResponse Subscription for {TQuery} responding with {TQueryResponse} on {Channel} in {Group}", typeof(TQuery), typeof(TQueryResponse), channel, group);
            var queryMessageFactory = GetMessageFactory<TQuery>(ignoreMessageHeader);
            var responseMessageFactory = GetMessageFactory<TQueryResponse>();
            (var connections, channel) = await GetConnectionsAsync<TQuery>(channel, ChannelMapper.MapTypes.QuerySubscription);
            return new SubscriptionCollection(await connections
                .WhenAll(conn =>
                    CreateSubscriptionAsync<TQuery, TQueryResponse>(
                       queryMessageFactory,
                       responseMessageFactory,
                       conn.MessageServiceConnection,
                       messageReceived,
                       errorReceived,
                       channel,
                       group,
                       synchronous,
                       conn.ServiceConnectionName,
                       messageFilter,
                       cancellationToken
                    )
               )
            );
        }
        #endregion
    }
}

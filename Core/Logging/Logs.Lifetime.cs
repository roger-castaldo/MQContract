using Microsoft.Extensions.Logging;
using System.Diagnostics.Metrics;

namespace MQContract.Logging
{
    internal static partial class Logs
    {
        internal static partial class Lifetime
        {
            [LoggerMessage(EventId = EventIds.Lifetime.ClosingConnection,Level = LogLevel.Debug,Message = "Closing contract connection")]
            public static partial void ClosingConnection(ILogger logger);

            [LoggerMessage(EventId = EventIds.Lifetime.ClosingAllInboxes,Level = LogLevel.Information,Message = "Closing all open inbox subscriptions")]
            public static partial void ClosingAllInboxes(ILogger logger);

            [LoggerMessage(EventId = EventIds.Lifetime.EndingSubscription,Level = LogLevel.Information,Message = "Calling subscription {ID} end async")]
            public static partial void SubscriptionEndAsync(ILogger logger, Guid id);
            
            [LoggerMessage(EventId = EventIds.Lifetime.RegisteringMiddleware, Level = LogLevel.Debug, Message = "Registering middleware of type {Type}")]
            public static partial void RegisteringMiddleware(ILogger logger, Type type);

            [LoggerMessage(EventId = EventIds.Lifetime.EnablingMetricMiddleware,Level = LogLevel.Debug,Message = "Enabling metrics on service connection with {Meter} and {UseInternal}")]
            public static partial void EnablingMetricMiddleware(ILogger logger, Meter? meter, bool useInternal);

            [LoggerMessage(EventId = EventIds.Lifetime.ConsumerRegistrationFailed,Level = LogLevel.Error,Message = "An error occured attempting to register a {ConsumerName} of type {ConsumerType}")]
            public static partial void ConsumerRegistrationFailed(ILogger logger, Exception exception, string consumerName, Type consumerType);

            [LoggerMessage(EventId = EventIds.Lifetime.CreatingPubSubSubscription,Level = LogLevel.Debug,Message = "Creating PubSub Subscription for {MessageType} on {Channel} in {Group}.")]
            public static partial void CreatingPubSubSubscription(ILogger logger, Type messageType, string? channel, string? group);

            [LoggerMessage(EventId = EventIds.Lifetime.EstablishingPubSubSubscription,Level = LogLevel.Information,Message = "Establishing PubSub Subscription connection.")]
            public static partial void EstablishingPubSubSubscription(ILogger logger);

            [LoggerMessage(EventId = EventIds.Lifetime.PubSubSubscriptionEstablishmentFailed,Level = LogLevel.Information,Message = "Establishment of PubSub Subscription connection failed.")]
            public static partial void PubSubSubscriptionEstablishmentFailed(ILogger logger);

            [LoggerMessage(EventId = EventIds.Lifetime.EstablishingPubSubServiceSubscription,Level = LogLevel.Debug,Message = "Establishing underlying service subscription for PubSub subscription.")]
            public static partial void EstablishingPubSubServiceSubscription(ILogger logger);

            [LoggerMessage(EventId = EventIds.Lifetime.EstablishingPubSubServiceSubscriptionSucceeded,Level = LogLevel.Information,Message = "Successfully established PubSub subscription.")]
            public static partial void PubSubSubscriptionEstablishmentSucceeded(ILogger logger);

            [LoggerMessage(EventId = EventIds.Lifetime.EstablishingInboxSubscription,Level = LogLevel.Debug,Message = "Establishing an instance of Inbox Message style handling for a QueryResponse call on {ConnectionName}")]
            public static partial void EstablishingInbox(ILogger logger, string? connectionName);

            [LoggerMessage(EventId = EventIds.Lifetime.SettingUpInboxSubscription,Level = LogLevel.Information,Message = "Setting up Inbox Message listener with {CorrelationID}")]
            public static partial void SettingUpInbox(ILogger logger, Guid correlationID);

            [LoggerMessage(EventId = EventIds.Lifetime.EstablishingNewInboxSubscription,Level = LogLevel.Debug,Message = "Establishing new Inbox Subscription for {ConnectionName}")]
            public static partial void EstablishingNewInbox(ILogger logger, string? connectionName);

            [LoggerMessage(EventId = EventIds.Lifetime.CreatingQueryResponseSubscription,Level = LogLevel.Debug,Message = "Creating QueryResponse subscription for message query {TQuery} and response {TQueryResponse} on channel {Channel} in group {Group}")]
            public static partial void CreatingSubscription(ILogger logger, Type tQuery, Type tQueryResponse, string? channel, string? group);

            [LoggerMessage(EventId = EventIds.Lifetime.ConstructingQueryResponseSubscription,Level = LogLevel.Information,Message = "Constructing QueryResponse subscription for {TQuery} answering with {TQueryResponse} on {Channel} in {Group}")]
            public static partial void ConstructingSubscription(ILogger logger, Type tQuery, Type tQueryResponse, string? channel, string? group);

            [LoggerMessage(EventId = EventIds.Lifetime.EstablishingQueryResponseSubscription,Level = LogLevel.Debug,Message = "Establishing QueryResponse subscription")]
            public static partial void EstablishingSubscription(ILogger logger);

            [LoggerMessage(EventId = EventIds.Lifetime.QueryResponseSubscriptionEstablishmentFailed,Level = LogLevel.Debug,Message = "Failed to establish subscription")]
            public static partial void EstablishingSubscriptionFailed(ILogger logger);

            [LoggerMessage(EventId = EventIds.Lifetime.EstablishingQueryResponseSubscriptionServiceSubscription,Level = LogLevel.Information,Message = "Establishing underlying service subscription for QueryResponse subscription.")]
            public static partial void EstablishingServiceSubscription(ILogger logger);

            [LoggerMessage(EventId = EventIds.Lifetime.EstablishingQueryResponseServiceSubscription,Level = LogLevel.Information,Message = "Establishing underlying QueryResponse service subscription.")]
            public static partial void EstablishingQueryResponseServiceSubscription(ILogger logger);

            [LoggerMessage(EventId = EventIds.Lifetime.QueryResponseSubscriptionEstablishmentSucceeded,Level = LogLevel.Information,Message = "Successfully established QueryResponse subscription.")]
            public static partial void EstablishingQueryResponseSubscriptionSuccess(ILogger logger);

            [LoggerMessage(EventId = EventIds.Lifetime.EstablishingQueryResponseSubscriptionWithPubSubServiceSubscription,Level = LogLevel.Information,Message = "Establishing underlying PubSub service subscription to listen for incoming queries.")]
            public static partial void EstablishingQueryResponseSubscriptionWithPubSubServiceSubscription(ILogger logger);

            [LoggerMessage(EventId = EventIds.Lifetime.QueryResponseSubscriptionEstablishmentFailedWithError,Level = LogLevel.Error,Message = "Error occurred while establishing the subscription.")]
            public static partial void ErrorEstablishingSubscription(ILogger logger, Exception exception);

            [LoggerMessage(EventId = EventIds.Lifetime.DisposingQueryResponseSubscription,Level = LogLevel.Information,Message = "Disposing resources for QueryResponseSubscription.")]
            public static partial void DisposingQueryResponseSubscription(ILogger logger);
            
            [LoggerMessage(EventId = EventIds.Lifetime.QueryResponseSubscriptionDisposed,Level = LogLevel.Information,Message = "Resources for QueryResponseSubscription have been disposed.")]
            public static partial void DisposedQueryResponseSubscription(ILogger logger);
        }
    }
}

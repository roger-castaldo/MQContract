namespace MQContract.Logging;

/// <summary>
/// Centralized well-known event id values used for structured logging across the MQContract library.
/// </summary>
/// <remarks>
/// Event id bands:
///  - 1xxxx: Trace (hot-path diagnostic)
///  - 2xxxx: Debug (state transitions)
///  - 3xxxx: Information (lifecycle events)
///  - 4xxxx: Warning (recoverable issues)
///  - 5xxxx: Error
///  - 9xxxx: Critical (invariants broken, data-loss risk)
/// Event id concern grouping:
///  - x1xxx: Lifetime (connection, subscription, middleware registration)
///  - x2xxx: Pipeline (middleware execution, message production/decoding, resilience)
///  - x3xxx: Publishing (publish operations, queries, bulk publish, transmission/timeout errors)
///  - x4xxx: Consuming (processing, acknowledgement, response handling)
///  - x5xxx: Transport (connection discovery, pinging, connector connection issues)
///  - x6xxx - x8xxx: (reserved for future concern groups)
/// Event id area grouping:
///  - xx1xx: Connections  (connection lifecycle, discovery, pinging)
///  - xx2xx: Subscriptions (subscription lifecycle, establishment, disposal)
///  - xx3xx: Middleware (registration, execution)
///  - xx4xx: Service Messages (production, decoding, filtering, processing attempts)
///  - xx5xx: Consumer Operations (acknowledgement, response handling)
///  - xx6xx: Resilience (retries, fallbacks)
///  - xx7xx: Contract Messages (Pubsub, queries, query responses)
/// The nested classes group ids by concern (Lifetime, Pipeline, Publishing, Consuming, Transport).
/// </remarks>
public static class EventIds
{
    /// <summary>
    /// Connection / lifetime related event id values.
    /// These ids cover creation, teardown and lifecycle operations for subscriptions, inboxes and middleware registration.
    /// </summary>
    public static class Lifetime
    {
        /// <summary>
        /// Logged when an individual connection is being closed.
        /// (21100 - Debug)
        /// </summary>
        public const int ClosingConnection = 21100;

        /// <summary>
        /// Logged when all inboxes are being closed (full shutdown of inbox resources).
        /// (31101 - Information)
        /// </summary>
        public const int ClosingAllInboxes = 31101;

        /// <summary>
        /// Logged when a subscription is ending / being disposed.
        /// (31200 - Information)
        /// </summary>
        public const int EndingSubscription = 31200;

        /// <summary>
        /// Logged when a pub/sub subscription object is being created (start of creation).
        /// (21201 - Debug)
        /// </summary>
        public const int CreatingPubSubSubscription = 21201;

        /// <summary>
        /// Logged when the process to establish a pub/sub subscription begins.
        /// (31202 - Information)
        /// </summary>
        public const int EstablishingPubSubSubscription = 31202;

        /// <summary>
        /// Logged when establishing a pub/sub subscription fails.
        /// (51203 - Error)
        /// </summary>
        public const int PubSubSubscriptionEstablishmentFailed = 51203;

        /// <summary>
        /// Logged when establishing a pub/sub service-side subscription begins (service-backed subscription).
        /// (21204 - Debug)
        /// </summary>
        public const int EstablishingPubSubServiceSubscription = 21204;

        /// <summary>
        /// Logged when establishing a pub/sub service subscription completes successfully.
        /// (31205 - Information)
        /// </summary>
        public const int EstablishingPubSubServiceSubscriptionSucceeded = 31205;

        /// <summary>
        /// Logged when an inbox subscription establishment starts.
        /// (21206 - Debug)
        /// </summary>
        public const int EstablishingInboxSubscription = 21206;

        /// <summary>
        /// Logged when inbox subscription setup is in progress.
        /// (31207 - Information)
        /// </summary>
        public const int SettingUpInboxSubscription = 31207;

        /// <summary>
        /// Logged when establishing a brand new inbox subscription.
        /// (21208 - Debug)
        /// </summary>
        public const int EstablishingNewInboxSubscription = 21208;

        /// <summary>
        /// Logged when a query-response subscription is being created.
        /// (21209 - Debug)
        /// </summary>
        public const int CreatingQueryResponseSubscription = 21209;

        /// <summary>
        /// Logged when a query-response subscription object is being constructed.
        /// (31210 - Information)
        /// </summary>
        public const int ConstructingQueryResponseSubscription = 31210;

        /// <summary>
        /// Logged when the establishment of a query-response subscription starts.
        /// (21211 - Debug)
        /// </summary>
        public const int EstablishingQueryResponseSubscription = 21211;

        /// <summary>
        /// Logged when establishing a query-response subscription fails.
        /// (51212 - Error)
        /// </summary>
        public const int QueryResponseSubscriptionEstablishmentFailed = 51212;

        /// <summary>
        /// Logged when a query-response subscription's service-side subscription is being established.
        /// (31213 - Information)
        /// </summary>
        public const int EstablishingQueryResponseSubscriptionServiceSubscription = 31213;

        /// <summary>
        /// Logged when the query-response service subscription is being established.
        /// (31214 - Information)
        /// </summary>
        public const int EstablishingQueryResponseServiceSubscription = 31214;

        /// <summary>
        /// Logged when a query-response subscription establishment completes successfully.
        /// (31215 - Information)
        /// </summary>
        public const int QueryResponseSubscriptionEstablishmentSucceeded = 31215;

        /// <summary>
        /// Logged when establishing a query-response subscription that also uses a pub/sub service subscription.
        /// (31216 - Information)
        /// </summary>
        public const int EstablishingQueryResponseSubscriptionWithPubSubServiceSubscription = 31216;

        /// <summary>
        /// Logged when a query-response subscription establishment fails with an error detail.
        /// (51217 - Error)
        /// </summary>
        public const int QueryResponseSubscriptionEstablishmentFailedWithError = 51217;

        /// <summary>
        /// Logged when a query-response subscription is being disposed.
        /// (31218 - Information)
        /// </summary>
        public const int DisposingQueryResponseSubscription = 31218;

        /// <summary>
        /// Logged when a query-response subscription has been disposed.
        /// (31219 - Information)
        /// </summary>
        public const int QueryResponseSubscriptionDisposed = 31219;

        /// <summary>
        /// Logged when middleware is being registered with the pipeline.
        /// (21300 - Debug)
        /// </summary>
        public const int RegisteringMiddleware = 21300;

        /// <summary>
        /// Logged when metric middleware is explicitly enabled.
        /// (21301 - Debug)
        /// </summary>
        public const int EnablingMetricMiddleware = 21301;

        /// <summary>
        /// Logged when consumer registration fails.
        /// (51500 - Error)
        /// </summary>
        public const int ConsumerRegistrationFailed = 51500;
    }

    /// <summary>
    /// Pipeline related event id values.
    /// These ids cover middleware execution, message production/decoding and resilience events on the message processing pipeline.
    /// </summary>
    public static class Pipeline
    {
        /// <summary>
        /// Logged when executing 'before encode' middleware in the pipeline.
        /// (22300 - Debug)
        /// </summary>
        public const int ExecutingBeforeMessageEncodeMiddleware = 22300;

        /// <summary>
        /// Logged when executing a specific 'before encode' middleware (identifies a particular middleware).
        /// (22301 - Debug)
        /// </summary>
        public const int ExecutingSpecificBeforeMessageEncodeMiddleware = 22301;

        /// <summary>
        /// Logged when executing 'after encode' middleware.
        /// (22302 - Debug)
        /// </summary>
        public const int ExecutingAfterMessageEncodeMiddleware = 22302;

        /// <summary>
        /// Logged when executing 'before decode' middleware.
        /// (22303 - Debug)
        /// </summary>
        public const int ExecutingBeforeMessageDecodeMiddleware = 22303;

        /// <summary>
        /// Logged when executing 'after decode' middleware.
        /// (22304 - Debug)
        /// </summary>
        public const int ExecutingAfterMessageDecodeMiddleware = 22304;

        /// <summary>
        /// Logged when executing a specific 'after decode' middleware.
        /// (22305 - Debug)
        /// </summary>
        public const int ExecutingSpecificAfterMessageDecodeMiddleware = 22305;

        /// <summary>
        /// Logged when a service message is produced by the pipeline (a message sent internally or to a connector).
        /// (22400 - Information)
        /// </summary>
        public const int ProducingServiceMessage = 22400;

        /// <summary>
        /// Logged when a service message is filtered based on header values.
        /// (22401 - Information)
        /// </summary>
        public const int FilteringServiceMessageByHeaders = 22401;

        /// <summary>
        /// Logged when a service message is decoded into a domain/message object.
        /// (22402 - Information)
        /// </summary>
        public const int DecodingServiceMessage = 22402;

        /// <summary>
        /// Logged when a service message is filtered by the full message content (not only headers).
        /// (22403 - Information)
        /// </summary>
        public const int FilterServiceMessageInFull = 22403;

        /// <summary>
        /// Logged when processing a query response message in the pipeline.
        /// (22404 - Information)
        /// </summary>
        public const int ProcessingQueryResponse = 22404;

        /// <summary>
        /// Logged when attempting to process an inbox message (begin processing attempt).
        /// (32405 - Information)
        /// </summary>
        public const int AttemptingToProcessInboxMessage = 32405;

        /// <summary>
        /// Logged when processing a query response fails.
        /// (52406 - Error)
        /// </summary>
        public const int ProcessingQueryResponseFailed = 52406;

        /// <summary>
        /// Logged when an invalid query response message is received (invalid format or unexpected payload).
        /// (42407 - Warning)
        /// </summary>
        public const int ReceivedInvalidQueryResponseMessage = 42407;

        /// <summary>
        /// Logged when a resilience retry is triggered for an operation in the pipeline.
        /// (22600 - Debug)
        /// </summary>
        public const int ResilienceRetryTriggered = 22600;

        /// <summary>
        /// Logged when resilience fallback could not be performed after retries.
        /// (52601 - Error)
        /// </summary>
        public const int ResilienceFailedToFallback = 52601;

        /// <summary>
        /// Logged when extracting a query response CLR type from a message or headers.
        /// (22700 - Debug)
        /// </summary>
        public const int ExtractingQueryResponseType = 22700;

        /// <summary>
        /// Logged when a query reply channel has been mapped (associating a response channel for a query).
        /// (22701 - Debug)
        /// </summary>
        public const int QueryReplyChannelMapped = 22701;
    }

    /// <summary>
    /// Publishing related event id values.
    /// These ids cover publishing operations, queries, bulk publish and related transmission/timeout errors.
    /// </summary>
    public static class Publishing
    {
        /// <summary>
        /// Logged when a normal message publish operation is performed.
        /// (23702 - Information)
        /// </summary>
        public const int PublishingMessage = 23702;

        /// <summary>
        /// Logged when multiple messages are being published in bulk.
        /// (23703 - Information)
        /// </summary>
        public const int BulkPublishingMessages = 23703;

        /// <summary>
        /// Logged when executing the internal bulk publish routine.
        /// (23704 - Debug)
        /// </summary>
        public const int ExecutingBulkPublish = 23704;

        /// <summary>
        /// Logged when executing a query (publish+await response) from the publisher side.
        /// (23705 - Information)
        /// </summary>
        public const int ExecutingQuery = 23705;

        /// <summary>
        /// Logged when an inbox query has timed out waiting for a response.
        /// (23706 - Information)
        /// </summary>
        public const int InboxQueryTimedOut = 23706;

        /// <summary>
        /// Logged when an inbox query is being transmitted to a remote service/connector.
        /// (33707 - Information)
        /// </summary>
        public const int TransmittingInboxQuery = 33707;

        /// <summary>
        /// Logged when transmitting an inbox query failed.
        /// (53708 - Error)
        /// </summary>
        public const int TransmittingInboxQueryFailed = 53708;

        /// <summary>
        /// Logged when a query operation throws an exception.
        /// (53709 - Error)
        /// </summary>
        public const int QueryExceptionOccured = 53709;

        /// <summary>
        /// Logged when attempting a query response (publishing a response to a received query).
        /// (23710 - Information)
        /// </summary>
        public const int AttemptingQueryResponse = 23710;

        /// <summary>
        /// Logged when executing a query response on the dedicated query-response service.
        /// (33711 - Information)
        /// </summary>
        public const int ExecutingQueryResponseOnQueryResponseService = 33711;

        /// <summary>
        /// Logged when executing a query response on the inbox service.
        /// (33712 - Information)
        /// </summary>
        public const int ExecutingQueryResponseOnInboxService = 33712;

        /// <summary>
        /// Logged when executing a query response on the pub/sub service.
        /// (33713 - Information)
        /// </summary>
        public const int ExecutingQueryResponseOnPubSubService = 33713;

        /// <summary>
        /// Logged when attempting a query response that will be performed via the pub/sub service.
        /// (33714 - Information)
        /// </summary>
        public const int AttemptingQueryResponseOnPubSubService = 33714;

        /// <summary>
        /// Logged when starting a pub/sub listener that receives query responses.
        /// (23715 - Information)
        /// </summary>
        public const int StartingQueryResponsePubSubListener = 23715;

        /// <summary>
        /// Logged when a pub/sub query is being transmitted.
        /// (23716 - Information)
        /// </summary>
        public const int TransmittingPubSubQuery = 23716;

        /// <summary>
        /// Logged when a pub/sub query transmission fails.
        /// (53717 - Error)
        /// </summary>
        public const int TransmittingPubSubQueryFailed = 53717;

        /// <summary>
        /// Logged when awaiting a response for a pub/sub query.
        /// (23718 - Information)
        /// </summary>
        public const int WaitingOnPubSubQueryResponse = 23718;

        /// <summary>
        /// Logged when a pub/sub query response is received.
        /// (23719 - Information)
        /// </summary>
        public const int PubSubQueryResponseRecieved = 23719;
    }

    /// <summary>
    /// Consumer related event id values.
    /// Covers processing, acknowledgement and response handling while consuming messages.
    /// </summary>
    public static class Consuming
    {
        /// <summary>
        /// Logged when a service message is being processed by a consumer.
        /// (22408 - Information)
        /// </summary>
        public const int ProcessingServiceMessage = 22408;

        /// <summary>
        /// Logged when a service message is acknowledged by the consumer.
        /// (22409 - Information)
        /// </summary>
        public const int AcknowledgingServiceMessage = 22409;

        /// <summary>
        /// Logged when processing a service message fails.
        /// (52410 - Error)
        /// </summary>
        public const int ProcessingServiceMessageFailed = 52410;

        /// <summary>
        /// Logged when a message is queued / waiting to be processed.
        /// (22411 - Information)
        /// </summary>
        public const int WaitingToProcessMessage = 22411;

        /// <summary>
        /// Logged when a previously waiting message is released from its wait (ready to be processed).
        /// (22412 - Information)
        /// </summary>
        public const int ReleasingMessageWait = 22412;

        /// <summary>
        /// Logged when a consumed message results in returning an error response to the caller.
        /// (42413 - Warning)
        /// </summary>
        public const int ReturningErrorMessage = 42413;

        /// <summary>
        /// Logged when a consumed message results in returning a valid response to the caller.
        /// (32414 - Information)
        /// </summary>
        public const int ReturningValidResponse = 32414;

        /// <summary>
        /// Logged when a subscription cancellation token is being cancelled (consumer shutting down or unsubscribing).
        /// (22220 - Debug)
        /// </summary>
        public const int CancellingSubscriptionToken = 22220;
    }

    /// <summary>
    /// Transport related event id values.
    /// These ids cover connection discovery, pinging and issues locating connector connections.
    /// </summary>
    public static class Transport
    {
        /// <summary>
        /// Logged when a ping to a service connection is performed.
        /// (25102 - Debug)
        /// </summary>
        public const int PingServiceConnection = 25102;

        /// <summary>
        /// Logged when attempting to locate connections (discovery/lookup).
        /// (25103 - Debug)
        /// </summary>
        public const int LocatingConnections = 25103;

        /// <summary>
        /// Logged when locating connections for a specific map type (lookup for a typed mapping).
        /// (25104 - Debug)
        /// </summary>
        public const int LocatingConnectionsForMapType = 25104;

        /// <summary>
        /// Logged when unable to find any matching connections for a lookup.
        /// (55105 - Error)
        /// </summary>
        public const int UnableToLocateConnections = 55105;

        /// <summary>
        /// Logged when too many connections are located (unexpected multiplicity).
        /// (55106 - Error)
        /// </summary>
        public const int LocatedTooManyConnections = 55106;

        /// <summary>
        /// Logged when too many connections are located for a specific map type (unexpected multiplicity for typed lookup).
        /// (55107 - Error)
        /// </summary>
        public const int LocatedTooManyConnectionsForMapType = 55107;
    }
}

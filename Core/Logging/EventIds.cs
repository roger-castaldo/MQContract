using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace MQContract.Logging
{
    /// Band	Meaning
    /// 1xxxx Trace(hot path)
    /// 2xxxx Debug(state transitions)
    /// 3xxxx Information(lifecycle)
    /// 4xxxx Warning(recoverable)
    /// 5xxxx Error
    /// 9xxxx Critical / invariant broken(bug / data loss risk)

    public static class EventIds
    {
        /// Connections xx1xx
        /// Subscriptions xx2xx
        /// Middleware xx3xx
        /// ServiceMessages xx4xx
        /// Consumers xx5xx
        /// Resilience xx6xx
        /// ContractMessages xx7xx

        /// Lifetime x1xxx
        public static class Lifetime
        {
            public const int ClosingConnection = 21100;
            public const int ClosingAllInboxes = 31101;

            public const int EndingSubscription = 31200;
            public const int CreatingPubSubSubscription = 21201;
            public const int EstablishingPubSubSubscription = 31202;
            public const int PubSubSubscriptionEstablishmentFailed = 51203;
            public const int EstablishingPubSubServiceSubscription = 21204;
            public const int EstablishingPubSubServiceSubscriptionSucceeded = 31205;
            public const int EstablishingInboxSubscription = 21206;
            public const int SettingUpInboxSubscription = 31207;
            public const int EstablishingNewInboxSubscription = 21208;
            public const int CreatingQueryResponseSubscription = 21209;
            public const int ConstructingQueryResponseSubscription = 31210;
            public const int EstablishingQueryResponseSubscription = 21211;
            public const int QueryResponseSubscriptionEstablishmentFailed = 51212;
            public const int EstablishingQueryResponseSubscriptionServiceSubscription = 31213;
            public const int EstablishingQueryResponseServiceSubscription = 31214;
            public const int QueryResponseSubscriptionEstablishmentSucceeded = 31215;
            public const int EstablishingQueryResponseSubscriptionWithPubSubServiceSubscription = 31216;
            public const int QueryResponseSubscriptionEstablishmentFailedWithError = 51217;
            public const int DisposingQueryResponseSubscription = 31218;
            public const int QueryResponseSubscriptionDisposed = 31219;

            public const int RegisteringMiddleware = 21300;
            public const int EnablingMetricMiddleware = 21301;

            public const int ConsumerRegistrationFailed = 51500;
        }
        /// Pipeline x2xxx
        public static class Pipeline
        {
            public const int ExecutingBeforeMessageEncodeMiddleware = 22300;
            public const int ExecutingSpecificBeforeMessageEncodeMiddleware = 22301;
            public const int ExecutingAfterMessageEncodeMiddleware = 22302;
            public const int ExecutingBeforeMessageDecodeMiddleware = 22303;
            public const int ExecutingAfterMessageDecodeMiddleware = 22304;
            public const int ExecutingSpecificAfterMessageDecodeMiddleware = 22305;

            public const int ProducingServiceMessage = 22400;
            public const int FilteringServiceMessageByHeaders = 22401;
            public const int DecodingServiceMessage = 22402;
            public const int FilterServiceMessageInFull = 22403;
            public const int ProcessingQueryResponse = 22404;
            public const int AttemptingToProcessInboxMessage = 32405;
            public const int ProcessingQueryResponseFailed = 52406;
            public const int ReceivedInvalidQueryResponseMessage = 42407;

            public const int ResilienceRetryTriggered = 22600;
            public const int ResilienceFailedToFallback = 52601;

            public const int ExtractingQueryResponseType = 22700;
            public const int QueryReplyChannelMapped = 22701;
        }
        /// Publishers x3xxx
        public static class Publishing
        {
            public const int PublishingMessage = 23702;
            public const int BulkPublishingMessages = 23703;
            public const int ExecutingBulkPublish = 23704;
            public const int ExecutingQuery = 23705;
            public const int InboxQueryTimedOut = 23706;
            public const int TransmittingInboxQuery = 33707;
            public const int TransmittingInboxQueryFailed = 53708;
            public const int QueryExceptionOccured = 53709;
            public const int AttemptingQueryResponse = 23710;
            public const int ExecutingQueryResponseOnQueryResponseService = 33711;
            public const int ExecutingQueryResponseOnInboxService = 33712;
            public const int ExecutingQueryResponseOnPubSubService = 33713;
            public const int AttemptingQueryResponseOnPubSubService = 33714;
            public const int StartingQueryResponsePubSubListener = 23715;
            public const int TransmittingPubSubQuery = 23716;
            public const int TransmittingPubSubQueryFailed = 53717;
            public const int WaitingOnPubSubQueryResponse = 23718;
            public const int PubSubQueryResponseRecieved = 23719;
        }
        /// Consumers x4xxx
        public static class Consuming
        {
            public const int ProcessingServiceMessage = 22408;
            public const int AcknowledgingServiceMessage = 22409;
            public const int ProcessingServiceMessageFailed = 52410;
            public const int WaitingToProcessMessage = 22411;
            public const int ReleasingMessageWait = 22412;
            public const int ReturningErrorMessage = 42413;
            public const int ReturningValidResponse = 32414;

            public const int CancellingSubscriptionToken = 22220;
        }
        /// Transport x5xxx
        public static class Transport
        {
            public const int PingServiceConnection = 25102;
            public const int LocatingConnections = 25103;
            public const int LocatingConnectionsForMapType = 25104;
            public const int UnableToLocateConnections = 55105;
            public const int LocatedTooManyConnections = 55106;
            public const int LocatedTooManyConnectionsForMapType = 55107;
        }
    }
}

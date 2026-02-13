using MQContract.Interfaces;
using System.Diagnostics;

namespace AutomatedTesting.ConnectionTests
{
    internal static class ConnectionHelper
    {
        private const string MessageIdTag = "mqcontract.messageid";

        private static void ValidateCommonActivityTags<T>(Activity activity, Type connectionType, string messageTypeID, string messageID, string? connectionName)
        {
            Assert.IsTrue(activity.Tags.Any(t => Equals(t.Key, "mqcontract.serviceconnectiontype") && Equals(t.Value, connectionType.FullName)));
            Assert.IsTrue(connectionName==null || activity.Tags.Any(t => Equals(t.Key, "mqcontract.serviceconnectionname") && Equals(t.Value, connectionName)));
            Assert.AreEqual(typeof(T).Name, activity.Tags.FirstOrDefault(t => Equals(t.Key, "mqcontract.messagetypeclass")).Value);
            Assert.AreEqual(messageTypeID, activity.Tags.FirstOrDefault(t => Equals(t.Key, "mqcontract.messagetypetag")).Value);
            Assert.IsTrue(activity.Tags.Any(t => Equals(t.Key, MessageIdTag) && Equals(t.Value, messageID)));
        }

        private static void ValidateEncodeDecodeActivity(Activity activity, bool encode, string id)
        {
            Assert.IsTrue(activity.Events.Any(evnt => Equals((encode ? "MessageEncoded" : "MessageDecoded"), evnt.Name)
            && evnt.Tags.Any(t => Equals(t.Key, $"mqcontract.{(encode ? "messageencodingduration" : "messagedecodingduration")}") && !Equals(t.Value, "Unknown"))
            && evnt.Tags.Any(t => Equals(t.Key, MessageIdTag) && Equals(t.Value, id))));
        }

        private static void ValidatePublishActivityBase<T>(Activity activity, string activityType, Type connectionType, string messageTypeID, string messageID, MessageHeader header, string channel, bool success, bool withTrace, string? connectionName = null)
        {
            Assert.AreEqual(activityType, activity.DisplayName);
            Assert.AreEqual(ActivityKind.Producer, activity.Kind);
            Assert.AreEqual((success ? ActivityStatusCode.Ok : ActivityStatusCode.Error), activity.Status);
            if (withTrace)
            {
                Assert.AreEqual(activity.TraceId.ToString(), header["_traceParentId"]);
                Assert.AreEqual(activity.SpanId.ToString(), header["_traceParentSpanId"]);
            }
            else
            {
                Assert.IsNull(header["_traceParentId"]);
                Assert.IsNull(header["_traceParentSpanId"]);
            }
            ValidateCommonActivityTags<T>(activity, connectionType, messageTypeID, messageID, connectionName);
            Assert.AreEqual(channel, activity.Tags.FirstOrDefault(t => Equals(t.Key, "mqcontract.initialchannel")).Value);
            Assert.AreEqual(channel, activity.Tags.FirstOrDefault(t => Equals(t.Key, "mqcontract.transmissionchannel")).Value);
            ValidateEncodeDecodeActivity(activity, true, messageID);
            Assert.IsTrue(activity.Events.Any(evnt => Equals("MessageChannelMapped", evnt.Name)
            && evnt.Tags.Any(t => Equals(t.Key, "mqcontract.initialchannel") && Equals(t.Value, channel))
            && evnt.Tags.Any(t => Equals(t.Key, "mqcontract.mappedchannel") && Equals(t.Value, channel))));
        }

        public static void ValidatePublishActivity<T>(ServiceMessage message, Activity activity, string activityType, Type connectionType, bool success, bool withTrace, bool includePublish = true, string? connectionName = null)
        {
            ValidatePublishActivityBase<T>(activity, activityType, connectionType, message.MessageTypeID, message.ID, message.Header, message.Channel, success, withTrace, connectionName);
            Assert.AreEqual((includePublish ? 3 : 2), activity.Events.Count());
            if (includePublish)
            {
                Assert.IsTrue(activity.Events.Any(evnt => Equals("MessagePublished", evnt.Name)
                && evnt.Tags.Any(t => Equals(t.Key, "mqcontract.status") && Equals(t.Value, (success ? "Success" : "Fail"))
                && evnt.Tags.Any(t => Equals(t.Key, "mqcontract.serviceconnectiontype") && Equals(t.Value, connectionType.FullName))
                && (
                    connectionName==null
                    || evnt.Tags.Any(t => Equals(t.Key, "mqcontract.serviceconnectionname") && Equals(t.Value, connectionName))
                )
                && evnt.Tags.Any(t => Equals(t.Key, MessageIdTag) && Equals(t.Value, message.ID)))));
            }
        }

        public static void ValidatePublishActivity<T>(ServiceQueryResult queryResult, Activity activity, string activityType, Type connectionType, bool success, bool withTrace, string? connectionName = null)
        {
            ValidatePublishActivityBase<T>(activity, activityType, connectionType, queryResult.MessageTypeID, queryResult.ID, queryResult.Header, string.Empty, success, withTrace, connectionName);
            Assert.AreEqual(2, activity.Events.Count());
        }

        public static void ValidateBulkPublishActivity<T>(IEnumerable<ServiceMessage> messages, Activity activity, string activityType, Type connectionType, bool success, bool withTrace, string? connectionName = null)
        {
            Assert.AreEqual(activityType, activity.DisplayName);
            Assert.AreEqual(ActivityKind.Producer, activity.Kind);
            Assert.AreEqual((success ? ActivityStatusCode.Ok : ActivityStatusCode.Error), activity.Status);
            if (withTrace)
                Assert.IsTrue(messages.All(message => Equals(activity.TraceId.ToString(), message.Header["_traceParentId"])
                && Equals(activity.SpanId.ToString(), message.Header["_traceParentSpanId"])));
            else
                Assert.IsTrue(messages.All(message => message.Header["_traceParentId"]==null
                    && message.Header["_traceParentSpanId"]==null));
            Assert.AreEqual(3 * messages.Count(), activity.Events.Count());
            foreach (var message in messages)
            {
                ValidateCommonActivityTags<T>(activity, connectionType, message.MessageTypeID, message.ID, connectionName);
                Assert.AreEqual(message.Channel, activity.Tags.FirstOrDefault(t => Equals(t.Key, "mqcontract.initialchannel")).Value);
                Assert.AreEqual(message.Channel, activity.Tags.FirstOrDefault(t => Equals(t.Key, "mqcontract.transmissionchannel")).Value);
                ValidateEncodeDecodeActivity(activity, true, message.ID);
                Assert.IsTrue(activity.Events.Any(evnt => Equals("MessageChannelMapped", evnt.Name)
                && evnt.Tags.Any(t => Equals(t.Key, "mqcontract.initialchannel") && Equals(t.Value, message.Channel))
                && evnt.Tags.Any(t => Equals(t.Key, "mqcontract.mappedchannel") && Equals(t.Value, message.Channel))));
                Assert.IsTrue(activity.Events.Any(evnt => Equals("BulkMessagePublished", evnt.Name)
                && evnt.Tags.Any(t => Equals(t.Key, "mqcontract.status") && Equals(t.Value, (success ? "Success" : "Fail")))
                && evnt.Tags.Any(t => Equals(t.Key, "mqcontract.serviceconnectiontype") && Equals(t.Value, connectionType.FullName))
                && evnt.Tags.Any(t => Equals(t.Key, MessageIdTag) && Equals(t.Value, message.ID))));
            }
        }

        public static void ValidateConsumeActivity<T>(ServiceQueryResult queryResult, Activity activity, string activityType, Type connectionType, bool success, string? connectionName = null)
        {
            Assert.AreEqual(activityType, activity.DisplayName);
            Assert.AreEqual(ActivityKind.Consumer, activity.Kind);
            Assert.AreEqual((success ? ActivityStatusCode.Ok : ActivityStatusCode.Error), activity.Status);
            Assert.IsNull(queryResult.Header["_traceParentId"]);
            Assert.IsNull(queryResult.Header["_traceParentSpanId"]);
            ValidateCommonActivityTags<T>(activity, connectionType, queryResult.MessageTypeID, queryResult.ID, connectionName);
            Assert.AreEqual(string.Empty, activity.Tags.FirstOrDefault(t => Equals(t.Key, "mqcontract.recievedchannel")).Value);
            Assert.AreEqual(1, activity.Events.Count());
            ValidateEncodeDecodeActivity(activity, false, queryResult.ID);
            Assert.IsTrue(activity.Events.Any(evnt => Equals("MessageDecoded", evnt.Name)
            && evnt.Tags.Any(t => Equals(t.Key, "mqcontract.messagedecodingduration") && !Equals(t.Value, "Unknown"))
            && evnt.Tags.Any(t => Equals(t.Key, MessageIdTag) && Equals(t.Value, queryResult.ID))));
        }

        public static void ValidateConsumeActivity<T>(ReceivedServiceMessage recievedMessage, Activity activity, string activityType, Type connectionType, bool success, bool withTrace, string? connectionName = null)
        {
            Assert.AreEqual(activityType, activity.DisplayName);
            Assert.AreEqual(ActivityKind.Consumer, activity.Kind);
            Assert.AreEqual((success ? ActivityStatusCode.Ok : ActivityStatusCode.Error), activity.Status);
            if (withTrace)
                Assert.IsTrue(activity.ParentId?.Contains($"-{recievedMessage.Header["_traceParentId"]}-{recievedMessage.Header["_traceParentSpanId"]}-")??false);
            else
            {
                Assert.IsNull(recievedMessage.Header["_traceParentId"]);
                Assert.IsNull(recievedMessage.Header["_traceParentSpanId"]);
            }
            ValidateCommonActivityTags<T>(activity, connectionType, recievedMessage.MessageTypeID, recievedMessage.ID, connectionName);
            Assert.AreEqual(recievedMessage.Channel, activity.Tags.FirstOrDefault(t => Equals(t.Key, "mqcontract.recievedchannel")).Value);
            Assert.AreEqual(1, activity.Events.Count());
            ValidateEncodeDecodeActivity(activity, false, recievedMessage.ID);
        }

        public static void ValidateConsumeActivity<T>(ReceivedServiceMessage recievedMessage, Activity activity, string activityType, Type connectionType, Type consumerType, bool success, bool withTrace)
        {
            ValidateConsumeActivity<T>(recievedMessage, activity, activityType, connectionType, success, withTrace);
            Assert.AreEqual(consumerType.Name, activity.Tags.FirstOrDefault(t => Equals(t.Key, "mqcontract.consumerclass")).Value);
        }

        public static (ActivityListener listener, List<Activity> capturedActivities, string sourceName) SetupTelemetry()
        {
            var sourceName = Helper.GenerateRandomString(20);
            var capturedActivities = new List<Activity>();

            var listener = new ActivityListener()
            {
                ShouldListenTo = source => source.Name == sourceName,
                Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllData,
                ActivityStarted = activity => capturedActivities.Add(activity),
                ActivityStopped = _ => { }
            };
            ActivitySource.AddActivityListener(listener);
            return (listener, capturedActivities, sourceName);
        }

        public static void AssignResiliencePolicy<T>(
            IMultiServiceContractConnection contractConnection,
            string? serviceName, string? channel, Type? messageType, bool useGenerics,
            int? retryCount, int? circuitBreakFailureCount)
            => AssignResiliencePolicy<T, IMultiServiceContractConnection>(
                contractConnection,
                channel,
                messageType,
                useGenerics,
                retryCount,
                circuitBreakFailureCount,
                serviceName
            );

        public static void AssignResiliencePolicy<T>(
            IMappedContractConnection contractConnection,
            string? serviceName, string? channel, Type? messageType, bool useGenerics,
            int? retryCount, int? circuitBreakFailureCount)
            => AssignResiliencePolicy<T, IMappedContractConnection>(
                contractConnection,
                channel,
                messageType,
                useGenerics,
                retryCount,
                circuitBreakFailureCount,
                serviceName
            );

        public static void AssignResiliencePolicy<T>(IContractedConnection contractConnection,
            string? channel, Type? messageType, bool useGenerics,
            int? retryCount, int? circuitBreakFailureCount)
            => AssignResiliencePolicy<T, IContractedConnection>(
                contractConnection,
                channel,
                messageType,
                useGenerics,
                retryCount,
                circuitBreakFailureCount,
                null
            );

        public static void AssignResiliencePolicy<T, C>(
            IResilientContractConnection<C> contractConnection,
            string? channel, Type? messageType, bool useGenerics,
            int? retryCount, int? circuitBreakFailureCount,
            string? serviceName)
            where C : IBaseContractConnection
        {
            (int retryCount, Func<int, TimeSpan> sleepDurationProvider)? retryPolicy = null;
            if (retryCount!=null)
                retryPolicy = (retryCount.Value, (cnt) => TimeSpan.FromMilliseconds(5));
            (int handledEventsAllowedBeforeBreaking, TimeSpan durationOfBreak)? circuitBreakerPolicy = null;
            if (circuitBreakFailureCount!=null)
                circuitBreakerPolicy = (circuitBreakFailureCount.Value, TimeSpan.FromMinutes(1));
            if (serviceName!=null && contractConnection is IMappableContractConnection<C> mappableContractConnection)
            {
                if (channel!=null)
                    mappableContractConnection.RegisterResiliencePolicy(
                        serviceName,
                        channel,
                        retryPolicy: retryPolicy,
                        circuitBreakPolicy: circuitBreakerPolicy
                    );
                else if (messageType!=null)
                    mappableContractConnection.RegisterResiliencePolicy(
                        serviceName,
                        messageType,
                        retryPolicy: retryPolicy,
                        circuitBreakPolicy: circuitBreakerPolicy
                    );
                else if (useGenerics)
                    mappableContractConnection.RegisterResiliencePolicy<T>(
                        serviceName,
                        retryPolicy: retryPolicy,
                        circuitBreakPolicy: circuitBreakerPolicy
                    );
                else
                    mappableContractConnection.RegisterResiliencePolicy(
                        serviceName,
                        retryPolicy: retryPolicy,
                        circuitBreakPolicy: circuitBreakerPolicy
                    );
            }
            else
            {
                if (channel!=null)
                    contractConnection.RegisterResiliencePolicy(
                        channel,
                        retryPolicy: retryPolicy,
                        circuitBreakPolicy: circuitBreakerPolicy
                    );
                else if (messageType!=null)
                    contractConnection.RegisterResiliencePolicy(
                        messageType,
                        retryPolicy: retryPolicy,
                        circuitBreakPolicy: circuitBreakerPolicy
                    );
                else if (useGenerics)
                    contractConnection.RegisterResiliencePolicy<T>(
                        retryPolicy: retryPolicy,
                        circuitBreakPolicy: circuitBreakerPolicy
                    );
                else
                    contractConnection.RegisterResiliencePolicy(
                        retryPolicy: retryPolicy,
                        circuitBreakPolicy: circuitBreakerPolicy
                    );
            }
        }
    }
}

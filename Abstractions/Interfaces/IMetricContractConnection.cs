using MQContract.Interfaces.Middleware;
using System.Diagnostics.Metrics;

namespace MQContract.Interfaces
{
    /// <summary>
    /// Houses the metric pieces for a given contract connection
    /// </summary>
    /// <typeparam name="CC">The underlying type that is being represented here which must be IBaseContractConnection, CC is used for method chaining.</typeparam>
    public interface IMetricContractConnection<CC> : IResilientContractConnection<CC>
        where CC : IBaseContractConnection
    {
        /// <summary>
        /// Register a middleware of a given type T to be used by the contract connection
        /// </summary>
        /// <typeparam name="T">The type of middle ware to register, it must implement IBeforeDecodeMiddleware or IBeforeEncodeMiddleware or IAfterDecodeMiddleware or IAfterEncodeMiddleware</typeparam>
        /// <returns>The Contract Connection instance to allow chaining calls</returns>
        CC RegisterMiddleware<T>()
            where T : IMiddleware;
        /// <summary>
        /// Register a middleware of a given type T to be used by the contract connection
        /// </summary>
        /// <param name="constructInstance">Callback to create the instance</param>
        /// <typeparam name="T">The type of middle ware to register, it must implement IBeforeDecodeMiddleware or IBeforeEncodeMiddleware or IAfterDecodeMiddleware or IAfterEncodeMiddleware</typeparam>
        /// <returns>The Contract Connection instance to allow chaining calls</returns>
        CC RegisterMiddleware<T>(Func<T> constructInstance)
            where T : IMiddleware;
        /// <summary>
        /// Register a middleware of a given type T to be used by the contract connection
        /// </summary>
        /// <typeparam name="T">The type of middle ware to register, it must implement IBeforeEncodeSpecificTypeMiddleware&lt;M&gt; or IAfterDecodeSpecificTypeMiddleware&lt;M&gt;</typeparam>
        /// <typeparam name="M">The message type that this middleware is specifically called for</typeparam>
        /// <returns>The Contract Connection instance to allow chaining calls</returns>
        CC RegisterMiddleware<T, M>()
            where T : ISpecificTypeMiddleware<M>;
        /// <summary>
        /// Register a middleware of a given type T to be used by the contract connection
        /// </summary>
        /// <param name="constructInstance">Callback to create the instance</param>
        /// <typeparam name="T">The type of middle ware to register, it must implement IBeforeEncodeSpecificTypeMiddleware&lt;M&gt; or IAfterDecodeSpecificTypeMiddleware&lt;M&gt;</typeparam>
        /// <typeparam name="M">The message type that this middleware is specifically called for</typeparam>
        /// <returns>The Contract Connection instance to allow chaining calls</returns>
        CC RegisterMiddleware<T, M>(Func<T> constructInstance)
            where T : ISpecificTypeMiddleware<M>;
        /// <summary>
        /// Called to enable Open Telemetry capabilities within the Contract Connection which will include passing activity information across the messages
        /// </summary>
        /// <param name="activitySource">Used to override the Activity Source name if desired, otherwise it will default to MQContract</param>
        /// <param name="linkActivitiesAcrossSystems">Setting this to true will automatically include headers in the messages to allow for linking the calling activity on one service to the activity on the receiver</param>
        /// <returns>The Contract Connection instance to allow chaining calls</returns>
        CC EnableOpenTelemetry(string activitySource = "MQContract", bool linkActivitiesAcrossSystems = true);
        /// <summary>
        /// Called to activate the metrics tracking middleware for this connection instance
        /// </summary>
        /// <param name="meter">The Meter item to create all system metrics against</param>
        /// <param name="useInternal">Indicates if the internal metrics collector should be used</param>
        /// <returns>The Contract Connection instance to allow chaining calls</returns>
        /// <remarks>
        /// For the Meter metrics, all durations are in ms and the following values and patterns will apply:
        /// mqcontract.messages.sent.count = count of messages sent (Counter&lt;long&gt;)
        /// mqcontract.messages.sent.bytes = count of bytes sent (message data) (Counter&lt;long&gt;)
        /// mqcontract.messages.received.count = count of messages received (Counter&lt;long&gt;)
        /// mqcontract.messages.received.bytes = count of bytes received (message data) (Counter&lt;long&gt;)
        /// mqcontract.messages.encodingduration = milliseconds to encode messages (Histogram&lt;double&gt;)
        /// mqcontract.messages.decodingduration = milliseconds to decode messages (Histogram&lt;double&gt;)
        /// mqcontract.types.{MessageTypeName}.{MessageVersion(_ instead of .)}.sent.count = count of messages sent of a given type (Counter&lt;long&gt;)
        /// mqcontract.types.{MessageTypeName}.{MessageVersion(_ instead of .)}.sent.bytes = count of bytes sent (message data) of a given type (Counter&lt;long&gt;)
        /// mqcontract.types.{MessageTypeName}.{MessageVersion(_ instead of .)}.received.count = count of messages received of a given type (Counter&lt;long&gt;)
        /// mqcontract.types.{MessageTypeName}.{MessageVersion(_ instead of .)}.received.bytes = count of bytes received (message data) of a given type (Counter&lt;long&gt;)
        /// mqcontract.types.{MessageTypeName}.{MessageVersion(_ instead of .)}.encodingduration = milliseconds to encode messages of a given type (Histogram&lt;double&gt;)
        /// mqcontract.types.{MessageTypeName}.{MessageVersion(_ instead of .)}.decodingduration = milliseconds to decode messages of a given type (Histogram&lt;double&gt;)
        /// mqcontract.channels.{Channel}.sent.count = count of messages sent for a given channel (Counter&lt;long&gt;)
        /// mqcontract.channels.{Channel}.sent.bytes = count of bytes sent (message data) for a given channel (Counter&lt;long&gt;)
        /// mqcontract.channels.{Channel}.received.count = count of messages received for a given channel (Counter&lt;long&gt;)
        /// mqcontract.channels.{Channel}.received.bytes = count of bytes received (message data) for a given channel (Counter&lt;long&gt;)
        /// mqcontract.channels.{Channel}.encodingduration = milliseconds to encode messages for a given channel (Histogram&lt;double&gt;)
        /// mqcontract.channels.{Channel}.decodingduration = milliseconds to decode messages for a given channel (Histogram&lt;double&gt;)
        /// </remarks>
        CC AddMetrics(Meter? meter, bool useInternal);
        /// <summary>
        /// Called to get a snapshot of the current global metrics.  Will return null if internal metrics are not enabled.
        /// </summary>
        /// <param name="sent">true when the sent metrics are desired, false when received are desired</param>
        /// <returns>A record of the current metric snapshot or null if not available</returns>
        IContractMetric? GetSnapshot(bool sent);
        /// <summary>
        /// Called to get a snapshot of the metrics for a given message type.  Will return null if internal metrics are not enabled.
        /// </summary>
        /// <param name="messageType">The type of message to look for</param>
        /// <param name="sent">true when the sent metrics are desired, false when received are desired</param>
        /// <returns>A record of the current metric snapshot or null if not available</returns>
        IContractMetric? GetSnapshot(Type messageType, bool sent);
        /// <summary>
        /// Called to get a snapshot of the metrics for a given message type.  Will return null if internal metrics are not enabled.
        /// </summary>
        /// <typeparam name="T">The type of message to look for</typeparam>
        /// <param name="sent">true when the sent metrics are desired, false when received are desired</param>
        /// <returns>A record of the current metric snapshot or null if not available</returns>
        IContractMetric? GetSnapshot<T>(bool sent)
            where T : class;
        /// <summary>
        /// Called to get a snapshot of the metrics for a given message channel.  Will return null if internal metrics are not enabled.
        /// </summary>
        /// <param name="channel">The channel to look for</param>
        /// <param name="sent">true when the sent metrics are desired, false when received are desired</param>
        /// <returns>A record of the current metric snapshot or null if not available</returns>
        IContractMetric? GetSnapshot(string channel, bool sent);
    }
}

using MQContract.Interfaces.Service;
using MQContract.Messages;
using MQContract.Middleware;
using System.Diagnostics;

namespace MQContract.Connections
{
    internal static class OtelHelper
    {
        private const string MessagePublishStatusKey = $"{OpenTelemetryMiddleware.KeyBase}.status";
        private const string ConnectionNameKey = $"{OpenTelemetryMiddleware.KeyBase}.serviceconnectionname";
        private const string ConnectionTypeKey = $"{OpenTelemetryMiddleware.KeyBase}.serviceconnectiontype";

        public static void AssignConnectionType(Activity? activity, IMessageServiceConnection serviceConnection, string? connectionName = null)
        {
            activity?.SetTag(ConnectionTypeKey, serviceConnection.GetType().FullName);
            if (connectionName != null) 
                activity?.SetTag(ConnectionNameKey, connectionName);
        }

        public static KeyValuePair<string, object?> CreateConnectionTypeTag(IMessageServiceConnection serviceConnection)
            => new(ConnectionTypeKey, serviceConnection.GetType().FullName);

        public static KeyValuePair<string, object?> CreateMessagePublishStatusTag(TransmissionResult? transmissionResult)
            => new(MessagePublishStatusKey, (transmissionResult?.IsError??true ? "Fail" : "Success"));

        public static void AddMessagePublishedEvent(Activity? activity, ServiceMessage serviceMessage, TransmissionResult result, IMessageServiceConnection serviceConnection, string? connectionName = null)
            => activity?.AddEvent(new("MessagePublished", tags: new([
                new(OpenTelemetryMiddleware.MessageIdKey,serviceMessage.ID),
                new(MessagePublishStatusKey,(result.IsError ? "Fail" : "Success")),
                CreateConnectionTypeTag(serviceConnection),
                new(ConnectionNameKey,connectionName)
            ])));
    }
}

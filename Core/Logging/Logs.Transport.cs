using Microsoft.Extensions.Logging;

namespace MQContract.Logging
{
    internal static partial class Logs
    {
        internal static partial class Transport
        {
            [LoggerMessage(EventId = EventIds.Transport.PingServiceConnection,Level = LogLevel.Debug,Message = "Attempting to call Ping against an underlying service connection")]
            public static partial void PingServiceConnection(ILogger logger);

            [LoggerMessage(EventId = EventIds.Transport.LocatingConnections,Level = LogLevel.Debug,Message = "Locating connection(s) for {Channel}, {MessageType}, {HeaderKeys}")]
            public static partial void LocatingConnections(ILogger logger, string channel, Type messageType, IEnumerable<string> headerKeys);

            [LoggerMessage(EventId = EventIds.Transport.LocatingConnectionsForMapType,Level = LogLevel.Debug,Message = "Locating a connection for {Channel}, {MessageType} and {MapType}")]
            public static partial void LocatingConnectionsForMapType(ILogger logger, string? channel, Type messageType, ChannelMapper.MapTypes mapType);

            [LoggerMessage(EventId = EventIds.Transport.UnableToLocateConnections,Level = LogLevel.Error,Message = "Unable to locate any connections matching {Channel}, {MessageType}, {HeaderKeys}")]
            public static partial void UnableToLocateConnections(ILogger logger, string channel, Type messageType, IEnumerable<string> headerKeys);

            [LoggerMessage(EventId = EventIds.Transport.LocatedTooManyConnections,Level = LogLevel.Error,Message = "Located more than 1 connection for {Channel}, {MessageType} and {HeaderKeys}")]
            public static partial void LocatedTooManyConnections(ILogger logger, string channel, Type messageType, IEnumerable<string> headerKeys);

            [LoggerMessage(EventId = EventIds.Transport.LocatedTooManyConnectionsForMapType,Level = LogLevel.Error,Message = "Located more than 1 connection for {Channel}, {MessageType} and {MapType}")]
            public static partial void LocatedTooManyConnectionsForMapType(ILogger logger, string? channel, Type messageType, ChannelMapper.MapTypes mapType);
        }
    }
}

using Microsoft.Extensions.Logging;

namespace MQContract.Loggers
{
    /// 4000-4999
    internal static partial class MappableConnectionLog
    {
        [LoggerMessage(
            EventId = 4000,
            Level = LogLevel.Debug,
            Message = "Locating connection(s) for {Channel}, {MessageType}, {HeaderKeys}"
            )]
        public static partial void LocatingConnections(ILogger logger, string channel, Type messageType, string headerKeys);

        [LoggerMessage(
            EventId = 4001,
            Level = LogLevel.Debug,
            Message = "Locating a connection for {Channel}, {MessageType} and {MapType}"
            )]
        public static partial void LocatingConnectionsForMapType(ILogger logger, string? channel, Type messageType, ChannelMapper.MapTypes mapType);

        [LoggerMessage(
            EventId = 4900,
            Level = LogLevel.Error,
            Message = "Unable to locate any connections matching {Channel}, {MessageType}, {HeaderKeys}"
            )]
        public static partial void UnableToLocateConnections(ILogger logger, string channel, Type messageType, string headerKeys);

        [LoggerMessage(
            EventId = 4901,
            Level = LogLevel.Error,
            Message = "Located more than 1 connection for {Channel}, {MessageType} and {HeaderKeys}"
            )]
        public static partial void LocatedTooManyConnections(ILogger logger, string channel, Type messageType, string headerKeys);

        [LoggerMessage(
            EventId = 4902,
            Level = LogLevel.Error,
            Message = "Located more than 1 connection for {Channel}, {MessageType} and {MapType}"
            )]
        public static partial void LocatedTooManyConnectionsForMapType(ILogger logger, string? channel, Type messageType, ChannelMapper.MapTypes mapType);
    }
}

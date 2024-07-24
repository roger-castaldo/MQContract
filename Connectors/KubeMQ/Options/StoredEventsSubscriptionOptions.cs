using MQContract.Interfaces.Service;
using static MQContract.KubeMQ.Options.StoredEventsSubscriptionOptions;

namespace MQContract.KubeMQ.Options
{
    public record StoredEventsSubscriptionOptions(MessageReadStyle ReadStyle=MessageReadStyle.StartNewOnly,long ReadOffset=0) : IServiceChannelOptions
    {
        /// <summary>
        /// These are the different read styles to use when subscribing to a stored Event PubSub
        /// </summary>
        public enum MessageReadStyle
        {
            /// <summary>
            /// Start from the new ones (unread ones) only
            /// </summary>
            StartNewOnly = 1,
            /// <summary>
            /// Start at the beginning
            /// </summary>
            StartFromFirst = 2,
            /// <summary>
            /// Start at the last message
            /// </summary>
            StartFromLast = 3,
            /// <summary>
            /// Start at message number X (this value is specified when creating the listener)
            /// </summary>
            StartAtSequence = 4,
            /// <summary>
            /// Start at time X (this value is specified when creating the listener)
            /// </summary>
            StartAtTime = 5,
            /// <summary>
            /// Start at Time Delte X (this value is specified when creating the listener)
            /// </summary>
            StartAtTimeDelta = 6
        };

    }
}

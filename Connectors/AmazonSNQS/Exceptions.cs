using Amazon.SimpleNotificationService;
using Amazon.SQS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MQContract.AmazonSNQS
{
    /// <summary>
    /// Thrown when an attempt to create a subscription occurs but the SQSClient is null
    /// </summary>
    public class SqsClientNullException : ArgumentNullException
    {
        internal SqsClientNullException()
            : base("sqsClient","The connection must have an SQSClient in order to create a subscription") { }

        internal static void ThrowIfNull(AmazonSQSClient? sqsClient)
        {
            if (sqsClient==null)
                throw new SqsClientNullException();
        }
    }

    /// <summary>
    /// Thrown when the queue for a subscription request cannot be found
    /// </summary>
    public class UnableToLocateQueueException : ArgumentNullException
    {
        internal UnableToLocateQueueException(string channel)
            : base("queue", $"Unable to locate the queue for {channel}") { }

        internal static void ThrowIfNullOrWhitespace(string? queue,string channel)
        {
            if (string.IsNullOrWhiteSpace(queue))
                throw new UnableToLocateQueueException(channel);
        }
    }

    /// <summary>
    /// Thrown when the Message recieved from a Queue is not of the excepted format
    /// </summary>
    public class InvalidQueueMessageException : InvalidCastException
    {
        internal InvalidQueueMessageException()
            : base("Unable to process message in queue, expected a JSON content with the property Message containing the message.") { }
    }

    /// <summary>
    /// Thrown when a publish call is attempted but you have not supplied either of the connection clients
    /// </summary>
    public class NoClientsSetException : Exception
    {
        internal NoClientsSetException()
            : base("Both the sqsClient and snsClient are null, unable to publish any messages") { }

        internal static void ThrowIfBothNull(AmazonSimpleNotificationServiceClient? snsClient , AmazonSQSClient? sqsClient)
        {
            if (snsClient==null && sqsClient==null)
                throw new TransmissionException(new NoClientsSetException(),true);
        }
    }

    /// <summary>
    /// Thrown when a public call is attempted but it is unable to locate a topic or queue for the given channel name
    /// </summary>
    public class NoChannelFoundException : ArgumentOutOfRangeException
    {
        internal NoChannelFoundException(string channel)
            : base(nameof(channel), "Unable to locate a Topic or Queue for the given channel") {
            Channel=channel;
        }

        /// <summary>
        /// The channel that was unable to be translated to a topic or queue
        /// </summary>
        public string Channel { get; private init; }
    }
}

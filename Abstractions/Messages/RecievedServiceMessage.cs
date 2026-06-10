namespace MQContract.Messages
{
    /// <summary>
    /// A Received Service Message that gets passed back up into the Contract Connection when a message is received from the underlying service connection
    /// </summary>
    public record ReceivedServiceMessage : EncodedMessage
    {
        /// <summary>
        /// Houses the channel that the Received Message was received on
        /// </summary>
        public string Channel { get; }

        /// <summary>
        /// A timestamp for when the message was received
        /// </summary>
        public DateTime ReceivedTimestamp { get; private init; } = DateTime.Now;

        /// <summary>
        /// The acknowledgement callback to be called when the message is received if the underlying service requires it
        /// </summary>
        public Func<ValueTask>? Acknowledge { get; }

        /// <summary>
        /// Default constructor for a service message
        /// </summary>
        /// <param name="id">The unique ID of the message</param>
        /// <param name="messageTypeID">An identifier that identifies the type of message encoded</param>
        /// <param name="channel">The channel to transmit the message on</param>
        /// <param name="header">The headers to transmit with the message</param>
        /// <param name="data">The content of the message</param>
        /// <param name="acknowledge">The acknowledgement callback to be called when the message is received if the underlying service requires it</param>
        public ReceivedServiceMessage(string id, string messageTypeID, string channel, MessageHeader header, ReadOnlyMemory<byte> data, Func<ValueTask>? acknowledge = null)
            : base(id, header, messageTypeID, data)
        {
            Channel = channel;
            Acknowledge = acknowledge;
        }
    }
}

using MQContract.Interfaces.Messages;

namespace MQContract.Messages
{
    /// <summary>
    /// Used as the base of an Encoded Message which can be a ServiceMessage, ReceivedServiceMessage, or ServiceQueryResult. This is used to house the common properties of all of these message types and to avoid code duplication.
    /// </summary>
    public record EncodedMessage : IEncodedMessage, IDisposable
    {
        private bool disposedValue;

        /// <summary>
        /// The message type id to transmit across
        /// </summary>
        public string MessageTypeID { get; }
        /// <summary>
        /// The header for the given message
        /// </summary>
        public MessageHeader Header { get; }
        /// <summary>
        /// The encoded message
        /// </summary>
        public ReadOnlyMemory<byte> Data { get; set; }
        /// <summary>
        /// The unique ID of the message
        /// </summary>
        public string ID { get; }

        /// <summary>
        /// Default constructor for a Service Query Result
        /// </summary>
        /// <param name="id">The unique ID of the message</param>
        /// <param name="messageTypeID">An identifier that identifies the type of message encoded</param>
        /// <param name="header">The headers to transmit with the message</param>
        /// <param name="data">The content of the message</param>
        protected EncodedMessage(string id, MessageHeader header, string messageTypeID, ReadOnlyMemory<byte> data)
        {
            ID = id;
            MessageTypeID = messageTypeID;
            Header = header;
            Data = data;
        }

        /// <summary>
        /// Releases the resources used by the current instance and optionally disposes of managed resources.
        /// </summary>
        /// <remarks>This method is called by the public Dispose method and should be overridden in
        /// derived classes to release additional resources. It is important to ensure that this method is called only
        /// once to avoid disposing of resources multiple times.</remarks>
        /// <param name="disposing">Indicates whether to release both managed and unmanaged resources (<see langword="true"/>) or only unmanaged
        /// resources (<see langword="false"/>).</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    ((IDisposable)Header).Dispose();
                }
                disposedValue=true;
            }
        }

        void IDisposable.Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}

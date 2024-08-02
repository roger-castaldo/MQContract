using Grpc.Core;

namespace MQContract.KubeMQ
{
    /// <summary>
    /// Houses the Connection Settings to use to connect to the particular instance of KubeMQ
    /// </summary>
    public class ConnectionOptions
    {
        private const int DEFAULT_MAX_SIZE = 1024 * 1024 * 100; // 100MB
        /// <summary>
        /// The address and port to connection to.  This can be the dns name or an ip address.
        /// Use the format {ip/name}:{portnumber}.  Typically KubeMQ is configured to listen on 
        /// port 50000
        /// </summary>
        public string Address { get; init; } = "http://localhost:50000";
        /// <summary>
        /// The Unique Identification to be used when connecting to the KubeMQ server
        /// </summary>
        public string ClientId { get; init; } = Guid.NewGuid().ToString();
        /// <summary>
        /// The authentication token to use when connecting to the KubeMQ server
        /// </summary>
        public string AuthToken { get; init; } = string.Empty;
        /// <summary>
        /// The SSL Root certificate to use when connecting to the KubeMQ server
        /// </summary>
        public string SSLRootCertificate { get; init; } = string.Empty;
        /// <summary>
        /// The SSL Key to use when connecting to the KubeMQ server
        /// </summary>
        public string SSLKey { get; init; } = string.Empty;
        /// <summary>
        /// The SSL Certificat to use when connecting to the KubeMQ server
        /// </summary>
        public string SSLCertificate { get; init; } = string.Empty;
        /// <summary>
        /// Milliseconds to wait in between attempted reconnects to the KubeMQ server
        /// </summary>
        public int ReconnectInterval { get; init; } = 1000;
        /// <summary>
        /// The maximum body size in bytes configured on the KubeMQ server, default is 100MB.
        /// If the encoded message exceeds the size, it will zip it in an attempt to transmit the 
        /// message.  If it still fails in size, an exception will be thrown.
        /// </summary>
        public int MaxBodySize { get; init; } = DEFAULT_MAX_SIZE;
        /// <summary>
        /// Timeout in milliseconds to use as a default for RPC calls if there is an override desired. 
        /// Otherwise the default is 5000.
        /// </summary>
        public int? DefaultRPCTimeout { get; init; } = null;

        /// <summary>
        /// Logging instance to use in underlying service layer
        /// </summary>
        public Microsoft.Extensions.Logging.ILogger? Logger { get; init; } = null;

        internal SslCredentials? SSLCredentials
        {
            get
            {
                if (string.IsNullOrEmpty(SSLCertificate))
                    return null;
                if (!string.IsNullOrEmpty(SSLCertificate) && !string.IsNullOrEmpty(SSLKey))
                    return new SslCredentials(SSLRootCertificate, new KeyCertificatePair(SSLCertificate, SSLKey));
                else
                    return new SslCredentials(SSLRootCertificate);
            }
        }

        internal Metadata GrpcMetadata
        {
            get
            {
                var result = new Metadata();
                if (!string.IsNullOrEmpty(AuthToken))
                    result.Add(new Metadata.Entry("authorization", AuthToken));
                return result;
            }
        }
    }
}

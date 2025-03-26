using Microsoft.Extensions.Logging;
using MQContract.Interfaces;
using MQContract.Interfaces.Encoding;
using MQContract.Interfaces.Encrypting;
using MQContract.Interfaces.Service;

namespace MQContract
{
    /// <summary>
    /// The primary class for producing an instance of either an IContractConnection or an IMultiServiceContractConnection
    /// </summary>
    public static class ContractConnection
    {
        /// <summary>
        /// This is the call used to create an instance of a Contract Connection which will return the Interface
        /// </summary>
        /// <param name="serviceConnection">The service connection implementation to use for the underlying message requests.</param>
        /// <param name="defaultMessageEncoder">A default message encoder implementation if desired.  If there is no specific encoder for a given type, this encoder would be called.  The built in default being used dotnet Json serializer.</param>
        /// <param name="defaultMessageEncryptor">A default message encryptor implementation if desired.  If there is no specific encryptor </param>
        /// <param name="serviceProvider">A service prodivder instance supplied in the case that dependency injection might be necessary</param>
        /// <param name="logger">An instance of a logger if logging is desired</param>
        /// <param name="channelMapper">An instance of a ChannelMapper used to translate channels from one instance to another based on class channel attributes or supplied channels if necessary.
        /// For example, it might be necessary for a Nats.IO instance when you are trying to read from a stored message stream that is comprised of another channel or set of channels
        /// </param>
        /// <returns>An instance of IContractConnection</returns>
        public static IContractedConnection Instance(IMessageServiceConnection serviceConnection,
        IMessageEncoder? defaultMessageEncoder = null,
        IMessageEncryptor? defaultMessageEncryptor = null,
        IServiceProvider? serviceProvider = null,
        ILogger? logger = null,
        ChannelMapper? channelMapper = null)
            => new MQContract.Connections.Connection(serviceConnection, defaultMessageEncoder, defaultMessageEncryptor, serviceProvider, logger, channelMapper);

        /// <summary>
        /// This is the call used to create an instance of a Multi Service Contract Connection which will return the Interface
        /// </summary>
        /// <param name="defaultMessageEncoder">A default message encoder implementation if desired.  If there is no specific encoder for a given type, this encoder would be called.  The built in default being used dotnet Json serializer.</param>
        /// <param name="defaultMessageEncryptor">A default message encryptor implementation if desired.  If there is no specific encryptor </param>
        /// <param name="serviceProvider">A service prodivder instance supplied in the case that dependency injection might be necessary</param>
        /// <param name="logger">An instance of a logger if logging is desired</param>
        /// <param name="channelMapper">An instance of a ChannelMapper used to translate channels from one instance to another based on class channel attributes or supplied channels if necessary.
        /// For example, it might be necessary for a Nats.IO instance when you are trying to read from a stored message stream that is comprised of another channel or set of channels
        /// </param>
        /// <returns>An instance of IMultiServiceContractConnection</returns>
        public static IMultiServiceContractConnection MultiServiceInstance(IMessageEncoder? defaultMessageEncoder = null,
        IMessageEncryptor? defaultMessageEncryptor = null,
        IServiceProvider? serviceProvider = null,
        ILogger? logger = null,
        ChannelMapper? channelMapper = null)
            => new MQContract.Connections.MultiServiceConnection(defaultMessageEncoder, defaultMessageEncryptor, serviceProvider, logger, channelMapper);

        /// <summary>
        /// This is the call used to create an instance of a Mapped Service Contract Connection which will return the Interface
        /// </summary>
        /// <param name="defaultMessageEncoder">A default message encoder implementation if desired.  If there is no specific encoder for a given type, this encoder would be called.  The built in default being used dotnet Json serializer.</param>
        /// <param name="defaultMessageEncryptor">A default message encryptor implementation if desired.  If there is no specific encryptor </param>
        /// <param name="serviceProvider">A service prodivder instance supplied in the case that dependency injection might be necessary</param>
        /// <param name="logger">An instance of a logger if logging is desired</param>
        /// <param name="channelMapper">An instance of a ChannelMapper used to translate channels from one instance to another based on class channel attributes or supplied channels if necessary.
        /// For example, it might be necessary for a Nats.IO instance when you are trying to read from a stored message stream that is comprised of another channel or set of channels
        /// </param>
        /// <returns>An instance of IMultiServiceContractConnection</returns>
        public static IMappedContractConnection MappedServiceInstance(IMessageEncoder? defaultMessageEncoder = null,
        IMessageEncryptor? defaultMessageEncryptor = null,
        IServiceProvider? serviceProvider = null,
        ILogger? logger = null,
        ChannelMapper? channelMapper = null)
            => new MQContract.Connections.MappedConnection(defaultMessageEncoder, defaultMessageEncryptor, serviceProvider, logger, channelMapper);
    }
}

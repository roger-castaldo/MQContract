namespace MQContract;

/// <summary>
/// Thrown when an error occurs attempting to transmit a given message and is flagged if it is fatal or not
/// </summary>
/// <param name="underlyingError">The underlying error that occured</param>
/// <param name="isFatal">Flag if this error is a fatal error (don't run resilience when it is fatal)</param>
public sealed class TransmissionException(Exception underlyingError, bool isFatal = false)
    : Exception("Transmission Exception occured", underlyingError)
{
    /// <summary>
    /// Indicates if the error that occured is fatal
    /// </summary>
    public bool IsFatal => isFatal;
}

/// <summary>
/// Houses the different types of Resilience errors that can occur
/// </summary>
public enum ResilienceTypes
{
    /// <summary>
    /// Failed through the retry attempts defined
    /// </summary>
    Retry,
    /// <summary>
    /// Failed due to the Circuit being broken based on the policy defined
    /// </summary>
    CircuitBreak
};

/// <summary>
/// Thrown when a Resilience failure occurs attempting to transmit a message
/// </summary>
/// <param name="type">The type of resilience failure that occured</param>
/// <param name="error">An underlying error for the resilience (may be a circuit broken or the underlying error that retry has failed through)</param>
public sealed class ResilienceException(ResilienceTypes type, Exception error)
    : Exception("The action failed through the resilliance", error)
{
    /// <summary>
    /// The type of resilience failure that has occured
    /// </summary>
    public ResilienceTypes Type => type;
}

/// <summary>
/// Thrown when a Ping Attempt fails
/// </summary>
public sealed class PingFailedException(string message)
    : Exception(message)
{ }

/// <summary>
/// Thrown when the type specified in a UseMQContract for the encoder does not match the contract type
/// </summary>
public sealed class InvalidEncoderException
    : Exception
{
    internal InvalidEncoderException(Type contractType)
        : base($"Cannot link an encoder type that does not implement the interface IMessageTypeEncoder<{contractType.Name}>")
    { }
}

/// <summary>
/// Thrown when the type specified in a UseMQContract for the encryptor does not match the contract type
/// </summary>
public sealed class InvalidEncryptorException
    : Exception
{
    internal InvalidEncryptorException(Type contractType)
        : base($"Cannot link an encryptor type that does not implement the interface IMessageTypeEncoder<{contractType.Name}>")
    { }
}

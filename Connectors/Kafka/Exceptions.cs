namespace MQContract.Kafka;

/// <summary>
/// Thrown when a publish message fails to persist in the system
/// </summary>
public class PersistenceFailedException : Exception
{
    internal PersistenceFailedException()
        : base("Persistence Failed") { }
}

/// <summary>
/// Thrown when the service is unable to find a schema for a given message and it is set to fail when missing
/// </summary>
public class MissingSchemaException : Exception
{
    internal MissingSchemaException(string messageType)
        : base($"Unable to load a schema from the registry for the message type {messageType}") { }
}

/// <summary>
/// Thrown when the content of a message fails to validate against the schema
/// </summary>
public class SchemaValidationFailedException : Exception
{
    internal SchemaValidationFailedException(int schemaId, string messageType)
        : base($"The schema with id {schemaId} failed to validate against the message type {messageType}") { }
}

/// <summary>
/// Thrown when the Connection is unable to obtain the Broker MetaData to create a PingResponse
/// </summary>
public class UnableToPingException : Exception
{
    internal UnableToPingException()
        : base("Unable to extract Meta Data from broker to obtain PingResponse") { }
}

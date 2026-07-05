namespace MQContract.Messages;

/// <summary>
/// Represents a transmission message with an optional identifier and header.
/// </summary>
/// <typeparam name="TMessage">The type of the message payload.</typeparam>
/// <param name="Message">The message object to be transmitted</param>
/// <param name="ID">The optional message ID for the message if desired</param>
/// <param name="Header">The optional headers to pass with the message during transmission</param>
public sealed record TransmissionMessage<TMessage>(
    TMessage Message,
    string? ID = null,
    MessageHeader? Header = null
);

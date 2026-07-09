namespace MQContract.Interfaces.Middleware;

/// <summary>
/// Used to provide message information to a context aware middleware from defined Message Contexts (code generated items)
/// </summary>
/// <param name="MessageType">The type of the Message</param>
/// <param name="Channel">The default Channel if defined for the message</param>
/// <param name="MessageTypeID">The expected Message Type ID that should define this message</param>
/// <param name="ResponseChannel">The default Response Channel if defined for the message</param>
/// <param name="ResponseType">The expected Response Type if defined for the message</param>
public readonly record struct MessageContextDefintion(Type MessageType, string? Channel, string MessageTypeID, string? ResponseChannel, Type? ResponseType);

using MQContract.Messages;

namespace MQContract.Interfaces.Middleware;

/// <summary>
/// Represents a decodable message that will run through the middleware
/// </summary>
/// <param name="MessageHeader">The headers supplied with the message</param>
/// <param name="Data">The message data</param>
public readonly record struct DecodableMessage(MessageHeader MessageHeader, ReadOnlyMemory<byte> Data);

/// <summary>
/// Represents a decoded message that will run through the middleware
/// </summary>
/// <typeparam name="TMessage">The type of message it is</typeparam>
/// <param name="MessageHeader">The headers supplied with the message</param>
/// <param name="Message">The decoded message</param>
public readonly record struct DecodedMessage<TMessage>(MessageHeader MessageHeader, TMessage Message);

/// <summary>
/// Represents an encodable message that will run through the middleware
/// </summary>
/// <typeparam name="TMessage">The type of message it is</typeparam>
/// <param name="MessageHeader">THe headers supplied with the message</param>
/// <param name="Message">The message itself</param>
/// <param name="Channel">The channel the message was request to go through</param>
public readonly record struct EncodableMessage<TMessage>(MessageHeader MessageHeader, TMessage Message, string? Channel);

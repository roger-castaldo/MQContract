using MQContract.Messages;

namespace MQContract.CQRS;

/// <summary>
/// Houses the given transmission context for a command or query.  This is used to pass on additional 
/// string properties between calls as well as houses the unique identifiers that can be used to link
/// chained commands/queries
/// </summary>
public sealed record Context
{
    private const string CorrelationIdHeaderKey = "x-mqcontract-cqrs-correlation-id";
    private const string CausationIdHeaderKey = "x-mqcontract-cqrs-causation-id";
    private readonly MessageHeader messageHeader;
    private readonly Dictionary<string, string?> properties = [];

    /// <summary>
    /// The unique identifier for the given message
    /// </summary>
    public Guid MessageId { get; private init; } = Guid.NewGuid();

    /// <summary>
    /// Default constructor
    /// </summary>
    public Context()
    {
        messageHeader = new([
            new KeyValuePair<string,string?>(CorrelationIdHeaderKey, Guid.NewGuid().ToString())
       ]);
    }

    internal Context(MessageHeader messageHeader, Guid messageID)
    {
        this.messageHeader = messageHeader;
        MessageId = messageID;
    }

    /// <summary>
    /// Used to add/remove values in the context
    /// </summary>
    /// <param name="key">The key for the value to access</param>
    /// <returns>The value that is currently assigned to the provided key or null if missing</returns>
    public string? this[string key]
    {
        get
        {
            if (properties.TryGetValue(key, out var result))
                return result;
            return messageHeader[key];
        }
        set
        {
            properties.Remove(key);
            properties.Add(key, value);
        }
    }

    /// <summary>
    /// The list of the available keys
    /// </summary>
    public IEnumerable<string> Keys
        => properties.Keys
        .Concat(messageHeader.Keys)
        .Where(k => !Equals(k, CorrelationIdHeaderKey) && !Equals(k, CausationIdHeaderKey));

    /// <summary>
    /// The unique identifier for the given message chain
    /// </summary>
    public Guid CorrelationId => Guid.Parse(this[CorrelationIdHeaderKey]!);
    internal Guid? CausationId => (string.IsNullOrWhiteSpace(this[CausationIdHeaderKey]) ? null : Guid.Parse(this[CausationIdHeaderKey]!));

    internal Context CloneToChild()
        => new(new MessageHeader(messageHeader, 
                properties
                    .AsEnumerable()
                    .Where(pair => !Equals(pair.Key, CausationIdHeaderKey))
                    .Append(new(CausationIdHeaderKey,MessageId.ToString()))
        ), Guid.NewGuid());

    internal MessageHeader AsMessageHeader()
        => new(messageHeader, properties);

    internal IEnumerable<KeyValuePair<string, string?>> AsEnumerable()
        => AsMessageHeader().Select(pair => new KeyValuePair<string, string?>(pair.Key, pair.Value));
    
}

using MQContract.Interfaces.Middleware;
using System.Diagnostics;

namespace MQContract.Middleware;

internal class Context : IContext
{
    private const string MapTypeKey = "_MapType";
    private const string MaxMessageSizeKey = "_MaxMessageSize";
    private readonly Dictionary<string, object> values = [];

    public Context(ChannelMapper.MapTypes mapDirection, Activity? activity, uint? maxMessageSize = null, Type? expectedType = null)
    {
        this[MapTypeKey] = mapDirection;
        Activity = activity;
        this[MaxMessageSizeKey] = maxMessageSize??int.MaxValue;
        this[EncryptionMiddleware.ExpectedTypeKey] = expectedType;
    }

    public object? this[string key]
    {
        get => values.TryGetValue(key, out var value) ? value : null;
        set
        {
            if (value==null)
                values.Remove(key);
            else
                values.TryAdd(key, value);
        }
    }

    public Activity? Activity { get; private init; }

    public uint MaxMessageSize
        => (uint)this[MaxMessageSizeKey]!;

    public ChannelMapper.MapTypes MapDirection
        => (ChannelMapper.MapTypes)this[MapTypeKey]!;

}

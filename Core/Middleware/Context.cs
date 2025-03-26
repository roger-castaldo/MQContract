using MQContract.Interfaces.Middleware;
using System.Diagnostics;

namespace MQContract.Middleware
{
    internal class Context : IContext
    {
        private const string MapTypeKey = "_MapType";
        private readonly Dictionary<string, object> values = [];

        public Context(ChannelMapper.MapTypes mapDirection, Activity? activity)
        {
            this[MapTypeKey] = mapDirection;
            Activity = activity;
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

        public ChannelMapper.MapTypes MapDirection
            => (ChannelMapper.MapTypes)this[MapTypeKey]!;
    }
}

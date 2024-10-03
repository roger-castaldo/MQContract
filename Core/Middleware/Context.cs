using MQContract.Interfaces.Middleware;

namespace MQContract.Middleware
{
    internal class Context : IContext
    {
        private const string MapTypeKey = "_MapType";
        private readonly Dictionary<string, object> values = [];

        public Context(ChannelMapper.MapTypes mapDirection)
        {
            this[MapTypeKey] = mapDirection;
        }

        public object? this[string key] {
            get => values.TryGetValue(key, out var value) ? value : null;
            set
            {
                if (value==null)
                    values.Remove(key);
                else
                    values.TryAdd(key, value);
            }
        }

        public ChannelMapper.MapTypes MapDirection
            => (ChannelMapper.MapTypes)this[MapTypeKey]!;
    }
}

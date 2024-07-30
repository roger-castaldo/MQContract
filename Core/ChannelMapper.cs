namespace MQContract
{
    public class ChannelMapper
    {
        internal enum MapTypes
        {
            Publish,
            PublishSubscription,
            Query,
            QuerySubscription
        }

        private sealed record ChannelMap(MapTypes Type,Func<string,bool> IsMatch,Func<string,Task<string>> Change);

        private readonly List<ChannelMap> channelMaps = [];

        private ChannelMapper Append(MapTypes type, string originalChannel, string newChannel)
            => Append(type, (key) => Equals(key, originalChannel), (key) => Task.FromResult<string>(newChannel));

        private ChannelMapper Append(MapTypes type, string originalChannel, Func<string, Task<string>> change)
            => Append(type, (key) => Equals(key, originalChannel), change);

        private ChannelMapper Append(MapTypes type, Func<string, Task<string>> change)
            => Append(type, (key) => true, change);

        private ChannelMapper Append(MapTypes type,Func<string,bool> isMatch,Func<string,Task<string>> change)
        {
            channelMaps.Add(new(type, isMatch, change));
            return this;
        }

        public ChannelMapper AddPublishMap(string originalChannel,string newChannel)
            => Append(MapTypes.Publish,originalChannel,newChannel);

        public ChannelMapper AddPublishMap(string originalChannel, Func<string,Task<string>> mapFunction)
            => Append(MapTypes.Publish, originalChannel, mapFunction);

        public ChannelMapper AddPublishMap(Func<string,bool> isMatch, Func<string, Task<string>> mapFunction)
            => Append(MapTypes.Publish, isMatch, mapFunction);

        public ChannelMapper AddDefaultPublishMap(Func<string, Task<string>> mapFunction)
            => Append(MapTypes.Publish, mapFunction);

        public ChannelMapper AddPublishSubscriptionMap(string originalChannel, string newChannel)
            => Append(MapTypes.PublishSubscription, originalChannel, newChannel);

        public ChannelMapper AddPublishSubscriptionMap(string originalChannel, Func<string, Task<string>> mapFunction)
            => Append(MapTypes.PublishSubscription, originalChannel, mapFunction);

        public ChannelMapper AddPublishSubscriptionMap(Func<string, bool> isMatch, Func<string, Task<string>> mapFunction)
            => Append(MapTypes.PublishSubscription, isMatch, mapFunction);

        public ChannelMapper AddDefaultPublishSubscriptionMap(Func<string, Task<string>> mapFunction)
            => Append(MapTypes.PublishSubscription, mapFunction);

        public ChannelMapper AddQueryMap(string originalChannel, string newChannel)
            => Append(MapTypes.Query, originalChannel, newChannel);

        public ChannelMapper AddQueryMap(string originalChannel, Func<string, Task<string>> mapFunction)
            => Append(MapTypes.Query, originalChannel, mapFunction);

        public ChannelMapper AddQueryMap(Func<string, bool> isMatch, Func<string, Task<string>> mapFunction)
            => Append(MapTypes.Query, isMatch, mapFunction);

        public ChannelMapper AddDefaultQueryMap(Func<string, Task<string>> mapFunction)
            => Append(MapTypes.Query, mapFunction);

        public ChannelMapper AddQuerySubscriptionMap(string originalChannel, string newChannel)
            => Append(MapTypes.QuerySubscription, originalChannel, newChannel);

        public ChannelMapper AddQuerySubscriptionMap(string originalChannel, Func<string, Task<string>> mapFunction)
            => Append(MapTypes.QuerySubscription, originalChannel, mapFunction);

        public ChannelMapper AddQuerySubscriptionMap(Func<string, bool> isMatch, Func<string, Task<string>> mapFunction)
            => Append(MapTypes.QuerySubscription, isMatch, mapFunction);

        public ChannelMapper AddDefaultQuerySubscriptionMap(Func<string, Task<string>> mapFunction)
            => Append(MapTypes.QuerySubscription, mapFunction);

        internal Task<string> MapChannel(MapTypes mapType,string originalChannel)
        {
            var map = channelMaps.Find(m=>Equals(m.Type,mapType) && m.IsMatch(originalChannel));
            return map?.Change(originalChannel)??Task.FromResult<string>(originalChannel);
        }
    }
}

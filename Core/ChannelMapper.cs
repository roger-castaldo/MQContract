namespace MQContract
{
    /// <summary>
    /// Used to map channel names depending on the usage of the channel when necessary
    /// </summary>
    public sealed class ChannelMapper
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

        /// <summary>
        /// Add a direct map for publish calls
        /// </summary>
        /// <param name="originalChannel">The original channel that is being used in the connection</param>
        /// <param name="newChannel">The channel to map it to</param>
        /// <returns>The current instance of the Channel Mapper</returns>
        public ChannelMapper AddPublishMap(string originalChannel,string newChannel)
            => Append(MapTypes.Publish,originalChannel,newChannel);

        /// <summary>
        /// Add a map function for publish calls for a given channel
        /// </summary>
        /// <param name="originalChannel">The original channel that is being used in the connection</param>
        /// <param name="mapFunction">A function to be called with the channel supplied expecting a mapped channel name</param>
        /// <returns>The current instance of the Channel Mapper</returns>
        public ChannelMapper AddPublishMap(string originalChannel, Func<string,Task<string>> mapFunction)
            => Append(MapTypes.Publish, originalChannel, mapFunction);

        /// <summary>
        /// Add a map function call pair for publish calls
        /// </summary>
        /// <param name="isMatch">A callback that will return true if the supplied function will mape that channel</param>
        /// <param name="mapFunction">A function to be called with the channel supplied expecting a mapped channel name</param>
        /// <returns>The current instance of the Channel Mapper</returns>
        public ChannelMapper AddPublishMap(Func<string,bool> isMatch, Func<string, Task<string>> mapFunction)
            => Append(MapTypes.Publish, isMatch, mapFunction);
        /// <summary>
        /// Add a default map function to call for publish calls
        /// </summary>
        /// <param name="mapFunction">A function to be called with the channel supplied expecting a mapped channel name</param>
        /// <returns>The current instance of the Channel Mapper</returns>
        public ChannelMapper AddDefaultPublishMap(Func<string, Task<string>> mapFunction)
            => Append(MapTypes.Publish, mapFunction);
        /// <summary>
        /// Add a direct map for pub/sub subscription calls
        /// </summary>
        /// <param name="originalChannel">The original channel that is being used in the connection</param>
        /// <param name="newChannel">The channel to map it to</param>
        /// <returns>The current instance of the Channel Mapper</returns>
        public ChannelMapper AddPublishSubscriptionMap(string originalChannel, string newChannel)
            => Append(MapTypes.PublishSubscription, originalChannel, newChannel);
        /// <summary>
        /// Add a map function for pub/sub subscription calls
        /// </summary>
        /// <param name="originalChannel">The original channel that is being used in the connection</param>
        /// <param name="mapFunction">A function to be called with the channel supplied expecting a mapped channel name</param>
        /// <returns>The current instance of the Channel Mapper</returns>
        public ChannelMapper AddPublishSubscriptionMap(string originalChannel, Func<string, Task<string>> mapFunction)
            => Append(MapTypes.PublishSubscription, originalChannel, mapFunction);
        /// <summary>
        /// Add a map function call pair for pub/sub subscription calls
        /// </summary>
        /// <param name="isMatch">A callback that will return true if the supplied function will mape that channel</param>
        /// <param name="mapFunction">A function to be called with the channel supplied expecting a mapped channel name</param>
        /// <returns>The current instance of the Channel Mapper</returns>
        public ChannelMapper AddPublishSubscriptionMap(Func<string, bool> isMatch, Func<string, Task<string>> mapFunction)
            => Append(MapTypes.PublishSubscription, isMatch, mapFunction);
        /// <summary>
        /// Add a default map function to call for pub/sub subscription calls
        /// </summary>
        /// <param name="mapFunction">A function to be called with the channel supplied expecting a mapped channel name</param>
        /// <returns>The current instance of the Channel Mapper</returns>
        public ChannelMapper AddDefaultPublishSubscriptionMap(Func<string, Task<string>> mapFunction)
            => Append(MapTypes.PublishSubscription, mapFunction);
        /// <summary>
        /// Add a direct map for query calls
        /// </summary>
        /// <param name="originalChannel">The original channel that is being used in the connection</param>
        /// <param name="newChannel">The channel to map it to</param>
        /// <returns>The current instance of the Channel Mapper</returns>
        public ChannelMapper AddQueryMap(string originalChannel, string newChannel)
            => Append(MapTypes.Query, originalChannel, newChannel);
        /// <summary>
        /// Add a map function for query calls for a given channel
        /// </summary>
        /// <param name="originalChannel">The original channel that is being used in the connection</param>
        /// <param name="mapFunction">A function to be called with the channel supplied expecting a mapped channel name</param>
        /// <returns>The current instance of the Channel Mapper</returns>
        public ChannelMapper AddQueryMap(string originalChannel, Func<string, Task<string>> mapFunction)
            => Append(MapTypes.Query, originalChannel, mapFunction);
        /// <summary>
        /// Add a map function call pair for query calls
        /// </summary>
        /// <param name="isMatch">A callback that will return true if the supplied function will mape that channel</param>
        /// <param name="mapFunction">A function to be called with the channel supplied expecting a mapped channel name</param>
        /// <returns>The current instance of the Channel Mapper</returns>
        public ChannelMapper AddQueryMap(Func<string, bool> isMatch, Func<string, Task<string>> mapFunction)
            => Append(MapTypes.Query, isMatch, mapFunction);
        /// <summary>
        /// Add a default map function to call for query calls
        /// </summary>
        /// <param name="mapFunction">A function to be called with the channel supplied expecting a mapped channel name</param>
        /// <returns>The current instance of the Channel Mapper</returns>
        public ChannelMapper AddDefaultQueryMap(Func<string, Task<string>> mapFunction)
            => Append(MapTypes.Query, mapFunction);
        /// <summary>
        /// Add a direct map for query/response subscription calls
        /// </summary>
        /// <param name="originalChannel">The original channel that is being used in the connection</param>
        /// <param name="newChannel">The channel to map it to</param>
        /// <returns>The current instance of the Channel Mapper</returns>
        public ChannelMapper AddQuerySubscriptionMap(string originalChannel, string newChannel)
            => Append(MapTypes.QuerySubscription, originalChannel, newChannel);
        /// <summary>
        /// Add a map function for query/response subscription calls
        /// </summary>
        /// <param name="originalChannel">The original channel that is being used in the connection</param>
        /// <param name="mapFunction">A function to be called with the channel supplied expecting a mapped channel name</param>
        /// <returns>The current instance of the Channel Mapper</returns>
        public ChannelMapper AddQuerySubscriptionMap(string originalChannel, Func<string, Task<string>> mapFunction)
            => Append(MapTypes.QuerySubscription, originalChannel, mapFunction);
        /// <summary>
        /// Add a map function call pair for query/response subscription calls
        /// </summary>
        /// <param name="isMatch">A callback that will return true if the supplied function will mape that channel</param>
        /// <param name="mapFunction">A function to be called with the channel supplied expecting a mapped channel name</param>
        /// <returns>The current instance of the Channel Mapper</returns>
        public ChannelMapper AddQuerySubscriptionMap(Func<string, bool> isMatch, Func<string, Task<string>> mapFunction)
            => Append(MapTypes.QuerySubscription, isMatch, mapFunction);
        /// <summary>
        /// Add a default map function to call for query/response subscription calls
        /// </summary>
        /// <param name="mapFunction">A function to be called with the channel supplied expecting a mapped channel name</param>
        /// <returns>The current instance of the Channel Mapper</returns>
        public ChannelMapper AddDefaultQuerySubscriptionMap(Func<string, Task<string>> mapFunction)
            => Append(MapTypes.QuerySubscription, mapFunction);

        internal Task<string> MapChannel(MapTypes mapType,string originalChannel)
        {
            var map = channelMaps.Find(m=>Equals(m.Type,mapType) && m.IsMatch(originalChannel));
            return map?.Change(originalChannel)??Task.FromResult<string>(originalChannel);
        }
    }
}

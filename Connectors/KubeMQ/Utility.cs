namespace MQContract.KubeMQ
{
    internal static class Utility
    {
        internal static long ToUnixTime(DateTime timestamp)
        {
            return new DateTimeOffset(timestamp).ToUniversalTime().ToUnixTimeSeconds();
        }

        internal static DateTime FromUnixTime(long timestamp)
        {
            try
            {
                return DateTimeOffset.FromUnixTimeSeconds(timestamp).DateTime.ToLocalTime();
            }
            catch (Exception)
            {
                try
                {
                    return DateTimeOffset.FromUnixTimeMilliseconds(timestamp/1000000).DateTime.ToLocalTime();
                }
                catch (Exception)
                {
                    return DateTime.MaxValue;
                }
            }
        }
    }
}

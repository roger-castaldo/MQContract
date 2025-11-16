using MQContract;
using MQContract.InMemory;
using MQContract.Interfaces;
using System.Security.Cryptography;

namespace CQRSTesting
{
    internal static class Helper
    {
        private const string ValidCharacters = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz1234567890";

        public static string GenerateRandomString(int length)
            => RandomNumberGenerator.GetString(ValidCharacters, length);

        public static string GenerateRandomString()
            => RandomNumberGenerator.GetString(ValidCharacters, RandomNumberGenerator.GetInt32(5, 250));
        public static IContractConnection ProduceConnection()
            =>ContractConnection.Instance(new Connection());

        private static readonly TimeSpan Delay = TimeSpan.FromMilliseconds(5);

        public static async Task<bool> WaitForCount<T>(IEnumerable<T> values, int count, TimeSpan maxTime)
            where T : class
        {
            var task = new Task(() =>
            {
                while (values.Count()<count)
                    Task.Delay(Delay).Wait();
            });
            task.Start();
            return (await Task.WhenAny(task, Task.Delay(maxTime))) == task || values.Count()>=count;
        }
    }
}

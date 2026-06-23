namespace ConnectorTesting.Helpers;

internal static class TestHelper
{
    private static readonly TimeSpan Delay = TimeSpan.FromMilliseconds(100);

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

    const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

    public static string RandomString(int length)
    {
        var random = new Random();
        return new string(Enumerable.Repeat(chars, length)
            .Select(s => s[random.Next(s.Length)]).ToArray());
    }
}

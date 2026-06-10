using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
}

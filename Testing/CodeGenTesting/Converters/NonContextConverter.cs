using CodeGenTesting.Messages;
using MQContract.Interfaces.Conversion;

namespace CodeGenTesting.Converters
{
    internal class NonContextConverter : IMessageConverter<Announcement, NonContextMessage>
    {
        ValueTask<NonContextMessage> IMessageConverter<Announcement, NonContextMessage>.ConvertAsync(Announcement source)
            => throw new NotImplementedException();
    }
}

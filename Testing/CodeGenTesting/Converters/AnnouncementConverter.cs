using CodeGenTesting.Messages;
using Microsoft.Extensions.DependencyInjection;
using MQContract.Interfaces.Conversion;

namespace CodeGenTesting.Converters
{
    internal class AnnouncementConverter : IMessageConverter<Announcement, PartyAnnouncement>,
        IMessageConverter<PartyAnnouncement, DirectAnnouncement>
    {
        public AnnouncementConverter() { }

        [ActivatorUtilitiesConstructor]
        public AnnouncementConverter(IServiceInjection serviceInjection)
        {
            ArgumentNullException.ThrowIfNull(serviceInjection, nameof(serviceInjection));
        }

        ValueTask<PartyAnnouncement> IMessageConverter<Announcement, PartyAnnouncement>.ConvertAsync(Announcement source)
            => ValueTask.FromResult(new PartyAnnouncement(source.Message, null));

        ValueTask<DirectAnnouncement> IMessageConverter<PartyAnnouncement, DirectAnnouncement>.ConvertAsync(PartyAnnouncement source)
            => ValueTask.FromResult(new DirectAnnouncement(source.Message, source.From, null));
    }
}

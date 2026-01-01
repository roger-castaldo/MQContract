using CodeGenTesting.Messages;
using MQContract.Interfaces.Conversion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeGenTesting.Converters
{
    internal class AnnouncementConverter : IMessageConverter<Announcement, PartyAnnouncement>,
        IMessageConverter<PartyAnnouncement, DirectAnnouncement>
    {
        ValueTask<PartyAnnouncement> IMessageConverter<Announcement, PartyAnnouncement>.ConvertAsync(Announcement source)
            => ValueTask.FromResult(new PartyAnnouncement(source.Message, null));

        ValueTask<DirectAnnouncement> IMessageConverter<PartyAnnouncement, DirectAnnouncement>.ConvertAsync(PartyAnnouncement source)
            => ValueTask.FromResult(new DirectAnnouncement(source.Message, source.From, null));
    }
}

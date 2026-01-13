using CodeGenTesting.Messages;
using Microsoft.Extensions.DependencyInjection;
using MQContract.Interfaces.Encoding;
using System.Text;

namespace CodeGenTesting.Encoders
{
    internal class DirectAnnouncementEncoder : IMessageTypeEncoder<DirectAnnouncement>
    {
        public DirectAnnouncementEncoder() { }

        [ActivatorUtilitiesConstructor]
        public DirectAnnouncementEncoder(IServiceInjection serviceInjection)
        {
            ArgumentNullException.ThrowIfNull(serviceInjection, nameof(serviceInjection));
        }

        ValueTask<DirectAnnouncement?> IMessageTypeEncoder<DirectAnnouncement>.DecodeAsync(Stream stream)
        {
            var br = new BinaryReader(stream);
            var message = UTF8Encoding.UTF8.GetString(br.ReadBytes(br.ReadInt32()));
            string? from = null;
            var len = br.ReadInt32();
            if (len>0)
                from = UTF8Encoding.UTF8.GetString(br.ReadBytes(len));
            string? to = null;
            len = br.ReadInt32();
            if (len>0)
                to= UTF8Encoding.UTF8.GetString(br.ReadBytes(len));
            return ValueTask.FromResult<DirectAnnouncement?>(new(message, from, to));
        }

        ValueTask<byte[]> IMessageTypeEncoder<DirectAnnouncement>.EncodeAsync(DirectAnnouncement message)
        {
            using var ms = new MemoryStream();
            var bw = new BinaryWriter(ms);
            var bytes = UTF8Encoding.UTF8.GetBytes(message.Message);
            bw.Write(bytes.Length);
            bw.Write(bytes);
            if (message.From != null)
            {
                bytes = UTF8Encoding.UTF8.GetBytes(message.From);
                bw.Write(bytes.Length);
                bw.Write(bytes);
            }
            else
                bw.Write((int)0);
            if (message.To != null)
            {
                bytes = UTF8Encoding.UTF8.GetBytes(message.To);
                bw.Write(bytes.Length);
                bw.Write(bytes);
            }
            else
                bw.Write((int)0);


            bw.Flush();
            return ValueTask.FromResult(ms.ToArray());
        }
    }
}

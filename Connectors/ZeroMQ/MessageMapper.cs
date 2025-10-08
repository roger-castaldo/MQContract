using MQContract.Messages;
using System.Text;

namespace MQContract.ZeroMQ
{
    internal static class MessageMapper
    {
        private static void WriteString(string? value,BinaryWriter binaryWriter)
        {
            if (value == null)
                binaryWriter.Write(0);
            else
            {
                var data = UTF8Encoding.UTF8.GetBytes(value);
                binaryWriter.Write(data.Length);
                binaryWriter.Write(data);
            }
        }

        private static string? ReadString(BinaryReader br)
        {
            var len = br.ReadInt32();
            return (len==0 ? null : UTF8Encoding.UTF8.GetString(br.ReadBytes(len)));
        }
        public static byte[] Map(ServiceMessage message, Guid? correlationId = null, string? inboxAddress = null, string? channelOverride = null)
        {
            using var ms = new MemoryStream();
            using var bw = new BinaryWriter(ms);
            WriteString(message.ID, bw);
            WriteString(channelOverride??message.Channel, bw);
            WriteString(message.MessageTypeID, bw);
            if (correlationId==null)
                WriteString(null, bw);
            else
            {

                WriteString(inboxAddress,bw);
                bw.Write(correlationId.Value.ToByteArray());
            }
            bw.Write(message.Header.Keys.Count());
            foreach (var key in message.Header.Keys)
            {
                WriteString(key, bw);
                WriteString(message.Header[key], bw);
            }
            bw.Write(message.Data.Length);
            bw.Write(message.Data.ToArray());
            return ms.ToArray();
        }

        public static (ReceivedInboxServiceMessage recievedMessage,string? inboxAddress) Map(byte[] data)
        {
            using var ms = new MemoryStream(data);
            using var br = new BinaryReader(ms);
            var id = ReadString(br)!;
            var channel = ReadString(br)!;
            var messageTypeID = ReadString(br)!;
            var inboxAddress = ReadString(br);
            Guid? correlationId = null;
            if (!string.IsNullOrWhiteSpace(inboxAddress))
                correlationId = new Guid(br.ReadBytes(16));
            var headLength = br.ReadInt32();
            var headers = new Dictionary<string, string?>();
            while (headLength>headers.Count)
                headers.Add(ReadString(br)!,ReadString(br));
            var messageContent = br.ReadBytes(br.ReadInt32());
            return (
                new ReceivedInboxServiceMessage(id,messageTypeID,channel,new(headers),correlationId??Guid.Empty,messageContent),
                inboxAddress
            );
        }
    }
}

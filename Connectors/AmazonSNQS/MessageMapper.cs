using Amazon.SimpleNotificationService.Model;
using Amazon.SQS.Model;
using MQContract.Messages;
using System.Text;
using System.Text.Json;

namespace MQContract.AmazonSNQS
{
    internal static class MessageMapper
    {
        private static void WriteString(string? value, BinaryWriter binaryWriter)
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

        private static string ServiceMessageToString(ServiceMessage message)
        {
            using var ms = new MemoryStream();
            using var bw = new BinaryWriter(ms);
            WriteString(message.ID, bw);
            WriteString(message.MessageTypeID, bw);
            WriteString(message.Channel, bw);
            bw.Write(message.Header.Count);
            message.Header.ForEach(pair =>
            {
                WriteString(pair.Key, bw);
                WriteString(pair.Value, bw);
            });
            bw.Write(message.Data.Length);
            bw.Write(message.Data.ToArray());
            bw.Flush();
            return Convert.ToBase64String(ms.ToArray());
        }

        public static PublishRequest Map(ServiceMessage message, Topic topic)
            => new()
            {
                TopicArn=topic.TopicArn,
                Message=ServiceMessageToString(message)
            };

        public static SendMessageRequest Map(ServiceMessage message, string queueURL)
            => new(queueURL, ServiceMessageToString(message));

        public static ReceivedServiceMessage Map(Amazon.SQS.Model.Message message, Func<ValueTask> acknowledge)
        {
            var doc = JsonDocument.Parse(message.Body);
            if (!doc.RootElement.TryGetProperty("Message", out var contentElement))
                throw new InvalidQueueMessageException();
            using var ms = new MemoryStream(Convert.FromBase64String(contentElement.GetString()!), false);
            using var br = new BinaryReader(ms);
            var messageID = ReadString(br)!;
            var messageTypeID = ReadString(br)!;
            var channel = ReadString(br)!;
            var headLength = br.ReadInt32();
            var headers = new Dictionary<string, string?>();
            while (headLength>headers.Count)
                headers.Add(ReadString(br)!, ReadString(br));
            var messageContent = br.ReadBytes(br.ReadInt32());
            return new(messageID, messageTypeID, channel, new(headers), messageContent, acknowledge);
        }
    }
}

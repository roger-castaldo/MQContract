using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MQContract
{
    internal static class Constants
    {
        private const string BaseActivityName = "MQContract";
        public const string PublishActivityName = $"{BaseActivityName}.PublishMessage";
        public const string PublishQueryActivityName = $"{BaseActivityName}.PublishQueryMessage";
        public const string BulkPublishActivityName = $"{BaseActivityName}.BulkPublishMessages";
        public const string ConsumeActivityName = $"{BaseActivityName}.ConsumeMessage";
        public const string ConsumeQueryActivityName = $"{BaseActivityName}.ConsumeQueryMessage";
        public const string ProduceQueryResponseActivityName = $"{BaseActivityName}.ProduceQueryResponse";
        public const string ConsumeQueryResponseActivityName = $"{BaseActivityName}.ConsumeQueryResponse";
        public const string PublishBulkMessagesMessageEvent = $"{BulkPublishActivityName}.MessagePublished";

        public const string BulkPublishCountTag = "mqcontract.bulkmessagecount";

    }
}

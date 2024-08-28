using Moq;
using MQContract.Interfaces.Service;
using MQContract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutomatedTesting.Messages
{
    [TestClass]
    public class MessageHeaderTests
    {
        [TestMethod]
        public void TestMessageHeaderPrimaryConstructor()
        {
            #region Arrange
            IEnumerable<KeyValuePair<string, string>> data = [
                new KeyValuePair<string,string>("key1","value1"),
                new KeyValuePair<string,string>("key2","value2")
            ];
            #endregion

            #region Act
            var header = new MessageHeader(data);
            #endregion

            #region Assert
            Assert.AreEqual(2, header.Keys.Count());
            Assert.IsTrue(data.All(pair=>header.Keys.Contains(pair.Key) && Equals(header[pair.Key],pair.Value)));
            #endregion

            #region Verify
            #endregion
        }

        [TestMethod]
        public void TestMessageHeaderDictionaryConstructor()
        {
            #region Arrange
            var data = new Dictionary<string,string>([
                new KeyValuePair<string,string>("key1","value1"),
                new KeyValuePair<string,string>("key2","value2")
            ]);
            #endregion

            #region Act
            var header = new MessageHeader(data);
            #endregion

            #region Assert
            Assert.AreEqual(2, header.Keys.Count());
            Assert.IsTrue(data.All(pair => header.Keys.Contains(pair.Key) && Equals(header[pair.Key], pair.Value)));
            #endregion

            #region Verify
            #endregion
        }

        [TestMethod]
        public void TestMessageHeaderMergeConstructorWithOriginalAndExtension()
        {
            #region Arrange
            var originalHeader = new MessageHeader([
                new KeyValuePair<string,string>("key1","value1"),
                new KeyValuePair<string,string>("key2","value2")
            ]);
            var data = new Dictionary<string,string?>([
                new KeyValuePair<string,string?>("key3","value3"),
                new KeyValuePair<string,string?>("key4","value4")
            ]);
            #endregion

            #region Act
            var header = new MessageHeader(originalHeader,data);
            #endregion

            #region Assert
            Assert.AreEqual(4, header.Keys.Count());
            Assert.IsTrue(originalHeader.Keys.All(k => header.Keys.Contains(k) && Equals(header[k], originalHeader[k])));
            Assert.IsTrue(data.All(pair => header.Keys.Contains(pair.Key) && Equals(header[pair.Key], pair.Value)));
            #endregion

            #region Verify
            #endregion
        }

        [TestMethod]
        public void TestMessageHeaderMergeConstructorWithOriginalAndNullExtension()
        {
            #region Arrange
            var originalHeader = new MessageHeader([
                new KeyValuePair<string,string>("key1","value1"),
                new KeyValuePair<string,string>("key2","value2")
            ]);
            #endregion

            #region Act
            var header = new MessageHeader(originalHeader, null);
            #endregion

            #region Assert
            Assert.AreEqual(2, header.Keys.Count());
            Assert.IsTrue(originalHeader.Keys.All(k => header.Keys.Contains(k) && Equals(header[k], originalHeader[k])));
            #endregion

            #region Verify
            #endregion
        }

        [TestMethod]
        public void TestMessageHeaderMergeConstructorWithNullOriginalAndExtension()
        {
            #region Arrange
            var data = new Dictionary<string, string?>([
                new KeyValuePair<string,string?>("key3","value3"),
                new KeyValuePair<string,string?>("key4","value4")
            ]);
            #endregion

            #region Act
            var header = new MessageHeader(null, data);
            #endregion

            #region Assert
            Assert.AreEqual(2, header.Keys.Count());
            Assert.IsTrue(data.All(pair => header.Keys.Contains(pair.Key) && Equals(header[pair.Key], pair.Value)));
            #endregion

            #region Verify
            #endregion
        }

        [TestMethod]
        public void TestMessageHeaderMergeConstructorWithOriginalAndExtensionWithSameKeys()
        {
            #region Arrange
            var originalHeader = new MessageHeader([
                new KeyValuePair<string,string>("key1","value1"),
                new KeyValuePair<string,string>("key2","value2")
            ]);
            var data = new Dictionary<string, string?>([
                new KeyValuePair<string,string?>("key1","value3"),
                new KeyValuePair<string,string?>("key2","value4")
            ]);
            #endregion

            #region Act
            var header = new MessageHeader(originalHeader, data);
            #endregion

            #region Assert
            Assert.AreEqual(2, header.Keys.Count());
            Assert.IsTrue(originalHeader.Keys.All(k => header.Keys.Contains(k) && !Equals(header[k], originalHeader[k])));
            Assert.IsTrue(data.All(pair => header.Keys.Contains(pair.Key) && Equals(header[pair.Key], pair.Value)));
            #endregion

            #region Verify
            #endregion
        }
    }
}

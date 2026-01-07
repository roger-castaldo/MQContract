using Castle.Core.Internal;
using CodeGenTesting.Encoders;
using CodeGenTesting.Encryptors;
using CodeGenTesting.Messages;
using Moq;
using MQContract;
using MQContract.Attributes;
using MQContract.Interfaces.Encoding;
using MQContract.Interfaces.Encrypting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeGenTesting
{
    [TestClass]
    public class ContextTests
    {
        [TestMethod]
        public void CheckIsMessageCodeGenerated()
        {
            //Arrange
            var context = new MyMessageContext();
            //Act

            //Assert
            Assert.IsTrue(context.IsMessageCodeGenerated<Announcement>());
            Assert.IsTrue(context.IsMessageCodeGenerated<PartyAnnouncement>());
            Assert.IsTrue(context.IsMessageCodeGenerated<DirectAnnouncement>());
            Assert.IsTrue(context.IsMessageCodeGenerated<Prompt>());
            Assert.IsFalse(context.IsMessageCodeGenerated<NonContextMessage>());
            //Verify
        }

        [TestMethod]
        public void CheckMessageType()
        {
            //Arrange
            var context = new MyMessageContext();
            //Act

            //Assert
            ValidateMessageType<Announcement>(context.TryGetMessageType(typeof(Announcement)));
            ValidateMessageType<PartyAnnouncement>(context.TryGetMessageType(typeof(PartyAnnouncement)));
            ValidateMessageType<DirectAnnouncement>(context.TryGetMessageType(typeof(DirectAnnouncement)));
            ValidateMessageType<Prompt>(context.TryGetMessageType(typeof(Prompt)));
            Assert.IsNull(context.TryGetMessageType(typeof(NonContextMessage)));
            //Verify
        }

        private static void ValidateMessageType<TMessage>(MQContractMessageContext.MessageTypeDefinition? definition)
        {
            Assert.IsNotNull(definition);
            var messageAtt = typeof(TMessage).GetAttribute<MessageAttribute>();
            Assert.AreEqual(messageAtt?.Channel, definition?.Channel);
            Assert.AreEqual(messageAtt?.TypeName??typeof(TMessage).Name, definition?.TypeName);
            Assert.AreEqual(messageAtt?.TypeVersion.ToString()??"0.0.0.0", definition?.TypeVersion.ToString());
            var queryAtt = typeof(TMessage).GetAttribute<QueryMessageAttribute>();
            if (queryAtt!=null)
            {
                Assert.AreEqual(queryAtt?.ResponseChannel, definition?.ResponseChannel);
                Assert.AreEqual(queryAtt?.ResponseTimeout, definition?.ResponseTimeout);
                Assert.AreEqual(queryAtt?.ResponseType, definition?.ResponseType);
            }
            else
            {
                Assert.IsNull(definition?.ResponseChannel);
                Assert.IsNull(definition?.ResponseTimeout);
                Assert.IsNull(definition?.ResponseType);
            }
        }

        [TestMethod]
        public void CheckMessageEncoder()
        {
            //Arrange
            var context = new MyMessageContext();
            var mockMessageEncoder = new Mock<IMessageEncoder>();
            var mockServiceProvider = MockServiceProvider.Instance;
            //Act

            //Assert
            Assert.AreEqual("CodeGenTesting.MyMessageContext+DefaultJsonEncoder`1[CodeGenTesting.Messages.Announcement]", context.TryGetMessageEncoder<Announcement>(null, null)?.GetType().ToString());
            Assert.AreEqual(mockMessageEncoder.Object, context.TryGetMessageEncoder<Announcement>(mockMessageEncoder.Object, null));
            Assert.AreEqual(mockMessageEncoder.Object, context.TryGetMessageEncoder<Announcement>(mockMessageEncoder.Object, mockServiceProvider.Object));

            Assert.IsInstanceOfType(context.TryGetMessageEncoder<PartyAnnouncement>(null, null), typeof(PartyAnnouncementEncoder));
            Assert.IsInstanceOfType(context.TryGetMessageEncoder<PartyAnnouncement>(mockMessageEncoder.Object, null), typeof(PartyAnnouncementEncoder));
            Assert.IsInstanceOfType(context.TryGetMessageEncoder<PartyAnnouncement>(mockMessageEncoder.Object, mockServiceProvider.Object), typeof(PartyAnnouncementEncoder));

            Assert.IsInstanceOfType(context.TryGetMessageEncoder<DirectAnnouncement>(null, null), typeof(DirectAnnouncementEncoder));
            Assert.IsInstanceOfType(context.TryGetMessageEncoder<DirectAnnouncement>(mockMessageEncoder.Object, null), typeof(DirectAnnouncementEncoder));
            Assert.IsInstanceOfType(context.TryGetMessageEncoder<DirectAnnouncement>(mockMessageEncoder.Object, mockServiceProvider.Object), typeof(DirectAnnouncementEncoder));

            Assert.AreEqual("CodeGenTesting.MyMessageContext+DefaultJsonEncoder`1[CodeGenTesting.Messages.Prompt]", context.TryGetMessageEncoder<Prompt>(null, null)?.GetType().ToString());
            Assert.AreEqual(mockMessageEncoder.Object, context.TryGetMessageEncoder<Prompt>(mockMessageEncoder.Object, null));
            Assert.AreEqual(mockMessageEncoder.Object, context.TryGetMessageEncoder<Prompt>(mockMessageEncoder.Object, mockServiceProvider.Object));

            Assert.IsNull(context.TryGetMessageEncoder<NonContextMessage>(null, null));
            Assert.IsNull(context.TryGetMessageEncoder<NonContextMessage>(mockMessageEncoder.Object, null));
            Assert.IsNull(context.TryGetMessageEncoder<NonContextMessage>(mockMessageEncoder.Object, mockServiceProvider.Object));
            //Verify
            mockServiceProvider.Verify(x => x.GetService(typeof(IServiceInjection)), Times.Exactly(2));
        }

        [TestMethod]
        public void CheckMessageEncryptor()
        {
            //Arrange
            var context = new MyMessageContext();
            var mockMessageEncryptor = new Mock<IMessageEncryptor>();
            var mockServiceProvider = MockServiceProvider.Instance;
            //Act

            //Assert
            Assert.AreEqual("CodeGenTesting.MyMessageContext+NonEncryptor", context.TryGetMessageEncryptor(typeof(Announcement), null, null)?.GetType().ToString());
            Assert.AreEqual(mockMessageEncryptor.Object, context.TryGetMessageEncryptor(typeof(Announcement), mockMessageEncryptor.Object, null));
            Assert.AreEqual(mockMessageEncryptor.Object, context.TryGetMessageEncryptor(typeof(Announcement), mockMessageEncryptor.Object, mockServiceProvider.Object));

            Assert.AreEqual("CodeGenTesting.MyMessageContext+NonEncryptor",context.TryGetMessageEncryptor(typeof(PartyAnnouncement), null, null)?.GetType().ToString());
            Assert.AreEqual(mockMessageEncryptor.Object, context.TryGetMessageEncryptor(typeof(PartyAnnouncement), mockMessageEncryptor.Object, null));
            Assert.AreEqual(mockMessageEncryptor.Object, context.TryGetMessageEncryptor(typeof(PartyAnnouncement), mockMessageEncryptor.Object, mockServiceProvider.Object));

            Assert.IsInstanceOfType(context.TryGetMessageEncryptor(typeof(DirectAnnouncement), null, null), typeof(DirectAnnouncementEncryptor));
            Assert.IsInstanceOfType(context.TryGetMessageEncryptor(typeof(DirectAnnouncement), mockMessageEncryptor.Object, null), typeof(DirectAnnouncementEncryptor));
            Assert.IsInstanceOfType(context.TryGetMessageEncryptor(typeof(DirectAnnouncement), mockMessageEncryptor.Object, mockServiceProvider.Object), typeof(DirectAnnouncementEncryptor));

            Assert.AreEqual("CodeGenTesting.MyMessageContext+NonEncryptor", context.TryGetMessageEncryptor(typeof(Prompt), null, null)?.GetType().ToString());
            Assert.AreEqual(mockMessageEncryptor.Object, context.TryGetMessageEncryptor(typeof(Prompt), mockMessageEncryptor.Object, null));
            Assert.AreEqual(mockMessageEncryptor.Object, context.TryGetMessageEncryptor(typeof(Prompt), mockMessageEncryptor.Object, mockServiceProvider.Object));

            Assert.IsNull(context.TryGetMessageEncryptor(typeof(NonContextMessage), null, null));
            Assert.IsNull(context.TryGetMessageEncryptor(typeof(NonContextMessage), mockMessageEncryptor.Object, null));
            Assert.IsNull(context.TryGetMessageEncryptor(typeof(NonContextMessage), mockMessageEncryptor.Object, mockServiceProvider.Object));
            //Verify
            mockServiceProvider.Verify(x => x.GetService(typeof(IServiceInjection)), Times.Once);
        }

    }
}

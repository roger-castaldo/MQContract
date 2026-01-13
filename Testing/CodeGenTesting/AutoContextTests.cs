using Castle.Core.Internal;
using CodeGenTesting.Converters;
using CodeGenTesting.Encoders;
using CodeGenTesting.Encryptors;
using CodeGenTesting.Messages;
using Moq;
using MQContract;
using MQContract.Attributes;
using MQContract.Interfaces;
using MQContract.Interfaces.Conversion;
using MQContract.Interfaces.Encoding;
using MQContract.Interfaces.Encrypting;
using MQContract.Interfaces.Messages;
using MQContract.Messages;

namespace CodeGenTesting
{
    [TestClass]
    public class AutoContextTests
    {
        [TestMethod]
        public void CheckIsMessageCodeGenerated()
        {
            //Arrange
            var context = new MyAutoMessageContext();
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
            var context = new MyAutoMessageContext();
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
            var context = new MyAutoMessageContext();
            var mockMessageEncoder = new Mock<IMessageEncoder>();
            var mockServiceProvider = MockServiceProvider.Instance;
            //Act

            //Assert
            Assert.AreEqual("CodeGenTesting.MyAutoMessageContext+DefaultJsonEncoder`1[CodeGenTesting.Messages.Announcement]", context.TryGetMessageEncoder<Announcement>(null, null)?.GetType().ToString());
            Assert.AreEqual(mockMessageEncoder.Object, context.TryGetMessageEncoder<Announcement>(mockMessageEncoder.Object, null));
            Assert.AreEqual(mockMessageEncoder.Object, context.TryGetMessageEncoder<Announcement>(mockMessageEncoder.Object, mockServiceProvider.Object));

            Assert.IsInstanceOfType(context.TryGetMessageEncoder<PartyAnnouncement>(null, null), typeof(PartyAnnouncementEncoder));
            Assert.IsInstanceOfType(context.TryGetMessageEncoder<PartyAnnouncement>(mockMessageEncoder.Object, null), typeof(PartyAnnouncementEncoder));
            Assert.IsInstanceOfType(context.TryGetMessageEncoder<PartyAnnouncement>(mockMessageEncoder.Object, mockServiceProvider.Object), typeof(PartyAnnouncementEncoder));

            Assert.IsInstanceOfType(context.TryGetMessageEncoder<DirectAnnouncement>(null, null), typeof(DirectAnnouncementEncoder));
            Assert.IsInstanceOfType(context.TryGetMessageEncoder<DirectAnnouncement>(mockMessageEncoder.Object, null), typeof(DirectAnnouncementEncoder));
            Assert.IsInstanceOfType(context.TryGetMessageEncoder<DirectAnnouncement>(mockMessageEncoder.Object, mockServiceProvider.Object), typeof(DirectAnnouncementEncoder));

            Assert.AreEqual("CodeGenTesting.MyAutoMessageContext+DefaultJsonEncoder`1[CodeGenTesting.Messages.Prompt]", context.TryGetMessageEncoder<Prompt>(null, null)?.GetType().ToString());
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
            var context = new MyAutoMessageContext();
            var mockMessageEncryptor = new Mock<IMessageEncryptor>();
            var mockServiceProvider = MockServiceProvider.Instance;
            //Act

            //Assert
            Assert.AreEqual("CodeGenTesting.MyAutoMessageContext+NonEncryptor", context.TryGetMessageEncryptor(typeof(Announcement), null, null)?.GetType().ToString());
            Assert.AreEqual(mockMessageEncryptor.Object, context.TryGetMessageEncryptor(typeof(Announcement), mockMessageEncryptor.Object, null));
            Assert.AreEqual(mockMessageEncryptor.Object, context.TryGetMessageEncryptor(typeof(Announcement), mockMessageEncryptor.Object, mockServiceProvider.Object));

            Assert.AreEqual("CodeGenTesting.MyAutoMessageContext+NonEncryptor",context.TryGetMessageEncryptor(typeof(PartyAnnouncement), null, null)?.GetType().ToString());
            Assert.AreEqual(mockMessageEncryptor.Object, context.TryGetMessageEncryptor(typeof(PartyAnnouncement), mockMessageEncryptor.Object, null));
            Assert.AreEqual(mockMessageEncryptor.Object, context.TryGetMessageEncryptor(typeof(PartyAnnouncement), mockMessageEncryptor.Object, mockServiceProvider.Object));

            Assert.IsInstanceOfType(context.TryGetMessageEncryptor(typeof(DirectAnnouncement), null, null), typeof(DirectAnnouncementEncryptor));
            Assert.IsInstanceOfType(context.TryGetMessageEncryptor(typeof(DirectAnnouncement), mockMessageEncryptor.Object, null), typeof(DirectAnnouncementEncryptor));
            Assert.IsInstanceOfType(context.TryGetMessageEncryptor(typeof(DirectAnnouncement), mockMessageEncryptor.Object, mockServiceProvider.Object), typeof(DirectAnnouncementEncryptor));

            Assert.AreEqual("CodeGenTesting.MyAutoMessageContext+NonEncryptor", context.TryGetMessageEncryptor(typeof(Prompt), null, null)?.GetType().ToString());
            Assert.AreEqual(mockMessageEncryptor.Object, context.TryGetMessageEncryptor(typeof(Prompt), mockMessageEncryptor.Object, null));
            Assert.AreEqual(mockMessageEncryptor.Object, context.TryGetMessageEncryptor(typeof(Prompt), mockMessageEncryptor.Object, mockServiceProvider.Object));

            Assert.IsNull(context.TryGetMessageEncryptor(typeof(NonContextMessage), null, null));
            Assert.IsNull(context.TryGetMessageEncryptor(typeof(NonContextMessage), mockMessageEncryptor.Object, null));
            Assert.IsNull(context.TryGetMessageEncryptor(typeof(NonContextMessage), mockMessageEncryptor.Object, mockServiceProvider.Object));
            //Verify
            mockServiceProvider.Verify(x => x.GetService(typeof(IServiceInjection)), Times.Once);
        }

        [TestMethod]
        public async Task CheckDecodingCallback()
        {
            //Arrange
            var context = new MyAutoMessageContext();
            var mockServiceProvider = MockServiceProvider.Instance;
            var messageContext = new MessageContext();
            messageContext.RegisterContext(context);
            IMessageEncoder globalEncoder = new JsonGlobalEncoder();
            IMessageTypeEncoder<PartyAnnouncement> partyEncoder = new PartyAnnouncementEncoder();
            IMessageTypeEncoder<DirectAnnouncement> directEncoder = new DirectAnnouncementEncoder();


            var announcement = new Announcement(Helper.RandomString());
            IEncodedMessage announcementMessage = new DummyEncodedMessage(
                messageContext.MessageID<Announcement>(),
                await globalEncoder.EncodeAsync<Announcement>(announcement)
            );
            var partyAnnouncement = new PartyAnnouncement(Helper.RandomString(), Helper.RandomString());
            IEncodedMessage partyMessage = new DummyEncodedMessage(
                messageContext.MessageID<PartyAnnouncement>(),
                await partyEncoder.EncodeAsync(partyAnnouncement)
            );
            var directAnnouncement = new DirectAnnouncement(Helper.RandomString(), Helper.RandomString(), Helper.RandomString());
            IEncodedMessage directMessage = new DummyEncodedMessage(
                messageContext.MessageID<DirectAnnouncement>(),
                await directEncoder.EncodeAsync(directAnnouncement)
            );
            var prompt = new Prompt(Helper.RandomString(), Helper.RandomString());
            IEncodedMessage promptMessage = new DummyEncodedMessage(
                messageContext.MessageID<Prompt>(),
                await globalEncoder.EncodeAsync<Prompt>(prompt)
            );

            //Act
            var announcementCalls = new Func<IEncodedMessage, ValueTask<object?>>?[]{
                context.TryGetDecodingCallback(announcementMessage.MessageTypeID, null, null),
                context.TryGetDecodingCallback(announcementMessage.MessageTypeID, globalEncoder, null),
                context.TryGetDecodingCallback(announcementMessage.MessageTypeID, globalEncoder, mockServiceProvider.Object)
            };

            var partyCalls = new Func<IEncodedMessage, ValueTask<object?>>?[]{
                context.TryGetDecodingCallback(partyMessage.MessageTypeID, null, null),
                context.TryGetDecodingCallback(partyMessage.MessageTypeID, globalEncoder, null),
                context.TryGetDecodingCallback(partyMessage.MessageTypeID, globalEncoder, mockServiceProvider.Object)
            };

            var directCalls = new Func<IEncodedMessage, ValueTask<object?>>?[]{
                context.TryGetDecodingCallback(directMessage.MessageTypeID, null, null),
                context.TryGetDecodingCallback(directMessage.MessageTypeID, globalEncoder, null),
                context.TryGetDecodingCallback(directMessage.MessageTypeID, globalEncoder, mockServiceProvider.Object)
            };

            var promptCalls = new Func<IEncodedMessage, ValueTask<object?>>?[]{
                context.TryGetDecodingCallback(promptMessage.MessageTypeID, null, null),
                context.TryGetDecodingCallback(promptMessage.MessageTypeID, globalEncoder, null),
                context.TryGetDecodingCallback(promptMessage.MessageTypeID, globalEncoder, mockServiceProvider.Object)
            };

            //Assert
            foreach(var call in announcementCalls)
            {
                Assert.IsNotNull(call);
                Assert.AreEqual(announcement, await call(announcementMessage));
            }

            foreach (var call in partyCalls)
            {
                Assert.IsNotNull(call);
                Assert.AreEqual(partyAnnouncement, await call(partyMessage));
            }

            foreach (var call in directCalls)
            {
                Assert.IsNotNull(call);
                Assert.AreEqual(directAnnouncement, await call(directMessage));
            }

            foreach (var call in promptCalls)
            {
                Assert.IsNotNull(call);
                Assert.AreEqual(prompt, await call(promptMessage));
            }

            //Verify
            mockServiceProvider.Verify(x => x.GetService(typeof(IServiceInjection)), Times.Exactly(2));
        }

        [TestMethod]
        public async Task CheckConverterCallback()
        {
            //Arrange
            var context = new MyAutoMessageContext();
            var mockServiceProvider = MockServiceProvider.Instance;
            var messageContext = new MessageContext();
            messageContext.RegisterContext(context);
            IMessageEncoder globalEncoder = new JsonGlobalEncoder();
            IMessageTypeEncoder<PartyAnnouncement> partyEncoder = new PartyAnnouncementEncoder();
            var converter = new AnnouncementConverter();

            var announcement = new Announcement(Helper.RandomString());
            IEncodedMessage announcementMessage = new DummyEncodedMessage(
                messageContext.MessageID<Announcement>(),
                await globalEncoder.EncodeAsync<Announcement>(announcement)
            );
            var partyAnnouncement = new PartyAnnouncement(Helper.RandomString(), Helper.RandomString());
            IEncodedMessage partyMessage = new DummyEncodedMessage(
                messageContext.MessageID<PartyAnnouncement>(),
                await partyEncoder.EncodeAsync(partyAnnouncement)
            );

            var partyFromAnnouncement = await ((IMessageConverter<Announcement, PartyAnnouncement>)converter).ConvertAsync(announcement);
            var directFromAnnouncement = await ((IMessageConverter<PartyAnnouncement, DirectAnnouncement>)converter).ConvertAsync(await ((IMessageConverter<Announcement, PartyAnnouncement>)converter).ConvertAsync(announcement));
            var directFromParty = await ((IMessageConverter<PartyAnnouncement, DirectAnnouncement>)converter).ConvertAsync(partyAnnouncement);

            //Act
            var announcementToParty = context.TryGetMessageConverter<PartyAnnouncement>(announcementMessage.MessageTypeID, context.TryGetDecodingCallback(announcementMessage.MessageTypeID, null, null)!, null);
            var announcementToPartyWithService = context.TryGetMessageConverter<PartyAnnouncement>(announcementMessage.MessageTypeID, context.TryGetDecodingCallback(announcementMessage.MessageTypeID, null, null)!, mockServiceProvider.Object);

            var announcementToDirect = context.TryGetMessageConverter<DirectAnnouncement>(announcementMessage.MessageTypeID, context.TryGetDecodingCallback(announcementMessage.MessageTypeID, null, null)!, null);
            var announcementToDirectWithService = context.TryGetMessageConverter<DirectAnnouncement>(announcementMessage.MessageTypeID, context.TryGetDecodingCallback(announcementMessage.MessageTypeID, null, null)!, mockServiceProvider.Object);

            var partyToDirect = context.TryGetMessageConverter<DirectAnnouncement>(partyMessage.MessageTypeID, context.TryGetDecodingCallback(partyMessage.MessageTypeID, null, null)!, null);
            var partyToDirectWithService = context.TryGetMessageConverter<DirectAnnouncement>(partyMessage.MessageTypeID, context.TryGetDecodingCallback(partyMessage.MessageTypeID, null, null)!, mockServiceProvider.Object);

            //Assert
            Assert.IsNull(context.TryGetMessageConverter<Prompt>(announcementMessage.MessageTypeID, context.TryGetDecodingCallback(announcementMessage.MessageTypeID, null, null)!, null));
            Assert.IsNull(context.TryGetMessageConverter<Prompt>(announcementMessage.MessageTypeID, context.TryGetDecodingCallback(announcementMessage.MessageTypeID, null, null)!, mockServiceProvider.Object));
            
            Assert.IsNotNull(announcementToParty);
            Assert.AreEqual(partyFromAnnouncement, await announcementToParty(announcementMessage));
            Assert.IsNotNull(announcementToPartyWithService);
            Assert.AreEqual(partyFromAnnouncement, await announcementToPartyWithService(announcementMessage));
            
            Assert.IsNotNull(announcementToDirect);
            Assert.AreEqual(directFromAnnouncement, await announcementToDirect(announcementMessage));
            Assert.IsNotNull(announcementToDirectWithService);
            Assert.AreEqual(directFromAnnouncement, await announcementToDirectWithService(announcementMessage));

            Assert.IsNotNull(partyToDirect);
            Assert.AreEqual(directFromParty, await partyToDirect(partyMessage));
            Assert.IsNotNull(partyToDirectWithService);
            Assert.AreEqual(directFromParty, await partyToDirectWithService(partyMessage));


            //Verify
            mockServiceProvider.Verify(x => x.GetService(typeof(IServiceInjection)), Times.Exactly(4));
        }

        [TestMethod]
        [DataRow(null, null, null, null, null)]
        [DataRow(100, null, null, null, null)]
        [DataRow(null, "otherChannel", null, null, null)]
        [DataRow(null, null, "replyChannel", null, null)]
        [DataRow(null, null, null, "somekey","somevalue")]
        public async Task CheckContractConnectionQuery(int? timeout, string? channel, string? responseChannel, string? headerKey,string? headerValue)
        {
            //Arrange
            var context = new MyAutoMessageContext();
            var prompt = new Prompt(Helper.RandomString(), Helper.RandomString());
            var replyResult = new QueryResult<Reply>(
                Helper.RandomString(),
                new([
                    new KeyValuePair<string,string>("testkey",Helper.RandomString())
                ]),
                new(Helper.RandomString()),
                new(new Exception(Helper.RandomString()), true)
            );
            var mockConnection = new Mock<IContractConnection>();
            mockConnection.Setup(c => c.QueryAsync<Prompt, Reply>(It.IsAny<Prompt>(), It.IsAny<TimeSpan?>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<MessageHeader?>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(replyResult);
            TimeSpan? messageTimeout = (timeout==null ? null : TimeSpan.FromSeconds(timeout.Value));
            MessageHeader? header = null;
            if (!string.IsNullOrWhiteSpace(headerKey))
                header = new MessageHeader([new KeyValuePair<string, string>(headerKey!, headerValue!)]);
            //Act
            var call = context.TryExecuteQuery<Prompt>(mockConnection.Object, prompt, messageTimeout , channel, responseChannel, header, new CancellationToken());
            Assert.IsTrue(call.HasValue);
            var result = await call.Value.AsTask();

            //Assert
            Assert.AreEqual(replyResult.ID, result.ID);
            Assert.IsTrue(replyResult.Header.Keys.SequenceEqual(result.Header.Keys));
            Assert.AreEqual(replyResult.Header["testkey"], result.Header["testkey"]);
            Assert.AreEqual(replyResult.Result, result.Result);
            Assert.AreEqual(replyResult.IsError, result.IsError);
            Assert.AreEqual(replyResult.Error?.Message, result.Error?.Message);
            Assert.AreEqual(replyResult.Error?.IsFatal, result.Error?.IsFatal);
            //Verify
            mockConnection.Verify(c => c.QueryAsync<Prompt, Reply>(prompt, messageTimeout, channel, responseChannel, header, It.IsAny<CancellationToken>()), Times.Once);
        }

        [TestMethod]
        [DataRow(null, null, null, null, null)]
        [DataRow(100, null, null, null, null)]
        [DataRow(null, "otherChannel", null, null, null)]
        [DataRow(null, null, "replyChannel", null, null)]
        [DataRow(null, null, null, "somekey", "somevalue")]
        public async Task CheckMultiServiceContractConnectionQuery(int? timeout, string? channel, string? responseChannel, string? headerKey, string? headerValue)
        {
            //Arrange
            var context = new MyAutoMessageContext();
            var prompt = new Prompt(Helper.RandomString(), Helper.RandomString());
            var replyResult = new QueryResult<Reply>(
                Helper.RandomString(),
                new([
                    new KeyValuePair<string,string>("testkey",Helper.RandomString())
                ]),
                new(Helper.RandomString()),
                new(new Exception(Helper.RandomString()), true)
            );
            var mockConnection = new Mock<IMultiServiceContractConnection>();
            mockConnection.Setup(c => c.QueryAsync<Prompt, Reply>(It.IsAny<Prompt>(), It.IsAny<TimeSpan?>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<MessageHeader?>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync([replyResult]);
            TimeSpan? messageTimeout = (timeout==null ? null : TimeSpan.FromSeconds(timeout.Value));
            MessageHeader? header = null;
            if (!string.IsNullOrWhiteSpace(headerKey))
                header = new MessageHeader([new KeyValuePair<string, string>(headerKey!, headerValue!)]);
            //Act
            var call = context.TryExecuteQuery<Prompt>(mockConnection.Object, prompt, messageTimeout, channel, responseChannel, header, new CancellationToken());
            Assert.IsTrue(call.HasValue);
            var results = await call.Value.AsTask();

            //Assert
            Assert.AreEqual(1, results.Count());
            var result = results.First();
            Assert.AreEqual(replyResult.ID, result.ID);
            Assert.IsTrue(replyResult.Header.Keys.SequenceEqual(result.Header.Keys));
            Assert.AreEqual(replyResult.Header["testkey"], result.Header["testkey"]);
            Assert.AreEqual(replyResult.Result, result.Result);
            Assert.AreEqual(replyResult.IsError, result.IsError);
            Assert.AreEqual(replyResult.Error?.Message, result.Error?.Message);
            Assert.AreEqual(replyResult.Error?.IsFatal, result.Error?.IsFatal);
            //Verify
            mockConnection.Verify(c => c.QueryAsync<Prompt, Reply>(prompt, messageTimeout, channel, responseChannel, header, It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}

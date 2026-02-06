using CodeGenTesting.Encoders;
using CodeGenTesting.Encryptors;
using CodeGenTesting.Messages;
using Moq;
using MQContract;
using MQContract.Interfaces;
using MQContract.Interfaces.Encoding;
using MQContract.Interfaces.Service;
using MQContract.Messages;
using System.Text.Json;

namespace CodeGenTesting
{
    [TestClass]
    public class DynamicCodeTests
    {
        [ClassInitialize]
        public static void SetupTests(TestContext context)
        {
            DynamicCodeGate.TestOverride=false;
        }

        [TestMethod]
        public void CheckConverters()
        {
            //Arrange
            var context = new MessageContext();
            context.RegisterContext(new MyMessageContext());

            //Act
            var nonContextConverter = context.GetMessageConverter<NonContextMessage>(context.MessageID<Announcement>(), null, null);
            var partyConverter = context.GetMessageConverter<PartyAnnouncement>(context.MessageID<Announcement>(), null, null);

            //Assert
            Assert.IsNull(nonContextConverter);
            Assert.IsNotNull(partyConverter);
            //Verify
        }

        [TestMethod]
        public async Task CheckEncodingCallbacks()
        {
            //Arrange
            var context = new MessageContext();
            context.RegisterContext(new MyMessageContext());
            var nonMessage = new NonContextMessage(Helper.RandomString());
            var partyMessage = new PartyAnnouncement(Helper.RandomString(), Helper.RandomString());

            var nonContextConverter = context.GetEncodingCallbacks<NonContextMessage>(null, null);
            var partyConverter = context.GetEncodingCallbacks<PartyAnnouncement>(null, null);

            //Act
            var nonData = await nonContextConverter.encodeMessage(nonMessage);
            var partyData = await partyConverter.encodeMessage(partyMessage);

            var nonDecodedMessage = await nonContextConverter.decodeMessage(new MemoryStream(nonData));
            var partyDecodedMessage = await partyConverter.decodeMessage(new MemoryStream(partyData));

            var nonJsonMessage = JsonSerializer.Deserialize<NonContextMessage>(new MemoryStream(nonData));

            //Assert
            Assert.AreEqual(nonMessage, nonDecodedMessage);
            Assert.AreEqual(nonMessage, nonJsonMessage);
            Assert.AreEqual(partyMessage, partyDecodedMessage);

            //Verify
        }

        [TestMethod]
        public async Task CheckDecodingCallbacks()
        {
            //Arrange
            var context = new MessageContext();
            context.RegisterContext(new MyMessageContext());

            var globalEncodcer = new Mock<IMessageEncoder>();
            globalEncodcer.Setup(x => x.DecodeAsync<object>(It.IsAny<Stream>()))
                .Returns(async (Stream stream) => await JsonSerializer.DeserializeAsync<object>(stream));

            var nonMessage = new NonContextMessage(Helper.RandomString());
            var partyMessage = new PartyAnnouncement(Helper.RandomString(), Helper.RandomString());

            var nonData = JsonSerializer.SerializeToUtf8Bytes<NonContextMessage>(nonMessage);
            var partyData = await ((IMessageTypeEncoder<PartyAnnouncement>)new PartyAnnouncementEncoder()).EncodeAsync(partyMessage);

            var nonDecoder = context.GetDecodingCallback(context.MessageID<NonContextMessage>(), null, null);
            var globalNonDecoder = context.GetDecodingCallback(context.MessageID<NonContextMessage>(), globalEncodcer.Object, null);
            var partyDecoder = context.GetDecodingCallback(context.MessageID<PartyAnnouncement>(), null, null);

            //Act
            var nonDecodedMessage = (JsonElement?)(await nonDecoder(new DummyEncodedMessage("", nonData)));
            var gloalNonDecodedMessage = (JsonElement?)(await globalNonDecoder(new DummyEncodedMessage("", nonData)));
            var partyDecodedMessage = await partyDecoder(new DummyEncodedMessage("", partyData));


            //Assert
            Assert.AreEqual(nonMessage.Message, nonDecodedMessage?.GetProperty("Message").GetString());
            Assert.AreEqual(nonMessage.Message, gloalNonDecodedMessage?.GetProperty("Message").GetString());
            Assert.AreEqual(partyMessage, partyDecodedMessage);

            //Verify
        }

        [TestMethod]
        public void CheckEncryptors()
        {
            //Arrange
            var context = new MessageContext();
            context.RegisterContext(new MyMessageContext());

            //Act
            var nonContextEncryptor = context.GetMessageEncryptor(typeof(NonContextMessage), null, null);
            var directAnnouncementEncryptor = context.GetMessageEncryptor(typeof(DirectAnnouncement), null, null);

            //Assert
            Assert.IsNull(nonContextEncryptor);
            Assert.IsNotNull(directAnnouncementEncryptor);
            Assert.IsInstanceOfType<DirectAnnouncementEncryptor>(directAnnouncementEncryptor);
            //Verify
        }

        [TestMethod]
        public async Task CheckContextConnectionQuery()
        {
            //Arrange
            var context = new MessageContext();
            context.RegisterContext(new MyMessageContext());
            var prompt = new Prompt(Helper.RandomString(), Helper.RandomString());
            var nonPrompt = new NonContextPrompt(Helper.RandomString(), Helper.RandomString());
            var replyResult = new QueryResult<Reply>(
                Helper.RandomString(),
                new([
                    new KeyValuePair<string,string>("testkey",Helper.RandomString())
                ]),
                new(Helper.RandomString()),
                new(new Exception(Helper.RandomString()), true)
            );
            var nonReplyResult = new QueryResult<NonContextReply>(
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
            mockConnection.Setup(c => c.QueryAsync<NonContextPrompt, NonContextReply>(It.IsAny<NonContextPrompt>(), It.IsAny<TimeSpan?>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<MessageHeader?>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(nonReplyResult);

            //Act
            var error = await Assert.ThrowsExactlyAsync<DynamicCodeNotSupportedException>(async () => await context.ExecuteQuery<NonContextPrompt>(mockConnection.Object, nonPrompt, null, null, null, null, new()));
            var result = await context.ExecuteQuery<Prompt>(mockConnection.Object, prompt, null, null, null, null, new());

            //Assert
            Assert.IsNotNull(error);
            Assert.AreEqual("Unable to execute query due to dynamic code not supported and query type not defined in context", error.Message);
            Assert.AreEqual(replyResult.ID, result.ID);
            Assert.IsTrue(replyResult.Header.Keys.SequenceEqual(result.Header.Keys));
            Assert.AreEqual(replyResult.Header["testkey"], result.Header["testkey"]);
            Assert.AreEqual(replyResult.Result, result.Result);
            Assert.AreEqual(replyResult.IsError, result.IsError);
            Assert.AreEqual(replyResult.Error?.Message, result.Error?.Message);
            Assert.AreEqual(replyResult.Error?.IsFatal, result.Error?.IsFatal);
            //Verify
            mockConnection.Verify(c => c.QueryAsync<Prompt, Reply>(prompt, null, null, null, null, It.IsAny<CancellationToken>()), Times.Once);
            mockConnection.Verify(c => c.QueryAsync<NonContextPrompt, NonContextReply>(nonPrompt, null, null, null, null, It.IsAny<CancellationToken>()), Times.Never);
        }

        [TestMethod]
        public async Task CheckContextMultiServiceConnectionQuery()
        {
            //Arrange
            var context = new MessageContext();
            context.RegisterContext(new MyMessageContext());
            var prompt = new Prompt(Helper.RandomString(), Helper.RandomString());
            var nonPrompt = new NonContextPrompt(Helper.RandomString(), Helper.RandomString());
            var replyResult = new QueryResult<Reply>(
                Helper.RandomString(),
                new([
                    new KeyValuePair<string,string>("testkey",Helper.RandomString())
                ]),
                new(Helper.RandomString()),
                new(new Exception(Helper.RandomString()), true)
            );
            var nonReplyResult = new QueryResult<NonContextReply>(
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
            mockConnection.Setup(c => c.QueryAsync<NonContextPrompt, NonContextReply>(It.IsAny<NonContextPrompt>(), It.IsAny<TimeSpan?>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<MessageHeader?>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync([nonReplyResult]);

            //Act
            var error = await Assert.ThrowsExactlyAsync<DynamicCodeNotSupportedException>(async () => await context.ExecuteQuery<NonContextPrompt>(mockConnection.Object, nonPrompt, null, null, null, null, new()));
            var results = await context.ExecuteQuery<Prompt>(mockConnection.Object, prompt, null, null, null, null, new());

            //Assert
            Assert.IsNotNull(error);
            Assert.AreEqual("Unable to execute query due to dynamic code not supported and query type not defined in context", error.Message);
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
            mockConnection.Verify(c => c.QueryAsync<Prompt, Reply>(prompt, null, null, null, null, It.IsAny<CancellationToken>()), Times.Once);
            mockConnection.Verify(c => c.QueryAsync<NonContextPrompt, NonContextReply>(nonPrompt, null, null, null, null, It.IsAny<CancellationToken>()), Times.Never);
        }

        [TestMethod]
        public async Task CheckAutoConsumerLoads()
        {
            //Arrange
            var mockServiceConnection = new Mock<IMessageServiceConnection>();
            var connection = ContractConnection.Instance(mockServiceConnection.Object);
            connection.RegisterMessageContextAsync(new MyMessageContext());

            //Act
            var errorWithAssembly = await Assert.ThrowsExactlyAsync<DynamicCodeNotSupportedException>(async () => await connection.AutoRegisterAllConsumersAsync(this.GetType().Assembly));
            var errorWithoutAssembly = await Assert.ThrowsExactlyAsync<DynamicCodeNotSupportedException>(async () => await connection.AutoRegisterAllConsumersAsync());

            //Assert
            Assert.IsNotNull(errorWithAssembly);
            Assert.AreEqual("Unable Auto Register consumers without reflection", errorWithAssembly.Message);
            Assert.IsNotNull(errorWithoutAssembly);
            Assert.AreEqual("Unable Auto Register consumers without reflection", errorWithoutAssembly.Message);
            //Verify
        }
    }
}

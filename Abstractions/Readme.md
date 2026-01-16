<a name='assembly'></a>
# MQContract.Abstractions

## Contents

- [ChildTransmissionResult](#T-MQContract-Messages-ChildTransmissionResult 'MQContract.Messages.ChildTransmissionResult')
  - [#ctor(ServiceName,Error)](#M-MQContract-Messages-ChildTransmissionResult-#ctor-System-String,MQContract-Messages-ErrorMessage- 'MQContract.Messages.ChildTransmissionResult.#ctor(System.String,MQContract.Messages.ErrorMessage)')
  - [Error](#P-MQContract-Messages-ChildTransmissionResult-Error 'MQContract.Messages.ChildTransmissionResult.Error')
  - [IsError](#P-MQContract-Messages-ChildTransmissionResult-IsError 'MQContract.Messages.ChildTransmissionResult.IsError')
  - [ServiceName](#P-MQContract-Messages-ChildTransmissionResult-ServiceName 'MQContract.Messages.ChildTransmissionResult.ServiceName')
- [ConsumerAttribute](#T-MQContract-Attributes-ConsumerAttribute 'MQContract.Attributes.ConsumerAttribute')
  - [#ctor(channel,group,ignoreMessageTypeHeader)](#M-MQContract-Attributes-ConsumerAttribute-#ctor-System-String,System-String,System-Boolean- 'MQContract.Attributes.ConsumerAttribute.#ctor(System.String,System.String,System.Boolean)')
  - [Channel](#P-MQContract-Attributes-ConsumerAttribute-Channel 'MQContract.Attributes.ConsumerAttribute.Channel')
  - [Group](#P-MQContract-Attributes-ConsumerAttribute-Group 'MQContract.Attributes.ConsumerAttribute.Group')
  - [IgnoreMessageTypeHeader](#P-MQContract-Attributes-ConsumerAttribute-IgnoreMessageTypeHeader 'MQContract.Attributes.ConsumerAttribute.IgnoreMessageTypeHeader')
- [ConsumerContractConnectionExtensions](#T-MQContract-Extensions-ConsumerContractConnectionExtensions 'MQContract.Extensions.ConsumerContractConnectionExtensions')
  - [RegisterPubSubAsyncConsumerAsync\`\`1(connectionTask,consumerType,channel,group,ignoreMessageHeader,cancellationToken)](#M-MQContract-Extensions-ConsumerContractConnectionExtensions-RegisterPubSubAsyncConsumerAsync``1-System-Threading-Tasks-ValueTask{MQContract-Interfaces-IConsumerContractConnection{``0}},System-Type,System-String,System-String,System-Boolean,System-Threading-CancellationToken- 'MQContract.Extensions.ConsumerContractConnectionExtensions.RegisterPubSubAsyncConsumerAsync``1(System.Threading.Tasks.ValueTask{MQContract.Interfaces.IConsumerContractConnection{``0}},System.Type,System.String,System.String,System.Boolean,System.Threading.CancellationToken)')
  - [RegisterPubSubAsyncConsumerAsync\`\`3(connectionTask,consumer,channel,group,ignoreMessageHeader,messageFilters,cancellationToken)](#M-MQContract-Extensions-ConsumerContractConnectionExtensions-RegisterPubSubAsyncConsumerAsync``3-System-Threading-Tasks-ValueTask{MQContract-Interfaces-IConsumerContractConnection{``0}},``2,System-String,System-String,System-Boolean,MQContract-Messages-MessageFilters{``1},System-Threading-CancellationToken- 'MQContract.Extensions.ConsumerContractConnectionExtensions.RegisterPubSubAsyncConsumerAsync``3(System.Threading.Tasks.ValueTask{MQContract.Interfaces.IConsumerContractConnection{``0}},``2,System.String,System.String,System.Boolean,MQContract.Messages.MessageFilters{``1},System.Threading.CancellationToken)')
  - [RegisterPubSubAsyncConsumerAsync\`\`3(connectionTask,channel,group,ignoreMessageHeader,messageFilters,cancellationToken)](#M-MQContract-Extensions-ConsumerContractConnectionExtensions-RegisterPubSubAsyncConsumerAsync``3-System-Threading-Tasks-ValueTask{MQContract-Interfaces-IConsumerContractConnection{``0}},System-String,System-String,System-Boolean,MQContract-Messages-MessageFilters{``1},System-Threading-CancellationToken- 'MQContract.Extensions.ConsumerContractConnectionExtensions.RegisterPubSubAsyncConsumerAsync``3(System.Threading.Tasks.ValueTask{MQContract.Interfaces.IConsumerContractConnection{``0}},System.String,System.String,System.Boolean,MQContract.Messages.MessageFilters{``1},System.Threading.CancellationToken)')
  - [RegisterPubSubConsumerAsync\`\`1(connectionTask,consumerType,channel,group,ignoreMessageHeader,cancellationToken)](#M-MQContract-Extensions-ConsumerContractConnectionExtensions-RegisterPubSubConsumerAsync``1-System-Threading-Tasks-ValueTask{MQContract-Interfaces-IConsumerContractConnection{``0}},System-Type,System-String,System-String,System-Boolean,System-Threading-CancellationToken- 'MQContract.Extensions.ConsumerContractConnectionExtensions.RegisterPubSubConsumerAsync``1(System.Threading.Tasks.ValueTask{MQContract.Interfaces.IConsumerContractConnection{``0}},System.Type,System.String,System.String,System.Boolean,System.Threading.CancellationToken)')
  - [RegisterPubSubConsumerAsync\`\`3(connectionTask,consumer,channel,group,ignoreMessageHeader,messageFilters,cancellationToken)](#M-MQContract-Extensions-ConsumerContractConnectionExtensions-RegisterPubSubConsumerAsync``3-System-Threading-Tasks-ValueTask{MQContract-Interfaces-IConsumerContractConnection{``0}},``2,System-String,System-String,System-Boolean,MQContract-Messages-MessageFilters{``1},System-Threading-CancellationToken- 'MQContract.Extensions.ConsumerContractConnectionExtensions.RegisterPubSubConsumerAsync``3(System.Threading.Tasks.ValueTask{MQContract.Interfaces.IConsumerContractConnection{``0}},``2,System.String,System.String,System.Boolean,MQContract.Messages.MessageFilters{``1},System.Threading.CancellationToken)')
  - [RegisterPubSubConsumerAsync\`\`3(connectionTask,channel,group,ignoreMessageHeader,messageFilters,cancellationToken)](#M-MQContract-Extensions-ConsumerContractConnectionExtensions-RegisterPubSubConsumerAsync``3-System-Threading-Tasks-ValueTask{MQContract-Interfaces-IConsumerContractConnection{``0}},System-String,System-String,System-Boolean,MQContract-Messages-MessageFilters{``1},System-Threading-CancellationToken- 'MQContract.Extensions.ConsumerContractConnectionExtensions.RegisterPubSubConsumerAsync``3(System.Threading.Tasks.ValueTask{MQContract.Interfaces.IConsumerContractConnection{``0}},System.String,System.String,System.Boolean,MQContract.Messages.MessageFilters{``1},System.Threading.CancellationToken)')
  - [RegisterQueryResponseAsyncConsumerAsync\`\`1(connectionTask,consumerType,channel,group,ignoreMessageHeader,cancellationToken)](#M-MQContract-Extensions-ConsumerContractConnectionExtensions-RegisterQueryResponseAsyncConsumerAsync``1-System-Threading-Tasks-ValueTask{MQContract-Interfaces-IConsumerContractConnection{``0}},System-Type,System-String,System-String,System-Boolean,System-Threading-CancellationToken- 'MQContract.Extensions.ConsumerContractConnectionExtensions.RegisterQueryResponseAsyncConsumerAsync``1(System.Threading.Tasks.ValueTask{MQContract.Interfaces.IConsumerContractConnection{``0}},System.Type,System.String,System.String,System.Boolean,System.Threading.CancellationToken)')
  - [RegisterQueryResponseAsyncConsumerAsync\`\`4(connectionTask,consumer,channel,group,ignoreMessageHeader,messageFilters,cancellationToken)](#M-MQContract-Extensions-ConsumerContractConnectionExtensions-RegisterQueryResponseAsyncConsumerAsync``4-System-Threading-Tasks-ValueTask{MQContract-Interfaces-IConsumerContractConnection{``0}},``3,System-String,System-String,System-Boolean,MQContract-Messages-MessageFilters{``1},System-Threading-CancellationToken- 'MQContract.Extensions.ConsumerContractConnectionExtensions.RegisterQueryResponseAsyncConsumerAsync``4(System.Threading.Tasks.ValueTask{MQContract.Interfaces.IConsumerContractConnection{``0}},``3,System.String,System.String,System.Boolean,MQContract.Messages.MessageFilters{``1},System.Threading.CancellationToken)')
  - [RegisterQueryResponseAsyncConsumerAsync\`\`4(connectionTask,channel,group,ignoreMessageHeader,messageFilters,cancellationToken)](#M-MQContract-Extensions-ConsumerContractConnectionExtensions-RegisterQueryResponseAsyncConsumerAsync``4-System-Threading-Tasks-ValueTask{MQContract-Interfaces-IConsumerContractConnection{``0}},System-String,System-String,System-Boolean,MQContract-Messages-MessageFilters{``1},System-Threading-CancellationToken- 'MQContract.Extensions.ConsumerContractConnectionExtensions.RegisterQueryResponseAsyncConsumerAsync``4(System.Threading.Tasks.ValueTask{MQContract.Interfaces.IConsumerContractConnection{``0}},System.String,System.String,System.Boolean,MQContract.Messages.MessageFilters{``1},System.Threading.CancellationToken)')
  - [RegisterQueryResponseConsumerAsync\`\`1(connectionTask,consumerType,channel,group,ignoreMessageHeader,cancellationToken)](#M-MQContract-Extensions-ConsumerContractConnectionExtensions-RegisterQueryResponseConsumerAsync``1-System-Threading-Tasks-ValueTask{MQContract-Interfaces-IConsumerContractConnection{``0}},System-Type,System-String,System-String,System-Boolean,System-Threading-CancellationToken- 'MQContract.Extensions.ConsumerContractConnectionExtensions.RegisterQueryResponseConsumerAsync``1(System.Threading.Tasks.ValueTask{MQContract.Interfaces.IConsumerContractConnection{``0}},System.Type,System.String,System.String,System.Boolean,System.Threading.CancellationToken)')
  - [RegisterQueryResponseConsumerAsync\`\`4(connectionTask,consumer,channel,group,ignoreMessageHeader,messageFilters,cancellationToken)](#M-MQContract-Extensions-ConsumerContractConnectionExtensions-RegisterQueryResponseConsumerAsync``4-System-Threading-Tasks-ValueTask{MQContract-Interfaces-IConsumerContractConnection{``0}},``3,System-String,System-String,System-Boolean,MQContract-Messages-MessageFilters{``1},System-Threading-CancellationToken- 'MQContract.Extensions.ConsumerContractConnectionExtensions.RegisterQueryResponseConsumerAsync``4(System.Threading.Tasks.ValueTask{MQContract.Interfaces.IConsumerContractConnection{``0}},``3,System.String,System.String,System.Boolean,MQContract.Messages.MessageFilters{``1},System.Threading.CancellationToken)')
  - [RegisterQueryResponseConsumerAsync\`\`4(connectionTask,channel,group,ignoreMessageHeader,messageFilters,cancellationToken)](#M-MQContract-Extensions-ConsumerContractConnectionExtensions-RegisterQueryResponseConsumerAsync``4-System-Threading-Tasks-ValueTask{MQContract-Interfaces-IConsumerContractConnection{``0}},System-String,System-String,System-Boolean,MQContract-Messages-MessageFilters{``1},System-Threading-CancellationToken- 'MQContract.Extensions.ConsumerContractConnectionExtensions.RegisterQueryResponseConsumerAsync``4(System.Threading.Tasks.ValueTask{MQContract.Interfaces.IConsumerContractConnection{``0}},System.String,System.String,System.Boolean,MQContract.Messages.MessageFilters{``1},System.Threading.CancellationToken)')
- [DecodableMessage](#T-MQContract-Interfaces-Middleware-DecodableMessage 'MQContract.Interfaces.Middleware.DecodableMessage')
  - [#ctor(MessageHeader,Data)](#M-MQContract-Interfaces-Middleware-DecodableMessage-#ctor-MQContract-Messages-MessageHeader,System-ReadOnlyMemory{System-Byte}- 'MQContract.Interfaces.Middleware.DecodableMessage.#ctor(MQContract.Messages.MessageHeader,System.ReadOnlyMemory{System.Byte})')
  - [Data](#P-MQContract-Interfaces-Middleware-DecodableMessage-Data 'MQContract.Interfaces.Middleware.DecodableMessage.Data')
  - [MessageHeader](#P-MQContract-Interfaces-Middleware-DecodableMessage-MessageHeader 'MQContract.Interfaces.Middleware.DecodableMessage.MessageHeader')
- [DecodedMessage\`1](#T-MQContract-Interfaces-Middleware-DecodedMessage`1 'MQContract.Interfaces.Middleware.DecodedMessage`1')
  - [#ctor(MessageHeader,Message)](#M-MQContract-Interfaces-Middleware-DecodedMessage`1-#ctor-MQContract-Messages-MessageHeader,`0- 'MQContract.Interfaces.Middleware.DecodedMessage`1.#ctor(MQContract.Messages.MessageHeader,`0)')
  - [Message](#P-MQContract-Interfaces-Middleware-DecodedMessage`1-Message 'MQContract.Interfaces.Middleware.DecodedMessage`1.Message')
  - [MessageHeader](#P-MQContract-Interfaces-Middleware-DecodedMessage`1-MessageHeader 'MQContract.Interfaces.Middleware.DecodedMessage`1.MessageHeader')
- [EncodableMessage\`1](#T-MQContract-Interfaces-Middleware-EncodableMessage`1 'MQContract.Interfaces.Middleware.EncodableMessage`1')
  - [#ctor(MessageHeader,Message,Channel)](#M-MQContract-Interfaces-Middleware-EncodableMessage`1-#ctor-MQContract-Messages-MessageHeader,`0,System-String- 'MQContract.Interfaces.Middleware.EncodableMessage`1.#ctor(MQContract.Messages.MessageHeader,`0,System.String)')
  - [Channel](#P-MQContract-Interfaces-Middleware-EncodableMessage`1-Channel 'MQContract.Interfaces.Middleware.EncodableMessage`1.Channel')
  - [Message](#P-MQContract-Interfaces-Middleware-EncodableMessage`1-Message 'MQContract.Interfaces.Middleware.EncodableMessage`1.Message')
  - [MessageHeader](#P-MQContract-Interfaces-Middleware-EncodableMessage`1-MessageHeader 'MQContract.Interfaces.Middleware.EncodableMessage`1.MessageHeader')
- [EncryptionResult](#T-MQContract-Interfaces-Encrypting-EncryptionResult 'MQContract.Interfaces.Encrypting.EncryptionResult')
  - [#ctor(Headers,Data)](#M-MQContract-Interfaces-Encrypting-EncryptionResult-#ctor-System-Collections-Generic-Dictionary{System-String,System-String},System-Byte[]- 'MQContract.Interfaces.Encrypting.EncryptionResult.#ctor(System.Collections.Generic.Dictionary{System.String,System.String},System.Byte[])')
  - [Data](#P-MQContract-Interfaces-Encrypting-EncryptionResult-Data 'MQContract.Interfaces.Encrypting.EncryptionResult.Data')
  - [Headers](#P-MQContract-Interfaces-Encrypting-EncryptionResult-Headers 'MQContract.Interfaces.Encrypting.EncryptionResult.Headers')
- [ErrorMessage](#T-MQContract-Messages-ErrorMessage 'MQContract.Messages.ErrorMessage')
  - [#ctor(exception,isFatal)](#M-MQContract-Messages-ErrorMessage-#ctor-System-Exception,System-Boolean- 'MQContract.Messages.ErrorMessage.#ctor(System.Exception,System.Boolean)')
  - [Exception](#P-MQContract-Messages-ErrorMessage-Exception 'MQContract.Messages.ErrorMessage.Exception')
  - [IsFatal](#P-MQContract-Messages-ErrorMessage-IsFatal 'MQContract.Messages.ErrorMessage.IsFatal')
  - [Message](#P-MQContract-Messages-ErrorMessage-Message 'MQContract.Messages.ErrorMessage.Message')
- [IAfterDecodeMiddleware](#T-MQContract-Interfaces-Middleware-IAfterDecodeMiddleware 'MQContract.Interfaces.Middleware.IAfterDecodeMiddleware')
  - [AfterMessageDecodeAsync\`\`1(context,ID,message,receivedTimestamp,processedTimeStamp)](#M-MQContract-Interfaces-Middleware-IAfterDecodeMiddleware-AfterMessageDecodeAsync``1-MQContract-Interfaces-Middleware-IContext,System-String,MQContract-Interfaces-Middleware-DecodedMessage{``0},System-DateTime,System-DateTime- 'MQContract.Interfaces.Middleware.IAfterDecodeMiddleware.AfterMessageDecodeAsync``1(MQContract.Interfaces.Middleware.IContext,System.String,MQContract.Interfaces.Middleware.DecodedMessage{``0},System.DateTime,System.DateTime)')
- [IAfterDecodeSpecificTypeMiddleware\`1](#T-MQContract-Interfaces-Middleware-IAfterDecodeSpecificTypeMiddleware`1 'MQContract.Interfaces.Middleware.IAfterDecodeSpecificTypeMiddleware`1')
  - [AfterMessageDecodeAsync(context,ID,message,receivedTimestamp,processedTimeStamp)](#M-MQContract-Interfaces-Middleware-IAfterDecodeSpecificTypeMiddleware`1-AfterMessageDecodeAsync-MQContract-Interfaces-Middleware-IContext,System-String,MQContract-Interfaces-Middleware-DecodedMessage{`0},System-DateTime,System-DateTime- 'MQContract.Interfaces.Middleware.IAfterDecodeSpecificTypeMiddleware`1.AfterMessageDecodeAsync(MQContract.Interfaces.Middleware.IContext,System.String,MQContract.Interfaces.Middleware.DecodedMessage{`0},System.DateTime,System.DateTime)')
- [IAfterEncodeMiddleware](#T-MQContract-Interfaces-Middleware-IAfterEncodeMiddleware 'MQContract.Interfaces.Middleware.IAfterEncodeMiddleware')
  - [AfterMessageEncodeAsync(messageType,context,message)](#M-MQContract-Interfaces-Middleware-IAfterEncodeMiddleware-AfterMessageEncodeAsync-System-Type,MQContract-Interfaces-Middleware-IContext,MQContract-Messages-ServiceMessage- 'MQContract.Interfaces.Middleware.IAfterEncodeMiddleware.AfterMessageEncodeAsync(System.Type,MQContract.Interfaces.Middleware.IContext,MQContract.Messages.ServiceMessage)')
- [IBaseConsumer](#T-MQContract-Interfaces-Consumers-IBaseConsumer 'MQContract.Interfaces.Consumers.IBaseConsumer')
  - [ErrorRecieved(error)](#M-MQContract-Interfaces-Consumers-IBaseConsumer-ErrorRecieved-System-Exception- 'MQContract.Interfaces.Consumers.IBaseConsumer.ErrorRecieved(System.Exception)')
- [IBaseContractConnection](#T-MQContract-Interfaces-IBaseContractConnection 'MQContract.Interfaces.IBaseContractConnection')
  - [HealthCheck](#P-MQContract-Interfaces-IBaseContractConnection-HealthCheck 'MQContract.Interfaces.IBaseContractConnection.HealthCheck')
  - [CloseAsync()](#M-MQContract-Interfaces-IBaseContractConnection-CloseAsync 'MQContract.Interfaces.IBaseContractConnection.CloseAsync')
  - [SubscribeAsync\`\`1(messageReceived,errorReceived,channel,group,ignoreMessageHeader,messageFilters,cancellationToken)](#M-MQContract-Interfaces-IBaseContractConnection-SubscribeAsync``1-System-Func{MQContract-Interfaces-IReceivedMessage{``0},System-Threading-Tasks-ValueTask},System-Action{System-Exception},System-String,System-String,System-Boolean,MQContract-Messages-MessageFilters{``0},System-Threading-CancellationToken- 'MQContract.Interfaces.IBaseContractConnection.SubscribeAsync``1(System.Func{MQContract.Interfaces.IReceivedMessage{``0},System.Threading.Tasks.ValueTask},System.Action{System.Exception},System.String,System.String,System.Boolean,MQContract.Messages.MessageFilters{``0},System.Threading.CancellationToken)')
  - [SubscribeAsync\`\`1(messageReceived,errorReceived,channel,group,ignoreMessageHeader,messageFilters,cancellationToken)](#M-MQContract-Interfaces-IBaseContractConnection-SubscribeAsync``1-System-Action{MQContract-Interfaces-IReceivedMessage{``0}},System-Action{System-Exception},System-String,System-String,System-Boolean,MQContract-Messages-MessageFilters{``0},System-Threading-CancellationToken- 'MQContract.Interfaces.IBaseContractConnection.SubscribeAsync``1(System.Action{MQContract.Interfaces.IReceivedMessage{``0}},System.Action{System.Exception},System.String,System.String,System.Boolean,MQContract.Messages.MessageFilters{``0},System.Threading.CancellationToken)')
  - [SubscribeQueryAsyncResponseAsync\`\`2(messageReceived,errorReceived,channel,group,ignoreMessageHeader,messageFilters,cancellationToken)](#M-MQContract-Interfaces-IBaseContractConnection-SubscribeQueryAsyncResponseAsync``2-System-Func{MQContract-Interfaces-IReceivedMessage{``0},System-Threading-Tasks-ValueTask{MQContract-Messages-QueryResponseMessage{``1}}},System-Action{System-Exception},System-String,System-String,System-Boolean,MQContract-Messages-MessageFilters{``0},System-Threading-CancellationToken- 'MQContract.Interfaces.IBaseContractConnection.SubscribeQueryAsyncResponseAsync``2(System.Func{MQContract.Interfaces.IReceivedMessage{``0},System.Threading.Tasks.ValueTask{MQContract.Messages.QueryResponseMessage{``1}}},System.Action{System.Exception},System.String,System.String,System.Boolean,MQContract.Messages.MessageFilters{``0},System.Threading.CancellationToken)')
  - [SubscribeQueryResponseAsync\`\`2(messageReceived,errorReceived,channel,group,ignoreMessageHeader,messageFilters,cancellationToken)](#M-MQContract-Interfaces-IBaseContractConnection-SubscribeQueryResponseAsync``2-System-Func{MQContract-Interfaces-IReceivedMessage{``0},MQContract-Messages-QueryResponseMessage{``1}},System-Action{System-Exception},System-String,System-String,System-Boolean,MQContract-Messages-MessageFilters{``0},System-Threading-CancellationToken- 'MQContract.Interfaces.IBaseContractConnection.SubscribeQueryResponseAsync``2(System.Func{MQContract.Interfaces.IReceivedMessage{``0},MQContract.Messages.QueryResponseMessage{``1}},System.Action{System.Exception},System.String,System.String,System.Boolean,MQContract.Messages.MessageFilters{``0},System.Threading.CancellationToken)')
- [IBeforeDecodeMiddleware](#T-MQContract-Interfaces-Middleware-IBeforeDecodeMiddleware 'MQContract.Interfaces.Middleware.IBeforeDecodeMiddleware')
  - [BeforeMessageDecodeAsync(context,id,messageTypeID,messageChannel,message)](#M-MQContract-Interfaces-Middleware-IBeforeDecodeMiddleware-BeforeMessageDecodeAsync-MQContract-Interfaces-Middleware-IContext,System-String,System-String,System-String,MQContract-Interfaces-Middleware-DecodableMessage- 'MQContract.Interfaces.Middleware.IBeforeDecodeMiddleware.BeforeMessageDecodeAsync(MQContract.Interfaces.Middleware.IContext,System.String,System.String,System.String,MQContract.Interfaces.Middleware.DecodableMessage)')
- [IBeforeEncodeMiddleware](#T-MQContract-Interfaces-Middleware-IBeforeEncodeMiddleware 'MQContract.Interfaces.Middleware.IBeforeEncodeMiddleware')
  - [BeforeMessageEncodeAsync\`\`1(context,message)](#M-MQContract-Interfaces-Middleware-IBeforeEncodeMiddleware-BeforeMessageEncodeAsync``1-MQContract-Interfaces-Middleware-IContext,MQContract-Interfaces-Middleware-EncodableMessage{``0}- 'MQContract.Interfaces.Middleware.IBeforeEncodeMiddleware.BeforeMessageEncodeAsync``1(MQContract.Interfaces.Middleware.IContext,MQContract.Interfaces.Middleware.EncodableMessage{``0})')
- [IBeforeEncodeSpecificTypeMiddleware\`1](#T-MQContract-Interfaces-Middleware-IBeforeEncodeSpecificTypeMiddleware`1 'MQContract.Interfaces.Middleware.IBeforeEncodeSpecificTypeMiddleware`1')
  - [BeforeMessageEncodeAsync(context,message)](#M-MQContract-Interfaces-Middleware-IBeforeEncodeSpecificTypeMiddleware`1-BeforeMessageEncodeAsync-MQContract-Interfaces-Middleware-IContext,MQContract-Interfaces-Middleware-EncodableMessage{`0}- 'MQContract.Interfaces.Middleware.IBeforeEncodeSpecificTypeMiddleware`1.BeforeMessageEncodeAsync(MQContract.Interfaces.Middleware.IContext,MQContract.Interfaces.Middleware.EncodableMessage{`0})')
- [IBulkPublishableMessageServiceConnection](#T-MQContract-Interfaces-Service-IBulkPublishableMessageServiceConnection 'MQContract.Interfaces.Service.IBulkPublishableMessageServiceConnection')
  - [BulkPublishAsync(messages,cancellationToken)](#M-MQContract-Interfaces-Service-IBulkPublishableMessageServiceConnection-BulkPublishAsync-System-Collections-Generic-IEnumerable{MQContract-Messages-ServiceMessage},System-Threading-CancellationToken- 'MQContract.Interfaces.Service.IBulkPublishableMessageServiceConnection.BulkPublishAsync(System.Collections.Generic.IEnumerable{MQContract.Messages.ServiceMessage},System.Threading.CancellationToken)')
- [IConsumerContractConnection\`1](#T-MQContract-Interfaces-IConsumerContractConnection`1 'MQContract.Interfaces.IConsumerContractConnection`1')
  - [AutoRegisterAllConsumersAsync(assembly,cancellationToken)](#M-MQContract-Interfaces-IConsumerContractConnection`1-AutoRegisterAllConsumersAsync-System-Reflection-Assembly,System-Threading-CancellationToken- 'MQContract.Interfaces.IConsumerContractConnection`1.AutoRegisterAllConsumersAsync(System.Reflection.Assembly,System.Threading.CancellationToken)')
  - [RegisterPubSubAsyncConsumerAsync(consumerType,channel,group,ignoreMessageHeader,cancellationToken)](#M-MQContract-Interfaces-IConsumerContractConnection`1-RegisterPubSubAsyncConsumerAsync-System-Type,System-String,System-String,System-Boolean,System-Threading-CancellationToken- 'MQContract.Interfaces.IConsumerContractConnection`1.RegisterPubSubAsyncConsumerAsync(System.Type,System.String,System.String,System.Boolean,System.Threading.CancellationToken)')
  - [RegisterPubSubAsyncConsumerAsync\`\`2(consumer,channel,group,ignoreMessageHeader,messageFilters,cancellationToken)](#M-MQContract-Interfaces-IConsumerContractConnection`1-RegisterPubSubAsyncConsumerAsync``2-``1,System-String,System-String,System-Boolean,MQContract-Messages-MessageFilters{``0},System-Threading-CancellationToken- 'MQContract.Interfaces.IConsumerContractConnection`1.RegisterPubSubAsyncConsumerAsync``2(``1,System.String,System.String,System.Boolean,MQContract.Messages.MessageFilters{``0},System.Threading.CancellationToken)')
  - [RegisterPubSubAsyncConsumerAsync\`\`2(channel,group,ignoreMessageHeader,messageFilters,cancellationToken)](#M-MQContract-Interfaces-IConsumerContractConnection`1-RegisterPubSubAsyncConsumerAsync``2-System-String,System-String,System-Boolean,MQContract-Messages-MessageFilters{``0},System-Threading-CancellationToken- 'MQContract.Interfaces.IConsumerContractConnection`1.RegisterPubSubAsyncConsumerAsync``2(System.String,System.String,System.Boolean,MQContract.Messages.MessageFilters{``0},System.Threading.CancellationToken)')
  - [RegisterPubSubConsumerAsync(consumerType,channel,group,ignoreMessageHeader,cancellationToken)](#M-MQContract-Interfaces-IConsumerContractConnection`1-RegisterPubSubConsumerAsync-System-Type,System-String,System-String,System-Boolean,System-Threading-CancellationToken- 'MQContract.Interfaces.IConsumerContractConnection`1.RegisterPubSubConsumerAsync(System.Type,System.String,System.String,System.Boolean,System.Threading.CancellationToken)')
  - [RegisterPubSubConsumerAsync\`\`2(consumer,channel,group,ignoreMessageHeader,messageFilters,cancellationToken)](#M-MQContract-Interfaces-IConsumerContractConnection`1-RegisterPubSubConsumerAsync``2-``1,System-String,System-String,System-Boolean,MQContract-Messages-MessageFilters{``0},System-Threading-CancellationToken- 'MQContract.Interfaces.IConsumerContractConnection`1.RegisterPubSubConsumerAsync``2(``1,System.String,System.String,System.Boolean,MQContract.Messages.MessageFilters{``0},System.Threading.CancellationToken)')
  - [RegisterPubSubConsumerAsync\`\`2(channel,group,ignoreMessageHeader,messageFilters,cancellationToken)](#M-MQContract-Interfaces-IConsumerContractConnection`1-RegisterPubSubConsumerAsync``2-System-String,System-String,System-Boolean,MQContract-Messages-MessageFilters{``0},System-Threading-CancellationToken- 'MQContract.Interfaces.IConsumerContractConnection`1.RegisterPubSubConsumerAsync``2(System.String,System.String,System.Boolean,MQContract.Messages.MessageFilters{``0},System.Threading.CancellationToken)')
  - [RegisterQueryResponseAsyncConsumerAsync(consumerType,channel,group,ignoreMessageHeader,cancellationToken)](#M-MQContract-Interfaces-IConsumerContractConnection`1-RegisterQueryResponseAsyncConsumerAsync-System-Type,System-String,System-String,System-Boolean,System-Threading-CancellationToken- 'MQContract.Interfaces.IConsumerContractConnection`1.RegisterQueryResponseAsyncConsumerAsync(System.Type,System.String,System.String,System.Boolean,System.Threading.CancellationToken)')
  - [RegisterQueryResponseAsyncConsumerAsync\`\`3(consumer,channel,group,ignoreMessageHeader,messageFilters,cancellationToken)](#M-MQContract-Interfaces-IConsumerContractConnection`1-RegisterQueryResponseAsyncConsumerAsync``3-``2,System-String,System-String,System-Boolean,MQContract-Messages-MessageFilters{``0},System-Threading-CancellationToken- 'MQContract.Interfaces.IConsumerContractConnection`1.RegisterQueryResponseAsyncConsumerAsync``3(``2,System.String,System.String,System.Boolean,MQContract.Messages.MessageFilters{``0},System.Threading.CancellationToken)')
  - [RegisterQueryResponseAsyncConsumerAsync\`\`3(channel,group,ignoreMessageHeader,messageFilters,cancellationToken)](#M-MQContract-Interfaces-IConsumerContractConnection`1-RegisterQueryResponseAsyncConsumerAsync``3-System-String,System-String,System-Boolean,MQContract-Messages-MessageFilters{``0},System-Threading-CancellationToken- 'MQContract.Interfaces.IConsumerContractConnection`1.RegisterQueryResponseAsyncConsumerAsync``3(System.String,System.String,System.Boolean,MQContract.Messages.MessageFilters{``0},System.Threading.CancellationToken)')
  - [RegisterQueryResponseConsumerAsync(consumerType,channel,group,ignoreMessageHeader,cancellationToken)](#M-MQContract-Interfaces-IConsumerContractConnection`1-RegisterQueryResponseConsumerAsync-System-Type,System-String,System-String,System-Boolean,System-Threading-CancellationToken- 'MQContract.Interfaces.IConsumerContractConnection`1.RegisterQueryResponseConsumerAsync(System.Type,System.String,System.String,System.Boolean,System.Threading.CancellationToken)')
  - [RegisterQueryResponseConsumerAsync\`\`3(consumer,channel,group,ignoreMessageHeader,messageFilters,cancellationToken)](#M-MQContract-Interfaces-IConsumerContractConnection`1-RegisterQueryResponseConsumerAsync``3-``2,System-String,System-String,System-Boolean,MQContract-Messages-MessageFilters{``0},System-Threading-CancellationToken- 'MQContract.Interfaces.IConsumerContractConnection`1.RegisterQueryResponseConsumerAsync``3(``2,System.String,System.String,System.Boolean,MQContract.Messages.MessageFilters{``0},System.Threading.CancellationToken)')
  - [RegisterQueryResponseConsumerAsync\`\`3(channel,group,ignoreMessageHeader,messageFilters,cancellationToken)](#M-MQContract-Interfaces-IConsumerContractConnection`1-RegisterQueryResponseConsumerAsync``3-System-String,System-String,System-Boolean,MQContract-Messages-MessageFilters{``0},System-Threading-CancellationToken- 'MQContract.Interfaces.IConsumerContractConnection`1.RegisterQueryResponseConsumerAsync``3(System.String,System.String,System.Boolean,MQContract.Messages.MessageFilters{``0},System.Threading.CancellationToken)')
- [IContext](#T-MQContract-Interfaces-Middleware-IContext 'MQContract.Interfaces.Middleware.IContext')
  - [Activity](#P-MQContract-Interfaces-Middleware-IContext-Activity 'MQContract.Interfaces.Middleware.IContext.Activity')
  - [Item](#P-MQContract-Interfaces-Middleware-IContext-Item-System-String- 'MQContract.Interfaces.Middleware.IContext.Item(System.String)')
- [IContractConnection](#T-MQContract-Interfaces-IContractConnection 'MQContract.Interfaces.IContractConnection')
  - [BulkPublishAsync\`\`1(messages,channel,cancellationToken)](#M-MQContract-Interfaces-IContractConnection-BulkPublishAsync``1-System-Collections-Generic-IEnumerable{System-ValueTuple{``0,MQContract-Messages-MessageHeader}},System-String,System-Threading-CancellationToken- 'MQContract.Interfaces.IContractConnection.BulkPublishAsync``1(System.Collections.Generic.IEnumerable{System.ValueTuple{``0,MQContract.Messages.MessageHeader}},System.String,System.Threading.CancellationToken)')
  - [PingAsync()](#M-MQContract-Interfaces-IContractConnection-PingAsync 'MQContract.Interfaces.IContractConnection.PingAsync')
  - [PublishAsync\`\`1(message,channel,messageHeader,cancellationToken)](#M-MQContract-Interfaces-IContractConnection-PublishAsync``1-``0,System-String,MQContract-Messages-MessageHeader,System-Threading-CancellationToken- 'MQContract.Interfaces.IContractConnection.PublishAsync``1(``0,System.String,MQContract.Messages.MessageHeader,System.Threading.CancellationToken)')
  - [QueryAsync\`\`1(message,timeout,channel,responseChannel,messageHeader,cancellationToken)](#M-MQContract-Interfaces-IContractConnection-QueryAsync``1-``0,System-Nullable{System-TimeSpan},System-String,System-String,MQContract-Messages-MessageHeader,System-Threading-CancellationToken- 'MQContract.Interfaces.IContractConnection.QueryAsync``1(``0,System.Nullable{System.TimeSpan},System.String,System.String,MQContract.Messages.MessageHeader,System.Threading.CancellationToken)')
  - [QueryAsync\`\`2(message,timeout,channel,responseChannel,messageHeader,cancellationToken)](#M-MQContract-Interfaces-IContractConnection-QueryAsync``2-``0,System-Nullable{System-TimeSpan},System-String,System-String,MQContract-Messages-MessageHeader,System-Threading-CancellationToken- 'MQContract.Interfaces.IContractConnection.QueryAsync``2(``0,System.Nullable{System.TimeSpan},System.String,System.String,MQContract.Messages.MessageHeader,System.Threading.CancellationToken)')
- [IContractMetric](#T-MQContract-Interfaces-IContractMetric 'MQContract.Interfaces.IContractMetric')
  - [MessageBytes](#P-MQContract-Interfaces-IContractMetric-MessageBytes 'MQContract.Interfaces.IContractMetric.MessageBytes')
  - [MessageBytesAverage](#P-MQContract-Interfaces-IContractMetric-MessageBytesAverage 'MQContract.Interfaces.IContractMetric.MessageBytesAverage')
  - [MessageBytesMax](#P-MQContract-Interfaces-IContractMetric-MessageBytesMax 'MQContract.Interfaces.IContractMetric.MessageBytesMax')
  - [MessageBytesMin](#P-MQContract-Interfaces-IContractMetric-MessageBytesMin 'MQContract.Interfaces.IContractMetric.MessageBytesMin')
  - [MessageConversionAverage](#P-MQContract-Interfaces-IContractMetric-MessageConversionAverage 'MQContract.Interfaces.IContractMetric.MessageConversionAverage')
  - [MessageConversionDuration](#P-MQContract-Interfaces-IContractMetric-MessageConversionDuration 'MQContract.Interfaces.IContractMetric.MessageConversionDuration')
  - [MessageConversionMax](#P-MQContract-Interfaces-IContractMetric-MessageConversionMax 'MQContract.Interfaces.IContractMetric.MessageConversionMax')
  - [MessageConversionMin](#P-MQContract-Interfaces-IContractMetric-MessageConversionMin 'MQContract.Interfaces.IContractMetric.MessageConversionMin')
  - [Messages](#P-MQContract-Interfaces-IContractMetric-Messages 'MQContract.Interfaces.IContractMetric.Messages')
- [IContractedConnection](#T-MQContract-Interfaces-IContractedConnection 'MQContract.Interfaces.IContractedConnection')
- [IEncodedMessage](#T-MQContract-Interfaces-Messages-IEncodedMessage 'MQContract.Interfaces.Messages.IEncodedMessage')
  - [Data](#P-MQContract-Interfaces-Messages-IEncodedMessage-Data 'MQContract.Interfaces.Messages.IEncodedMessage.Data')
  - [Header](#P-MQContract-Interfaces-Messages-IEncodedMessage-Header 'MQContract.Interfaces.Messages.IEncodedMessage.Header')
  - [MessageTypeID](#P-MQContract-Interfaces-Messages-IEncodedMessage-MessageTypeID 'MQContract.Interfaces.Messages.IEncodedMessage.MessageTypeID')
- [IHeaderFilteredConsumer](#T-MQContract-Interfaces-Consumers-IHeaderFilteredConsumer 'MQContract.Interfaces.Consumers.IHeaderFilteredConsumer')
  - [Filter](#P-MQContract-Interfaces-Consumers-IHeaderFilteredConsumer-Filter 'MQContract.Interfaces.Consumers.IHeaderFilteredConsumer.Filter')
- [IInboxQueryableMessageServiceConnection](#T-MQContract-Interfaces-Service-IInboxQueryableMessageServiceConnection 'MQContract.Interfaces.Service.IInboxQueryableMessageServiceConnection')
  - [EstablishInboxSubscriptionAsync(messageReceived,cancellationToken)](#M-MQContract-Interfaces-Service-IInboxQueryableMessageServiceConnection-EstablishInboxSubscriptionAsync-System-Func{MQContract-Messages-ReceivedInboxServiceMessage,System-Threading-Tasks-ValueTask},System-Threading-CancellationToken- 'MQContract.Interfaces.Service.IInboxQueryableMessageServiceConnection.EstablishInboxSubscriptionAsync(System.Func{MQContract.Messages.ReceivedInboxServiceMessage,System.Threading.Tasks.ValueTask},System.Threading.CancellationToken)')
  - [QueryAsync(message,correlationID,cancellationToken)](#M-MQContract-Interfaces-Service-IInboxQueryableMessageServiceConnection-QueryAsync-MQContract-Messages-ServiceMessage,System-Guid,System-Threading-CancellationToken- 'MQContract.Interfaces.Service.IInboxQueryableMessageServiceConnection.QueryAsync(MQContract.Messages.ServiceMessage,System.Guid,System.Threading.CancellationToken)')
- [IMappableContractConnection\`1](#T-MQContract-Interfaces-IMappableContractConnection`1 'MQContract.Interfaces.IMappableContractConnection`1')
  - [RegisterResiliencePolicy(serviceConnectionName,retryPolicy,circuitBreakPolicy)](#M-MQContract-Interfaces-IMappableContractConnection`1-RegisterResiliencePolicy-System-String,System-Nullable{System-ValueTuple{System-Int32,System-Func{System-Int32,System-TimeSpan}}},System-Nullable{System-ValueTuple{System-Int32,System-TimeSpan}}- 'MQContract.Interfaces.IMappableContractConnection`1.RegisterResiliencePolicy(System.String,System.Nullable{System.ValueTuple{System.Int32,System.Func{System.Int32,System.TimeSpan}}},System.Nullable{System.ValueTuple{System.Int32,System.TimeSpan}})')
  - [RegisterResiliencePolicy(serviceConnectionName,messageType,retryPolicy,circuitBreakPolicy)](#M-MQContract-Interfaces-IMappableContractConnection`1-RegisterResiliencePolicy-System-String,System-Type,System-Nullable{System-ValueTuple{System-Int32,System-Func{System-Int32,System-TimeSpan}}},System-Nullable{System-ValueTuple{System-Int32,System-TimeSpan}}- 'MQContract.Interfaces.IMappableContractConnection`1.RegisterResiliencePolicy(System.String,System.Type,System.Nullable{System.ValueTuple{System.Int32,System.Func{System.Int32,System.TimeSpan}}},System.Nullable{System.ValueTuple{System.Int32,System.TimeSpan}})')
  - [RegisterResiliencePolicy(serviceConnectionName,messageChannel,retryPolicy,circuitBreakPolicy)](#M-MQContract-Interfaces-IMappableContractConnection`1-RegisterResiliencePolicy-System-String,System-String,System-Nullable{System-ValueTuple{System-Int32,System-Func{System-Int32,System-TimeSpan}}},System-Nullable{System-ValueTuple{System-Int32,System-TimeSpan}}- 'MQContract.Interfaces.IMappableContractConnection`1.RegisterResiliencePolicy(System.String,System.String,System.Nullable{System.ValueTuple{System.Int32,System.Func{System.Int32,System.TimeSpan}}},System.Nullable{System.ValueTuple{System.Int32,System.TimeSpan}})')
  - [RegisterResiliencePolicy\`\`1(serviceConnectionName,retryPolicy,circuitBreakPolicy)](#M-MQContract-Interfaces-IMappableContractConnection`1-RegisterResiliencePolicy``1-System-String,System-Nullable{System-ValueTuple{System-Int32,System-Func{System-Int32,System-TimeSpan}}},System-Nullable{System-ValueTuple{System-Int32,System-TimeSpan}}- 'MQContract.Interfaces.IMappableContractConnection`1.RegisterResiliencePolicy``1(System.String,System.Nullable{System.ValueTuple{System.Int32,System.Func{System.Int32,System.TimeSpan}}},System.Nullable{System.ValueTuple{System.Int32,System.TimeSpan}})')
  - [RegisterServiceConnection(checkCallback,serviceConnectionName,messageServiceConnection)](#M-MQContract-Interfaces-IMappableContractConnection`1-RegisterServiceConnection-System-Func{System-ValueTuple{System-String,System-Type,MQContract-Messages-MessageHeader},System-Boolean},System-String,MQContract-Interfaces-Service-IMessageServiceConnection- 'MQContract.Interfaces.IMappableContractConnection`1.RegisterServiceConnection(System.Func{System.ValueTuple{System.String,System.Type,MQContract.Messages.MessageHeader},System.Boolean},System.String,MQContract.Interfaces.Service.IMessageServiceConnection)')
  - [RegisterServiceConnection(channel,serviceConnectionName,messageServiceConnection)](#M-MQContract-Interfaces-IMappableContractConnection`1-RegisterServiceConnection-System-String,System-String,MQContract-Interfaces-Service-IMessageServiceConnection- 'MQContract.Interfaces.IMappableContractConnection`1.RegisterServiceConnection(System.String,System.String,MQContract.Interfaces.Service.IMessageServiceConnection)')
  - [RegisterServiceConnection(messageType,serviceConnectionName,messageServiceConnection)](#M-MQContract-Interfaces-IMappableContractConnection`1-RegisterServiceConnection-System-Type,System-String,MQContract-Interfaces-Service-IMessageServiceConnection- 'MQContract.Interfaces.IMappableContractConnection`1.RegisterServiceConnection(System.Type,System.String,MQContract.Interfaces.Service.IMessageServiceConnection)')
  - [RegisterServiceConnection(messageHeaderKey,messageHeaderValue,serviceConnectionName,messageServiceConnection)](#M-MQContract-Interfaces-IMappableContractConnection`1-RegisterServiceConnection-System-String,System-String,System-String,MQContract-Interfaces-Service-IMessageServiceConnection- 'MQContract.Interfaces.IMappableContractConnection`1.RegisterServiceConnection(System.String,System.String,System.String,MQContract.Interfaces.Service.IMessageServiceConnection)')
  - [RegisterServiceConnection\`\`1(serviceConnectionName,messageServiceConnection)](#M-MQContract-Interfaces-IMappableContractConnection`1-RegisterServiceConnection``1-System-String,MQContract-Interfaces-Service-IMessageServiceConnection- 'MQContract.Interfaces.IMappableContractConnection`1.RegisterServiceConnection``1(System.String,MQContract.Interfaces.Service.IMessageServiceConnection)')
- [IMappedContractConnection](#T-MQContract-Interfaces-IMappedContractConnection 'MQContract.Interfaces.IMappedContractConnection')
- [IMessageContextContractConnection\`1](#T-MQContract-Interfaces-IMessageContextContractConnection`1 'MQContract.Interfaces.IMessageContextContractConnection`1')
  - [RegisterMessageContext(messageContext)](#M-MQContract-Interfaces-IMessageContextContractConnection`1-RegisterMessageContext-MQContract-MQContractMessageContext- 'MQContract.Interfaces.IMessageContextContractConnection`1.RegisterMessageContext(MQContract.MQContractMessageContext)')
- [IMessageConverter\`2](#T-MQContract-Interfaces-Conversion-IMessageConverter`2 'MQContract.Interfaces.Conversion.IMessageConverter`2')
  - [ConvertAsync(source)](#M-MQContract-Interfaces-Conversion-IMessageConverter`2-ConvertAsync-`0- 'MQContract.Interfaces.Conversion.IMessageConverter`2.ConvertAsync(`0)')
- [IMessageEncoder](#T-MQContract-Interfaces-Encoding-IMessageEncoder 'MQContract.Interfaces.Encoding.IMessageEncoder')
  - [DecodeAsync\`\`1(stream)](#M-MQContract-Interfaces-Encoding-IMessageEncoder-DecodeAsync``1-System-IO-Stream- 'MQContract.Interfaces.Encoding.IMessageEncoder.DecodeAsync``1(System.IO.Stream)')
  - [EncodeAsync\`\`1(message)](#M-MQContract-Interfaces-Encoding-IMessageEncoder-EncodeAsync``1-``0- 'MQContract.Interfaces.Encoding.IMessageEncoder.EncodeAsync``1(``0)')
- [IMessageEncryptor](#T-MQContract-Interfaces-Encrypting-IMessageEncryptor 'MQContract.Interfaces.Encrypting.IMessageEncryptor')
  - [DecryptAsync(stream,headers)](#M-MQContract-Interfaces-Encrypting-IMessageEncryptor-DecryptAsync-System-IO-Stream,MQContract-Messages-MessageHeader- 'MQContract.Interfaces.Encrypting.IMessageEncryptor.DecryptAsync(System.IO.Stream,MQContract.Messages.MessageHeader)')
  - [EncryptAsync(data)](#M-MQContract-Interfaces-Encrypting-IMessageEncryptor-EncryptAsync-System-Byte[]- 'MQContract.Interfaces.Encrypting.IMessageEncryptor.EncryptAsync(System.Byte[])')
- [IMessageFilteredConsumer\`1](#T-MQContract-Interfaces-Consumers-IMessageFilteredConsumer`1 'MQContract.Interfaces.Consumers.IMessageFilteredConsumer`1')
  - [Filter](#P-MQContract-Interfaces-Consumers-IMessageFilteredConsumer`1-Filter 'MQContract.Interfaces.Consumers.IMessageFilteredConsumer`1.Filter')
- [IMessageServiceConnection](#T-MQContract-Interfaces-Service-IMessageServiceConnection 'MQContract.Interfaces.Service.IMessageServiceConnection')
  - [MaxMessageBodySize](#P-MQContract-Interfaces-Service-IMessageServiceConnection-MaxMessageBodySize 'MQContract.Interfaces.Service.IMessageServiceConnection.MaxMessageBodySize')
  - [CloseAsync()](#M-MQContract-Interfaces-Service-IMessageServiceConnection-CloseAsync 'MQContract.Interfaces.Service.IMessageServiceConnection.CloseAsync')
  - [PublishAsync(message,cancellationToken)](#M-MQContract-Interfaces-Service-IMessageServiceConnection-PublishAsync-MQContract-Messages-ServiceMessage,System-Threading-CancellationToken- 'MQContract.Interfaces.Service.IMessageServiceConnection.PublishAsync(MQContract.Messages.ServiceMessage,System.Threading.CancellationToken)')
  - [SubscribeAsync(messageReceived,errorReceived,channel,group,cancellationToken)](#M-MQContract-Interfaces-Service-IMessageServiceConnection-SubscribeAsync-System-Func{MQContract-Messages-ReceivedServiceMessage,System-Threading-Tasks-ValueTask},System-Action{System-Exception},System-String,System-String,System-Threading-CancellationToken- 'MQContract.Interfaces.Service.IMessageServiceConnection.SubscribeAsync(System.Func{MQContract.Messages.ReceivedServiceMessage,System.Threading.Tasks.ValueTask},System.Action{System.Exception},System.String,System.String,System.Threading.CancellationToken)')
- [IMessageTypeEncoder\`1](#T-MQContract-Interfaces-Encoding-IMessageTypeEncoder`1 'MQContract.Interfaces.Encoding.IMessageTypeEncoder`1')
  - [DecodeAsync(stream)](#M-MQContract-Interfaces-Encoding-IMessageTypeEncoder`1-DecodeAsync-System-IO-Stream- 'MQContract.Interfaces.Encoding.IMessageTypeEncoder`1.DecodeAsync(System.IO.Stream)')
  - [EncodeAsync(message)](#M-MQContract-Interfaces-Encoding-IMessageTypeEncoder`1-EncodeAsync-`0- 'MQContract.Interfaces.Encoding.IMessageTypeEncoder`1.EncodeAsync(`0)')
- [IMessageTypeEncryptor\`1](#T-MQContract-Interfaces-Encrypting-IMessageTypeEncryptor`1 'MQContract.Interfaces.Encrypting.IMessageTypeEncryptor`1')
- [IMetricContractConnection\`1](#T-MQContract-Interfaces-IMetricContractConnection`1 'MQContract.Interfaces.IMetricContractConnection`1')
  - [AddMetrics(meter,useInternal)](#M-MQContract-Interfaces-IMetricContractConnection`1-AddMetrics-System-Diagnostics-Metrics-Meter,System-Boolean- 'MQContract.Interfaces.IMetricContractConnection`1.AddMetrics(System.Diagnostics.Metrics.Meter,System.Boolean)')
  - [EnableOpenTelemetry(activitySource,linkActivitiesAcrossSystems)](#M-MQContract-Interfaces-IMetricContractConnection`1-EnableOpenTelemetry-System-String,System-Boolean- 'MQContract.Interfaces.IMetricContractConnection`1.EnableOpenTelemetry(System.String,System.Boolean)')
  - [GetSnapshot(sent)](#M-MQContract-Interfaces-IMetricContractConnection`1-GetSnapshot-System-Boolean- 'MQContract.Interfaces.IMetricContractConnection`1.GetSnapshot(System.Boolean)')
  - [GetSnapshot(messageType,sent)](#M-MQContract-Interfaces-IMetricContractConnection`1-GetSnapshot-System-Type,System-Boolean- 'MQContract.Interfaces.IMetricContractConnection`1.GetSnapshot(System.Type,System.Boolean)')
  - [GetSnapshot(channel,sent)](#M-MQContract-Interfaces-IMetricContractConnection`1-GetSnapshot-System-String,System-Boolean- 'MQContract.Interfaces.IMetricContractConnection`1.GetSnapshot(System.String,System.Boolean)')
  - [GetSnapshot\`\`1(sent)](#M-MQContract-Interfaces-IMetricContractConnection`1-GetSnapshot``1-System-Boolean- 'MQContract.Interfaces.IMetricContractConnection`1.GetSnapshot``1(System.Boolean)')
- [IMiddleware](#T-MQContract-Interfaces-Middleware-IMiddleware 'MQContract.Interfaces.Middleware.IMiddleware')
- [IMiddlewareContractConnection\`1](#T-MQContract-Interfaces-IMiddlewareContractConnection`1 'MQContract.Interfaces.IMiddlewareContractConnection`1')
  - [RegisterMiddleware(middleware)](#M-MQContract-Interfaces-IMiddlewareContractConnection`1-RegisterMiddleware-System-Type- 'MQContract.Interfaces.IMiddlewareContractConnection`1.RegisterMiddleware(System.Type)')
  - [RegisterMiddleware(instance)](#M-MQContract-Interfaces-IMiddlewareContractConnection`1-RegisterMiddleware-MQContract-Interfaces-Middleware-IMiddleware- 'MQContract.Interfaces.IMiddlewareContractConnection`1.RegisterMiddleware(MQContract.Interfaces.Middleware.IMiddleware)')
  - [RegisterMiddleware(constructInstance)](#M-MQContract-Interfaces-IMiddlewareContractConnection`1-RegisterMiddleware-System-Func{MQContract-Interfaces-Middleware-IMiddleware}- 'MQContract.Interfaces.IMiddlewareContractConnection`1.RegisterMiddleware(System.Func{MQContract.Interfaces.Middleware.IMiddleware})')
  - [RegisterMiddleware\`\`1()](#M-MQContract-Interfaces-IMiddlewareContractConnection`1-RegisterMiddleware``1 'MQContract.Interfaces.IMiddlewareContractConnection`1.RegisterMiddleware``1')
  - [RegisterMiddleware\`\`1(constructInstance)](#M-MQContract-Interfaces-IMiddlewareContractConnection`1-RegisterMiddleware``1-System-Func{``0}- 'MQContract.Interfaces.IMiddlewareContractConnection`1.RegisterMiddleware``1(System.Func{``0})')
  - [RegisterMiddleware\`\`1(constructInstance)](#M-MQContract-Interfaces-IMiddlewareContractConnection`1-RegisterMiddleware``1-System-Func{MQContract-Interfaces-Middleware-ISpecificTypeMiddleware{``0}}- 'MQContract.Interfaces.IMiddlewareContractConnection`1.RegisterMiddleware``1(System.Func{MQContract.Interfaces.Middleware.ISpecificTypeMiddleware{``0}})')
  - [RegisterMiddleware\`\`1(instance)](#M-MQContract-Interfaces-IMiddlewareContractConnection`1-RegisterMiddleware``1-MQContract-Interfaces-Middleware-ISpecificTypeMiddleware{``0}- 'MQContract.Interfaces.IMiddlewareContractConnection`1.RegisterMiddleware``1(MQContract.Interfaces.Middleware.ISpecificTypeMiddleware{``0})')
  - [RegisterMiddleware\`\`2()](#M-MQContract-Interfaces-IMiddlewareContractConnection`1-RegisterMiddleware``2 'MQContract.Interfaces.IMiddlewareContractConnection`1.RegisterMiddleware``2')
  - [RegisterMiddleware\`\`2(constructInstance)](#M-MQContract-Interfaces-IMiddlewareContractConnection`1-RegisterMiddleware``2-System-Func{``0}- 'MQContract.Interfaces.IMiddlewareContractConnection`1.RegisterMiddleware``2(System.Func{``0})')
- [IMultiServiceContractConnection](#T-MQContract-Interfaces-IMultiServiceContractConnection 'MQContract.Interfaces.IMultiServiceContractConnection')
  - [BulkPublishAsync\`\`1(messages,channel,cancellationToken)](#M-MQContract-Interfaces-IMultiServiceContractConnection-BulkPublishAsync``1-System-Collections-Generic-IEnumerable{System-ValueTuple{``0,MQContract-Messages-MessageHeader}},System-String,System-Threading-CancellationToken- 'MQContract.Interfaces.IMultiServiceContractConnection.BulkPublishAsync``1(System.Collections.Generic.IEnumerable{System.ValueTuple{``0,MQContract.Messages.MessageHeader}},System.String,System.Threading.CancellationToken)')
  - [PingAsync()](#M-MQContract-Interfaces-IMultiServiceContractConnection-PingAsync 'MQContract.Interfaces.IMultiServiceContractConnection.PingAsync')
  - [PublishAsync\`\`1(message,channel,messageHeader,cancellationToken)](#M-MQContract-Interfaces-IMultiServiceContractConnection-PublishAsync``1-``0,System-String,MQContract-Messages-MessageHeader,System-Threading-CancellationToken- 'MQContract.Interfaces.IMultiServiceContractConnection.PublishAsync``1(``0,System.String,MQContract.Messages.MessageHeader,System.Threading.CancellationToken)')
  - [QueryAsync\`\`1(message,timeout,channel,responseChannel,messageHeader,cancellationToken)](#M-MQContract-Interfaces-IMultiServiceContractConnection-QueryAsync``1-``0,System-Nullable{System-TimeSpan},System-String,System-String,MQContract-Messages-MessageHeader,System-Threading-CancellationToken- 'MQContract.Interfaces.IMultiServiceContractConnection.QueryAsync``1(``0,System.Nullable{System.TimeSpan},System.String,System.String,MQContract.Messages.MessageHeader,System.Threading.CancellationToken)')
  - [QueryAsync\`\`2(message,timeout,channel,responseChannel,messageHeader,cancellationToken)](#M-MQContract-Interfaces-IMultiServiceContractConnection-QueryAsync``2-``0,System-Nullable{System-TimeSpan},System-String,System-String,MQContract-Messages-MessageHeader,System-Threading-CancellationToken- 'MQContract.Interfaces.IMultiServiceContractConnection.QueryAsync``2(``0,System.Nullable{System.TimeSpan},System.String,System.String,MQContract.Messages.MessageHeader,System.Threading.CancellationToken)')
  - [RegisterServiceConnection(serviceConnectionName,messageServiceConnection)](#M-MQContract-Interfaces-IMultiServiceContractConnection-RegisterServiceConnection-System-String,MQContract-Interfaces-Service-IMessageServiceConnection- 'MQContract.Interfaces.IMultiServiceContractConnection.RegisterServiceConnection(System.String,MQContract.Interfaces.Service.IMessageServiceConnection)')
- [IPingableMessageServiceConnection](#T-MQContract-Interfaces-Service-IPingableMessageServiceConnection 'MQContract.Interfaces.Service.IPingableMessageServiceConnection')
  - [PingAsync()](#M-MQContract-Interfaces-Service-IPingableMessageServiceConnection-PingAsync 'MQContract.Interfaces.Service.IPingableMessageServiceConnection.PingAsync')
- [IPubSubAsyncConsumer\`1](#T-MQContract-Interfaces-Consumers-IPubSubAsyncConsumer`1 'MQContract.Interfaces.Consumers.IPubSubAsyncConsumer`1')
  - [MessageReceivedAsync(message)](#M-MQContract-Interfaces-Consumers-IPubSubAsyncConsumer`1-MessageReceivedAsync-MQContract-Interfaces-IReceivedMessage{`0}- 'MQContract.Interfaces.Consumers.IPubSubAsyncConsumer`1.MessageReceivedAsync(MQContract.Interfaces.IReceivedMessage{`0})')
- [IPubSubConsumer\`1](#T-MQContract-Interfaces-Consumers-IPubSubConsumer`1 'MQContract.Interfaces.Consumers.IPubSubConsumer`1')
  - [MessageReceived(message)](#M-MQContract-Interfaces-Consumers-IPubSubConsumer`1-MessageReceived-MQContract-Interfaces-IReceivedMessage{`0}- 'MQContract.Interfaces.Consumers.IPubSubConsumer`1.MessageReceived(MQContract.Interfaces.IReceivedMessage{`0})')
- [IQueryResponseAsyncConsumer\`2](#T-MQContract-Interfaces-Consumers-IQueryResponseAsyncConsumer`2 'MQContract.Interfaces.Consumers.IQueryResponseAsyncConsumer`2')
  - [MessageReceivedAsync(message)](#M-MQContract-Interfaces-Consumers-IQueryResponseAsyncConsumer`2-MessageReceivedAsync-MQContract-Interfaces-IReceivedMessage{`0}- 'MQContract.Interfaces.Consumers.IQueryResponseAsyncConsumer`2.MessageReceivedAsync(MQContract.Interfaces.IReceivedMessage{`0})')
- [IQueryResponseConsumer\`2](#T-MQContract-Interfaces-Consumers-IQueryResponseConsumer`2 'MQContract.Interfaces.Consumers.IQueryResponseConsumer`2')
  - [MessageReceived(message)](#M-MQContract-Interfaces-Consumers-IQueryResponseConsumer`2-MessageReceived-MQContract-Interfaces-IReceivedMessage{`0}- 'MQContract.Interfaces.Consumers.IQueryResponseConsumer`2.MessageReceived(MQContract.Interfaces.IReceivedMessage{`0})')
- [IQueryResponseMessageServiceConnection](#T-MQContract-Interfaces-Service-IQueryResponseMessageServiceConnection 'MQContract.Interfaces.Service.IQueryResponseMessageServiceConnection')
  - [QueryAsync(message,timeout,cancellationToken)](#M-MQContract-Interfaces-Service-IQueryResponseMessageServiceConnection-QueryAsync-MQContract-Messages-ServiceMessage,System-TimeSpan,System-Threading-CancellationToken- 'MQContract.Interfaces.Service.IQueryResponseMessageServiceConnection.QueryAsync(MQContract.Messages.ServiceMessage,System.TimeSpan,System.Threading.CancellationToken)')
- [IQueryableMessageServiceConnection](#T-MQContract-Interfaces-Service-IQueryableMessageServiceConnection 'MQContract.Interfaces.Service.IQueryableMessageServiceConnection')
  - [DefaultTimeout](#P-MQContract-Interfaces-Service-IQueryableMessageServiceConnection-DefaultTimeout 'MQContract.Interfaces.Service.IQueryableMessageServiceConnection.DefaultTimeout')
  - [SubscribeQueryAsync(messageReceived,errorReceived,channel,group,cancellationToken)](#M-MQContract-Interfaces-Service-IQueryableMessageServiceConnection-SubscribeQueryAsync-System-Func{MQContract-Messages-ReceivedServiceMessage,System-Threading-Tasks-ValueTask{MQContract-Messages-ServiceMessage}},System-Action{System-Exception},System-String,System-String,System-Threading-CancellationToken- 'MQContract.Interfaces.Service.IQueryableMessageServiceConnection.SubscribeQueryAsync(System.Func{MQContract.Messages.ReceivedServiceMessage,System.Threading.Tasks.ValueTask{MQContract.Messages.ServiceMessage}},System.Action{System.Exception},System.String,System.String,System.Threading.CancellationToken)')
- [IReceivedMessage\`1](#T-MQContract-Interfaces-IReceivedMessage`1 'MQContract.Interfaces.IReceivedMessage`1')
  - [Activity](#P-MQContract-Interfaces-IReceivedMessage`1-Activity 'MQContract.Interfaces.IReceivedMessage`1.Activity')
  - [Headers](#P-MQContract-Interfaces-IReceivedMessage`1-Headers 'MQContract.Interfaces.IReceivedMessage`1.Headers')
  - [ID](#P-MQContract-Interfaces-IReceivedMessage`1-ID 'MQContract.Interfaces.IReceivedMessage`1.ID')
  - [Message](#P-MQContract-Interfaces-IReceivedMessage`1-Message 'MQContract.Interfaces.IReceivedMessage`1.Message')
  - [ProcessedTimestamp](#P-MQContract-Interfaces-IReceivedMessage`1-ProcessedTimestamp 'MQContract.Interfaces.IReceivedMessage`1.ProcessedTimestamp')
  - [ReceivedTimestamp](#P-MQContract-Interfaces-IReceivedMessage`1-ReceivedTimestamp 'MQContract.Interfaces.IReceivedMessage`1.ReceivedTimestamp')
- [IResilientContractConnection\`1](#T-MQContract-Interfaces-IResilientContractConnection`1 'MQContract.Interfaces.IResilientContractConnection`1')
  - [RegisterResiliencePolicy(retryPolicy,circuitBreakPolicy)](#M-MQContract-Interfaces-IResilientContractConnection`1-RegisterResiliencePolicy-System-Nullable{System-ValueTuple{System-Int32,System-Func{System-Int32,System-TimeSpan}}},System-Nullable{System-ValueTuple{System-Int32,System-TimeSpan}}- 'MQContract.Interfaces.IResilientContractConnection`1.RegisterResiliencePolicy(System.Nullable{System.ValueTuple{System.Int32,System.Func{System.Int32,System.TimeSpan}}},System.Nullable{System.ValueTuple{System.Int32,System.TimeSpan}})')
  - [RegisterResiliencePolicy(messageType,retryPolicy,circuitBreakPolicy)](#M-MQContract-Interfaces-IResilientContractConnection`1-RegisterResiliencePolicy-System-Type,System-Nullable{System-ValueTuple{System-Int32,System-Func{System-Int32,System-TimeSpan}}},System-Nullable{System-ValueTuple{System-Int32,System-TimeSpan}}- 'MQContract.Interfaces.IResilientContractConnection`1.RegisterResiliencePolicy(System.Type,System.Nullable{System.ValueTuple{System.Int32,System.Func{System.Int32,System.TimeSpan}}},System.Nullable{System.ValueTuple{System.Int32,System.TimeSpan}})')
  - [RegisterResiliencePolicy(messageChannel,retryPolicy,circuitBreakPolicy)](#M-MQContract-Interfaces-IResilientContractConnection`1-RegisterResiliencePolicy-System-String,System-Nullable{System-ValueTuple{System-Int32,System-Func{System-Int32,System-TimeSpan}}},System-Nullable{System-ValueTuple{System-Int32,System-TimeSpan}}- 'MQContract.Interfaces.IResilientContractConnection`1.RegisterResiliencePolicy(System.String,System.Nullable{System.ValueTuple{System.Int32,System.Func{System.Int32,System.TimeSpan}}},System.Nullable{System.ValueTuple{System.Int32,System.TimeSpan}})')
  - [RegisterResiliencePolicy\`\`1(retryPolicy,circuitBreakPolicy)](#M-MQContract-Interfaces-IResilientContractConnection`1-RegisterResiliencePolicy``1-System-Nullable{System-ValueTuple{System-Int32,System-Func{System-Int32,System-TimeSpan}}},System-Nullable{System-ValueTuple{System-Int32,System-TimeSpan}}- 'MQContract.Interfaces.IResilientContractConnection`1.RegisterResiliencePolicy``1(System.Nullable{System.ValueTuple{System.Int32,System.Func{System.Int32,System.TimeSpan}}},System.Nullable{System.ValueTuple{System.Int32,System.TimeSpan}})')
- [IServiceSubscription](#T-MQContract-Interfaces-Service-IServiceSubscription 'MQContract.Interfaces.Service.IServiceSubscription')
  - [EndAsync()](#M-MQContract-Interfaces-Service-IServiceSubscription-EndAsync 'MQContract.Interfaces.Service.IServiceSubscription.EndAsync')
- [ISpecificTypeMiddleware\`1](#T-MQContract-Interfaces-Middleware-ISpecificTypeMiddleware`1 'MQContract.Interfaces.Middleware.ISpecificTypeMiddleware`1')
- [ISubscription](#T-MQContract-Interfaces-ISubscription 'MQContract.Interfaces.ISubscription')
  - [EndAsync()](#M-MQContract-Interfaces-ISubscription-EndAsync 'MQContract.Interfaces.ISubscription.EndAsync')
- [InvalidEncoderException](#T-MQContract-InvalidEncoderException 'MQContract.InvalidEncoderException')
- [InvalidEncryptorException](#T-MQContract-InvalidEncryptorException 'MQContract.InvalidEncryptorException')
- [MQContractMessageContext](#T-MQContract-MQContractMessageContext 'MQContract.MQContractMessageContext')
  - [IsMessageCodeGenerated(messageType)](#M-MQContract-MQContractMessageContext-IsMessageCodeGenerated-System-Type- 'MQContract.MQContractMessageContext.IsMessageCodeGenerated(System.Type)')
  - [IsMessageCodeGenerated\`\`1()](#M-MQContract-MQContractMessageContext-IsMessageCodeGenerated``1 'MQContract.MQContractMessageContext.IsMessageCodeGenerated``1')
  - [TryExecuteQuery\`\`1(contractConnection,message,timeout,channel,responseChannel,messageHeader,cancellationToken)](#M-MQContract-MQContractMessageContext-TryExecuteQuery``1-MQContract-Interfaces-IContractConnection,System-Object,System-Nullable{System-TimeSpan},System-String,System-String,MQContract-Messages-MessageHeader,System-Threading-CancellationToken- 'MQContract.MQContractMessageContext.TryExecuteQuery``1(MQContract.Interfaces.IContractConnection,System.Object,System.Nullable{System.TimeSpan},System.String,System.String,MQContract.Messages.MessageHeader,System.Threading.CancellationToken)')
  - [TryExecuteQuery\`\`1(contractConnection,message,timeout,channel,responseChannel,messageHeader,cancellationToken)](#M-MQContract-MQContractMessageContext-TryExecuteQuery``1-MQContract-Interfaces-IMultiServiceContractConnection,System-Object,System-Nullable{System-TimeSpan},System-String,System-String,MQContract-Messages-MessageHeader,System-Threading-CancellationToken- 'MQContract.MQContractMessageContext.TryExecuteQuery``1(MQContract.Interfaces.IMultiServiceContractConnection,System.Object,System.Nullable{System.TimeSpan},System.String,System.String,MQContract.Messages.MessageHeader,System.Threading.CancellationToken)')
  - [TryGetDecodingCallback(messageID,globalMessageEncoder,serviceProvider)](#M-MQContract-MQContractMessageContext-TryGetDecodingCallback-System-String,MQContract-Interfaces-Encoding-IMessageEncoder,System-IServiceProvider- 'MQContract.MQContractMessageContext.TryGetDecodingCallback(System.String,MQContract.Interfaces.Encoding.IMessageEncoder,System.IServiceProvider)')
  - [TryGetMessageConverter\`\`1(messageID,messageDecode,serviceProvider)](#M-MQContract-MQContractMessageContext-TryGetMessageConverter``1-System-String,System-Func{MQContract-Interfaces-Messages-IEncodedMessage,System-Threading-Tasks-ValueTask{System-Object}},System-IServiceProvider- 'MQContract.MQContractMessageContext.TryGetMessageConverter``1(System.String,System.Func{MQContract.Interfaces.Messages.IEncodedMessage,System.Threading.Tasks.ValueTask{System.Object}},System.IServiceProvider)')
  - [TryGetMessageEncoder\`\`1(globalMessageEncoder,serviceProvider)](#M-MQContract-MQContractMessageContext-TryGetMessageEncoder``1-MQContract-Interfaces-Encoding-IMessageEncoder,System-IServiceProvider- 'MQContract.MQContractMessageContext.TryGetMessageEncoder``1(MQContract.Interfaces.Encoding.IMessageEncoder,System.IServiceProvider)')
  - [TryGetMessageEncryptor(messageType,globalEncryptor,serviceProvider)](#M-MQContract-MQContractMessageContext-TryGetMessageEncryptor-System-Type,MQContract-Interfaces-Encrypting-IMessageEncryptor,System-IServiceProvider- 'MQContract.MQContractMessageContext.TryGetMessageEncryptor(System.Type,MQContract.Interfaces.Encrypting.IMessageEncryptor,System.IServiceProvider)')
  - [TryGetMessageType(messageType)](#M-MQContract-MQContractMessageContext-TryGetMessageType-System-Type- 'MQContract.MQContractMessageContext.TryGetMessageType(System.Type)')
- [MQContractMessageContextAttribute](#T-MQContract-Attributes-MQContractMessageContextAttribute 'MQContract.Attributes.MQContractMessageContextAttribute')
  - [#ctor(locateEncoders,locateConverters,locateEncryptors)](#M-MQContract-Attributes-MQContractMessageContextAttribute-#ctor-System-Boolean,System-Boolean,System-Boolean- 'MQContract.Attributes.MQContractMessageContextAttribute.#ctor(System.Boolean,System.Boolean,System.Boolean)')
  - [LocateConverters](#P-MQContract-Attributes-MQContractMessageContextAttribute-LocateConverters 'MQContract.Attributes.MQContractMessageContextAttribute.LocateConverters')
  - [LocateEncoders](#P-MQContract-Attributes-MQContractMessageContextAttribute-LocateEncoders 'MQContract.Attributes.MQContractMessageContextAttribute.LocateEncoders')
  - [LocateEncryptors](#P-MQContract-Attributes-MQContractMessageContextAttribute-LocateEncryptors 'MQContract.Attributes.MQContractMessageContextAttribute.LocateEncryptors')
- [MessageAttribute](#T-MQContract-Attributes-MessageAttribute 'MQContract.Attributes.MessageAttribute')
  - [#ctor(channel,typeName,typeVersion)](#M-MQContract-Attributes-MessageAttribute-#ctor-System-String,System-String,System-String- 'MQContract.Attributes.MessageAttribute.#ctor(System.String,System.String,System.String)')
  - [Channel](#P-MQContract-Attributes-MessageAttribute-Channel 'MQContract.Attributes.MessageAttribute.Channel')
  - [TypeName](#P-MQContract-Attributes-MessageAttribute-TypeName 'MQContract.Attributes.MessageAttribute.TypeName')
  - [TypeVersion](#P-MQContract-Attributes-MessageAttribute-TypeVersion 'MQContract.Attributes.MessageAttribute.TypeVersion')
- [MessageFilterResult](#T-MQContract-MessageFilterResult 'MQContract.MessageFilterResult')
  - [Allow](#F-MQContract-MessageFilterResult-Allow 'MQContract.MessageFilterResult.Allow')
  - [DropAndAcknowledge](#F-MQContract-MessageFilterResult-DropAndAcknowledge 'MQContract.MessageFilterResult.DropAndAcknowledge')
  - [DropAndDontAcknowledge](#F-MQContract-MessageFilterResult-DropAndDontAcknowledge 'MQContract.MessageFilterResult.DropAndDontAcknowledge')
- [MessageFilters\`1](#T-MQContract-Messages-MessageFilters`1 'MQContract.Messages.MessageFilters`1')
  - [#ctor(HeaderFilter,MessageFilter)](#M-MQContract-Messages-MessageFilters`1-#ctor-System-Func{MQContract-Messages-MessageHeader,System-Threading-Tasks-ValueTask{MQContract-MessageFilterResult}},System-Func{`0,MQContract-Messages-MessageHeader,System-Threading-Tasks-ValueTask{MQContract-MessageFilterResult}}- 'MQContract.Messages.MessageFilters`1.#ctor(System.Func{MQContract.Messages.MessageHeader,System.Threading.Tasks.ValueTask{MQContract.MessageFilterResult}},System.Func{`0,MQContract.Messages.MessageHeader,System.Threading.Tasks.ValueTask{MQContract.MessageFilterResult}})')
  - [HeaderFilter](#P-MQContract-Messages-MessageFilters`1-HeaderFilter 'MQContract.Messages.MessageFilters`1.HeaderFilter')
  - [MessageFilter](#P-MQContract-Messages-MessageFilters`1-MessageFilter 'MQContract.Messages.MessageFilters`1.MessageFilter')
- [MessageHeader](#T-MQContract-Messages-MessageHeader 'MQContract.Messages.MessageHeader')
  - [#ctor(data)](#M-MQContract-Messages-MessageHeader-#ctor-System-Collections-Generic-IEnumerable{System-Collections-Generic-KeyValuePair{System-String,System-String}}- 'MQContract.Messages.MessageHeader.#ctor(System.Collections.Generic.IEnumerable{System.Collections.Generic.KeyValuePair{System.String,System.String}})')
  - [#ctor(headers)](#M-MQContract-Messages-MessageHeader-#ctor-System-Collections-Generic-Dictionary{System-String,System-String}- 'MQContract.Messages.MessageHeader.#ctor(System.Collections.Generic.Dictionary{System.String,System.String})')
  - [#ctor(originalHeader,appendedHeader)](#M-MQContract-Messages-MessageHeader-#ctor-MQContract-Messages-MessageHeader,System-Collections-Generic-Dictionary{System-String,System-String}- 'MQContract.Messages.MessageHeader.#ctor(MQContract.Messages.MessageHeader,System.Collections.Generic.Dictionary{System.String,System.String})')
  - [#ctor(originalHeader,appendedHeader)](#M-MQContract-Messages-MessageHeader-#ctor-MQContract-Messages-MessageHeader,MQContract-Messages-MessageHeader- 'MQContract.Messages.MessageHeader.#ctor(MQContract.Messages.MessageHeader,MQContract.Messages.MessageHeader)')
  - [Item](#P-MQContract-Messages-MessageHeader-Item-System-String- 'MQContract.Messages.MessageHeader.Item(System.String)')
  - [Keys](#P-MQContract-Messages-MessageHeader-Keys 'MQContract.Messages.MessageHeader.Keys')
- [MessageTypeDefinition](#T-MQContract-MQContractMessageContext-MessageTypeDefinition 'MQContract.MQContractMessageContext.MessageTypeDefinition')
  - [#ctor(Channel,TypeName,TypeVersion,ResponseChannel,ResponseTimeout,ResponseType)](#M-MQContract-MQContractMessageContext-MessageTypeDefinition-#ctor-System-String,System-String,System-Version,System-String,System-Nullable{System-TimeSpan},System-Type- 'MQContract.MQContractMessageContext.MessageTypeDefinition.#ctor(System.String,System.String,System.Version,System.String,System.Nullable{System.TimeSpan},System.Type)')
  - [Channel](#P-MQContract-MQContractMessageContext-MessageTypeDefinition-Channel 'MQContract.MQContractMessageContext.MessageTypeDefinition.Channel')
  - [ResponseChannel](#P-MQContract-MQContractMessageContext-MessageTypeDefinition-ResponseChannel 'MQContract.MQContractMessageContext.MessageTypeDefinition.ResponseChannel')
  - [ResponseTimeout](#P-MQContract-MQContractMessageContext-MessageTypeDefinition-ResponseTimeout 'MQContract.MQContractMessageContext.MessageTypeDefinition.ResponseTimeout')
  - [ResponseType](#P-MQContract-MQContractMessageContext-MessageTypeDefinition-ResponseType 'MQContract.MQContractMessageContext.MessageTypeDefinition.ResponseType')
  - [TypeName](#P-MQContract-MQContractMessageContext-MessageTypeDefinition-TypeName 'MQContract.MQContractMessageContext.MessageTypeDefinition.TypeName')
  - [TypeVersion](#P-MQContract-MQContractMessageContext-MessageTypeDefinition-TypeVersion 'MQContract.MQContractMessageContext.MessageTypeDefinition.TypeVersion')
- [MultiTransmissionResult](#T-MQContract-Messages-MultiTransmissionResult 'MQContract.Messages.MultiTransmissionResult')
  - [#ctor(ID,Results)](#M-MQContract-Messages-MultiTransmissionResult-#ctor-System-String,System-Collections-Generic-IEnumerable{MQContract-Messages-ChildTransmissionResult}- 'MQContract.Messages.MultiTransmissionResult.#ctor(System.String,System.Collections.Generic.IEnumerable{MQContract.Messages.ChildTransmissionResult})')
  - [HasError](#P-MQContract-Messages-MultiTransmissionResult-HasError 'MQContract.Messages.MultiTransmissionResult.HasError')
  - [ID](#P-MQContract-Messages-MultiTransmissionResult-ID 'MQContract.Messages.MultiTransmissionResult.ID')
  - [Results](#P-MQContract-Messages-MultiTransmissionResult-Results 'MQContract.Messages.MultiTransmissionResult.Results')
- [PingFailedException](#T-MQContract-PingFailedException 'MQContract.PingFailedException')
  - [#ctor()](#M-MQContract-PingFailedException-#ctor-System-String- 'MQContract.PingFailedException.#ctor(System.String)')
- [PingResult](#T-MQContract-Messages-PingResult 'MQContract.Messages.PingResult')
  - [#ctor(Host,Version,ResponseTime)](#M-MQContract-Messages-PingResult-#ctor-System-String,System-String,System-TimeSpan- 'MQContract.Messages.PingResult.#ctor(System.String,System.String,System.TimeSpan)')
  - [Host](#P-MQContract-Messages-PingResult-Host 'MQContract.Messages.PingResult.Host')
  - [ResponseTime](#P-MQContract-Messages-PingResult-ResponseTime 'MQContract.Messages.PingResult.ResponseTime')
  - [Version](#P-MQContract-Messages-PingResult-Version 'MQContract.Messages.PingResult.Version')
- [QueryMessageAttribute](#T-MQContract-Attributes-QueryMessageAttribute 'MQContract.Attributes.QueryMessageAttribute')
  - [#ctor(channel,typeName,typeVersion,responseChannel,responseTimeoutMilliseconds,responseType)](#M-MQContract-Attributes-QueryMessageAttribute-#ctor-System-String,System-String,System-String,System-String,System-Int32,System-Type- 'MQContract.Attributes.QueryMessageAttribute.#ctor(System.String,System.String,System.String,System.String,System.Int32,System.Type)')
  - [ResponseChannel](#P-MQContract-Attributes-QueryMessageAttribute-ResponseChannel 'MQContract.Attributes.QueryMessageAttribute.ResponseChannel')
  - [ResponseTimeout](#P-MQContract-Attributes-QueryMessageAttribute-ResponseTimeout 'MQContract.Attributes.QueryMessageAttribute.ResponseTimeout')
  - [ResponseType](#P-MQContract-Attributes-QueryMessageAttribute-ResponseType 'MQContract.Attributes.QueryMessageAttribute.ResponseType')
- [QueryResponseMessage\`1](#T-MQContract-Messages-QueryResponseMessage`1 'MQContract.Messages.QueryResponseMessage`1')
  - [#ctor(Message,Headers)](#M-MQContract-Messages-QueryResponseMessage`1-#ctor-`0,System-Collections-Generic-Dictionary{System-String,System-String}- 'MQContract.Messages.QueryResponseMessage`1.#ctor(`0,System.Collections.Generic.Dictionary{System.String,System.String})')
  - [Headers](#P-MQContract-Messages-QueryResponseMessage`1-Headers 'MQContract.Messages.QueryResponseMessage`1.Headers')
  - [Message](#P-MQContract-Messages-QueryResponseMessage`1-Message 'MQContract.Messages.QueryResponseMessage`1.Message')
- [QueryResult\`1](#T-MQContract-Messages-QueryResult`1 'MQContract.Messages.QueryResult`1')
  - [#ctor(ID,Header,Result,Error)](#M-MQContract-Messages-QueryResult`1-#ctor-System-String,MQContract-Messages-MessageHeader,`0,MQContract-Messages-ErrorMessage- 'MQContract.Messages.QueryResult`1.#ctor(System.String,MQContract.Messages.MessageHeader,`0,MQContract.Messages.ErrorMessage)')
  - [Header](#P-MQContract-Messages-QueryResult`1-Header 'MQContract.Messages.QueryResult`1.Header')
  - [Result](#P-MQContract-Messages-QueryResult`1-Result 'MQContract.Messages.QueryResult`1.Result')
- [ReceivedInboxServiceMessage](#T-MQContract-Messages-ReceivedInboxServiceMessage 'MQContract.Messages.ReceivedInboxServiceMessage')
  - [#ctor(ID,MessageTypeID,Channel,Header,CorrelationID,Data,Acknowledge)](#M-MQContract-Messages-ReceivedInboxServiceMessage-#ctor-System-String,System-String,System-String,MQContract-Messages-MessageHeader,System-Guid,System-ReadOnlyMemory{System-Byte},System-Func{System-Threading-Tasks-ValueTask}- 'MQContract.Messages.ReceivedInboxServiceMessage.#ctor(System.String,System.String,System.String,MQContract.Messages.MessageHeader,System.Guid,System.ReadOnlyMemory{System.Byte},System.Func{System.Threading.Tasks.ValueTask})')
  - [CorrelationID](#P-MQContract-Messages-ReceivedInboxServiceMessage-CorrelationID 'MQContract.Messages.ReceivedInboxServiceMessage.CorrelationID')
- [ReceivedServiceMessage](#T-MQContract-Messages-ReceivedServiceMessage 'MQContract.Messages.ReceivedServiceMessage')
  - [#ctor(ID,MessageTypeID,Channel,Header,Data,Acknowledge)](#M-MQContract-Messages-ReceivedServiceMessage-#ctor-System-String,System-String,System-String,MQContract-Messages-MessageHeader,System-ReadOnlyMemory{System-Byte},System-Func{System-Threading-Tasks-ValueTask}- 'MQContract.Messages.ReceivedServiceMessage.#ctor(System.String,System.String,System.String,MQContract.Messages.MessageHeader,System.ReadOnlyMemory{System.Byte},System.Func{System.Threading.Tasks.ValueTask})')
  - [Acknowledge](#P-MQContract-Messages-ReceivedServiceMessage-Acknowledge 'MQContract.Messages.ReceivedServiceMessage.Acknowledge')
  - [ReceivedTimestamp](#P-MQContract-Messages-ReceivedServiceMessage-ReceivedTimestamp 'MQContract.Messages.ReceivedServiceMessage.ReceivedTimestamp')
- [ResilienceException](#T-MQContract-ResilienceException 'MQContract.ResilienceException')
  - [#ctor(type,error)](#M-MQContract-ResilienceException-#ctor-MQContract-ResilienceTypes,System-Exception- 'MQContract.ResilienceException.#ctor(MQContract.ResilienceTypes,System.Exception)')
  - [Type](#P-MQContract-ResilienceException-Type 'MQContract.ResilienceException.Type')
- [ResilienceTypes](#T-MQContract-ResilienceTypes 'MQContract.ResilienceTypes')
  - [CircuitBreak](#F-MQContract-ResilienceTypes-CircuitBreak 'MQContract.ResilienceTypes.CircuitBreak')
  - [Retry](#F-MQContract-ResilienceTypes-Retry 'MQContract.ResilienceTypes.Retry')
- [ServiceMessage](#T-MQContract-Messages-ServiceMessage 'MQContract.Messages.ServiceMessage')
  - [#ctor(ID,MessageTypeID,Channel,Header,Data)](#M-MQContract-Messages-ServiceMessage-#ctor-System-String,System-String,System-String,MQContract-Messages-MessageHeader,System-ReadOnlyMemory{System-Byte}- 'MQContract.Messages.ServiceMessage.#ctor(System.String,System.String,System.String,MQContract.Messages.MessageHeader,System.ReadOnlyMemory{System.Byte})')
  - [Channel](#P-MQContract-Messages-ServiceMessage-Channel 'MQContract.Messages.ServiceMessage.Channel')
  - [Data](#P-MQContract-Messages-ServiceMessage-Data 'MQContract.Messages.ServiceMessage.Data')
  - [Header](#P-MQContract-Messages-ServiceMessage-Header 'MQContract.Messages.ServiceMessage.Header')
  - [ID](#P-MQContract-Messages-ServiceMessage-ID 'MQContract.Messages.ServiceMessage.ID')
  - [MessageTypeID](#P-MQContract-Messages-ServiceMessage-MessageTypeID 'MQContract.Messages.ServiceMessage.MessageTypeID')
- [ServiceQueryResult](#T-MQContract-Messages-ServiceQueryResult 'MQContract.Messages.ServiceQueryResult')
  - [#ctor(ID,Header,MessageTypeID,Data)](#M-MQContract-Messages-ServiceQueryResult-#ctor-System-String,MQContract-Messages-MessageHeader,System-String,System-ReadOnlyMemory{System-Byte}- 'MQContract.Messages.ServiceQueryResult.#ctor(System.String,MQContract.Messages.MessageHeader,System.String,System.ReadOnlyMemory{System.Byte})')
  - [Data](#P-MQContract-Messages-ServiceQueryResult-Data 'MQContract.Messages.ServiceQueryResult.Data')
  - [Header](#P-MQContract-Messages-ServiceQueryResult-Header 'MQContract.Messages.ServiceQueryResult.Header')
  - [ID](#P-MQContract-Messages-ServiceQueryResult-ID 'MQContract.Messages.ServiceQueryResult.ID')
  - [MessageTypeID](#P-MQContract-Messages-ServiceQueryResult-MessageTypeID 'MQContract.Messages.ServiceQueryResult.MessageTypeID')
- [TransmissionException](#T-MQContract-TransmissionException 'MQContract.TransmissionException')
  - [#ctor(underlyingError,isFatal)](#M-MQContract-TransmissionException-#ctor-System-Exception,System-Boolean- 'MQContract.TransmissionException.#ctor(System.Exception,System.Boolean)')
  - [IsFatal](#P-MQContract-TransmissionException-IsFatal 'MQContract.TransmissionException.IsFatal')
- [TransmissionResult](#T-MQContract-Messages-TransmissionResult 'MQContract.Messages.TransmissionResult')
  - [#ctor(ID,Error)](#M-MQContract-Messages-TransmissionResult-#ctor-System-String,MQContract-Messages-ErrorMessage- 'MQContract.Messages.TransmissionResult.#ctor(System.String,MQContract.Messages.ErrorMessage)')
  - [Error](#P-MQContract-Messages-TransmissionResult-Error 'MQContract.Messages.TransmissionResult.Error')
  - [ID](#P-MQContract-Messages-TransmissionResult-ID 'MQContract.Messages.TransmissionResult.ID')
  - [IsError](#P-MQContract-Messages-TransmissionResult-IsError 'MQContract.Messages.TransmissionResult.IsError')
- [UseMqContractAttribute](#T-MQContract-Attributes-UseMqContractAttribute 'MQContract.Attributes.UseMqContractAttribute')
  - [#ctor(contractType,encoderType,converters,messageEncryptor)](#M-MQContract-Attributes-UseMqContractAttribute-#ctor-System-Type,System-Type,System-Type[],System-Type- 'MQContract.Attributes.UseMqContractAttribute.#ctor(System.Type,System.Type,System.Type[],System.Type)')
  - [ContractType](#P-MQContract-Attributes-UseMqContractAttribute-ContractType 'MQContract.Attributes.UseMqContractAttribute.ContractType')
  - [Converters](#P-MQContract-Attributes-UseMqContractAttribute-Converters 'MQContract.Attributes.UseMqContractAttribute.Converters')
  - [EncoderType](#P-MQContract-Attributes-UseMqContractAttribute-EncoderType 'MQContract.Attributes.UseMqContractAttribute.EncoderType')
  - [MessageEncryptor](#P-MQContract-Attributes-UseMqContractAttribute-MessageEncryptor 'MQContract.Attributes.UseMqContractAttribute.MessageEncryptor')

<a name='T-MQContract-Messages-ChildTransmissionResult'></a>
## ChildTransmissionResult `type`

##### Namespace

MQContract.Messages

##### Summary

Houses the result of a transmission into an underlying service with the corresponding name

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| ServiceName | [T:MQContract.Messages.ChildTransmissionResult](#T-T-MQContract-Messages-ChildTransmissionResult 'T:MQContract.Messages.ChildTransmissionResult') | The unique name of the underlying service that was used to transmit |

<a name='M-MQContract-Messages-ChildTransmissionResult-#ctor-System-String,MQContract-Messages-ErrorMessage-'></a>
### #ctor(ServiceName,Error) `constructor`

##### Summary

Houses the result of a transmission into an underlying service with the corresponding name

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| ServiceName | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | The unique name of the underlying service that was used to transmit |
| Error | [MQContract.Messages.ErrorMessage](#T-MQContract-Messages-ErrorMessage 'MQContract.Messages.ErrorMessage') | An error message if an error occured |

<a name='P-MQContract-Messages-ChildTransmissionResult-Error'></a>
### Error `property`

##### Summary

An error message if an error occured

<a name='P-MQContract-Messages-ChildTransmissionResult-IsError'></a>
### IsError `property`

##### Summary

Flag to indicate if the result is an error

<a name='P-MQContract-Messages-ChildTransmissionResult-ServiceName'></a>
### ServiceName `property`

##### Summary

The unique name of the underlying service that was used to transmit

<a name='T-MQContract-Attributes-ConsumerAttribute'></a>
## ConsumerAttribute `type`

##### Namespace

MQContract.Attributes

##### Summary

Use this attribute to define the Channel, Group and/or IngoreMessageTypeHeader flag
for a given Consumer

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| channel | [T:MQContract.Attributes.ConsumerAttribute](#T-T-MQContract-Attributes-ConsumerAttribute 'T:MQContract.Attributes.ConsumerAttribute') | The channel the consumer will listen on |

##### Example

```
[Consumer(channel: "Arrivals", group: "Group1")]
public class MyConsumer : IPubSubConsumer<ArrivalAnnouncement>
{
    public void MessageReceived(IReceivedMessage<ArrivalAnnouncement> message)
    {
        Console.WriteLine($"Received: {message.Message.FirstName}");
    }
}
```

<a name='M-MQContract-Attributes-ConsumerAttribute-#ctor-System-String,System-String,System-Boolean-'></a>
### #ctor(channel,group,ignoreMessageTypeHeader) `constructor`

##### Summary

Use this attribute to define the Channel, Group and/or IngoreMessageTypeHeader flag
for a given Consumer

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| channel | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | The channel the consumer will listen on |
| group | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | The group the consumer will register as |
| ignoreMessageTypeHeader | [System.Boolean](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Boolean 'System.Boolean') | A falg to indicate if ignoring the message type is desired |

##### Example

```
[Consumer(channel: "Arrivals", group: "Group1")]
public class MyConsumer : IPubSubConsumer<ArrivalAnnouncement>
{
    public void MessageReceived(IReceivedMessage<ArrivalAnnouncement> message)
    {
        Console.WriteLine($"Received: {message.Message.FirstName}");
    }
}
```

<a name='P-MQContract-Attributes-ConsumerAttribute-Channel'></a>
### Channel `property`

##### Summary

The channel to register the consumer on

<a name='P-MQContract-Attributes-ConsumerAttribute-Group'></a>
### Group `property`

##### Summary

The group to register the consumer to

<a name='P-MQContract-Attributes-ConsumerAttribute-IgnoreMessageTypeHeader'></a>
### IgnoreMessageTypeHeader `property`

##### Summary

Indicates if the message type should be ignored

<a name='T-MQContract-Extensions-ConsumerContractConnectionExtensions'></a>
## ConsumerContractConnectionExtensions `type`

##### Namespace

MQContract.Extensions

##### Summary

Houses the extension calls to allow for fluent consumer registrations

<a name='M-MQContract-Extensions-ConsumerContractConnectionExtensions-RegisterPubSubAsyncConsumerAsync``1-System-Threading-Tasks-ValueTask{MQContract-Interfaces-IConsumerContractConnection{``0}},System-Type,System-String,System-String,System-Boolean,System-Threading-CancellationToken-'></a>
### RegisterPubSubAsyncConsumerAsync\`\`1(connectionTask,consumerType,channel,group,ignoreMessageHeader,cancellationToken) `method`

##### Summary

Called to register a PubSubAsyncConsumer into the contract connection.

##### Returns

The Contract Connection instance to allow chaining calls

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| connectionTask | [System.Threading.Tasks.ValueTask{MQContract.Interfaces.IConsumerContractConnection{\`\`0}}](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Threading.Tasks.ValueTask 'System.Threading.Tasks.ValueTask{MQContract.Interfaces.IConsumerContractConnection{``0}}') | Original Registration task |
| consumerType | [System.Type](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Type 'System.Type') | The type instance to be constructed and registered into the system.  It must implement IPubSubAsyncConsumer<T>. |
| channel | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | Specifies the message channel to use.  The prefered method is using the MessageChannelAttribute on the Message class. |
| group | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | The subscription group if desired (typically used when multiple instances of the same system are running) |
| ignoreMessageHeader | [System.Boolean](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Boolean 'System.Boolean') | If true, the message type specified will be ignored and it will automatically attempt to convert the underlying message to the given class |
| cancellationToken | [System.Threading.CancellationToken](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Threading.CancellationToken 'System.Threading.CancellationToken') | A cancellation token |

##### Generic Types

| Name | Description |
| ---- | ----------- |
| TContractConnection | Contract Connection |

<a name='M-MQContract-Extensions-ConsumerContractConnectionExtensions-RegisterPubSubAsyncConsumerAsync``3-System-Threading-Tasks-ValueTask{MQContract-Interfaces-IConsumerContractConnection{``0}},``2,System-String,System-String,System-Boolean,MQContract-Messages-MessageFilters{``1},System-Threading-CancellationToken-'></a>
### RegisterPubSubAsyncConsumerAsync\`\`3(connectionTask,consumer,channel,group,ignoreMessageHeader,messageFilters,cancellationToken) `method`

##### Summary

Called to register a PubSubAsyncConsumer into the contract connection

##### Returns

The Contract Connection instance to allow chaining calls

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| connectionTask | [System.Threading.Tasks.ValueTask{MQContract.Interfaces.IConsumerContractConnection{\`\`0}}](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Threading.Tasks.ValueTask 'System.Threading.Tasks.ValueTask{MQContract.Interfaces.IConsumerContractConnection{``0}}') | Original Registration task |
| consumer | [\`\`2](#T-``2 '``2') | An instance of the consumer |
| channel | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | Specifies the message channel to use.  The prefered method is using the MessageChannelAttribute on the Message class. |
| group | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | The subscription group if desired (typically used when multiple instances of the same system are running) |
| ignoreMessageHeader | [System.Boolean](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Boolean 'System.Boolean') | If true, the message type specified will be ignored and it will automatically attempt to convert the underlying message to the given class |
| messageFilters | [MQContract.Messages.MessageFilters{\`\`1}](#T-MQContract-Messages-MessageFilters{``1} 'MQContract.Messages.MessageFilters{``1}') | Provides any filtering options for this subscription to filter out messages prior to action calls if desired |
| cancellationToken | [System.Threading.CancellationToken](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Threading.CancellationToken 'System.Threading.CancellationToken') | A cancellation token |

##### Generic Types

| Name | Description |
| ---- | ----------- |
| TContractConnection | Contract Connection |
| TMessage | The Message type |
| TConsumer | The type that implements IPubSubAsyncConsumer<TMessage> |

<a name='M-MQContract-Extensions-ConsumerContractConnectionExtensions-RegisterPubSubAsyncConsumerAsync``3-System-Threading-Tasks-ValueTask{MQContract-Interfaces-IConsumerContractConnection{``0}},System-String,System-String,System-Boolean,MQContract-Messages-MessageFilters{``1},System-Threading-CancellationToken-'></a>
### RegisterPubSubAsyncConsumerAsync\`\`3(connectionTask,channel,group,ignoreMessageHeader,messageFilters,cancellationToken) `method`

##### Summary

Called to register a PubSubAsyncConsumer into the contract connection.  This will create an instance of the TConsumer type that is requested and register it.

##### Returns

The Contract Connection instance to allow chaining calls

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| connectionTask | [System.Threading.Tasks.ValueTask{MQContract.Interfaces.IConsumerContractConnection{\`\`0}}](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Threading.Tasks.ValueTask 'System.Threading.Tasks.ValueTask{MQContract.Interfaces.IConsumerContractConnection{``0}}') | Original Registration task |
| channel | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | Specifies the message channel to use.  The prefered method is using the MessageChannelAttribute on the Message class. |
| group | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | The subscription group if desired (typically used when multiple instances of the same system are running) |
| ignoreMessageHeader | [System.Boolean](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Boolean 'System.Boolean') | If true, the message type specified will be ignored and it will automatically attempt to convert the underlying message to the given class |
| messageFilters | [MQContract.Messages.MessageFilters{\`\`1}](#T-MQContract-Messages-MessageFilters{``1} 'MQContract.Messages.MessageFilters{``1}') | Provides any filtering options for this subscription to filter out messages prior to action calls if desired |
| cancellationToken | [System.Threading.CancellationToken](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Threading.CancellationToken 'System.Threading.CancellationToken') | A cancellation token |

##### Generic Types

| Name | Description |
| ---- | ----------- |
| TContractConnection | Contract Connection |
| TMessage | The Message type |
| TConsumer | The type that implements IPubSubAsyncConsumer<TMessage> |

<a name='M-MQContract-Extensions-ConsumerContractConnectionExtensions-RegisterPubSubConsumerAsync``1-System-Threading-Tasks-ValueTask{MQContract-Interfaces-IConsumerContractConnection{``0}},System-Type,System-String,System-String,System-Boolean,System-Threading-CancellationToken-'></a>
### RegisterPubSubConsumerAsync\`\`1(connectionTask,consumerType,channel,group,ignoreMessageHeader,cancellationToken) `method`

##### Summary

Called to register a PubSubConsumer into the contract connection.

##### Returns

The Contract Connection instance to allow chaining calls

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| connectionTask | [System.Threading.Tasks.ValueTask{MQContract.Interfaces.IConsumerContractConnection{\`\`0}}](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Threading.Tasks.ValueTask 'System.Threading.Tasks.ValueTask{MQContract.Interfaces.IConsumerContractConnection{``0}}') | Original Registration task |
| consumerType | [System.Type](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Type 'System.Type') | The type instance to be constructed and registered into the system.  It must implement IPubSubConsumer<T>. |
| channel | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | Specifies the message channel to use.  The prefered method is using the MessageChannelAttribute on the Message class. |
| group | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | The subscription group if desired (typically used when multiple instances of the same system are running) |
| ignoreMessageHeader | [System.Boolean](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Boolean 'System.Boolean') | If true, the message type specified will be ignored and it will automatically attempt to convert the underlying message to the given class |
| cancellationToken | [System.Threading.CancellationToken](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Threading.CancellationToken 'System.Threading.CancellationToken') | A cancellation token |

##### Generic Types

| Name | Description |
| ---- | ----------- |
| TContractConnection | Contract Connection |

<a name='M-MQContract-Extensions-ConsumerContractConnectionExtensions-RegisterPubSubConsumerAsync``3-System-Threading-Tasks-ValueTask{MQContract-Interfaces-IConsumerContractConnection{``0}},``2,System-String,System-String,System-Boolean,MQContract-Messages-MessageFilters{``1},System-Threading-CancellationToken-'></a>
### RegisterPubSubConsumerAsync\`\`3(connectionTask,consumer,channel,group,ignoreMessageHeader,messageFilters,cancellationToken) `method`

##### Summary

Called to register a PubSubConsumer into the contract connection

##### Returns

The Contract Connection instance to allow chaining calls

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| connectionTask | [System.Threading.Tasks.ValueTask{MQContract.Interfaces.IConsumerContractConnection{\`\`0}}](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Threading.Tasks.ValueTask 'System.Threading.Tasks.ValueTask{MQContract.Interfaces.IConsumerContractConnection{``0}}') | Original Registration task |
| consumer | [\`\`2](#T-``2 '``2') | An instance of the consumer |
| channel | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | Specifies the message channel to use.  The prefered method is using the MessageChannelAttribute on the Message class. |
| group | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | The subscription group if desired (typically used when multiple instances of the same system are running) |
| ignoreMessageHeader | [System.Boolean](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Boolean 'System.Boolean') | If true, the message type specified will be ignored and it will automatically attempt to convert the underlying message to the given class |
| messageFilters | [MQContract.Messages.MessageFilters{\`\`1}](#T-MQContract-Messages-MessageFilters{``1} 'MQContract.Messages.MessageFilters{``1}') | Provides any filtering options for this subscription to filter out messages prior to action calls if desired |
| cancellationToken | [System.Threading.CancellationToken](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Threading.CancellationToken 'System.Threading.CancellationToken') | A cancellation token |

##### Generic Types

| Name | Description |
| ---- | ----------- |
| TContractConnection | Contract Connection |
| TMessage | The Message type |
| TConsumer | The type that implements IPubSubConsumer<TMessage> |

<a name='M-MQContract-Extensions-ConsumerContractConnectionExtensions-RegisterPubSubConsumerAsync``3-System-Threading-Tasks-ValueTask{MQContract-Interfaces-IConsumerContractConnection{``0}},System-String,System-String,System-Boolean,MQContract-Messages-MessageFilters{``1},System-Threading-CancellationToken-'></a>
### RegisterPubSubConsumerAsync\`\`3(connectionTask,channel,group,ignoreMessageHeader,messageFilters,cancellationToken) `method`

##### Summary

Called to register a PubSubConsumer into the contract connection.  This will create an instance of the TConsumer type that is requested and register it.

##### Returns

The Contract Connection instance to allow chaining calls

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| connectionTask | [System.Threading.Tasks.ValueTask{MQContract.Interfaces.IConsumerContractConnection{\`\`0}}](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Threading.Tasks.ValueTask 'System.Threading.Tasks.ValueTask{MQContract.Interfaces.IConsumerContractConnection{``0}}') | Original Registration task |
| channel | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | Specifies the message channel to use.  The prefered method is using the MessageChannelAttribute on the Message class. |
| group | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | The subscription group if desired (typically used when multiple instances of the same system are running) |
| ignoreMessageHeader | [System.Boolean](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Boolean 'System.Boolean') | If true, the message type specified will be ignored and it will automatically attempt to convert the underlying message to the given class |
| messageFilters | [MQContract.Messages.MessageFilters{\`\`1}](#T-MQContract-Messages-MessageFilters{``1} 'MQContract.Messages.MessageFilters{``1}') | Provides any filtering options for this subscription to filter out messages prior to action calls if desired |
| cancellationToken | [System.Threading.CancellationToken](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Threading.CancellationToken 'System.Threading.CancellationToken') | A cancellation token |

##### Generic Types

| Name | Description |
| ---- | ----------- |
| TContractConnection | Contract Connection |
| TMessage | The Message type |
| TConsumer | The type that implements IPubSubConsumer<TMessage> |

<a name='M-MQContract-Extensions-ConsumerContractConnectionExtensions-RegisterQueryResponseAsyncConsumerAsync``1-System-Threading-Tasks-ValueTask{MQContract-Interfaces-IConsumerContractConnection{``0}},System-Type,System-String,System-String,System-Boolean,System-Threading-CancellationToken-'></a>
### RegisterQueryResponseAsyncConsumerAsync\`\`1(connectionTask,consumerType,channel,group,ignoreMessageHeader,cancellationToken) `method`

##### Summary

Called to register a QueryResponseAsyncConsumer into the contract connection.

##### Returns

The Contract Connection instance to allow chaining calls

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| connectionTask | [System.Threading.Tasks.ValueTask{MQContract.Interfaces.IConsumerContractConnection{\`\`0}}](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Threading.Tasks.ValueTask 'System.Threading.Tasks.ValueTask{MQContract.Interfaces.IConsumerContractConnection{``0}}') | Original Registration task |
| consumerType | [System.Type](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Type 'System.Type') | The type instance to be constructed and registered into the system.  It must implement IQueryResponseAsyncConsumer<Q,R>. |
| channel | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | Specifies the message channel to use.  The prefered method is using the MessageChannelAttribute on the Message class. |
| group | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | The subscription group if desired (typically used when multiple instances of the same system are running) |
| ignoreMessageHeader | [System.Boolean](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Boolean 'System.Boolean') | If true, the message type specified will be ignored and it will automatically attempt to convert the underlying message to the given class |
| cancellationToken | [System.Threading.CancellationToken](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Threading.CancellationToken 'System.Threading.CancellationToken') | A cancellation token |

##### Generic Types

| Name | Description |
| ---- | ----------- |
| TContractConnection | Contract Connection |

<a name='M-MQContract-Extensions-ConsumerContractConnectionExtensions-RegisterQueryResponseAsyncConsumerAsync``4-System-Threading-Tasks-ValueTask{MQContract-Interfaces-IConsumerContractConnection{``0}},``3,System-String,System-String,System-Boolean,MQContract-Messages-MessageFilters{``1},System-Threading-CancellationToken-'></a>
### RegisterQueryResponseAsyncConsumerAsync\`\`4(connectionTask,consumer,channel,group,ignoreMessageHeader,messageFilters,cancellationToken) `method`

##### Summary

Called to register a QueryResponseAsyncConsumer into the contract connection

##### Returns

The Contract Connection instance to allow chaining calls

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| connectionTask | [System.Threading.Tasks.ValueTask{MQContract.Interfaces.IConsumerContractConnection{\`\`0}}](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Threading.Tasks.ValueTask 'System.Threading.Tasks.ValueTask{MQContract.Interfaces.IConsumerContractConnection{``0}}') | Original Registration task |
| consumer | [\`\`3](#T-``3 '``3') | An instance of the consumer |
| channel | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | Specifies the message channel to use.  The prefered method is using the MessageChannelAttribute on the Message class. |
| group | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | The subscription group if desired (typically used when multiple instances of the same system are running) |
| ignoreMessageHeader | [System.Boolean](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Boolean 'System.Boolean') | If true, the message type specified will be ignored and it will automatically attempt to convert the underlying message to the given class |
| messageFilters | [MQContract.Messages.MessageFilters{\`\`1}](#T-MQContract-Messages-MessageFilters{``1} 'MQContract.Messages.MessageFilters{``1}') | Provides any filtering options for this subscription to filter out messages prior to action calls if desired |
| cancellationToken | [System.Threading.CancellationToken](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Threading.CancellationToken 'System.Threading.CancellationToken') | A cancellation token |

##### Generic Types

| Name | Description |
| ---- | ----------- |
| TContractConnection | Contract Connection |
| TQuery | The type of message to listen for |
| TQueryResponse | The type of message to respond with |
| TConsumer | The type that implements IQueryResponseAsyncConsumer<TQuery,TQueryResponse> |

<a name='M-MQContract-Extensions-ConsumerContractConnectionExtensions-RegisterQueryResponseAsyncConsumerAsync``4-System-Threading-Tasks-ValueTask{MQContract-Interfaces-IConsumerContractConnection{``0}},System-String,System-String,System-Boolean,MQContract-Messages-MessageFilters{``1},System-Threading-CancellationToken-'></a>
### RegisterQueryResponseAsyncConsumerAsync\`\`4(connectionTask,channel,group,ignoreMessageHeader,messageFilters,cancellationToken) `method`

##### Summary

Called to register a QueryResponseAsyncConsumer into the contract connection.  This will create an instance of the TConsumer type that is requested and register it.

##### Returns

The Contract Connection instance to allow chaining calls

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| connectionTask | [System.Threading.Tasks.ValueTask{MQContract.Interfaces.IConsumerContractConnection{\`\`0}}](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Threading.Tasks.ValueTask 'System.Threading.Tasks.ValueTask{MQContract.Interfaces.IConsumerContractConnection{``0}}') | Original Registration task |
| channel | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | Specifies the message channel to use.  The prefered method is using the MessageChannelAttribute on the Message class. |
| group | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | The subscription group if desired (typically used when multiple instances of the same system are running) |
| ignoreMessageHeader | [System.Boolean](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Boolean 'System.Boolean') | If true, the message type specified will be ignored and it will automatically attempt to convert the underlying message to the given class |
| messageFilters | [MQContract.Messages.MessageFilters{\`\`1}](#T-MQContract-Messages-MessageFilters{``1} 'MQContract.Messages.MessageFilters{``1}') | Provides any filtering options for this subscription to filter out messages prior to action calls if desired |
| cancellationToken | [System.Threading.CancellationToken](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Threading.CancellationToken 'System.Threading.CancellationToken') | A cancellation token |

##### Generic Types

| Name | Description |
| ---- | ----------- |
| TContractConnection | Contract Connection |
| TQuery | The type of message to listen for |
| TQueryResponse | The type of message to respond with |
| TConsumer | The type that implements IQueryResponseAsyncConsumer<TQuery,TQueryResponse> |

<a name='M-MQContract-Extensions-ConsumerContractConnectionExtensions-RegisterQueryResponseConsumerAsync``1-System-Threading-Tasks-ValueTask{MQContract-Interfaces-IConsumerContractConnection{``0}},System-Type,System-String,System-String,System-Boolean,System-Threading-CancellationToken-'></a>
### RegisterQueryResponseConsumerAsync\`\`1(connectionTask,consumerType,channel,group,ignoreMessageHeader,cancellationToken) `method`

##### Summary

Called to register a QueryResponseConsumer into the contract connection.

##### Returns

The Contract Connection instance to allow chaining calls

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| connectionTask | [System.Threading.Tasks.ValueTask{MQContract.Interfaces.IConsumerContractConnection{\`\`0}}](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Threading.Tasks.ValueTask 'System.Threading.Tasks.ValueTask{MQContract.Interfaces.IConsumerContractConnection{``0}}') | Original Registration task |
| consumerType | [System.Type](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Type 'System.Type') | The type instance to be constructed and registered into the system.  It must implement IQueryResponseConsumer<Q,R>. |
| channel | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | Specifies the message channel to use.  The prefered method is using the MessageChannelAttribute on the Message class. |
| group | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | The subscription group if desired (typically used when multiple instances of the same system are running) |
| ignoreMessageHeader | [System.Boolean](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Boolean 'System.Boolean') | If true, the message type specified will be ignored and it will automatically attempt to convert the underlying message to the given class |
| cancellationToken | [System.Threading.CancellationToken](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Threading.CancellationToken 'System.Threading.CancellationToken') | A cancellation token |

##### Generic Types

| Name | Description |
| ---- | ----------- |
| TContractConnection | Contract Connection |

<a name='M-MQContract-Extensions-ConsumerContractConnectionExtensions-RegisterQueryResponseConsumerAsync``4-System-Threading-Tasks-ValueTask{MQContract-Interfaces-IConsumerContractConnection{``0}},``3,System-String,System-String,System-Boolean,MQContract-Messages-MessageFilters{``1},System-Threading-CancellationToken-'></a>
### RegisterQueryResponseConsumerAsync\`\`4(connectionTask,consumer,channel,group,ignoreMessageHeader,messageFilters,cancellationToken) `method`

##### Summary

Called to register a QueryResponseConsumer into the contract connection

##### Returns

The Contract Connection instance to allow chaining calls

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| connectionTask | [System.Threading.Tasks.ValueTask{MQContract.Interfaces.IConsumerContractConnection{\`\`0}}](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Threading.Tasks.ValueTask 'System.Threading.Tasks.ValueTask{MQContract.Interfaces.IConsumerContractConnection{``0}}') | Original Registration task |
| consumer | [\`\`3](#T-``3 '``3') | An instance of the consumer |
| channel | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | Specifies the message channel to use.  The prefered method is using the MessageChannelAttribute on the Message class. |
| group | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | The subscription group if desired (typically used when multiple instances of the same system are running) |
| ignoreMessageHeader | [System.Boolean](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Boolean 'System.Boolean') | If true, the message type specified will be ignored and it will automatically attempt to convert the underlying message to the given class |
| messageFilters | [MQContract.Messages.MessageFilters{\`\`1}](#T-MQContract-Messages-MessageFilters{``1} 'MQContract.Messages.MessageFilters{``1}') | Provides any filtering options for this subscription to filter out messages prior to action calls if desired |
| cancellationToken | [System.Threading.CancellationToken](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Threading.CancellationToken 'System.Threading.CancellationToken') | A cancellation token |

##### Generic Types

| Name | Description |
| ---- | ----------- |
| TContractConnection | Contract Connection |
| TQuery | The type of message to listen for |
| TQueryResponse | The type of message to respond with |
| TConsumer | The type that implements IQueryResponseConsumer<TQuery,TQueryResponse> |

<a name='M-MQContract-Extensions-ConsumerContractConnectionExtensions-RegisterQueryResponseConsumerAsync``4-System-Threading-Tasks-ValueTask{MQContract-Interfaces-IConsumerContractConnection{``0}},System-String,System-String,System-Boolean,MQContract-Messages-MessageFilters{``1},System-Threading-CancellationToken-'></a>
### RegisterQueryResponseConsumerAsync\`\`4(connectionTask,channel,group,ignoreMessageHeader,messageFilters,cancellationToken) `method`

##### Summary

Called to register a QueryResponseConsumer into the contract connection.  This will create an instance of the TConsumer type that is requested and register it.

##### Returns

The Contract Connection instance to allow chaining calls

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| connectionTask | [System.Threading.Tasks.ValueTask{MQContract.Interfaces.IConsumerContractConnection{\`\`0}}](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Threading.Tasks.ValueTask 'System.Threading.Tasks.ValueTask{MQContract.Interfaces.IConsumerContractConnection{``0}}') | Original Registration task |
| channel | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | Specifies the message channel to use.  The prefered method is using the MessageChannelAttribute on the Message class. |
| group | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | The subscription group if desired (typically used when multiple instances of the same system are running) |
| ignoreMessageHeader | [System.Boolean](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Boolean 'System.Boolean') | If true, the message type specified will be ignored and it will automatically attempt to convert the underlying message to the given class |
| messageFilters | [MQContract.Messages.MessageFilters{\`\`1}](#T-MQContract-Messages-MessageFilters{``1} 'MQContract.Messages.MessageFilters{``1}') | Provides any filtering options for this subscription to filter out messages prior to action calls if desired |
| cancellationToken | [System.Threading.CancellationToken](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Threading.CancellationToken 'System.Threading.CancellationToken') | A cancellation token |

##### Generic Types

| Name | Description |
| ---- | ----------- |
| TContractConnection | Contract Connection |
| TQuery | The type of message to listen for |
| TQueryResponse | The type of message to respond with |
| TConsumer | The type that implements IQueryResponseConsumer<TQuery,TQueryResponse> |

<a name='T-MQContract-Interfaces-Middleware-DecodableMessage'></a>
## DecodableMessage `type`

##### Namespace

MQContract.Interfaces.Middleware

##### Summary

Represents a decodable message that will run through the middleware

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| MessageHeader | [T:MQContract.Interfaces.Middleware.DecodableMessage](#T-T-MQContract-Interfaces-Middleware-DecodableMessage 'T:MQContract.Interfaces.Middleware.DecodableMessage') | The headers supplied with the message |

<a name='M-MQContract-Interfaces-Middleware-DecodableMessage-#ctor-MQContract-Messages-MessageHeader,System-ReadOnlyMemory{System-Byte}-'></a>
### #ctor(MessageHeader,Data) `constructor`

##### Summary

Represents a decodable message that will run through the middleware

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| MessageHeader | [MQContract.Messages.MessageHeader](#T-MQContract-Messages-MessageHeader 'MQContract.Messages.MessageHeader') | The headers supplied with the message |
| Data | [System.ReadOnlyMemory{System.Byte}](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.ReadOnlyMemory 'System.ReadOnlyMemory{System.Byte}') | The message data |

<a name='P-MQContract-Interfaces-Middleware-DecodableMessage-Data'></a>
### Data `property`

##### Summary

The message data

<a name='P-MQContract-Interfaces-Middleware-DecodableMessage-MessageHeader'></a>
### MessageHeader `property`

##### Summary

The headers supplied with the message

<a name='T-MQContract-Interfaces-Middleware-DecodedMessage`1'></a>
## DecodedMessage\`1 `type`

##### Namespace

MQContract.Interfaces.Middleware

##### Summary

Represents a decoded message that will run through the middleware

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| MessageHeader | [T:MQContract.Interfaces.Middleware.DecodedMessage\`1](#T-T-MQContract-Interfaces-Middleware-DecodedMessage`1 'T:MQContract.Interfaces.Middleware.DecodedMessage`1') | The headers supplied with the message |

##### Generic Types

| Name | Description |
| ---- | ----------- |
| TMessage | The type of message it is |

<a name='M-MQContract-Interfaces-Middleware-DecodedMessage`1-#ctor-MQContract-Messages-MessageHeader,`0-'></a>
### #ctor(MessageHeader,Message) `constructor`

##### Summary

Represents a decoded message that will run through the middleware

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| MessageHeader | [MQContract.Messages.MessageHeader](#T-MQContract-Messages-MessageHeader 'MQContract.Messages.MessageHeader') | The headers supplied with the message |
| Message | [\`0](#T-`0 '`0') | The decoded message |

##### Generic Types

| Name | Description |
| ---- | ----------- |
| TMessage | The type of message it is |

<a name='P-MQContract-Interfaces-Middleware-DecodedMessage`1-Message'></a>
### Message `property`

##### Summary

The decoded message

<a name='P-MQContract-Interfaces-Middleware-DecodedMessage`1-MessageHeader'></a>
### MessageHeader `property`

##### Summary

The headers supplied with the message

<a name='T-MQContract-Interfaces-Middleware-EncodableMessage`1'></a>
## EncodableMessage\`1 `type`

##### Namespace

MQContract.Interfaces.Middleware

##### Summary

Represents an encodable message that will run through the middleware

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| MessageHeader | [T:MQContract.Interfaces.Middleware.EncodableMessage\`1](#T-T-MQContract-Interfaces-Middleware-EncodableMessage`1 'T:MQContract.Interfaces.Middleware.EncodableMessage`1') | THe headers supplied with the message |

##### Generic Types

| Name | Description |
| ---- | ----------- |
| TMessage | The type of message it is |

<a name='M-MQContract-Interfaces-Middleware-EncodableMessage`1-#ctor-MQContract-Messages-MessageHeader,`0,System-String-'></a>
### #ctor(MessageHeader,Message,Channel) `constructor`

##### Summary

Represents an encodable message that will run through the middleware

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| MessageHeader | [MQContract.Messages.MessageHeader](#T-MQContract-Messages-MessageHeader 'MQContract.Messages.MessageHeader') | THe headers supplied with the message |
| Message | [\`0](#T-`0 '`0') | The message itself |
| Channel | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | The channel the message was request to go through |

##### Generic Types

| Name | Description |
| ---- | ----------- |
| TMessage | The type of message it is |

<a name='P-MQContract-Interfaces-Middleware-EncodableMessage`1-Channel'></a>
### Channel `property`

##### Summary

The channel the message was request to go through

<a name='P-MQContract-Interfaces-Middleware-EncodableMessage`1-Message'></a>
### Message `property`

##### Summary

The message itself

<a name='P-MQContract-Interfaces-Middleware-EncodableMessage`1-MessageHeader'></a>
### MessageHeader `property`

##### Summary

THe headers supplied with the message

<a name='T-MQContract-Interfaces-Encrypting-EncryptionResult'></a>
## EncryptionResult `type`

##### Namespace

MQContract.Interfaces.Encrypting

##### Summary

Houses the returned results from a message encryption call

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| Headers | [T:MQContract.Interfaces.Encrypting.EncryptionResult](#T-T-MQContract-Interfaces-Encrypting-EncryptionResult 'T:MQContract.Interfaces.Encrypting.EncryptionResult') | Any additional headers to add to the message |

<a name='M-MQContract-Interfaces-Encrypting-EncryptionResult-#ctor-System-Collections-Generic-Dictionary{System-String,System-String},System-Byte[]-'></a>
### #ctor(Headers,Data) `constructor`

##### Summary

Houses the returned results from a message encryption call

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| Headers | [System.Collections.Generic.Dictionary{System.String,System.String}](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Collections.Generic.Dictionary 'System.Collections.Generic.Dictionary{System.String,System.String}') | Any additional headers to add to the message |
| Data | [System.Byte[]](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Byte[] 'System.Byte[]') | The resulting encrypted data |

<a name='P-MQContract-Interfaces-Encrypting-EncryptionResult-Data'></a>
### Data `property`

##### Summary

The resulting encrypted data

<a name='P-MQContract-Interfaces-Encrypting-EncryptionResult-Headers'></a>
### Headers `property`

##### Summary

Any additional headers to add to the message

<a name='T-MQContract-Messages-ErrorMessage'></a>
## ErrorMessage `type`

##### Namespace

MQContract.Messages

##### Summary

Houses an Exception thrown that needs to be recorded as part of the tranmission result

<a name='M-MQContract-Messages-ErrorMessage-#ctor-System-Exception,System-Boolean-'></a>
### #ctor(exception,isFatal) `constructor`

##### Summary

Used to construct an instance of the Error object

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| exception | [System.Exception](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Exception 'System.Exception') | The error that occured |
| isFatal | [System.Boolean](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Boolean 'System.Boolean') | Used to indicate if the error was fatal.  There is also some additional checks inside that will set to Fatal if a given exception type is supplied. |

##### Remarks

The exceptions that will override and mark fatal are ObjectDisposedException, ArgumentNullException, ArgumentOutOfRangeException, OperationCanceledException, InvalidOperationException

<a name='P-MQContract-Messages-ErrorMessage-Exception'></a>
### Exception `property`

##### Summary

The exception that was thrown

<a name='P-MQContract-Messages-ErrorMessage-IsFatal'></a>
### IsFatal `property`

##### Summary

Used to indicate if it was Fatal or not.  If it is a Fatal exception, it will cause the Retry Policies to not be used

<a name='P-MQContract-Messages-ErrorMessage-Message'></a>
### Message `property`

##### Summary

The error message from the exception

<a name='T-MQContract-Interfaces-Middleware-IAfterDecodeMiddleware'></a>
## IAfterDecodeMiddleware `type`

##### Namespace

MQContract.Interfaces.Middleware

##### Summary

This interface represents a Middleware to execute after a Message has been decoded from a ServiceMessage to the expected Class

<a name='M-MQContract-Interfaces-Middleware-IAfterDecodeMiddleware-AfterMessageDecodeAsync``1-MQContract-Interfaces-Middleware-IContext,System-String,MQContract-Interfaces-Middleware-DecodedMessage{``0},System-DateTime,System-DateTime-'></a>
### AfterMessageDecodeAsync\`\`1(context,ID,message,receivedTimestamp,processedTimeStamp) `method`

##### Summary

This is the method invoked as part of the Middleware processing during message decoding

##### Returns

The message and header to allow for changes if desired

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| context | [MQContract.Interfaces.Middleware.IContext](#T-MQContract-Interfaces-Middleware-IContext 'MQContract.Interfaces.Middleware.IContext') | A shared context that exists from the start of this decode process instance |
| ID | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | The id of the message |
| message | [MQContract.Interfaces.Middleware.DecodedMessage{\`\`0}](#T-MQContract-Interfaces-Middleware-DecodedMessage{``0} 'MQContract.Interfaces.Middleware.DecodedMessage{``0}') | The Decoded service message that includes both the message and headers |
| receivedTimestamp | [System.DateTime](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.DateTime 'System.DateTime') | The timestamp of when the message was recieved |
| processedTimeStamp | [System.DateTime](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.DateTime 'System.DateTime') | The timestamp of when the message was decoded into a Class |

##### Generic Types

| Name | Description |
| ---- | ----------- |
| TMessage | This will be the type of the Message that was decoded |

<a name='T-MQContract-Interfaces-Middleware-IAfterDecodeSpecificTypeMiddleware`1'></a>
## IAfterDecodeSpecificTypeMiddleware\`1 `type`

##### Namespace

MQContract.Interfaces.Middleware

##### Summary

This interface represents a Middleware to execute after a Message of the given type T has been decoded from a ServiceMessage to the expected Class

<a name='M-MQContract-Interfaces-Middleware-IAfterDecodeSpecificTypeMiddleware`1-AfterMessageDecodeAsync-MQContract-Interfaces-Middleware-IContext,System-String,MQContract-Interfaces-Middleware-DecodedMessage{`0},System-DateTime,System-DateTime-'></a>
### AfterMessageDecodeAsync(context,ID,message,receivedTimestamp,processedTimeStamp) `method`

##### Summary

This is the method invoked as part of the Middleware processing during message decoding

##### Returns

The message and header to allow for changes if desired

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| context | [MQContract.Interfaces.Middleware.IContext](#T-MQContract-Interfaces-Middleware-IContext 'MQContract.Interfaces.Middleware.IContext') | A shared context that exists from the start of this decode process instance |
| ID | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | The id of the message |
| message | [MQContract.Interfaces.Middleware.DecodedMessage{\`0}](#T-MQContract-Interfaces-Middleware-DecodedMessage{`0} 'MQContract.Interfaces.Middleware.DecodedMessage{`0}') | The decoded message including the headers |
| receivedTimestamp | [System.DateTime](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.DateTime 'System.DateTime') | The timestamp of when the message was recieved |
| processedTimeStamp | [System.DateTime](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.DateTime 'System.DateTime') | The timestamp of when the message was decoded into a Class |

<a name='T-MQContract-Interfaces-Middleware-IAfterEncodeMiddleware'></a>
## IAfterEncodeMiddleware `type`

##### Namespace

MQContract.Interfaces.Middleware

##### Summary

This interface represents a Middleware to execute after a Message has been encoded to a ServiceMessage from the supplied Class

<a name='M-MQContract-Interfaces-Middleware-IAfterEncodeMiddleware-AfterMessageEncodeAsync-System-Type,MQContract-Interfaces-Middleware-IContext,MQContract-Messages-ServiceMessage-'></a>
### AfterMessageEncodeAsync(messageType,context,message) `method`

##### Summary

This is the method invoked as part of the Middleware processing during message encoding

##### Returns

The message to allow for changes if desired

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| messageType | [System.Type](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Type 'System.Type') | The class of the message type that was encoded |
| context | [MQContract.Interfaces.Middleware.IContext](#T-MQContract-Interfaces-Middleware-IContext 'MQContract.Interfaces.Middleware.IContext') | A shared context that exists from the start of this encode process instance |
| message | [MQContract.Messages.ServiceMessage](#T-MQContract-Messages-ServiceMessage 'MQContract.Messages.ServiceMessage') | The resulting encoded message |

<a name='T-MQContract-Interfaces-Consumers-IBaseConsumer'></a>
## IBaseConsumer `type`

##### Namespace

MQContract.Interfaces.Consumers

##### Summary

Represents the Base for all Consumer interfaces and contains the common method definition

<a name='M-MQContract-Interfaces-Consumers-IBaseConsumer-ErrorRecieved-System-Exception-'></a>
### ErrorRecieved(error) `method`

##### Summary

Called when an error is received from within the underlying subscription that is using this Consumer

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| error | [System.Exception](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Exception 'System.Exception') | The error that occured |

<a name='T-MQContract-Interfaces-IBaseContractConnection'></a>
## IBaseContractConnection `type`

##### Namespace

MQContract.Interfaces

##### Summary

Represents the Base for all Contract Connections and contains the definition of all items defined by all Contract Connections

<a name='P-MQContract-Interfaces-IBaseContractConnection-HealthCheck'></a>
### HealthCheck `property`

##### Summary

Provides a usable HealthCheck implementation to provide HealthCheck information for the given Contract Connection

<a name='M-MQContract-Interfaces-IBaseContractConnection-CloseAsync'></a>
### CloseAsync() `method`

##### Summary

Called to close off the contract connection and close it's underlying service connection

##### Returns

A task for the closure of the connection

##### Parameters

This method has no parameters.

<a name='M-MQContract-Interfaces-IBaseContractConnection-SubscribeAsync``1-System-Func{MQContract-Interfaces-IReceivedMessage{``0},System-Threading-Tasks-ValueTask},System-Action{System-Exception},System-String,System-String,System-Boolean,MQContract-Messages-MessageFilters{``0},System-Threading-CancellationToken-'></a>
### SubscribeAsync\`\`1(messageReceived,errorReceived,channel,group,ignoreMessageHeader,messageFilters,cancellationToken) `method`

##### Summary

Called to create a subscription into the underlying service Pub/Sub style and have the messages processed asynchronously

##### Returns

A subscription instance that can be ended when desired

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| messageReceived | [System.Func{MQContract.Interfaces.IReceivedMessage{\`\`0},System.Threading.Tasks.ValueTask}](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Func 'System.Func{MQContract.Interfaces.IReceivedMessage{``0},System.Threading.Tasks.ValueTask}') | The callback invoked when a new message is received |
| errorReceived | [System.Action{System.Exception}](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Action 'System.Action{System.Exception}') | The callback to invoke when an error occurs |
| channel | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | Specifies the message channel to use.  The prefered method is using the MessageChannelAttribute on the class. |
| group | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | The subscription group if desired (typically used when multiple instances of the same system are running) |
| ignoreMessageHeader | [System.Boolean](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Boolean 'System.Boolean') | If true, the message type specified will be ignored and it will automatically attempt to convert the underlying message to the given class |
| messageFilters | [MQContract.Messages.MessageFilters{\`\`0}](#T-MQContract-Messages-MessageFilters{``0} 'MQContract.Messages.MessageFilters{``0}') | Provides any filtering options for this subscription to filter out messages prior to action calls if desired |
| cancellationToken | [System.Threading.CancellationToken](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Threading.CancellationToken 'System.Threading.CancellationToken') | A cancellation token |

##### Generic Types

| Name | Description |
| ---- | ----------- |
| TMessage | The type of message to listen for |

##### Example

```
await contractConnection.SubscribeAsync<ArrivalAnnouncement>(
    (message) => {
        Console.WriteLine($"Arrival: {message.Message.FirstName}");
        return ValueTask.CompletedTask;
    },
    (error) => Console.WriteLine($"Error: {error.Message}")
);
```

<a name='M-MQContract-Interfaces-IBaseContractConnection-SubscribeAsync``1-System-Action{MQContract-Interfaces-IReceivedMessage{``0}},System-Action{System-Exception},System-String,System-String,System-Boolean,MQContract-Messages-MessageFilters{``0},System-Threading-CancellationToken-'></a>
### SubscribeAsync\`\`1(messageReceived,errorReceived,channel,group,ignoreMessageHeader,messageFilters,cancellationToken) `method`

##### Summary

Called to create a subscription into the underlying service Pub/Sub style and have the messages processed syncrhonously

##### Returns

A subscription instance that can be ended when desired

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| messageReceived | [System.Action{MQContract.Interfaces.IReceivedMessage{\`\`0}}](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Action 'System.Action{MQContract.Interfaces.IReceivedMessage{``0}}') | The callback invoked when a new message is received |
| errorReceived | [System.Action{System.Exception}](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Action 'System.Action{System.Exception}') | The callback to invoke when an error occurs |
| channel | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | Specifies the message channel to use.  The prefered method is using the MessageChannelAttribute on the class. |
| group | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | The subscription group if desired (typically used when multiple instances of the same system are running) |
| ignoreMessageHeader | [System.Boolean](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Boolean 'System.Boolean') | If true, the message type specified will be ignored and it will automatically attempt to convert the underlying message to the given class |
| messageFilters | [MQContract.Messages.MessageFilters{\`\`0}](#T-MQContract-Messages-MessageFilters{``0} 'MQContract.Messages.MessageFilters{``0}') | Provides any filtering options for this subscription to filter out messages prior to action calls if desired |
| cancellationToken | [System.Threading.CancellationToken](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Threading.CancellationToken 'System.Threading.CancellationToken') | A cancellation token |

##### Generic Types

| Name | Description |
| ---- | ----------- |
| TMessage | The type of message to listen for |

<a name='M-MQContract-Interfaces-IBaseContractConnection-SubscribeQueryAsyncResponseAsync``2-System-Func{MQContract-Interfaces-IReceivedMessage{``0},System-Threading-Tasks-ValueTask{MQContract-Messages-QueryResponseMessage{``1}}},System-Action{System-Exception},System-String,System-String,System-Boolean,MQContract-Messages-MessageFilters{``0},System-Threading-CancellationToken-'></a>
### SubscribeQueryAsyncResponseAsync\`\`2(messageReceived,errorReceived,channel,group,ignoreMessageHeader,messageFilters,cancellationToken) `method`

##### Summary

Called to create a subscription into the underlying service Query/Reponse style and have the messages processed asynchronously

##### Returns

A subscription instance that can be ended when desired

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| messageReceived | [System.Func{MQContract.Interfaces.IReceivedMessage{\`\`0},System.Threading.Tasks.ValueTask{MQContract.Messages.QueryResponseMessage{\`\`1}}}](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Func 'System.Func{MQContract.Interfaces.IReceivedMessage{``0},System.Threading.Tasks.ValueTask{MQContract.Messages.QueryResponseMessage{``1}}}') | The callback invoked when a new message is received expecting a response of the type response |
| errorReceived | [System.Action{System.Exception}](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Action 'System.Action{System.Exception}') | The callback invoked when an error occurs. |
| channel | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | Specifies the message channel to use.  The prefered method is using the MessageChannelAttribute on the class. |
| group | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | The subscription group if desired (typically used when multiple instances of the same system are running) |
| ignoreMessageHeader | [System.Boolean](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Boolean 'System.Boolean') | If true, the message type specified will be ignored and it will automatically attempt to convert the underlying message to the given class |
| messageFilters | [MQContract.Messages.MessageFilters{\`\`0}](#T-MQContract-Messages-MessageFilters{``0} 'MQContract.Messages.MessageFilters{``0}') | Provides any filtering options for this subscription to filter out messages prior to action calls if desired |
| cancellationToken | [System.Threading.CancellationToken](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Threading.CancellationToken 'System.Threading.CancellationToken') | A cancellation token |

##### Generic Types

| Name | Description |
| ---- | ----------- |
| TQuery | The type of message to listen for |
| TQueryResponse | The type of message to respond with |

<a name='M-MQContract-Interfaces-IBaseContractConnection-SubscribeQueryResponseAsync``2-System-Func{MQContract-Interfaces-IReceivedMessage{``0},MQContract-Messages-QueryResponseMessage{``1}},System-Action{System-Exception},System-String,System-String,System-Boolean,MQContract-Messages-MessageFilters{``0},System-Threading-CancellationToken-'></a>
### SubscribeQueryResponseAsync\`\`2(messageReceived,errorReceived,channel,group,ignoreMessageHeader,messageFilters,cancellationToken) `method`

##### Summary

Called to create a subscription into the underlying service Query/Reponse style and have the messages processed synchronously

##### Returns

A subscription instance that can be ended when desired

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| messageReceived | [System.Func{MQContract.Interfaces.IReceivedMessage{\`\`0},MQContract.Messages.QueryResponseMessage{\`\`1}}](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Func 'System.Func{MQContract.Interfaces.IReceivedMessage{``0},MQContract.Messages.QueryResponseMessage{``1}}') | The callback invoked when a new message is received expecting a response of the type response |
| errorReceived | [System.Action{System.Exception}](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Action 'System.Action{System.Exception}') | The callback invoked when an error occurs. |
| channel | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | Specifies the message channel to use.  The prefered method is using the MessageChannelAttribute on the class. |
| group | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | The subscription group if desired (typically used when multiple instances of the same system are running) |
| ignoreMessageHeader | [System.Boolean](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Boolean 'System.Boolean') | If true, the message type specified will be ignored and it will automatically attempt to convert the underlying message to the given class |
| messageFilters | [MQContract.Messages.MessageFilters{\`\`0}](#T-MQContract-Messages-MessageFilters{``0} 'MQContract.Messages.MessageFilters{``0}') | Provides any filtering options for this subscription to filter out messages prior to action calls if desired |
| cancellationToken | [System.Threading.CancellationToken](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Threading.CancellationToken 'System.Threading.CancellationToken') | A cancellation token |

##### Generic Types

| Name | Description |
| ---- | ----------- |
| TQuery | The type of message to listen for |
| TQueryResponse | The type of message to respond with |

<a name='T-MQContract-Interfaces-Middleware-IBeforeDecodeMiddleware'></a>
## IBeforeDecodeMiddleware `type`

##### Namespace

MQContract.Interfaces.Middleware

##### Summary

This interface represents a Middleware to execute before decoding a ServiceMessage

<a name='M-MQContract-Interfaces-Middleware-IBeforeDecodeMiddleware-BeforeMessageDecodeAsync-MQContract-Interfaces-Middleware-IContext,System-String,System-String,System-String,MQContract-Interfaces-Middleware-DecodableMessage-'></a>
### BeforeMessageDecodeAsync(context,id,messageTypeID,messageChannel,message) `method`

##### Summary

This is the method invoked as part of the Middleware processing prior to the message decoding

##### Returns

The message header and data to allow for changes if desired

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| context | [MQContract.Interfaces.Middleware.IContext](#T-MQContract-Interfaces-Middleware-IContext 'MQContract.Interfaces.Middleware.IContext') | A shared context that exists from the start of this decode process instance |
| id | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | The id of the message |
| messageTypeID | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | The message type id |
| messageChannel | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | The channel the message was recieved on |
| message | [MQContract.Interfaces.Middleware.DecodableMessage](#T-MQContract-Interfaces-Middleware-DecodableMessage 'MQContract.Interfaces.Middleware.DecodableMessage') | The decodable message housing headers and the data |

<a name='T-MQContract-Interfaces-Middleware-IBeforeEncodeMiddleware'></a>
## IBeforeEncodeMiddleware `type`

##### Namespace

MQContract.Interfaces.Middleware

##### Summary

This interface represents a Middleware to execute Before a message is encoded

<a name='M-MQContract-Interfaces-Middleware-IBeforeEncodeMiddleware-BeforeMessageEncodeAsync``1-MQContract-Interfaces-Middleware-IContext,MQContract-Interfaces-Middleware-EncodableMessage{``0}-'></a>
### BeforeMessageEncodeAsync\`\`1(context,message) `method`

##### Summary

This is the method invoked as part of the Middle Ware processing during message encoding

##### Returns

The message, channel and header to allow for changes if desired

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| context | [MQContract.Interfaces.Middleware.IContext](#T-MQContract-Interfaces-Middleware-IContext 'MQContract.Interfaces.Middleware.IContext') | A shared context that exists from the start of this encoding instance |
| message | [MQContract.Interfaces.Middleware.EncodableMessage{\`\`0}](#T-MQContract-Interfaces-Middleware-EncodableMessage{``0} 'MQContract.Interfaces.Middleware.EncodableMessage{``0}') | The message being encoded including headers and channel |

##### Generic Types

| Name | Description |
| ---- | ----------- |
| TMessage | The type of message being processed |

<a name='T-MQContract-Interfaces-Middleware-IBeforeEncodeSpecificTypeMiddleware`1'></a>
## IBeforeEncodeSpecificTypeMiddleware\`1 `type`

##### Namespace

MQContract.Interfaces.Middleware

##### Summary

This interface represents a Middleware to execute Before a specific message type is encoded

<a name='M-MQContract-Interfaces-Middleware-IBeforeEncodeSpecificTypeMiddleware`1-BeforeMessageEncodeAsync-MQContract-Interfaces-Middleware-IContext,MQContract-Interfaces-Middleware-EncodableMessage{`0}-'></a>
### BeforeMessageEncodeAsync(context,message) `method`

##### Summary

This is the method invoked as part of the Middle Ware processing during message encoding

##### Returns

The message, channel and header to allow for changes if desired

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| context | [MQContract.Interfaces.Middleware.IContext](#T-MQContract-Interfaces-Middleware-IContext 'MQContract.Interfaces.Middleware.IContext') | A shared context that exists from the start of this encoding instance |
| message | [MQContract.Interfaces.Middleware.EncodableMessage{\`0}](#T-MQContract-Interfaces-Middleware-EncodableMessage{`0} 'MQContract.Interfaces.Middleware.EncodableMessage{`0}') | The message being encoded including headers and channel |

<a name='T-MQContract-Interfaces-Service-IBulkPublishableMessageServiceConnection'></a>
## IBulkPublishableMessageServiceConnection `type`

##### Namespace

MQContract.Interfaces.Service

##### Summary

Used to implement a service that supports bulk message publishing

<a name='M-MQContract-Interfaces-Service-IBulkPublishableMessageServiceConnection-BulkPublishAsync-System-Collections-Generic-IEnumerable{MQContract-Messages-ServiceMessage},System-Threading-CancellationToken-'></a>
### BulkPublishAsync(messages,cancellationToken) `method`

##### Summary

Implements a publish call to publish the given messages in bulk

##### Returns

A transmission result instance indicating the result for each message

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| messages | [System.Collections.Generic.IEnumerable{MQContract.Messages.ServiceMessage}](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Collections.Generic.IEnumerable 'System.Collections.Generic.IEnumerable{MQContract.Messages.ServiceMessage}') | The message to publish |
| cancellationToken | [System.Threading.CancellationToken](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Threading.CancellationToken 'System.Threading.CancellationToken') | A cancellation token |

<a name='T-MQContract-Interfaces-IConsumerContractConnection`1'></a>
## IConsumerContractConnection\`1 `type`

##### Namespace

MQContract.Interfaces

##### Summary

This interface represents a portion of the Contract Connection, specifically the portion for registering all Consumer classes

<a name='M-MQContract-Interfaces-IConsumerContractConnection`1-AutoRegisterAllConsumersAsync-System-Reflection-Assembly,System-Threading-CancellationToken-'></a>
### AutoRegisterAllConsumersAsync(assembly,cancellationToken) `method`

##### Summary

Called to load all defined consumers found within the application, either within the supplied assembly or if null within the default LoadContext

##### Returns

The Contract Connection instance to allow chaining calls

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| assembly | [System.Reflection.Assembly](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Reflection.Assembly 'System.Reflection.Assembly') | Optional parameter to specify loading from a single assembly, if not supplied will load all from within the default LoadContext |
| cancellationToken | [System.Threading.CancellationToken](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Threading.CancellationToken 'System.Threading.CancellationToken') | A cancellation token |

<a name='M-MQContract-Interfaces-IConsumerContractConnection`1-RegisterPubSubAsyncConsumerAsync-System-Type,System-String,System-String,System-Boolean,System-Threading-CancellationToken-'></a>
### RegisterPubSubAsyncConsumerAsync(consumerType,channel,group,ignoreMessageHeader,cancellationToken) `method`

##### Summary

Called to register a PubSubAsyncConsumer into the contract connection.

##### Returns

The Contract Connection instance to allow chaining calls

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| consumerType | [System.Type](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Type 'System.Type') | The type instance to be constructed and registered into the system.  It must implement IPubSubAsyncConsumer<T>. |
| channel | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | Specifies the message channel to use.  The prefered method is using the MessageChannelAttribute on the Message class. |
| group | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | The subscription group if desired (typically used when multiple instances of the same system are running) |
| ignoreMessageHeader | [System.Boolean](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Boolean 'System.Boolean') | If true, the message type specified will be ignored and it will automatically attempt to convert the underlying message to the given class |
| cancellationToken | [System.Threading.CancellationToken](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Threading.CancellationToken 'System.Threading.CancellationToken') | A cancellation token |

<a name='M-MQContract-Interfaces-IConsumerContractConnection`1-RegisterPubSubAsyncConsumerAsync``2-``1,System-String,System-String,System-Boolean,MQContract-Messages-MessageFilters{``0},System-Threading-CancellationToken-'></a>
### RegisterPubSubAsyncConsumerAsync\`\`2(consumer,channel,group,ignoreMessageHeader,messageFilters,cancellationToken) `method`

##### Summary

Called to register a PubSubAsyncConsumer into the contract connection

##### Returns

The Contract Connection instance to allow chaining calls

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| consumer | [\`\`1](#T-``1 '``1') | An instance of the consumer |
| channel | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | Specifies the message channel to use.  The prefered method is using the MessageChannelAttribute on the Message class. |
| group | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | The subscription group if desired (typically used when multiple instances of the same system are running) |
| ignoreMessageHeader | [System.Boolean](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Boolean 'System.Boolean') | If true, the message type specified will be ignored and it will automatically attempt to convert the underlying message to the given class |
| messageFilters | [MQContract.Messages.MessageFilters{\`\`0}](#T-MQContract-Messages-MessageFilters{``0} 'MQContract.Messages.MessageFilters{``0}') | Provides any filtering options for this subscription to filter out messages prior to action calls if desired |
| cancellationToken | [System.Threading.CancellationToken](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Threading.CancellationToken 'System.Threading.CancellationToken') | A cancellation token |

##### Generic Types

| Name | Description |
| ---- | ----------- |
| TMessage | The Message type |
| TConsumer | The type that implements PubSubAsyncConsumer<TMessage> |

<a name='M-MQContract-Interfaces-IConsumerContractConnection`1-RegisterPubSubAsyncConsumerAsync``2-System-String,System-String,System-Boolean,MQContract-Messages-MessageFilters{``0},System-Threading-CancellationToken-'></a>
### RegisterPubSubAsyncConsumerAsync\`\`2(channel,group,ignoreMessageHeader,messageFilters,cancellationToken) `method`

##### Summary

Called to register a PubSubAsyncConsumer into the contract connection.  This will create an instance of the TConsumer type that is requested and register it.

##### Returns

The Contract Connection instance to allow chaining calls

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| channel | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | Specifies the message channel to use.  The prefered method is using the MessageChannelAttribute on the Message class. |
| group | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | The subscription group if desired (typically used when multiple instances of the same system are running) |
| ignoreMessageHeader | [System.Boolean](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Boolean 'System.Boolean') | If true, the message type specified will be ignored and it will automatically attempt to convert the underlying message to the given class |
| messageFilters | [MQContract.Messages.MessageFilters{\`\`0}](#T-MQContract-Messages-MessageFilters{``0} 'MQContract.Messages.MessageFilters{``0}') | Provides any filtering options for this subscription to filter out messages prior to action calls if desired |
| cancellationToken | [System.Threading.CancellationToken](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Threading.CancellationToken 'System.Threading.CancellationToken') | A cancellation token |

##### Generic Types

| Name | Description |
| ---- | ----------- |
| TMessage | The Message type |
| TConsumer | The type that implements PubSubAsyncConsumer<TMessage> |

<a name='M-MQContract-Interfaces-IConsumerContractConnection`1-RegisterPubSubConsumerAsync-System-Type,System-String,System-String,System-Boolean,System-Threading-CancellationToken-'></a>
### RegisterPubSubConsumerAsync(consumerType,channel,group,ignoreMessageHeader,cancellationToken) `method`

##### Summary

Called to register a PubSubConsumer into the contract connection.

##### Returns

The Contract Connection instance to allow chaining calls

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| consumerType | [System.Type](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Type 'System.Type') | The type instance to be constructed and registered into the system.  It must implement IPubSubConsumer<T>. |
| channel | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | Specifies the message channel to use.  The prefered method is using the MessageChannelAttribute on the Message class. |
| group | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | The subscription group if desired (typically used when multiple instances of the same system are running) |
| ignoreMessageHeader | [System.Boolean](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Boolean 'System.Boolean') | If true, the message type specified will be ignored and it will automatically attempt to convert the underlying message to the given class |
| cancellationToken | [System.Threading.CancellationToken](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Threading.CancellationToken 'System.Threading.CancellationToken') | A cancellation token |

<a name='M-MQContract-Interfaces-IConsumerContractConnection`1-RegisterPubSubConsumerAsync``2-``1,System-String,System-String,System-Boolean,MQContract-Messages-MessageFilters{``0},System-Threading-CancellationToken-'></a>
### RegisterPubSubConsumerAsync\`\`2(consumer,channel,group,ignoreMessageHeader,messageFilters,cancellationToken) `method`

##### Summary

Called to register a PubSubConsumer into the contract connection

##### Returns

The Contract Connection instance to allow chaining calls

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| consumer | [\`\`1](#T-``1 '``1') | An instance of the consumer |
| channel | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | Specifies the message channel to use.  The prefered method is using the MessageChannelAttribute on the Message class. |
| group | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | The subscription group if desired (typically used when multiple instances of the same system are running) |
| ignoreMessageHeader | [System.Boolean](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Boolean 'System.Boolean') | If true, the message type specified will be ignored and it will automatically attempt to convert the underlying message to the given class |
| messageFilters | [MQContract.Messages.MessageFilters{\`\`0}](#T-MQContract-Messages-MessageFilters{``0} 'MQContract.Messages.MessageFilters{``0}') | Provides any filtering options for this subscription to filter out messages prior to action calls if desired |
| cancellationToken | [System.Threading.CancellationToken](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Threading.CancellationToken 'System.Threading.CancellationToken') | A cancellation token |

##### Generic Types

| Name | Description |
| ---- | ----------- |
| TMessage | The Message type |
| TConsumer | The type that implements IPubSubConsumer<TMessage> |

<a name='M-MQContract-Interfaces-IConsumerContractConnection`1-RegisterPubSubConsumerAsync``2-System-String,System-String,System-Boolean,MQContract-Messages-MessageFilters{``0},System-Threading-CancellationToken-'></a>
### RegisterPubSubConsumerAsync\`\`2(channel,group,ignoreMessageHeader,messageFilters,cancellationToken) `method`

##### Summary

Called to register a PubSubConsumer into the contract connection.  This will create an instance of the TConsumer type that is requested and register it.

##### Returns

The Contract Connection instance to allow chaining calls

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| channel | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | Specifies the message channel to use.  The prefered method is using the MessageChannelAttribute on the Message class. |
| group | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | The subscription group if desired (typically used when multiple instances of the same system are running) |
| ignoreMessageHeader | [System.Boolean](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Boolean 'System.Boolean') | If true, the message type specified will be ignored and it will automatically attempt to convert the underlying message to the given class |
| messageFilters | [MQContract.Messages.MessageFilters{\`\`0}](#T-MQContract-Messages-MessageFilters{``0} 'MQContract.Messages.MessageFilters{``0}') | Provides any filtering options for this subscription to filter out messages prior to action calls if desired |
| cancellationToken | [System.Threading.CancellationToken](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Threading.CancellationToken 'System.Threading.CancellationToken') | A cancellation token |

##### Generic Types

| Name | Description |
| ---- | ----------- |
| TMessage | The Message type |
| TConsumer | The type that implements IPubSubConsumer<TMessage> |

<a name='M-MQContract-Interfaces-IConsumerContractConnection`1-RegisterQueryResponseAsyncConsumerAsync-System-Type,System-String,System-String,System-Boolean,System-Threading-CancellationToken-'></a>
### RegisterQueryResponseAsyncConsumerAsync(consumerType,channel,group,ignoreMessageHeader,cancellationToken) `method`

##### Summary

Called to register a QueryResponseAsyncConsumer into the contract connection.

##### Returns

The Contract Connection instance to allow chaining calls

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| consumerType | [System.Type](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Type 'System.Type') | The type instance to be constructed and registered into the system.  It must implement IQueryResponseAsyncConsumer<Q,R>. |
| channel | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | Specifies the message channel to use.  The prefered method is using the MessageChannelAttribute on the Message class. |
| group | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | The subscription group if desired (typically used when multiple instances of the same system are running) |
| ignoreMessageHeader | [System.Boolean](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Boolean 'System.Boolean') | If true, the message type specified will be ignored and it will automatically attempt to convert the underlying message to the given class |
| cancellationToken | [System.Threading.CancellationToken](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Threading.CancellationToken 'System.Threading.CancellationToken') | A cancellation token |

<a name='M-MQContract-Interfaces-IConsumerContractConnection`1-RegisterQueryResponseAsyncConsumerAsync``3-``2,System-String,System-String,System-Boolean,MQContract-Messages-MessageFilters{``0},System-Threading-CancellationToken-'></a>
### RegisterQueryResponseAsyncConsumerAsync\`\`3(consumer,channel,group,ignoreMessageHeader,messageFilters,cancellationToken) `method`

##### Summary

Called to register a QueryResponseAsyncConsumer into the contract connection

##### Returns

The Contract Connection instance to allow chaining calls

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| consumer | [\`\`2](#T-``2 '``2') | An instance of the consumer |
| channel | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | Specifies the message channel to use.  The prefered method is using the MessageChannelAttribute on the Message class. |
| group | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | The subscription group if desired (typically used when multiple instances of the same system are running) |
| ignoreMessageHeader | [System.Boolean](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Boolean 'System.Boolean') | If true, the message type specified will be ignored and it will automatically attempt to convert the underlying message to the given class |
| messageFilters | [MQContract.Messages.MessageFilters{\`\`0}](#T-MQContract-Messages-MessageFilters{``0} 'MQContract.Messages.MessageFilters{``0}') | Provides any filtering options for this subscription to filter out messages prior to action calls if desired |
| cancellationToken | [System.Threading.CancellationToken](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Threading.CancellationToken 'System.Threading.CancellationToken') | A cancellation token |

##### Generic Types

| Name | Description |
| ---- | ----------- |
| TQuery | The type of message to listen for |
| TQueryResponse | The type of message to respond with |
| TConsumer | The type that implements IQueryResponseAsyncConsumer<TQuery,TQueryResponse> |

<a name='M-MQContract-Interfaces-IConsumerContractConnection`1-RegisterQueryResponseAsyncConsumerAsync``3-System-String,System-String,System-Boolean,MQContract-Messages-MessageFilters{``0},System-Threading-CancellationToken-'></a>
### RegisterQueryResponseAsyncConsumerAsync\`\`3(channel,group,ignoreMessageHeader,messageFilters,cancellationToken) `method`

##### Summary

Called to register a QueryResponseAsyncConsumer into the contract connection.  This will create an instance of the TConsumer type that is requested and register it.

##### Returns

The Contract Connection instance to allow chaining calls

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| channel | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | Specifies the message channel to use.  The prefered method is using the MessageChannelAttribute on the Message class. |
| group | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | The subscription group if desired (typically used when multiple instances of the same system are running) |
| ignoreMessageHeader | [System.Boolean](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Boolean 'System.Boolean') | If true, the message type specified will be ignored and it will automatically attempt to convert the underlying message to the given class |
| messageFilters | [MQContract.Messages.MessageFilters{\`\`0}](#T-MQContract-Messages-MessageFilters{``0} 'MQContract.Messages.MessageFilters{``0}') | Provides any filtering options for this subscription to filter out messages prior to action calls if desired |
| cancellationToken | [System.Threading.CancellationToken](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Threading.CancellationToken 'System.Threading.CancellationToken') | A cancellation token |

##### Generic Types

| Name | Description |
| ---- | ----------- |
| TQuery | The type of message to listen for |
| TQueryResponse | The type of message to respond with |
| TConsumer | The type that implements IQueryResponseAsyncConsumer<TQuery,TQueryResponse> |

<a name='M-MQContract-Interfaces-IConsumerContractConnection`1-RegisterQueryResponseConsumerAsync-System-Type,System-String,System-String,System-Boolean,System-Threading-CancellationToken-'></a>
### RegisterQueryResponseConsumerAsync(consumerType,channel,group,ignoreMessageHeader,cancellationToken) `method`

##### Summary

Called to register a QueryResponseConsumer into the contract connection.

##### Returns

The Contract Connection instance to allow chaining calls

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| consumerType | [System.Type](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Type 'System.Type') | The type instance to be constructed and registered into the system.  It must implement IQueryResponseConsumer<Q,R>. |
| channel | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | Specifies the message channel to use.  The prefered method is using the MessageChannelAttribute on the Message class. |
| group | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | The subscription group if desired (typically used when multiple instances of the same system are running) |
| ignoreMessageHeader | [System.Boolean](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Boolean 'System.Boolean') | If true, the message type specified will be ignored and it will automatically attempt to convert the underlying message to the given class |
| cancellationToken | [System.Threading.CancellationToken](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Threading.CancellationToken 'System.Threading.CancellationToken') | A cancellation token |

<a name='M-MQContract-Interfaces-IConsumerContractConnection`1-RegisterQueryResponseConsumerAsync``3-``2,System-String,System-String,System-Boolean,MQContract-Messages-MessageFilters{``0},System-Threading-CancellationToken-'></a>
### RegisterQueryResponseConsumerAsync\`\`3(consumer,channel,group,ignoreMessageHeader,messageFilters,cancellationToken) `method`

##### Summary

Called to register a QueryResponseConsumer into the contract connection

##### Returns

The Contract Connection instance to allow chaining calls

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| consumer | [\`\`2](#T-``2 '``2') | An instance of the consumer |
| channel | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | Specifies the message channel to use.  The prefered method is using the MessageChannelAttribute on the Message class. |
| group | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | The subscription group if desired (typically used when multiple instances of the same system are running) |
| ignoreMessageHeader | [System.Boolean](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Boolean 'System.Boolean') | If true, the message type specified will be ignored and it will automatically attempt to convert the underlying message to the given class |
| messageFilters | [MQContract.Messages.MessageFilters{\`\`0}](#T-MQContract-Messages-MessageFilters{``0} 'MQContract.Messages.MessageFilters{``0}') | Provides any filtering options for this subscription to filter out messages prior to action calls if desired |
| cancellationToken | [System.Threading.CancellationToken](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Threading.CancellationToken 'System.Threading.CancellationToken') | A cancellation token |

##### Generic Types

| Name | Description |
| ---- | ----------- |
| TQuery | The type of message to listen for |
| TQueryResponse | The type of message to respond with |
| TConsumer | The type that implements IQueryResponseConsumer<TQuery,TQueryResponse> |

<a name='M-MQContract-Interfaces-IConsumerContractConnection`1-RegisterQueryResponseConsumerAsync``3-System-String,System-String,System-Boolean,MQContract-Messages-MessageFilters{``0},System-Threading-CancellationToken-'></a>
### RegisterQueryResponseConsumerAsync\`\`3(channel,group,ignoreMessageHeader,messageFilters,cancellationToken) `method`

##### Summary

Called to register a QueryResponseConsumer into the contract connection.  This will create an instance of the TConsumer type that is requested and register it.

##### Returns

The Contract Connection instance to allow chaining calls

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| channel | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | Specifies the message channel to use.  The prefered method is using the MessageChannelAttribute on the Message class. |
| group | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | The subscription group if desired (typically used when multiple instances of the same system are running) |
| ignoreMessageHeader | [System.Boolean](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Boolean 'System.Boolean') | If true, the message type specified will be ignored and it will automatically attempt to convert the underlying message to the given class |
| messageFilters | [MQContract.Messages.MessageFilters{\`\`0}](#T-MQContract-Messages-MessageFilters{``0} 'MQContract.Messages.MessageFilters{``0}') | Provides any filtering options for this subscription to filter out messages prior to action calls if desired |
| cancellationToken | [System.Threading.CancellationToken](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Threading.CancellationToken 'System.Threading.CancellationToken') | A cancellation token |

##### Generic Types

| Name | Description |
| ---- | ----------- |
| TQuery | The type of message to listen for |
| TQueryResponse | The type of message to respond with |
| TConsumer | The type that implements IQueryResponseConsumer<TQuery,TQueryResponse> |

<a name='T-MQContract-Interfaces-Middleware-IContext'></a>
## IContext `type`

##### Namespace

MQContract.Interfaces.Middleware

##### Summary

This is used to represent a Context for the middleware calls to use that exists from the start to the end of the message conversion process

<a name='P-MQContract-Interfaces-Middleware-IContext-Activity'></a>
### Activity `property`

##### Summary

Houses the current activity (if set) for the given context for Open Telemetry usage

<a name='P-MQContract-Interfaces-Middleware-IContext-Item-System-String-'></a>
### Item `property`

##### Summary

Used to store and retreive values from the context during the conversion process.

##### Returns

The value if it exists in the context

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| key | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | The unique key to use |

<a name='T-MQContract-Interfaces-IContractConnection'></a>
## IContractConnection `type`

##### Namespace

MQContract.Interfaces

##### Summary

This interface represents the Core class for the MQContract system, IE the ContractConnection

<a name='M-MQContract-Interfaces-IContractConnection-BulkPublishAsync``1-System-Collections-Generic-IEnumerable{System-ValueTuple{``0,MQContract-Messages-MessageHeader}},System-String,System-Threading-CancellationToken-'></a>
### BulkPublishAsync\`\`1(messages,channel,cancellationToken) `method`

##### Summary

Called to send a bulk set of messages into the underlying service Pub/Sub style

##### Returns

A result indicating the tranmission results

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| messages | [System.Collections.Generic.IEnumerable{System.ValueTuple{\`\`0,MQContract.Messages.MessageHeader}}](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Collections.Generic.IEnumerable 'System.Collections.Generic.IEnumerable{System.ValueTuple{``0,MQContract.Messages.MessageHeader}}') | The set of messages to transmit, optionally with their given headers |
| channel | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | Specifies the message channel to use.  The prefered method is using the MessageChannelAttribute on the class. |
| cancellationToken | [System.Threading.CancellationToken](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Threading.CancellationToken 'System.Threading.CancellationToken') | A cancellation token |

##### Generic Types

| Name | Description |
| ---- | ----------- |
| TMessage | The type of message to send |

<a name='M-MQContract-Interfaces-IContractConnection-PingAsync'></a>
### PingAsync() `method`

##### Summary

Called to Ping the underlying system to obtain both information and ensure it is up.  Not all Services support this method.

##### Returns



##### Parameters

This method has no parameters.

<a name='M-MQContract-Interfaces-IContractConnection-PublishAsync``1-``0,System-String,MQContract-Messages-MessageHeader,System-Threading-CancellationToken-'></a>
### PublishAsync\`\`1(message,channel,messageHeader,cancellationToken) `method`

##### Summary

Called to send a message into the underlying service Pub/Sub style

##### Returns

A result indicating the tranmission results

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| message | [\`\`0](#T-``0 '``0') | The message to send |
| channel | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | Specifies the message channel to use.  The prefered method is using the MessageChannelAttribute on the class. |
| messageHeader | [MQContract.Messages.MessageHeader](#T-MQContract-Messages-MessageHeader 'MQContract.Messages.MessageHeader') | The headers to pass along with the message |
| cancellationToken | [System.Threading.CancellationToken](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Threading.CancellationToken 'System.Threading.CancellationToken') | A cancellation token |

##### Generic Types

| Name | Description |
| ---- | ----------- |
| TMessage | The type of message to send |

##### Example

```
var result = await contractConnection.PublishAsync(new ArrivalAnnouncement("John", "Doe"));
Console.WriteLine($"Published ID: {result.ID}");
```

<a name='M-MQContract-Interfaces-IContractConnection-QueryAsync``1-``0,System-Nullable{System-TimeSpan},System-String,System-String,MQContract-Messages-MessageHeader,System-Threading-CancellationToken-'></a>
### QueryAsync\`\`1(message,timeout,channel,responseChannel,messageHeader,cancellationToken) `method`

##### Summary

Called to send a message into the underlying service in the Query/Response style.  The return type is not specified here and is instead obtained from the QueryResponseTypeAttribute
attached to the Query message type class.

##### Returns

A result indicating the success or failure as well as the returned message

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| message | [\`\`0](#T-``0 '``0') | The message to send |
| timeout | [System.Nullable{System.TimeSpan}](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Nullable 'System.Nullable{System.TimeSpan}') | The allowed timeout prior to a response being received |
| channel | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | Specifies the message channel to use.  The prefered method is using the MessageChannelAttribute on the class. |
| responseChannel | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | Specifies the message channel to use for the response.  The preferred method is using the QueryResponseChannelAttribute on the class.  This is 
only used when the underlying connection does not support a QueryResponse style messaging. |
| messageHeader | [MQContract.Messages.MessageHeader](#T-MQContract-Messages-MessageHeader 'MQContract.Messages.MessageHeader') | The headers to pass along with the message |
| cancellationToken | [System.Threading.CancellationToken](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Threading.CancellationToken 'System.Threading.CancellationToken') | A cancellation token |

##### Generic Types

| Name | Description |
| ---- | ----------- |
| TQuery | The type of message to send for the query |

<a name='M-MQContract-Interfaces-IContractConnection-QueryAsync``2-``0,System-Nullable{System-TimeSpan},System-String,System-String,MQContract-Messages-MessageHeader,System-Threading-CancellationToken-'></a>
### QueryAsync\`\`2(message,timeout,channel,responseChannel,messageHeader,cancellationToken) `method`

##### Summary

Called to send a message into the underlying service in the Query/Response style

##### Returns

A result indicating the success or failure as well as the returned message

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| message | [\`\`0](#T-``0 '``0') | The message to send |
| timeout | [System.Nullable{System.TimeSpan}](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Nullable 'System.Nullable{System.TimeSpan}') | The allowed timeout prior to a response being received |
| channel | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | Specifies the message channel to use.  The prefered method is using the MessageChannelAttribute on the class. |
| responseChannel | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | Specifies the message channel to use for the response.  The preferred method is using the QueryResponseChannelAttribute on the class.  This is 
only used when the underlying connection does not support a QueryResponse style messaging. |
| messageHeader | [MQContract.Messages.MessageHeader](#T-MQContract-Messages-MessageHeader 'MQContract.Messages.MessageHeader') | The headers to pass along with the message |
| cancellationToken | [System.Threading.CancellationToken](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Threading.CancellationToken 'System.Threading.CancellationToken') | A cancellation token |

##### Generic Types

| Name | Description |
| ---- | ----------- |
| TQuery | The type of message to send for the query |
| TQueryResponse | The type of message to expect back for the response |

##### Example

```
var response = await contractConnection.QueryAsync<Greeting, string>(new Greeting("John", "Doe"));
Console.WriteLine($"Response: {response.Result}");
```

<a name='T-MQContract-Interfaces-IContractMetric'></a>
## IContractMetric `type`

##### Namespace

MQContract.Interfaces

##### Summary

Houses a set of metrics that were requested from the internal metric tracker.
All message conversion durations are calculated from the perspective:
    - When a class is being sent from the point of starting the middleware to the point where the class has been encoded into a service message and the middleware has completed
    - When a service message is being recieved from the point of starting the middleware to the point where the class has been built from the service message and the middleware has completed

<a name='P-MQContract-Interfaces-IContractMetric-MessageBytes'></a>
### MessageBytes `property`

##### Summary

Total amount of bytes from the messages

<a name='P-MQContract-Interfaces-IContractMetric-MessageBytesAverage'></a>
### MessageBytesAverage `property`

##### Summary

Average number of bytes from the messages

<a name='P-MQContract-Interfaces-IContractMetric-MessageBytesMax'></a>
### MessageBytesMax `property`

##### Summary

Maximum number of bytes from the messages

<a name='P-MQContract-Interfaces-IContractMetric-MessageBytesMin'></a>
### MessageBytesMin `property`

##### Summary

Minimum number of bytes from the messages

<a name='P-MQContract-Interfaces-IContractMetric-MessageConversionAverage'></a>
### MessageConversionAverage `property`

##### Summary

Average time to encode/decode the messages

<a name='P-MQContract-Interfaces-IContractMetric-MessageConversionDuration'></a>
### MessageConversionDuration `property`

##### Summary

Total time spent converting the messages

<a name='P-MQContract-Interfaces-IContractMetric-MessageConversionMax'></a>
### MessageConversionMax `property`

##### Summary

Maximum time to encode/decode a message

<a name='P-MQContract-Interfaces-IContractMetric-MessageConversionMin'></a>
### MessageConversionMin `property`

##### Summary

Minimum time to encode/decode a message

<a name='P-MQContract-Interfaces-IContractMetric-Messages'></a>
### Messages `property`

##### Summary

Total number of messages

<a name='T-MQContract-Interfaces-IContractedConnection'></a>
## IContractedConnection `type`

##### Namespace

MQContract.Interfaces

##### Summary

The base representation of a Contract Connection, specifically a single service connection supporting contract connection

<a name='T-MQContract-Interfaces-Messages-IEncodedMessage'></a>
## IEncodedMessage `type`

##### Namespace

MQContract.Interfaces.Messages

##### Summary

Used to house an underlying message that has been encoded and is ready to be "shipped" into the underlying service layer

<a name='P-MQContract-Interfaces-Messages-IEncodedMessage-Data'></a>
### Data `property`

##### Summary

The encoded message

<a name='P-MQContract-Interfaces-Messages-IEncodedMessage-Header'></a>
### Header `property`

##### Summary

The header for the given message

<a name='P-MQContract-Interfaces-Messages-IEncodedMessage-MessageTypeID'></a>
### MessageTypeID `property`

##### Summary

The message type id to transmit across

<a name='T-MQContract-Interfaces-Consumers-IHeaderFilteredConsumer'></a>
## IHeaderFilteredConsumer `type`

##### Namespace

MQContract.Interfaces.Consumers

##### Summary

Used to define a consumer that will filter out given messages using a header filter

<a name='P-MQContract-Interfaces-Consumers-IHeaderFilteredConsumer-Filter'></a>
### Filter `property`

##### Summary

The filter callback to be invoked that will be supplied the current headers and expect back a filter type

<a name='T-MQContract-Interfaces-Service-IInboxQueryableMessageServiceConnection'></a>
## IInboxQueryableMessageServiceConnection `type`

##### Namespace

MQContract.Interfaces.Service

##### Summary

Used to implement an Inbox style query response underlying service, this is if the service does not support QueryResponse messaging but does support a sort of query inbox response 
style pub sub where you can specify the destination down to a specific instance.

<a name='M-MQContract-Interfaces-Service-IInboxQueryableMessageServiceConnection-EstablishInboxSubscriptionAsync-System-Func{MQContract-Messages-ReceivedInboxServiceMessage,System-Threading-Tasks-ValueTask},System-Threading-CancellationToken-'></a>
### EstablishInboxSubscriptionAsync(messageReceived,cancellationToken) `method`

##### Summary

Establish the inbox subscription with the underlying service connection

##### Returns

A service subscription object specifically tied to the RPC inbox for this particular connection instance

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| messageReceived | [System.Func{MQContract.Messages.ReceivedInboxServiceMessage,System.Threading.Tasks.ValueTask}](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Func 'System.Func{MQContract.Messages.ReceivedInboxServiceMessage,System.Threading.Tasks.ValueTask}') | Callback called when a message is recieved in the RPC inbox |
| cancellationToken | [System.Threading.CancellationToken](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Threading.CancellationToken 'System.Threading.CancellationToken') | A cancellation token |

<a name='M-MQContract-Interfaces-Service-IInboxQueryableMessageServiceConnection-QueryAsync-MQContract-Messages-ServiceMessage,System-Guid,System-Threading-CancellationToken-'></a>
### QueryAsync(message,correlationID,cancellationToken) `method`

##### Summary

Called to publish a Query Request when using the inbox style

##### Returns

The transmission result of submitting the message

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| message | [MQContract.Messages.ServiceMessage](#T-MQContract-Messages-ServiceMessage 'MQContract.Messages.ServiceMessage') | The service message to submit |
| correlationID | [System.Guid](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Guid 'System.Guid') | The unique ID of the message to use for handling when the response is proper and is expected in the inbox subscription |
| cancellationToken | [System.Threading.CancellationToken](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Threading.CancellationToken 'System.Threading.CancellationToken') | A cancellation token |

<a name='T-MQContract-Interfaces-IMappableContractConnection`1'></a>
## IMappableContractConnection\`1 `type`

##### Namespace

MQContract.Interfaces

##### Summary

A Mappable Contract Connection which supports mapping one or more Service Connections to a given message type, channel and or headers
This also defines the extended resillience functionality to allow for a resillience policy to be set at the connection level

##### Generic Types

| Name | Description |
| ---- | ----------- |
| TContractConnection | The underlying type that is being represented here which must be IBaseContractConnection, CC is used for method chaining. |

<a name='M-MQContract-Interfaces-IMappableContractConnection`1-RegisterResiliencePolicy-System-String,System-Nullable{System-ValueTuple{System-Int32,System-Func{System-Int32,System-TimeSpan}}},System-Nullable{System-ValueTuple{System-Int32,System-TimeSpan}}-'></a>
### RegisterResiliencePolicy(serviceConnectionName,retryPolicy,circuitBreakPolicy) `method`

##### Summary

Register a default resiliency policy that will apply to any message transmissions that do not have a specific policy

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| serviceConnectionName | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | The name of the service connection, not necessarily unique, but can be used for logging and other things |
| retryPolicy | [System.Nullable{System.ValueTuple{System.Int32,System.Func{System.Int32,System.TimeSpan}}}](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Nullable 'System.Nullable{System.ValueTuple{System.Int32,System.Func{System.Int32,System.TimeSpan}}}') | The settings to use for retries if desired |
| circuitBreakPolicy | [System.Nullable{System.ValueTuple{System.Int32,System.TimeSpan}}](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Nullable 'System.Nullable{System.ValueTuple{System.Int32,System.TimeSpan}}') | The settings to use for circuit breaking if desired |

<a name='M-MQContract-Interfaces-IMappableContractConnection`1-RegisterResiliencePolicy-System-String,System-Type,System-Nullable{System-ValueTuple{System-Int32,System-Func{System-Int32,System-TimeSpan}}},System-Nullable{System-ValueTuple{System-Int32,System-TimeSpan}}-'></a>
### RegisterResiliencePolicy(serviceConnectionName,messageType,retryPolicy,circuitBreakPolicy) `method`

##### Summary

Register a resiliency policy that will apply to any message transmission of the given messageType

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| serviceConnectionName | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | The name of the service connection, not necessarily unique, but can be used for logging and other things |
| messageType | [System.Type](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Type 'System.Type') | The type of message to associate this policy to |
| retryPolicy | [System.Nullable{System.ValueTuple{System.Int32,System.Func{System.Int32,System.TimeSpan}}}](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Nullable 'System.Nullable{System.ValueTuple{System.Int32,System.Func{System.Int32,System.TimeSpan}}}') | The settings to use for retries if desired |
| circuitBreakPolicy | [System.Nullable{System.ValueTuple{System.Int32,System.TimeSpan}}](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Nullable 'System.Nullable{System.ValueTuple{System.Int32,System.TimeSpan}}') | The settings to use for circuit breaking if desired |

<a name='M-MQContract-Interfaces-IMappableContractConnection`1-RegisterResiliencePolicy-System-String,System-String,System-Nullable{System-ValueTuple{System-Int32,System-Func{System-Int32,System-TimeSpan}}},System-Nullable{System-ValueTuple{System-Int32,System-TimeSpan}}-'></a>
### RegisterResiliencePolicy(serviceConnectionName,messageChannel,retryPolicy,circuitBreakPolicy) `method`

##### Summary

Register a resiliency policy that will apply to any message transmission of a message on the given channel

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| serviceConnectionName | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | The name of the service connection, not necessarily unique, but can be used for logging and other things |
| messageChannel | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | The message channel to apply this policy to |
| retryPolicy | [System.Nullable{System.ValueTuple{System.Int32,System.Func{System.Int32,System.TimeSpan}}}](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Nullable 'System.Nullable{System.ValueTuple{System.Int32,System.Func{System.Int32,System.TimeSpan}}}') | The settings to use for retries if desired |
| circuitBreakPolicy | [System.Nullable{System.ValueTuple{System.Int32,System.TimeSpan}}](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Nullable 'System.Nullable{System.ValueTuple{System.Int32,System.TimeSpan}}') | The settings to use for circuit breaking if desired |

<a name='M-MQContract-Interfaces-IMappableContractConnection`1-RegisterResiliencePolicy``1-System-String,System-Nullable{System-ValueTuple{System-Int32,System-Func{System-Int32,System-TimeSpan}}},System-Nullable{System-ValueTuple{System-Int32,System-TimeSpan}}-'></a>
### RegisterResiliencePolicy\`\`1(serviceConnectionName,retryPolicy,circuitBreakPolicy) `method`

##### Summary

Register a resiliency policy that will apply to any message transmission of message type T

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| serviceConnectionName | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | The name of the service connection, not necessarily unique, but can be used for logging and other things |
| retryPolicy | [System.Nullable{System.ValueTuple{System.Int32,System.Func{System.Int32,System.TimeSpan}}}](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Nullable 'System.Nullable{System.ValueTuple{System.Int32,System.Func{System.Int32,System.TimeSpan}}}') | The settings to use for retries if desired |
| circuitBreakPolicy | [System.Nullable{System.ValueTuple{System.Int32,System.TimeSpan}}](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Nullable 'System.Nullable{System.ValueTuple{System.Int32,System.TimeSpan}}') | The settings to use for circuit breaking if desired |

##### Generic Types

| Name | Description |
| ---- | ----------- |
| TMessage | The type of message to associate this policy to |

<a name='M-MQContract-Interfaces-IMappableContractConnection`1-RegisterServiceConnection-System-Func{System-ValueTuple{System-String,System-Type,MQContract-Messages-MessageHeader},System-Boolean},System-String,MQContract-Interfaces-Service-IMessageServiceConnection-'></a>
### RegisterServiceConnection(checkCallback,serviceConnectionName,messageServiceConnection) `method`

##### Summary

Register a service connection using a callback for mapping

##### Returns



##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| checkCallback | [System.Func{System.ValueTuple{System.String,System.Type,MQContract.Messages.MessageHeader},System.Boolean}](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Func 'System.Func{System.ValueTuple{System.String,System.Type,MQContract.Messages.MessageHeader},System.Boolean}') | The callback to check if this connection can be used with the given parameters |
| serviceConnectionName | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | The name of the service connection, not necessarily unique, but can be used for logging and other things |
| messageServiceConnection | [MQContract.Interfaces.Service.IMessageServiceConnection](#T-MQContract-Interfaces-Service-IMessageServiceConnection 'MQContract.Interfaces.Service.IMessageServiceConnection') | The service connection to use when the checkCallback returns true |

<a name='M-MQContract-Interfaces-IMappableContractConnection`1-RegisterServiceConnection-System-String,System-String,MQContract-Interfaces-Service-IMessageServiceConnection-'></a>
### RegisterServiceConnection(channel,serviceConnectionName,messageServiceConnection) `method`

##### Summary

Register a service connection for a given channel

##### Returns



##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| channel | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | The channel that the service connection should be used for |
| serviceConnectionName | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | The name of the service connection, not necessarily unique, but can be used for logging and other things |
| messageServiceConnection | [MQContract.Interfaces.Service.IMessageServiceConnection](#T-MQContract-Interfaces-Service-IMessageServiceConnection 'MQContract.Interfaces.Service.IMessageServiceConnection') | The service connection to use when the channel is used |

<a name='M-MQContract-Interfaces-IMappableContractConnection`1-RegisterServiceConnection-System-Type,System-String,MQContract-Interfaces-Service-IMessageServiceConnection-'></a>
### RegisterServiceConnection(messageType,serviceConnectionName,messageServiceConnection) `method`

##### Summary

Register a service connection for a given message type

##### Returns



##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| messageType | [System.Type](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Type 'System.Type') | The type of message that the service connection should be used for |
| serviceConnectionName | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | The name of the service connection, not necessarily unique, but can be used for logging and other things |
| messageServiceConnection | [MQContract.Interfaces.Service.IMessageServiceConnection](#T-MQContract-Interfaces-Service-IMessageServiceConnection 'MQContract.Interfaces.Service.IMessageServiceConnection') | The service connection to use when the messageType is used |

<a name='M-MQContract-Interfaces-IMappableContractConnection`1-RegisterServiceConnection-System-String,System-String,System-String,MQContract-Interfaces-Service-IMessageServiceConnection-'></a>
### RegisterServiceConnection(messageHeaderKey,messageHeaderValue,serviceConnectionName,messageServiceConnection) `method`

##### Summary

Register a service connection to be used when the messageHeader contains the messageHeaderKey and it's value is messageHeaderValue

##### Returns



##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| messageHeaderKey | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | The key value for the message header |
| messageHeaderValue | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | The value for the message header |
| serviceConnectionName | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | The name of the service connection, not necessarily unique, but can be used for logging and other things |
| messageServiceConnection | [MQContract.Interfaces.Service.IMessageServiceConnection](#T-MQContract-Interfaces-Service-IMessageServiceConnection 'MQContract.Interfaces.Service.IMessageServiceConnection') | The service connection to use when the messageHeader has the key and the value matches |

<a name='M-MQContract-Interfaces-IMappableContractConnection`1-RegisterServiceConnection``1-System-String,MQContract-Interfaces-Service-IMessageServiceConnection-'></a>
### RegisterServiceConnection\`\`1(serviceConnectionName,messageServiceConnection) `method`

##### Summary

Register a service connection for a given message type

##### Returns



##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| serviceConnectionName | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | The name of the service connection, not necessarily unique, but can be used for logging and other things |
| messageServiceConnection | [MQContract.Interfaces.Service.IMessageServiceConnection](#T-MQContract-Interfaces-Service-IMessageServiceConnection 'MQContract.Interfaces.Service.IMessageServiceConnection') | The service connection to use when the message is of type T |

##### Generic Types

| Name | Description |
| ---- | ----------- |
| TMessage | The type of message that the service connection should be used for |

<a name='T-MQContract-Interfaces-IMappedContractConnection'></a>
## IMappedContractConnection `type`

##### Namespace

MQContract.Interfaces

##### Summary

The representation of a Mapped Contract Connection which is built to use 1 or more service connections for the calls

<a name='T-MQContract-Interfaces-IMessageContextContractConnection`1'></a>
## IMessageContextContractConnection\`1 `type`

##### Namespace

MQContract.Interfaces

##### Summary

Houses the message context pieces for a given contract connection

##### Generic Types

| Name | Description |
| ---- | ----------- |
| TContractConnection | The underlying type that is being represented here which must be IBaseContractConnection, CC is used for method chaining. |

<a name='M-MQContract-Interfaces-IMessageContextContractConnection`1-RegisterMessageContext-MQContract-MQContractMessageContext-'></a>
### RegisterMessageContext(messageContext) `method`

##### Summary

Called to register a Message Context with the given connection

##### Returns

The Contract Connection instance to allow chaining calls

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| messageContext | [MQContract.MQContractMessageContext](#T-MQContract-MQContractMessageContext 'MQContract.MQContractMessageContext') | The Message Context (that the code generator has built upon) to register |

<a name='T-MQContract-Interfaces-Conversion-IMessageConverter`2'></a>
## IMessageConverter\`2 `type`

##### Namespace

MQContract.Interfaces.Conversion

##### Summary

Used to define a message converter.  These are called upon if a 
message is received on a channel of type T but it is waiting for 
message of type V

##### Generic Types

| Name | Description |
| ---- | ----------- |
| TSourceMessage | The source message type |
| TDestinationMessage | The destination message type |

<a name='M-MQContract-Interfaces-Conversion-IMessageConverter`2-ConvertAsync-`0-'></a>
### ConvertAsync(source) `method`

##### Summary

Called to convert a message from type T to type V

##### Returns

The source message converted to the destination type V

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| source | [\`0](#T-`0 '`0') | The message to convert |

<a name='T-MQContract-Interfaces-Encoding-IMessageEncoder'></a>
## IMessageEncoder `type`

##### Namespace

MQContract.Interfaces.Encoding

##### Summary

An implementation of this is used to encode/decode message bodies when 
specified for a connection.  This is to allow for an override of the 
default encoding of Json for the messages.

<a name='M-MQContract-Interfaces-Encoding-IMessageEncoder-DecodeAsync``1-System-IO-Stream-'></a>
### DecodeAsync\`\`1(stream) `method`

##### Summary

Called to decode a message from a byte array

##### Returns

Null when fails or the value of T that was encoded inside the stream

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| stream | [System.IO.Stream](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.IO.Stream 'System.IO.Stream') | A stream representing the byte array data that was transmitted as the message body in KubeMQ |

##### Generic Types

| Name | Description |
| ---- | ----------- |
| TMessage | The type of message being decoded |

<a name='M-MQContract-Interfaces-Encoding-IMessageEncoder-EncodeAsync``1-``0-'></a>
### EncodeAsync\`\`1(message) `method`

##### Summary

Called to encode a message into a byte array

##### Returns

A byte array of the message in it's encoded form that will be transmitted

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| message | [\`\`0](#T-``0 '``0') | The message being encoded |

##### Generic Types

| Name | Description |
| ---- | ----------- |
| TMessage | The type of message being encoded |

<a name='T-MQContract-Interfaces-Encrypting-IMessageEncryptor'></a>
## IMessageEncryptor `type`

##### Namespace

MQContract.Interfaces.Encrypting

##### Summary

An implementation of this is used to encrypt/decrypt message bodies when 
specified for a connection.  This is to allow for extended message security
if desired.

<a name='M-MQContract-Interfaces-Encrypting-IMessageEncryptor-DecryptAsync-System-IO-Stream,MQContract-Messages-MessageHeader-'></a>
### DecryptAsync(stream,headers) `method`

##### Summary

Called to decrypt the message body stream received as a message

##### Returns

A decrypted stream of the message body

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| stream | [System.IO.Stream](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.IO.Stream 'System.IO.Stream') | The stream representing the message body binary data |
| headers | [MQContract.Messages.MessageHeader](#T-MQContract-Messages-MessageHeader 'MQContract.Messages.MessageHeader') | The message headers that were provided by the message |

<a name='M-MQContract-Interfaces-Encrypting-IMessageEncryptor-EncryptAsync-System-Byte[]-'></a>
### EncryptAsync(data) `method`

##### Summary

Called to encrypt the message body prior to transmitting a message

##### Returns

An encrypted byte array of the message body and any headers that might be needed

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| data | [System.Byte[]](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Byte[] 'System.Byte[]') | The original unencrypted body data |

<a name='T-MQContract-Interfaces-Consumers-IMessageFilteredConsumer`1'></a>
## IMessageFilteredConsumer\`1 `type`

##### Namespace

MQContract.Interfaces.Consumers

##### Summary

Used to define a consumer that will filter out messages of a given message type

##### Generic Types

| Name | Description |
| ---- | ----------- |
| TMessage | The type of message the filter understands |

<a name='P-MQContract-Interfaces-Consumers-IMessageFilteredConsumer`1-Filter'></a>
### Filter `property`

##### Summary

Provides the filter callback that will be supplied the message headers and current message 
and expects back a filter instruction

<a name='T-MQContract-Interfaces-Service-IMessageServiceConnection'></a>
## IMessageServiceConnection `type`

##### Namespace

MQContract.Interfaces.Service

##### Summary

Defines an underlying service connection.  This interface is used to allow for the creation of multiple underlying connection types to support the ability to use common code while
being able to run against 1 or more Message services.

<a name='P-MQContract-Interfaces-Service-IMessageServiceConnection-MaxMessageBodySize'></a>
### MaxMessageBodySize `property`

##### Summary

Maximum supported message body size in bytes

<a name='M-MQContract-Interfaces-Service-IMessageServiceConnection-CloseAsync'></a>
### CloseAsync() `method`

##### Summary

Implements a call to close off the connection when the ContractConnection is closed

##### Returns

A task that the close is running in

##### Parameters

This method has no parameters.

<a name='M-MQContract-Interfaces-Service-IMessageServiceConnection-PublishAsync-MQContract-Messages-ServiceMessage,System-Threading-CancellationToken-'></a>
### PublishAsync(message,cancellationToken) `method`

##### Summary

Implements a publish call to publish the given message

##### Returns

A transmission result instance indicating the result

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| message | [MQContract.Messages.ServiceMessage](#T-MQContract-Messages-ServiceMessage 'MQContract.Messages.ServiceMessage') | The message to publish |
| cancellationToken | [System.Threading.CancellationToken](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Threading.CancellationToken 'System.Threading.CancellationToken') | A cancellation token |

<a name='M-MQContract-Interfaces-Service-IMessageServiceConnection-SubscribeAsync-System-Func{MQContract-Messages-ReceivedServiceMessage,System-Threading-Tasks-ValueTask},System-Action{System-Exception},System-String,System-String,System-Threading-CancellationToken-'></a>
### SubscribeAsync(messageReceived,errorReceived,channel,group,cancellationToken) `method`

##### Summary

Implements a call to create a subscription to a given channel as a member of a given group

##### Returns

A service subscription object

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| messageReceived | [System.Func{MQContract.Messages.ReceivedServiceMessage,System.Threading.Tasks.ValueTask}](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Func 'System.Func{MQContract.Messages.ReceivedServiceMessage,System.Threading.Tasks.ValueTask}') | The callback to invoke when a message is received |
| errorReceived | [System.Action{System.Exception}](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Action 'System.Action{System.Exception}') | The callback to invoke when an exception occurs |
| channel | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | The name of the channel to subscribe to |
| group | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | The consumer group to register as |
| cancellationToken | [System.Threading.CancellationToken](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Threading.CancellationToken 'System.Threading.CancellationToken') | A cancellation token |

<a name='T-MQContract-Interfaces-Encoding-IMessageTypeEncoder`1'></a>
## IMessageTypeEncoder\`1 `type`

##### Namespace

MQContract.Interfaces.Encoding

##### Summary

Used to define a specific encoder for the message type of T
This is used to override the default Json and the Global one for the connection if specified

##### Generic Types

| Name | Description |
| ---- | ----------- |
| TMessage | The type of message that this encoder supports |

<a name='M-MQContract-Interfaces-Encoding-IMessageTypeEncoder`1-DecodeAsync-System-IO-Stream-'></a>
### DecodeAsync(stream) `method`

##### Summary

Called to decode the message from a byte stream into the specified type

##### Returns

null if the Decode fails, otherwise an instance of the message decoded from the stream

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| stream | [System.IO.Stream](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.IO.Stream 'System.IO.Stream') | The byte stream containing the encoded message |

<a name='M-MQContract-Interfaces-Encoding-IMessageTypeEncoder`1-EncodeAsync-`0-'></a>
### EncodeAsync(message) `method`

##### Summary

Called to encode the message into a byte array

##### Returns

The message encoded as a byte array

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| message | [\`0](#T-`0 '`0') | The message value to encode |

<a name='T-MQContract-Interfaces-Encrypting-IMessageTypeEncryptor`1'></a>
## IMessageTypeEncryptor\`1 `type`

##### Namespace

MQContract.Interfaces.Encrypting

##### Summary

Used to define a specific message encryptor for the type T.  
This will override the global decryptor if specified for this connection 
as well as the default of not encrypting the message body

##### Generic Types

| Name | Description |
| ---- | ----------- |
| TMessage | The type of message that this encryptor supports |

<a name='T-MQContract-Interfaces-IMetricContractConnection`1'></a>
## IMetricContractConnection\`1 `type`

##### Namespace

MQContract.Interfaces

##### Summary

Houses the metric pieces for a given contract connection

##### Generic Types

| Name | Description |
| ---- | ----------- |
| TContractConnection | The underlying type that is being represented here which must be IBaseContractConnection, CC is used for method chaining. |

<a name='M-MQContract-Interfaces-IMetricContractConnection`1-AddMetrics-System-Diagnostics-Metrics-Meter,System-Boolean-'></a>
### AddMetrics(meter,useInternal) `method`

##### Summary

Called to activate the metrics tracking middleware for this connection instance

##### Returns

The Contract Connection instance to allow chaining calls

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| meter | [System.Diagnostics.Metrics.Meter](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Diagnostics.Metrics.Meter 'System.Diagnostics.Metrics.Meter') | The Meter item to create all system metrics against |
| useInternal | [System.Boolean](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Boolean 'System.Boolean') | Indicates if the internal metrics collector should be used |

##### Remarks

For the Meter metrics, all durations are in ms and the following values and patterns will apply:
mqcontract.messages.sent.count = count of messages sent (Counter<long>)
mqcontract.messages.sent.bytes = count of bytes sent (message data) (Counter<long>)
mqcontract.messages.received.count = count of messages received (Counter<long>)
mqcontract.messages.received.bytes = count of bytes received (message data) (Counter<long>)
mqcontract.messages.encodingduration = milliseconds to encode messages (Histogram<double>)
mqcontract.messages.decodingduration = milliseconds to decode messages (Histogram<double>)
mqcontract.types.{MessageTypeName}.{MessageVersion(_ instead of .)}.sent.count = count of messages sent of a given type (Counter<long>)
mqcontract.types.{MessageTypeName}.{MessageVersion(_ instead of .)}.sent.bytes = count of bytes sent (message data) of a given type (Counter<long>)
mqcontract.types.{MessageTypeName}.{MessageVersion(_ instead of .)}.received.count = count of messages received of a given type (Counter<long>)
mqcontract.types.{MessageTypeName}.{MessageVersion(_ instead of .)}.received.bytes = count of bytes received (message data) of a given type (Counter<long>)
mqcontract.types.{MessageTypeName}.{MessageVersion(_ instead of .)}.encodingduration = milliseconds to encode messages of a given type (Histogram<double>)
mqcontract.types.{MessageTypeName}.{MessageVersion(_ instead of .)}.decodingduration = milliseconds to decode messages of a given type (Histogram<double>)
mqcontract.channels.{Channel}.sent.count = count of messages sent for a given channel (Counter<long>)
mqcontract.channels.{Channel}.sent.bytes = count of bytes sent (message data) for a given channel (Counter<long>)
mqcontract.channels.{Channel}.received.count = count of messages received for a given channel (Counter<long>)
mqcontract.channels.{Channel}.received.bytes = count of bytes received (message data) for a given channel (Counter<long>)
mqcontract.channels.{Channel}.encodingduration = milliseconds to encode messages for a given channel (Histogram<double>)
mqcontract.channels.{Channel}.decodingduration = milliseconds to decode messages for a given channel (Histogram<double>)

<a name='M-MQContract-Interfaces-IMetricContractConnection`1-EnableOpenTelemetry-System-String,System-Boolean-'></a>
### EnableOpenTelemetry(activitySource,linkActivitiesAcrossSystems) `method`

##### Summary

Called to enable Open Telemetry capabilities within the Contract Connection which will include passing activity information across the messages

##### Returns

The Contract Connection instance to allow chaining calls

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| activitySource | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | Used to override the Activity Source name if desired, otherwise it will default to MQContract |
| linkActivitiesAcrossSystems | [System.Boolean](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Boolean 'System.Boolean') | Setting this to true will automatically include headers in the messages to allow for linking the calling activity on one service to the activity on the receiver |

<a name='M-MQContract-Interfaces-IMetricContractConnection`1-GetSnapshot-System-Boolean-'></a>
### GetSnapshot(sent) `method`

##### Summary

Called to get a snapshot of the current global metrics.  Will return null if internal metrics are not enabled.

##### Returns

A record of the current metric snapshot or null if not available

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| sent | [System.Boolean](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Boolean 'System.Boolean') | true when the sent metrics are desired, false when received are desired |

<a name='M-MQContract-Interfaces-IMetricContractConnection`1-GetSnapshot-System-Type,System-Boolean-'></a>
### GetSnapshot(messageType,sent) `method`

##### Summary

Called to get a snapshot of the metrics for a given message type.  Will return null if internal metrics are not enabled.

##### Returns

A record of the current metric snapshot or null if not available

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| messageType | [System.Type](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Type 'System.Type') | The type of message to look for |
| sent | [System.Boolean](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Boolean 'System.Boolean') | true when the sent metrics are desired, false when received are desired |

<a name='M-MQContract-Interfaces-IMetricContractConnection`1-GetSnapshot-System-String,System-Boolean-'></a>
### GetSnapshot(channel,sent) `method`

##### Summary

Called to get a snapshot of the metrics for a given message channel.  Will return null if internal metrics are not enabled.

##### Returns

A record of the current metric snapshot or null if not available

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| channel | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | The channel to look for |
| sent | [System.Boolean](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Boolean 'System.Boolean') | true when the sent metrics are desired, false when received are desired |

<a name='M-MQContract-Interfaces-IMetricContractConnection`1-GetSnapshot``1-System-Boolean-'></a>
### GetSnapshot\`\`1(sent) `method`

##### Summary

Called to get a snapshot of the metrics for a given message type.  Will return null if internal metrics are not enabled.

##### Returns

A record of the current metric snapshot or null if not available

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| sent | [System.Boolean](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Boolean 'System.Boolean') | true when the sent metrics are desired, false when received are desired |

##### Generic Types

| Name | Description |
| ---- | ----------- |
| TMessage | The type of message to look for |

<a name='T-MQContract-Interfaces-Middleware-IMiddleware'></a>
## IMiddleware `type`

##### Namespace

MQContract.Interfaces.Middleware

##### Summary

Base Middleware just used to limit Generic Types for Register Middleware

<a name='T-MQContract-Interfaces-IMiddlewareContractConnection`1'></a>
## IMiddlewareContractConnection\`1 `type`

##### Namespace

MQContract.Interfaces

##### Summary

Houses the middleware pieces for a given contract connection

##### Generic Types

| Name | Description |
| ---- | ----------- |
| TContractConnection | The underlying type that is being represented here which must be IBaseContractConnection, CC is used for method chaining. |

<a name='M-MQContract-Interfaces-IMiddlewareContractConnection`1-RegisterMiddleware-System-Type-'></a>
### RegisterMiddleware(middleware) `method`

##### Summary

Register a middleware of a given type

##### Returns

The Contract Connection instance to allow chaining calls

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| middleware | [System.Type](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Type 'System.Type') | The type of middle ware to register, it must implement IBeforeDecodeMiddleware or IBeforeEncodeMiddleware or IAfterDecodeMiddleware or IAfterEncodeMiddleware or IBeforeEncodeSpecificTypeMiddleware<> or IAfterDecodeSpecificTypeMiddleware<> |

<a name='M-MQContract-Interfaces-IMiddlewareContractConnection`1-RegisterMiddleware-MQContract-Interfaces-Middleware-IMiddleware-'></a>
### RegisterMiddleware(instance) `method`

##### Summary

Register a middleware instance

##### Returns

The Contract Connection instance to allow chaining calls

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| instance | [MQContract.Interfaces.Middleware.IMiddleware](#T-MQContract-Interfaces-Middleware-IMiddleware 'MQContract.Interfaces.Middleware.IMiddleware') | The middle ware to register, it must implement IBeforeDecodeMiddleware or IBeforeEncodeMiddleware or IAfterDecodeMiddleware or IAfterEncodeMiddleware or IBeforeEncodeSpecificTypeMiddleware<> or IAfterDecodeSpecificTypeMiddleware<> |

<a name='M-MQContract-Interfaces-IMiddlewareContractConnection`1-RegisterMiddleware-System-Func{MQContract-Interfaces-Middleware-IMiddleware}-'></a>
### RegisterMiddleware(constructInstance) `method`

##### Summary

Register a middleware through a construct instance function

##### Returns

The Contract Connection instance to allow chaining calls

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| constructInstance | [System.Func{MQContract.Interfaces.Middleware.IMiddleware}](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Func 'System.Func{MQContract.Interfaces.Middleware.IMiddleware}') | Callback to create the instance.  The object returned must implement IBeforeDecodeMiddleware or IBeforeEncodeMiddleware or IAfterDecodeMiddleware or IAfterEncodeMiddleware or IBeforeEncodeSpecificTypeMiddleware<> or IAfterDecodeSpecificTypeMiddleware<> |

<a name='M-MQContract-Interfaces-IMiddlewareContractConnection`1-RegisterMiddleware``1'></a>
### RegisterMiddleware\`\`1() `method`

##### Summary

Register a middleware of a given type T to be used by the contract connection

##### Returns

The Contract Connection instance to allow chaining calls

##### Parameters

This method has no parameters.

##### Generic Types

| Name | Description |
| ---- | ----------- |
| TMiddleware | The type of middle ware to register, it must implement IBeforeDecodeMiddleware or IBeforeEncodeMiddleware or IAfterDecodeMiddleware or IAfterEncodeMiddleware |

<a name='M-MQContract-Interfaces-IMiddlewareContractConnection`1-RegisterMiddleware``1-System-Func{``0}-'></a>
### RegisterMiddleware\`\`1(constructInstance) `method`

##### Summary

Register a middleware of a given type T to be used by the contract connection

##### Returns

The Contract Connection instance to allow chaining calls

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| constructInstance | [System.Func{\`\`0}](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Func 'System.Func{``0}') | Callback to create the instance |

##### Generic Types

| Name | Description |
| ---- | ----------- |
| TMiddleware | The type of middle ware to register, it must implement IBeforeDecodeMiddleware or IBeforeEncodeMiddleware or IAfterDecodeMiddleware or IAfterEncodeMiddleware |

<a name='M-MQContract-Interfaces-IMiddlewareContractConnection`1-RegisterMiddleware``1-System-Func{MQContract-Interfaces-Middleware-ISpecificTypeMiddleware{``0}}-'></a>
### RegisterMiddleware\`\`1(constructInstance) `method`

##### Summary

Register a middleware of a given type T to be used by the contract connection

##### Returns

The Contract Connection instance to allow chaining calls

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| constructInstance | [System.Func{MQContract.Interfaces.Middleware.ISpecificTypeMiddleware{\`\`0}}](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Func 'System.Func{MQContract.Interfaces.Middleware.ISpecificTypeMiddleware{``0}}') | Callback to create the instance.  The object returned it must implement IBeforeEncodeSpecificTypeMiddleware<M> or IAfterDecodeSpecificTypeMiddleware<M> |

##### Generic Types

| Name | Description |
| ---- | ----------- |
| TMessage | The message type that this middleware is specifically called for |

<a name='M-MQContract-Interfaces-IMiddlewareContractConnection`1-RegisterMiddleware``1-MQContract-Interfaces-Middleware-ISpecificTypeMiddleware{``0}-'></a>
### RegisterMiddleware\`\`1(instance) `method`

##### Summary

Register a middleware of a given type T to be used by the contract connection

##### Returns

The Contract Connection instance to allow chaining calls

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| instance | [MQContract.Interfaces.Middleware.ISpecificTypeMiddleware{\`\`0}](#T-MQContract-Interfaces-Middleware-ISpecificTypeMiddleware{``0} 'MQContract.Interfaces.Middleware.ISpecificTypeMiddleware{``0}') | The middle ware to register, it must implement it must implement IBeforeEncodeSpecificTypeMiddleware<M> or IAfterDecodeSpecificTypeMiddleware<M> |

##### Generic Types

| Name | Description |
| ---- | ----------- |
| TMessage | The message type that this middleware is specifically called for |

<a name='M-MQContract-Interfaces-IMiddlewareContractConnection`1-RegisterMiddleware``2'></a>
### RegisterMiddleware\`\`2() `method`

##### Summary

Register a middleware of a given type T to be used by the contract connection

##### Returns

The Contract Connection instance to allow chaining calls

##### Parameters

This method has no parameters.

##### Generic Types

| Name | Description |
| ---- | ----------- |
| TMiddleware | The type of middle ware to register, it must implement IBeforeEncodeSpecificTypeMiddleware<M> or IAfterDecodeSpecificTypeMiddleware<M> |
| TMessage | The message type that this middleware is specifically called for |

<a name='M-MQContract-Interfaces-IMiddlewareContractConnection`1-RegisterMiddleware``2-System-Func{``0}-'></a>
### RegisterMiddleware\`\`2(constructInstance) `method`

##### Summary

Register a middleware of a given type T to be used by the contract connection

##### Returns

The Contract Connection instance to allow chaining calls

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| constructInstance | [System.Func{\`\`0}](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Func 'System.Func{``0}') | Callback to create the instance |

##### Generic Types

| Name | Description |
| ---- | ----------- |
| TMiddleware | The type of middle ware to register, it must implement IBeforeEncodeSpecificTypeMiddleware<M> or IAfterDecodeSpecificTypeMiddleware<M> |
| TMessage | The message type that this middleware is specifically called for |

<a name='T-MQContract-Interfaces-IMultiServiceContractConnection'></a>
## IMultiServiceContractConnection `type`

##### Namespace

MQContract.Interfaces

##### Summary

This interface represents an extended Contract Connection that is built around the idea of having more than 1 underlying service connection that can be interacted with 
depending on the defined conditions

<a name='M-MQContract-Interfaces-IMultiServiceContractConnection-BulkPublishAsync``1-System-Collections-Generic-IEnumerable{System-ValueTuple{``0,MQContract-Messages-MessageHeader}},System-String,System-Threading-CancellationToken-'></a>
### BulkPublishAsync\`\`1(messages,channel,cancellationToken) `method`

##### Summary

Called to send a bulk set of messages into the underlying services Pub/Sub style

##### Returns

A result indicating the tranmission results

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| messages | [System.Collections.Generic.IEnumerable{System.ValueTuple{\`\`0,MQContract.Messages.MessageHeader}}](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Collections.Generic.IEnumerable 'System.Collections.Generic.IEnumerable{System.ValueTuple{``0,MQContract.Messages.MessageHeader}}') | The set of messages to transmit, optionally with their given headers |
| channel | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | Specifies the message channel to use.  The prefered method is using the MessageChannelAttribute on the class. |
| cancellationToken | [System.Threading.CancellationToken](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Threading.CancellationToken 'System.Threading.CancellationToken') | A cancellation token |

##### Generic Types

| Name | Description |
| ---- | ----------- |
| TMessage | The type of message to send |

<a name='M-MQContract-Interfaces-IMultiServiceContractConnection-PingAsync'></a>
### PingAsync() `method`

##### Summary

Called to Ping the underlying systems (assuming they implement the call) to obtain both information and ensure it is up.  Not all Services support this method.

##### Returns



##### Parameters

This method has no parameters.

<a name='M-MQContract-Interfaces-IMultiServiceContractConnection-PublishAsync``1-``0,System-String,MQContract-Messages-MessageHeader,System-Threading-CancellationToken-'></a>
### PublishAsync\`\`1(message,channel,messageHeader,cancellationToken) `method`

##### Summary

Called to send a message into the underlying services Pub/Sub style

##### Returns

A result indicating the tranmission results

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| message | [\`\`0](#T-``0 '``0') | The message to send |
| channel | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | Specifies the message channel to use.  The prefered method is using the MessageChannelAttribute on the class. |
| messageHeader | [MQContract.Messages.MessageHeader](#T-MQContract-Messages-MessageHeader 'MQContract.Messages.MessageHeader') | The headers to pass along with the message |
| cancellationToken | [System.Threading.CancellationToken](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Threading.CancellationToken 'System.Threading.CancellationToken') | A cancellation token |

##### Generic Types

| Name | Description |
| ---- | ----------- |
| TMessage | The type of message to send |

<a name='M-MQContract-Interfaces-IMultiServiceContractConnection-QueryAsync``1-``0,System-Nullable{System-TimeSpan},System-String,System-String,MQContract-Messages-MessageHeader,System-Threading-CancellationToken-'></a>
### QueryAsync\`\`1(message,timeout,channel,responseChannel,messageHeader,cancellationToken) `method`

##### Summary

Called to send a message into the underlying services in the Query/Response style.  The return type is not specified here and is instead obtained from the QueryResponseTypeAttribute
attached to the Query message type class.

##### Returns

A result indicating the success or failure as well as the returned message

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| message | [\`\`0](#T-``0 '``0') | The message to send |
| timeout | [System.Nullable{System.TimeSpan}](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Nullable 'System.Nullable{System.TimeSpan}') | The allowed timeout prior to a response being received |
| channel | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | Specifies the message channel to use.  The prefered method is using the MessageChannelAttribute on the class. |
| responseChannel | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | Specifies the message channel to use for the response.  The preferred method is using the QueryResponseChannelAttribute on the class.  This is 
only used when the underlying connection does not support a QueryResponse style messaging. |
| messageHeader | [MQContract.Messages.MessageHeader](#T-MQContract-Messages-MessageHeader 'MQContract.Messages.MessageHeader') | The headers to pass along with the message |
| cancellationToken | [System.Threading.CancellationToken](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Threading.CancellationToken 'System.Threading.CancellationToken') | A cancellation token |

##### Generic Types

| Name | Description |
| ---- | ----------- |
| TQuery | The type of message to send for the query |

<a name='M-MQContract-Interfaces-IMultiServiceContractConnection-QueryAsync``2-``0,System-Nullable{System-TimeSpan},System-String,System-String,MQContract-Messages-MessageHeader,System-Threading-CancellationToken-'></a>
### QueryAsync\`\`2(message,timeout,channel,responseChannel,messageHeader,cancellationToken) `method`

##### Summary

Called to send a message into the underlying services in the Query/Response style

##### Returns

A result indicating the success or failure as well as the returned message

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| message | [\`\`0](#T-``0 '``0') | The message to send |
| timeout | [System.Nullable{System.TimeSpan}](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Nullable 'System.Nullable{System.TimeSpan}') | The allowed timeout prior to a response being received |
| channel | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | Specifies the message channel to use.  The prefered method is using the MessageChannelAttribute on the class. |
| responseChannel | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | Specifies the message channel to use for the response.  The preferred method is using the QueryResponseChannelAttribute on the class.  This is 
only used when the underlying connection does not support a QueryResponse style messaging. |
| messageHeader | [MQContract.Messages.MessageHeader](#T-MQContract-Messages-MessageHeader 'MQContract.Messages.MessageHeader') | The headers to pass along with the message |
| cancellationToken | [System.Threading.CancellationToken](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Threading.CancellationToken 'System.Threading.CancellationToken') | A cancellation token |

##### Generic Types

| Name | Description |
| ---- | ----------- |
| TQuery | The type of message to send for the query |
| TQueryResponse | The type of message to expect back for the response |

<a name='M-MQContract-Interfaces-IMultiServiceContractConnection-RegisterServiceConnection-System-String,MQContract-Interfaces-Service-IMessageServiceConnection-'></a>
### RegisterServiceConnection(serviceConnectionName,messageServiceConnection) `method`

##### Summary

Register a service connection that will map to all calls

##### Returns



##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| serviceConnectionName | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | The name of the service connection, not necessarily unique, but can be used for logging and other things |
| messageServiceConnection | [MQContract.Interfaces.Service.IMessageServiceConnection](#T-MQContract-Interfaces-Service-IMessageServiceConnection 'MQContract.Interfaces.Service.IMessageServiceConnection') | The service connection to use for all calls |

<a name='T-MQContract-Interfaces-Service-IPingableMessageServiceConnection'></a>
## IPingableMessageServiceConnection `type`

##### Namespace

MQContract.Interfaces.Service

##### Summary

Extends the base MessageServiceConnection Interface to support service pinging

<a name='M-MQContract-Interfaces-Service-IPingableMessageServiceConnection-PingAsync'></a>
### PingAsync() `method`

##### Summary

Implemented Ping call if avaialble for the underlying service

##### Returns

A Ping Result

##### Parameters

This method has no parameters.

<a name='T-MQContract-Interfaces-Consumers-IPubSubAsyncConsumer`1'></a>
## IPubSubAsyncConsumer\`1 `type`

##### Namespace

MQContract.Interfaces.Consumers

##### Summary

Represents an Asynchronous PubSub Message Consumer to be registered to the ContractConnection

##### Generic Types

| Name | Description |
| ---- | ----------- |
| TMessage | The type of Message that this Consumer will consume |

<a name='M-MQContract-Interfaces-Consumers-IPubSubAsyncConsumer`1-MessageReceivedAsync-MQContract-Interfaces-IReceivedMessage{`0}-'></a>
### MessageReceivedAsync(message) `method`

##### Summary

Called when a message is recieved from the underlying subscription that is using this Consumer

##### Returns

A ValueTask for asynchronous operations

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| message | [MQContract.Interfaces.IReceivedMessage{\`0}](#T-MQContract-Interfaces-IReceivedMessage{`0} 'MQContract.Interfaces.IReceivedMessage{`0}') | The message that was received |

<a name='T-MQContract-Interfaces-Consumers-IPubSubConsumer`1'></a>
## IPubSubConsumer\`1 `type`

##### Namespace

MQContract.Interfaces.Consumers

##### Summary

Represents a PubSub Message Consumer to be registered to the ContractConnection

##### Generic Types

| Name | Description |
| ---- | ----------- |
| TMessage | The type of Message that this Consumer will consume |

<a name='M-MQContract-Interfaces-Consumers-IPubSubConsumer`1-MessageReceived-MQContract-Interfaces-IReceivedMessage{`0}-'></a>
### MessageReceived(message) `method`

##### Summary

Called when a message is recieved from the underlying subscription that is using this Consumer

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| message | [MQContract.Interfaces.IReceivedMessage{\`0}](#T-MQContract-Interfaces-IReceivedMessage{`0} 'MQContract.Interfaces.IReceivedMessage{`0}') | The message that was received |

<a name='T-MQContract-Interfaces-Consumers-IQueryResponseAsyncConsumer`2'></a>
## IQueryResponseAsyncConsumer\`2 `type`

##### Namespace

MQContract.Interfaces.Consumers

##### Summary

Represents an Asynchronous QueryResponse Message Consumer to be registered to the ContractConnection

##### Generic Types

| Name | Description |
| ---- | ----------- |
| TQuery | The type of Message that is received and will be consumed |
| TQueryResponse | The type of Message that is returned as a response |

<a name='M-MQContract-Interfaces-Consumers-IQueryResponseAsyncConsumer`2-MessageReceivedAsync-MQContract-Interfaces-IReceivedMessage{`0}-'></a>
### MessageReceivedAsync(message) `method`

##### Summary

Called when a message is received from the underlying subscript that is using this Consumer

##### Returns

The Response to the given Query Message

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| message | [MQContract.Interfaces.IReceivedMessage{\`0}](#T-MQContract-Interfaces-IReceivedMessage{`0} 'MQContract.Interfaces.IReceivedMessage{`0}') | The message that was received |

<a name='T-MQContract-Interfaces-Consumers-IQueryResponseConsumer`2'></a>
## IQueryResponseConsumer\`2 `type`

##### Namespace

MQContract.Interfaces.Consumers

##### Summary

Represents a QueryResponse Message Consumer to be registered to the ContractConnection

##### Generic Types

| Name | Description |
| ---- | ----------- |
| TQuery | The type of Message that is received and will be consumed |
| TQueryResponse | The type of Message that is returned as a response |

<a name='M-MQContract-Interfaces-Consumers-IQueryResponseConsumer`2-MessageReceived-MQContract-Interfaces-IReceivedMessage{`0}-'></a>
### MessageReceived(message) `method`

##### Summary

Called when a message is received from the underlying subscript that is using this Consumer

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| message | [MQContract.Interfaces.IReceivedMessage{\`0}](#T-MQContract-Interfaces-IReceivedMessage{`0} 'MQContract.Interfaces.IReceivedMessage{`0}') | The message that was received |

<a name='T-MQContract-Interfaces-Service-IQueryResponseMessageServiceConnection'></a>
## IQueryResponseMessageServiceConnection `type`

##### Namespace

MQContract.Interfaces.Service

##### Summary

Extends the base MessageServiceConnection Interface to Response Query messaging methodology if the underlying service supports it

<a name='M-MQContract-Interfaces-Service-IQueryResponseMessageServiceConnection-QueryAsync-MQContract-Messages-ServiceMessage,System-TimeSpan,System-Threading-CancellationToken-'></a>
### QueryAsync(message,timeout,cancellationToken) `method`

##### Summary

Implements a call to submit a response query request into the underlying service

##### Returns

A Query Result instance based on what happened

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| message | [MQContract.Messages.ServiceMessage](#T-MQContract-Messages-ServiceMessage 'MQContract.Messages.ServiceMessage') | The message to query with |
| timeout | [System.TimeSpan](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.TimeSpan 'System.TimeSpan') | The timeout for recieving a response |
| cancellationToken | [System.Threading.CancellationToken](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Threading.CancellationToken 'System.Threading.CancellationToken') | A cancellation token |

<a name='T-MQContract-Interfaces-Service-IQueryableMessageServiceConnection'></a>
## IQueryableMessageServiceConnection `type`

##### Namespace

MQContract.Interfaces.Service

##### Summary

Used to identify a message service that supports response query style messaging, either through inbox or directly

<a name='P-MQContract-Interfaces-Service-IQueryableMessageServiceConnection-DefaultTimeout'></a>
### DefaultTimeout `property`

##### Summary

The default timeout to use for RPC calls when it's not specified

<a name='M-MQContract-Interfaces-Service-IQueryableMessageServiceConnection-SubscribeQueryAsync-System-Func{MQContract-Messages-ReceivedServiceMessage,System-Threading-Tasks-ValueTask{MQContract-Messages-ServiceMessage}},System-Action{System-Exception},System-String,System-String,System-Threading-CancellationToken-'></a>
### SubscribeQueryAsync(messageReceived,errorReceived,channel,group,cancellationToken) `method`

##### Summary

Implements a call to create a subscription to a given channel as a member of a given group for responding to queries

##### Returns

A service subscription object

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| messageReceived | [System.Func{MQContract.Messages.ReceivedServiceMessage,System.Threading.Tasks.ValueTask{MQContract.Messages.ServiceMessage}}](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Func 'System.Func{MQContract.Messages.ReceivedServiceMessage,System.Threading.Tasks.ValueTask{MQContract.Messages.ServiceMessage}}') | The callback to be invoked when a message is received, returning the response message |
| errorReceived | [System.Action{System.Exception}](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Action 'System.Action{System.Exception}') | The callback to invoke when an exception occurs |
| channel | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | The name of the channel to subscribe to |
| group | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | The group to bind a consumer to |
| cancellationToken | [System.Threading.CancellationToken](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Threading.CancellationToken 'System.Threading.CancellationToken') | A cancellation token |

<a name='T-MQContract-Interfaces-IReceivedMessage`1'></a>
## IReceivedMessage\`1 `type`

##### Namespace

MQContract.Interfaces

##### Summary

An interface for describing a Message received on a Subscription to be passed into the appropriate callback

##### Generic Types

| Name | Description |
| ---- | ----------- |
| TMessage | The class type of the underlying message |

<a name='P-MQContract-Interfaces-IReceivedMessage`1-Activity'></a>
### Activity `property`

##### Summary

The Activity, used for OTel associated with this recieved message

<a name='P-MQContract-Interfaces-IReceivedMessage`1-Headers'></a>
### Headers `property`

##### Summary

The headers that were supplied with the message

<a name='P-MQContract-Interfaces-IReceivedMessage`1-ID'></a>
### ID `property`

##### Summary

The unique ID of the received message that was specified on the transmission side

<a name='P-MQContract-Interfaces-IReceivedMessage`1-Message'></a>
### Message `property`

##### Summary

The message that was transmitted

<a name='P-MQContract-Interfaces-IReceivedMessage`1-ProcessedTimestamp'></a>
### ProcessedTimestamp `property`

##### Summary

The timestamp of when the received message was converted into the actual class prior to calling the callback

<a name='P-MQContract-Interfaces-IReceivedMessage`1-ReceivedTimestamp'></a>
### ReceivedTimestamp `property`

##### Summary

The timestamp of when the message was received by the underlying service connection

<a name='T-MQContract-Interfaces-IResilientContractConnection`1'></a>
## IResilientContractConnection\`1 `type`

##### Namespace

MQContract.Interfaces

##### Summary

This interface represents the Resiliency extensions for the ContractConnection

##### Generic Types

| Name | Description |
| ---- | ----------- |
| TContractConnection | The underlying type that is being represented here which must be IBaseContractConnection, CC is used for method chaining. |

##### Remarks

All policies are applied against a response where the Error is set and the Error is not Fatal.
Resilience policies are selected in the following order, whichever one matches first:
Channel
Type
Default

<a name='M-MQContract-Interfaces-IResilientContractConnection`1-RegisterResiliencePolicy-System-Nullable{System-ValueTuple{System-Int32,System-Func{System-Int32,System-TimeSpan}}},System-Nullable{System-ValueTuple{System-Int32,System-TimeSpan}}-'></a>
### RegisterResiliencePolicy(retryPolicy,circuitBreakPolicy) `method`

##### Summary

Register a default resiliency policy that will apply to any message transmissions that do not have a specific policy

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| retryPolicy | [System.Nullable{System.ValueTuple{System.Int32,System.Func{System.Int32,System.TimeSpan}}}](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Nullable 'System.Nullable{System.ValueTuple{System.Int32,System.Func{System.Int32,System.TimeSpan}}}') | The settings to use for retries if desired |
| circuitBreakPolicy | [System.Nullable{System.ValueTuple{System.Int32,System.TimeSpan}}](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Nullable 'System.Nullable{System.ValueTuple{System.Int32,System.TimeSpan}}') | The settings to use for circuit breaking if desired |

<a name='M-MQContract-Interfaces-IResilientContractConnection`1-RegisterResiliencePolicy-System-Type,System-Nullable{System-ValueTuple{System-Int32,System-Func{System-Int32,System-TimeSpan}}},System-Nullable{System-ValueTuple{System-Int32,System-TimeSpan}}-'></a>
### RegisterResiliencePolicy(messageType,retryPolicy,circuitBreakPolicy) `method`

##### Summary

Register a resiliency policy that will apply to any message transmission of the given messageType

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| messageType | [System.Type](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Type 'System.Type') | The type of message to associate this policy to |
| retryPolicy | [System.Nullable{System.ValueTuple{System.Int32,System.Func{System.Int32,System.TimeSpan}}}](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Nullable 'System.Nullable{System.ValueTuple{System.Int32,System.Func{System.Int32,System.TimeSpan}}}') | The settings to use for retries if desired |
| circuitBreakPolicy | [System.Nullable{System.ValueTuple{System.Int32,System.TimeSpan}}](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Nullable 'System.Nullable{System.ValueTuple{System.Int32,System.TimeSpan}}') | The settings to use for circuit breaking if desired |

<a name='M-MQContract-Interfaces-IResilientContractConnection`1-RegisterResiliencePolicy-System-String,System-Nullable{System-ValueTuple{System-Int32,System-Func{System-Int32,System-TimeSpan}}},System-Nullable{System-ValueTuple{System-Int32,System-TimeSpan}}-'></a>
### RegisterResiliencePolicy(messageChannel,retryPolicy,circuitBreakPolicy) `method`

##### Summary

Register a resiliency policy that will apply to any message transmission of a message on the given channel

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| messageChannel | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | The message channel to apply this policy to |
| retryPolicy | [System.Nullable{System.ValueTuple{System.Int32,System.Func{System.Int32,System.TimeSpan}}}](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Nullable 'System.Nullable{System.ValueTuple{System.Int32,System.Func{System.Int32,System.TimeSpan}}}') | The settings to use for retries if desired |
| circuitBreakPolicy | [System.Nullable{System.ValueTuple{System.Int32,System.TimeSpan}}](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Nullable 'System.Nullable{System.ValueTuple{System.Int32,System.TimeSpan}}') | The settings to use for circuit breaking if desired |

<a name='M-MQContract-Interfaces-IResilientContractConnection`1-RegisterResiliencePolicy``1-System-Nullable{System-ValueTuple{System-Int32,System-Func{System-Int32,System-TimeSpan}}},System-Nullable{System-ValueTuple{System-Int32,System-TimeSpan}}-'></a>
### RegisterResiliencePolicy\`\`1(retryPolicy,circuitBreakPolicy) `method`

##### Summary

Register a resiliency policy that will apply to any message transmission of message type T

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| retryPolicy | [System.Nullable{System.ValueTuple{System.Int32,System.Func{System.Int32,System.TimeSpan}}}](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Nullable 'System.Nullable{System.ValueTuple{System.Int32,System.Func{System.Int32,System.TimeSpan}}}') | The settings to use for retries if desired |
| circuitBreakPolicy | [System.Nullable{System.ValueTuple{System.Int32,System.TimeSpan}}](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Nullable 'System.Nullable{System.ValueTuple{System.Int32,System.TimeSpan}}') | The settings to use for circuit breaking if desired |

##### Generic Types

| Name | Description |
| ---- | ----------- |
| TMessage | The type of message to associate this policy to |

<a name='T-MQContract-Interfaces-Service-IServiceSubscription'></a>
## IServiceSubscription `type`

##### Namespace

MQContract.Interfaces.Service

##### Summary

Represents an underlying service level subscription

<a name='M-MQContract-Interfaces-Service-IServiceSubscription-EndAsync'></a>
### EndAsync() `method`

##### Summary

Called to end the subscription

##### Returns

A task to allow for asynchronous ending of the subscription

##### Parameters

This method has no parameters.

<a name='T-MQContract-Interfaces-Middleware-ISpecificTypeMiddleware`1'></a>
## ISpecificTypeMiddleware\`1 `type`

##### Namespace

MQContract.Interfaces.Middleware

##### Summary

Base Specific Type Middleware just used to limit Generic Types for Register Middleware

<a name='T-MQContract-Interfaces-ISubscription'></a>
## ISubscription `type`

##### Namespace

MQContract.Interfaces

##### Summary

This interface represents a Contract Connection Subscription and is used to house and end the subscription

<a name='M-MQContract-Interfaces-ISubscription-EndAsync'></a>
### EndAsync() `method`

##### Summary

Called to end (close off) the subscription

##### Returns

A task that is ending the subscription and closing off the resources for it

##### Parameters

This method has no parameters.

<a name='T-MQContract-InvalidEncoderException'></a>
## InvalidEncoderException `type`

##### Namespace

MQContract

##### Summary

Thrown when the type specified in a UseMQContract for the encoder does not match the contract type

<a name='T-MQContract-InvalidEncryptorException'></a>
## InvalidEncryptorException `type`

##### Namespace

MQContract

##### Summary

Thrown when the type specified in a UseMQContract for the encryptor does not match the contract type

<a name='T-MQContract-MQContractMessageContext'></a>
## MQContractMessageContext `type`

##### Namespace

MQContract

##### Summary

Used to attache messages for the code generator to build up code for to improve performance and potentially handle AOT.  The implementation of this class must be made as 
partial and none of the virtual calls need to be implemented as the code generator will handle that.

##### Example

using MQContract;

namespace Messages;

[UseMqContractAttribute(typeof(Announcement))]
public partial class MyMessageContext : MQContractMessageContext { }

<a name='M-MQContract-MQContractMessageContext-IsMessageCodeGenerated-System-Type-'></a>
### IsMessageCodeGenerated(messageType) `method`

##### Summary

Called to determine if this context handles this particular message type

##### Returns

true if this context instance defines this message type

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| messageType | [System.Type](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Type 'System.Type') | The type of message to check for |

##### Remarks

This will be implemented by the code generator

<a name='M-MQContract-MQContractMessageContext-IsMessageCodeGenerated``1'></a>
### IsMessageCodeGenerated\`\`1() `method`

##### Summary

Called to determine if this context handles this particular message type

##### Returns

true if this context instance defines this message type

##### Parameters

This method has no parameters.

##### Generic Types

| Name | Description |
| ---- | ----------- |
| TMessage | The type of message to check for |

<a name='M-MQContract-MQContractMessageContext-TryExecuteQuery``1-MQContract-Interfaces-IContractConnection,System-Object,System-Nullable{System-TimeSpan},System-String,System-String,MQContract-Messages-MessageHeader,System-Threading-CancellationToken-'></a>
### TryExecuteQuery\`\`1(contractConnection,message,timeout,channel,responseChannel,messageHeader,cancellationToken) `method`

##### Summary

Called to attempt to execute a Query call with an unkown return type

##### Returns

null or the Query attempt against the given connection

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| contractConnection | [MQContract.Interfaces.IContractConnection](#T-MQContract-Interfaces-IContractConnection 'MQContract.Interfaces.IContractConnection') | An instance of the connection to invoke it against |
| message | [System.Object](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Object 'System.Object') | The query message |
| timeout | [System.Nullable{System.TimeSpan}](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Nullable 'System.Nullable{System.TimeSpan}') | The query timeout |
| channel | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | The channel to use |
| responseChannel | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | The response channel to use |
| messageHeader | [MQContract.Messages.MessageHeader](#T-MQContract-Messages-MessageHeader 'MQContract.Messages.MessageHeader') | The message header to use |
| cancellationToken | [System.Threading.CancellationToken](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Threading.CancellationToken 'System.Threading.CancellationToken') | A cancellation token |

##### Generic Types

| Name | Description |
| ---- | ----------- |
| TQuery | The type of message to query with |

##### Remarks

This will be implemented by the code generator

<a name='M-MQContract-MQContractMessageContext-TryExecuteQuery``1-MQContract-Interfaces-IMultiServiceContractConnection,System-Object,System-Nullable{System-TimeSpan},System-String,System-String,MQContract-Messages-MessageHeader,System-Threading-CancellationToken-'></a>
### TryExecuteQuery\`\`1(contractConnection,message,timeout,channel,responseChannel,messageHeader,cancellationToken) `method`

##### Summary

Called to attempt to execute a Query call with an unkown return type

##### Returns

null or the Query attempt against the given connection

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| contractConnection | [MQContract.Interfaces.IMultiServiceContractConnection](#T-MQContract-Interfaces-IMultiServiceContractConnection 'MQContract.Interfaces.IMultiServiceContractConnection') | An instance of the connection to invoke it against |
| message | [System.Object](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Object 'System.Object') | The query message |
| timeout | [System.Nullable{System.TimeSpan}](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Nullable 'System.Nullable{System.TimeSpan}') | The query timeout |
| channel | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | The channel to use |
| responseChannel | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | The response channel to use |
| messageHeader | [MQContract.Messages.MessageHeader](#T-MQContract-Messages-MessageHeader 'MQContract.Messages.MessageHeader') | The message header to use |
| cancellationToken | [System.Threading.CancellationToken](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Threading.CancellationToken 'System.Threading.CancellationToken') | A cancellation token |

##### Generic Types

| Name | Description |
| ---- | ----------- |
| TQuery | The type of message to query with |

##### Remarks

This will be implemented by the code generator

<a name='M-MQContract-MQContractMessageContext-TryGetDecodingCallback-System-String,MQContract-Interfaces-Encoding-IMessageEncoder,System-IServiceProvider-'></a>
### TryGetDecodingCallback(messageID,globalMessageEncoder,serviceProvider) `method`

##### Summary

Called to attempt to get the DecodingCallback for a given message

##### Returns

null or an instance of a decode callback

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| messageID | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | The message type id of the service message |
| globalMessageEncoder | [MQContract.Interfaces.Encoding.IMessageEncoder](#T-MQContract-Interfaces-Encoding-IMessageEncoder 'MQContract.Interfaces.Encoding.IMessageEncoder') | The global message encoder specified for this contract if any |
| serviceProvider | [System.IServiceProvider](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.IServiceProvider 'System.IServiceProvider') | An instance of the ServiceProvider used for DI if available |

##### Remarks

This will be implemented by the code generator

<a name='M-MQContract-MQContractMessageContext-TryGetMessageConverter``1-System-String,System-Func{MQContract-Interfaces-Messages-IEncodedMessage,System-Threading-Tasks-ValueTask{System-Object}},System-IServiceProvider-'></a>
### TryGetMessageConverter\`\`1(messageID,messageDecode,serviceProvider) `method`

##### Summary

Called to obtain a Message Converter to convert from the given message type id to the destination

##### Returns

null or an instance of a conversion callback

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| messageID | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | The message type id of the service message |
| messageDecode | [System.Func{MQContract.Interfaces.Messages.IEncodedMessage,System.Threading.Tasks.ValueTask{System.Object}}](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Func 'System.Func{MQContract.Interfaces.Messages.IEncodedMessage,System.Threading.Tasks.ValueTask{System.Object}}') | The service message decode call back obtained in another call to be able to decode the service message |
| serviceProvider | [System.IServiceProvider](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.IServiceProvider 'System.IServiceProvider') | An instance of the ServiceProvider used for DI if available |

##### Generic Types

| Name | Description |
| ---- | ----------- |
| TMessage | The type of message to convert to |

##### Remarks

This will be implemented by the code generator

<a name='M-MQContract-MQContractMessageContext-TryGetMessageEncoder``1-MQContract-Interfaces-Encoding-IMessageEncoder,System-IServiceProvider-'></a>
### TryGetMessageEncoder\`\`1(globalMessageEncoder,serviceProvider) `method`

##### Summary

Called to attempt to get the MessageEncoder specified for a given message.

##### Returns

null or an instance of an encoder to use

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| globalMessageEncoder | [MQContract.Interfaces.Encoding.IMessageEncoder](#T-MQContract-Interfaces-Encoding-IMessageEncoder 'MQContract.Interfaces.Encoding.IMessageEncoder') | The global message encoder specified for this contract if any |
| serviceProvider | [System.IServiceProvider](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.IServiceProvider 'System.IServiceProvider') | An instance of the ServiceProvider used for DI if available |

##### Generic Types

| Name | Description |
| ---- | ----------- |
| TMessage | The type of message to locate the encoder for |

##### Remarks

This will be implemented by the code generator

<a name='M-MQContract-MQContractMessageContext-TryGetMessageEncryptor-System-Type,MQContract-Interfaces-Encrypting-IMessageEncryptor,System-IServiceProvider-'></a>
### TryGetMessageEncryptor(messageType,globalEncryptor,serviceProvider) `method`

##### Summary

Called to obtain a Message Encryptor for a given message type

##### Returns

null or an instance of an encryptor for the given message

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| messageType | [System.Type](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Type 'System.Type') | The type of message to check for |
| globalEncryptor | [MQContract.Interfaces.Encrypting.IMessageEncryptor](#T-MQContract-Interfaces-Encrypting-IMessageEncryptor 'MQContract.Interfaces.Encrypting.IMessageEncryptor') | The instance of the global encryptor for the connection if supplied |
| serviceProvider | [System.IServiceProvider](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.IServiceProvider 'System.IServiceProvider') | An instance of the ServiceProvider used for DI if available |

##### Remarks

This will be implemented by the code generator

<a name='M-MQContract-MQContractMessageContext-TryGetMessageType-System-Type-'></a>
### TryGetMessageType(messageType) `method`

##### Summary

Called to obtain the Message Definition housed if available

##### Returns

null or the definition for the given message type

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| messageType | [System.Type](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Type 'System.Type') | The type of message to locate |

##### Remarks

This will be implemented by the code generator

<a name='T-MQContract-Attributes-MQContractMessageContextAttribute'></a>
## MQContractMessageContextAttribute `type`

##### Namespace

MQContract.Attributes

##### Summary

Used as part of the Message context for code generation to specify auto scanning settings.  Must be attached to an implementation of MQContractMessageContext.

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| locateEncoders | [T:MQContract.Attributes.MQContractMessageContextAttribute](#T-T-MQContract-Attributes-MQContractMessageContextAttribute 'T:MQContract.Attributes.MQContractMessageContextAttribute') | Set to true if you want to automatically scan for encoders for a message when none are specified |

<a name='M-MQContract-Attributes-MQContractMessageContextAttribute-#ctor-System-Boolean,System-Boolean,System-Boolean-'></a>
### #ctor(locateEncoders,locateConverters,locateEncryptors) `constructor`

##### Summary

Used as part of the Message context for code generation to specify auto scanning settings.  Must be attached to an implementation of MQContractMessageContext.

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| locateEncoders | [System.Boolean](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Boolean 'System.Boolean') | Set to true if you want to automatically scan for encoders for a message when none are specified |
| locateConverters | [System.Boolean](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Boolean 'System.Boolean') | Set to true if you want to automatically scan for converters for a message when none are specified |
| locateEncryptors | [System.Boolean](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Boolean 'System.Boolean') | Set to true if you want to automatically scan for encryptors for a message when none are specified |

<a name='P-MQContract-Attributes-MQContractMessageContextAttribute-LocateConverters'></a>
### LocateConverters `property`

##### Summary

Indicates if Converters are to be located when none are specified

<a name='P-MQContract-Attributes-MQContractMessageContextAttribute-LocateEncoders'></a>
### LocateEncoders `property`

##### Summary

Indicates if Encoders are to be located when none are specified

<a name='P-MQContract-Attributes-MQContractMessageContextAttribute-LocateEncryptors'></a>
### LocateEncryptors `property`

##### Summary

Indicates if Encryptors are to be located when none are specified

<a name='T-MQContract-Attributes-MessageAttribute'></a>
## MessageAttribute `type`

##### Namespace

MQContract.Attributes

##### Summary

Use this attribute to specify the Channel, TypeName and or Type Version of the 
Message being defined

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| channel | [T:MQContract.Attributes.MessageAttribute](#T-T-MQContract-Attributes-MessageAttribute 'T:MQContract.Attributes.MessageAttribute') | The channel to be used |

##### Example

```
[Message(channel: "Arrivals")]
public record ArrivalAnnouncement(string FirstName, string LastName);
```

##### Remarks



<a name='M-MQContract-Attributes-MessageAttribute-#ctor-System-String,System-String,System-String-'></a>
### #ctor(channel,typeName,typeVersion) `constructor`

##### Summary

Use this attribute to specify the Channel, TypeName and or Type Version of the 
Message being defined

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| channel | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | The channel to be used |
| typeName | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | The message type to use |
| typeVersion | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | The message version to use |

##### Example

```
[Message(channel: "Arrivals")]
public record ArrivalAnnouncement(string FirstName, string LastName);
```

##### Remarks



<a name='P-MQContract-Attributes-MessageAttribute-Channel'></a>
### Channel `property`

##### Summary

The Channel specified

<a name='P-MQContract-Attributes-MessageAttribute-TypeName'></a>
### TypeName `property`

##### Summary

The name of the message type used when transmitting

<a name='P-MQContract-Attributes-MessageAttribute-TypeVersion'></a>
### TypeVersion `property`

##### Summary

The version number to tag this message with during transmission

<a name='T-MQContract-MessageFilterResult'></a>
## MessageFilterResult `type`

##### Namespace

MQContract

##### Summary

These are the possible message filtering responses when supplying a message filtering action

<a name='F-MQContract-MessageFilterResult-Allow'></a>
### Allow `constants`

##### Summary

Allow the message to continue through

<a name='F-MQContract-MessageFilterResult-DropAndAcknowledge'></a>
### DropAndAcknowledge `constants`

##### Summary

Do not allow the message to continue to the callback and Acknowledge it within the service

<a name='F-MQContract-MessageFilterResult-DropAndDontAcknowledge'></a>
### DropAndDontAcknowledge `constants`

##### Summary

Do not allow the message to continue to the callback and do not Acknowledge it within the service

<a name='T-MQContract-Messages-MessageFilters`1'></a>
## MessageFilters\`1 `type`

##### Namespace

MQContract.Messages

##### Summary

Houses a set of message filtering calls for a given type.  This particular record can be passed in to pubsub consumers/subscriptions
to implement some pre-callback message filtering when receiving messages

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| HeaderFilter | [T:MQContract.Messages.MessageFilters\`1](#T-T-MQContract-Messages-MessageFilters`1 'T:MQContract.Messages.MessageFilters`1') | A callback filter used to filter a message by headers.  This call is made prior to attempting to convert the message into the appropriate type. |

##### Generic Types

| Name | Description |
| ---- | ----------- |
| TMessage | The type of message that the subscription and filter represents |

<a name='M-MQContract-Messages-MessageFilters`1-#ctor-System-Func{MQContract-Messages-MessageHeader,System-Threading-Tasks-ValueTask{MQContract-MessageFilterResult}},System-Func{`0,MQContract-Messages-MessageHeader,System-Threading-Tasks-ValueTask{MQContract-MessageFilterResult}}-'></a>
### #ctor(HeaderFilter,MessageFilter) `constructor`

##### Summary

Houses a set of message filtering calls for a given type.  This particular record can be passed in to pubsub consumers/subscriptions
to implement some pre-callback message filtering when receiving messages

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| HeaderFilter | [System.Func{MQContract.Messages.MessageHeader,System.Threading.Tasks.ValueTask{MQContract.MessageFilterResult}}](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Func 'System.Func{MQContract.Messages.MessageHeader,System.Threading.Tasks.ValueTask{MQContract.MessageFilterResult}}') | A callback filter used to filter a message by headers.  This call is made prior to attempting to convert the message into the appropriate type. |
| MessageFilter | [System.Func{\`0,MQContract.Messages.MessageHeader,System.Threading.Tasks.ValueTask{MQContract.MessageFilterResult}}](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Func 'System.Func{`0,MQContract.Messages.MessageHeader,System.Threading.Tasks.ValueTask{MQContract.MessageFilterResult}}') | A callback filter used to filter a message by the message and or headers.  This call is made after the attempt to convert the message into the approriate type. |

##### Generic Types

| Name | Description |
| ---- | ----------- |
| TMessage | The type of message that the subscription and filter represents |

<a name='P-MQContract-Messages-MessageFilters`1-HeaderFilter'></a>
### HeaderFilter `property`

##### Summary

A callback filter used to filter a message by headers.  This call is made prior to attempting to convert the message into the appropriate type.

<a name='P-MQContract-Messages-MessageFilters`1-MessageFilter'></a>
### MessageFilter `property`

##### Summary

A callback filter used to filter a message by the message and or headers.  This call is made after the attempt to convert the message into the approriate type.

<a name='T-MQContract-Messages-MessageHeader'></a>
## MessageHeader `type`

##### Namespace

MQContract.Messages

##### Summary

Houses additional headers to be passed through or that were passed along the service message

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| data | [T:MQContract.Messages.MessageHeader](#T-T-MQContract-Messages-MessageHeader 'T:MQContract.Messages.MessageHeader') | A list of KeyValuePairs that make up the header |

<a name='M-MQContract-Messages-MessageHeader-#ctor-System-Collections-Generic-IEnumerable{System-Collections-Generic-KeyValuePair{System-String,System-String}}-'></a>
### #ctor(data) `constructor`

##### Summary

Houses additional headers to be passed through or that were passed along the service message

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| data | [System.Collections.Generic.IEnumerable{System.Collections.Generic.KeyValuePair{System.String,System.String}}](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Collections.Generic.IEnumerable 'System.Collections.Generic.IEnumerable{System.Collections.Generic.KeyValuePair{System.String,System.String}}') | A list of KeyValuePairs that make up the header |

<a name='M-MQContract-Messages-MessageHeader-#ctor-System-Collections-Generic-Dictionary{System-String,System-String}-'></a>
### #ctor(headers) `constructor`

##### Summary

Constructor to create the MessageHeader class using a Dictionary

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| headers | [System.Collections.Generic.Dictionary{System.String,System.String}](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Collections.Generic.Dictionary 'System.Collections.Generic.Dictionary{System.String,System.String}') | The desired data for the header |

<a name='M-MQContract-Messages-MessageHeader-#ctor-MQContract-Messages-MessageHeader,System-Collections-Generic-Dictionary{System-String,System-String}-'></a>
### #ctor(originalHeader,appendedHeader) `constructor`

##### Summary

Constructor to create a merged message header with taking the original and appending the new values

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| originalHeader | [MQContract.Messages.MessageHeader](#T-MQContract-Messages-MessageHeader 'MQContract.Messages.MessageHeader') | The base header to use |
| appendedHeader | [System.Collections.Generic.Dictionary{System.String,System.String}](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Collections.Generic.Dictionary 'System.Collections.Generic.Dictionary{System.String,System.String}') | The additional properties to add |

<a name='M-MQContract-Messages-MessageHeader-#ctor-MQContract-Messages-MessageHeader,MQContract-Messages-MessageHeader-'></a>
### #ctor(originalHeader,appendedHeader) `constructor`

##### Summary

Constructor to create a merged message header with taking the original and appending the new values

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| originalHeader | [MQContract.Messages.MessageHeader](#T-MQContract-Messages-MessageHeader 'MQContract.Messages.MessageHeader') | The base header to use |
| appendedHeader | [MQContract.Messages.MessageHeader](#T-MQContract-Messages-MessageHeader 'MQContract.Messages.MessageHeader') | The additional properties to add |

<a name='P-MQContract-Messages-MessageHeader-Item-System-String-'></a>
### Item `property`

##### Summary

Called to obtain a header value for the given key if it exists

##### Returns

The value for the given key or null if not found

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| tagKey | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | The unique header key to get the value for |

<a name='P-MQContract-Messages-MessageHeader-Keys'></a>
### Keys `property`

##### Summary

A list of the available keys in the header

<a name='T-MQContract-MQContractMessageContext-MessageTypeDefinition'></a>
## MessageTypeDefinition `type`

##### Namespace

MQContract.MQContractMessageContext

##### Summary

Used to house Message Type Definitions that are built both through attributes and other class aspects

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| Channel | [T:MQContract.MQContractMessageContext.MessageTypeDefinition](#T-T-MQContract-MQContractMessageContext-MessageTypeDefinition 'T:MQContract.MQContractMessageContext.MessageTypeDefinition') | The channel that the message is to use by default |

<a name='M-MQContract-MQContractMessageContext-MessageTypeDefinition-#ctor-System-String,System-String,System-Version,System-String,System-Nullable{System-TimeSpan},System-Type-'></a>
### #ctor(Channel,TypeName,TypeVersion,ResponseChannel,ResponseTimeout,ResponseType) `constructor`

##### Summary

Used to house Message Type Definitions that are built both through attributes and other class aspects

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| Channel | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | The channel that the message is to use by default |
| TypeName | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | The Type Name used within the Message ID |
| TypeVersion | [System.Version](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Version 'System.Version') | The Version used within the Message ID |
| ResponseChannel | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | The response channel to use if specified |
| ResponseTimeout | [System.Nullable{System.TimeSpan}](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Nullable 'System.Nullable{System.TimeSpan}') | The response timeout to use if specified |
| ResponseType | [System.Type](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Type 'System.Type') | The response type to use if specified |

<a name='P-MQContract-MQContractMessageContext-MessageTypeDefinition-Channel'></a>
### Channel `property`

##### Summary

The channel that the message is to use by default

<a name='P-MQContract-MQContractMessageContext-MessageTypeDefinition-ResponseChannel'></a>
### ResponseChannel `property`

##### Summary

The response channel to use if specified

<a name='P-MQContract-MQContractMessageContext-MessageTypeDefinition-ResponseTimeout'></a>
### ResponseTimeout `property`

##### Summary

The response timeout to use if specified

<a name='P-MQContract-MQContractMessageContext-MessageTypeDefinition-ResponseType'></a>
### ResponseType `property`

##### Summary

The response type to use if specified

<a name='P-MQContract-MQContractMessageContext-MessageTypeDefinition-TypeName'></a>
### TypeName `property`

##### Summary

The Type Name used within the Message ID

<a name='P-MQContract-MQContractMessageContext-MessageTypeDefinition-TypeVersion'></a>
### TypeVersion `property`

##### Summary

The Version used within the Message ID

<a name='T-MQContract-Messages-MultiTransmissionResult'></a>
## MultiTransmissionResult `type`

##### Namespace

MQContract.Messages

##### Summary

Houses the result of a transmission into the system when using the MultiService method

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| ID | [T:MQContract.Messages.MultiTransmissionResult](#T-T-MQContract-Messages-MultiTransmissionResult 'T:MQContract.Messages.MultiTransmissionResult') | The unique ID of the message that was transmitted |

<a name='M-MQContract-Messages-MultiTransmissionResult-#ctor-System-String,System-Collections-Generic-IEnumerable{MQContract-Messages-ChildTransmissionResult}-'></a>
### #ctor(ID,Results) `constructor`

##### Summary

Houses the result of a transmission into the system when using the MultiService method

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| ID | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | The unique ID of the message that was transmitted |
| Results | [System.Collections.Generic.IEnumerable{MQContract.Messages.ChildTransmissionResult}](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Collections.Generic.IEnumerable 'System.Collections.Generic.IEnumerable{MQContract.Messages.ChildTransmissionResult}') | Houses all the results from each underlying system connection used |

<a name='P-MQContract-Messages-MultiTransmissionResult-HasError'></a>
### HasError `property`

##### Summary

Flag to indicate if there are any errors in the result

<a name='P-MQContract-Messages-MultiTransmissionResult-ID'></a>
### ID `property`

##### Summary

The unique ID of the message that was transmitted

<a name='P-MQContract-Messages-MultiTransmissionResult-Results'></a>
### Results `property`

##### Summary

Houses all the results from each underlying system connection used

<a name='T-MQContract-PingFailedException'></a>
## PingFailedException `type`

##### Namespace

MQContract

##### Summary

Thrown when a Ping Attempt fails

<a name='M-MQContract-PingFailedException-#ctor-System-String-'></a>
### #ctor() `constructor`

##### Summary

Thrown when a Ping Attempt fails

##### Parameters

This constructor has no parameters.

<a name='T-MQContract-Messages-PingResult'></a>
## PingResult `type`

##### Namespace

MQContract.Messages

##### Summary

Houses the results from a Ping call against a given underlying service

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| Host | [T:MQContract.Messages.PingResult](#T-T-MQContract-Messages-PingResult 'T:MQContract.Messages.PingResult') | The host name of the service, if provided |

<a name='M-MQContract-Messages-PingResult-#ctor-System-String,System-String,System-TimeSpan-'></a>
### #ctor(Host,Version,ResponseTime) `constructor`

##### Summary

Houses the results from a Ping call against a given underlying service

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| Host | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | The host name of the service, if provided |
| Version | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | The version of the service running, if provided |
| ResponseTime | [System.TimeSpan](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.TimeSpan 'System.TimeSpan') | How long it took for the server to respond |

<a name='P-MQContract-Messages-PingResult-Host'></a>
### Host `property`

##### Summary

The host name of the service, if provided

<a name='P-MQContract-Messages-PingResult-ResponseTime'></a>
### ResponseTime `property`

##### Summary

How long it took for the server to respond

<a name='P-MQContract-Messages-PingResult-Version'></a>
### Version `property`

##### Summary

The version of the service running, if provided

<a name='T-MQContract-Attributes-QueryMessageAttribute'></a>
## QueryMessageAttribute `type`

##### Namespace

MQContract.Attributes

##### Summary

Use this attribute to specify the Channel, TypeName, TypeVersion, ResponseChannel, DefaultTimeout and/or ResponseType 
of the given query call.
IMPORTANT:  The response channel value should either be specified here or on a given query call when the underlying service connection does not support 
either QueryResponse or Inbox style messaging

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| channel | [T:MQContract.Attributes.QueryMessageAttribute](#T-T-MQContract-Attributes-QueryMessageAttribute 'T:MQContract.Attributes.QueryMessageAttribute') | The channel to be used |

<a name='M-MQContract-Attributes-QueryMessageAttribute-#ctor-System-String,System-String,System-String,System-String,System-Int32,System-Type-'></a>
### #ctor(channel,typeName,typeVersion,responseChannel,responseTimeoutMilliseconds,responseType) `constructor`

##### Summary

Use this attribute to specify the Channel, TypeName, TypeVersion, ResponseChannel, DefaultTimeout and/or ResponseType 
of the given query call.
IMPORTANT:  The response channel value should either be specified here or on a given query call when the underlying service connection does not support 
either QueryResponse or Inbox style messaging

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| channel | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | The channel to be used |
| typeName | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | The query type to use |
| typeVersion | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | The query type version to use |
| responseChannel | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | The responce channel to be used when an underlying service connection does not support QueryResponse or Inbox |
| responseTimeoutMilliseconds | [System.Int32](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Int32 'System.Int32') | The query response timeout to default to |
| responseType | [System.Type](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Type 'System.Type') | The expected response type for the query |

<a name='P-MQContract-Attributes-QueryMessageAttribute-ResponseChannel'></a>
### ResponseChannel `property`

##### Summary

The Response Channel defined for the given query

<a name='P-MQContract-Attributes-QueryMessageAttribute-ResponseTimeout'></a>
### ResponseTimeout `property`

##### Summary

The Response Timeout defined for the given query

<a name='P-MQContract-Attributes-QueryMessageAttribute-ResponseType'></a>
### ResponseType `property`

##### Summary

The Response Type defined for the given query

<a name='T-MQContract-Messages-QueryResponseMessage`1'></a>
## QueryResponseMessage\`1 `type`

##### Namespace

MQContract.Messages

##### Summary

Houses the Query Response Message to be sent back from a query call

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| Message | [T:MQContract.Messages.QueryResponseMessage\`1](#T-T-MQContract-Messages-QueryResponseMessage`1 'T:MQContract.Messages.QueryResponseMessage`1') | The message to respond back with |

##### Generic Types

| Name | Description |
| ---- | ----------- |
| TQueryResponse | The type of message contained in the response |

<a name='M-MQContract-Messages-QueryResponseMessage`1-#ctor-`0,System-Collections-Generic-Dictionary{System-String,System-String}-'></a>
### #ctor(Message,Headers) `constructor`

##### Summary

Houses the Query Response Message to be sent back from a query call

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| Message | [\`0](#T-`0 '`0') | The message to respond back with |
| Headers | [System.Collections.Generic.Dictionary{System.String,System.String}](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Collections.Generic.Dictionary 'System.Collections.Generic.Dictionary{System.String,System.String}') | The headers to attach to the response |

##### Generic Types

| Name | Description |
| ---- | ----------- |
| TQueryResponse | The type of message contained in the response |

<a name='P-MQContract-Messages-QueryResponseMessage`1-Headers'></a>
### Headers `property`

##### Summary

The headers to attach to the response

<a name='P-MQContract-Messages-QueryResponseMessage`1-Message'></a>
### Message `property`

##### Summary

The message to respond back with

<a name='T-MQContract-Messages-QueryResult`1'></a>
## QueryResult\`1 `type`

##### Namespace

MQContract.Messages

##### Summary

Houses the result from a Query call into the system

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| ID | [T:MQContract.Messages.QueryResult\`1](#T-T-MQContract-Messages-QueryResult`1 'T:MQContract.Messages.QueryResult`1') | The unique ID of the message |

##### Generic Types

| Name | Description |
| ---- | ----------- |
| TQueryResponse | The type of message in the response |

<a name='M-MQContract-Messages-QueryResult`1-#ctor-System-String,MQContract-Messages-MessageHeader,`0,MQContract-Messages-ErrorMessage-'></a>
### #ctor(ID,Header,Result,Error) `constructor`

##### Summary

Houses the result from a Query call into the system

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| ID | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | The unique ID of the message |
| Header | [MQContract.Messages.MessageHeader](#T-MQContract-Messages-MessageHeader 'MQContract.Messages.MessageHeader') | The response headers |
| Result | [\`0](#T-`0 '`0') | The resulting response if there was one |
| Error | [MQContract.Messages.ErrorMessage](#T-MQContract-Messages-ErrorMessage 'MQContract.Messages.ErrorMessage') | The error message for the response if it failed and an error was returned |

##### Generic Types

| Name | Description |
| ---- | ----------- |
| TQueryResponse | The type of message in the response |

<a name='P-MQContract-Messages-QueryResult`1-Header'></a>
### Header `property`

##### Summary

The response headers

<a name='P-MQContract-Messages-QueryResult`1-Result'></a>
### Result `property`

##### Summary

The resulting response if there was one

<a name='T-MQContract-Messages-ReceivedInboxServiceMessage'></a>
## ReceivedInboxServiceMessage `type`

##### Namespace

MQContract.Messages

##### Summary

A Received Service Message that gets passed back up into the Contract Connection when a message is received from the underlying service connection

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| ID | [T:MQContract.Messages.ReceivedInboxServiceMessage](#T-T-MQContract-Messages-ReceivedInboxServiceMessage 'T:MQContract.Messages.ReceivedInboxServiceMessage') | The unique ID of the message |

<a name='M-MQContract-Messages-ReceivedInboxServiceMessage-#ctor-System-String,System-String,System-String,MQContract-Messages-MessageHeader,System-Guid,System-ReadOnlyMemory{System-Byte},System-Func{System-Threading-Tasks-ValueTask}-'></a>
### #ctor(ID,MessageTypeID,Channel,Header,CorrelationID,Data,Acknowledge) `constructor`

##### Summary

A Received Service Message that gets passed back up into the Contract Connection when a message is received from the underlying service connection

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| ID | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | The unique ID of the message |
| MessageTypeID | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | The message type id which is used for decoding to a class |
| Channel | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | The channel the message was received on |
| Header | [MQContract.Messages.MessageHeader](#T-MQContract-Messages-MessageHeader 'MQContract.Messages.MessageHeader') | The message headers that came through |
| CorrelationID | [System.Guid](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Guid 'System.Guid') | The query message correlation id supplied by the query call to tie to the response |
| Data | [System.ReadOnlyMemory{System.Byte}](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.ReadOnlyMemory 'System.ReadOnlyMemory{System.Byte}') | The binary content of the message that should be the encoded class |
| Acknowledge | [System.Func{System.Threading.Tasks.ValueTask}](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Func 'System.Func{System.Threading.Tasks.ValueTask}') | The acknowledgement callback to be called when the message is received if the underlying service requires it |

<a name='P-MQContract-Messages-ReceivedInboxServiceMessage-CorrelationID'></a>
### CorrelationID `property`

##### Summary

The query message correlation id supplied by the query call to tie to the response

<a name='T-MQContract-Messages-ReceivedServiceMessage'></a>
## ReceivedServiceMessage `type`

##### Namespace

MQContract.Messages

##### Summary

A Received Service Message that gets passed back up into the Contract Connection when a message is received from the underlying service connection

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| ID | [T:MQContract.Messages.ReceivedServiceMessage](#T-T-MQContract-Messages-ReceivedServiceMessage 'T:MQContract.Messages.ReceivedServiceMessage') | The unique ID of the message |

<a name='M-MQContract-Messages-ReceivedServiceMessage-#ctor-System-String,System-String,System-String,MQContract-Messages-MessageHeader,System-ReadOnlyMemory{System-Byte},System-Func{System-Threading-Tasks-ValueTask}-'></a>
### #ctor(ID,MessageTypeID,Channel,Header,Data,Acknowledge) `constructor`

##### Summary

A Received Service Message that gets passed back up into the Contract Connection when a message is received from the underlying service connection

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| ID | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | The unique ID of the message |
| MessageTypeID | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | The message type id which is used for decoding to a class |
| Channel | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | The channel the message was received on |
| Header | [MQContract.Messages.MessageHeader](#T-MQContract-Messages-MessageHeader 'MQContract.Messages.MessageHeader') | The message headers that came through |
| Data | [System.ReadOnlyMemory{System.Byte}](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.ReadOnlyMemory 'System.ReadOnlyMemory{System.Byte}') | The binary content of the message that should be the encoded class |
| Acknowledge | [System.Func{System.Threading.Tasks.ValueTask}](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Func 'System.Func{System.Threading.Tasks.ValueTask}') | The acknowledgement callback to be called when the message is received if the underlying service requires it |

<a name='P-MQContract-Messages-ReceivedServiceMessage-Acknowledge'></a>
### Acknowledge `property`

##### Summary

The acknowledgement callback to be called when the message is received if the underlying service requires it

<a name='P-MQContract-Messages-ReceivedServiceMessage-ReceivedTimestamp'></a>
### ReceivedTimestamp `property`

##### Summary

A timestamp for when the message was received

<a name='T-MQContract-ResilienceException'></a>
## ResilienceException `type`

##### Namespace

MQContract

##### Summary

Thrown when a Resilience failure occurs attempting to transmit a message

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| type | [T:MQContract.ResilienceException](#T-T-MQContract-ResilienceException 'T:MQContract.ResilienceException') | The type of resilience failure that occured |

<a name='M-MQContract-ResilienceException-#ctor-MQContract-ResilienceTypes,System-Exception-'></a>
### #ctor(type,error) `constructor`

##### Summary

Thrown when a Resilience failure occurs attempting to transmit a message

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| type | [MQContract.ResilienceTypes](#T-MQContract-ResilienceTypes 'MQContract.ResilienceTypes') | The type of resilience failure that occured |
| error | [System.Exception](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Exception 'System.Exception') | An underlying error for the resilience (may be a circuit broken or the underlying error that retry has failed through) |

<a name='P-MQContract-ResilienceException-Type'></a>
### Type `property`

##### Summary

The type of resilience failure that has occured

<a name='T-MQContract-ResilienceTypes'></a>
## ResilienceTypes `type`

##### Namespace

MQContract

##### Summary

Houses the different types of Resilience errors that can occur

<a name='F-MQContract-ResilienceTypes-CircuitBreak'></a>
### CircuitBreak `constants`

##### Summary

Failed due to the Circuit being broken based on the policy defined

<a name='F-MQContract-ResilienceTypes-Retry'></a>
### Retry `constants`

##### Summary

Failed through the retry attempts defined

<a name='T-MQContract-Messages-ServiceMessage'></a>
## ServiceMessage `type`

##### Namespace

MQContract.Messages

##### Summary

Houses a service level message that would be supplied to the underlying Service Connection for transmission purposes

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| ID | [T:MQContract.Messages.ServiceMessage](#T-T-MQContract-Messages-ServiceMessage 'T:MQContract.Messages.ServiceMessage') | The unique ID of the message |

<a name='M-MQContract-Messages-ServiceMessage-#ctor-System-String,System-String,System-String,MQContract-Messages-MessageHeader,System-ReadOnlyMemory{System-Byte}-'></a>
### #ctor(ID,MessageTypeID,Channel,Header,Data) `constructor`

##### Summary

Houses a service level message that would be supplied to the underlying Service Connection for transmission purposes

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| ID | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | The unique ID of the message |
| MessageTypeID | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | An identifier that identifies the type of message encoded |
| Channel | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | The channel to transmit the message on |
| Header | [MQContract.Messages.MessageHeader](#T-MQContract-Messages-MessageHeader 'MQContract.Messages.MessageHeader') | The headers to transmit with the message |
| Data | [System.ReadOnlyMemory{System.Byte}](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.ReadOnlyMemory 'System.ReadOnlyMemory{System.Byte}') | The content of the message |

<a name='P-MQContract-Messages-ServiceMessage-Channel'></a>
### Channel `property`

##### Summary

The channel to transmit the message on

<a name='P-MQContract-Messages-ServiceMessage-Data'></a>
### Data `property`

##### Summary

The content of the message

<a name='P-MQContract-Messages-ServiceMessage-Header'></a>
### Header `property`

##### Summary

The headers to transmit with the message

<a name='P-MQContract-Messages-ServiceMessage-ID'></a>
### ID `property`

##### Summary

The unique ID of the message

<a name='P-MQContract-Messages-ServiceMessage-MessageTypeID'></a>
### MessageTypeID `property`

##### Summary

An identifier that identifies the type of message encoded

<a name='T-MQContract-Messages-ServiceQueryResult'></a>
## ServiceQueryResult `type`

##### Namespace

MQContract.Messages

##### Summary

Houses a result from a query call from the Service Connection Level

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| ID | [T:MQContract.Messages.ServiceQueryResult](#T-T-MQContract-Messages-ServiceQueryResult 'T:MQContract.Messages.ServiceQueryResult') | The ID of the message |

<a name='M-MQContract-Messages-ServiceQueryResult-#ctor-System-String,MQContract-Messages-MessageHeader,System-String,System-ReadOnlyMemory{System-Byte}-'></a>
### #ctor(ID,Header,MessageTypeID,Data) `constructor`

##### Summary

Houses a result from a query call from the Service Connection Level

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| ID | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | The ID of the message |
| Header | [MQContract.Messages.MessageHeader](#T-MQContract-Messages-MessageHeader 'MQContract.Messages.MessageHeader') | The headers transmitted |
| MessageTypeID | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | The type of message encoded |
| Data | [System.ReadOnlyMemory{System.Byte}](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.ReadOnlyMemory 'System.ReadOnlyMemory{System.Byte}') | The encoded data of the message |

<a name='P-MQContract-Messages-ServiceQueryResult-Data'></a>
### Data `property`

##### Summary

The encoded data of the message

<a name='P-MQContract-Messages-ServiceQueryResult-Header'></a>
### Header `property`

##### Summary

The headers transmitted

<a name='P-MQContract-Messages-ServiceQueryResult-ID'></a>
### ID `property`

##### Summary

The ID of the message

<a name='P-MQContract-Messages-ServiceQueryResult-MessageTypeID'></a>
### MessageTypeID `property`

##### Summary

The type of message encoded

<a name='T-MQContract-TransmissionException'></a>
## TransmissionException `type`

##### Namespace

MQContract

##### Summary

Thrown when an error occurs attempting to transmit a given message and is flagged if it is fatal or not

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| underlyingError | [T:MQContract.TransmissionException](#T-T-MQContract-TransmissionException 'T:MQContract.TransmissionException') | The underlying error that occured |

<a name='M-MQContract-TransmissionException-#ctor-System-Exception,System-Boolean-'></a>
### #ctor(underlyingError,isFatal) `constructor`

##### Summary

Thrown when an error occurs attempting to transmit a given message and is flagged if it is fatal or not

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| underlyingError | [System.Exception](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Exception 'System.Exception') | The underlying error that occured |
| isFatal | [System.Boolean](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Boolean 'System.Boolean') | Flag if this error is a fatal error (don't run resilience when it is fatal) |

<a name='P-MQContract-TransmissionException-IsFatal'></a>
### IsFatal `property`

##### Summary

Indicates if the error that occured is fatal

<a name='T-MQContract-Messages-TransmissionResult'></a>
## TransmissionResult `type`

##### Namespace

MQContract.Messages

##### Summary

Houses the result of a transmission into the system

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| ID | [T:MQContract.Messages.TransmissionResult](#T-T-MQContract-Messages-TransmissionResult 'T:MQContract.Messages.TransmissionResult') | The unique ID of the message that was transmitted |

<a name='M-MQContract-Messages-TransmissionResult-#ctor-System-String,MQContract-Messages-ErrorMessage-'></a>
### #ctor(ID,Error) `constructor`

##### Summary

Houses the result of a transmission into the system

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| ID | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | The unique ID of the message that was transmitted |
| Error | [MQContract.Messages.ErrorMessage](#T-MQContract-Messages-ErrorMessage 'MQContract.Messages.ErrorMessage') | An error message if an error occured |

<a name='P-MQContract-Messages-TransmissionResult-Error'></a>
### Error `property`

##### Summary

An error message if an error occured

<a name='P-MQContract-Messages-TransmissionResult-ID'></a>
### ID `property`

##### Summary

The unique ID of the message that was transmitted

<a name='P-MQContract-Messages-TransmissionResult-IsError'></a>
### IsError `property`

##### Summary

Flag to indicate if the result is an error

<a name='T-MQContract-Attributes-UseMqContractAttribute'></a>
## UseMqContractAttribute `type`

##### Namespace

MQContract.Attributes

##### Summary

Used to mark a message for a Message Context to be included in the code generation.  Must be attached to an implementation of MQContractMessageContext.

<a name='M-MQContract-Attributes-UseMqContractAttribute-#ctor-System-Type,System-Type,System-Type[],System-Type-'></a>
### #ctor(contractType,encoderType,converters,messageEncryptor) `constructor`

##### Summary

Primary constructor

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| contractType | [System.Type](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Type 'System.Type') | The type of Message(Contract) to link |
| encoderType | [System.Type](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Type 'System.Type') | The type of Encoder to use with it if specifically desired |
| converters | [System.Type[]](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Type[] 'System.Type[]') | The type of Converters to use with it if specifically desired |
| messageEncryptor | [System.Type](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Type 'System.Type') | The type of Encryptor to use with it if specifically desired |

##### Exceptions

| Name | Description |
| ---- | ----------- |
| [System.Exception](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Exception 'System.Exception') |  |

<a name='P-MQContract-Attributes-UseMqContractAttribute-ContractType'></a>
### ContractType `property`

##### Summary

The Message(Contract) type

<a name='P-MQContract-Attributes-UseMqContractAttribute-Converters'></a>
### Converters `property`

##### Summary

The type of Converters to use with it if specified

<a name='P-MQContract-Attributes-UseMqContractAttribute-EncoderType'></a>
### EncoderType `property`

##### Summary

The type of Encoder to use with it if specified

<a name='P-MQContract-Attributes-UseMqContractAttribute-MessageEncryptor'></a>
### MessageEncryptor `property`

##### Summary

The type of Encryptor to use with it if specified

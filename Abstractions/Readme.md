<a name='assembly'></a>
# MQContract.Abstractions

## Contents

- [IAfterDecodeMiddleware](#T-MQContract-Interfaces-Middleware-IAfterDecodeMiddleware 'MQContract.Interfaces.Middleware.IAfterDecodeMiddleware')
  - [AfterMessageDecodeAsync\`\`1(context,message,ID,messageHeader,receivedTimestamp,processedTimeStamp)](#M-MQContract-Interfaces-Middleware-IAfterDecodeMiddleware-AfterMessageDecodeAsync``1-MQContract-Interfaces-Middleware-IContext,``0,System-String,MQContract-Messages-MessageHeader,System-DateTime,System-DateTime- 'MQContract.Interfaces.Middleware.IAfterDecodeMiddleware.AfterMessageDecodeAsync``1(MQContract.Interfaces.Middleware.IContext,``0,System.String,MQContract.Messages.MessageHeader,System.DateTime,System.DateTime)')
- [IAfterDecodeSpecificTypeMiddleware\`1](#T-MQContract-Interfaces-Middleware-IAfterDecodeSpecificTypeMiddleware`1 'MQContract.Interfaces.Middleware.IAfterDecodeSpecificTypeMiddleware`1')
  - [AfterMessageDecodeAsync(context,message,ID,messageHeader,receivedTimestamp,processedTimeStamp)](#M-MQContract-Interfaces-Middleware-IAfterDecodeSpecificTypeMiddleware`1-AfterMessageDecodeAsync-MQContract-Interfaces-Middleware-IContext,`0,System-String,MQContract-Messages-MessageHeader,System-DateTime,System-DateTime- 'MQContract.Interfaces.Middleware.IAfterDecodeSpecificTypeMiddleware`1.AfterMessageDecodeAsync(MQContract.Interfaces.Middleware.IContext,`0,System.String,MQContract.Messages.MessageHeader,System.DateTime,System.DateTime)')
- [IAfterEncodeMiddleware](#T-MQContract-Interfaces-Middleware-IAfterEncodeMiddleware 'MQContract.Interfaces.Middleware.IAfterEncodeMiddleware')
  - [AfterMessageEncodeAsync(messageType,context,message)](#M-MQContract-Interfaces-Middleware-IAfterEncodeMiddleware-AfterMessageEncodeAsync-System-Type,MQContract-Interfaces-Middleware-IContext,MQContract-Messages-ServiceMessage- 'MQContract.Interfaces.Middleware.IAfterEncodeMiddleware.AfterMessageEncodeAsync(System.Type,MQContract.Interfaces.Middleware.IContext,MQContract.Messages.ServiceMessage)')
- [IBeforeDecodeMiddleware](#T-MQContract-Interfaces-Middleware-IBeforeDecodeMiddleware 'MQContract.Interfaces.Middleware.IBeforeDecodeMiddleware')
  - [BeforeMessageDecodeAsync(context,id,messageHeader,messageTypeID,messageChannel,data)](#M-MQContract-Interfaces-Middleware-IBeforeDecodeMiddleware-BeforeMessageDecodeAsync-MQContract-Interfaces-Middleware-IContext,System-String,MQContract-Messages-MessageHeader,System-String,System-String,System-ReadOnlyMemory{System-Byte}- 'MQContract.Interfaces.Middleware.IBeforeDecodeMiddleware.BeforeMessageDecodeAsync(MQContract.Interfaces.Middleware.IContext,System.String,MQContract.Messages.MessageHeader,System.String,System.String,System.ReadOnlyMemory{System.Byte})')
- [IBeforeEncodeMiddleware](#T-MQContract-Interfaces-Middleware-IBeforeEncodeMiddleware 'MQContract.Interfaces.Middleware.IBeforeEncodeMiddleware')
  - [BeforeMessageEncodeAsync\`\`1(context,message,channel,messageHeader)](#M-MQContract-Interfaces-Middleware-IBeforeEncodeMiddleware-BeforeMessageEncodeAsync``1-MQContract-Interfaces-Middleware-IContext,``0,System-String,MQContract-Messages-MessageHeader- 'MQContract.Interfaces.Middleware.IBeforeEncodeMiddleware.BeforeMessageEncodeAsync``1(MQContract.Interfaces.Middleware.IContext,``0,System.String,MQContract.Messages.MessageHeader)')
- [IBeforeEncodeSpecificTypeMiddleware\`1](#T-MQContract-Interfaces-Middleware-IBeforeEncodeSpecificTypeMiddleware`1 'MQContract.Interfaces.Middleware.IBeforeEncodeSpecificTypeMiddleware`1')
  - [BeforeMessageEncodeAsync(context,message,channel,messageHeader)](#M-MQContract-Interfaces-Middleware-IBeforeEncodeSpecificTypeMiddleware`1-BeforeMessageEncodeAsync-MQContract-Interfaces-Middleware-IContext,`0,System-String,MQContract-Messages-MessageHeader- 'MQContract.Interfaces.Middleware.IBeforeEncodeSpecificTypeMiddleware`1.BeforeMessageEncodeAsync(MQContract.Interfaces.Middleware.IContext,`0,System.String,MQContract.Messages.MessageHeader)')
- [IContext](#T-MQContract-Interfaces-Middleware-IContext 'MQContract.Interfaces.Middleware.IContext')
  - [Item](#P-MQContract-Interfaces-Middleware-IContext-Item-System-String- 'MQContract.Interfaces.Middleware.IContext.Item(System.String)')
- [IContractConnection](#T-MQContract-Interfaces-IContractConnection 'MQContract.Interfaces.IContractConnection')
  - [CloseAsync()](#M-MQContract-Interfaces-IContractConnection-CloseAsync 'MQContract.Interfaces.IContractConnection.CloseAsync')
  - [GetSnapshot(sent)](#M-MQContract-Interfaces-IContractConnection-GetSnapshot-System-Boolean- 'MQContract.Interfaces.IContractConnection.GetSnapshot(System.Boolean)')
  - [GetSnapshot(messageType,sent)](#M-MQContract-Interfaces-IContractConnection-GetSnapshot-System-Type,System-Boolean- 'MQContract.Interfaces.IContractConnection.GetSnapshot(System.Type,System.Boolean)')
  - [GetSnapshot(channel,sent)](#M-MQContract-Interfaces-IContractConnection-GetSnapshot-System-String,System-Boolean- 'MQContract.Interfaces.IContractConnection.GetSnapshot(System.String,System.Boolean)')
  - [GetSnapshot\`\`1(sent)](#M-MQContract-Interfaces-IContractConnection-GetSnapshot``1-System-Boolean- 'MQContract.Interfaces.IContractConnection.GetSnapshot``1(System.Boolean)')
  - [PingAsync()](#M-MQContract-Interfaces-IContractConnection-PingAsync 'MQContract.Interfaces.IContractConnection.PingAsync')
  - [PublishAsync\`\`1(message,channel,messageHeader,cancellationToken)](#M-MQContract-Interfaces-IContractConnection-PublishAsync``1-``0,System-String,MQContract-Messages-MessageHeader,System-Threading-CancellationToken- 'MQContract.Interfaces.IContractConnection.PublishAsync``1(``0,System.String,MQContract.Messages.MessageHeader,System.Threading.CancellationToken)')
  - [QueryAsync\`\`1(message,timeout,channel,responseChannel,messageHeader,cancellationToken)](#M-MQContract-Interfaces-IContractConnection-QueryAsync``1-``0,System-Nullable{System-TimeSpan},System-String,System-String,MQContract-Messages-MessageHeader,System-Threading-CancellationToken- 'MQContract.Interfaces.IContractConnection.QueryAsync``1(``0,System.Nullable{System.TimeSpan},System.String,System.String,MQContract.Messages.MessageHeader,System.Threading.CancellationToken)')
  - [QueryAsync\`\`2(message,timeout,channel,responseChannel,messageHeader,cancellationToken)](#M-MQContract-Interfaces-IContractConnection-QueryAsync``2-``0,System-Nullable{System-TimeSpan},System-String,System-String,MQContract-Messages-MessageHeader,System-Threading-CancellationToken- 'MQContract.Interfaces.IContractConnection.QueryAsync``2(``0,System.Nullable{System.TimeSpan},System.String,System.String,MQContract.Messages.MessageHeader,System.Threading.CancellationToken)')
  - [RegisterMiddleware\`\`1()](#M-MQContract-Interfaces-IContractConnection-RegisterMiddleware``1 'MQContract.Interfaces.IContractConnection.RegisterMiddleware``1')
  - [RegisterMiddleware\`\`1(constructInstance)](#M-MQContract-Interfaces-IContractConnection-RegisterMiddleware``1-System-Func{``0}- 'MQContract.Interfaces.IContractConnection.RegisterMiddleware``1(System.Func{``0})')
  - [RegisterMiddleware\`\`2()](#M-MQContract-Interfaces-IContractConnection-RegisterMiddleware``2 'MQContract.Interfaces.IContractConnection.RegisterMiddleware``2')
  - [RegisterMiddleware\`\`2(constructInstance)](#M-MQContract-Interfaces-IContractConnection-RegisterMiddleware``2-System-Func{``0}- 'MQContract.Interfaces.IContractConnection.RegisterMiddleware``2(System.Func{``0})')
  - [SubscribeAsync\`\`1(messageReceived,errorReceived,channel,group,ignoreMessageHeader,cancellationToken)](#M-MQContract-Interfaces-IContractConnection-SubscribeAsync``1-System-Func{MQContract-Interfaces-IReceivedMessage{``0},System-Threading-Tasks-ValueTask},System-Action{System-Exception},System-String,System-String,System-Boolean,System-Threading-CancellationToken- 'MQContract.Interfaces.IContractConnection.SubscribeAsync``1(System.Func{MQContract.Interfaces.IReceivedMessage{``0},System.Threading.Tasks.ValueTask},System.Action{System.Exception},System.String,System.String,System.Boolean,System.Threading.CancellationToken)')
  - [SubscribeAsync\`\`1(messageReceived,errorReceived,channel,group,ignoreMessageHeader,cancellationToken)](#M-MQContract-Interfaces-IContractConnection-SubscribeAsync``1-System-Action{MQContract-Interfaces-IReceivedMessage{``0}},System-Action{System-Exception},System-String,System-String,System-Boolean,System-Threading-CancellationToken- 'MQContract.Interfaces.IContractConnection.SubscribeAsync``1(System.Action{MQContract.Interfaces.IReceivedMessage{``0}},System.Action{System.Exception},System.String,System.String,System.Boolean,System.Threading.CancellationToken)')
  - [SubscribeQueryAsyncResponseAsync\`\`2(messageReceived,errorReceived,channel,group,ignoreMessageHeader,cancellationToken)](#M-MQContract-Interfaces-IContractConnection-SubscribeQueryAsyncResponseAsync``2-System-Func{MQContract-Interfaces-IReceivedMessage{``0},System-Threading-Tasks-ValueTask{MQContract-Messages-QueryResponseMessage{``1}}},System-Action{System-Exception},System-String,System-String,System-Boolean,System-Threading-CancellationToken- 'MQContract.Interfaces.IContractConnection.SubscribeQueryAsyncResponseAsync``2(System.Func{MQContract.Interfaces.IReceivedMessage{``0},System.Threading.Tasks.ValueTask{MQContract.Messages.QueryResponseMessage{``1}}},System.Action{System.Exception},System.String,System.String,System.Boolean,System.Threading.CancellationToken)')
  - [SubscribeQueryResponseAsync\`\`2(messageReceived,errorReceived,channel,group,ignoreMessageHeader,cancellationToken)](#M-MQContract-Interfaces-IContractConnection-SubscribeQueryResponseAsync``2-System-Func{MQContract-Interfaces-IReceivedMessage{``0},MQContract-Messages-QueryResponseMessage{``1}},System-Action{System-Exception},System-String,System-String,System-Boolean,System-Threading-CancellationToken- 'MQContract.Interfaces.IContractConnection.SubscribeQueryResponseAsync``2(System.Func{MQContract.Interfaces.IReceivedMessage{``0},MQContract.Messages.QueryResponseMessage{``1}},System.Action{System.Exception},System.String,System.String,System.Boolean,System.Threading.CancellationToken)')
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
- [IEncodedMessage](#T-MQContract-Interfaces-Messages-IEncodedMessage 'MQContract.Interfaces.Messages.IEncodedMessage')
  - [Data](#P-MQContract-Interfaces-Messages-IEncodedMessage-Data 'MQContract.Interfaces.Messages.IEncodedMessage.Data')
  - [Header](#P-MQContract-Interfaces-Messages-IEncodedMessage-Header 'MQContract.Interfaces.Messages.IEncodedMessage.Header')
  - [MessageTypeID](#P-MQContract-Interfaces-Messages-IEncodedMessage-MessageTypeID 'MQContract.Interfaces.Messages.IEncodedMessage.MessageTypeID')
- [IMessageConverter\`2](#T-MQContract-Interfaces-Conversion-IMessageConverter`2 'MQContract.Interfaces.Conversion.IMessageConverter`2')
  - [ConvertAsync(source)](#M-MQContract-Interfaces-Conversion-IMessageConverter`2-ConvertAsync-`0- 'MQContract.Interfaces.Conversion.IMessageConverter`2.ConvertAsync(`0)')
- [IMessageEncoder](#T-MQContract-Interfaces-Encoding-IMessageEncoder 'MQContract.Interfaces.Encoding.IMessageEncoder')
  - [DecodeAsync\`\`1(stream)](#M-MQContract-Interfaces-Encoding-IMessageEncoder-DecodeAsync``1-System-IO-Stream- 'MQContract.Interfaces.Encoding.IMessageEncoder.DecodeAsync``1(System.IO.Stream)')
  - [EncodeAsync\`\`1(message)](#M-MQContract-Interfaces-Encoding-IMessageEncoder-EncodeAsync``1-``0- 'MQContract.Interfaces.Encoding.IMessageEncoder.EncodeAsync``1(``0)')
- [IMessageEncryptor](#T-MQContract-Interfaces-Encrypting-IMessageEncryptor 'MQContract.Interfaces.Encrypting.IMessageEncryptor')
  - [DecryptAsync(stream,headers)](#M-MQContract-Interfaces-Encrypting-IMessageEncryptor-DecryptAsync-System-IO-Stream,MQContract-Messages-MessageHeader- 'MQContract.Interfaces.Encrypting.IMessageEncryptor.DecryptAsync(System.IO.Stream,MQContract.Messages.MessageHeader)')
  - [EncryptAsync(data,headers)](#M-MQContract-Interfaces-Encrypting-IMessageEncryptor-EncryptAsync-System-Byte[],System-Collections-Generic-Dictionary{System-String,System-String}@- 'MQContract.Interfaces.Encrypting.IMessageEncryptor.EncryptAsync(System.Byte[],System.Collections.Generic.Dictionary{System.String,System.String}@)')
- [IMessageServiceConnection](#T-MQContract-Interfaces-Service-IMessageServiceConnection 'MQContract.Interfaces.Service.IMessageServiceConnection')
  - [MaxMessageBodySize](#P-MQContract-Interfaces-Service-IMessageServiceConnection-MaxMessageBodySize 'MQContract.Interfaces.Service.IMessageServiceConnection.MaxMessageBodySize')
  - [CloseAsync()](#M-MQContract-Interfaces-Service-IMessageServiceConnection-CloseAsync 'MQContract.Interfaces.Service.IMessageServiceConnection.CloseAsync')
  - [PublishAsync(message,cancellationToken)](#M-MQContract-Interfaces-Service-IMessageServiceConnection-PublishAsync-MQContract-Messages-ServiceMessage,System-Threading-CancellationToken- 'MQContract.Interfaces.Service.IMessageServiceConnection.PublishAsync(MQContract.Messages.ServiceMessage,System.Threading.CancellationToken)')
  - [SubscribeAsync(messageReceived,errorReceived,channel,group,cancellationToken)](#M-MQContract-Interfaces-Service-IMessageServiceConnection-SubscribeAsync-System-Action{MQContract-Messages-ReceivedServiceMessage},System-Action{System-Exception},System-String,System-String,System-Threading-CancellationToken- 'MQContract.Interfaces.Service.IMessageServiceConnection.SubscribeAsync(System.Action{MQContract.Messages.ReceivedServiceMessage},System.Action{System.Exception},System.String,System.String,System.Threading.CancellationToken)')
- [IMessageTypeEncoder\`1](#T-MQContract-Interfaces-Encoding-IMessageTypeEncoder`1 'MQContract.Interfaces.Encoding.IMessageTypeEncoder`1')
  - [DecodeAsync(stream)](#M-MQContract-Interfaces-Encoding-IMessageTypeEncoder`1-DecodeAsync-System-IO-Stream- 'MQContract.Interfaces.Encoding.IMessageTypeEncoder`1.DecodeAsync(System.IO.Stream)')
  - [EncodeAsync(message)](#M-MQContract-Interfaces-Encoding-IMessageTypeEncoder`1-EncodeAsync-`0- 'MQContract.Interfaces.Encoding.IMessageTypeEncoder`1.EncodeAsync(`0)')
- [IMessageTypeEncryptor\`1](#T-MQContract-Interfaces-Encrypting-IMessageTypeEncryptor`1 'MQContract.Interfaces.Encrypting.IMessageTypeEncryptor`1')
- [IPingableMessageServiceConnection](#T-MQContract-Interfaces-Service-IPingableMessageServiceConnection 'MQContract.Interfaces.Service.IPingableMessageServiceConnection')
  - [PingAsync()](#M-MQContract-Interfaces-Service-IPingableMessageServiceConnection-PingAsync 'MQContract.Interfaces.Service.IPingableMessageServiceConnection.PingAsync')
- [IQueryableMessageServiceConnection](#T-MQContract-Interfaces-Service-IQueryableMessageServiceConnection 'MQContract.Interfaces.Service.IQueryableMessageServiceConnection')
  - [DefaultTimout](#P-MQContract-Interfaces-Service-IQueryableMessageServiceConnection-DefaultTimout 'MQContract.Interfaces.Service.IQueryableMessageServiceConnection.DefaultTimout')
  - [QueryAsync(message,timeout,cancellationToken)](#M-MQContract-Interfaces-Service-IQueryableMessageServiceConnection-QueryAsync-MQContract-Messages-ServiceMessage,System-TimeSpan,System-Threading-CancellationToken- 'MQContract.Interfaces.Service.IQueryableMessageServiceConnection.QueryAsync(MQContract.Messages.ServiceMessage,System.TimeSpan,System.Threading.CancellationToken)')
  - [SubscribeQueryAsync(messageReceived,errorReceived,channel,group,cancellationToken)](#M-MQContract-Interfaces-Service-IQueryableMessageServiceConnection-SubscribeQueryAsync-System-Func{MQContract-Messages-ReceivedServiceMessage,System-Threading-Tasks-ValueTask{MQContract-Messages-ServiceMessage}},System-Action{System-Exception},System-String,System-String,System-Threading-CancellationToken- 'MQContract.Interfaces.Service.IQueryableMessageServiceConnection.SubscribeQueryAsync(System.Func{MQContract.Messages.ReceivedServiceMessage,System.Threading.Tasks.ValueTask{MQContract.Messages.ServiceMessage}},System.Action{System.Exception},System.String,System.String,System.Threading.CancellationToken)')
- [IReceivedMessage\`1](#T-MQContract-Interfaces-IReceivedMessage`1 'MQContract.Interfaces.IReceivedMessage`1')
  - [Headers](#P-MQContract-Interfaces-IReceivedMessage`1-Headers 'MQContract.Interfaces.IReceivedMessage`1.Headers')
  - [ID](#P-MQContract-Interfaces-IReceivedMessage`1-ID 'MQContract.Interfaces.IReceivedMessage`1.ID')
  - [Message](#P-MQContract-Interfaces-IReceivedMessage`1-Message 'MQContract.Interfaces.IReceivedMessage`1.Message')
  - [ProcessedTimestamp](#P-MQContract-Interfaces-IReceivedMessage`1-ProcessedTimestamp 'MQContract.Interfaces.IReceivedMessage`1.ProcessedTimestamp')
  - [ReceivedTimestamp](#P-MQContract-Interfaces-IReceivedMessage`1-ReceivedTimestamp 'MQContract.Interfaces.IReceivedMessage`1.ReceivedTimestamp')
- [IServiceSubscription](#T-MQContract-Interfaces-Service-IServiceSubscription 'MQContract.Interfaces.Service.IServiceSubscription')
  - [EndAsync()](#M-MQContract-Interfaces-Service-IServiceSubscription-EndAsync 'MQContract.Interfaces.Service.IServiceSubscription.EndAsync')
- [ISubscription](#T-MQContract-Interfaces-ISubscription 'MQContract.Interfaces.ISubscription')
  - [EndAsync()](#M-MQContract-Interfaces-ISubscription-EndAsync 'MQContract.Interfaces.ISubscription.EndAsync')
- [MessageChannelAttribute](#T-MQContract-Attributes-MessageChannelAttribute 'MQContract.Attributes.MessageChannelAttribute')
  - [#ctor(name)](#M-MQContract-Attributes-MessageChannelAttribute-#ctor-System-String- 'MQContract.Attributes.MessageChannelAttribute.#ctor(System.String)')
  - [Name](#P-MQContract-Attributes-MessageChannelAttribute-Name 'MQContract.Attributes.MessageChannelAttribute.Name')
- [MessageHeader](#T-MQContract-Messages-MessageHeader 'MQContract.Messages.MessageHeader')
  - [#ctor(data)](#M-MQContract-Messages-MessageHeader-#ctor-System-Collections-Generic-IEnumerable{System-Collections-Generic-KeyValuePair{System-String,System-String}}- 'MQContract.Messages.MessageHeader.#ctor(System.Collections.Generic.IEnumerable{System.Collections.Generic.KeyValuePair{System.String,System.String}})')
  - [#ctor(headers)](#M-MQContract-Messages-MessageHeader-#ctor-System-Collections-Generic-Dictionary{System-String,System-String}- 'MQContract.Messages.MessageHeader.#ctor(System.Collections.Generic.Dictionary{System.String,System.String})')
  - [#ctor(originalHeader,appendedHeader)](#M-MQContract-Messages-MessageHeader-#ctor-MQContract-Messages-MessageHeader,System-Collections-Generic-Dictionary{System-String,System-String}- 'MQContract.Messages.MessageHeader.#ctor(MQContract.Messages.MessageHeader,System.Collections.Generic.Dictionary{System.String,System.String})')
  - [Item](#P-MQContract-Messages-MessageHeader-Item-System-String- 'MQContract.Messages.MessageHeader.Item(System.String)')
  - [Keys](#P-MQContract-Messages-MessageHeader-Keys 'MQContract.Messages.MessageHeader.Keys')
- [MessageNameAttribute](#T-MQContract-Attributes-MessageNameAttribute 'MQContract.Attributes.MessageNameAttribute')
  - [#ctor(value)](#M-MQContract-Attributes-MessageNameAttribute-#ctor-System-String- 'MQContract.Attributes.MessageNameAttribute.#ctor(System.String)')
  - [Value](#P-MQContract-Attributes-MessageNameAttribute-Value 'MQContract.Attributes.MessageNameAttribute.Value')
- [MessageResponseTimeoutAttribute](#T-MQContract-Attributes-MessageResponseTimeoutAttribute 'MQContract.Attributes.MessageResponseTimeoutAttribute')
  - [#ctor(value)](#M-MQContract-Attributes-MessageResponseTimeoutAttribute-#ctor-System-Int32- 'MQContract.Attributes.MessageResponseTimeoutAttribute.#ctor(System.Int32)')
  - [TimeSpanValue](#P-MQContract-Attributes-MessageResponseTimeoutAttribute-TimeSpanValue 'MQContract.Attributes.MessageResponseTimeoutAttribute.TimeSpanValue')
  - [Value](#P-MQContract-Attributes-MessageResponseTimeoutAttribute-Value 'MQContract.Attributes.MessageResponseTimeoutAttribute.Value')
- [MessageVersionAttribute](#T-MQContract-Attributes-MessageVersionAttribute 'MQContract.Attributes.MessageVersionAttribute')
  - [#ctor(version)](#M-MQContract-Attributes-MessageVersionAttribute-#ctor-System-String- 'MQContract.Attributes.MessageVersionAttribute.#ctor(System.String)')
  - [Version](#P-MQContract-Attributes-MessageVersionAttribute-Version 'MQContract.Attributes.MessageVersionAttribute.Version')
- [PingResult](#T-MQContract-Messages-PingResult 'MQContract.Messages.PingResult')
  - [#ctor(Host,Version,ResponseTime)](#M-MQContract-Messages-PingResult-#ctor-System-String,System-String,System-TimeSpan- 'MQContract.Messages.PingResult.#ctor(System.String,System.String,System.TimeSpan)')
  - [Host](#P-MQContract-Messages-PingResult-Host 'MQContract.Messages.PingResult.Host')
  - [ResponseTime](#P-MQContract-Messages-PingResult-ResponseTime 'MQContract.Messages.PingResult.ResponseTime')
  - [Version](#P-MQContract-Messages-PingResult-Version 'MQContract.Messages.PingResult.Version')
- [QueryResponseChannelAttribute](#T-MQContract-Attributes-QueryResponseChannelAttribute 'MQContract.Attributes.QueryResponseChannelAttribute')
  - [#ctor(name)](#M-MQContract-Attributes-QueryResponseChannelAttribute-#ctor-System-String- 'MQContract.Attributes.QueryResponseChannelAttribute.#ctor(System.String)')
  - [Name](#P-MQContract-Attributes-QueryResponseChannelAttribute-Name 'MQContract.Attributes.QueryResponseChannelAttribute.Name')
- [QueryResponseMessage\`1](#T-MQContract-Messages-QueryResponseMessage`1 'MQContract.Messages.QueryResponseMessage`1')
  - [#ctor(Message,Headers)](#M-MQContract-Messages-QueryResponseMessage`1-#ctor-`0,System-Collections-Generic-Dictionary{System-String,System-String}- 'MQContract.Messages.QueryResponseMessage`1.#ctor(`0,System.Collections.Generic.Dictionary{System.String,System.String})')
  - [Headers](#P-MQContract-Messages-QueryResponseMessage`1-Headers 'MQContract.Messages.QueryResponseMessage`1.Headers')
  - [Message](#P-MQContract-Messages-QueryResponseMessage`1-Message 'MQContract.Messages.QueryResponseMessage`1.Message')
- [QueryResponseTypeAttribute](#T-MQContract-Attributes-QueryResponseTypeAttribute 'MQContract.Attributes.QueryResponseTypeAttribute')
  - [#ctor(responseType)](#M-MQContract-Attributes-QueryResponseTypeAttribute-#ctor-System-Type- 'MQContract.Attributes.QueryResponseTypeAttribute.#ctor(System.Type)')
  - [ResponseType](#P-MQContract-Attributes-QueryResponseTypeAttribute-ResponseType 'MQContract.Attributes.QueryResponseTypeAttribute.ResponseType')
- [QueryResult\`1](#T-MQContract-Messages-QueryResult`1 'MQContract.Messages.QueryResult`1')
  - [#ctor(ID,Header,Result,Error)](#M-MQContract-Messages-QueryResult`1-#ctor-System-String,MQContract-Messages-MessageHeader,`0,System-String- 'MQContract.Messages.QueryResult`1.#ctor(System.String,MQContract.Messages.MessageHeader,`0,System.String)')
  - [Header](#P-MQContract-Messages-QueryResult`1-Header 'MQContract.Messages.QueryResult`1.Header')
  - [Result](#P-MQContract-Messages-QueryResult`1-Result 'MQContract.Messages.QueryResult`1.Result')
- [ReceivedServiceMessage](#T-MQContract-Messages-ReceivedServiceMessage 'MQContract.Messages.ReceivedServiceMessage')
  - [#ctor(ID,MessageTypeID,Channel,Header,Data,Acknowledge)](#M-MQContract-Messages-ReceivedServiceMessage-#ctor-System-String,System-String,System-String,MQContract-Messages-MessageHeader,System-ReadOnlyMemory{System-Byte},System-Func{System-Threading-Tasks-ValueTask}- 'MQContract.Messages.ReceivedServiceMessage.#ctor(System.String,System.String,System.String,MQContract.Messages.MessageHeader,System.ReadOnlyMemory{System.Byte},System.Func{System.Threading.Tasks.ValueTask})')
  - [Acknowledge](#P-MQContract-Messages-ReceivedServiceMessage-Acknowledge 'MQContract.Messages.ReceivedServiceMessage.Acknowledge')
  - [ReceivedTimestamp](#P-MQContract-Messages-ReceivedServiceMessage-ReceivedTimestamp 'MQContract.Messages.ReceivedServiceMessage.ReceivedTimestamp')
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
- [TransmissionResult](#T-MQContract-Messages-TransmissionResult 'MQContract.Messages.TransmissionResult')
  - [#ctor(ID,Error)](#M-MQContract-Messages-TransmissionResult-#ctor-System-String,System-String- 'MQContract.Messages.TransmissionResult.#ctor(System.String,System.String)')
  - [Error](#P-MQContract-Messages-TransmissionResult-Error 'MQContract.Messages.TransmissionResult.Error')
  - [ID](#P-MQContract-Messages-TransmissionResult-ID 'MQContract.Messages.TransmissionResult.ID')
  - [IsError](#P-MQContract-Messages-TransmissionResult-IsError 'MQContract.Messages.TransmissionResult.IsError')

<a name='T-MQContract-Interfaces-Middleware-IAfterDecodeMiddleware'></a>
## IAfterDecodeMiddleware `type`

##### Namespace

MQContract.Interfaces.Middleware

##### Summary

This interface represents a Middleware to execute after a Message has been decoded from a ServiceMessage to the expected Class

<a name='M-MQContract-Interfaces-Middleware-IAfterDecodeMiddleware-AfterMessageDecodeAsync``1-MQContract-Interfaces-Middleware-IContext,``0,System-String,MQContract-Messages-MessageHeader,System-DateTime,System-DateTime-'></a>
### AfterMessageDecodeAsync\`\`1(context,message,ID,messageHeader,receivedTimestamp,processedTimeStamp) `method`

##### Summary

This is the method invoked as part of the Middleware processing during message decoding

##### Returns

The message and header to allow for changes if desired

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| context | [MQContract.Interfaces.Middleware.IContext](#T-MQContract-Interfaces-Middleware-IContext 'MQContract.Interfaces.Middleware.IContext') | A shared context that exists from the start of this decode process instance |
| message | [\`\`0](#T-``0 '``0') | The class message |
| ID | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | The id of the message |
| messageHeader | [MQContract.Messages.MessageHeader](#T-MQContract-Messages-MessageHeader 'MQContract.Messages.MessageHeader') | The headers from the message |
| receivedTimestamp | [System.DateTime](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.DateTime 'System.DateTime') | The timestamp of when the message was recieved |
| processedTimeStamp | [System.DateTime](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.DateTime 'System.DateTime') | The timestamp of when the message was decoded into a Class |

##### Generic Types

| Name | Description |
| ---- | ----------- |
| T | This will be the type of the Message that was decoded |

<a name='T-MQContract-Interfaces-Middleware-IAfterDecodeSpecificTypeMiddleware`1'></a>
## IAfterDecodeSpecificTypeMiddleware\`1 `type`

##### Namespace

MQContract.Interfaces.Middleware

##### Summary

This interface represents a Middleware to execute after a Message of the given type T has been decoded from a ServiceMessage to the expected Class

<a name='M-MQContract-Interfaces-Middleware-IAfterDecodeSpecificTypeMiddleware`1-AfterMessageDecodeAsync-MQContract-Interfaces-Middleware-IContext,`0,System-String,MQContract-Messages-MessageHeader,System-DateTime,System-DateTime-'></a>
### AfterMessageDecodeAsync(context,message,ID,messageHeader,receivedTimestamp,processedTimeStamp) `method`

##### Summary

This is the method invoked as part of the Middleware processing during message decoding

##### Returns

The message and header to allow for changes if desired

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| context | [MQContract.Interfaces.Middleware.IContext](#T-MQContract-Interfaces-Middleware-IContext 'MQContract.Interfaces.Middleware.IContext') | A shared context that exists from the start of this decode process instance |
| message | [\`0](#T-`0 '`0') | The class message |
| ID | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | The id of the message |
| messageHeader | [MQContract.Messages.MessageHeader](#T-MQContract-Messages-MessageHeader 'MQContract.Messages.MessageHeader') | The headers from the message |
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

<a name='T-MQContract-Interfaces-Middleware-IBeforeDecodeMiddleware'></a>
## IBeforeDecodeMiddleware `type`

##### Namespace

MQContract.Interfaces.Middleware

##### Summary

This interface represents a Middleware to execute before decoding a ServiceMessage

<a name='M-MQContract-Interfaces-Middleware-IBeforeDecodeMiddleware-BeforeMessageDecodeAsync-MQContract-Interfaces-Middleware-IContext,System-String,MQContract-Messages-MessageHeader,System-String,System-String,System-ReadOnlyMemory{System-Byte}-'></a>
### BeforeMessageDecodeAsync(context,id,messageHeader,messageTypeID,messageChannel,data) `method`

##### Summary

This is the method invoked as part of the Middleware processing prior to the message decoding

##### Returns

The message header and data to allow for changes if desired

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| context | [MQContract.Interfaces.Middleware.IContext](#T-MQContract-Interfaces-Middleware-IContext 'MQContract.Interfaces.Middleware.IContext') | A shared context that exists from the start of this decode process instance |
| id | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | The id of the message |
| messageHeader | [MQContract.Messages.MessageHeader](#T-MQContract-Messages-MessageHeader 'MQContract.Messages.MessageHeader') | The headers from the message |
| messageTypeID | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | The message type id |
| messageChannel | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | The channel the message was recieved on |
| data | [System.ReadOnlyMemory{System.Byte}](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.ReadOnlyMemory 'System.ReadOnlyMemory{System.Byte}') | The data of the message |

<a name='T-MQContract-Interfaces-Middleware-IBeforeEncodeMiddleware'></a>
## IBeforeEncodeMiddleware `type`

##### Namespace

MQContract.Interfaces.Middleware

##### Summary

This interface represents a Middleware to execute Before a message is encoded

<a name='M-MQContract-Interfaces-Middleware-IBeforeEncodeMiddleware-BeforeMessageEncodeAsync``1-MQContract-Interfaces-Middleware-IContext,``0,System-String,MQContract-Messages-MessageHeader-'></a>
### BeforeMessageEncodeAsync\`\`1(context,message,channel,messageHeader) `method`

##### Summary

This is the method invoked as part of the Middle Ware processing during message encoding

##### Returns

The message, channel and header to allow for changes if desired

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| context | [MQContract.Interfaces.Middleware.IContext](#T-MQContract-Interfaces-Middleware-IContext 'MQContract.Interfaces.Middleware.IContext') | A shared context that exists from the start of this encoding instance |
| message | [\`\`0](#T-``0 '``0') | The message being encoded |
| channel | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | The channel this message was requested to transmit to |
| messageHeader | [MQContract.Messages.MessageHeader](#T-MQContract-Messages-MessageHeader 'MQContract.Messages.MessageHeader') | The message headers being supplied |

##### Generic Types

| Name | Description |
| ---- | ----------- |
| T | The type of message being processed |

<a name='T-MQContract-Interfaces-Middleware-IBeforeEncodeSpecificTypeMiddleware`1'></a>
## IBeforeEncodeSpecificTypeMiddleware\`1 `type`

##### Namespace

MQContract.Interfaces.Middleware

##### Summary

This interface represents a Middleware to execute Before a specific message type is encoded

<a name='M-MQContract-Interfaces-Middleware-IBeforeEncodeSpecificTypeMiddleware`1-BeforeMessageEncodeAsync-MQContract-Interfaces-Middleware-IContext,`0,System-String,MQContract-Messages-MessageHeader-'></a>
### BeforeMessageEncodeAsync(context,message,channel,messageHeader) `method`

##### Summary

This is the method invoked as part of the Middle Ware processing during message encoding

##### Returns

The message, channel and header to allow for changes if desired

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| context | [MQContract.Interfaces.Middleware.IContext](#T-MQContract-Interfaces-Middleware-IContext 'MQContract.Interfaces.Middleware.IContext') | A shared context that exists from the start of this encoding instance |
| message | [\`0](#T-`0 '`0') | The message being encoded |
| channel | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | The channel this message was requested to transmit to |
| messageHeader | [MQContract.Messages.MessageHeader](#T-MQContract-Messages-MessageHeader 'MQContract.Messages.MessageHeader') | The message headers being supplied |

<a name='T-MQContract-Interfaces-Middleware-IContext'></a>
## IContext `type`

##### Namespace

MQContract.Interfaces.Middleware

##### Summary

This is used to represent a Context for the middleware calls to use that exists from the start to the end of the message conversion process

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

<a name='M-MQContract-Interfaces-IContractConnection-CloseAsync'></a>
### CloseAsync() `method`

##### Summary

Called to close off the contract connection and close it's underlying service connection

##### Returns

A task for the closure of the connection

##### Parameters

This method has no parameters.

<a name='M-MQContract-Interfaces-IContractConnection-GetSnapshot-System-Boolean-'></a>
### GetSnapshot(sent) `method`

##### Summary

Called to get a snapshot of the current global metrics.  Will return null if internal metrics are not enabled.

##### Returns

A record of the current metric snapshot or null if not available

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| sent | [System.Boolean](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Boolean 'System.Boolean') | true when the sent metrics are desired, false when received are desired |

<a name='M-MQContract-Interfaces-IContractConnection-GetSnapshot-System-Type,System-Boolean-'></a>
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

<a name='M-MQContract-Interfaces-IContractConnection-GetSnapshot-System-String,System-Boolean-'></a>
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

<a name='M-MQContract-Interfaces-IContractConnection-GetSnapshot``1-System-Boolean-'></a>
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
| T | The type of message to look for |

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
| T | The type of message to send |

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
| Q | The type of message to send for the query |

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
| Q | The type of message to send for the query |
| R | The type of message to expect back for the response |

<a name='M-MQContract-Interfaces-IContractConnection-RegisterMiddleware``1'></a>
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
| T | The type of middle ware to register, it must implement IBeforeDecodeMiddleware or IBeforeEncodeMiddleware or IAfterDecodeMiddleware or IAfterEncodeMiddleware |

<a name='M-MQContract-Interfaces-IContractConnection-RegisterMiddleware``1-System-Func{``0}-'></a>
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
| T | The type of middle ware to register, it must implement IBeforeDecodeMiddleware or IBeforeEncodeMiddleware or IAfterDecodeMiddleware or IAfterEncodeMiddleware |

<a name='M-MQContract-Interfaces-IContractConnection-RegisterMiddleware``2'></a>
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
| T | The type of middle ware to register, it must implement IBeforeEncodeSpecificTypeMiddleware<M> or IAfterDecodeSpecificTypeMiddleware<M> |
| M | The message type that this middleware is specifically called for |

<a name='M-MQContract-Interfaces-IContractConnection-RegisterMiddleware``2-System-Func{``0}-'></a>
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
| T | The type of middle ware to register, it must implement IBeforeEncodeSpecificTypeMiddleware<M> or IAfterDecodeSpecificTypeMiddleware<M> |
| M | The message type that this middleware is specifically called for |

<a name='M-MQContract-Interfaces-IContractConnection-SubscribeAsync``1-System-Func{MQContract-Interfaces-IReceivedMessage{``0},System-Threading-Tasks-ValueTask},System-Action{System-Exception},System-String,System-String,System-Boolean,System-Threading-CancellationToken-'></a>
### SubscribeAsync\`\`1(messageReceived,errorReceived,channel,group,ignoreMessageHeader,cancellationToken) `method`

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
| cancellationToken | [System.Threading.CancellationToken](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Threading.CancellationToken 'System.Threading.CancellationToken') | A cancellation token |

##### Generic Types

| Name | Description |
| ---- | ----------- |
| T | The type of message to listen for |

<a name='M-MQContract-Interfaces-IContractConnection-SubscribeAsync``1-System-Action{MQContract-Interfaces-IReceivedMessage{``0}},System-Action{System-Exception},System-String,System-String,System-Boolean,System-Threading-CancellationToken-'></a>
### SubscribeAsync\`\`1(messageReceived,errorReceived,channel,group,ignoreMessageHeader,cancellationToken) `method`

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
| cancellationToken | [System.Threading.CancellationToken](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Threading.CancellationToken 'System.Threading.CancellationToken') | A cancellation token |

##### Generic Types

| Name | Description |
| ---- | ----------- |
| T | The type of message to listen for |

<a name='M-MQContract-Interfaces-IContractConnection-SubscribeQueryAsyncResponseAsync``2-System-Func{MQContract-Interfaces-IReceivedMessage{``0},System-Threading-Tasks-ValueTask{MQContract-Messages-QueryResponseMessage{``1}}},System-Action{System-Exception},System-String,System-String,System-Boolean,System-Threading-CancellationToken-'></a>
### SubscribeQueryAsyncResponseAsync\`\`2(messageReceived,errorReceived,channel,group,ignoreMessageHeader,cancellationToken) `method`

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
| cancellationToken | [System.Threading.CancellationToken](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Threading.CancellationToken 'System.Threading.CancellationToken') | A cancellation token |

##### Generic Types

| Name | Description |
| ---- | ----------- |
| Q | The type of message to listen for |
| R | The type of message to respond with |

<a name='M-MQContract-Interfaces-IContractConnection-SubscribeQueryResponseAsync``2-System-Func{MQContract-Interfaces-IReceivedMessage{``0},MQContract-Messages-QueryResponseMessage{``1}},System-Action{System-Exception},System-String,System-String,System-Boolean,System-Threading-CancellationToken-'></a>
### SubscribeQueryResponseAsync\`\`2(messageReceived,errorReceived,channel,group,ignoreMessageHeader,cancellationToken) `method`

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
| cancellationToken | [System.Threading.CancellationToken](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Threading.CancellationToken 'System.Threading.CancellationToken') | A cancellation token |

##### Generic Types

| Name | Description |
| ---- | ----------- |
| Q | The type of message to listen for |
| R | The type of message to respond with |

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
| T | The source message type |
| V | The destination message type |

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
| T | The type of message being decoded |

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
| T | The type of message being encoded |

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

<a name='M-MQContract-Interfaces-Encrypting-IMessageEncryptor-EncryptAsync-System-Byte[],System-Collections-Generic-Dictionary{System-String,System-String}@-'></a>
### EncryptAsync(data,headers) `method`

##### Summary

Called to encrypt the message body prior to transmitting a message

##### Returns

An encrypted byte array of the message body

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| data | [System.Byte[]](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Byte[] 'System.Byte[]') | The original unencrypted body data |
| headers | [System.Collections.Generic.Dictionary{System.String,System.String}@](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Collections.Generic.Dictionary 'System.Collections.Generic.Dictionary{System.String,System.String}@') | The headers that are desired to attache to the message if needed |

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

<a name='M-MQContract-Interfaces-Service-IMessageServiceConnection-SubscribeAsync-System-Action{MQContract-Messages-ReceivedServiceMessage},System-Action{System-Exception},System-String,System-String,System-Threading-CancellationToken-'></a>
### SubscribeAsync(messageReceived,errorReceived,channel,group,cancellationToken) `method`

##### Summary

Implements a call to create a subscription to a given channel as a member of a given group

##### Returns

A service subscription object

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| messageReceived | [System.Action{MQContract.Messages.ReceivedServiceMessage}](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Action 'System.Action{MQContract.Messages.ReceivedServiceMessage}') | The callback to invoke when a message is received |
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
| T | The type of message that this encoder supports |

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
| T | The type of message that this encryptor supports |

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

<a name='T-MQContract-Interfaces-Service-IQueryableMessageServiceConnection'></a>
## IQueryableMessageServiceConnection `type`

##### Namespace

MQContract.Interfaces.Service

##### Summary

Extends the base MessageServiceConnection Interface to Response Query messaging methodology if the underlying service supports it

<a name='P-MQContract-Interfaces-Service-IQueryableMessageServiceConnection-DefaultTimout'></a>
### DefaultTimout `property`

##### Summary

The default timeout to use for RPC calls when it's not specified

<a name='M-MQContract-Interfaces-Service-IQueryableMessageServiceConnection-QueryAsync-MQContract-Messages-ServiceMessage,System-TimeSpan,System-Threading-CancellationToken-'></a>
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
| T | The class type of the underlying message |

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

<a name='T-MQContract-Attributes-MessageChannelAttribute'></a>
## MessageChannelAttribute `type`

##### Namespace

MQContract.Attributes

##### Summary

Use this attribute to specify the Channel name used for transmitting this message class.
This can be overidden by specifying the channel on the method calls, but a value must 
be specified, either using the attribute or by specifying in the input.

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| name | [T:MQContract.Attributes.MessageChannelAttribute](#T-T-MQContract-Attributes-MessageChannelAttribute 'T:MQContract.Attributes.MessageChannelAttribute') | The name of the Channel to be used for transmitting this message class. |

##### Remarks



<a name='M-MQContract-Attributes-MessageChannelAttribute-#ctor-System-String-'></a>
### #ctor(name) `constructor`

##### Summary

Use this attribute to specify the Channel name used for transmitting this message class.
This can be overidden by specifying the channel on the method calls, but a value must 
be specified, either using the attribute or by specifying in the input.

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| name | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | The name of the Channel to be used for transmitting this message class. |

##### Remarks



<a name='P-MQContract-Attributes-MessageChannelAttribute-Name'></a>
### Name `property`

##### Summary

The name of the channel specified

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

<a name='T-MQContract-Attributes-MessageNameAttribute'></a>
## MessageNameAttribute `type`

##### Namespace

MQContract.Attributes

##### Summary

Used to specify the name of the message type inside the system.  
Default is to use the class name, however, this can be used to 
override that and allow for different versions of a message to 
have the same name withing the tranmission system.

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| value | [T:MQContract.Attributes.MessageNameAttribute](#T-T-MQContract-Attributes-MessageNameAttribute 'T:MQContract.Attributes.MessageNameAttribute') | The name to use for the class when transmitting |

##### Remarks



<a name='M-MQContract-Attributes-MessageNameAttribute-#ctor-System-String-'></a>
### #ctor(value) `constructor`

##### Summary

Used to specify the name of the message type inside the system.  
Default is to use the class name, however, this can be used to 
override that and allow for different versions of a message to 
have the same name withing the tranmission system.

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| value | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | The name to use for the class when transmitting |

##### Remarks



<a name='P-MQContract-Attributes-MessageNameAttribute-Value'></a>
### Value `property`

##### Summary

The name of the class used when transmitting

<a name='T-MQContract-Attributes-MessageResponseTimeoutAttribute'></a>
## MessageResponseTimeoutAttribute `type`

##### Namespace

MQContract.Attributes

##### Summary

Use this attribute to specify the timeout (in milliseconds) for a response 
from an RPC call for the specific class that this is attached to.  This can 
be overridden by supplying a timeout value when making an RPC call.

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| value | [T:MQContract.Attributes.MessageResponseTimeoutAttribute](#T-T-MQContract-Attributes-MessageResponseTimeoutAttribute 'T:MQContract.Attributes.MessageResponseTimeoutAttribute') | The number of milliseconds for an RPC call response to return |

##### Remarks



<a name='M-MQContract-Attributes-MessageResponseTimeoutAttribute-#ctor-System-Int32-'></a>
### #ctor(value) `constructor`

##### Summary

Use this attribute to specify the timeout (in milliseconds) for a response 
from an RPC call for the specific class that this is attached to.  This can 
be overridden by supplying a timeout value when making an RPC call.

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| value | [System.Int32](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Int32 'System.Int32') | The number of milliseconds for an RPC call response to return |

##### Remarks



<a name='P-MQContract-Attributes-MessageResponseTimeoutAttribute-TimeSpanValue'></a>
### TimeSpanValue `property`

##### Summary

The converted TimeSpan value from the supplied milliseconds value in the constructor

<a name='P-MQContract-Attributes-MessageResponseTimeoutAttribute-Value'></a>
### Value `property`

##### Summary

The number of milliseconds for the timeout to trigger for this RPC call class

<a name='T-MQContract-Attributes-MessageVersionAttribute'></a>
## MessageVersionAttribute `type`

##### Namespace

MQContract.Attributes

##### Summary

Used to tag the version number of a specific message class.
By default all messages are tagged as version 0.0.0.0.
By using this tag, combined with the MessageName you can create multiple
versions of the same message and if you create converters for those versions
it allows you to not necessarily update code for call handling immediately.

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| version | [T:MQContract.Attributes.MessageVersionAttribute](#T-T-MQContract-Attributes-MessageVersionAttribute 'T:MQContract.Attributes.MessageVersionAttribute') | The version number to tag this message class during transmission |

##### Remarks



<a name='M-MQContract-Attributes-MessageVersionAttribute-#ctor-System-String-'></a>
### #ctor(version) `constructor`

##### Summary

Used to tag the version number of a specific message class.
By default all messages are tagged as version 0.0.0.0.
By using this tag, combined with the MessageName you can create multiple
versions of the same message and if you create converters for those versions
it allows you to not necessarily update code for call handling immediately.

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| version | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | The version number to tag this message class during transmission |

##### Remarks



<a name='P-MQContract-Attributes-MessageVersionAttribute-Version'></a>
### Version `property`

##### Summary

The version number to tag this class with during transmission

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

<a name='T-MQContract-Attributes-QueryResponseChannelAttribute'></a>
## QueryResponseChannelAttribute `type`

##### Namespace

MQContract.Attributes

##### Summary

Used to allow the specification of a response channel to be used without supplying it to the contract calls.  
IMPORTANT:  This particular attribute and the response channel argument are only used when the underlying connection does not support QueryResponse messaging.

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| name | [T:MQContract.Attributes.QueryResponseChannelAttribute](#T-T-MQContract-Attributes-QueryResponseChannelAttribute 'T:MQContract.Attributes.QueryResponseChannelAttribute') | The name of the channel to use for responses |

<a name='M-MQContract-Attributes-QueryResponseChannelAttribute-#ctor-System-String-'></a>
### #ctor(name) `constructor`

##### Summary

Used to allow the specification of a response channel to be used without supplying it to the contract calls.  
IMPORTANT:  This particular attribute and the response channel argument are only used when the underlying connection does not support QueryResponse messaging.

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| name | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | The name of the channel to use for responses |

<a name='P-MQContract-Attributes-QueryResponseChannelAttribute-Name'></a>
### Name `property`

##### Summary

The Name of the response channel

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
| T | The type of message contained in the response |

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
| T | The type of message contained in the response |

<a name='P-MQContract-Messages-QueryResponseMessage`1-Headers'></a>
### Headers `property`

##### Summary

The headers to attach to the response

<a name='P-MQContract-Messages-QueryResponseMessage`1-Message'></a>
### Message `property`

##### Summary

The message to respond back with

<a name='T-MQContract-Attributes-QueryResponseTypeAttribute'></a>
## QueryResponseTypeAttribute `type`

##### Namespace

MQContract.Attributes

##### Summary

Used to allow the specification of a response type without supplying it to the contract calls

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| responseType | [T:MQContract.Attributes.QueryResponseTypeAttribute](#T-T-MQContract-Attributes-QueryResponseTypeAttribute 'T:MQContract.Attributes.QueryResponseTypeAttribute') | The type of class that should be expected for a response |

##### Remarks

Default constructor

<a name='M-MQContract-Attributes-QueryResponseTypeAttribute-#ctor-System-Type-'></a>
### #ctor(responseType) `constructor`

##### Summary

Used to allow the specification of a response type without supplying it to the contract calls

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| responseType | [System.Type](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Type 'System.Type') | The type of class that should be expected for a response |

##### Remarks

Default constructor

<a name='P-MQContract-Attributes-QueryResponseTypeAttribute-ResponseType'></a>
### ResponseType `property`

##### Summary

The type of class that should be expected for a Response when not specified

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
| T | The type of message in the response |

<a name='M-MQContract-Messages-QueryResult`1-#ctor-System-String,MQContract-Messages-MessageHeader,`0,System-String-'></a>
### #ctor(ID,Header,Result,Error) `constructor`

##### Summary

Houses the result from a Query call into the system

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| ID | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | The unique ID of the message |
| Header | [MQContract.Messages.MessageHeader](#T-MQContract-Messages-MessageHeader 'MQContract.Messages.MessageHeader') | The response headers |
| Result | [\`0](#T-`0 '`0') | The resulting response if there was one |
| Error | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | The error message for the response if it failed and an error was returned |

##### Generic Types

| Name | Description |
| ---- | ----------- |
| T | The type of message in the response |

<a name='P-MQContract-Messages-QueryResult`1-Header'></a>
### Header `property`

##### Summary

The response headers

<a name='P-MQContract-Messages-QueryResult`1-Result'></a>
### Result `property`

##### Summary

The resulting response if there was one

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

<a name='M-MQContract-Messages-TransmissionResult-#ctor-System-String,System-String-'></a>
### #ctor(ID,Error) `constructor`

##### Summary

Houses the result of a transmission into the system

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| ID | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | The unique ID of the message that was transmitted |
| Error | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | An error message if an error occured |

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

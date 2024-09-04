<a name='assembly'></a>
# MQContract.Abstractions

## Contents

- [IContractConnection](#T-MQContract-Interfaces-IContractConnection 'MQContract.Interfaces.IContractConnection')
  - [CloseAsync()](#M-MQContract-Interfaces-IContractConnection-CloseAsync 'MQContract.Interfaces.IContractConnection.CloseAsync')
  - [PingAsync()](#M-MQContract-Interfaces-IContractConnection-PingAsync 'MQContract.Interfaces.IContractConnection.PingAsync')
  - [PublishAsync\`\`1(message,channel,messageHeader,options,cancellationToken)](#M-MQContract-Interfaces-IContractConnection-PublishAsync``1-``0,System-String,MQContract-Messages-MessageHeader,MQContract-Interfaces-Service-IServiceChannelOptions,System-Threading-CancellationToken- 'MQContract.Interfaces.IContractConnection.PublishAsync``1(``0,System.String,MQContract.Messages.MessageHeader,MQContract.Interfaces.Service.IServiceChannelOptions,System.Threading.CancellationToken)')
  - [QueryAsync\`\`1(message,timeout,channel,responseChannel,messageHeader,options,cancellationToken)](#M-MQContract-Interfaces-IContractConnection-QueryAsync``1-``0,System-Nullable{System-TimeSpan},System-String,System-String,MQContract-Messages-MessageHeader,MQContract-Interfaces-Service-IServiceChannelOptions,System-Threading-CancellationToken- 'MQContract.Interfaces.IContractConnection.QueryAsync``1(``0,System.Nullable{System.TimeSpan},System.String,System.String,MQContract.Messages.MessageHeader,MQContract.Interfaces.Service.IServiceChannelOptions,System.Threading.CancellationToken)')
  - [QueryAsync\`\`2(message,timeout,channel,responseChannel,messageHeader,options,cancellationToken)](#M-MQContract-Interfaces-IContractConnection-QueryAsync``2-``0,System-Nullable{System-TimeSpan},System-String,System-String,MQContract-Messages-MessageHeader,MQContract-Interfaces-Service-IServiceChannelOptions,System-Threading-CancellationToken- 'MQContract.Interfaces.IContractConnection.QueryAsync``2(``0,System.Nullable{System.TimeSpan},System.String,System.String,MQContract.Messages.MessageHeader,MQContract.Interfaces.Service.IServiceChannelOptions,System.Threading.CancellationToken)')
  - [SubscribeAsync\`\`1(messageRecieved,errorRecieved,channel,group,ignoreMessageHeader,options,cancellationToken)](#M-MQContract-Interfaces-IContractConnection-SubscribeAsync``1-System-Func{MQContract-Interfaces-IRecievedMessage{``0},System-Threading-Tasks-ValueTask},System-Action{System-Exception},System-String,System-String,System-Boolean,MQContract-Interfaces-Service-IServiceChannelOptions,System-Threading-CancellationToken- 'MQContract.Interfaces.IContractConnection.SubscribeAsync``1(System.Func{MQContract.Interfaces.IRecievedMessage{``0},System.Threading.Tasks.ValueTask},System.Action{System.Exception},System.String,System.String,System.Boolean,MQContract.Interfaces.Service.IServiceChannelOptions,System.Threading.CancellationToken)')
  - [SubscribeAsync\`\`1(messageRecieved,errorRecieved,channel,group,ignoreMessageHeader,options,cancellationToken)](#M-MQContract-Interfaces-IContractConnection-SubscribeAsync``1-System-Action{MQContract-Interfaces-IRecievedMessage{``0}},System-Action{System-Exception},System-String,System-String,System-Boolean,MQContract-Interfaces-Service-IServiceChannelOptions,System-Threading-CancellationToken- 'MQContract.Interfaces.IContractConnection.SubscribeAsync``1(System.Action{MQContract.Interfaces.IRecievedMessage{``0}},System.Action{System.Exception},System.String,System.String,System.Boolean,MQContract.Interfaces.Service.IServiceChannelOptions,System.Threading.CancellationToken)')
  - [SubscribeQueryAsyncResponseAsync\`\`2(messageRecieved,errorRecieved,channel,group,ignoreMessageHeader,options,cancellationToken)](#M-MQContract-Interfaces-IContractConnection-SubscribeQueryAsyncResponseAsync``2-System-Func{MQContract-Interfaces-IRecievedMessage{``0},System-Threading-Tasks-ValueTask{MQContract-Messages-QueryResponseMessage{``1}}},System-Action{System-Exception},System-String,System-String,System-Boolean,MQContract-Interfaces-Service-IServiceChannelOptions,System-Threading-CancellationToken- 'MQContract.Interfaces.IContractConnection.SubscribeQueryAsyncResponseAsync``2(System.Func{MQContract.Interfaces.IRecievedMessage{``0},System.Threading.Tasks.ValueTask{MQContract.Messages.QueryResponseMessage{``1}}},System.Action{System.Exception},System.String,System.String,System.Boolean,MQContract.Interfaces.Service.IServiceChannelOptions,System.Threading.CancellationToken)')
  - [SubscribeQueryResponseAsync\`\`2(messageRecieved,errorRecieved,channel,group,ignoreMessageHeader,options,cancellationToken)](#M-MQContract-Interfaces-IContractConnection-SubscribeQueryResponseAsync``2-System-Func{MQContract-Interfaces-IRecievedMessage{``0},MQContract-Messages-QueryResponseMessage{``1}},System-Action{System-Exception},System-String,System-String,System-Boolean,MQContract-Interfaces-Service-IServiceChannelOptions,System-Threading-CancellationToken- 'MQContract.Interfaces.IContractConnection.SubscribeQueryResponseAsync``2(System.Func{MQContract.Interfaces.IRecievedMessage{``0},MQContract.Messages.QueryResponseMessage{``1}},System.Action{System.Exception},System.String,System.String,System.Boolean,MQContract.Interfaces.Service.IServiceChannelOptions,System.Threading.CancellationToken)')
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
  - [DefaultTimout](#P-MQContract-Interfaces-Service-IMessageServiceConnection-DefaultTimout 'MQContract.Interfaces.Service.IMessageServiceConnection.DefaultTimout')
  - [MaxMessageBodySize](#P-MQContract-Interfaces-Service-IMessageServiceConnection-MaxMessageBodySize 'MQContract.Interfaces.Service.IMessageServiceConnection.MaxMessageBodySize')
  - [CloseAsync()](#M-MQContract-Interfaces-Service-IMessageServiceConnection-CloseAsync 'MQContract.Interfaces.Service.IMessageServiceConnection.CloseAsync')
  - [PublishAsync(message,options,cancellationToken)](#M-MQContract-Interfaces-Service-IMessageServiceConnection-PublishAsync-MQContract-Messages-ServiceMessage,MQContract-Interfaces-Service-IServiceChannelOptions,System-Threading-CancellationToken- 'MQContract.Interfaces.Service.IMessageServiceConnection.PublishAsync(MQContract.Messages.ServiceMessage,MQContract.Interfaces.Service.IServiceChannelOptions,System.Threading.CancellationToken)')
  - [SubscribeAsync(messageRecieved,errorRecieved,channel,group,options,cancellationToken)](#M-MQContract-Interfaces-Service-IMessageServiceConnection-SubscribeAsync-System-Action{MQContract-Messages-RecievedServiceMessage},System-Action{System-Exception},System-String,System-String,MQContract-Interfaces-Service-IServiceChannelOptions,System-Threading-CancellationToken- 'MQContract.Interfaces.Service.IMessageServiceConnection.SubscribeAsync(System.Action{MQContract.Messages.RecievedServiceMessage},System.Action{System.Exception},System.String,System.String,MQContract.Interfaces.Service.IServiceChannelOptions,System.Threading.CancellationToken)')
- [IMessageTypeEncoder\`1](#T-MQContract-Interfaces-Encoding-IMessageTypeEncoder`1 'MQContract.Interfaces.Encoding.IMessageTypeEncoder`1')
  - [DecodeAsync(stream)](#M-MQContract-Interfaces-Encoding-IMessageTypeEncoder`1-DecodeAsync-System-IO-Stream- 'MQContract.Interfaces.Encoding.IMessageTypeEncoder`1.DecodeAsync(System.IO.Stream)')
  - [EncodeAsync(message)](#M-MQContract-Interfaces-Encoding-IMessageTypeEncoder`1-EncodeAsync-`0- 'MQContract.Interfaces.Encoding.IMessageTypeEncoder`1.EncodeAsync(`0)')
- [IMessageTypeEncryptor\`1](#T-MQContract-Interfaces-Encrypting-IMessageTypeEncryptor`1 'MQContract.Interfaces.Encrypting.IMessageTypeEncryptor`1')
- [IPingableMessageServiceConnection](#T-MQContract-Interfaces-Service-IPingableMessageServiceConnection 'MQContract.Interfaces.Service.IPingableMessageServiceConnection')
  - [PingAsync()](#M-MQContract-Interfaces-Service-IPingableMessageServiceConnection-PingAsync 'MQContract.Interfaces.Service.IPingableMessageServiceConnection.PingAsync')
- [IQueryableMessageServiceConnection](#T-MQContract-Interfaces-Service-IQueryableMessageServiceConnection 'MQContract.Interfaces.Service.IQueryableMessageServiceConnection')
  - [QueryAsync(message,timeout,options,cancellationToken)](#M-MQContract-Interfaces-Service-IQueryableMessageServiceConnection-QueryAsync-MQContract-Messages-ServiceMessage,System-TimeSpan,MQContract-Interfaces-Service-IServiceChannelOptions,System-Threading-CancellationToken- 'MQContract.Interfaces.Service.IQueryableMessageServiceConnection.QueryAsync(MQContract.Messages.ServiceMessage,System.TimeSpan,MQContract.Interfaces.Service.IServiceChannelOptions,System.Threading.CancellationToken)')
  - [SubscribeQueryAsync(messageRecieved,errorRecieved,channel,group,options,cancellationToken)](#M-MQContract-Interfaces-Service-IQueryableMessageServiceConnection-SubscribeQueryAsync-System-Func{MQContract-Messages-RecievedServiceMessage,System-Threading-Tasks-ValueTask{MQContract-Messages-ServiceMessage}},System-Action{System-Exception},System-String,System-String,MQContract-Interfaces-Service-IServiceChannelOptions,System-Threading-CancellationToken- 'MQContract.Interfaces.Service.IQueryableMessageServiceConnection.SubscribeQueryAsync(System.Func{MQContract.Messages.RecievedServiceMessage,System.Threading.Tasks.ValueTask{MQContract.Messages.ServiceMessage}},System.Action{System.Exception},System.String,System.String,MQContract.Interfaces.Service.IServiceChannelOptions,System.Threading.CancellationToken)')
- [IRecievedMessage\`1](#T-MQContract-Interfaces-IRecievedMessage`1 'MQContract.Interfaces.IRecievedMessage`1')
  - [Headers](#P-MQContract-Interfaces-IRecievedMessage`1-Headers 'MQContract.Interfaces.IRecievedMessage`1.Headers')
  - [ID](#P-MQContract-Interfaces-IRecievedMessage`1-ID 'MQContract.Interfaces.IRecievedMessage`1.ID')
  - [Message](#P-MQContract-Interfaces-IRecievedMessage`1-Message 'MQContract.Interfaces.IRecievedMessage`1.Message')
  - [ProcessedTimestamp](#P-MQContract-Interfaces-IRecievedMessage`1-ProcessedTimestamp 'MQContract.Interfaces.IRecievedMessage`1.ProcessedTimestamp')
  - [RecievedTimestamp](#P-MQContract-Interfaces-IRecievedMessage`1-RecievedTimestamp 'MQContract.Interfaces.IRecievedMessage`1.RecievedTimestamp')
- [IServiceChannelOptions](#T-MQContract-Interfaces-Service-IServiceChannelOptions 'MQContract.Interfaces.Service.IServiceChannelOptions')
- [IServiceSubscription](#T-MQContract-Interfaces-Service-IServiceSubscription 'MQContract.Interfaces.Service.IServiceSubscription')
  - [EndAsync()](#M-MQContract-Interfaces-Service-IServiceSubscription-EndAsync 'MQContract.Interfaces.Service.IServiceSubscription.EndAsync')
- [ISubscription](#T-MQContract-Interfaces-ISubscription 'MQContract.Interfaces.ISubscription')
  - [EndAsync()](#M-MQContract-Interfaces-ISubscription-EndAsync 'MQContract.Interfaces.ISubscription.EndAsync')
- [InvalidChannelOptionsTypeException](#T-MQContract-InvalidChannelOptionsTypeException 'MQContract.InvalidChannelOptionsTypeException')
  - [ThrowIfNotNullAndNotOfType(options,expectedTypes)](#M-MQContract-InvalidChannelOptionsTypeException-ThrowIfNotNullAndNotOfType-MQContract-Interfaces-Service-IServiceChannelOptions,System-Collections-Generic-IEnumerable{System-Type}- 'MQContract.InvalidChannelOptionsTypeException.ThrowIfNotNullAndNotOfType(MQContract.Interfaces.Service.IServiceChannelOptions,System.Collections.Generic.IEnumerable{System.Type})')
  - [ThrowIfNotNullAndNotOfType\`\`1(options)](#M-MQContract-InvalidChannelOptionsTypeException-ThrowIfNotNullAndNotOfType``1-MQContract-Interfaces-Service-IServiceChannelOptions- 'MQContract.InvalidChannelOptionsTypeException.ThrowIfNotNullAndNotOfType``1(MQContract.Interfaces.Service.IServiceChannelOptions)')
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
  - [Value](#P-MQContract-Attributes-MessageResponseTimeoutAttribute-Value 'MQContract.Attributes.MessageResponseTimeoutAttribute.Value')
- [MessageVersionAttribute](#T-MQContract-Attributes-MessageVersionAttribute 'MQContract.Attributes.MessageVersionAttribute')
  - [#ctor(version)](#M-MQContract-Attributes-MessageVersionAttribute-#ctor-System-String- 'MQContract.Attributes.MessageVersionAttribute.#ctor(System.String)')
  - [Version](#P-MQContract-Attributes-MessageVersionAttribute-Version 'MQContract.Attributes.MessageVersionAttribute.Version')
- [NoChannelOptionsAvailableException](#T-MQContract-NoChannelOptionsAvailableException 'MQContract.NoChannelOptionsAvailableException')
  - [ThrowIfNotNull(options)](#M-MQContract-NoChannelOptionsAvailableException-ThrowIfNotNull-MQContract-Interfaces-Service-IServiceChannelOptions- 'MQContract.NoChannelOptionsAvailableException.ThrowIfNotNull(MQContract.Interfaces.Service.IServiceChannelOptions)')
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
- [RecievedServiceMessage](#T-MQContract-Messages-RecievedServiceMessage 'MQContract.Messages.RecievedServiceMessage')
  - [#ctor(ID,MessageTypeID,Channel,Header,Data)](#M-MQContract-Messages-RecievedServiceMessage-#ctor-System-String,System-String,System-String,MQContract-Messages-MessageHeader,System-ReadOnlyMemory{System-Byte}- 'MQContract.Messages.RecievedServiceMessage.#ctor(System.String,System.String,System.String,MQContract.Messages.MessageHeader,System.ReadOnlyMemory{System.Byte})')
  - [RecievedTimestamp](#P-MQContract-Messages-RecievedServiceMessage-RecievedTimestamp 'MQContract.Messages.RecievedServiceMessage.RecievedTimestamp')
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

<a name='M-MQContract-Interfaces-IContractConnection-PingAsync'></a>
### PingAsync() `method`

##### Summary

Called to Ping the underlying system to obtain both information and ensure it is up.  Not all Services support this method.

##### Returns



##### Parameters

This method has no parameters.

<a name='M-MQContract-Interfaces-IContractConnection-PublishAsync``1-``0,System-String,MQContract-Messages-MessageHeader,MQContract-Interfaces-Service-IServiceChannelOptions,System-Threading-CancellationToken-'></a>
### PublishAsync\`\`1(message,channel,messageHeader,options,cancellationToken) `method`

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
| options | [MQContract.Interfaces.Service.IServiceChannelOptions](#T-MQContract-Interfaces-Service-IServiceChannelOptions 'MQContract.Interfaces.Service.IServiceChannelOptions') | Any required Service Channel Options that will be passed down to the service Connection |
| cancellationToken | [System.Threading.CancellationToken](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Threading.CancellationToken 'System.Threading.CancellationToken') | A cancellation token |

##### Generic Types

| Name | Description |
| ---- | ----------- |
| T | The type of message to send |

<a name='M-MQContract-Interfaces-IContractConnection-QueryAsync``1-``0,System-Nullable{System-TimeSpan},System-String,System-String,MQContract-Messages-MessageHeader,MQContract-Interfaces-Service-IServiceChannelOptions,System-Threading-CancellationToken-'></a>
### QueryAsync\`\`1(message,timeout,channel,responseChannel,messageHeader,options,cancellationToken) `method`

##### Summary

Called to send a message into the underlying service in the Query/Response style.  The return type is not specified here and is instead obtained from the QueryResponseTypeAttribute
attached to the Query message type class.

##### Returns

A result indicating the success or failure as well as the returned message

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| message | [\`\`0](#T-``0 '``0') | The message to send |
| timeout | [System.Nullable{System.TimeSpan}](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Nullable 'System.Nullable{System.TimeSpan}') | The allowed timeout prior to a response being recieved |
| channel | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | Specifies the message channel to use.  The prefered method is using the MessageChannelAttribute on the class. |
| responseChannel | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | Specifies the message channel to use for the response.  The preferred method is using the QueryResponseChannelAttribute on the class.  This is 
only used when the underlying connection does not support a QueryResponse style messaging. |
| messageHeader | [MQContract.Messages.MessageHeader](#T-MQContract-Messages-MessageHeader 'MQContract.Messages.MessageHeader') | The headers to pass along with the message |
| options | [MQContract.Interfaces.Service.IServiceChannelOptions](#T-MQContract-Interfaces-Service-IServiceChannelOptions 'MQContract.Interfaces.Service.IServiceChannelOptions') | Any required Service Channel Options that will be passed down to the service Connection |
| cancellationToken | [System.Threading.CancellationToken](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Threading.CancellationToken 'System.Threading.CancellationToken') | A cancellation token |

##### Generic Types

| Name | Description |
| ---- | ----------- |
| Q | The type of message to send for the query |

<a name='M-MQContract-Interfaces-IContractConnection-QueryAsync``2-``0,System-Nullable{System-TimeSpan},System-String,System-String,MQContract-Messages-MessageHeader,MQContract-Interfaces-Service-IServiceChannelOptions,System-Threading-CancellationToken-'></a>
### QueryAsync\`\`2(message,timeout,channel,responseChannel,messageHeader,options,cancellationToken) `method`

##### Summary

Called to send a message into the underlying service in the Query/Response style

##### Returns

A result indicating the success or failure as well as the returned message

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| message | [\`\`0](#T-``0 '``0') | The message to send |
| timeout | [System.Nullable{System.TimeSpan}](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Nullable 'System.Nullable{System.TimeSpan}') | The allowed timeout prior to a response being recieved |
| channel | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | Specifies the message channel to use.  The prefered method is using the MessageChannelAttribute on the class. |
| responseChannel | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | Specifies the message channel to use for the response.  The preferred method is using the QueryResponseChannelAttribute on the class.  This is 
only used when the underlying connection does not support a QueryResponse style messaging. |
| messageHeader | [MQContract.Messages.MessageHeader](#T-MQContract-Messages-MessageHeader 'MQContract.Messages.MessageHeader') | The headers to pass along with the message |
| options | [MQContract.Interfaces.Service.IServiceChannelOptions](#T-MQContract-Interfaces-Service-IServiceChannelOptions 'MQContract.Interfaces.Service.IServiceChannelOptions') | Any required Service Channel Options that will be passed down to the service Connection |
| cancellationToken | [System.Threading.CancellationToken](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Threading.CancellationToken 'System.Threading.CancellationToken') | A cancellation token |

##### Generic Types

| Name | Description |
| ---- | ----------- |
| Q | The type of message to send for the query |
| R | The type of message to expect back for the response |

<a name='M-MQContract-Interfaces-IContractConnection-SubscribeAsync``1-System-Func{MQContract-Interfaces-IRecievedMessage{``0},System-Threading-Tasks-ValueTask},System-Action{System-Exception},System-String,System-String,System-Boolean,MQContract-Interfaces-Service-IServiceChannelOptions,System-Threading-CancellationToken-'></a>
### SubscribeAsync\`\`1(messageRecieved,errorRecieved,channel,group,ignoreMessageHeader,options,cancellationToken) `method`

##### Summary

Called to create a subscription into the underlying service Pub/Sub style and have the messages processed asynchronously

##### Returns

A subscription instance that can be ended when desired

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| messageRecieved | [System.Func{MQContract.Interfaces.IRecievedMessage{\`\`0},System.Threading.Tasks.ValueTask}](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Func 'System.Func{MQContract.Interfaces.IRecievedMessage{``0},System.Threading.Tasks.ValueTask}') | The callback invoked when a new message is recieved |
| errorRecieved | [System.Action{System.Exception}](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Action 'System.Action{System.Exception}') | The callback to invoke when an error occurs |
| channel | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | Specifies the message channel to use.  The prefered method is using the MessageChannelAttribute on the class. |
| group | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | The subscription group if desired (typically used when multiple instances of the same system are running) |
| ignoreMessageHeader | [System.Boolean](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Boolean 'System.Boolean') | If true, the message type specified will be ignored and it will automatically attempt to convert the underlying message to the given class |
| options | [MQContract.Interfaces.Service.IServiceChannelOptions](#T-MQContract-Interfaces-Service-IServiceChannelOptions 'MQContract.Interfaces.Service.IServiceChannelOptions') | Any required Service Channel Options that will be passed down to the service Connection |
| cancellationToken | [System.Threading.CancellationToken](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Threading.CancellationToken 'System.Threading.CancellationToken') | A cancellation token |

##### Generic Types

| Name | Description |
| ---- | ----------- |
| T | The type of message to listen for |

<a name='M-MQContract-Interfaces-IContractConnection-SubscribeAsync``1-System-Action{MQContract-Interfaces-IRecievedMessage{``0}},System-Action{System-Exception},System-String,System-String,System-Boolean,MQContract-Interfaces-Service-IServiceChannelOptions,System-Threading-CancellationToken-'></a>
### SubscribeAsync\`\`1(messageRecieved,errorRecieved,channel,group,ignoreMessageHeader,options,cancellationToken) `method`

##### Summary

Called to create a subscription into the underlying service Pub/Sub style and have the messages processed syncrhonously

##### Returns

A subscription instance that can be ended when desired

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| messageRecieved | [System.Action{MQContract.Interfaces.IRecievedMessage{\`\`0}}](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Action 'System.Action{MQContract.Interfaces.IRecievedMessage{``0}}') | The callback invoked when a new message is recieved |
| errorRecieved | [System.Action{System.Exception}](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Action 'System.Action{System.Exception}') | The callback to invoke when an error occurs |
| channel | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | Specifies the message channel to use.  The prefered method is using the MessageChannelAttribute on the class. |
| group | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | The subscription group if desired (typically used when multiple instances of the same system are running) |
| ignoreMessageHeader | [System.Boolean](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Boolean 'System.Boolean') | If true, the message type specified will be ignored and it will automatically attempt to convert the underlying message to the given class |
| options | [MQContract.Interfaces.Service.IServiceChannelOptions](#T-MQContract-Interfaces-Service-IServiceChannelOptions 'MQContract.Interfaces.Service.IServiceChannelOptions') | Any required Service Channel Options that will be passed down to the service Connection |
| cancellationToken | [System.Threading.CancellationToken](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Threading.CancellationToken 'System.Threading.CancellationToken') | A cancellation token |

##### Generic Types

| Name | Description |
| ---- | ----------- |
| T | The type of message to listen for |

<a name='M-MQContract-Interfaces-IContractConnection-SubscribeQueryAsyncResponseAsync``2-System-Func{MQContract-Interfaces-IRecievedMessage{``0},System-Threading-Tasks-ValueTask{MQContract-Messages-QueryResponseMessage{``1}}},System-Action{System-Exception},System-String,System-String,System-Boolean,MQContract-Interfaces-Service-IServiceChannelOptions,System-Threading-CancellationToken-'></a>
### SubscribeQueryAsyncResponseAsync\`\`2(messageRecieved,errorRecieved,channel,group,ignoreMessageHeader,options,cancellationToken) `method`

##### Summary

Called to create a subscription into the underlying service Query/Reponse style and have the messages processed asynchronously

##### Returns

A subscription instance that can be ended when desired

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| messageRecieved | [System.Func{MQContract.Interfaces.IRecievedMessage{\`\`0},System.Threading.Tasks.ValueTask{MQContract.Messages.QueryResponseMessage{\`\`1}}}](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Func 'System.Func{MQContract.Interfaces.IRecievedMessage{``0},System.Threading.Tasks.ValueTask{MQContract.Messages.QueryResponseMessage{``1}}}') | The callback invoked when a new message is recieved expecting a response of the type response |
| errorRecieved | [System.Action{System.Exception}](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Action 'System.Action{System.Exception}') | The callback invoked when an error occurs. |
| channel | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | Specifies the message channel to use.  The prefered method is using the MessageChannelAttribute on the class. |
| group | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | The subscription group if desired (typically used when multiple instances of the same system are running) |
| ignoreMessageHeader | [System.Boolean](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Boolean 'System.Boolean') | If true, the message type specified will be ignored and it will automatically attempt to convert the underlying message to the given class |
| options | [MQContract.Interfaces.Service.IServiceChannelOptions](#T-MQContract-Interfaces-Service-IServiceChannelOptions 'MQContract.Interfaces.Service.IServiceChannelOptions') | Any required Service Channel Options that will be passed down to the service Connection |
| cancellationToken | [System.Threading.CancellationToken](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Threading.CancellationToken 'System.Threading.CancellationToken') | A cancellation token |

##### Generic Types

| Name | Description |
| ---- | ----------- |
| Q | The type of message to listen for |
| R | The type of message to respond with |

<a name='M-MQContract-Interfaces-IContractConnection-SubscribeQueryResponseAsync``2-System-Func{MQContract-Interfaces-IRecievedMessage{``0},MQContract-Messages-QueryResponseMessage{``1}},System-Action{System-Exception},System-String,System-String,System-Boolean,MQContract-Interfaces-Service-IServiceChannelOptions,System-Threading-CancellationToken-'></a>
### SubscribeQueryResponseAsync\`\`2(messageRecieved,errorRecieved,channel,group,ignoreMessageHeader,options,cancellationToken) `method`

##### Summary

Called to create a subscription into the underlying service Query/Reponse style and have the messages processed synchronously

##### Returns

A subscription instance that can be ended when desired

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| messageRecieved | [System.Func{MQContract.Interfaces.IRecievedMessage{\`\`0},MQContract.Messages.QueryResponseMessage{\`\`1}}](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Func 'System.Func{MQContract.Interfaces.IRecievedMessage{``0},MQContract.Messages.QueryResponseMessage{``1}}') | The callback invoked when a new message is recieved expecting a response of the type response |
| errorRecieved | [System.Action{System.Exception}](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Action 'System.Action{System.Exception}') | The callback invoked when an error occurs. |
| channel | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | Specifies the message channel to use.  The prefered method is using the MessageChannelAttribute on the class. |
| group | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | The subscription group if desired (typically used when multiple instances of the same system are running) |
| ignoreMessageHeader | [System.Boolean](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Boolean 'System.Boolean') | If true, the message type specified will be ignored and it will automatically attempt to convert the underlying message to the given class |
| options | [MQContract.Interfaces.Service.IServiceChannelOptions](#T-MQContract-Interfaces-Service-IServiceChannelOptions 'MQContract.Interfaces.Service.IServiceChannelOptions') | Any required Service Channel Options that will be passed down to the service Connection |
| cancellationToken | [System.Threading.CancellationToken](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Threading.CancellationToken 'System.Threading.CancellationToken') | A cancellation token |

##### Generic Types

| Name | Description |
| ---- | ----------- |
| Q | The type of message to listen for |
| R | The type of message to respond with |

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
message is recieved on a channel of type T but it is waiting for 
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

Called to decrypt the message body stream recieved as a message

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

<a name='P-MQContract-Interfaces-Service-IMessageServiceConnection-DefaultTimout'></a>
### DefaultTimout `property`

##### Summary

The default timeout to use for RPC calls when it's not specified

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

<a name='M-MQContract-Interfaces-Service-IMessageServiceConnection-PublishAsync-MQContract-Messages-ServiceMessage,MQContract-Interfaces-Service-IServiceChannelOptions,System-Threading-CancellationToken-'></a>
### PublishAsync(message,options,cancellationToken) `method`

##### Summary

Implements a publish call to publish the given message

##### Returns

A transmission result instance indicating the result

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| message | [MQContract.Messages.ServiceMessage](#T-MQContract-Messages-ServiceMessage 'MQContract.Messages.ServiceMessage') | The message to publish |
| options | [MQContract.Interfaces.Service.IServiceChannelOptions](#T-MQContract-Interfaces-Service-IServiceChannelOptions 'MQContract.Interfaces.Service.IServiceChannelOptions') | The Service Channel Options instance that was supplied at the Contract Connection level |
| cancellationToken | [System.Threading.CancellationToken](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Threading.CancellationToken 'System.Threading.CancellationToken') | A cancellation token |

<a name='M-MQContract-Interfaces-Service-IMessageServiceConnection-SubscribeAsync-System-Action{MQContract-Messages-RecievedServiceMessage},System-Action{System-Exception},System-String,System-String,MQContract-Interfaces-Service-IServiceChannelOptions,System-Threading-CancellationToken-'></a>
### SubscribeAsync(messageRecieved,errorRecieved,channel,group,options,cancellationToken) `method`

##### Summary

Implements a call to create a subscription to a given channel as a member of a given group

##### Returns

A service subscription object

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| messageRecieved | [System.Action{MQContract.Messages.RecievedServiceMessage}](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Action 'System.Action{MQContract.Messages.RecievedServiceMessage}') | The callback to invoke when a message is recieved |
| errorRecieved | [System.Action{System.Exception}](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Action 'System.Action{System.Exception}') | The callback to invoke when an exception occurs |
| channel | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | The name of the channel to subscribe to |
| group | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | The subscription groupt to subscribe as |
| options | [MQContract.Interfaces.Service.IServiceChannelOptions](#T-MQContract-Interfaces-Service-IServiceChannelOptions 'MQContract.Interfaces.Service.IServiceChannelOptions') | The Service Channel Options instance that was supplied at the Contract Connection level |
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

<a name='M-MQContract-Interfaces-Service-IQueryableMessageServiceConnection-QueryAsync-MQContract-Messages-ServiceMessage,System-TimeSpan,MQContract-Interfaces-Service-IServiceChannelOptions,System-Threading-CancellationToken-'></a>
### QueryAsync(message,timeout,options,cancellationToken) `method`

##### Summary

Implements a call to submit a response query request into the underlying service

##### Returns

A Query Result instance based on what happened

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| message | [MQContract.Messages.ServiceMessage](#T-MQContract-Messages-ServiceMessage 'MQContract.Messages.ServiceMessage') | The message to query with |
| timeout | [System.TimeSpan](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.TimeSpan 'System.TimeSpan') | The timeout for recieving a response |
| options | [MQContract.Interfaces.Service.IServiceChannelOptions](#T-MQContract-Interfaces-Service-IServiceChannelOptions 'MQContract.Interfaces.Service.IServiceChannelOptions') | The Service Channel Options instance that was supplied at the Contract Connection level |
| cancellationToken | [System.Threading.CancellationToken](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Threading.CancellationToken 'System.Threading.CancellationToken') | A cancellation token |

<a name='M-MQContract-Interfaces-Service-IQueryableMessageServiceConnection-SubscribeQueryAsync-System-Func{MQContract-Messages-RecievedServiceMessage,System-Threading-Tasks-ValueTask{MQContract-Messages-ServiceMessage}},System-Action{System-Exception},System-String,System-String,MQContract-Interfaces-Service-IServiceChannelOptions,System-Threading-CancellationToken-'></a>
### SubscribeQueryAsync(messageRecieved,errorRecieved,channel,group,options,cancellationToken) `method`

##### Summary

Implements a call to create a subscription to a given channel as a member of a given group for responding to queries

##### Returns

A service subscription object

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| messageRecieved | [System.Func{MQContract.Messages.RecievedServiceMessage,System.Threading.Tasks.ValueTask{MQContract.Messages.ServiceMessage}}](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Func 'System.Func{MQContract.Messages.RecievedServiceMessage,System.Threading.Tasks.ValueTask{MQContract.Messages.ServiceMessage}}') | The callback to be invoked when a message is recieved, returning the response message |
| errorRecieved | [System.Action{System.Exception}](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Action 'System.Action{System.Exception}') | The callback to invoke when an exception occurs |
| channel | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | The name of the channel to subscribe to |
| group | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | The subscription groupt to subscribe as |
| options | [MQContract.Interfaces.Service.IServiceChannelOptions](#T-MQContract-Interfaces-Service-IServiceChannelOptions 'MQContract.Interfaces.Service.IServiceChannelOptions') | The Service Channel Options instance that was supplied at the Contract Connection level |
| cancellationToken | [System.Threading.CancellationToken](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Threading.CancellationToken 'System.Threading.CancellationToken') | A cancellation token |

<a name='T-MQContract-Interfaces-IRecievedMessage`1'></a>
## IRecievedMessage\`1 `type`

##### Namespace

MQContract.Interfaces

##### Summary

An interface for describing a Message recieved on a Subscription to be passed into the appropriate callback

##### Generic Types

| Name | Description |
| ---- | ----------- |
| T | The class type of the underlying message |

<a name='P-MQContract-Interfaces-IRecievedMessage`1-Headers'></a>
### Headers `property`

##### Summary

The headers that were supplied with the message

<a name='P-MQContract-Interfaces-IRecievedMessage`1-ID'></a>
### ID `property`

##### Summary

The unique ID of the recieved message that was specified on the transmission side

<a name='P-MQContract-Interfaces-IRecievedMessage`1-Message'></a>
### Message `property`

##### Summary

The message that was transmitted

<a name='P-MQContract-Interfaces-IRecievedMessage`1-ProcessedTimestamp'></a>
### ProcessedTimestamp `property`

##### Summary

The timestamp of when the recieved message was converted into the actual class prior to calling the callback

<a name='P-MQContract-Interfaces-IRecievedMessage`1-RecievedTimestamp'></a>
### RecievedTimestamp `property`

##### Summary

The timestamp of when the message was recieved by the underlying service connection

<a name='T-MQContract-Interfaces-Service-IServiceChannelOptions'></a>
## IServiceChannelOptions `type`

##### Namespace

MQContract.Interfaces.Service

##### Summary

Used to pass service channel options to the underlying service connection.  There are no implemented values this is simply mean to be a class marker.

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

<a name='T-MQContract-InvalidChannelOptionsTypeException'></a>
## InvalidChannelOptionsTypeException `type`

##### Namespace

MQContract

##### Summary

An exception thrown when the options supplied to an underlying system connection are not of an expected type.

<a name='M-MQContract-InvalidChannelOptionsTypeException-ThrowIfNotNullAndNotOfType-MQContract-Interfaces-Service-IServiceChannelOptions,System-Collections-Generic-IEnumerable{System-Type}-'></a>
### ThrowIfNotNullAndNotOfType(options,expectedTypes) `method`

##### Summary

Called to check if the options is one of the given types

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| options | [MQContract.Interfaces.Service.IServiceChannelOptions](#T-MQContract-Interfaces-Service-IServiceChannelOptions 'MQContract.Interfaces.Service.IServiceChannelOptions') | The supplied service channel options |
| expectedTypes | [System.Collections.Generic.IEnumerable{System.Type}](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Collections.Generic.IEnumerable 'System.Collections.Generic.IEnumerable{System.Type}') | The possible types it can be |

##### Exceptions

| Name | Description |
| ---- | ----------- |
| [MQContract.InvalidChannelOptionsTypeException](#T-MQContract-InvalidChannelOptionsTypeException 'MQContract.InvalidChannelOptionsTypeException') | Thrown when the options value is not null and not of any of the expected Types |

<a name='M-MQContract-InvalidChannelOptionsTypeException-ThrowIfNotNullAndNotOfType``1-MQContract-Interfaces-Service-IServiceChannelOptions-'></a>
### ThrowIfNotNullAndNotOfType\`\`1(options) `method`

##### Summary

Called to check if the options is of a given type

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| options | [MQContract.Interfaces.Service.IServiceChannelOptions](#T-MQContract-Interfaces-Service-IServiceChannelOptions 'MQContract.Interfaces.Service.IServiceChannelOptions') | The supplied service channel options |

##### Generic Types

| Name | Description |
| ---- | ----------- |
| T | The expected type for the ServiceChannelOptions |

##### Exceptions

| Name | Description |
| ---- | ----------- |
| [MQContract.InvalidChannelOptionsTypeException](#T-MQContract-InvalidChannelOptionsTypeException 'MQContract.InvalidChannelOptionsTypeException') | Thrown when the options value is not null and not of type T |

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

<a name='T-MQContract-NoChannelOptionsAvailableException'></a>
## NoChannelOptionsAvailableException `type`

##### Namespace

MQContract

##### Summary

An exception thrown when there are options supplied to an underlying system connection that does not support options for that particular instance

<a name='M-MQContract-NoChannelOptionsAvailableException-ThrowIfNotNull-MQContract-Interfaces-Service-IServiceChannelOptions-'></a>
### ThrowIfNotNull(options) `method`

##### Summary

Called to throw if options is not null

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| options | [MQContract.Interfaces.Service.IServiceChannelOptions](#T-MQContract-Interfaces-Service-IServiceChannelOptions 'MQContract.Interfaces.Service.IServiceChannelOptions') | The service channel options that were supplied |

##### Exceptions

| Name | Description |
| ---- | ----------- |
| [MQContract.NoChannelOptionsAvailableException](#T-MQContract-NoChannelOptionsAvailableException 'MQContract.NoChannelOptionsAvailableException') | Thrown when the options is not null |

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

<a name='T-MQContract-Messages-RecievedServiceMessage'></a>
## RecievedServiceMessage `type`

##### Namespace

MQContract.Messages

##### Summary

A Recieved Service Message that gets passed back up into the Contract Connection when a message is recieved from the underlying service connection

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| ID | [T:MQContract.Messages.RecievedServiceMessage](#T-T-MQContract-Messages-RecievedServiceMessage 'T:MQContract.Messages.RecievedServiceMessage') | The unique ID of the message |

<a name='M-MQContract-Messages-RecievedServiceMessage-#ctor-System-String,System-String,System-String,MQContract-Messages-MessageHeader,System-ReadOnlyMemory{System-Byte}-'></a>
### #ctor(ID,MessageTypeID,Channel,Header,Data) `constructor`

##### Summary

A Recieved Service Message that gets passed back up into the Contract Connection when a message is recieved from the underlying service connection

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| ID | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | The unique ID of the message |
| MessageTypeID | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | The message type id which is used for decoding to a class |
| Channel | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | The channel the message was recieved on |
| Header | [MQContract.Messages.MessageHeader](#T-MQContract-Messages-MessageHeader 'MQContract.Messages.MessageHeader') | The message headers that came through |
| Data | [System.ReadOnlyMemory{System.Byte}](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.ReadOnlyMemory 'System.ReadOnlyMemory{System.Byte}') | The binary content of the message that should be the encoded class |

<a name='P-MQContract-Messages-RecievedServiceMessage-RecievedTimestamp'></a>
### RecievedTimestamp `property`

##### Summary

A timestamp for when the message was recieved

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

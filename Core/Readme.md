<a name='assembly'></a>
# MQContract

## Contents

- [ChannelMapper](#T-MQContract-ChannelMapper 'MQContract.ChannelMapper')
  - [AddDefaultPublishMap(mapFunction)](#M-MQContract-ChannelMapper-AddDefaultPublishMap-System-Func{System-String,System-Threading-Tasks-ValueTask{System-String}}- 'MQContract.ChannelMapper.AddDefaultPublishMap(System.Func{System.String,System.Threading.Tasks.ValueTask{System.String}})')
  - [AddDefaultPublishSubscriptionMap(mapFunction)](#M-MQContract-ChannelMapper-AddDefaultPublishSubscriptionMap-System-Func{System-String,System-Threading-Tasks-ValueTask{System-String}}- 'MQContract.ChannelMapper.AddDefaultPublishSubscriptionMap(System.Func{System.String,System.Threading.Tasks.ValueTask{System.String}})')
  - [AddDefaultQueryMap(mapFunction)](#M-MQContract-ChannelMapper-AddDefaultQueryMap-System-Func{System-String,System-Threading-Tasks-ValueTask{System-String}}- 'MQContract.ChannelMapper.AddDefaultQueryMap(System.Func{System.String,System.Threading.Tasks.ValueTask{System.String}})')
  - [AddDefaultQuerySubscriptionMap(mapFunction)](#M-MQContract-ChannelMapper-AddDefaultQuerySubscriptionMap-System-Func{System-String,System-Threading-Tasks-ValueTask{System-String}}- 'MQContract.ChannelMapper.AddDefaultQuerySubscriptionMap(System.Func{System.String,System.Threading.Tasks.ValueTask{System.String}})')
  - [AddPublishMap(originalChannel,newChannel)](#M-MQContract-ChannelMapper-AddPublishMap-System-String,System-String- 'MQContract.ChannelMapper.AddPublishMap(System.String,System.String)')
  - [AddPublishMap(originalChannel,mapFunction)](#M-MQContract-ChannelMapper-AddPublishMap-System-String,System-Func{System-String,System-Threading-Tasks-ValueTask{System-String}}- 'MQContract.ChannelMapper.AddPublishMap(System.String,System.Func{System.String,System.Threading.Tasks.ValueTask{System.String}})')
  - [AddPublishMap(isMatch,mapFunction)](#M-MQContract-ChannelMapper-AddPublishMap-System-Func{System-String,System-Boolean},System-Func{System-String,System-Threading-Tasks-ValueTask{System-String}}- 'MQContract.ChannelMapper.AddPublishMap(System.Func{System.String,System.Boolean},System.Func{System.String,System.Threading.Tasks.ValueTask{System.String}})')
  - [AddPublishSubscriptionMap(originalChannel,newChannel)](#M-MQContract-ChannelMapper-AddPublishSubscriptionMap-System-String,System-String- 'MQContract.ChannelMapper.AddPublishSubscriptionMap(System.String,System.String)')
  - [AddPublishSubscriptionMap(originalChannel,mapFunction)](#M-MQContract-ChannelMapper-AddPublishSubscriptionMap-System-String,System-Func{System-String,System-Threading-Tasks-ValueTask{System-String}}- 'MQContract.ChannelMapper.AddPublishSubscriptionMap(System.String,System.Func{System.String,System.Threading.Tasks.ValueTask{System.String}})')
  - [AddPublishSubscriptionMap(isMatch,mapFunction)](#M-MQContract-ChannelMapper-AddPublishSubscriptionMap-System-Func{System-String,System-Boolean},System-Func{System-String,System-Threading-Tasks-ValueTask{System-String}}- 'MQContract.ChannelMapper.AddPublishSubscriptionMap(System.Func{System.String,System.Boolean},System.Func{System.String,System.Threading.Tasks.ValueTask{System.String}})')
  - [AddQueryMap(originalChannel,newChannel)](#M-MQContract-ChannelMapper-AddQueryMap-System-String,System-String- 'MQContract.ChannelMapper.AddQueryMap(System.String,System.String)')
  - [AddQueryMap(originalChannel,mapFunction)](#M-MQContract-ChannelMapper-AddQueryMap-System-String,System-Func{System-String,System-Threading-Tasks-ValueTask{System-String}}- 'MQContract.ChannelMapper.AddQueryMap(System.String,System.Func{System.String,System.Threading.Tasks.ValueTask{System.String}})')
  - [AddQueryMap(isMatch,mapFunction)](#M-MQContract-ChannelMapper-AddQueryMap-System-Func{System-String,System-Boolean},System-Func{System-String,System-Threading-Tasks-ValueTask{System-String}}- 'MQContract.ChannelMapper.AddQueryMap(System.Func{System.String,System.Boolean},System.Func{System.String,System.Threading.Tasks.ValueTask{System.String}})')
  - [AddQuerySubscriptionMap(originalChannel,newChannel)](#M-MQContract-ChannelMapper-AddQuerySubscriptionMap-System-String,System-String- 'MQContract.ChannelMapper.AddQuerySubscriptionMap(System.String,System.String)')
  - [AddQuerySubscriptionMap(originalChannel,mapFunction)](#M-MQContract-ChannelMapper-AddQuerySubscriptionMap-System-String,System-Func{System-String,System-Threading-Tasks-ValueTask{System-String}}- 'MQContract.ChannelMapper.AddQuerySubscriptionMap(System.String,System.Func{System.String,System.Threading.Tasks.ValueTask{System.String}})')
  - [AddQuerySubscriptionMap(isMatch,mapFunction)](#M-MQContract-ChannelMapper-AddQuerySubscriptionMap-System-Func{System-String,System-Boolean},System-Func{System-String,System-Threading-Tasks-ValueTask{System-String}}- 'MQContract.ChannelMapper.AddQuerySubscriptionMap(System.Func{System.String,System.Boolean},System.Func{System.String,System.Threading.Tasks.ValueTask{System.String}})')
- [ContractConnection](#T-MQContract-ContractConnection 'MQContract.ContractConnection')
  - [#ctor(serviceConnection,defaultMessageEncoder,defaultMessageEncryptor,serviceProvider,logger,channelMapper)](#M-MQContract-ContractConnection-#ctor-MQContract-Interfaces-Service-IMessageServiceConnection,MQContract-Interfaces-Encoding-IMessageEncoder,MQContract-Interfaces-Encrypting-IMessageEncryptor,System-IServiceProvider,Microsoft-Extensions-Logging-ILogger,MQContract-ChannelMapper- 'MQContract.ContractConnection.#ctor(MQContract.Interfaces.Service.IMessageServiceConnection,MQContract.Interfaces.Encoding.IMessageEncoder,MQContract.Interfaces.Encrypting.IMessageEncryptor,System.IServiceProvider,Microsoft.Extensions.Logging.ILogger,MQContract.ChannelMapper)')
  - [CloseAsync()](#M-MQContract-ContractConnection-CloseAsync 'MQContract.ContractConnection.CloseAsync')
  - [Dispose()](#M-MQContract-ContractConnection-Dispose 'MQContract.ContractConnection.Dispose')
  - [DisposeAsync()](#M-MQContract-ContractConnection-DisposeAsync 'MQContract.ContractConnection.DisposeAsync')
  - [PingAsync()](#M-MQContract-ContractConnection-PingAsync 'MQContract.ContractConnection.PingAsync')
  - [PublishAsync\`\`1(message,channel,messageHeader,options,cancellationToken)](#M-MQContract-ContractConnection-PublishAsync``1-``0,System-String,MQContract-Messages-MessageHeader,MQContract-Interfaces-Service-IServiceChannelOptions,System-Threading-CancellationToken- 'MQContract.ContractConnection.PublishAsync``1(``0,System.String,MQContract.Messages.MessageHeader,MQContract.Interfaces.Service.IServiceChannelOptions,System.Threading.CancellationToken)')
  - [QueryAsync\`\`1(message,timeout,channel,messageHeader,options,cancellationToken)](#M-MQContract-ContractConnection-QueryAsync``1-``0,System-Nullable{System-TimeSpan},System-String,MQContract-Messages-MessageHeader,MQContract-Interfaces-Service-IServiceChannelOptions,System-Threading-CancellationToken- 'MQContract.ContractConnection.QueryAsync``1(``0,System.Nullable{System.TimeSpan},System.String,MQContract.Messages.MessageHeader,MQContract.Interfaces.Service.IServiceChannelOptions,System.Threading.CancellationToken)')
  - [QueryAsync\`\`2(message,timeout,channel,messageHeader,options,cancellationToken)](#M-MQContract-ContractConnection-QueryAsync``2-``0,System-Nullable{System-TimeSpan},System-String,MQContract-Messages-MessageHeader,MQContract-Interfaces-Service-IServiceChannelOptions,System-Threading-CancellationToken- 'MQContract.ContractConnection.QueryAsync``2(``0,System.Nullable{System.TimeSpan},System.String,MQContract.Messages.MessageHeader,MQContract.Interfaces.Service.IServiceChannelOptions,System.Threading.CancellationToken)')
  - [SubscribeAsync\`\`1(messageRecieved,errorRecieved,channel,group,ignoreMessageHeader,options,cancellationToken)](#M-MQContract-ContractConnection-SubscribeAsync``1-System-Func{MQContract-Interfaces-IRecievedMessage{``0},System-Threading-Tasks-ValueTask},System-Action{System-Exception},System-String,System-String,System-Boolean,MQContract-Interfaces-Service-IServiceChannelOptions,System-Threading-CancellationToken- 'MQContract.ContractConnection.SubscribeAsync``1(System.Func{MQContract.Interfaces.IRecievedMessage{``0},System.Threading.Tasks.ValueTask},System.Action{System.Exception},System.String,System.String,System.Boolean,MQContract.Interfaces.Service.IServiceChannelOptions,System.Threading.CancellationToken)')
  - [SubscribeAsync\`\`1(messageRecieved,errorRecieved,channel,group,ignoreMessageHeader,options,cancellationToken)](#M-MQContract-ContractConnection-SubscribeAsync``1-System-Action{MQContract-Interfaces-IRecievedMessage{``0}},System-Action{System-Exception},System-String,System-String,System-Boolean,MQContract-Interfaces-Service-IServiceChannelOptions,System-Threading-CancellationToken- 'MQContract.ContractConnection.SubscribeAsync``1(System.Action{MQContract.Interfaces.IRecievedMessage{``0}},System.Action{System.Exception},System.String,System.String,System.Boolean,MQContract.Interfaces.Service.IServiceChannelOptions,System.Threading.CancellationToken)')
  - [SubscribeQueryAsyncResponseAsync\`\`2(messageRecieved,errorRecieved,channel,group,ignoreMessageHeader,options,cancellationToken)](#M-MQContract-ContractConnection-SubscribeQueryAsyncResponseAsync``2-System-Func{MQContract-Interfaces-IRecievedMessage{``0},System-Threading-Tasks-ValueTask{MQContract-Messages-QueryResponseMessage{``1}}},System-Action{System-Exception},System-String,System-String,System-Boolean,MQContract-Interfaces-Service-IServiceChannelOptions,System-Threading-CancellationToken- 'MQContract.ContractConnection.SubscribeQueryAsyncResponseAsync``2(System.Func{MQContract.Interfaces.IRecievedMessage{``0},System.Threading.Tasks.ValueTask{MQContract.Messages.QueryResponseMessage{``1}}},System.Action{System.Exception},System.String,System.String,System.Boolean,MQContract.Interfaces.Service.IServiceChannelOptions,System.Threading.CancellationToken)')
  - [SubscribeQueryResponseAsync\`\`2(messageRecieved,errorRecieved,channel,group,ignoreMessageHeader,options,cancellationToken)](#M-MQContract-ContractConnection-SubscribeQueryResponseAsync``2-System-Func{MQContract-Interfaces-IRecievedMessage{``0},MQContract-Messages-QueryResponseMessage{``1}},System-Action{System-Exception},System-String,System-String,System-Boolean,MQContract-Interfaces-Service-IServiceChannelOptions,System-Threading-CancellationToken- 'MQContract.ContractConnection.SubscribeQueryResponseAsync``2(System.Func{MQContract.Interfaces.IRecievedMessage{``0},MQContract.Messages.QueryResponseMessage{``1}},System.Action{System.Exception},System.String,System.String,System.Boolean,MQContract.Interfaces.Service.IServiceChannelOptions,System.Threading.CancellationToken)')
- [MessageChannelNullException](#T-MQContract-MessageChannelNullException 'MQContract.MessageChannelNullException')
- [MessageConversionException](#T-MQContract-MessageConversionException 'MQContract.MessageConversionException')
- [QueryResponseException](#T-MQContract-QueryResponseException 'MQContract.QueryResponseException')
- [SubscriptionFailedException](#T-MQContract-SubscriptionFailedException 'MQContract.SubscriptionFailedException')
- [UnknownResponseTypeException](#T-MQContract-UnknownResponseTypeException 'MQContract.UnknownResponseTypeException')

<a name='T-MQContract-ChannelMapper'></a>
## ChannelMapper `type`

##### Namespace

MQContract

##### Summary

Used to map channel names depending on the usage of the channel when necessary

<a name='M-MQContract-ChannelMapper-AddDefaultPublishMap-System-Func{System-String,System-Threading-Tasks-ValueTask{System-String}}-'></a>
### AddDefaultPublishMap(mapFunction) `method`

##### Summary

Add a default map function to call for publish calls

##### Returns

The current instance of the Channel Mapper

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| mapFunction | [System.Func{System.String,System.Threading.Tasks.ValueTask{System.String}}](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Func 'System.Func{System.String,System.Threading.Tasks.ValueTask{System.String}}') | A function to be called with the channel supplied expecting a mapped channel name |

<a name='M-MQContract-ChannelMapper-AddDefaultPublishSubscriptionMap-System-Func{System-String,System-Threading-Tasks-ValueTask{System-String}}-'></a>
### AddDefaultPublishSubscriptionMap(mapFunction) `method`

##### Summary

Add a default map function to call for pub/sub subscription calls

##### Returns

The current instance of the Channel Mapper

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| mapFunction | [System.Func{System.String,System.Threading.Tasks.ValueTask{System.String}}](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Func 'System.Func{System.String,System.Threading.Tasks.ValueTask{System.String}}') | A function to be called with the channel supplied expecting a mapped channel name |

<a name='M-MQContract-ChannelMapper-AddDefaultQueryMap-System-Func{System-String,System-Threading-Tasks-ValueTask{System-String}}-'></a>
### AddDefaultQueryMap(mapFunction) `method`

##### Summary

Add a default map function to call for query calls

##### Returns

The current instance of the Channel Mapper

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| mapFunction | [System.Func{System.String,System.Threading.Tasks.ValueTask{System.String}}](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Func 'System.Func{System.String,System.Threading.Tasks.ValueTask{System.String}}') | A function to be called with the channel supplied expecting a mapped channel name |

<a name='M-MQContract-ChannelMapper-AddDefaultQuerySubscriptionMap-System-Func{System-String,System-Threading-Tasks-ValueTask{System-String}}-'></a>
### AddDefaultQuerySubscriptionMap(mapFunction) `method`

##### Summary

Add a default map function to call for query/response subscription calls

##### Returns

The current instance of the Channel Mapper

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| mapFunction | [System.Func{System.String,System.Threading.Tasks.ValueTask{System.String}}](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Func 'System.Func{System.String,System.Threading.Tasks.ValueTask{System.String}}') | A function to be called with the channel supplied expecting a mapped channel name |

<a name='M-MQContract-ChannelMapper-AddPublishMap-System-String,System-String-'></a>
### AddPublishMap(originalChannel,newChannel) `method`

##### Summary

Add a direct map for publish calls

##### Returns

The current instance of the Channel Mapper

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| originalChannel | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | The original channel that is being used in the connection |
| newChannel | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | The channel to map it to |

<a name='M-MQContract-ChannelMapper-AddPublishMap-System-String,System-Func{System-String,System-Threading-Tasks-ValueTask{System-String}}-'></a>
### AddPublishMap(originalChannel,mapFunction) `method`

##### Summary

Add a map function for publish calls for a given channel

##### Returns

The current instance of the Channel Mapper

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| originalChannel | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | The original channel that is being used in the connection |
| mapFunction | [System.Func{System.String,System.Threading.Tasks.ValueTask{System.String}}](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Func 'System.Func{System.String,System.Threading.Tasks.ValueTask{System.String}}') | A function to be called with the channel supplied expecting a mapped channel name |

<a name='M-MQContract-ChannelMapper-AddPublishMap-System-Func{System-String,System-Boolean},System-Func{System-String,System-Threading-Tasks-ValueTask{System-String}}-'></a>
### AddPublishMap(isMatch,mapFunction) `method`

##### Summary

Add a map function call pair for publish calls

##### Returns

The current instance of the Channel Mapper

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| isMatch | [System.Func{System.String,System.Boolean}](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Func 'System.Func{System.String,System.Boolean}') | A callback that will return true if the supplied function will mape that channel |
| mapFunction | [System.Func{System.String,System.Threading.Tasks.ValueTask{System.String}}](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Func 'System.Func{System.String,System.Threading.Tasks.ValueTask{System.String}}') | A function to be called with the channel supplied expecting a mapped channel name |

<a name='M-MQContract-ChannelMapper-AddPublishSubscriptionMap-System-String,System-String-'></a>
### AddPublishSubscriptionMap(originalChannel,newChannel) `method`

##### Summary

Add a direct map for pub/sub subscription calls

##### Returns

The current instance of the Channel Mapper

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| originalChannel | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | The original channel that is being used in the connection |
| newChannel | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | The channel to map it to |

<a name='M-MQContract-ChannelMapper-AddPublishSubscriptionMap-System-String,System-Func{System-String,System-Threading-Tasks-ValueTask{System-String}}-'></a>
### AddPublishSubscriptionMap(originalChannel,mapFunction) `method`

##### Summary

Add a map function for pub/sub subscription calls

##### Returns

The current instance of the Channel Mapper

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| originalChannel | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | The original channel that is being used in the connection |
| mapFunction | [System.Func{System.String,System.Threading.Tasks.ValueTask{System.String}}](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Func 'System.Func{System.String,System.Threading.Tasks.ValueTask{System.String}}') | A function to be called with the channel supplied expecting a mapped channel name |

<a name='M-MQContract-ChannelMapper-AddPublishSubscriptionMap-System-Func{System-String,System-Boolean},System-Func{System-String,System-Threading-Tasks-ValueTask{System-String}}-'></a>
### AddPublishSubscriptionMap(isMatch,mapFunction) `method`

##### Summary

Add a map function call pair for pub/sub subscription calls

##### Returns

The current instance of the Channel Mapper

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| isMatch | [System.Func{System.String,System.Boolean}](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Func 'System.Func{System.String,System.Boolean}') | A callback that will return true if the supplied function will mape that channel |
| mapFunction | [System.Func{System.String,System.Threading.Tasks.ValueTask{System.String}}](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Func 'System.Func{System.String,System.Threading.Tasks.ValueTask{System.String}}') | A function to be called with the channel supplied expecting a mapped channel name |

<a name='M-MQContract-ChannelMapper-AddQueryMap-System-String,System-String-'></a>
### AddQueryMap(originalChannel,newChannel) `method`

##### Summary

Add a direct map for query calls

##### Returns

The current instance of the Channel Mapper

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| originalChannel | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | The original channel that is being used in the connection |
| newChannel | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | The channel to map it to |

<a name='M-MQContract-ChannelMapper-AddQueryMap-System-String,System-Func{System-String,System-Threading-Tasks-ValueTask{System-String}}-'></a>
### AddQueryMap(originalChannel,mapFunction) `method`

##### Summary

Add a map function for query calls for a given channel

##### Returns

The current instance of the Channel Mapper

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| originalChannel | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | The original channel that is being used in the connection |
| mapFunction | [System.Func{System.String,System.Threading.Tasks.ValueTask{System.String}}](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Func 'System.Func{System.String,System.Threading.Tasks.ValueTask{System.String}}') | A function to be called with the channel supplied expecting a mapped channel name |

<a name='M-MQContract-ChannelMapper-AddQueryMap-System-Func{System-String,System-Boolean},System-Func{System-String,System-Threading-Tasks-ValueTask{System-String}}-'></a>
### AddQueryMap(isMatch,mapFunction) `method`

##### Summary

Add a map function call pair for query calls

##### Returns

The current instance of the Channel Mapper

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| isMatch | [System.Func{System.String,System.Boolean}](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Func 'System.Func{System.String,System.Boolean}') | A callback that will return true if the supplied function will mape that channel |
| mapFunction | [System.Func{System.String,System.Threading.Tasks.ValueTask{System.String}}](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Func 'System.Func{System.String,System.Threading.Tasks.ValueTask{System.String}}') | A function to be called with the channel supplied expecting a mapped channel name |

<a name='M-MQContract-ChannelMapper-AddQuerySubscriptionMap-System-String,System-String-'></a>
### AddQuerySubscriptionMap(originalChannel,newChannel) `method`

##### Summary

Add a direct map for query/response subscription calls

##### Returns

The current instance of the Channel Mapper

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| originalChannel | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | The original channel that is being used in the connection |
| newChannel | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | The channel to map it to |

<a name='M-MQContract-ChannelMapper-AddQuerySubscriptionMap-System-String,System-Func{System-String,System-Threading-Tasks-ValueTask{System-String}}-'></a>
### AddQuerySubscriptionMap(originalChannel,mapFunction) `method`

##### Summary

Add a map function for query/response subscription calls

##### Returns

The current instance of the Channel Mapper

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| originalChannel | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | The original channel that is being used in the connection |
| mapFunction | [System.Func{System.String,System.Threading.Tasks.ValueTask{System.String}}](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Func 'System.Func{System.String,System.Threading.Tasks.ValueTask{System.String}}') | A function to be called with the channel supplied expecting a mapped channel name |

<a name='M-MQContract-ChannelMapper-AddQuerySubscriptionMap-System-Func{System-String,System-Boolean},System-Func{System-String,System-Threading-Tasks-ValueTask{System-String}}-'></a>
### AddQuerySubscriptionMap(isMatch,mapFunction) `method`

##### Summary

Add a map function call pair for query/response subscription calls

##### Returns

The current instance of the Channel Mapper

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| isMatch | [System.Func{System.String,System.Boolean}](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Func 'System.Func{System.String,System.Boolean}') | A callback that will return true if the supplied function will mape that channel |
| mapFunction | [System.Func{System.String,System.Threading.Tasks.ValueTask{System.String}}](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Func 'System.Func{System.String,System.Threading.Tasks.ValueTask{System.String}}') | A function to be called with the channel supplied expecting a mapped channel name |

<a name='T-MQContract-ContractConnection'></a>
## ContractConnection `type`

##### Namespace

MQContract

##### Summary

This is the primary class for this library and is used to create a Contract style connection between systems using the underlying service connection layer.

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| serviceConnection | [T:MQContract.ContractConnection](#T-T-MQContract-ContractConnection 'T:MQContract.ContractConnection') | The service connection implementation to use for the underlying message requests. |

<a name='M-MQContract-ContractConnection-#ctor-MQContract-Interfaces-Service-IMessageServiceConnection,MQContract-Interfaces-Encoding-IMessageEncoder,MQContract-Interfaces-Encrypting-IMessageEncryptor,System-IServiceProvider,Microsoft-Extensions-Logging-ILogger,MQContract-ChannelMapper-'></a>
### #ctor(serviceConnection,defaultMessageEncoder,defaultMessageEncryptor,serviceProvider,logger,channelMapper) `constructor`

##### Summary

This is the primary class for this library and is used to create a Contract style connection between systems using the underlying service connection layer.

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| serviceConnection | [MQContract.Interfaces.Service.IMessageServiceConnection](#T-MQContract-Interfaces-Service-IMessageServiceConnection 'MQContract.Interfaces.Service.IMessageServiceConnection') | The service connection implementation to use for the underlying message requests. |
| defaultMessageEncoder | [MQContract.Interfaces.Encoding.IMessageEncoder](#T-MQContract-Interfaces-Encoding-IMessageEncoder 'MQContract.Interfaces.Encoding.IMessageEncoder') | A default message encoder implementation if desired.  If there is no specific encoder for a given type, this encoder would be called.  The built in default being used dotnet Json serializer. |
| defaultMessageEncryptor | [MQContract.Interfaces.Encrypting.IMessageEncryptor](#T-MQContract-Interfaces-Encrypting-IMessageEncryptor 'MQContract.Interfaces.Encrypting.IMessageEncryptor') | A default message encryptor implementation if desired.  If there is no specific encryptor |
| serviceProvider | [System.IServiceProvider](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.IServiceProvider 'System.IServiceProvider') | A service prodivder instance supplied in the case that dependency injection might be necessary |
| logger | [Microsoft.Extensions.Logging.ILogger](#T-Microsoft-Extensions-Logging-ILogger 'Microsoft.Extensions.Logging.ILogger') | An instance of a logger if logging is desired |
| channelMapper | [MQContract.ChannelMapper](#T-MQContract-ChannelMapper 'MQContract.ChannelMapper') | An instance of a ChannelMapper used to translate channels from one instance to another based on class channel attributes or supplied channels if necessary.
For example, it might be necessary for a Nats.IO instance when you are trying to read from a stored message stream that is comprised of another channel or set of channels |

<a name='M-MQContract-ContractConnection-CloseAsync'></a>
### CloseAsync() `method`

##### Summary

Called to close off this connection and it's underlying service connection

##### Returns



##### Parameters

This method has no parameters.

<a name='M-MQContract-ContractConnection-Dispose'></a>
### Dispose() `method`

##### Summary

Called to dispose of the resources contained within

##### Parameters

This method has no parameters.

<a name='M-MQContract-ContractConnection-DisposeAsync'></a>
### DisposeAsync() `method`

##### Summary

Called to dispose of the resources contained within

##### Returns

A task of the underlying resources being disposed

##### Parameters

This method has no parameters.

<a name='M-MQContract-ContractConnection-PingAsync'></a>
### PingAsync() `method`

##### Summary

Called to execute a ping against the service layer

##### Returns

The ping result from the service layer, if supported

##### Parameters

This method has no parameters.

<a name='M-MQContract-ContractConnection-PublishAsync``1-``0,System-String,MQContract-Messages-MessageHeader,MQContract-Interfaces-Service-IServiceChannelOptions,System-Threading-CancellationToken-'></a>
### PublishAsync\`\`1(message,channel,messageHeader,options,cancellationToken) `method`

##### Summary

Called to publish a message out into the service layer in the Pub/Sub style

##### Returns

An instance of the TransmissionResult record to indicate success or failure and an ID

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| message | [\`\`0](#T-``0 '``0') | The instance of the message to publish |
| channel | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | Used to override the MessageChannelAttribute from the class or to specify a channel to transmit the message on |
| messageHeader | [MQContract.Messages.MessageHeader](#T-MQContract-Messages-MessageHeader 'MQContract.Messages.MessageHeader') | A message header to be sent across with the message |
| options | [MQContract.Interfaces.Service.IServiceChannelOptions](#T-MQContract-Interfaces-Service-IServiceChannelOptions 'MQContract.Interfaces.Service.IServiceChannelOptions') | An instance of a ServiceChannelOptions to pass down to the service layer if desired and/or necessary |
| cancellationToken | [System.Threading.CancellationToken](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Threading.CancellationToken 'System.Threading.CancellationToken') | A cancellation token |

##### Generic Types

| Name | Description |
| ---- | ----------- |
| T | The type of message to publish |

<a name='M-MQContract-ContractConnection-QueryAsync``1-``0,System-Nullable{System-TimeSpan},System-String,MQContract-Messages-MessageHeader,MQContract-Interfaces-Service-IServiceChannelOptions,System-Threading-CancellationToken-'></a>
### QueryAsync\`\`1(message,timeout,channel,messageHeader,options,cancellationToken) `method`

##### Summary

Called to publish a message in the Query/Response style except the response Type is gathered from the QueryResponseTypeAttribute

##### Returns

A QueryResult that will contain the response message and or an error

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| message | [\`\`0](#T-``0 '``0') | The message to transmit for the query |
| timeout | [System.Nullable{System.TimeSpan}](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Nullable 'System.Nullable{System.TimeSpan}') | The timeout to allow for waiting for a response |
| channel | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | Used to override the MessageChannelAttribute from the class or to specify a channel to transmit the message on |
| messageHeader | [MQContract.Messages.MessageHeader](#T-MQContract-Messages-MessageHeader 'MQContract.Messages.MessageHeader') | A message header to be sent across with the message |
| options | [MQContract.Interfaces.Service.IServiceChannelOptions](#T-MQContract-Interfaces-Service-IServiceChannelOptions 'MQContract.Interfaces.Service.IServiceChannelOptions') | An instance of a ServiceChannelOptions to pass down to the service layer if desired and/or necessary |
| cancellationToken | [System.Threading.CancellationToken](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Threading.CancellationToken 'System.Threading.CancellationToken') | A cancellation token |

##### Generic Types

| Name | Description |
| ---- | ----------- |
| Q | The type of message to transmit for the Query |

##### Exceptions

| Name | Description |
| ---- | ----------- |
| [MQContract.UnknownResponseTypeException](#T-MQContract-UnknownResponseTypeException 'MQContract.UnknownResponseTypeException') | Thrown when the supplied Query type does not have a QueryResponseTypeAttribute and therefore a response type cannot be determined |

<a name='M-MQContract-ContractConnection-QueryAsync``2-``0,System-Nullable{System-TimeSpan},System-String,MQContract-Messages-MessageHeader,MQContract-Interfaces-Service-IServiceChannelOptions,System-Threading-CancellationToken-'></a>
### QueryAsync\`\`2(message,timeout,channel,messageHeader,options,cancellationToken) `method`

##### Summary

Called to publish a message in the Query/Response style

##### Returns

A QueryResult that will contain the response message and or an error

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| message | [\`\`0](#T-``0 '``0') | The message to transmit for the query |
| timeout | [System.Nullable{System.TimeSpan}](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Nullable 'System.Nullable{System.TimeSpan}') | The timeout to allow for waiting for a response |
| channel | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | Used to override the MessageChannelAttribute from the class or to specify a channel to transmit the message on |
| messageHeader | [MQContract.Messages.MessageHeader](#T-MQContract-Messages-MessageHeader 'MQContract.Messages.MessageHeader') | A message header to be sent across with the message |
| options | [MQContract.Interfaces.Service.IServiceChannelOptions](#T-MQContract-Interfaces-Service-IServiceChannelOptions 'MQContract.Interfaces.Service.IServiceChannelOptions') | An instance of a ServiceChannelOptions to pass down to the service layer if desired and/or necessary |
| cancellationToken | [System.Threading.CancellationToken](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Threading.CancellationToken 'System.Threading.CancellationToken') | A cancellation token |

##### Generic Types

| Name | Description |
| ---- | ----------- |
| Q | The type of message to transmit for the Query |
| R | The type of message expected as a response |

<a name='M-MQContract-ContractConnection-SubscribeAsync``1-System-Func{MQContract-Interfaces-IRecievedMessage{``0},System-Threading-Tasks-ValueTask},System-Action{System-Exception},System-String,System-String,System-Boolean,MQContract-Interfaces-Service-IServiceChannelOptions,System-Threading-CancellationToken-'></a>
### SubscribeAsync\`\`1(messageRecieved,errorRecieved,channel,group,ignoreMessageHeader,options,cancellationToken) `method`

##### Summary

Called to establish a Subscription in the sevice layer for the Pub/Sub style messaging processing messages asynchronously

##### Returns

An instance of the Subscription that can be held or called to end

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| messageRecieved | [System.Func{MQContract.Interfaces.IRecievedMessage{\`\`0},System.Threading.Tasks.ValueTask}](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Func 'System.Func{MQContract.Interfaces.IRecievedMessage{``0},System.Threading.Tasks.ValueTask}') | The callback to be executed when a message is recieved |
| errorRecieved | [System.Action{System.Exception}](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Action 'System.Action{System.Exception}') | The callback to be executed when an error occurs |
| channel | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | Used to override the MessageChannelAttribute from the class or to specify a channel to listen for messages on |
| group | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | Used to specify a group to associate to at the service layer (refer to groups in KubeMQ, Nats.IO, etc) |
| ignoreMessageHeader | [System.Boolean](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Boolean 'System.Boolean') | If set to true this will cause the subscription to ignore the message type specified and assume that the type of message is of type T |
| options | [MQContract.Interfaces.Service.IServiceChannelOptions](#T-MQContract-Interfaces-Service-IServiceChannelOptions 'MQContract.Interfaces.Service.IServiceChannelOptions') | An instance of a ServiceChannelOptions to pass down to the service layer if desired and/or necessary |
| cancellationToken | [System.Threading.CancellationToken](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Threading.CancellationToken 'System.Threading.CancellationToken') | A cancellation token |

##### Generic Types

| Name | Description |
| ---- | ----------- |
| T | The type of message to listen for |

##### Exceptions

| Name | Description |
| ---- | ----------- |
| [MQContract.SubscriptionFailedException](#T-MQContract-SubscriptionFailedException 'MQContract.SubscriptionFailedException') | An exception thrown when the subscription has failed to establish |

<a name='M-MQContract-ContractConnection-SubscribeAsync``1-System-Action{MQContract-Interfaces-IRecievedMessage{``0}},System-Action{System-Exception},System-String,System-String,System-Boolean,MQContract-Interfaces-Service-IServiceChannelOptions,System-Threading-CancellationToken-'></a>
### SubscribeAsync\`\`1(messageRecieved,errorRecieved,channel,group,ignoreMessageHeader,options,cancellationToken) `method`

##### Summary

Called to establish a Subscription in the sevice layer for the Pub/Sub style messaging processing messages synchronously

##### Returns

An instance of the Subscription that can be held or called to end

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| messageRecieved | [System.Action{MQContract.Interfaces.IRecievedMessage{\`\`0}}](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Action 'System.Action{MQContract.Interfaces.IRecievedMessage{``0}}') | The callback to be executed when a message is recieved |
| errorRecieved | [System.Action{System.Exception}](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Action 'System.Action{System.Exception}') | The callback to be executed when an error occurs |
| channel | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | Used to override the MessageChannelAttribute from the class or to specify a channel to listen for messages on |
| group | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | Used to specify a group to associate to at the service layer (refer to groups in KubeMQ, Nats.IO, etc) |
| ignoreMessageHeader | [System.Boolean](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Boolean 'System.Boolean') | If set to true this will cause the subscription to ignore the message type specified and assume that the type of message is of type T |
| options | [MQContract.Interfaces.Service.IServiceChannelOptions](#T-MQContract-Interfaces-Service-IServiceChannelOptions 'MQContract.Interfaces.Service.IServiceChannelOptions') | An instance of a ServiceChannelOptions to pass down to the service layer if desired and/or necessary |
| cancellationToken | [System.Threading.CancellationToken](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Threading.CancellationToken 'System.Threading.CancellationToken') | A cancellation token |

##### Generic Types

| Name | Description |
| ---- | ----------- |
| T | The type of message to listen for |

##### Exceptions

| Name | Description |
| ---- | ----------- |
| [MQContract.SubscriptionFailedException](#T-MQContract-SubscriptionFailedException 'MQContract.SubscriptionFailedException') | An exception thrown when the subscription has failed to establish |

<a name='M-MQContract-ContractConnection-SubscribeQueryAsyncResponseAsync``2-System-Func{MQContract-Interfaces-IRecievedMessage{``0},System-Threading-Tasks-ValueTask{MQContract-Messages-QueryResponseMessage{``1}}},System-Action{System-Exception},System-String,System-String,System-Boolean,MQContract-Interfaces-Service-IServiceChannelOptions,System-Threading-CancellationToken-'></a>
### SubscribeQueryAsyncResponseAsync\`\`2(messageRecieved,errorRecieved,channel,group,ignoreMessageHeader,options,cancellationToken) `method`

##### Summary

Creates a subscription with the underlying service layer for the Query/Response style processing messages asynchronously

##### Returns

An instance of the Subscription that can be held or called to end

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| messageRecieved | [System.Func{MQContract.Interfaces.IRecievedMessage{\`\`0},System.Threading.Tasks.ValueTask{MQContract.Messages.QueryResponseMessage{\`\`1}}}](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Func 'System.Func{MQContract.Interfaces.IRecievedMessage{``0},System.Threading.Tasks.ValueTask{MQContract.Messages.QueryResponseMessage{``1}}}') | The callback to be executed when a message is recieved and expects a returned response |
| errorRecieved | [System.Action{System.Exception}](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Action 'System.Action{System.Exception}') | The callback to be executed when an error occurs |
| channel | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | Used to override the MessageChannelAttribute from the class or to specify a channel to listen for messages on |
| group | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | Used to specify a group to associate to at the service layer (refer to groups in KubeMQ, Nats.IO, etc) |
| ignoreMessageHeader | [System.Boolean](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Boolean 'System.Boolean') | If set to true this will cause the subscription to ignore the message type specified and assume that the type of message is of type T |
| options | [MQContract.Interfaces.Service.IServiceChannelOptions](#T-MQContract-Interfaces-Service-IServiceChannelOptions 'MQContract.Interfaces.Service.IServiceChannelOptions') | An instance of a ServiceChannelOptions to pass down to the service layer if desired and/or necessary |
| cancellationToken | [System.Threading.CancellationToken](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Threading.CancellationToken 'System.Threading.CancellationToken') | A cancellation token |

##### Generic Types

| Name | Description |
| ---- | ----------- |
| Q | The expected message type for the Query |
| R | The expected message type for the Response |

##### Exceptions

| Name | Description |
| ---- | ----------- |
| [MQContract.SubscriptionFailedException](#T-MQContract-SubscriptionFailedException 'MQContract.SubscriptionFailedException') | An exception thrown when the subscription has failed to establish |

<a name='M-MQContract-ContractConnection-SubscribeQueryResponseAsync``2-System-Func{MQContract-Interfaces-IRecievedMessage{``0},MQContract-Messages-QueryResponseMessage{``1}},System-Action{System-Exception},System-String,System-String,System-Boolean,MQContract-Interfaces-Service-IServiceChannelOptions,System-Threading-CancellationToken-'></a>
### SubscribeQueryResponseAsync\`\`2(messageRecieved,errorRecieved,channel,group,ignoreMessageHeader,options,cancellationToken) `method`

##### Summary

Creates a subscription with the underlying service layer for the Query/Response style processing messages synchronously

##### Returns

An instance of the Subscription that can be held or called to end

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| messageRecieved | [System.Func{MQContract.Interfaces.IRecievedMessage{\`\`0},MQContract.Messages.QueryResponseMessage{\`\`1}}](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Func 'System.Func{MQContract.Interfaces.IRecievedMessage{``0},MQContract.Messages.QueryResponseMessage{``1}}') | The callback to be executed when a message is recieved and expects a returned response |
| errorRecieved | [System.Action{System.Exception}](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Action 'System.Action{System.Exception}') | The callback to be executed when an error occurs |
| channel | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | Used to override the MessageChannelAttribute from the class or to specify a channel to listen for messages on |
| group | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | Used to specify a group to associate to at the service layer (refer to groups in KubeMQ, Nats.IO, etc) |
| ignoreMessageHeader | [System.Boolean](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Boolean 'System.Boolean') | If set to true this will cause the subscription to ignore the message type specified and assume that the type of message is of type T |
| options | [MQContract.Interfaces.Service.IServiceChannelOptions](#T-MQContract-Interfaces-Service-IServiceChannelOptions 'MQContract.Interfaces.Service.IServiceChannelOptions') | An instance of a ServiceChannelOptions to pass down to the service layer if desired and/or necessary |
| cancellationToken | [System.Threading.CancellationToken](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Threading.CancellationToken 'System.Threading.CancellationToken') | A cancellation token |

##### Generic Types

| Name | Description |
| ---- | ----------- |
| Q | The expected message type for the Query |
| R | The expected message type for the Response |

##### Exceptions

| Name | Description |
| ---- | ----------- |
| [MQContract.SubscriptionFailedException](#T-MQContract-SubscriptionFailedException 'MQContract.SubscriptionFailedException') | An exception thrown when the subscription has failed to establish |

<a name='T-MQContract-MessageChannelNullException'></a>
## MessageChannelNullException `type`

##### Namespace

MQContract

##### Summary

Thrown when a call is made but the system is unable to detect the channel

<a name='T-MQContract-MessageConversionException'></a>
## MessageConversionException `type`

##### Namespace

MQContract

##### Summary

Thrown when an incoming data message causes a null object return from a converter

<a name='T-MQContract-QueryResponseException'></a>
## QueryResponseException `type`

##### Namespace

MQContract

##### Summary

Thrown when a Query call is made and there is an error in the response

<a name='T-MQContract-SubscriptionFailedException'></a>
## SubscriptionFailedException `type`

##### Namespace

MQContract

##### Summary

Thrown when a Subscription has failed to be established/created

<a name='T-MQContract-UnknownResponseTypeException'></a>
## UnknownResponseTypeException `type`

##### Namespace

MQContract

##### Summary

Thrown when a QueryResponse type message is attempted without specifying the response type and there is no Response Type attribute for the query class.

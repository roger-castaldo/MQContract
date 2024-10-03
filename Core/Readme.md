<a name='assembly'></a>
# MQContract

## Contents

- [ChannelMapper](#T-MQContract-ChannelMapper 'MQContract.ChannelMapper')
  - [AddDefaultPublishMap(mapFunction)](#M-MQContract-ChannelMapper-AddDefaultPublishMap-System-Func{System-String,System-Threading-Tasks-ValueTask{System-String}}- 'MQContract.ChannelMapper.AddDefaultPublishMap(System.Func{System.String,System.Threading.Tasks.ValueTask{System.String}})')
  - [AddDefaultPublishSubscriptionMap(mapFunction)](#M-MQContract-ChannelMapper-AddDefaultPublishSubscriptionMap-System-Func{System-String,System-Threading-Tasks-ValueTask{System-String}}- 'MQContract.ChannelMapper.AddDefaultPublishSubscriptionMap(System.Func{System.String,System.Threading.Tasks.ValueTask{System.String}})')
  - [AddDefaultQueryMap(mapFunction)](#M-MQContract-ChannelMapper-AddDefaultQueryMap-System-Func{System-String,System-Threading-Tasks-ValueTask{System-String}}- 'MQContract.ChannelMapper.AddDefaultQueryMap(System.Func{System.String,System.Threading.Tasks.ValueTask{System.String}})')
  - [AddDefaultQueryResponseMap(mapFunction)](#M-MQContract-ChannelMapper-AddDefaultQueryResponseMap-System-Func{System-String,System-Threading-Tasks-ValueTask{System-String}}- 'MQContract.ChannelMapper.AddDefaultQueryResponseMap(System.Func{System.String,System.Threading.Tasks.ValueTask{System.String}})')
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
  - [AddQueryResponseMap(originalChannel,newChannel)](#M-MQContract-ChannelMapper-AddQueryResponseMap-System-String,System-String- 'MQContract.ChannelMapper.AddQueryResponseMap(System.String,System.String)')
  - [AddQueryResponseMap(originalChannel,mapFunction)](#M-MQContract-ChannelMapper-AddQueryResponseMap-System-String,System-Func{System-String,System-Threading-Tasks-ValueTask{System-String}}- 'MQContract.ChannelMapper.AddQueryResponseMap(System.String,System.Func{System.String,System.Threading.Tasks.ValueTask{System.String}})')
  - [AddQueryResponseMap(isMatch,mapFunction)](#M-MQContract-ChannelMapper-AddQueryResponseMap-System-Func{System-String,System-Boolean},System-Func{System-String,System-Threading-Tasks-ValueTask{System-String}}- 'MQContract.ChannelMapper.AddQueryResponseMap(System.Func{System.String,System.Boolean},System.Func{System.String,System.Threading.Tasks.ValueTask{System.String}})')
  - [AddQuerySubscriptionMap(originalChannel,newChannel)](#M-MQContract-ChannelMapper-AddQuerySubscriptionMap-System-String,System-String- 'MQContract.ChannelMapper.AddQuerySubscriptionMap(System.String,System.String)')
  - [AddQuerySubscriptionMap(originalChannel,mapFunction)](#M-MQContract-ChannelMapper-AddQuerySubscriptionMap-System-String,System-Func{System-String,System-Threading-Tasks-ValueTask{System-String}}- 'MQContract.ChannelMapper.AddQuerySubscriptionMap(System.String,System.Func{System.String,System.Threading.Tasks.ValueTask{System.String}})')
  - [AddQuerySubscriptionMap(isMatch,mapFunction)](#M-MQContract-ChannelMapper-AddQuerySubscriptionMap-System-Func{System-String,System-Boolean},System-Func{System-String,System-Threading-Tasks-ValueTask{System-String}}- 'MQContract.ChannelMapper.AddQuerySubscriptionMap(System.Func{System.String,System.Boolean},System.Func{System.String,System.Threading.Tasks.ValueTask{System.String}})')
- [ContractConnection](#T-MQContract-ContractConnection 'MQContract.ContractConnection')
  - [Instance(serviceConnection,defaultMessageEncoder,defaultMessageEncryptor,serviceProvider,logger,channelMapper)](#M-MQContract-ContractConnection-Instance-MQContract-Interfaces-Service-IMessageServiceConnection,MQContract-Interfaces-Encoding-IMessageEncoder,MQContract-Interfaces-Encrypting-IMessageEncryptor,System-IServiceProvider,Microsoft-Extensions-Logging-ILogger,MQContract-ChannelMapper- 'MQContract.ContractConnection.Instance(MQContract.Interfaces.Service.IMessageServiceConnection,MQContract.Interfaces.Encoding.IMessageEncoder,MQContract.Interfaces.Encrypting.IMessageEncryptor,System.IServiceProvider,Microsoft.Extensions.Logging.ILogger,MQContract.ChannelMapper)')
- [InvalidQueryResponseMessageReceived](#T-MQContract-InvalidQueryResponseMessageReceived 'MQContract.InvalidQueryResponseMessageReceived')
- [MessageChannelNullException](#T-MQContract-MessageChannelNullException 'MQContract.MessageChannelNullException')
- [MessageConversionException](#T-MQContract-MessageConversionException 'MQContract.MessageConversionException')
- [QueryExecutionFailedException](#T-MQContract-QueryExecutionFailedException 'MQContract.QueryExecutionFailedException')
- [QueryResponseException](#T-MQContract-QueryResponseException 'MQContract.QueryResponseException')
- [QuerySubmissionFailedException](#T-MQContract-QuerySubmissionFailedException 'MQContract.QuerySubmissionFailedException')
- [QueryTimeoutException](#T-MQContract-QueryTimeoutException 'MQContract.QueryTimeoutException')
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

<a name='M-MQContract-ChannelMapper-AddDefaultQueryResponseMap-System-Func{System-String,System-Threading-Tasks-ValueTask{System-String}}-'></a>
### AddDefaultQueryResponseMap(mapFunction) `method`

##### Summary

Add a default map function to call for query/response response calls

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

<a name='M-MQContract-ChannelMapper-AddQueryResponseMap-System-String,System-String-'></a>
### AddQueryResponseMap(originalChannel,newChannel) `method`

##### Summary

Add a direct map for query/response response calls

##### Returns

The current instance of the Channel Mapper

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| originalChannel | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | The original channel that is being used in the connection |
| newChannel | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | The channel to map it to |

<a name='M-MQContract-ChannelMapper-AddQueryResponseMap-System-String,System-Func{System-String,System-Threading-Tasks-ValueTask{System-String}}-'></a>
### AddQueryResponseMap(originalChannel,mapFunction) `method`

##### Summary

Add a map function for query/response response calls

##### Returns

The current instance of the Channel Mapper

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| originalChannel | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | The original channel that is being used in the connection |
| mapFunction | [System.Func{System.String,System.Threading.Tasks.ValueTask{System.String}}](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Func 'System.Func{System.String,System.Threading.Tasks.ValueTask{System.String}}') | A function to be called with the channel supplied expecting a mapped channel name |

<a name='M-MQContract-ChannelMapper-AddQueryResponseMap-System-Func{System-String,System-Boolean},System-Func{System-String,System-Threading-Tasks-ValueTask{System-String}}-'></a>
### AddQueryResponseMap(isMatch,mapFunction) `method`

##### Summary

Add a map function call pair for query/response response calls

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

The primary ContractConnection item which implements IContractConnection

<a name='M-MQContract-ContractConnection-Instance-MQContract-Interfaces-Service-IMessageServiceConnection,MQContract-Interfaces-Encoding-IMessageEncoder,MQContract-Interfaces-Encrypting-IMessageEncryptor,System-IServiceProvider,Microsoft-Extensions-Logging-ILogger,MQContract-ChannelMapper-'></a>
### Instance(serviceConnection,defaultMessageEncoder,defaultMessageEncryptor,serviceProvider,logger,channelMapper) `method`

##### Summary

This is the call used to create an instance of a Contract Connection which will return the Interface

##### Returns

An instance of IContractConnection

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

<a name='T-MQContract-InvalidQueryResponseMessageReceived'></a>
## InvalidQueryResponseMessageReceived `type`

##### Namespace

MQContract

##### Summary

Thrown when a query call message is received without proper data

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

<a name='T-MQContract-QueryExecutionFailedException'></a>
## QueryExecutionFailedException `type`

##### Namespace

MQContract

##### Summary

Thrown when a query call is being made to a service that does not support query response and the listener cannot be created

<a name='T-MQContract-QueryResponseException'></a>
## QueryResponseException `type`

##### Namespace

MQContract

##### Summary

Thrown when a Query call is made and there is an error in the response

<a name='T-MQContract-QuerySubmissionFailedException'></a>
## QuerySubmissionFailedException `type`

##### Namespace

MQContract

##### Summary

Thrown when a query call is being made to an inbox style service and the message fails to transmit

<a name='T-MQContract-QueryTimeoutException'></a>
## QueryTimeoutException `type`

##### Namespace

MQContract

##### Summary

Thrown when a query call times out waiting for the response

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

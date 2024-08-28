<a name='assembly'></a>
# MQContract.Kafka

## Contents

- [Connection](#T-MQContract-Kafka-Connection 'MQContract.Kafka.Connection')
  - [#ctor(clientConfig)](#M-MQContract-Kafka-Connection-#ctor-Confluent-Kafka-ClientConfig- 'MQContract.Kafka.Connection.#ctor(Confluent.Kafka.ClientConfig)')
  - [DefaultTimout](#P-MQContract-Kafka-Connection-DefaultTimout 'MQContract.Kafka.Connection.DefaultTimout')
  - [MaxMessageBodySize](#P-MQContract-Kafka-Connection-MaxMessageBodySize 'MQContract.Kafka.Connection.MaxMessageBodySize')
  - [CloseAsync()](#M-MQContract-Kafka-Connection-CloseAsync 'MQContract.Kafka.Connection.CloseAsync')
  - [Dispose()](#M-MQContract-Kafka-Connection-Dispose 'MQContract.Kafka.Connection.Dispose')
  - [DisposeAsync()](#M-MQContract-Kafka-Connection-DisposeAsync 'MQContract.Kafka.Connection.DisposeAsync')
  - [PingAsync()](#M-MQContract-Kafka-Connection-PingAsync 'MQContract.Kafka.Connection.PingAsync')
  - [PublishAsync(message,options,cancellationToken)](#M-MQContract-Kafka-Connection-PublishAsync-MQContract-Messages-ServiceMessage,MQContract-Interfaces-Service-IServiceChannelOptions,System-Threading-CancellationToken- 'MQContract.Kafka.Connection.PublishAsync(MQContract.Messages.ServiceMessage,MQContract.Interfaces.Service.IServiceChannelOptions,System.Threading.CancellationToken)')
  - [QueryAsync(message,timeout,options,cancellationToken)](#M-MQContract-Kafka-Connection-QueryAsync-MQContract-Messages-ServiceMessage,System-TimeSpan,MQContract-Interfaces-Service-IServiceChannelOptions,System-Threading-CancellationToken- 'MQContract.Kafka.Connection.QueryAsync(MQContract.Messages.ServiceMessage,System.TimeSpan,MQContract.Interfaces.Service.IServiceChannelOptions,System.Threading.CancellationToken)')
  - [SubscribeAsync(messageRecieved,errorRecieved,channel,group,options,cancellationToken)](#M-MQContract-Kafka-Connection-SubscribeAsync-System-Action{MQContract-Messages-RecievedServiceMessage},System-Action{System-Exception},System-String,System-String,MQContract-Interfaces-Service-IServiceChannelOptions,System-Threading-CancellationToken- 'MQContract.Kafka.Connection.SubscribeAsync(System.Action{MQContract.Messages.RecievedServiceMessage},System.Action{System.Exception},System.String,System.String,MQContract.Interfaces.Service.IServiceChannelOptions,System.Threading.CancellationToken)')
  - [SubscribeQueryAsync(messageRecieved,errorRecieved,channel,group,options,cancellationToken)](#M-MQContract-Kafka-Connection-SubscribeQueryAsync-System-Func{MQContract-Messages-RecievedServiceMessage,System-Threading-Tasks-ValueTask{MQContract-Messages-ServiceMessage}},System-Action{System-Exception},System-String,System-String,MQContract-Interfaces-Service-IServiceChannelOptions,System-Threading-CancellationToken- 'MQContract.Kafka.Connection.SubscribeQueryAsync(System.Func{MQContract.Messages.RecievedServiceMessage,System.Threading.Tasks.ValueTask{MQContract.Messages.ServiceMessage}},System.Action{System.Exception},System.String,System.String,MQContract.Interfaces.Service.IServiceChannelOptions,System.Threading.CancellationToken)')
- [QueryChannelOptions](#T-MQContract-Kafka-Options-QueryChannelOptions 'MQContract.Kafka.Options.QueryChannelOptions')
  - [#ctor(ReplyChannel)](#M-MQContract-Kafka-Options-QueryChannelOptions-#ctor-System-String- 'MQContract.Kafka.Options.QueryChannelOptions.#ctor(System.String)')
  - [ReplyChannel](#P-MQContract-Kafka-Options-QueryChannelOptions-ReplyChannel 'MQContract.Kafka.Options.QueryChannelOptions.ReplyChannel')

<a name='T-MQContract-Kafka-Connection'></a>
## Connection `type`

##### Namespace

MQContract.Kafka

##### Summary

This is the MessageServiceConnection implementation for using Kafka

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| clientConfig | [T:MQContract.Kafka.Connection](#T-T-MQContract-Kafka-Connection 'T:MQContract.Kafka.Connection') |  |

<a name='M-MQContract-Kafka-Connection-#ctor-Confluent-Kafka-ClientConfig-'></a>
### #ctor(clientConfig) `constructor`

##### Summary

This is the MessageServiceConnection implementation for using Kafka

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| clientConfig | [Confluent.Kafka.ClientConfig](#T-Confluent-Kafka-ClientConfig 'Confluent.Kafka.ClientConfig') |  |

<a name='P-MQContract-Kafka-Connection-DefaultTimout'></a>
### DefaultTimout `property`

##### Summary

The default timeout to use for RPC calls when not specified by the class or in the call.
DEFAULT:1 minute if not specified inside the connection options

<a name='P-MQContract-Kafka-Connection-MaxMessageBodySize'></a>
### MaxMessageBodySize `property`

##### Summary

The maximum message body size allowed

<a name='M-MQContract-Kafka-Connection-CloseAsync'></a>
### CloseAsync() `method`

##### Summary

Called to close off the underlying Kafka Connection

##### Returns



##### Parameters

This method has no parameters.

<a name='M-MQContract-Kafka-Connection-Dispose'></a>
### Dispose() `method`

##### Summary

Called to dispose of the required resources

##### Parameters

This method has no parameters.

<a name='M-MQContract-Kafka-Connection-DisposeAsync'></a>
### DisposeAsync() `method`

##### Summary

Called to dispose of the object correctly and allow it to clean up it's resources

##### Returns

A task required for disposal

##### Parameters

This method has no parameters.

<a name='M-MQContract-Kafka-Connection-PingAsync'></a>
### PingAsync() `method`

##### Summary

Not implemented as Kafka does not support this particular action

##### Returns

Throws NotImplementedException

##### Parameters

This method has no parameters.

##### Exceptions

| Name | Description |
| ---- | ----------- |
| [System.NotImplementedException](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.NotImplementedException 'System.NotImplementedException') | Thrown because Kafka does not support this particular action |

<a name='M-MQContract-Kafka-Connection-PublishAsync-MQContract-Messages-ServiceMessage,MQContract-Interfaces-Service-IServiceChannelOptions,System-Threading-CancellationToken-'></a>
### PublishAsync(message,options,cancellationToken) `method`

##### Summary

Called to publish a message into the Kafka server

##### Returns

Transmition result identifying if it worked or not

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| message | [MQContract.Messages.ServiceMessage](#T-MQContract-Messages-ServiceMessage 'MQContract.Messages.ServiceMessage') | The service message being sent |
| options | [MQContract.Interfaces.Service.IServiceChannelOptions](#T-MQContract-Interfaces-Service-IServiceChannelOptions 'MQContract.Interfaces.Service.IServiceChannelOptions') | The service channel options, if desired, specifically the PublishChannelOptions which is used to access the storage capabilities of KubeMQ |
| cancellationToken | [System.Threading.CancellationToken](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Threading.CancellationToken 'System.Threading.CancellationToken') | A cancellation token |

##### Exceptions

| Name | Description |
| ---- | ----------- |
| [MQContract.NoChannelOptionsAvailableException](#T-MQContract-NoChannelOptionsAvailableException 'MQContract.NoChannelOptionsAvailableException') | Thrown if options was supplied because there are no implemented options for this call |

<a name='M-MQContract-Kafka-Connection-QueryAsync-MQContract-Messages-ServiceMessage,System-TimeSpan,MQContract-Interfaces-Service-IServiceChannelOptions,System-Threading-CancellationToken-'></a>
### QueryAsync(message,timeout,options,cancellationToken) `method`

##### Summary

Called to publish a query into the Kafka server

##### Returns

The resulting response

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| message | [MQContract.Messages.ServiceMessage](#T-MQContract-Messages-ServiceMessage 'MQContract.Messages.ServiceMessage') | The service message being sent |
| timeout | [System.TimeSpan](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.TimeSpan 'System.TimeSpan') | The timeout supplied for the query to response |
| options | [MQContract.Interfaces.Service.IServiceChannelOptions](#T-MQContract-Interfaces-Service-IServiceChannelOptions 'MQContract.Interfaces.Service.IServiceChannelOptions') | The options specifically for this call and must be supplied.  Must be instance of QueryChannelOptions. |
| cancellationToken | [System.Threading.CancellationToken](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Threading.CancellationToken 'System.Threading.CancellationToken') | A cancellation token |

##### Exceptions

| Name | Description |
| ---- | ----------- |
| [System.ArgumentNullException](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.ArgumentNullException 'System.ArgumentNullException') | Thrown if options is null |
| [MQContract.InvalidChannelOptionsTypeException](#T-MQContract-InvalidChannelOptionsTypeException 'MQContract.InvalidChannelOptionsTypeException') | Thrown if the options that was supplied is not an instance of QueryChannelOptions |
| [System.ArgumentNullException](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.ArgumentNullException 'System.ArgumentNullException') | Thrown if the ReplyChannel is blank or null as it needs to be set |
| [MQContract.Kafka.QueryExecutionFailedException](#T-MQContract-Kafka-QueryExecutionFailedException 'MQContract.Kafka.QueryExecutionFailedException') | Thrown when the query fails to execute |
| [MQContract.Kafka.QueryAsyncReponseException](#T-MQContract-Kafka-QueryAsyncReponseException 'MQContract.Kafka.QueryAsyncReponseException') | Thrown when the responding instance has provided an error |
| [MQContract.Kafka.QueryResultMissingException](#T-MQContract-Kafka-QueryResultMissingException 'MQContract.Kafka.QueryResultMissingException') | Thrown when there is no response to be found for the query |

<a name='M-MQContract-Kafka-Connection-SubscribeAsync-System-Action{MQContract-Messages-RecievedServiceMessage},System-Action{System-Exception},System-String,System-String,MQContract-Interfaces-Service-IServiceChannelOptions,System-Threading-CancellationToken-'></a>
### SubscribeAsync(messageRecieved,errorRecieved,channel,group,options,cancellationToken) `method`

##### Summary

Called to create a subscription to the underlying Kafka server

##### Returns



##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| messageRecieved | [System.Action{MQContract.Messages.RecievedServiceMessage}](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Action 'System.Action{MQContract.Messages.RecievedServiceMessage}') | Callback for when a message is recieved |
| errorRecieved | [System.Action{System.Exception}](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Action 'System.Action{System.Exception}') | Callback for when an error occurs |
| channel | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | The name of the channel to bind to |
| group | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | The group to subscribe as part of |
| options | [MQContract.Interfaces.Service.IServiceChannelOptions](#T-MQContract-Interfaces-Service-IServiceChannelOptions 'MQContract.Interfaces.Service.IServiceChannelOptions') |  |
| cancellationToken | [System.Threading.CancellationToken](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Threading.CancellationToken 'System.Threading.CancellationToken') |  |

##### Exceptions

| Name | Description |
| ---- | ----------- |
| [MQContract.NoChannelOptionsAvailableException](#T-MQContract-NoChannelOptionsAvailableException 'MQContract.NoChannelOptionsAvailableException') | Thrown if options was supplied because there are no implemented options for this call |

<a name='M-MQContract-Kafka-Connection-SubscribeQueryAsync-System-Func{MQContract-Messages-RecievedServiceMessage,System-Threading-Tasks-ValueTask{MQContract-Messages-ServiceMessage}},System-Action{System-Exception},System-String,System-String,MQContract-Interfaces-Service-IServiceChannelOptions,System-Threading-CancellationToken-'></a>
### SubscribeQueryAsync(messageRecieved,errorRecieved,channel,group,options,cancellationToken) `method`

##### Summary

Called to create a subscription for queries to the underlying Kafka server

##### Returns

A subscription instance

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| messageRecieved | [System.Func{MQContract.Messages.RecievedServiceMessage,System.Threading.Tasks.ValueTask{MQContract.Messages.ServiceMessage}}](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Func 'System.Func{MQContract.Messages.RecievedServiceMessage,System.Threading.Tasks.ValueTask{MQContract.Messages.ServiceMessage}}') | Callback for when a query is recieved |
| errorRecieved | [System.Action{System.Exception}](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Action 'System.Action{System.Exception}') | Callback for when an error occurs |
| channel | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | The name of the channel to bind to |
| group | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | The group to subscribe as part of |
| options | [MQContract.Interfaces.Service.IServiceChannelOptions](#T-MQContract-Interfaces-Service-IServiceChannelOptions 'MQContract.Interfaces.Service.IServiceChannelOptions') | Optional QueryChannelOptions to be supplied that will specify the ReplyChannel if not supplied by query message |
| cancellationToken | [System.Threading.CancellationToken](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Threading.CancellationToken 'System.Threading.CancellationToken') | A cancellation token |

##### Exceptions

| Name | Description |
| ---- | ----------- |
| [MQContract.InvalidChannelOptionsTypeException](#T-MQContract-InvalidChannelOptionsTypeException 'MQContract.InvalidChannelOptionsTypeException') | Thrown when options is not null and is not an instance of the type QueryChannelOptions |

<a name='T-MQContract-Kafka-Options-QueryChannelOptions'></a>
## QueryChannelOptions `type`

##### Namespace

MQContract.Kafka.Options

##### Summary

Houses the QueryChannelOptions used for performing Query Response style messaging in Kafka

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| ReplyChannel | [T:MQContract.Kafka.Options.QueryChannelOptions](#T-T-MQContract-Kafka-Options-QueryChannelOptions 'T:MQContract.Kafka.Options.QueryChannelOptions') | The reply channel to use.  This channel should be setup with a short retention policy, no longer than 5 minutes. |

<a name='M-MQContract-Kafka-Options-QueryChannelOptions-#ctor-System-String-'></a>
### #ctor(ReplyChannel) `constructor`

##### Summary

Houses the QueryChannelOptions used for performing Query Response style messaging in Kafka

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| ReplyChannel | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | The reply channel to use.  This channel should be setup with a short retention policy, no longer than 5 minutes. |

<a name='P-MQContract-Kafka-Options-QueryChannelOptions-ReplyChannel'></a>
### ReplyChannel `property`

##### Summary

The reply channel to use.  This channel should be setup with a short retention policy, no longer than 5 minutes.

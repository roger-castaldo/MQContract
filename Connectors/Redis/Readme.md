<a name='assembly'></a>
# MQContract.Redis

## Contents

- [Connection](#T-MQContract-Redis-Connection 'MQContract.Redis.Connection')
  - [#ctor(configuration)](#M-MQContract-Redis-Connection-#ctor-StackExchange-Redis-ConfigurationOptions- 'MQContract.Redis.Connection.#ctor(StackExchange.Redis.ConfigurationOptions)')
  - [DefaultTimout](#P-MQContract-Redis-Connection-DefaultTimout 'MQContract.Redis.Connection.DefaultTimout')
  - [MaxMessageBodySize](#P-MQContract-Redis-Connection-MaxMessageBodySize 'MQContract.Redis.Connection.MaxMessageBodySize')
  - [CloseAsync()](#M-MQContract-Redis-Connection-CloseAsync 'MQContract.Redis.Connection.CloseAsync')
  - [DefineConsumerGroupAsync(channel,group)](#M-MQContract-Redis-Connection-DefineConsumerGroupAsync-System-String,System-String- 'MQContract.Redis.Connection.DefineConsumerGroupAsync(System.String,System.String)')
  - [Dispose()](#M-MQContract-Redis-Connection-Dispose 'MQContract.Redis.Connection.Dispose')
  - [DisposeAsync()](#M-MQContract-Redis-Connection-DisposeAsync 'MQContract.Redis.Connection.DisposeAsync')
  - [PublishAsync(message,cancellationToken)](#M-MQContract-Redis-Connection-PublishAsync-MQContract-Messages-ServiceMessage,System-Threading-CancellationToken- 'MQContract.Redis.Connection.PublishAsync(MQContract.Messages.ServiceMessage,System.Threading.CancellationToken)')
  - [QueryAsync(message,timeout,cancellationToken)](#M-MQContract-Redis-Connection-QueryAsync-MQContract-Messages-ServiceMessage,System-TimeSpan,System-Threading-CancellationToken- 'MQContract.Redis.Connection.QueryAsync(MQContract.Messages.ServiceMessage,System.TimeSpan,System.Threading.CancellationToken)')
  - [SubscribeAsync(messageRecieved,errorRecieved,channel,group,cancellationToken)](#M-MQContract-Redis-Connection-SubscribeAsync-System-Action{MQContract-Messages-RecievedServiceMessage},System-Action{System-Exception},System-String,System-String,System-Threading-CancellationToken- 'MQContract.Redis.Connection.SubscribeAsync(System.Action{MQContract.Messages.RecievedServiceMessage},System.Action{System.Exception},System.String,System.String,System.Threading.CancellationToken)')
  - [SubscribeQueryAsync(messageRecieved,errorRecieved,channel,group,cancellationToken)](#M-MQContract-Redis-Connection-SubscribeQueryAsync-System-Func{MQContract-Messages-RecievedServiceMessage,System-Threading-Tasks-ValueTask{MQContract-Messages-ServiceMessage}},System-Action{System-Exception},System-String,System-String,System-Threading-CancellationToken- 'MQContract.Redis.Connection.SubscribeQueryAsync(System.Func{MQContract.Messages.RecievedServiceMessage,System.Threading.Tasks.ValueTask{MQContract.Messages.ServiceMessage}},System.Action{System.Exception},System.String,System.String,System.Threading.CancellationToken)')

<a name='T-MQContract-Redis-Connection'></a>
## Connection `type`

##### Namespace

MQContract.Redis

##### Summary

This is the MessageServiceConnection implementation for using Redis

<a name='M-MQContract-Redis-Connection-#ctor-StackExchange-Redis-ConfigurationOptions-'></a>
### #ctor(configuration) `constructor`

##### Summary

Default constructor that requires the Redis Configuration settings to be provided

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| configuration | [StackExchange.Redis.ConfigurationOptions](#T-StackExchange-Redis-ConfigurationOptions 'StackExchange.Redis.ConfigurationOptions') |  |

<a name='P-MQContract-Redis-Connection-DefaultTimout'></a>
### DefaultTimout `property`

##### Summary

The default timeout to allow for a Query Response call to execute, defaults to 1 minute

<a name='P-MQContract-Redis-Connection-MaxMessageBodySize'></a>
### MaxMessageBodySize `property`

##### Summary

The maximum message body size allowed, defaults to 4MB

<a name='M-MQContract-Redis-Connection-CloseAsync'></a>
### CloseAsync() `method`

##### Summary

Called to close off the underlying Redis Connection

##### Returns



##### Parameters

This method has no parameters.

<a name='M-MQContract-Redis-Connection-DefineConsumerGroupAsync-System-String,System-String-'></a>
### DefineConsumerGroupAsync(channel,group) `method`

##### Summary

Called to define a consumer group inside redis for a given channel

##### Returns

A ValueTask while the operation executes asynchronously

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| channel | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | The name of the channel to use |
| group | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | The name of the group to use |

<a name='M-MQContract-Redis-Connection-Dispose'></a>
### Dispose() `method`

##### Summary

Called to dispose of the object correctly and allow it to clean up it's resources

##### Parameters

This method has no parameters.

<a name='M-MQContract-Redis-Connection-DisposeAsync'></a>
### DisposeAsync() `method`

##### Summary

Called to dispose of the object correctly and allow it to clean up it's resources

##### Returns

A task required for disposal

##### Parameters

This method has no parameters.

<a name='M-MQContract-Redis-Connection-PublishAsync-MQContract-Messages-ServiceMessage,System-Threading-CancellationToken-'></a>
### PublishAsync(message,cancellationToken) `method`

##### Summary

Called to publish a message into the Redis server

##### Returns

Transmition result identifying if it worked or not

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| message | [MQContract.Messages.ServiceMessage](#T-MQContract-Messages-ServiceMessage 'MQContract.Messages.ServiceMessage') | The service message being sent |
| cancellationToken | [System.Threading.CancellationToken](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Threading.CancellationToken 'System.Threading.CancellationToken') | A cancellation token |

<a name='M-MQContract-Redis-Connection-QueryAsync-MQContract-Messages-ServiceMessage,System-TimeSpan,System-Threading-CancellationToken-'></a>
### QueryAsync(message,timeout,cancellationToken) `method`

##### Summary

Called to publish a query into the Redis server

##### Returns

The resulting response

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| message | [MQContract.Messages.ServiceMessage](#T-MQContract-Messages-ServiceMessage 'MQContract.Messages.ServiceMessage') | The service message being sent |
| timeout | [System.TimeSpan](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.TimeSpan 'System.TimeSpan') | The timeout supplied for the query to response |
| cancellationToken | [System.Threading.CancellationToken](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Threading.CancellationToken 'System.Threading.CancellationToken') | A cancellation token |

##### Exceptions

| Name | Description |
| ---- | ----------- |
| [System.TimeoutException](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.TimeoutException 'System.TimeoutException') | Thrown when the response times out |

<a name='M-MQContract-Redis-Connection-SubscribeAsync-System-Action{MQContract-Messages-RecievedServiceMessage},System-Action{System-Exception},System-String,System-String,System-Threading-CancellationToken-'></a>
### SubscribeAsync(messageRecieved,errorRecieved,channel,group,cancellationToken) `method`

##### Summary

Called to create a subscription to the underlying Redis server

##### Returns



##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| messageRecieved | [System.Action{MQContract.Messages.RecievedServiceMessage}](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Action 'System.Action{MQContract.Messages.RecievedServiceMessage}') | Callback for when a message is recieved |
| errorRecieved | [System.Action{System.Exception}](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Action 'System.Action{System.Exception}') | Callback for when an error occurs |
| channel | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | The name of the channel to bind to |
| group | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | The name of the group to bind the consumer to |
| cancellationToken | [System.Threading.CancellationToken](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Threading.CancellationToken 'System.Threading.CancellationToken') | A cancellation token |

<a name='M-MQContract-Redis-Connection-SubscribeQueryAsync-System-Func{MQContract-Messages-RecievedServiceMessage,System-Threading-Tasks-ValueTask{MQContract-Messages-ServiceMessage}},System-Action{System-Exception},System-String,System-String,System-Threading-CancellationToken-'></a>
### SubscribeQueryAsync(messageRecieved,errorRecieved,channel,group,cancellationToken) `method`

##### Summary

Called to create a subscription for queries to the underlying Redis server

##### Returns

A subscription instance

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| messageRecieved | [System.Func{MQContract.Messages.RecievedServiceMessage,System.Threading.Tasks.ValueTask{MQContract.Messages.ServiceMessage}}](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Func 'System.Func{MQContract.Messages.RecievedServiceMessage,System.Threading.Tasks.ValueTask{MQContract.Messages.ServiceMessage}}') | Callback for when a query is recieved |
| errorRecieved | [System.Action{System.Exception}](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Action 'System.Action{System.Exception}') | Callback for when an error occurs |
| channel | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | The name of the channel to bind to |
| group | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | The group to bind to |
| cancellationToken | [System.Threading.CancellationToken](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Threading.CancellationToken 'System.Threading.CancellationToken') | A cancellation token |

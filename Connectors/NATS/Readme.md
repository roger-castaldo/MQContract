<a name='assembly'></a>
# MQContract.NATS

## Contents

- [Connection](#T-MQContract-NATS-Connection 'MQContract.NATS.Connection')
  - [#ctor(options)](#M-MQContract-NATS-Connection-#ctor-NATS-Client-Core-NatsOpts- 'MQContract.NATS.Connection.#ctor(NATS.Client.Core.NatsOpts)')
  - [DefaultTimout](#P-MQContract-NATS-Connection-DefaultTimout 'MQContract.NATS.Connection.DefaultTimout')
  - [MaxMessageBodySize](#P-MQContract-NATS-Connection-MaxMessageBodySize 'MQContract.NATS.Connection.MaxMessageBodySize')
  - [CloseAsync()](#M-MQContract-NATS-Connection-CloseAsync 'MQContract.NATS.Connection.CloseAsync')
  - [CreateStreamAsync(streamConfig,cancellationToken)](#M-MQContract-NATS-Connection-CreateStreamAsync-NATS-Client-JetStream-Models-StreamConfig,System-Threading-CancellationToken- 'MQContract.NATS.Connection.CreateStreamAsync(NATS.Client.JetStream.Models.StreamConfig,System.Threading.CancellationToken)')
  - [Dispose()](#M-MQContract-NATS-Connection-Dispose 'MQContract.NATS.Connection.Dispose')
  - [DisposeAsync()](#M-MQContract-NATS-Connection-DisposeAsync 'MQContract.NATS.Connection.DisposeAsync')
  - [PingAsync()](#M-MQContract-NATS-Connection-PingAsync 'MQContract.NATS.Connection.PingAsync')
  - [PublishAsync(message,cancellationToken)](#M-MQContract-NATS-Connection-PublishAsync-MQContract-Messages-ServiceMessage,System-Threading-CancellationToken- 'MQContract.NATS.Connection.PublishAsync(MQContract.Messages.ServiceMessage,System.Threading.CancellationToken)')
  - [QueryAsync(message,timeout,cancellationToken)](#M-MQContract-NATS-Connection-QueryAsync-MQContract-Messages-ServiceMessage,System-TimeSpan,System-Threading-CancellationToken- 'MQContract.NATS.Connection.QueryAsync(MQContract.Messages.ServiceMessage,System.TimeSpan,System.Threading.CancellationToken)')
  - [RegisterConsumerConfig(channelName,consumerConfig)](#M-MQContract-NATS-Connection-RegisterConsumerConfig-System-String,NATS-Client-JetStream-Models-ConsumerConfig- 'MQContract.NATS.Connection.RegisterConsumerConfig(System.String,NATS.Client.JetStream.Models.ConsumerConfig)')
  - [SubscribeAsync(messageRecieved,errorRecieved,channel,group,cancellationToken)](#M-MQContract-NATS-Connection-SubscribeAsync-System-Action{MQContract-Messages-RecievedServiceMessage},System-Action{System-Exception},System-String,System-String,System-Threading-CancellationToken- 'MQContract.NATS.Connection.SubscribeAsync(System.Action{MQContract.Messages.RecievedServiceMessage},System.Action{System.Exception},System.String,System.String,System.Threading.CancellationToken)')
  - [SubscribeQueryAsync(messageRecieved,errorRecieved,channel,group,cancellationToken)](#M-MQContract-NATS-Connection-SubscribeQueryAsync-System-Func{MQContract-Messages-RecievedServiceMessage,System-Threading-Tasks-ValueTask{MQContract-Messages-ServiceMessage}},System-Action{System-Exception},System-String,System-String,System-Threading-CancellationToken- 'MQContract.NATS.Connection.SubscribeQueryAsync(System.Func{MQContract.Messages.RecievedServiceMessage,System.Threading.Tasks.ValueTask{MQContract.Messages.ServiceMessage}},System.Action{System.Exception},System.String,System.String,System.Threading.CancellationToken)')
- [UnableToConnectException](#T-MQContract-NATS-UnableToConnectException 'MQContract.NATS.UnableToConnectException')

<a name='T-MQContract-NATS-Connection'></a>
## Connection `type`

##### Namespace

MQContract.NATS

##### Summary

This is the MessageServiceConnection implementation for using NATS.io

<a name='M-MQContract-NATS-Connection-#ctor-NATS-Client-Core-NatsOpts-'></a>
### #ctor(options) `constructor`

##### Summary

Primary constructor to create an instance using the supplied configuration options.

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| options | [NATS.Client.Core.NatsOpts](#T-NATS-Client-Core-NatsOpts 'NATS.Client.Core.NatsOpts') |  |

<a name='P-MQContract-NATS-Connection-DefaultTimout'></a>
### DefaultTimout `property`

##### Summary

The default timeout to use for RPC calls when not specified by class or in the call.
DEFAULT: 30 seconds

<a name='P-MQContract-NATS-Connection-MaxMessageBodySize'></a>
### MaxMessageBodySize `property`

##### Summary

The maximum message body size allowed.
DEFAULT: 1MB

<a name='M-MQContract-NATS-Connection-CloseAsync'></a>
### CloseAsync() `method`

##### Summary

Called to close off the contract connection and close it's underlying service connection

##### Returns

A task for the closure of the connection

##### Parameters

This method has no parameters.

<a name='M-MQContract-NATS-Connection-CreateStreamAsync-NATS-Client-JetStream-Models-StreamConfig,System-Threading-CancellationToken-'></a>
### CreateStreamAsync(streamConfig,cancellationToken) `method`

##### Summary

Called to define a Stream inside the underlying NATS context.  This is an exposure of the NatsJSContext.CreateStreamAsync

##### Returns

The stream creation result

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| streamConfig | [NATS.Client.JetStream.Models.StreamConfig](#T-NATS-Client-JetStream-Models-StreamConfig 'NATS.Client.JetStream.Models.StreamConfig') | The configuration settings for the stream |
| cancellationToken | [System.Threading.CancellationToken](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Threading.CancellationToken 'System.Threading.CancellationToken') | A cancellation token |

<a name='M-MQContract-NATS-Connection-Dispose'></a>
### Dispose() `method`

##### Summary

Called to dispose of the object correctly and allow it to clean up it's resources

##### Parameters

This method has no parameters.

<a name='M-MQContract-NATS-Connection-DisposeAsync'></a>
### DisposeAsync() `method`

##### Summary

Called to dispose of the object correctly and allow it to clean up it's resources

##### Returns

A task required for disposal

##### Parameters

This method has no parameters.

<a name='M-MQContract-NATS-Connection-PingAsync'></a>
### PingAsync() `method`

##### Summary

Called to ping the NATS.io service

##### Returns

The Ping Result including service information

##### Parameters

This method has no parameters.

<a name='M-MQContract-NATS-Connection-PublishAsync-MQContract-Messages-ServiceMessage,System-Threading-CancellationToken-'></a>
### PublishAsync(message,cancellationToken) `method`

##### Summary

Called to publish a message into the NATS io server

##### Returns

Transmition result identifying if it worked or not

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| message | [MQContract.Messages.ServiceMessage](#T-MQContract-Messages-ServiceMessage 'MQContract.Messages.ServiceMessage') | The service message being sent |
| cancellationToken | [System.Threading.CancellationToken](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Threading.CancellationToken 'System.Threading.CancellationToken') | A cancellation token |

<a name='M-MQContract-NATS-Connection-QueryAsync-MQContract-Messages-ServiceMessage,System-TimeSpan,System-Threading-CancellationToken-'></a>
### QueryAsync(message,timeout,cancellationToken) `method`

##### Summary

Called to publish a query into the NATS io server

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
| [MQContract.NATS.QueryAsyncReponseException](#T-MQContract-NATS-QueryAsyncReponseException 'MQContract.NATS.QueryAsyncReponseException') | Thrown when an error comes from the responding service |

<a name='M-MQContract-NATS-Connection-RegisterConsumerConfig-System-String,NATS-Client-JetStream-Models-ConsumerConfig-'></a>
### RegisterConsumerConfig(channelName,consumerConfig) `method`

##### Summary

Called to register a consumer configuration for a given channel.  This is only used for stream channels and allows for configuring
storing and reading patterns

##### Returns

The underlying connection to allow for chaining

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| channelName | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | The underlying stream name that this configuration applies to |
| consumerConfig | [NATS.Client.JetStream.Models.ConsumerConfig](#T-NATS-Client-JetStream-Models-ConsumerConfig 'NATS.Client.JetStream.Models.ConsumerConfig') | The consumer configuration to use for that stream |

<a name='M-MQContract-NATS-Connection-SubscribeAsync-System-Action{MQContract-Messages-RecievedServiceMessage},System-Action{System-Exception},System-String,System-String,System-Threading-CancellationToken-'></a>
### SubscribeAsync(messageRecieved,errorRecieved,channel,group,cancellationToken) `method`

##### Summary

Called to create a subscription to the underlying nats server

##### Returns

A subscription instance

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| messageRecieved | [System.Action{MQContract.Messages.RecievedServiceMessage}](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Action 'System.Action{MQContract.Messages.RecievedServiceMessage}') | Callback for when a message is recieved |
| errorRecieved | [System.Action{System.Exception}](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Action 'System.Action{System.Exception}') | Callback for when an error occurs |
| channel | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | The name of the channel to bind to |
| group | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | The name of the group to bind the consumer to |
| cancellationToken | [System.Threading.CancellationToken](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Threading.CancellationToken 'System.Threading.CancellationToken') | A cancellation token |

<a name='M-MQContract-NATS-Connection-SubscribeQueryAsync-System-Func{MQContract-Messages-RecievedServiceMessage,System-Threading-Tasks-ValueTask{MQContract-Messages-ServiceMessage}},System-Action{System-Exception},System-String,System-String,System-Threading-CancellationToken-'></a>
### SubscribeQueryAsync(messageRecieved,errorRecieved,channel,group,cancellationToken) `method`

##### Summary

Called to create a subscription for queries to the underlying NATS server

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

<a name='T-MQContract-NATS-UnableToConnectException'></a>
## UnableToConnectException `type`

##### Namespace

MQContract.NATS

##### Summary

Thrown when an error occurs attempting to connect to the NATS server.  
Specifically this will be thrown when the Ping that is executed on each initial connection fails.

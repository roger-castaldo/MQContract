<a name='assembly'></a>
# MQContract.NATS

## Contents

- [Connection](#T-MQContract-NATS-Connection 'MQContract.NATS.Connection')
  - [#ctor(options)](#M-MQContract-NATS-Connection-#ctor-NATS-Client-Core-NatsOpts- 'MQContract.NATS.Connection.#ctor(NATS.Client.Core.NatsOpts)')
  - [DefaultTimout](#P-MQContract-NATS-Connection-DefaultTimout 'MQContract.NATS.Connection.DefaultTimout')
  - [MaxMessageBodySize](#P-MQContract-NATS-Connection-MaxMessageBodySize 'MQContract.NATS.Connection.MaxMessageBodySize')
  - [CreateStreamAsync(streamConfig,cancellationToken)](#M-MQContract-NATS-Connection-CreateStreamAsync-NATS-Client-JetStream-Models-StreamConfig,System-Threading-CancellationToken- 'MQContract.NATS.Connection.CreateStreamAsync(NATS.Client.JetStream.Models.StreamConfig,System.Threading.CancellationToken)')
  - [PingAsync()](#M-MQContract-NATS-Connection-PingAsync 'MQContract.NATS.Connection.PingAsync')
  - [PublishAsync(message,options,cancellationToken)](#M-MQContract-NATS-Connection-PublishAsync-MQContract-Messages-ServiceMessage,MQContract-Interfaces-Service-IServiceChannelOptions,System-Threading-CancellationToken- 'MQContract.NATS.Connection.PublishAsync(MQContract.Messages.ServiceMessage,MQContract.Interfaces.Service.IServiceChannelOptions,System.Threading.CancellationToken)')
  - [QueryAsync(message,timeout,options,cancellationToken)](#M-MQContract-NATS-Connection-QueryAsync-MQContract-Messages-ServiceMessage,System-TimeSpan,MQContract-Interfaces-Service-IServiceChannelOptions,System-Threading-CancellationToken- 'MQContract.NATS.Connection.QueryAsync(MQContract.Messages.ServiceMessage,System.TimeSpan,MQContract.Interfaces.Service.IServiceChannelOptions,System.Threading.CancellationToken)')
  - [SubscribeAsync(messageRecieved,errorRecieved,channel,group,options,cancellationToken)](#M-MQContract-NATS-Connection-SubscribeAsync-System-Action{MQContract-Messages-RecievedServiceMessage},System-Action{System-Exception},System-String,System-String,MQContract-Interfaces-Service-IServiceChannelOptions,System-Threading-CancellationToken- 'MQContract.NATS.Connection.SubscribeAsync(System.Action{MQContract.Messages.RecievedServiceMessage},System.Action{System.Exception},System.String,System.String,MQContract.Interfaces.Service.IServiceChannelOptions,System.Threading.CancellationToken)')
  - [SubscribeQueryAsync(messageRecieved,errorRecieved,channel,group,options,cancellationToken)](#M-MQContract-NATS-Connection-SubscribeQueryAsync-System-Func{MQContract-Messages-RecievedServiceMessage,System-Threading-Tasks-ValueTask{MQContract-Messages-ServiceMessage}},System-Action{System-Exception},System-String,System-String,MQContract-Interfaces-Service-IServiceChannelOptions,System-Threading-CancellationToken- 'MQContract.NATS.Connection.SubscribeQueryAsync(System.Func{MQContract.Messages.RecievedServiceMessage,System.Threading.Tasks.ValueTask{MQContract.Messages.ServiceMessage}},System.Action{System.Exception},System.String,System.String,MQContract.Interfaces.Service.IServiceChannelOptions,System.Threading.CancellationToken)')
- [StreamPublishChannelOptions](#T-MQContract-NATS-Options-StreamPublishChannelOptions 'MQContract.NATS.Options.StreamPublishChannelOptions')
  - [#ctor(Config)](#M-MQContract-NATS-Options-StreamPublishChannelOptions-#ctor-NATS-Client-JetStream-Models-StreamConfig- 'MQContract.NATS.Options.StreamPublishChannelOptions.#ctor(NATS.Client.JetStream.Models.StreamConfig)')
  - [Config](#P-MQContract-NATS-Options-StreamPublishChannelOptions-Config 'MQContract.NATS.Options.StreamPublishChannelOptions.Config')
- [StreamPublishSubscriberOptions](#T-MQContract-NATS-Options-StreamPublishSubscriberOptions 'MQContract.NATS.Options.StreamPublishSubscriberOptions')
  - [#ctor(StreamConfig,ConsumerConfig)](#M-MQContract-NATS-Options-StreamPublishSubscriberOptions-#ctor-NATS-Client-JetStream-Models-StreamConfig,NATS-Client-JetStream-Models-ConsumerConfig- 'MQContract.NATS.Options.StreamPublishSubscriberOptions.#ctor(NATS.Client.JetStream.Models.StreamConfig,NATS.Client.JetStream.Models.ConsumerConfig)')
  - [ConsumerConfig](#P-MQContract-NATS-Options-StreamPublishSubscriberOptions-ConsumerConfig 'MQContract.NATS.Options.StreamPublishSubscriberOptions.ConsumerConfig')
  - [StreamConfig](#P-MQContract-NATS-Options-StreamPublishSubscriberOptions-StreamConfig 'MQContract.NATS.Options.StreamPublishSubscriberOptions.StreamConfig')
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

<a name='M-MQContract-NATS-Connection-PingAsync'></a>
### PingAsync() `method`

##### Summary

Called to ping the NATS.io service

##### Returns

The Ping Result including service information

##### Parameters

This method has no parameters.

<a name='M-MQContract-NATS-Connection-PublishAsync-MQContract-Messages-ServiceMessage,MQContract-Interfaces-Service-IServiceChannelOptions,System-Threading-CancellationToken-'></a>
### PublishAsync(message,options,cancellationToken) `method`

##### Summary

Called to publish a message into the NATS io server

##### Returns

Transmition result identifying if it worked or not

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| message | [MQContract.Messages.ServiceMessage](#T-MQContract-Messages-ServiceMessage 'MQContract.Messages.ServiceMessage') | The service message being sent |
| options | [MQContract.Interfaces.Service.IServiceChannelOptions](#T-MQContract-Interfaces-Service-IServiceChannelOptions 'MQContract.Interfaces.Service.IServiceChannelOptions') | The service channel options, if desired, specifically the StreamPublishChannelOptions which is used to access streams vs standard publish method |
| cancellationToken | [System.Threading.CancellationToken](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Threading.CancellationToken 'System.Threading.CancellationToken') | A cancellation token |

##### Exceptions

| Name | Description |
| ---- | ----------- |
| [MQContract.InvalidChannelOptionsTypeException](#T-MQContract-InvalidChannelOptionsTypeException 'MQContract.InvalidChannelOptionsTypeException') | Thrown when an attempt to pass an options object that is not of the type StreamPublishChannelOptions |

<a name='M-MQContract-NATS-Connection-QueryAsync-MQContract-Messages-ServiceMessage,System-TimeSpan,MQContract-Interfaces-Service-IServiceChannelOptions,System-Threading-CancellationToken-'></a>
### QueryAsync(message,timeout,options,cancellationToken) `method`

##### Summary

Called to publish a query into the NATS io server

##### Returns

The resulting response

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| message | [MQContract.Messages.ServiceMessage](#T-MQContract-Messages-ServiceMessage 'MQContract.Messages.ServiceMessage') | The service message being sent |
| timeout | [System.TimeSpan](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.TimeSpan 'System.TimeSpan') | The timeout supplied for the query to response |
| options | [MQContract.Interfaces.Service.IServiceChannelOptions](#T-MQContract-Interfaces-Service-IServiceChannelOptions 'MQContract.Interfaces.Service.IServiceChannelOptions') | Should be null here as there is no Service Channel Options implemented for this call |
| cancellationToken | [System.Threading.CancellationToken](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Threading.CancellationToken 'System.Threading.CancellationToken') | A cancellation token |

##### Exceptions

| Name | Description |
| ---- | ----------- |
| [MQContract.NoChannelOptionsAvailableException](#T-MQContract-NoChannelOptionsAvailableException 'MQContract.NoChannelOptionsAvailableException') | Thrown if options was supplied because there are no implemented options for this call |
| [MQContract.NATS.QueryAsyncReponseException](#T-MQContract-NATS-QueryAsyncReponseException 'MQContract.NATS.QueryAsyncReponseException') | Thrown when an error comes from the responding service |

<a name='M-MQContract-NATS-Connection-SubscribeAsync-System-Action{MQContract-Messages-RecievedServiceMessage},System-Action{System-Exception},System-String,System-String,MQContract-Interfaces-Service-IServiceChannelOptions,System-Threading-CancellationToken-'></a>
### SubscribeAsync(messageRecieved,errorRecieved,channel,group,options,cancellationToken) `method`

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
| group | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | The queueGroup to use for the subscription |
| options | [MQContract.Interfaces.Service.IServiceChannelOptions](#T-MQContract-Interfaces-Service-IServiceChannelOptions 'MQContract.Interfaces.Service.IServiceChannelOptions') | The service channel options, if desired, specifically the StreamPublishSubscriberOptions which is used to access streams vs standard subscription |
| cancellationToken | [System.Threading.CancellationToken](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Threading.CancellationToken 'System.Threading.CancellationToken') | A cancellation token |

##### Exceptions

| Name | Description |
| ---- | ----------- |
| [MQContract.InvalidChannelOptionsTypeException](#T-MQContract-InvalidChannelOptionsTypeException 'MQContract.InvalidChannelOptionsTypeException') | Thrown when options is not null and is not an instance of the type StreamPublishSubscriberOptions |

<a name='M-MQContract-NATS-Connection-SubscribeQueryAsync-System-Func{MQContract-Messages-RecievedServiceMessage,System-Threading-Tasks-ValueTask{MQContract-Messages-ServiceMessage}},System-Action{System-Exception},System-String,System-String,MQContract-Interfaces-Service-IServiceChannelOptions,System-Threading-CancellationToken-'></a>
### SubscribeQueryAsync(messageRecieved,errorRecieved,channel,group,options,cancellationToken) `method`

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
| group | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | The queueGroup to use for the subscription |
| options | [MQContract.Interfaces.Service.IServiceChannelOptions](#T-MQContract-Interfaces-Service-IServiceChannelOptions 'MQContract.Interfaces.Service.IServiceChannelOptions') | Should be null here as there is no Service Channel Options implemented for this call |
| cancellationToken | [System.Threading.CancellationToken](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Threading.CancellationToken 'System.Threading.CancellationToken') | A cancellation token |

##### Exceptions

| Name | Description |
| ---- | ----------- |
| [MQContract.NoChannelOptionsAvailableException](#T-MQContract-NoChannelOptionsAvailableException 'MQContract.NoChannelOptionsAvailableException') | Thrown if options was supplied because there are no implemented options for this call |

<a name='T-MQContract-NATS-Options-StreamPublishChannelOptions'></a>
## StreamPublishChannelOptions `type`

##### Namespace

MQContract.NATS.Options

##### Summary

Used to specify when a publish call is publishing to a JetStream

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| Config | [T:MQContract.NATS.Options.StreamPublishChannelOptions](#T-T-MQContract-NATS-Options-StreamPublishChannelOptions 'T:MQContract.NATS.Options.StreamPublishChannelOptions') | The StreamConfig to use if not already defined |

<a name='M-MQContract-NATS-Options-StreamPublishChannelOptions-#ctor-NATS-Client-JetStream-Models-StreamConfig-'></a>
### #ctor(Config) `constructor`

##### Summary

Used to specify when a publish call is publishing to a JetStream

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| Config | [NATS.Client.JetStream.Models.StreamConfig](#T-NATS-Client-JetStream-Models-StreamConfig 'NATS.Client.JetStream.Models.StreamConfig') | The StreamConfig to use if not already defined |

<a name='P-MQContract-NATS-Options-StreamPublishChannelOptions-Config'></a>
### Config `property`

##### Summary

The StreamConfig to use if not already defined

<a name='T-MQContract-NATS-Options-StreamPublishSubscriberOptions'></a>
## StreamPublishSubscriberOptions `type`

##### Namespace

MQContract.NATS.Options

##### Summary

Used to specify when a subscription call is subscribing to a JetStream and not the standard subscription

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| StreamConfig | [T:MQContract.NATS.Options.StreamPublishSubscriberOptions](#T-T-MQContract-NATS-Options-StreamPublishSubscriberOptions 'T:MQContract.NATS.Options.StreamPublishSubscriberOptions') | The StreamConfig to use if not already defined |

<a name='M-MQContract-NATS-Options-StreamPublishSubscriberOptions-#ctor-NATS-Client-JetStream-Models-StreamConfig,NATS-Client-JetStream-Models-ConsumerConfig-'></a>
### #ctor(StreamConfig,ConsumerConfig) `constructor`

##### Summary

Used to specify when a subscription call is subscribing to a JetStream and not the standard subscription

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| StreamConfig | [NATS.Client.JetStream.Models.StreamConfig](#T-NATS-Client-JetStream-Models-StreamConfig 'NATS.Client.JetStream.Models.StreamConfig') | The StreamConfig to use if not already defined |
| ConsumerConfig | [NATS.Client.JetStream.Models.ConsumerConfig](#T-NATS-Client-JetStream-Models-ConsumerConfig 'NATS.Client.JetStream.Models.ConsumerConfig') | The ConsumerCondig to use if specific settings are required |

<a name='P-MQContract-NATS-Options-StreamPublishSubscriberOptions-ConsumerConfig'></a>
### ConsumerConfig `property`

##### Summary

The ConsumerCondig to use if specific settings are required

<a name='P-MQContract-NATS-Options-StreamPublishSubscriberOptions-StreamConfig'></a>
### StreamConfig `property`

##### Summary

The StreamConfig to use if not already defined

<a name='T-MQContract-NATS-UnableToConnectException'></a>
## UnableToConnectException `type`

##### Namespace

MQContract.NATS

##### Summary

Thrown when an error occurs attempting to connect to the NATS server.  
Specifically this will be thrown when the Ping that is executed on each initial connection fails.

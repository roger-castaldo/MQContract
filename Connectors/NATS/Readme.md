<a name='assembly'></a>
# MQContract.NATS

## Contents

- [Connection](#T-MQContract-NATS-Connection 'MQContract.NATS.Connection')
  - [#ctor(options)](#M-MQContract-NATS-Connection-#ctor-NATS-Client-Core-NatsOpts- 'MQContract.NATS.Connection.#ctor(NATS.Client.Core.NatsOpts)')
  - [DefaultTimeout](#P-MQContract-NATS-Connection-DefaultTimeout 'MQContract.NATS.Connection.DefaultTimeout')
  - [MaxMessageBodySize](#P-MQContract-NATS-Connection-MaxMessageBodySize 'MQContract.NATS.Connection.MaxMessageBodySize')
  - [CreateStreamAsync(streamConfig,cancellationToken)](#M-MQContract-NATS-Connection-CreateStreamAsync-NATS-Client-JetStream-Models-StreamConfig,System-Threading-CancellationToken- 'MQContract.NATS.Connection.CreateStreamAsync(NATS.Client.JetStream.Models.StreamConfig,System.Threading.CancellationToken)')
  - [RegisterConsumerConfig(channelName,consumerConfig)](#M-MQContract-NATS-Connection-RegisterConsumerConfig-System-String,NATS-Client-JetStream-Models-ConsumerConfig- 'MQContract.NATS.Connection.RegisterConsumerConfig(System.String,NATS.Client.JetStream.Models.ConsumerConfig)')
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

<a name='P-MQContract-NATS-Connection-DefaultTimeout'></a>
### DefaultTimeout `property`

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

<a name='T-MQContract-NATS-UnableToConnectException'></a>
## UnableToConnectException `type`

##### Namespace

MQContract.NATS

##### Summary

Thrown when an error occurs attempting to connect to the NATS server.  
Specifically this will be thrown when the Ping that is executed on each initial connection fails.

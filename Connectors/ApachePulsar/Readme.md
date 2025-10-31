<a name='assembly'></a>
# MQContract.ApachePulsar

## Contents

- [Connection](#T-MQContract-ApachePulsar-Connection 'MQContract.ApachePulsar.Connection')
  - [#ctor(pulsarClientBuilder)](#M-MQContract-ApachePulsar-Connection-#ctor-DotPulsar-Abstractions-IPulsarClientBuilder- 'MQContract.ApachePulsar.Connection.#ctor(DotPulsar.Abstractions.IPulsarClientBuilder)')
  - [MaxMessageBodySize](#P-MQContract-ApachePulsar-Connection-MaxMessageBodySize 'MQContract.ApachePulsar.Connection.MaxMessageBodySize')
  - [PulsarClient](#P-MQContract-ApachePulsar-Connection-PulsarClient 'MQContract.ApachePulsar.Connection.PulsarClient')

<a name='T-MQContract-ApachePulsar-Connection'></a>
## Connection `type`

##### Namespace

MQContract.ApachePulsar

##### Summary

This is the MessageServiceConnection implemenation for using ApaxhePulsar

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| pulsarClientBuilder | [T:MQContract.ApachePulsar.Connection](#T-T-MQContract-ApachePulsar-Connection 'T:MQContract.ApachePulsar.Connection') | An instance of a pulsar client builder used to build the underlying client connection |

<a name='M-MQContract-ApachePulsar-Connection-#ctor-DotPulsar-Abstractions-IPulsarClientBuilder-'></a>
### #ctor(pulsarClientBuilder) `constructor`

##### Summary

This is the MessageServiceConnection implemenation for using ApaxhePulsar

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| pulsarClientBuilder | [DotPulsar.Abstractions.IPulsarClientBuilder](#T-DotPulsar-Abstractions-IPulsarClientBuilder 'DotPulsar.Abstractions.IPulsarClientBuilder') | An instance of a pulsar client builder used to build the underlying client connection |

<a name='P-MQContract-ApachePulsar-Connection-MaxMessageBodySize'></a>
### MaxMessageBodySize `property`

##### Summary

Max Message Body Size in bytes, default 5MB

<a name='P-MQContract-ApachePulsar-Connection-PulsarClient'></a>
### PulsarClient `property`

##### Summary

The underlying connection, exposed for external usage

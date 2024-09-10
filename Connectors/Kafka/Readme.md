<a name='assembly'></a>
# MQContract.Kafka

## Contents

- [Connection](#T-MQContract-Kafka-Connection 'MQContract.Kafka.Connection')
  - [#ctor(clientConfig)](#M-MQContract-Kafka-Connection-#ctor-Confluent-Kafka-ClientConfig- 'MQContract.Kafka.Connection.#ctor(Confluent.Kafka.ClientConfig)')
  - [MaxMessageBodySize](#P-MQContract-Kafka-Connection-MaxMessageBodySize 'MQContract.Kafka.Connection.MaxMessageBodySize')
  - [CloseAsync()](#M-MQContract-Kafka-Connection-CloseAsync 'MQContract.Kafka.Connection.CloseAsync')
  - [Dispose()](#M-MQContract-Kafka-Connection-Dispose 'MQContract.Kafka.Connection.Dispose')
  - [DisposeAsync()](#M-MQContract-Kafka-Connection-DisposeAsync 'MQContract.Kafka.Connection.DisposeAsync')
  - [PublishAsync(message,cancellationToken)](#M-MQContract-Kafka-Connection-PublishAsync-MQContract-Messages-ServiceMessage,System-Threading-CancellationToken- 'MQContract.Kafka.Connection.PublishAsync(MQContract.Messages.ServiceMessage,System.Threading.CancellationToken)')
  - [SubscribeAsync(messageRecieved,errorRecieved,channel,group,cancellationToken)](#M-MQContract-Kafka-Connection-SubscribeAsync-System-Action{MQContract-Messages-RecievedServiceMessage},System-Action{System-Exception},System-String,System-String,System-Threading-CancellationToken- 'MQContract.Kafka.Connection.SubscribeAsync(System.Action{MQContract.Messages.RecievedServiceMessage},System.Action{System.Exception},System.String,System.String,System.Threading.CancellationToken)')

<a name='T-MQContract-Kafka-Connection'></a>
## Connection `type`

##### Namespace

MQContract.Kafka

##### Summary

This is the MessageServiceConnection implementation for using Kafka

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| clientConfig | [T:MQContract.Kafka.Connection](#T-T-MQContract-Kafka-Connection 'T:MQContract.Kafka.Connection') | The Kafka Client Configuration to provide |

<a name='M-MQContract-Kafka-Connection-#ctor-Confluent-Kafka-ClientConfig-'></a>
### #ctor(clientConfig) `constructor`

##### Summary

This is the MessageServiceConnection implementation for using Kafka

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| clientConfig | [Confluent.Kafka.ClientConfig](#T-Confluent-Kafka-ClientConfig 'Confluent.Kafka.ClientConfig') | The Kafka Client Configuration to provide |

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

<a name='M-MQContract-Kafka-Connection-PublishAsync-MQContract-Messages-ServiceMessage,System-Threading-CancellationToken-'></a>
### PublishAsync(message,cancellationToken) `method`

##### Summary

Called to publish a message into the Kafka server

##### Returns

Transmition result identifying if it worked or not

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| message | [MQContract.Messages.ServiceMessage](#T-MQContract-Messages-ServiceMessage 'MQContract.Messages.ServiceMessage') | The service message being sent |
| cancellationToken | [System.Threading.CancellationToken](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Threading.CancellationToken 'System.Threading.CancellationToken') | A cancellation token |

<a name='M-MQContract-Kafka-Connection-SubscribeAsync-System-Action{MQContract-Messages-RecievedServiceMessage},System-Action{System-Exception},System-String,System-String,System-Threading-CancellationToken-'></a>
### SubscribeAsync(messageRecieved,errorRecieved,channel,group,cancellationToken) `method`

##### Summary

Called to create a subscription to the underlying Kafka server

##### Returns



##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| messageRecieved | [System.Action{MQContract.Messages.RecievedServiceMessage}](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Action 'System.Action{MQContract.Messages.RecievedServiceMessage}') | Callback for when a message is recieved |
| errorRecieved | [System.Action{System.Exception}](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Action 'System.Action{System.Exception}') | Callback for when an error occurs |
| channel | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | The name of the channel to bind to |
| group | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | The name of the group to bind the consumer to |
| cancellationToken | [System.Threading.CancellationToken](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Threading.CancellationToken 'System.Threading.CancellationToken') | A cancellation token |

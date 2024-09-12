<a name='assembly'></a>
# MQContract.RabbitMQ

## Contents

- [Connection](#T-MQContract-RabbitMQ-Connection 'MQContract.RabbitMQ.Connection')
  - [#ctor(factory)](#M-MQContract-RabbitMQ-Connection-#ctor-RabbitMQ-Client-ConnectionFactory- 'MQContract.RabbitMQ.Connection.#ctor(RabbitMQ.Client.ConnectionFactory)')
  - [MaxMessageBodySize](#P-MQContract-RabbitMQ-Connection-MaxMessageBodySize 'MQContract.RabbitMQ.Connection.MaxMessageBodySize')
  - [PublishAsync(message,cancellationToken)](#M-MQContract-RabbitMQ-Connection-PublishAsync-MQContract-Messages-ServiceMessage,System-Threading-CancellationToken- 'MQContract.RabbitMQ.Connection.PublishAsync(MQContract.Messages.ServiceMessage,System.Threading.CancellationToken)')
  - [SubscribeAsync(messageRecieved,errorRecieved,channel,group,cancellationToken)](#M-MQContract-RabbitMQ-Connection-SubscribeAsync-System-Action{MQContract-Messages-RecievedServiceMessage},System-Action{System-Exception},System-String,System-String,System-Threading-CancellationToken- 'MQContract.RabbitMQ.Connection.SubscribeAsync(System.Action{MQContract.Messages.RecievedServiceMessage},System.Action{System.Exception},System.String,System.String,System.Threading.CancellationToken)')

<a name='T-MQContract-RabbitMQ-Connection'></a>
## Connection `type`

##### Namespace

MQContract.RabbitMQ

##### Summary

This is the MessageServiceConnection implemenation for using RabbitMQ

<a name='M-MQContract-RabbitMQ-Connection-#ctor-RabbitMQ-Client-ConnectionFactory-'></a>
### #ctor(factory) `constructor`

##### Summary

Default constructor for creating instance

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| factory | [RabbitMQ.Client.ConnectionFactory](#T-RabbitMQ-Client-ConnectionFactory 'RabbitMQ.Client.ConnectionFactory') | The connection factory to use that was built with required authentication and connection information |

<a name='P-MQContract-RabbitMQ-Connection-MaxMessageBodySize'></a>
### MaxMessageBodySize `property`

##### Summary

The maximum message body size allowed

<a name='M-MQContract-RabbitMQ-Connection-PublishAsync-MQContract-Messages-ServiceMessage,System-Threading-CancellationToken-'></a>
### PublishAsync(message,cancellationToken) `method`

##### Summary

Called to publish a message into the ActiveMQ server

##### Returns

Transmition result identifying if it worked or not

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| message | [MQContract.Messages.ServiceMessage](#T-MQContract-Messages-ServiceMessage 'MQContract.Messages.ServiceMessage') | The service message being sent |
| cancellationToken | [System.Threading.CancellationToken](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Threading.CancellationToken 'System.Threading.CancellationToken') | A cancellation token |

<a name='M-MQContract-RabbitMQ-Connection-SubscribeAsync-System-Action{MQContract-Messages-RecievedServiceMessage},System-Action{System-Exception},System-String,System-String,System-Threading-CancellationToken-'></a>
### SubscribeAsync(messageRecieved,errorRecieved,channel,group,cancellationToken) `method`

##### Summary

Called to create a subscription to the underlying RabbitMQ server

##### Returns



##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| messageRecieved | [System.Action{MQContract.Messages.RecievedServiceMessage}](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Action 'System.Action{MQContract.Messages.RecievedServiceMessage}') | Callback for when a message is recieved |
| errorRecieved | [System.Action{System.Exception}](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Action 'System.Action{System.Exception}') | Callback for when an error occurs |
| channel | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | The name of the channel to bind to |
| group | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') |  |
| cancellationToken | [System.Threading.CancellationToken](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Threading.CancellationToken 'System.Threading.CancellationToken') |  |

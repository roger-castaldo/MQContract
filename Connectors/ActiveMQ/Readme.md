<a name='assembly'></a>
# MQContract.ActiveMQ

## Contents

- [Connection](#T-MQContract-ActiveMQ-Connection 'MQContract.ActiveMQ.Connection')
  - [#ctor(ConnectUri,username,password)](#M-MQContract-ActiveMQ-Connection-#ctor-System-Uri,System-String,System-String- 'MQContract.ActiveMQ.Connection.#ctor(System.Uri,System.String,System.String)')
  - [MaxMessageBodySize](#P-MQContract-ActiveMQ-Connection-MaxMessageBodySize 'MQContract.ActiveMQ.Connection.MaxMessageBodySize')
  - [CloseAsync()](#M-MQContract-ActiveMQ-Connection-CloseAsync 'MQContract.ActiveMQ.Connection.CloseAsync')
  - [Dispose()](#M-MQContract-ActiveMQ-Connection-Dispose 'MQContract.ActiveMQ.Connection.Dispose')
  - [DisposeAsync()](#M-MQContract-ActiveMQ-Connection-DisposeAsync 'MQContract.ActiveMQ.Connection.DisposeAsync')
  - [PublishAsync(message,cancellationToken)](#M-MQContract-ActiveMQ-Connection-PublishAsync-MQContract-Messages-ServiceMessage,System-Threading-CancellationToken- 'MQContract.ActiveMQ.Connection.PublishAsync(MQContract.Messages.ServiceMessage,System.Threading.CancellationToken)')
  - [SubscribeAsync(messageRecieved,errorRecieved,channel,group,cancellationToken)](#M-MQContract-ActiveMQ-Connection-SubscribeAsync-System-Action{MQContract-Messages-RecievedServiceMessage},System-Action{System-Exception},System-String,System-String,System-Threading-CancellationToken- 'MQContract.ActiveMQ.Connection.SubscribeAsync(System.Action{MQContract.Messages.RecievedServiceMessage},System.Action{System.Exception},System.String,System.String,System.Threading.CancellationToken)')

<a name='T-MQContract-ActiveMQ-Connection'></a>
## Connection `type`

##### Namespace

MQContract.ActiveMQ

##### Summary

This is the MessageServiceConnection implemenation for using ActiveMQ

<a name='M-MQContract-ActiveMQ-Connection-#ctor-System-Uri,System-String,System-String-'></a>
### #ctor(ConnectUri,username,password) `constructor`

##### Summary

Default constructor for creating instance

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| ConnectUri | [System.Uri](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Uri 'System.Uri') | The connection url to use |
| username | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | The username to use |
| password | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | The password to use |

<a name='P-MQContract-ActiveMQ-Connection-MaxMessageBodySize'></a>
### MaxMessageBodySize `property`

##### Summary

The maximum message body size allowed

<a name='M-MQContract-ActiveMQ-Connection-CloseAsync'></a>
### CloseAsync() `method`

##### Summary

Called to close off the underlying ActiveMQ Connection

##### Returns



##### Parameters

This method has no parameters.

<a name='M-MQContract-ActiveMQ-Connection-Dispose'></a>
### Dispose() `method`

##### Summary

Called to dispose of the required resources

##### Parameters

This method has no parameters.

<a name='M-MQContract-ActiveMQ-Connection-DisposeAsync'></a>
### DisposeAsync() `method`

##### Summary

Called to dispose of the object correctly and allow it to clean up it's resources

##### Returns

A task required for disposal

##### Parameters

This method has no parameters.

<a name='M-MQContract-ActiveMQ-Connection-PublishAsync-MQContract-Messages-ServiceMessage,System-Threading-CancellationToken-'></a>
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

<a name='M-MQContract-ActiveMQ-Connection-SubscribeAsync-System-Action{MQContract-Messages-RecievedServiceMessage},System-Action{System-Exception},System-String,System-String,System-Threading-CancellationToken-'></a>
### SubscribeAsync(messageRecieved,errorRecieved,channel,group,cancellationToken) `method`

##### Summary

Called to create a subscription to the underlying ActiveMQ server

##### Returns



##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| messageRecieved | [System.Action{MQContract.Messages.RecievedServiceMessage}](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Action 'System.Action{MQContract.Messages.RecievedServiceMessage}') | Callback for when a message is recieved |
| errorRecieved | [System.Action{System.Exception}](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Action 'System.Action{System.Exception}') | Callback for when an error occurs |
| channel | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | The name of the channel to bind to |
| group | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | The group to bind the consumer to |
| cancellationToken | [System.Threading.CancellationToken](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Threading.CancellationToken 'System.Threading.CancellationToken') | A cancellation token |

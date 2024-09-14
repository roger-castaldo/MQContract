<a name='assembly'></a>
# MQContract.RabbitMQ

## Contents

- [Connection](#T-MQContract-RabbitMQ-Connection 'MQContract.RabbitMQ.Connection')
  - [#ctor(factory)](#M-MQContract-RabbitMQ-Connection-#ctor-RabbitMQ-Client-ConnectionFactory- 'MQContract.RabbitMQ.Connection.#ctor(RabbitMQ.Client.ConnectionFactory)')
  - [DefaultTimout](#P-MQContract-RabbitMQ-Connection-DefaultTimout 'MQContract.RabbitMQ.Connection.DefaultTimout')
  - [MaxMessageBodySize](#P-MQContract-RabbitMQ-Connection-MaxMessageBodySize 'MQContract.RabbitMQ.Connection.MaxMessageBodySize')
  - [CloseAsync()](#M-MQContract-RabbitMQ-Connection-CloseAsync 'MQContract.RabbitMQ.Connection.CloseAsync')
  - [Dispose()](#M-MQContract-RabbitMQ-Connection-Dispose 'MQContract.RabbitMQ.Connection.Dispose')
  - [ExchangeDeclare(exchange,type,durable,autoDelete,arguments)](#M-MQContract-RabbitMQ-Connection-ExchangeDeclare-System-String,System-String,System-Boolean,System-Boolean,System-Collections-Generic-IDictionary{System-String,System-Object}- 'MQContract.RabbitMQ.Connection.ExchangeDeclare(System.String,System.String,System.Boolean,System.Boolean,System.Collections.Generic.IDictionary{System.String,System.Object})')
  - [PublishAsync(message,cancellationToken)](#M-MQContract-RabbitMQ-Connection-PublishAsync-MQContract-Messages-ServiceMessage,System-Threading-CancellationToken- 'MQContract.RabbitMQ.Connection.PublishAsync(MQContract.Messages.ServiceMessage,System.Threading.CancellationToken)')
  - [QueryAsync(message,timeout,cancellationToken)](#M-MQContract-RabbitMQ-Connection-QueryAsync-MQContract-Messages-ServiceMessage,System-TimeSpan,System-Threading-CancellationToken- 'MQContract.RabbitMQ.Connection.QueryAsync(MQContract.Messages.ServiceMessage,System.TimeSpan,System.Threading.CancellationToken)')
  - [QueueDeclare(queue,durable,exclusive,autoDelete,arguments)](#M-MQContract-RabbitMQ-Connection-QueueDeclare-System-String,System-Boolean,System-Boolean,System-Boolean,System-Collections-Generic-IDictionary{System-String,System-Object}- 'MQContract.RabbitMQ.Connection.QueueDeclare(System.String,System.Boolean,System.Boolean,System.Boolean,System.Collections.Generic.IDictionary{System.String,System.Object})')
  - [QueueDelete(queue,ifUnused,ifEmpty)](#M-MQContract-RabbitMQ-Connection-QueueDelete-System-String,System-Boolean,System-Boolean- 'MQContract.RabbitMQ.Connection.QueueDelete(System.String,System.Boolean,System.Boolean)')
  - [SubscribeAsync(messageRecieved,errorRecieved,channel,group,cancellationToken)](#M-MQContract-RabbitMQ-Connection-SubscribeAsync-System-Action{MQContract-Messages-RecievedServiceMessage},System-Action{System-Exception},System-String,System-String,System-Threading-CancellationToken- 'MQContract.RabbitMQ.Connection.SubscribeAsync(System.Action{MQContract.Messages.RecievedServiceMessage},System.Action{System.Exception},System.String,System.String,System.Threading.CancellationToken)')
  - [SubscribeQueryAsync(messageRecieved,errorRecieved,channel,group,cancellationToken)](#M-MQContract-RabbitMQ-Connection-SubscribeQueryAsync-System-Func{MQContract-Messages-RecievedServiceMessage,System-Threading-Tasks-ValueTask{MQContract-Messages-ServiceMessage}},System-Action{System-Exception},System-String,System-String,System-Threading-CancellationToken- 'MQContract.RabbitMQ.Connection.SubscribeQueryAsync(System.Func{MQContract.Messages.RecievedServiceMessage,System.Threading.Tasks.ValueTask{MQContract.Messages.ServiceMessage}},System.Action{System.Exception},System.String,System.String,System.Threading.CancellationToken)')

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

<a name='P-MQContract-RabbitMQ-Connection-DefaultTimout'></a>
### DefaultTimout `property`

##### Summary

The default timeout to use for RPC calls when not specified by class or in the call.
DEFAULT: 1 minute

<a name='P-MQContract-RabbitMQ-Connection-MaxMessageBodySize'></a>
### MaxMessageBodySize `property`

##### Summary

The maximum message body size allowed

<a name='M-MQContract-RabbitMQ-Connection-CloseAsync'></a>
### CloseAsync() `method`

##### Summary

Called to close off the contract connection and close it's underlying service connection

##### Returns

A task for the closure of the connection

##### Parameters

This method has no parameters.

<a name='M-MQContract-RabbitMQ-Connection-Dispose'></a>
### Dispose() `method`

##### Summary

Called to dispose of the object correctly and allow it to clean up it's resources

##### Parameters

This method has no parameters.

<a name='M-MQContract-RabbitMQ-Connection-ExchangeDeclare-System-String,System-String,System-Boolean,System-Boolean,System-Collections-Generic-IDictionary{System-String,System-Object}-'></a>
### ExchangeDeclare(exchange,type,durable,autoDelete,arguments) `method`

##### Summary

Used to decalre an exchange inside the RabbitMQ server

##### Returns

The connection to allow for chaining calls

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| exchange | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | The name of the exchange |
| type | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | The type of the exchange |
| durable | [System.Boolean](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Boolean 'System.Boolean') | Is this durable |
| autoDelete | [System.Boolean](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Boolean 'System.Boolean') | Auto Delete when connection closed |
| arguments | [System.Collections.Generic.IDictionary{System.String,System.Object}](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Collections.Generic.IDictionary 'System.Collections.Generic.IDictionary{System.String,System.Object}') | Additional arguements |

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

<a name='M-MQContract-RabbitMQ-Connection-QueryAsync-MQContract-Messages-ServiceMessage,System-TimeSpan,System-Threading-CancellationToken-'></a>
### QueryAsync(message,timeout,cancellationToken) `method`

##### Summary

Called to publish a query into the RabbitMQ server

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
| [System.InvalidOperationException](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.InvalidOperationException 'System.InvalidOperationException') | Thrown when the query fails to send |
| [System.TimeoutException](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.TimeoutException 'System.TimeoutException') | Thrown when the query times out |

<a name='M-MQContract-RabbitMQ-Connection-QueueDeclare-System-String,System-Boolean,System-Boolean,System-Boolean,System-Collections-Generic-IDictionary{System-String,System-Object}-'></a>
### QueueDeclare(queue,durable,exclusive,autoDelete,arguments) `method`

##### Summary

Used to declare a queue inside the RabbitMQ server

##### Returns

The connection to allow for chaining calls

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| queue | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | The name of the queue |
| durable | [System.Boolean](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Boolean 'System.Boolean') | Is this queue durable |
| exclusive | [System.Boolean](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Boolean 'System.Boolean') | Is this queue exclusive |
| autoDelete | [System.Boolean](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Boolean 'System.Boolean') | Auto Delete queue when connection closed |
| arguments | [System.Collections.Generic.IDictionary{System.String,System.Object}](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Collections.Generic.IDictionary 'System.Collections.Generic.IDictionary{System.String,System.Object}') | Additional arguements |

<a name='M-MQContract-RabbitMQ-Connection-QueueDelete-System-String,System-Boolean,System-Boolean-'></a>
### QueueDelete(queue,ifUnused,ifEmpty) `method`

##### Summary

Used to delete a queue inside the RabbitMQ server

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| queue | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | The name of the queue |
| ifUnused | [System.Boolean](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Boolean 'System.Boolean') | Is unused |
| ifEmpty | [System.Boolean](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Boolean 'System.Boolean') | Is Empty |

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

<a name='M-MQContract-RabbitMQ-Connection-SubscribeQueryAsync-System-Func{MQContract-Messages-RecievedServiceMessage,System-Threading-Tasks-ValueTask{MQContract-Messages-ServiceMessage}},System-Action{System-Exception},System-String,System-String,System-Threading-CancellationToken-'></a>
### SubscribeQueryAsync(messageRecieved,errorRecieved,channel,group,cancellationToken) `method`

##### Summary

Called to create a subscription for queries to the underlying RabbitMQ server

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

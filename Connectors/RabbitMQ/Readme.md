<a name='assembly'></a>
# MQContract.RabbitMQ

## Contents

- [Connection](#T-MQContract-RabbitMQ-Connection 'MQContract.RabbitMQ.Connection')
  - [#ctor(factory)](#M-MQContract-RabbitMQ-Connection-#ctor-RabbitMQ-Client-ConnectionFactory- 'MQContract.RabbitMQ.Connection.#ctor(RabbitMQ.Client.ConnectionFactory)')
  - [DefaultTimeout](#P-MQContract-RabbitMQ-Connection-DefaultTimeout 'MQContract.RabbitMQ.Connection.DefaultTimeout')
  - [MaxMessageBodySize](#P-MQContract-RabbitMQ-Connection-MaxMessageBodySize 'MQContract.RabbitMQ.Connection.MaxMessageBodySize')
  - [RabbitMQConnection](#P-MQContract-RabbitMQ-Connection-RabbitMQConnection 'MQContract.RabbitMQ.Connection.RabbitMQConnection')
  - [ExchangeDeclareAsync(exchange,type,durable,autoDelete,arguments)](#M-MQContract-RabbitMQ-Connection-ExchangeDeclareAsync-System-String,System-String,System-Boolean,System-Boolean,System-Collections-Generic-IDictionary{System-String,System-Object}- 'MQContract.RabbitMQ.Connection.ExchangeDeclareAsync(System.String,System.String,System.Boolean,System.Boolean,System.Collections.Generic.IDictionary{System.String,System.Object})')
  - [QueueDeclareAsync(queue,durable,exclusive,autoDelete,arguments)](#M-MQContract-RabbitMQ-Connection-QueueDeclareAsync-System-String,System-Boolean,System-Boolean,System-Boolean,System-Collections-Generic-IDictionary{System-String,System-Object}- 'MQContract.RabbitMQ.Connection.QueueDeclareAsync(System.String,System.Boolean,System.Boolean,System.Boolean,System.Collections.Generic.IDictionary{System.String,System.Object})')
  - [QueueDeleteAsync(queue,ifUnused,ifEmpty)](#M-MQContract-RabbitMQ-Connection-QueueDeleteAsync-System-String,System-Boolean,System-Boolean- 'MQContract.RabbitMQ.Connection.QueueDeleteAsync(System.String,System.Boolean,System.Boolean)')

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

<a name='P-MQContract-RabbitMQ-Connection-DefaultTimeout'></a>
### DefaultTimeout `property`

##### Summary

The default timeout to use for RPC calls when not specified by class or in the call.
DEFAULT: 1 minute

<a name='P-MQContract-RabbitMQ-Connection-MaxMessageBodySize'></a>
### MaxMessageBodySize `property`

##### Summary

The maximum message body size allowed

<a name='P-MQContract-RabbitMQ-Connection-RabbitMQConnection'></a>
### RabbitMQConnection `property`

##### Summary

Houses the underlying Rabbit MQ Connection

<a name='M-MQContract-RabbitMQ-Connection-ExchangeDeclareAsync-System-String,System-String,System-Boolean,System-Boolean,System-Collections-Generic-IDictionary{System-String,System-Object}-'></a>
### ExchangeDeclareAsync(exchange,type,durable,autoDelete,arguments) `method`

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

<a name='M-MQContract-RabbitMQ-Connection-QueueDeclareAsync-System-String,System-Boolean,System-Boolean,System-Boolean,System-Collections-Generic-IDictionary{System-String,System-Object}-'></a>
### QueueDeclareAsync(queue,durable,exclusive,autoDelete,arguments) `method`

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

<a name='M-MQContract-RabbitMQ-Connection-QueueDeleteAsync-System-String,System-Boolean,System-Boolean-'></a>
### QueueDeleteAsync(queue,ifUnused,ifEmpty) `method`

##### Summary

Used to delete a queue inside the RabbitMQ server

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| queue | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | The name of the queue |
| ifUnused | [System.Boolean](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Boolean 'System.Boolean') | Is unused |
| ifEmpty | [System.Boolean](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Boolean 'System.Boolean') | Is Empty |

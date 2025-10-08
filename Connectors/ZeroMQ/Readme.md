<a name='assembly'></a>
# MQContract.ZeroMQ

## Contents

- [Connection](#T-MQContract-ZeroMQ-Connection 'MQContract.ZeroMQ.Connection')
  - [#ctor()](#M-MQContract-ZeroMQ-Connection-#ctor 'MQContract.ZeroMQ.Connection.#ctor')
  - [MaxMessageBodySize](#P-MQContract-ZeroMQ-Connection-MaxMessageBodySize 'MQContract.ZeroMQ.Connection.MaxMessageBodySize')
  - [BindAsServer(address)](#M-MQContract-ZeroMQ-Connection-BindAsServer-System-String- 'MQContract.ZeroMQ.Connection.BindAsServer(System.String)')
  - [BindInboxAddress(address)](#M-MQContract-ZeroMQ-Connection-BindInboxAddress-System-String- 'MQContract.ZeroMQ.Connection.BindInboxAddress(System.String)')
  - [ConnectToServer(address)](#M-MQContract-ZeroMQ-Connection-ConnectToServer-System-String- 'MQContract.ZeroMQ.Connection.ConnectToServer(System.String)')
- [UndefinedInboxException](#T-MQContract-ZeroMQ-UndefinedInboxException 'MQContract.ZeroMQ.UndefinedInboxException')

<a name='T-MQContract-ZeroMQ-Connection'></a>
## Connection `type`

##### Namespace

MQContract.ZeroMQ

##### Summary

This is the MessageServiceConnection implementation for using ZeroMQ

<a name='M-MQContract-ZeroMQ-Connection-#ctor'></a>
### #ctor() `constructor`

##### Summary

Default Constructor

##### Parameters

This constructor has no parameters.

<a name='P-MQContract-ZeroMQ-Connection-MaxMessageBodySize'></a>
### MaxMessageBodySize `property`

##### Summary

The maximum message body size allowed, defaults to 4MB

<a name='M-MQContract-ZeroMQ-Connection-BindAsServer-System-String-'></a>
### BindAsServer(address) `method`

##### Summary

Called to establish a binding for this instance to act as a server

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| address | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | The address string to bind to |

<a name='M-MQContract-ZeroMQ-Connection-BindInboxAddress-System-String-'></a>
### BindInboxAddress(address) `method`

##### Summary

Called to establish the binding for the inbox address, used for query response

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| address | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | The address string to bind to |

<a name='M-MQContract-ZeroMQ-Connection-ConnectToServer-System-String-'></a>
### ConnectToServer(address) `method`

##### Summary

Called to establish a connection to a listening server

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| address | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | The address string for the server to connect to |

<a name='T-MQContract-ZeroMQ-UndefinedInboxException'></a>
## UndefinedInboxException `type`

##### Namespace

MQContract.ZeroMQ

##### Summary

Thrown when you have attempted to either create the inbox subscription or made a call to Query without setting up the inbox first

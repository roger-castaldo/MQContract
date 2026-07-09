<a name='assembly'></a>
# MQContract.ActiveMQ

## Contents

- [Connection](#T-MQContract-ActiveMQ-Connection 'MQContract.ActiveMQ.Connection')
  - [#ctor(ConnectUri,username,password)](#M-MQContract-ActiveMQ-Connection-#ctor-System-Uri,System-String,System-String- 'MQContract.ActiveMQ.Connection.#ctor(System.Uri,System.String,System.String)')
  - [ActiveMQConnection](#P-MQContract-ActiveMQ-Connection-ActiveMQConnection 'MQContract.ActiveMQ.Connection.ActiveMQConnection')
  - [DefaultTimeout](#P-MQContract-ActiveMQ-Connection-DefaultTimeout 'MQContract.ActiveMQ.Connection.DefaultTimeout')

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

<a name='P-MQContract-ActiveMQ-Connection-ActiveMQConnection'></a>
### ActiveMQConnection `property`

##### Summary

Underlying connection used to connection to ActiveMQ.  Exposed here for additional control if required.

<a name='P-MQContract-ActiveMQ-Connection-DefaultTimeout'></a>
### DefaultTimeout `property`

##### Summary

The default timeout to use for RPC calls when not specified by class or in the call.
DEFAULT: 1 minute

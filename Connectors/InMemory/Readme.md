<a name='assembly'></a>
# MQContract.InMemory

## Contents

- [Connection](#T-MQContract-InMemory-Connection 'MQContract.InMemory.Connection')
  - [DefaultTimeout](#P-MQContract-InMemory-Connection-DefaultTimeout 'MQContract.InMemory.Connection.DefaultTimeout')
  - [MaxMessageBodySize](#P-MQContract-InMemory-Connection-MaxMessageBodySize 'MQContract.InMemory.Connection.MaxMessageBodySize')
- [TransmissionResultException](#T-MQContract-InMemory-TransmissionResultException 'MQContract.InMemory.TransmissionResultException')

<a name='T-MQContract-InMemory-Connection'></a>
## Connection `type`

##### Namespace

MQContract.InMemory

##### Summary

Used as an in memory connection messaging system where all transmission are done through Channels within the connection.  You must use the same underlying connection.

<a name='P-MQContract-InMemory-Connection-DefaultTimeout'></a>
### DefaultTimeout `property`

##### Summary

Default timeout for a given QueryResponse call
default: 1 minute

<a name='P-MQContract-InMemory-Connection-MaxMessageBodySize'></a>
### MaxMessageBodySize `property`

##### Summary

Maximum allowed message body size in bytes
default: 4MB

<a name='T-MQContract-InMemory-TransmissionResultException'></a>
## TransmissionResultException `type`

##### Namespace

MQContract.InMemory

##### Summary

Thrown when a message transmission has failed within the In Memory system

<a name='assembly'></a>
# MQContract.AzureServiceBus

## Contents

- [Connection](#T-MQContract-AzureServiceBus-Connection 'MQContract.AzureServiceBus.Connection')
  - [#ctor(client,pingableQueue)](#M-MQContract-AzureServiceBus-Connection-#ctor-Azure-Messaging-ServiceBus-ServiceBusClient,System-String- 'MQContract.AzureServiceBus.Connection.#ctor(Azure.Messaging.ServiceBus.ServiceBusClient,System.String)')
  - [MaxMessageBodySize](#P-MQContract-AzureServiceBus-Connection-MaxMessageBodySize 'MQContract.AzureServiceBus.Connection.MaxMessageBodySize')

<a name='T-MQContract-AzureServiceBus-Connection'></a>
## Connection `type`

##### Namespace

MQContract.AzureServiceBus

##### Summary

This is the MessageServiceConnection implemenation for using AzureServiceBus

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| client | [T:MQContract.AzureServiceBus.Connection](#T-T-MQContract-AzureServiceBus-Connection 'T:MQContract.AzureServiceBus.Connection') | The ServiceBusClient to use with this instance |

##### Remarks

In order to use the InboxQueryable capabilites that have been built here you should have a QueryResponse.Inbox Topic and subsequent Subscription 
with RequiresSession as true

<a name='M-MQContract-AzureServiceBus-Connection-#ctor-Azure-Messaging-ServiceBus-ServiceBusClient,System-String-'></a>
### #ctor(client,pingableQueue) `constructor`

##### Summary

This is the MessageServiceConnection implemenation for using AzureServiceBus

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| client | [Azure.Messaging.ServiceBus.ServiceBusClient](#T-Azure-Messaging-ServiceBus-ServiceBusClient 'Azure.Messaging.ServiceBus.ServiceBusClient') | The ServiceBusClient to use with this instance |
| pingableQueue | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | A queue to create a receiver against as a form of pinging to ensure connectivity |

##### Remarks

In order to use the InboxQueryable capabilites that have been built here you should have a QueryResponse.Inbox Topic and subsequent Subscription 
with RequiresSession as true

<a name='P-MQContract-AzureServiceBus-Connection-MaxMessageBodySize'></a>
### MaxMessageBodySize `property`

##### Summary

Maximum supported message body size in bytes

<a name='assembly'></a>
# MQContract.GooglePubSub

## Contents

- [Connection](#T-MQContract-GooglePubSub-Connection 'MQContract.GooglePubSub.Connection')
  - [#ctor(projectId,publisherServiceBuilder,subscriberServiceBuilder)](#M-MQContract-GooglePubSub-Connection-#ctor-System-String,Google-Cloud-PubSub-V1-PublisherServiceApiClientBuilder,Google-Cloud-PubSub-V1-SubscriberServiceApiClientBuilder- 'MQContract.GooglePubSub.Connection.#ctor(System.String,Google.Cloud.PubSub.V1.PublisherServiceApiClientBuilder,Google.Cloud.PubSub.V1.SubscriberServiceApiClientBuilder)')
  - [ProjectId](#P-MQContract-GooglePubSub-Connection-ProjectId 'MQContract.GooglePubSub.Connection.ProjectId')
  - [PublisherServiceApi](#P-MQContract-GooglePubSub-Connection-PublisherServiceApi 'MQContract.GooglePubSub.Connection.PublisherServiceApi')
  - [SubscriberServiceApi](#P-MQContract-GooglePubSub-Connection-SubscriberServiceApi 'MQContract.GooglePubSub.Connection.SubscriberServiceApi')

<a name='T-MQContract-GooglePubSub-Connection'></a>
## Connection `type`

##### Namespace

MQContract.GooglePubSub

##### Summary

This is the MessageServiceConnection implementation for using GooglePubSub

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| projectId | [T:MQContract.GooglePubSub.Connection](#T-T-MQContract-GooglePubSub-Connection 'T:MQContract.GooglePubSub.Connection') | The project id to connect to through the PubSub Connections |

<a name='M-MQContract-GooglePubSub-Connection-#ctor-System-String,Google-Cloud-PubSub-V1-PublisherServiceApiClientBuilder,Google-Cloud-PubSub-V1-SubscriberServiceApiClientBuilder-'></a>
### #ctor(projectId,publisherServiceBuilder,subscriberServiceBuilder) `constructor`

##### Summary

This is the MessageServiceConnection implementation for using GooglePubSub

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| projectId | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | The project id to connect to through the PubSub Connections |
| publisherServiceBuilder | [Google.Cloud.PubSub.V1.PublisherServiceApiClientBuilder](#T-Google-Cloud-PubSub-V1-PublisherServiceApiClientBuilder 'Google.Cloud.PubSub.V1.PublisherServiceApiClientBuilder') | Used for building publishers |
| subscriberServiceBuilder | [Google.Cloud.PubSub.V1.SubscriberServiceApiClientBuilder](#T-Google-Cloud-PubSub-V1-SubscriberServiceApiClientBuilder 'Google.Cloud.PubSub.V1.SubscriberServiceApiClientBuilder') | Used for building subscribers |

<a name='P-MQContract-GooglePubSub-Connection-ProjectId'></a>
### ProjectId `property`

##### Summary

Houses the project id that was supplied in the constructor

<a name='P-MQContract-GooglePubSub-Connection-PublisherServiceApi'></a>
### PublisherServiceApi `property`

##### Summary

Houses the Publisher Service API Client used in the underlying service

<a name='P-MQContract-GooglePubSub-Connection-SubscriberServiceApi'></a>
### SubscriberServiceApi `property`

##### Summary

Houses the Subscriber Service API Client used in the underlying service

<a name='assembly'></a>
# MQContract.GooglePubSub

## Contents

- [Connection](#T-MQContract-GooglePubSub-Connection 'MQContract.GooglePubSub.Connection')
  - [#ctor(projectId,publisherClientApi,subscriberClientApi)](#M-MQContract-GooglePubSub-Connection-#ctor-System-String,Google-Cloud-PubSub-V1-PublisherServiceApiClient,Google-Cloud-PubSub-V1-SubscriberServiceApiClient- 'MQContract.GooglePubSub.Connection.#ctor(System.String,Google.Cloud.PubSub.V1.PublisherServiceApiClient,Google.Cloud.PubSub.V1.SubscriberServiceApiClient)')

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

<a name='M-MQContract-GooglePubSub-Connection-#ctor-System-String,Google-Cloud-PubSub-V1-PublisherServiceApiClient,Google-Cloud-PubSub-V1-SubscriberServiceApiClient-'></a>
### #ctor(projectId,publisherClientApi,subscriberClientApi) `constructor`

##### Summary

This is the MessageServiceConnection implementation for using GooglePubSub

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| projectId | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | The project id to connect to through the PubSub Connections |
| publisherClientApi | [Google.Cloud.PubSub.V1.PublisherServiceApiClient](#T-Google-Cloud-PubSub-V1-PublisherServiceApiClient 'Google.Cloud.PubSub.V1.PublisherServiceApiClient') | Used for building publishers |
| subscriberClientApi | [Google.Cloud.PubSub.V1.SubscriberServiceApiClient](#T-Google-Cloud-PubSub-V1-SubscriberServiceApiClient 'Google.Cloud.PubSub.V1.SubscriberServiceApiClient') | Used for building subscribers |

<a name='assembly'></a>
# MQContract.AmazonSNQS

## Contents

- [Connection](#T-MQContract-AmazonSNQS-Connection 'MQContract.AmazonSNQS.Connection')
  - [#ctor()](#M-MQContract-AmazonSNQS-Connection-#ctor-System-Nullable{System-ValueTuple{Amazon-Runtime-AWSCredentials,Amazon-SimpleNotificationService-AmazonSimpleNotificationServiceConfig}},System-Nullable{System-ValueTuple{Amazon-Runtime-AWSCredentials,Amazon-SQS-AmazonSQSConfig}}- 'MQContract.AmazonSNQS.Connection.#ctor(System.Nullable{System.ValueTuple{Amazon.Runtime.AWSCredentials,Amazon.SimpleNotificationService.AmazonSimpleNotificationServiceConfig}},System.Nullable{System.ValueTuple{Amazon.Runtime.AWSCredentials,Amazon.SQS.AmazonSQSConfig}})')
  - [MaxMessageBodySize](#P-MQContract-AmazonSNQS-Connection-MaxMessageBodySize 'MQContract.AmazonSNQS.Connection.MaxMessageBodySize')
  - [SNSClient](#P-MQContract-AmazonSNQS-Connection-SNSClient 'MQContract.AmazonSNQS.Connection.SNSClient')
  - [SQSClient](#P-MQContract-AmazonSNQS-Connection-SQSClient 'MQContract.AmazonSNQS.Connection.SQSClient')
- [InvalidQueueMessageException](#T-MQContract-AmazonSNQS-InvalidQueueMessageException 'MQContract.AmazonSNQS.InvalidQueueMessageException')
- [NoChannelFoundException](#T-MQContract-AmazonSNQS-NoChannelFoundException 'MQContract.AmazonSNQS.NoChannelFoundException')
  - [Channel](#P-MQContract-AmazonSNQS-NoChannelFoundException-Channel 'MQContract.AmazonSNQS.NoChannelFoundException.Channel')
- [NoClientsSetException](#T-MQContract-AmazonSNQS-NoClientsSetException 'MQContract.AmazonSNQS.NoClientsSetException')
- [SqsClientNullException](#T-MQContract-AmazonSNQS-SqsClientNullException 'MQContract.AmazonSNQS.SqsClientNullException')
- [UnableToLocateQueueException](#T-MQContract-AmazonSNQS-UnableToLocateQueueException 'MQContract.AmazonSNQS.UnableToLocateQueueException')

<a name='T-MQContract-AmazonSNQS-Connection'></a>
## Connection `type`

##### Namespace

MQContract.AmazonSNQS

##### Summary

This is the MessageServiceConnection implementation for using Amazon SNS/SQS

<a name='M-MQContract-AmazonSNQS-Connection-#ctor-System-Nullable{System-ValueTuple{Amazon-Runtime-AWSCredentials,Amazon-SimpleNotificationService-AmazonSimpleNotificationServiceConfig}},System-Nullable{System-ValueTuple{Amazon-Runtime-AWSCredentials,Amazon-SQS-AmazonSQSConfig}}-'></a>
### #ctor() `constructor`

##### Summary

Default constructor

##### Parameters

This constructor has no parameters.

<a name='P-MQContract-AmazonSNQS-Connection-MaxMessageBodySize'></a>
### MaxMessageBodySize `property`

##### Summary

The maximum message body size allowed, defaults to 256Kb

<a name='P-MQContract-AmazonSNQS-Connection-SNSClient'></a>
### SNSClient `property`

##### Summary

Houses the SNSClient that was supplied to the connection, this is used for access wrt administration and other items

<a name='P-MQContract-AmazonSNQS-Connection-SQSClient'></a>
### SQSClient `property`

##### Summary

Houses the SQSClient that was supplied to the connection, this is used for access wrt administration and other items

<a name='T-MQContract-AmazonSNQS-InvalidQueueMessageException'></a>
## InvalidQueueMessageException `type`

##### Namespace

MQContract.AmazonSNQS

##### Summary

Thrown when the Message recieved from a Queue is not of the excepted format

<a name='T-MQContract-AmazonSNQS-NoChannelFoundException'></a>
## NoChannelFoundException `type`

##### Namespace

MQContract.AmazonSNQS

##### Summary

Thrown when a public call is attempted but it is unable to locate a topic or queue for the given channel name

<a name='P-MQContract-AmazonSNQS-NoChannelFoundException-Channel'></a>
### Channel `property`

##### Summary

The channel that was unable to be translated to a topic or queue

<a name='T-MQContract-AmazonSNQS-NoClientsSetException'></a>
## NoClientsSetException `type`

##### Namespace

MQContract.AmazonSNQS

##### Summary

Thrown when a publish call is attempted but you have not supplied either of the connection clients

<a name='T-MQContract-AmazonSNQS-SqsClientNullException'></a>
## SqsClientNullException `type`

##### Namespace

MQContract.AmazonSNQS

##### Summary

Thrown when an attempt to create a subscription occurs but the SQSClient is null

<a name='T-MQContract-AmazonSNQS-UnableToLocateQueueException'></a>
## UnableToLocateQueueException `type`

##### Namespace

MQContract.AmazonSNQS

##### Summary

Thrown when the queue for a subscription request cannot be found

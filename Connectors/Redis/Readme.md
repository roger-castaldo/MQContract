<a name='assembly'></a>
# MQContract.Redis

## Contents

- [Connection](#T-MQContract-Redis-Connection 'MQContract.Redis.Connection')
  - [#ctor(configuration)](#M-MQContract-Redis-Connection-#ctor-StackExchange-Redis-ConfigurationOptions- 'MQContract.Redis.Connection.#ctor(StackExchange.Redis.ConfigurationOptions)')
  - [DefaultTimout](#P-MQContract-Redis-Connection-DefaultTimout 'MQContract.Redis.Connection.DefaultTimout')
  - [MaxMessageBodySize](#P-MQContract-Redis-Connection-MaxMessageBodySize 'MQContract.Redis.Connection.MaxMessageBodySize')
  - [DefineConsumerGroupAsync(channel,group)](#M-MQContract-Redis-Connection-DefineConsumerGroupAsync-System-String,System-String- 'MQContract.Redis.Connection.DefineConsumerGroupAsync(System.String,System.String)')

<a name='T-MQContract-Redis-Connection'></a>
## Connection `type`

##### Namespace

MQContract.Redis

##### Summary

This is the MessageServiceConnection implementation for using Redis

<a name='M-MQContract-Redis-Connection-#ctor-StackExchange-Redis-ConfigurationOptions-'></a>
### #ctor(configuration) `constructor`

##### Summary

Default constructor that requires the Redis Configuration settings to be provided

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| configuration | [StackExchange.Redis.ConfigurationOptions](#T-StackExchange-Redis-ConfigurationOptions 'StackExchange.Redis.ConfigurationOptions') | The configuration to use for the redis connections |

<a name='P-MQContract-Redis-Connection-DefaultTimout'></a>
### DefaultTimout `property`

##### Summary

The default timeout to allow for a Query Response call to execute, defaults to 1 minute

<a name='P-MQContract-Redis-Connection-MaxMessageBodySize'></a>
### MaxMessageBodySize `property`

##### Summary

The maximum message body size allowed, defaults to 4MB

<a name='M-MQContract-Redis-Connection-DefineConsumerGroupAsync-System-String,System-String-'></a>
### DefineConsumerGroupAsync(channel,group) `method`

##### Summary

Called to define a consumer group inside redis for a given channel

##### Returns

A ValueTask while the operation executes asynchronously

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| channel | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | The name of the channel to use |
| group | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | The name of the group to use |

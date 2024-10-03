<a name='assembly'></a>
# MQContract.HiveMQ

## Contents

- [Connection](#T-MQContract-HiveMQ-Connection 'MQContract.HiveMQ.Connection')
  - [#ctor(clientOptions)](#M-MQContract-HiveMQ-Connection-#ctor-HiveMQtt-Client-Options-HiveMQClientOptions- 'MQContract.HiveMQ.Connection.#ctor(HiveMQtt.Client.Options.HiveMQClientOptions)')
  - [DefaultTimeout](#P-MQContract-HiveMQ-Connection-DefaultTimeout 'MQContract.HiveMQ.Connection.DefaultTimeout')
- [ConnectionFailedException](#T-MQContract-HiveMQ-ConnectionFailedException 'MQContract.HiveMQ.ConnectionFailedException')

<a name='T-MQContract-HiveMQ-Connection'></a>
## Connection `type`

##### Namespace

MQContract.HiveMQ

##### Summary

This is the MessageServiceConnection implementation for using HiveMQ

<a name='M-MQContract-HiveMQ-Connection-#ctor-HiveMQtt-Client-Options-HiveMQClientOptions-'></a>
### #ctor(clientOptions) `constructor`

##### Summary

Default constructor that requires the HiveMQ client options settings to be provided

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| clientOptions | [HiveMQtt.Client.Options.HiveMQClientOptions](#T-HiveMQtt-Client-Options-HiveMQClientOptions 'HiveMQtt.Client.Options.HiveMQClientOptions') | The required client options to connect to the HiveMQ instance |

<a name='P-MQContract-HiveMQ-Connection-DefaultTimeout'></a>
### DefaultTimeout `property`

##### Summary

The default timeout to allow for a Query Response call to execute, defaults to 1 minute

<a name='T-MQContract-HiveMQ-ConnectionFailedException'></a>
## ConnectionFailedException `type`

##### Namespace

MQContract.HiveMQ

##### Summary

Thrown when the service connection is unable to connect to the HiveMQTT server

<a name='assembly'></a>
# MQContract.HiveMQ

## Contents

- [Connection](#T-MQContract-HiveMQ-Connection 'MQContract.HiveMQ.Connection')
  - [#ctor(clientOptions)](#M-MQContract-HiveMQ-Connection-#ctor-HiveMQtt-Client-Options-HiveMQClientOptions- 'MQContract.HiveMQ.Connection.#ctor(HiveMQtt.Client.Options.HiveMQClientOptions)')
  - [DefaultTimout](#P-MQContract-HiveMQ-Connection-DefaultTimout 'MQContract.HiveMQ.Connection.DefaultTimout')

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

<a name='P-MQContract-HiveMQ-Connection-DefaultTimout'></a>
### DefaultTimout `property`

##### Summary

The default timeout to allow for a Query Response call to execute, defaults to 1 minute

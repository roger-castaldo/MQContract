<a name='assembly'></a>
# MQContract.Kafka

## Contents

- [Connection](#T-MQContract-Kafka-Connection 'MQContract.Kafka.Connection')
  - [#ctor(clientConfig)](#M-MQContract-Kafka-Connection-#ctor-Confluent-Kafka-ClientConfig- 'MQContract.Kafka.Connection.#ctor(Confluent.Kafka.ClientConfig)')
- [SchemaValidationMiddleware](#T-MQContract-Kafka-Middleware-SchemaValidationMiddleware 'MQContract.Kafka.Middleware.SchemaValidationMiddleware')
  - [#ctor(schemaRegistryClient,failOnMissingSchema,autoRegisterSchema,registerSchemaType,extractSchemaAsync,validateSchemaAsync)](#M-MQContract-Kafka-Middleware-SchemaValidationMiddleware-#ctor-Confluent-SchemaRegistry-ISchemaRegistryClient,System-Boolean,System-Boolean,Confluent-SchemaRegistry-SchemaType,System-Func{System-Type,System-Threading-Tasks-ValueTask{System-String}},System-Func{Confluent-SchemaRegistry-Schema,System-IO-Stream,System-Threading-Tasks-ValueTask{System-Boolean}}- 'MQContract.Kafka.Middleware.SchemaValidationMiddleware.#ctor(Confluent.SchemaRegistry.ISchemaRegistryClient,System.Boolean,System.Boolean,Confluent.SchemaRegistry.SchemaType,System.Func{System.Type,System.Threading.Tasks.ValueTask{System.String}},System.Func{Confluent.SchemaRegistry.Schema,System.IO.Stream,System.Threading.Tasks.ValueTask{System.Boolean}})')

<a name='T-MQContract-Kafka-Connection'></a>
## Connection `type`

##### Namespace

MQContract.Kafka

##### Summary

This is the MessageServiceConnection implementation for using Kafka

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| clientConfig | [T:MQContract.Kafka.Connection](#T-T-MQContract-Kafka-Connection 'T:MQContract.Kafka.Connection') | The Kafka Client Configuration to provide |

<a name='M-MQContract-Kafka-Connection-#ctor-Confluent-Kafka-ClientConfig-'></a>
### #ctor(clientConfig) `constructor`

##### Summary

This is the MessageServiceConnection implementation for using Kafka

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| clientConfig | [Confluent.Kafka.ClientConfig](#T-Confluent-Kafka-ClientConfig 'Confluent.Kafka.ClientConfig') | The Kafka Client Configuration to provide |

<a name='T-MQContract-Kafka-Middleware-SchemaValidationMiddleware'></a>
## SchemaValidationMiddleware `type`

##### Namespace

MQContract.Kafka.Middleware

##### Summary

Used to inject a confluent schema registry validation middleware that will use the confluent style schema registry to validate message types and 
attach schema information to each message

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| schemaRegistryClient | [T:MQContract.Kafka.Middleware.SchemaValidationMiddleware](#T-T-MQContract-Kafka-Middleware-SchemaValidationMiddleware 'T:MQContract.Kafka.Middleware.SchemaValidationMiddleware') | A schema registry client used to validate messages |

<a name='M-MQContract-Kafka-Middleware-SchemaValidationMiddleware-#ctor-Confluent-SchemaRegistry-ISchemaRegistryClient,System-Boolean,System-Boolean,Confluent-SchemaRegistry-SchemaType,System-Func{System-Type,System-Threading-Tasks-ValueTask{System-String}},System-Func{Confluent-SchemaRegistry-Schema,System-IO-Stream,System-Threading-Tasks-ValueTask{System-Boolean}}-'></a>
### #ctor(schemaRegistryClient,failOnMissingSchema,autoRegisterSchema,registerSchemaType,extractSchemaAsync,validateSchemaAsync) `constructor`

##### Summary

Used to inject a confluent schema registry validation middleware that will use the confluent style schema registry to validate message types and 
attach schema information to each message

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| schemaRegistryClient | [Confluent.SchemaRegistry.ISchemaRegistryClient](#T-Confluent-SchemaRegistry-ISchemaRegistryClient 'Confluent.SchemaRegistry.ISchemaRegistryClient') | A schema registry client used to validate messages |
| failOnMissingSchema | [System.Boolean](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Boolean 'System.Boolean') | Indicates if the message should fail when no schema is available |
| autoRegisterSchema | [System.Boolean](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Boolean 'System.Boolean') | Indicates if the system should attempt to register a schema when one is not found on the publish side |
| registerSchemaType | [Confluent.SchemaRegistry.SchemaType](#T-Confluent-SchemaRegistry-SchemaType 'Confluent.SchemaRegistry.SchemaType') | The default schema registration type to use when registering a schema |
| extractSchemaAsync | [System.Func{System.Type,System.Threading.Tasks.ValueTask{System.String}}](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Func 'System.Func{System.Type,System.Threading.Tasks.ValueTask{System.String}}') | An alternative call to generate the schema for a given message type, otherwise NJsonSchema will be used |
| validateSchemaAsync | [System.Func{Confluent.SchemaRegistry.Schema,System.IO.Stream,System.Threading.Tasks.ValueTask{System.Boolean}}](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Func 'System.Func{Confluent.SchemaRegistry.Schema,System.IO.Stream,System.Threading.Tasks.ValueTask{System.Boolean}}') | An alternative call to validate the schema and the incoming message content, otherwise it will default through NJsonSchema |

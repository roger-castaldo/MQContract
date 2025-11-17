<a name='assembly'></a>
# MQContract.CQRS

## Contents

- [CommandAttribute](#T-MQContract-CQRS-Attributes-CommandAttribute 'MQContract.CQRS.Attributes.CommandAttribute')
  - [#ctor(channel,typeName,typeVersion)](#M-MQContract-CQRS-Attributes-CommandAttribute-#ctor-System-String,System-String,System-String- 'MQContract.CQRS.Attributes.CommandAttribute.#ctor(System.String,System.String,System.String)')
- [CommandCallException](#T-MQContract-CQRS-CommandCallException 'MQContract.CQRS.CommandCallException')
  - [Error](#P-MQContract-CQRS-CommandCallException-Error 'MQContract.CQRS.CommandCallException.Error')
- [CommandTimeoutException](#T-MQContract-CQRS-CommandTimeoutException 'MQContract.CQRS.CommandTimeoutException')
- [Context](#T-MQContract-CQRS-Context 'MQContract.CQRS.Context')
  - [#ctor()](#M-MQContract-CQRS-Context-#ctor 'MQContract.CQRS.Context.#ctor')
  - [CorrelationId](#P-MQContract-CQRS-Context-CorrelationId 'MQContract.CQRS.Context.CorrelationId')
  - [Item](#P-MQContract-CQRS-Context-Item-System-String- 'MQContract.CQRS.Context.Item(System.String)')
  - [Keys](#P-MQContract-CQRS-Context-Keys 'MQContract.CQRS.Context.Keys')
  - [MessageId](#P-MQContract-CQRS-Context-MessageId 'MQContract.CQRS.Context.MessageId')
- [ICQRSConnection](#T-MQContract-CQRS-Interfaces-ICQRSConnection 'MQContract.CQRS.Interfaces.ICQRSConnection')
  - [ExecuteCommandAsync\`\`1(command,context,cancellationToken)](#M-MQContract-CQRS-Interfaces-ICQRSConnection-ExecuteCommandAsync``1-``0,MQContract-CQRS-Context,System-Threading-CancellationToken- 'MQContract.CQRS.Interfaces.ICQRSConnection.ExecuteCommandAsync``1(``0,MQContract.CQRS.Context,System.Threading.CancellationToken)')
  - [ExecuteCommandAsync\`\`2(command,context,timeout,cancellationToken)](#M-MQContract-CQRS-Interfaces-ICQRSConnection-ExecuteCommandAsync``2-``0,MQContract-CQRS-Context,System-Nullable{System-TimeSpan},System-Threading-CancellationToken- 'MQContract.CQRS.Interfaces.ICQRSConnection.ExecuteCommandAsync``2(``0,MQContract.CQRS.Context,System.Nullable{System.TimeSpan},System.Threading.CancellationToken)')
  - [ExecuteQueryAsync\`\`2(query,context,timeout,cancellationToken)](#M-MQContract-CQRS-Interfaces-ICQRSConnection-ExecuteQueryAsync``2-``0,MQContract-CQRS-Context,System-Nullable{System-TimeSpan},System-Threading-CancellationToken- 'MQContract.CQRS.Interfaces.ICQRSConnection.ExecuteQueryAsync``2(``0,MQContract.CQRS.Context,System.Nullable{System.TimeSpan},System.Threading.CancellationToken)')
  - [RegisterCommandProcessorAsync\`\`1(processor,group)](#M-MQContract-CQRS-Interfaces-ICQRSConnection-RegisterCommandProcessorAsync``1-MQContract-CQRS-Interfaces-Command-ICommandProcessor{``0},System-String- 'MQContract.CQRS.Interfaces.ICQRSConnection.RegisterCommandProcessorAsync``1(MQContract.CQRS.Interfaces.Command.ICommandProcessor{``0},System.String)')
  - [RegisterCommandProcessorAsync\`\`2(processor,group)](#M-MQContract-CQRS-Interfaces-ICQRSConnection-RegisterCommandProcessorAsync``2-MQContract-CQRS-Interfaces-Command-ICommandProcessor{``0,``1},System-String- 'MQContract.CQRS.Interfaces.ICQRSConnection.RegisterCommandProcessorAsync``2(MQContract.CQRS.Interfaces.Command.ICommandProcessor{``0,``1},System.String)')
  - [RegisterQueryProcessorAsync\`\`2(processor,group)](#M-MQContract-CQRS-Interfaces-ICQRSConnection-RegisterQueryProcessorAsync``2-MQContract-CQRS-Interfaces-Query-IQueryProcessor{``0,``1},System-String- 'MQContract.CQRS.Interfaces.ICQRSConnection.RegisterQueryProcessorAsync``2(MQContract.CQRS.Interfaces.Query.IQueryProcessor{``0,``1},System.String)')
- [ICQRSConnectionExtension](#T-MQContract-CQRS-Extensions-ICQRSConnectionExtension 'MQContract.CQRS.Extensions.ICQRSConnectionExtension')
  - [RegisterCommandProcessorAsync\`\`1(cqrsConnectionTask,processor,group)](#M-MQContract-CQRS-Extensions-ICQRSConnectionExtension-RegisterCommandProcessorAsync``1-System-Threading-Tasks-ValueTask{MQContract-CQRS-Interfaces-ICQRSConnection},MQContract-CQRS-Interfaces-Command-ICommandProcessor{``0},System-String- 'MQContract.CQRS.Extensions.ICQRSConnectionExtension.RegisterCommandProcessorAsync``1(System.Threading.Tasks.ValueTask{MQContract.CQRS.Interfaces.ICQRSConnection},MQContract.CQRS.Interfaces.Command.ICommandProcessor{``0},System.String)')
  - [RegisterCommandProcessorAsync\`\`2(cqrsConnectionTask,processor,group)](#M-MQContract-CQRS-Extensions-ICQRSConnectionExtension-RegisterCommandProcessorAsync``2-System-Threading-Tasks-ValueTask{MQContract-CQRS-Interfaces-ICQRSConnection},MQContract-CQRS-Interfaces-Command-ICommandProcessor{``0,``1},System-String- 'MQContract.CQRS.Extensions.ICQRSConnectionExtension.RegisterCommandProcessorAsync``2(System.Threading.Tasks.ValueTask{MQContract.CQRS.Interfaces.ICQRSConnection},MQContract.CQRS.Interfaces.Command.ICommandProcessor{``0,``1},System.String)')
  - [RegisterQueryProcessorAsync\`\`2(cqrsConnectionTask,processor,group)](#M-MQContract-CQRS-Extensions-ICQRSConnectionExtension-RegisterQueryProcessorAsync``2-System-Threading-Tasks-ValueTask{MQContract-CQRS-Interfaces-ICQRSConnection},MQContract-CQRS-Interfaces-Query-IQueryProcessor{``0,``1},System-String- 'MQContract.CQRS.Extensions.ICQRSConnectionExtension.RegisterQueryProcessorAsync``2(System.Threading.Tasks.ValueTask{MQContract.CQRS.Interfaces.ICQRSConnection},MQContract.CQRS.Interfaces.Query.IQueryProcessor{``0,``1},System.String)')
- [ICommand](#T-MQContract-CQRS-Interfaces-Command-ICommand 'MQContract.CQRS.Interfaces.Command.ICommand')
- [ICommandInvocationContext\`1](#T-MQContract-CQRS-Interfaces-Command-ICommandInvocationContext`1 'MQContract.CQRS.Interfaces.Command.ICommandInvocationContext`1')
  - [Command](#P-MQContract-CQRS-Interfaces-Command-ICommandInvocationContext`1-Command 'MQContract.CQRS.Interfaces.Command.ICommandInvocationContext`1.Command')
- [ICommandProcessor\`1](#T-MQContract-CQRS-Interfaces-Command-ICommandProcessor`1 'MQContract.CQRS.Interfaces.Command.ICommandProcessor`1')
  - [ProcessCommandAsync(invocationContext,cancellationToken)](#M-MQContract-CQRS-Interfaces-Command-ICommandProcessor`1-ProcessCommandAsync-MQContract-CQRS-Interfaces-Command-ICommandInvocationContext{`0},System-Threading-CancellationToken- 'MQContract.CQRS.Interfaces.Command.ICommandProcessor`1.ProcessCommandAsync(MQContract.CQRS.Interfaces.Command.ICommandInvocationContext{`0},System.Threading.CancellationToken)')
- [ICommandProcessor\`2](#T-MQContract-CQRS-Interfaces-Command-ICommandProcessor`2 'MQContract.CQRS.Interfaces.Command.ICommandProcessor`2')
  - [ProcessCommandAsync(invocationContext,cancellationToken)](#M-MQContract-CQRS-Interfaces-Command-ICommandProcessor`2-ProcessCommandAsync-MQContract-CQRS-Interfaces-Command-ICommandInvocationContext{`0},System-Threading-CancellationToken- 'MQContract.CQRS.Interfaces.Command.ICommandProcessor`2.ProcessCommandAsync(MQContract.CQRS.Interfaces.Command.ICommandInvocationContext{`0},System.Threading.CancellationToken)')
- [ICommand\`1](#T-MQContract-CQRS-Interfaces-Command-ICommand`1 'MQContract.CQRS.Interfaces.Command.ICommand`1')
- [IContextFilteredProcessor](#T-MQContract-CQRS-Interfaces-IContextFilteredProcessor 'MQContract.CQRS.Interfaces.IContextFilteredProcessor')
  - [Filter](#P-MQContract-CQRS-Interfaces-IContextFilteredProcessor-Filter 'MQContract.CQRS.Interfaces.IContextFilteredProcessor.Filter')
- [IContractConnectionExtension](#T-MQContract-CQRS-Extensions-IContractConnectionExtension 'MQContract.CQRS.Extensions.IContractConnectionExtension')
  - [CreateCQRSConnection(contractConnection,cancelationTokenChannel)](#M-MQContract-CQRS-Extensions-IContractConnectionExtension-CreateCQRSConnection-MQContract-Interfaces-IContractConnection,System-String- 'MQContract.CQRS.Extensions.IContractConnectionExtension.CreateCQRSConnection(MQContract.Interfaces.IContractConnection,System.String)')
- [IFilteredCommandProcessor\`1](#T-MQContract-CQRS-Interfaces-Command-IFilteredCommandProcessor`1 'MQContract.CQRS.Interfaces.Command.IFilteredCommandProcessor`1')
  - [Filter](#P-MQContract-CQRS-Interfaces-Command-IFilteredCommandProcessor`1-Filter 'MQContract.CQRS.Interfaces.Command.IFilteredCommandProcessor`1.Filter')
- [IFilteredQueryProcessor\`2](#T-MQContract-CQRS-Interfaces-Query-IFilteredQueryProcessor`2 'MQContract.CQRS.Interfaces.Query.IFilteredQueryProcessor`2')
  - [Filter](#P-MQContract-CQRS-Interfaces-Query-IFilteredQueryProcessor`2-Filter 'MQContract.CQRS.Interfaces.Query.IFilteredQueryProcessor`2.Filter')
- [IInvocationContext](#T-MQContract-CQRS-Interfaces-IInvocationContext 'MQContract.CQRS.Interfaces.IInvocationContext')
  - [Activity](#P-MQContract-CQRS-Interfaces-IInvocationContext-Activity 'MQContract.CQRS.Interfaces.IInvocationContext.Activity')
  - [CausationId](#P-MQContract-CQRS-Interfaces-IInvocationContext-CausationId 'MQContract.CQRS.Interfaces.IInvocationContext.CausationId')
  - [CorrelationId](#P-MQContract-CQRS-Interfaces-IInvocationContext-CorrelationId 'MQContract.CQRS.Interfaces.IInvocationContext.CorrelationId')
  - [Item](#P-MQContract-CQRS-Interfaces-IInvocationContext-Item-System-String- 'MQContract.CQRS.Interfaces.IInvocationContext.Item(System.String)')
  - [Keys](#P-MQContract-CQRS-Interfaces-IInvocationContext-Keys 'MQContract.CQRS.Interfaces.IInvocationContext.Keys')
  - [MessageId](#P-MQContract-CQRS-Interfaces-IInvocationContext-MessageId 'MQContract.CQRS.Interfaces.IInvocationContext.MessageId')
  - [ExecuteCommandAsync\`\`1(command)](#M-MQContract-CQRS-Interfaces-IInvocationContext-ExecuteCommandAsync``1-``0- 'MQContract.CQRS.Interfaces.IInvocationContext.ExecuteCommandAsync``1(``0)')
  - [ExecuteCommandAsync\`\`2(command,timeout)](#M-MQContract-CQRS-Interfaces-IInvocationContext-ExecuteCommandAsync``2-``0,System-Nullable{System-TimeSpan}- 'MQContract.CQRS.Interfaces.IInvocationContext.ExecuteCommandAsync``2(``0,System.Nullable{System.TimeSpan})')
  - [ExecuteQueryAsync\`\`2(query,timeout)](#M-MQContract-CQRS-Interfaces-IInvocationContext-ExecuteQueryAsync``2-``0,System-Nullable{System-TimeSpan}- 'MQContract.CQRS.Interfaces.IInvocationContext.ExecuteQueryAsync``2(``0,System.Nullable{System.TimeSpan})')
- [IProcessor](#T-MQContract-CQRS-Interfaces-IProcessor 'MQContract.CQRS.Interfaces.IProcessor')
  - [ErrorRecieved(error)](#M-MQContract-CQRS-Interfaces-IProcessor-ErrorRecieved-System-Exception- 'MQContract.CQRS.Interfaces.IProcessor.ErrorRecieved(System.Exception)')
- [IQuery](#T-MQContract-CQRS-Interfaces-Query-IQuery 'MQContract.CQRS.Interfaces.Query.IQuery')
- [IQueryInvocationContext\`1](#T-MQContract-CQRS-Interfaces-Query-IQueryInvocationContext`1 'MQContract.CQRS.Interfaces.Query.IQueryInvocationContext`1')
  - [Query](#P-MQContract-CQRS-Interfaces-Query-IQueryInvocationContext`1-Query 'MQContract.CQRS.Interfaces.Query.IQueryInvocationContext`1.Query')
- [IQueryProcessor\`2](#T-MQContract-CQRS-Interfaces-Query-IQueryProcessor`2 'MQContract.CQRS.Interfaces.Query.IQueryProcessor`2')
  - [ProcessQueryAsync(invocationContext,cancellationToken)](#M-MQContract-CQRS-Interfaces-Query-IQueryProcessor`2-ProcessQueryAsync-MQContract-CQRS-Interfaces-Query-IQueryInvocationContext{`0},System-Threading-CancellationToken- 'MQContract.CQRS.Interfaces.Query.IQueryProcessor`2.ProcessQueryAsync(MQContract.CQRS.Interfaces.Query.IQueryInvocationContext{`0},System.Threading.CancellationToken)')
- [InvalidConnectionException](#T-MQContract-CQRS-InvalidConnectionException 'MQContract.CQRS.InvalidConnectionException')
- [QueryAttribute](#T-MQContract-CQRS-Attributes-QueryAttribute 'MQContract.CQRS.Attributes.QueryAttribute')
  - [#ctor(channel,typeName,typeVersion,responseChannel,responseTimeoutMilliseconds)](#M-MQContract-CQRS-Attributes-QueryAttribute-#ctor-System-String,System-String,System-String,System-String,System-Int32- 'MQContract.CQRS.Attributes.QueryAttribute.#ctor(System.String,System.String,System.String,System.String,System.Int32)')
- [QueryCallException](#T-MQContract-CQRS-QueryCallException 'MQContract.CQRS.QueryCallException')
  - [Error](#P-MQContract-CQRS-QueryCallException-Error 'MQContract.CQRS.QueryCallException.Error')

<a name='T-MQContract-CQRS-Attributes-CommandAttribute'></a>
## CommandAttribute `type`

##### Namespace

MQContract.CQRS.Attributes

##### Summary

Use this attribute to specify the Channel, TypeName and or TypeVersion of the 
Command being defined

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| channel | [T:MQContract.CQRS.Attributes.CommandAttribute](#T-T-MQContract-CQRS-Attributes-CommandAttribute 'T:MQContract.CQRS.Attributes.CommandAttribute') | The channel to be used |

<a name='M-MQContract-CQRS-Attributes-CommandAttribute-#ctor-System-String,System-String,System-String-'></a>
### #ctor(channel,typeName,typeVersion) `constructor`

##### Summary

Use this attribute to specify the Channel, TypeName and or TypeVersion of the 
Command being defined

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| channel | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | The channel to be used |
| typeName | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | The command type to use |
| typeVersion | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | The command type version to use |

<a name='T-MQContract-CQRS-CommandCallException'></a>
## CommandCallException `type`

##### Namespace

MQContract.CQRS

##### Summary

Thrown when a command execution call throws an error from the underlying contract connection

<a name='P-MQContract-CQRS-CommandCallException-Error'></a>
### Error `property`

##### Summary

The error that occured while attempting to execute a given command

<a name='T-MQContract-CQRS-CommandTimeoutException'></a>
## CommandTimeoutException `type`

##### Namespace

MQContract.CQRS

##### Summary

Thrown when a command call's timeout is exceeded prior to a response being returned

<a name='T-MQContract-CQRS-Context'></a>
## Context `type`

##### Namespace

MQContract.CQRS

##### Summary

Houses the given transmission context for a command or query.  This is used to pass on additional 
string properties between calls as well as houses the unique identifiers that can be used to link
chained commands/queries

<a name='M-MQContract-CQRS-Context-#ctor'></a>
### #ctor() `constructor`

##### Summary

Default constructor

##### Parameters

This constructor has no parameters.

<a name='P-MQContract-CQRS-Context-CorrelationId'></a>
### CorrelationId `property`

##### Summary

The unique identifier for the given message chain

<a name='P-MQContract-CQRS-Context-Item-System-String-'></a>
### Item `property`

##### Summary

Used to add/remove values in the context

##### Returns

The value that is currently assigned to the provided key or null if missing

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| key | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | The key for the value to access |

<a name='P-MQContract-CQRS-Context-Keys'></a>
### Keys `property`

##### Summary

The list of the available keys

<a name='P-MQContract-CQRS-Context-MessageId'></a>
### MessageId `property`

##### Summary

The unique identifier for the given message

<a name='T-MQContract-CQRS-Interfaces-ICQRSConnection'></a>
## ICQRSConnection `type`

##### Namespace

MQContract.CQRS.Interfaces

##### Summary

The primary interface for all CQRS operations.  This is used to both execute commands/queries as well as to register processors for those items

<a name='M-MQContract-CQRS-Interfaces-ICQRSConnection-ExecuteCommandAsync``1-``0,MQContract-CQRS-Context,System-Threading-CancellationToken-'></a>
### ExecuteCommandAsync\`\`1(command,context,cancellationToken) `method`

##### Summary

Called to execute a command of the given type

##### Returns

A ValueTask to allow for async execution

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| command | [\`\`0](#T-``0 '``0') | The command to execute |
| context | [MQContract.CQRS.Context](#T-MQContract-CQRS-Context 'MQContract.CQRS.Context') | The context to use if desired |
| cancellationToken | [System.Threading.CancellationToken](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Threading.CancellationToken 'System.Threading.CancellationToken') | A cancellation token to use if desired |

##### Generic Types

| Name | Description |
| ---- | ----------- |
| TCommand | The type of command to execute |

<a name='M-MQContract-CQRS-Interfaces-ICQRSConnection-ExecuteCommandAsync``2-``0,MQContract-CQRS-Context,System-Nullable{System-TimeSpan},System-Threading-CancellationToken-'></a>
### ExecuteCommandAsync\`\`2(command,context,timeout,cancellationToken) `method`

##### Summary

Called to execute a command of the given type that is expected to provide a given response

##### Returns

The exepected result type

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| command | [\`\`0](#T-``0 '``0') | The command to execute |
| context | [MQContract.CQRS.Context](#T-MQContract-CQRS-Context 'MQContract.CQRS.Context') | The context to use if desired |
| timeout | [System.Nullable{System.TimeSpan}](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Nullable 'System.Nullable{System.TimeSpan}') | The timeout to allow for the execution, if not specified the underlying contract connection defaults will apply |
| cancellationToken | [System.Threading.CancellationToken](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Threading.CancellationToken 'System.Threading.CancellationToken') | A cancellation token to use if desired |

##### Generic Types

| Name | Description |
| ---- | ----------- |
| TCommand | The type of command to execute |
| TCommandResult | The type of response to expect |

<a name='M-MQContract-CQRS-Interfaces-ICQRSConnection-ExecuteQueryAsync``2-``0,MQContract-CQRS-Context,System-Nullable{System-TimeSpan},System-Threading-CancellationToken-'></a>
### ExecuteQueryAsync\`\`2(query,context,timeout,cancellationToken) `method`

##### Summary

Called to execute a query of the given type

##### Returns

The exepected result type

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| query | [\`\`0](#T-``0 '``0') | The query to execute |
| context | [MQContract.CQRS.Context](#T-MQContract-CQRS-Context 'MQContract.CQRS.Context') | The context to use if desired |
| timeout | [System.Nullable{System.TimeSpan}](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Nullable 'System.Nullable{System.TimeSpan}') | The timeout to allow for the execution, if not specified the underlying contract connection defaults will apply |
| cancellationToken | [System.Threading.CancellationToken](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Threading.CancellationToken 'System.Threading.CancellationToken') | A cancellation token to use if desired |

##### Generic Types

| Name | Description |
| ---- | ----------- |
| TQuery | The type of query to execute |
| TQueryResponse | The type of response to expect |

<a name='M-MQContract-CQRS-Interfaces-ICQRSConnection-RegisterCommandProcessorAsync``1-MQContract-CQRS-Interfaces-Command-ICommandProcessor{``0},System-String-'></a>
### RegisterCommandProcessorAsync\`\`1(processor,group) `method`

##### Summary

Register a Command processor

##### Returns

The underlying CQRS connection

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| processor | [MQContract.CQRS.Interfaces.Command.ICommandProcessor{\`\`0}](#T-MQContract-CQRS-Interfaces-Command-ICommandProcessor{``0} 'MQContract.CQRS.Interfaces.Command.ICommandProcessor{``0}') | The command processor to register |
| group | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | The group name to assign it if desired |

##### Generic Types

| Name | Description |
| ---- | ----------- |
| TCommand | The type of command the processor handles |

<a name='M-MQContract-CQRS-Interfaces-ICQRSConnection-RegisterCommandProcessorAsync``2-MQContract-CQRS-Interfaces-Command-ICommandProcessor{``0,``1},System-String-'></a>
### RegisterCommandProcessorAsync\`\`2(processor,group) `method`

##### Summary

Register a Command processor for a command with a response

##### Returns

The underlying CQRS connection

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| processor | [MQContract.CQRS.Interfaces.Command.ICommandProcessor{\`\`0,\`\`1}](#T-MQContract-CQRS-Interfaces-Command-ICommandProcessor{``0,``1} 'MQContract.CQRS.Interfaces.Command.ICommandProcessor{``0,``1}') | The command processor to register |
| group | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | The group name to assign it if desired |

##### Generic Types

| Name | Description |
| ---- | ----------- |
| TCommand | The type of command the processor handles |
| TCommandResult | The type of response for the command call |

<a name='M-MQContract-CQRS-Interfaces-ICQRSConnection-RegisterQueryProcessorAsync``2-MQContract-CQRS-Interfaces-Query-IQueryProcessor{``0,``1},System-String-'></a>
### RegisterQueryProcessorAsync\`\`2(processor,group) `method`

##### Summary

Register a Query processor

##### Returns

The underlying CQRS connection

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| processor | [MQContract.CQRS.Interfaces.Query.IQueryProcessor{\`\`0,\`\`1}](#T-MQContract-CQRS-Interfaces-Query-IQueryProcessor{``0,``1} 'MQContract.CQRS.Interfaces.Query.IQueryProcessor{``0,``1}') | The query processor to register |
| group | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | The group name to assign it if desired |

##### Generic Types

| Name | Description |
| ---- | ----------- |
| TQuery | The type of Query the processor handles |
| TQueryResponse | The type of response for the query call |

<a name='T-MQContract-CQRS-Extensions-ICQRSConnectionExtension'></a>
## ICQRSConnectionExtension `type`

##### Namespace

MQContract.CQRS.Extensions

##### Summary

Extension calls used to provide a Fluent style set of calls for registering the different processors.

<a name='M-MQContract-CQRS-Extensions-ICQRSConnectionExtension-RegisterCommandProcessorAsync``1-System-Threading-Tasks-ValueTask{MQContract-CQRS-Interfaces-ICQRSConnection},MQContract-CQRS-Interfaces-Command-ICommandProcessor{``0},System-String-'></a>
### RegisterCommandProcessorAsync\`\`1(cqrsConnectionTask,processor,group) `method`

##### Summary

Register a Command processor

##### Returns

The underlying CQRS connection

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| cqrsConnectionTask | [System.Threading.Tasks.ValueTask{MQContract.CQRS.Interfaces.ICQRSConnection}](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Threading.Tasks.ValueTask 'System.Threading.Tasks.ValueTask{MQContract.CQRS.Interfaces.ICQRSConnection}') | The previous registration call task |
| processor | [MQContract.CQRS.Interfaces.Command.ICommandProcessor{\`\`0}](#T-MQContract-CQRS-Interfaces-Command-ICommandProcessor{``0} 'MQContract.CQRS.Interfaces.Command.ICommandProcessor{``0}') | The command processor to register |
| group | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | The group name to assign it if desired |

##### Generic Types

| Name | Description |
| ---- | ----------- |
| TCommand | The type of command the processor handles |

<a name='M-MQContract-CQRS-Extensions-ICQRSConnectionExtension-RegisterCommandProcessorAsync``2-System-Threading-Tasks-ValueTask{MQContract-CQRS-Interfaces-ICQRSConnection},MQContract-CQRS-Interfaces-Command-ICommandProcessor{``0,``1},System-String-'></a>
### RegisterCommandProcessorAsync\`\`2(cqrsConnectionTask,processor,group) `method`

##### Summary

Register a Command processor for a command with a response

##### Returns

The underlying CQRS connection

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| cqrsConnectionTask | [System.Threading.Tasks.ValueTask{MQContract.CQRS.Interfaces.ICQRSConnection}](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Threading.Tasks.ValueTask 'System.Threading.Tasks.ValueTask{MQContract.CQRS.Interfaces.ICQRSConnection}') | The previous registration call task |
| processor | [MQContract.CQRS.Interfaces.Command.ICommandProcessor{\`\`0,\`\`1}](#T-MQContract-CQRS-Interfaces-Command-ICommandProcessor{``0,``1} 'MQContract.CQRS.Interfaces.Command.ICommandProcessor{``0,``1}') | The command processor to register |
| group | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | The group name to assign it if desired |

##### Generic Types

| Name | Description |
| ---- | ----------- |
| TCommand | The type of command the processor handles |
| TCommandResult | The type of response for the command call |

<a name='M-MQContract-CQRS-Extensions-ICQRSConnectionExtension-RegisterQueryProcessorAsync``2-System-Threading-Tasks-ValueTask{MQContract-CQRS-Interfaces-ICQRSConnection},MQContract-CQRS-Interfaces-Query-IQueryProcessor{``0,``1},System-String-'></a>
### RegisterQueryProcessorAsync\`\`2(cqrsConnectionTask,processor,group) `method`

##### Summary

Register a Query processor

##### Returns

The underlying CQRS connection

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| cqrsConnectionTask | [System.Threading.Tasks.ValueTask{MQContract.CQRS.Interfaces.ICQRSConnection}](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Threading.Tasks.ValueTask 'System.Threading.Tasks.ValueTask{MQContract.CQRS.Interfaces.ICQRSConnection}') | The previous registration call task |
| processor | [MQContract.CQRS.Interfaces.Query.IQueryProcessor{\`\`0,\`\`1}](#T-MQContract-CQRS-Interfaces-Query-IQueryProcessor{``0,``1} 'MQContract.CQRS.Interfaces.Query.IQueryProcessor{``0,``1}') | The query processor to register |
| group | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | The group name to assign it if desired |

##### Generic Types

| Name | Description |
| ---- | ----------- |
| TQuery | The type of Query the processor handles |
| TQueryResponse | The type of response for the query call |

<a name='T-MQContract-CQRS-Interfaces-Command-ICommand'></a>
## ICommand `type`

##### Namespace

MQContract.CQRS.Interfaces.Command

##### Summary

Used to identify a command type

<a name='T-MQContract-CQRS-Interfaces-Command-ICommandInvocationContext`1'></a>
## ICommandInvocationContext\`1 `type`

##### Namespace

MQContract.CQRS.Interfaces.Command

##### Summary

Represents a given execution context for a command

##### Generic Types

| Name | Description |
| ---- | ----------- |
| TCommand | The type of command housed within this context |

<a name='P-MQContract-CQRS-Interfaces-Command-ICommandInvocationContext`1-Command'></a>
### Command `property`

##### Summary

The command for this invocation context instance

<a name='T-MQContract-CQRS-Interfaces-Command-ICommandProcessor`1'></a>
## ICommandProcessor\`1 `type`

##### Namespace

MQContract.CQRS.Interfaces.Command

##### Summary

Defines a command processor for the given type of command

##### Generic Types

| Name | Description |
| ---- | ----------- |
| TCommand | The type of command |

<a name='M-MQContract-CQRS-Interfaces-Command-ICommandProcessor`1-ProcessCommandAsync-MQContract-CQRS-Interfaces-Command-ICommandInvocationContext{`0},System-Threading-CancellationToken-'></a>
### ProcessCommandAsync(invocationContext,cancellationToken) `method`

##### Summary

The callback executed against the command

##### Returns

A ValueTask for async purposes

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| invocationContext | [MQContract.CQRS.Interfaces.Command.ICommandInvocationContext{\`0}](#T-MQContract-CQRS-Interfaces-Command-ICommandInvocationContext{`0} 'MQContract.CQRS.Interfaces.Command.ICommandInvocationContext{`0}') | The current invocation context for this command instance |
| cancellationToken | [System.Threading.CancellationToken](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Threading.CancellationToken 'System.Threading.CancellationToken') | A cancellation token |

<a name='T-MQContract-CQRS-Interfaces-Command-ICommandProcessor`2'></a>
## ICommandProcessor\`2 `type`

##### Namespace

MQContract.CQRS.Interfaces.Command

##### Summary

Defines a command processor for the given type of command that expects a response

##### Generic Types

| Name | Description |
| ---- | ----------- |
| TCommand | The type of command |
| TCommandResult | The type of response from the command |

<a name='M-MQContract-CQRS-Interfaces-Command-ICommandProcessor`2-ProcessCommandAsync-MQContract-CQRS-Interfaces-Command-ICommandInvocationContext{`0},System-Threading-CancellationToken-'></a>
### ProcessCommandAsync(invocationContext,cancellationToken) `method`

##### Summary

The callback executed against the command

##### Returns

The result from the command execution

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| invocationContext | [MQContract.CQRS.Interfaces.Command.ICommandInvocationContext{\`0}](#T-MQContract-CQRS-Interfaces-Command-ICommandInvocationContext{`0} 'MQContract.CQRS.Interfaces.Command.ICommandInvocationContext{`0}') | The current invocation context for this command instance |
| cancellationToken | [System.Threading.CancellationToken](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Threading.CancellationToken 'System.Threading.CancellationToken') | A cancellation token |

<a name='T-MQContract-CQRS-Interfaces-Command-ICommand`1'></a>
## ICommand\`1 `type`

##### Namespace

MQContract.CQRS.Interfaces.Command

##### Summary

Used to identify a command type that is expected to provide a response

##### Generic Types

| Name | Description |
| ---- | ----------- |
| TCommandResult | The type of response expected from this command |

<a name='T-MQContract-CQRS-Interfaces-IContextFilteredProcessor'></a>
## IContextFilteredProcessor `type`

##### Namespace

MQContract.CQRS.Interfaces

##### Summary

Used to define a processor that will filter incoming calls based on the context

<a name='P-MQContract-CQRS-Interfaces-IContextFilteredProcessor-Filter'></a>
### Filter `property`

##### Summary

The filter callback that will be supplied a context instance and will return a 
filter type response

<a name='T-MQContract-CQRS-Extensions-IContractConnectionExtension'></a>
## IContractConnectionExtension `type`

##### Namespace

MQContract.CQRS.Extensions

##### Summary

Houses extension methods for the CQRS connections linked to a Contract Connection

<a name='M-MQContract-CQRS-Extensions-IContractConnectionExtension-CreateCQRSConnection-MQContract-Interfaces-IContractConnection,System-String-'></a>
### CreateCQRSConnection(contractConnection,cancelationTokenChannel) `method`

##### Summary

Creates a CQRS connection instance linked to the given contract connection
WARNING:  THe Contract Connection cannot be a MultiService style connection, it only supports the single instance or mapped.

##### Returns



##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| contractConnection | [MQContract.Interfaces.IContractConnection](#T-MQContract-Interfaces-IContractConnection 'MQContract.Interfaces.IContractConnection') | The contract connection it will be linked to. |
| cancelationTokenChannel | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | The channel to use for distributing cancelling token Cancel calls |

<a name='T-MQContract-CQRS-Interfaces-Command-IFilteredCommandProcessor`1'></a>
## IFilteredCommandProcessor\`1 `type`

##### Namespace

MQContract.CQRS.Interfaces.Command

##### Summary

Used to define a command processor that will filter incoming messages based on the command

##### Generic Types

| Name | Description |
| ---- | ----------- |
| TCommand | The type of command that this processor handles |

<a name='P-MQContract-CQRS-Interfaces-Command-IFilteredCommandProcessor`1-Filter'></a>
### Filter `property`

##### Summary

The filter callback expected to return a filter result and will be supplied the current 
context and command instance

<a name='T-MQContract-CQRS-Interfaces-Query-IFilteredQueryProcessor`2'></a>
## IFilteredQueryProcessor\`2 `type`

##### Namespace

MQContract.CQRS.Interfaces.Query

##### Summary

Used to define a query processor that will filter incoming messages based on the query

##### Generic Types

| Name | Description |
| ---- | ----------- |
| TQuery | The type of query that this processor handles |
| TQueryResponse | The type of response that this query processor provides |

<a name='P-MQContract-CQRS-Interfaces-Query-IFilteredQueryProcessor`2-Filter'></a>
### Filter `property`

##### Summary

The filter callback expected to return a filter result and will be supplied the current 
context and query instance

<a name='T-MQContract-CQRS-Interfaces-IInvocationContext'></a>
## IInvocationContext `type`

##### Namespace

MQContract.CQRS.Interfaces

##### Summary

Defines an invocation context for a given message and or query instance

<a name='P-MQContract-CQRS-Interfaces-IInvocationContext-Activity'></a>
### Activity `property`

##### Summary

The underlying activity used with OTEL.  This will be set if the underlying Contract Connection being used had OTEL enabled.

<a name='P-MQContract-CQRS-Interfaces-IInvocationContext-CausationId'></a>
### CausationId `property`

##### Summary

The id of a source message if there is one through call chaining

<a name='P-MQContract-CQRS-Interfaces-IInvocationContext-CorrelationId'></a>
### CorrelationId `property`

##### Summary

The unique id for the given message chain

<a name='P-MQContract-CQRS-Interfaces-IInvocationContext-Item-System-String-'></a>
### Item `property`

##### Summary

Used to set and get context specific data (current invocation context)

##### Returns

The stored context value for the given key or null when missing

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| key | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | The key for the data to retrieve |

<a name='P-MQContract-CQRS-Interfaces-IInvocationContext-Keys'></a>
### Keys `property`

##### Summary

The available keys that exist in this context

<a name='P-MQContract-CQRS-Interfaces-IInvocationContext-MessageId'></a>
### MessageId `property`

##### Summary

The unique id for the given message

<a name='M-MQContract-CQRS-Interfaces-IInvocationContext-ExecuteCommandAsync``1-``0-'></a>
### ExecuteCommandAsync\`\`1(command) `method`

##### Summary

Called to execute a command of the given type through the current context

##### Returns

A ValueTask to allow for async execution

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| command | [\`\`0](#T-``0 '``0') | The command to execute |

##### Generic Types

| Name | Description |
| ---- | ----------- |
| TCommand | The type of command to execute |

<a name='M-MQContract-CQRS-Interfaces-IInvocationContext-ExecuteCommandAsync``2-``0,System-Nullable{System-TimeSpan}-'></a>
### ExecuteCommandAsync\`\`2(command,timeout) `method`

##### Summary

Called to execute a command of the given type that is expected to provide a given response through the current context

##### Returns

The exepected result type

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| command | [\`\`0](#T-``0 '``0') | The command to execute |
| timeout | [System.Nullable{System.TimeSpan}](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Nullable 'System.Nullable{System.TimeSpan}') | The timeout to allow for the execution, if not specified the underlying contract connection defaults will apply |

##### Generic Types

| Name | Description |
| ---- | ----------- |
| TCommand | The type of command to execute |
| TCommandResult | The type of response to expect |

<a name='M-MQContract-CQRS-Interfaces-IInvocationContext-ExecuteQueryAsync``2-``0,System-Nullable{System-TimeSpan}-'></a>
### ExecuteQueryAsync\`\`2(query,timeout) `method`

##### Summary

Called to execute a query of the given type through the current context

##### Returns

The exepected result type

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| query | [\`\`0](#T-``0 '``0') | The query to execute |
| timeout | [System.Nullable{System.TimeSpan}](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Nullable 'System.Nullable{System.TimeSpan}') | The timeout to allow for the execution, if not specified the underlying contract connection defaults will apply |

##### Generic Types

| Name | Description |
| ---- | ----------- |
| TQuery | The type of query to execute |
| TQueryResponse | The type of response to expect |

<a name='T-MQContract-CQRS-Interfaces-IProcessor'></a>
## IProcessor `type`

##### Namespace

MQContract.CQRS.Interfaces

##### Summary

The base interface housing common calls for a Processor

<a name='M-MQContract-CQRS-Interfaces-IProcessor-ErrorRecieved-System-Exception-'></a>
### ErrorRecieved(error) `method`

##### Summary

Called when an error is supplied from the underlying Contract Connection

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| error | [System.Exception](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Exception 'System.Exception') | The error that occured |

<a name='T-MQContract-CQRS-Interfaces-Query-IQuery'></a>
## IQuery `type`

##### Namespace

MQContract.CQRS.Interfaces.Query

##### Summary

Used to identify a query type

<a name='T-MQContract-CQRS-Interfaces-Query-IQueryInvocationContext`1'></a>
## IQueryInvocationContext\`1 `type`

##### Namespace

MQContract.CQRS.Interfaces.Query

##### Summary

Represents a given execution context for a query

##### Generic Types

| Name | Description |
| ---- | ----------- |
| TQuery | The type of query housed within this context |

<a name='P-MQContract-CQRS-Interfaces-Query-IQueryInvocationContext`1-Query'></a>
### Query `property`

##### Summary

The query for this invocation context instance

<a name='T-MQContract-CQRS-Interfaces-Query-IQueryProcessor`2'></a>
## IQueryProcessor\`2 `type`

##### Namespace

MQContract.CQRS.Interfaces.Query

##### Summary

Defines a query processor for the given type of query that expects the given response

##### Generic Types

| Name | Description |
| ---- | ----------- |
| TQuery | The type of query |
| TQueryResponse | The type of response from the query |

<a name='M-MQContract-CQRS-Interfaces-Query-IQueryProcessor`2-ProcessQueryAsync-MQContract-CQRS-Interfaces-Query-IQueryInvocationContext{`0},System-Threading-CancellationToken-'></a>
### ProcessQueryAsync(invocationContext,cancellationToken) `method`

##### Summary

the callback executed against the query

##### Returns

The result from the query execution

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| invocationContext | [MQContract.CQRS.Interfaces.Query.IQueryInvocationContext{\`0}](#T-MQContract-CQRS-Interfaces-Query-IQueryInvocationContext{`0} 'MQContract.CQRS.Interfaces.Query.IQueryInvocationContext{`0}') | The current invocation context for this query instance |
| cancellationToken | [System.Threading.CancellationToken](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Threading.CancellationToken 'System.Threading.CancellationToken') | A cancellation token |

<a name='T-MQContract-CQRS-InvalidConnectionException'></a>
## InvalidConnectionException `type`

##### Namespace

MQContract.CQRS

##### Summary

Thrown when an invalid contract connection type is supplied in an attempt to create a CQRS connection

<a name='T-MQContract-CQRS-Attributes-QueryAttribute'></a>
## QueryAttribute `type`

##### Namespace

MQContract.CQRS.Attributes

##### Summary

Use this attribute to specify the Channel, TypeName, TypeVersion, Response Channel and or Response timeout 
for the Query being defined

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| channel | [T:MQContract.CQRS.Attributes.QueryAttribute](#T-T-MQContract-CQRS-Attributes-QueryAttribute 'T:MQContract.CQRS.Attributes.QueryAttribute') | The channel to be used |

<a name='M-MQContract-CQRS-Attributes-QueryAttribute-#ctor-System-String,System-String,System-String,System-String,System-Int32-'></a>
### #ctor(channel,typeName,typeVersion,responseChannel,responseTimeoutMilliseconds) `constructor`

##### Summary

Use this attribute to specify the Channel, TypeName, TypeVersion, Response Channel and or Response timeout 
for the Query being defined

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| channel | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | The channel to be used |
| typeName | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | The query type to use |
| typeVersion | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | The query type version to use |
| responseChannel | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | The responce channel to be used when an underlying service connection does not support QueryResponse or Inbox |
| responseTimeoutMilliseconds | [System.Int32](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Int32 'System.Int32') | The query response timeout to default to |

<a name='T-MQContract-CQRS-QueryCallException'></a>
## QueryCallException `type`

##### Namespace

MQContract.CQRS

##### Summary

Thrown when a query execution call throws an error from the underlying contract connection

<a name='P-MQContract-CQRS-QueryCallException-Error'></a>
### Error `property`

##### Summary

The error that occured while attempting to execute a given query

<a name='assembly'></a>
# MQContract

## Contents

- [ChannelMapper](#T-MQContract-ChannelMapper 'MQContract.ChannelMapper')
  - [AddDefaultPublishMap(mapFunction)](#M-MQContract-ChannelMapper-AddDefaultPublishMap-System-Func{System-String,System-Threading-Tasks-ValueTask{System-String}}- 'MQContract.ChannelMapper.AddDefaultPublishMap(System.Func{System.String,System.Threading.Tasks.ValueTask{System.String}})')
  - [AddDefaultPublishSubscriptionMap(mapFunction)](#M-MQContract-ChannelMapper-AddDefaultPublishSubscriptionMap-System-Func{System-String,System-Threading-Tasks-ValueTask{System-String}}- 'MQContract.ChannelMapper.AddDefaultPublishSubscriptionMap(System.Func{System.String,System.Threading.Tasks.ValueTask{System.String}})')
  - [AddDefaultQueryMap(mapFunction)](#M-MQContract-ChannelMapper-AddDefaultQueryMap-System-Func{System-String,System-Threading-Tasks-ValueTask{System-String}}- 'MQContract.ChannelMapper.AddDefaultQueryMap(System.Func{System.String,System.Threading.Tasks.ValueTask{System.String}})')
  - [AddDefaultQueryResponseMap(mapFunction)](#M-MQContract-ChannelMapper-AddDefaultQueryResponseMap-System-Func{System-String,System-Threading-Tasks-ValueTask{System-String}}- 'MQContract.ChannelMapper.AddDefaultQueryResponseMap(System.Func{System.String,System.Threading.Tasks.ValueTask{System.String}})')
  - [AddDefaultQuerySubscriptionMap(mapFunction)](#M-MQContract-ChannelMapper-AddDefaultQuerySubscriptionMap-System-Func{System-String,System-Threading-Tasks-ValueTask{System-String}}- 'MQContract.ChannelMapper.AddDefaultQuerySubscriptionMap(System.Func{System.String,System.Threading.Tasks.ValueTask{System.String}})')
  - [AddPublishMap(originalChannel,newChannel)](#M-MQContract-ChannelMapper-AddPublishMap-System-String,System-String- 'MQContract.ChannelMapper.AddPublishMap(System.String,System.String)')
  - [AddPublishMap(originalChannel,mapFunction)](#M-MQContract-ChannelMapper-AddPublishMap-System-String,System-Func{System-String,System-Threading-Tasks-ValueTask{System-String}}- 'MQContract.ChannelMapper.AddPublishMap(System.String,System.Func{System.String,System.Threading.Tasks.ValueTask{System.String}})')
  - [AddPublishMap(isMatch,mapFunction)](#M-MQContract-ChannelMapper-AddPublishMap-System-Func{System-String,System-Boolean},System-Func{System-String,System-Threading-Tasks-ValueTask{System-String}}- 'MQContract.ChannelMapper.AddPublishMap(System.Func{System.String,System.Boolean},System.Func{System.String,System.Threading.Tasks.ValueTask{System.String}})')
  - [AddPublishSubscriptionMap(originalChannel,newChannel)](#M-MQContract-ChannelMapper-AddPublishSubscriptionMap-System-String,System-String- 'MQContract.ChannelMapper.AddPublishSubscriptionMap(System.String,System.String)')
  - [AddPublishSubscriptionMap(originalChannel,mapFunction)](#M-MQContract-ChannelMapper-AddPublishSubscriptionMap-System-String,System-Func{System-String,System-Threading-Tasks-ValueTask{System-String}}- 'MQContract.ChannelMapper.AddPublishSubscriptionMap(System.String,System.Func{System.String,System.Threading.Tasks.ValueTask{System.String}})')
  - [AddPublishSubscriptionMap(isMatch,mapFunction)](#M-MQContract-ChannelMapper-AddPublishSubscriptionMap-System-Func{System-String,System-Boolean},System-Func{System-String,System-Threading-Tasks-ValueTask{System-String}}- 'MQContract.ChannelMapper.AddPublishSubscriptionMap(System.Func{System.String,System.Boolean},System.Func{System.String,System.Threading.Tasks.ValueTask{System.String}})')
  - [AddQueryMap(originalChannel,newChannel)](#M-MQContract-ChannelMapper-AddQueryMap-System-String,System-String- 'MQContract.ChannelMapper.AddQueryMap(System.String,System.String)')
  - [AddQueryMap(originalChannel,mapFunction)](#M-MQContract-ChannelMapper-AddQueryMap-System-String,System-Func{System-String,System-Threading-Tasks-ValueTask{System-String}}- 'MQContract.ChannelMapper.AddQueryMap(System.String,System.Func{System.String,System.Threading.Tasks.ValueTask{System.String}})')
  - [AddQueryMap(isMatch,mapFunction)](#M-MQContract-ChannelMapper-AddQueryMap-System-Func{System-String,System-Boolean},System-Func{System-String,System-Threading-Tasks-ValueTask{System-String}}- 'MQContract.ChannelMapper.AddQueryMap(System.Func{System.String,System.Boolean},System.Func{System.String,System.Threading.Tasks.ValueTask{System.String}})')
  - [AddQueryResponseMap(originalChannel,newChannel)](#M-MQContract-ChannelMapper-AddQueryResponseMap-System-String,System-String- 'MQContract.ChannelMapper.AddQueryResponseMap(System.String,System.String)')
  - [AddQueryResponseMap(originalChannel,mapFunction)](#M-MQContract-ChannelMapper-AddQueryResponseMap-System-String,System-Func{System-String,System-Threading-Tasks-ValueTask{System-String}}- 'MQContract.ChannelMapper.AddQueryResponseMap(System.String,System.Func{System.String,System.Threading.Tasks.ValueTask{System.String}})')
  - [AddQueryResponseMap(isMatch,mapFunction)](#M-MQContract-ChannelMapper-AddQueryResponseMap-System-Func{System-String,System-Boolean},System-Func{System-String,System-Threading-Tasks-ValueTask{System-String}}- 'MQContract.ChannelMapper.AddQueryResponseMap(System.Func{System.String,System.Boolean},System.Func{System.String,System.Threading.Tasks.ValueTask{System.String}})')
  - [AddQuerySubscriptionMap(originalChannel,newChannel)](#M-MQContract-ChannelMapper-AddQuerySubscriptionMap-System-String,System-String- 'MQContract.ChannelMapper.AddQuerySubscriptionMap(System.String,System.String)')
  - [AddQuerySubscriptionMap(originalChannel,mapFunction)](#M-MQContract-ChannelMapper-AddQuerySubscriptionMap-System-String,System-Func{System-String,System-Threading-Tasks-ValueTask{System-String}}- 'MQContract.ChannelMapper.AddQuerySubscriptionMap(System.String,System.Func{System.String,System.Threading.Tasks.ValueTask{System.String}})')
  - [AddQuerySubscriptionMap(isMatch,mapFunction)](#M-MQContract-ChannelMapper-AddQuerySubscriptionMap-System-Func{System-String,System-Boolean},System-Func{System-String,System-Threading-Tasks-ValueTask{System-String}}- 'MQContract.ChannelMapper.AddQuerySubscriptionMap(System.Func{System.String,System.Boolean},System.Func{System.String,System.Threading.Tasks.ValueTask{System.String}})')
- [ConsumerRegistrationFailedException](#T-MQContract-ConsumerRegistrationFailedException 'MQContract.ConsumerRegistrationFailedException')
- [Consuming](#T-MQContract-Logging-EventIds-Consuming 'MQContract.Logging.EventIds.Consuming')
  - [AcknowledgingServiceMessage](#F-MQContract-Logging-EventIds-Consuming-AcknowledgingServiceMessage 'MQContract.Logging.EventIds.Consuming.AcknowledgingServiceMessage')
  - [CancellingSubscriptionToken](#F-MQContract-Logging-EventIds-Consuming-CancellingSubscriptionToken 'MQContract.Logging.EventIds.Consuming.CancellingSubscriptionToken')
  - [ProcessingServiceMessage](#F-MQContract-Logging-EventIds-Consuming-ProcessingServiceMessage 'MQContract.Logging.EventIds.Consuming.ProcessingServiceMessage')
  - [ProcessingServiceMessageFailed](#F-MQContract-Logging-EventIds-Consuming-ProcessingServiceMessageFailed 'MQContract.Logging.EventIds.Consuming.ProcessingServiceMessageFailed')
  - [ReleasingMessageWait](#F-MQContract-Logging-EventIds-Consuming-ReleasingMessageWait 'MQContract.Logging.EventIds.Consuming.ReleasingMessageWait')
  - [ReturningErrorMessage](#F-MQContract-Logging-EventIds-Consuming-ReturningErrorMessage 'MQContract.Logging.EventIds.Consuming.ReturningErrorMessage')
  - [ReturningValidResponse](#F-MQContract-Logging-EventIds-Consuming-ReturningValidResponse 'MQContract.Logging.EventIds.Consuming.ReturningValidResponse')
  - [WaitingToProcessMessage](#F-MQContract-Logging-EventIds-Consuming-WaitingToProcessMessage 'MQContract.Logging.EventIds.Consuming.WaitingToProcessMessage')
- [ContractConnection](#T-MQContract-ContractConnection 'MQContract.ContractConnection')
  - [Instance(serviceConnection,defaultMessageEncoder,defaultMessageEncryptor,serviceProvider,logger,channelMapper)](#M-MQContract-ContractConnection-Instance-MQContract-Interfaces-Service-IMessageServiceConnection,MQContract-Interfaces-Encoding-IMessageEncoder,MQContract-Interfaces-Encrypting-IMessageEncryptor,System-IServiceProvider,Microsoft-Extensions-Logging-ILogger,MQContract-ChannelMapper- 'MQContract.ContractConnection.Instance(MQContract.Interfaces.Service.IMessageServiceConnection,MQContract.Interfaces.Encoding.IMessageEncoder,MQContract.Interfaces.Encrypting.IMessageEncryptor,System.IServiceProvider,Microsoft.Extensions.Logging.ILogger,MQContract.ChannelMapper)')
  - [MappedServiceInstance(defaultMessageEncoder,defaultMessageEncryptor,serviceProvider,logger,channelMapper)](#M-MQContract-ContractConnection-MappedServiceInstance-MQContract-Interfaces-Encoding-IMessageEncoder,MQContract-Interfaces-Encrypting-IMessageEncryptor,System-IServiceProvider,Microsoft-Extensions-Logging-ILogger,MQContract-ChannelMapper- 'MQContract.ContractConnection.MappedServiceInstance(MQContract.Interfaces.Encoding.IMessageEncoder,MQContract.Interfaces.Encrypting.IMessageEncryptor,System.IServiceProvider,Microsoft.Extensions.Logging.ILogger,MQContract.ChannelMapper)')
  - [MultiServiceInstance(defaultMessageEncoder,defaultMessageEncryptor,serviceProvider,logger,channelMapper)](#M-MQContract-ContractConnection-MultiServiceInstance-MQContract-Interfaces-Encoding-IMessageEncoder,MQContract-Interfaces-Encrypting-IMessageEncryptor,System-IServiceProvider,Microsoft-Extensions-Logging-ILogger,MQContract-ChannelMapper- 'MQContract.ContractConnection.MultiServiceInstance(MQContract.Interfaces.Encoding.IMessageEncoder,MQContract.Interfaces.Encrypting.IMessageEncryptor,System.IServiceProvider,Microsoft.Extensions.Logging.ILogger,MQContract.ChannelMapper)')
- [DynamicCodeNotSupportedException](#T-MQContract-DynamicCodeNotSupportedException 'MQContract.DynamicCodeNotSupportedException')
- [EventIds](#T-MQContract-Logging-EventIds 'MQContract.Logging.EventIds')
- [InvalidConsumerTypeException](#T-MQContract-InvalidConsumerTypeException 'MQContract.InvalidConsumerTypeException')
- [InvalidMiddlewareException](#T-MQContract-InvalidMiddlewareException 'MQContract.InvalidMiddlewareException')
- [InvalidPolicyArgumentsException](#T-MQContract-InvalidPolicyArgumentsException 'MQContract.InvalidPolicyArgumentsException')
- [InvalidQueryResponseMessageReceivedException](#T-MQContract-InvalidQueryResponseMessageReceivedException 'MQContract.InvalidQueryResponseMessageReceivedException')
- [InvalidRetryCircuitBreakTriggersException](#T-MQContract-InvalidRetryCircuitBreakTriggersException 'MQContract.InvalidRetryCircuitBreakTriggersException')
- [Lifetime](#T-MQContract-Logging-EventIds-Lifetime 'MQContract.Logging.EventIds.Lifetime')
  - [ClosingAllInboxes](#F-MQContract-Logging-EventIds-Lifetime-ClosingAllInboxes 'MQContract.Logging.EventIds.Lifetime.ClosingAllInboxes')
  - [ClosingConnection](#F-MQContract-Logging-EventIds-Lifetime-ClosingConnection 'MQContract.Logging.EventIds.Lifetime.ClosingConnection')
  - [ConstructingQueryResponseSubscription](#F-MQContract-Logging-EventIds-Lifetime-ConstructingQueryResponseSubscription 'MQContract.Logging.EventIds.Lifetime.ConstructingQueryResponseSubscription')
  - [ConsumerRegistrationFailed](#F-MQContract-Logging-EventIds-Lifetime-ConsumerRegistrationFailed 'MQContract.Logging.EventIds.Lifetime.ConsumerRegistrationFailed')
  - [CreatingPubSubSubscription](#F-MQContract-Logging-EventIds-Lifetime-CreatingPubSubSubscription 'MQContract.Logging.EventIds.Lifetime.CreatingPubSubSubscription')
  - [CreatingQueryResponseSubscription](#F-MQContract-Logging-EventIds-Lifetime-CreatingQueryResponseSubscription 'MQContract.Logging.EventIds.Lifetime.CreatingQueryResponseSubscription')
  - [DisposingQueryResponseSubscription](#F-MQContract-Logging-EventIds-Lifetime-DisposingQueryResponseSubscription 'MQContract.Logging.EventIds.Lifetime.DisposingQueryResponseSubscription')
  - [EnablingMetricMiddleware](#F-MQContract-Logging-EventIds-Lifetime-EnablingMetricMiddleware 'MQContract.Logging.EventIds.Lifetime.EnablingMetricMiddleware')
  - [EndingSubscription](#F-MQContract-Logging-EventIds-Lifetime-EndingSubscription 'MQContract.Logging.EventIds.Lifetime.EndingSubscription')
  - [EstablishingInboxSubscription](#F-MQContract-Logging-EventIds-Lifetime-EstablishingInboxSubscription 'MQContract.Logging.EventIds.Lifetime.EstablishingInboxSubscription')
  - [EstablishingNewInboxSubscription](#F-MQContract-Logging-EventIds-Lifetime-EstablishingNewInboxSubscription 'MQContract.Logging.EventIds.Lifetime.EstablishingNewInboxSubscription')
  - [EstablishingPubSubServiceSubscription](#F-MQContract-Logging-EventIds-Lifetime-EstablishingPubSubServiceSubscription 'MQContract.Logging.EventIds.Lifetime.EstablishingPubSubServiceSubscription')
  - [EstablishingPubSubServiceSubscriptionSucceeded](#F-MQContract-Logging-EventIds-Lifetime-EstablishingPubSubServiceSubscriptionSucceeded 'MQContract.Logging.EventIds.Lifetime.EstablishingPubSubServiceSubscriptionSucceeded')
  - [EstablishingPubSubSubscription](#F-MQContract-Logging-EventIds-Lifetime-EstablishingPubSubSubscription 'MQContract.Logging.EventIds.Lifetime.EstablishingPubSubSubscription')
  - [EstablishingQueryResponseServiceSubscription](#F-MQContract-Logging-EventIds-Lifetime-EstablishingQueryResponseServiceSubscription 'MQContract.Logging.EventIds.Lifetime.EstablishingQueryResponseServiceSubscription')
  - [EstablishingQueryResponseSubscription](#F-MQContract-Logging-EventIds-Lifetime-EstablishingQueryResponseSubscription 'MQContract.Logging.EventIds.Lifetime.EstablishingQueryResponseSubscription')
  - [EstablishingQueryResponseSubscriptionServiceSubscription](#F-MQContract-Logging-EventIds-Lifetime-EstablishingQueryResponseSubscriptionServiceSubscription 'MQContract.Logging.EventIds.Lifetime.EstablishingQueryResponseSubscriptionServiceSubscription')
  - [EstablishingQueryResponseSubscriptionWithPubSubServiceSubscription](#F-MQContract-Logging-EventIds-Lifetime-EstablishingQueryResponseSubscriptionWithPubSubServiceSubscription 'MQContract.Logging.EventIds.Lifetime.EstablishingQueryResponseSubscriptionWithPubSubServiceSubscription')
  - [PubSubSubscriptionEstablishmentFailed](#F-MQContract-Logging-EventIds-Lifetime-PubSubSubscriptionEstablishmentFailed 'MQContract.Logging.EventIds.Lifetime.PubSubSubscriptionEstablishmentFailed')
  - [QueryResponseSubscriptionDisposed](#F-MQContract-Logging-EventIds-Lifetime-QueryResponseSubscriptionDisposed 'MQContract.Logging.EventIds.Lifetime.QueryResponseSubscriptionDisposed')
  - [QueryResponseSubscriptionEstablishmentFailed](#F-MQContract-Logging-EventIds-Lifetime-QueryResponseSubscriptionEstablishmentFailed 'MQContract.Logging.EventIds.Lifetime.QueryResponseSubscriptionEstablishmentFailed')
  - [QueryResponseSubscriptionEstablishmentFailedWithError](#F-MQContract-Logging-EventIds-Lifetime-QueryResponseSubscriptionEstablishmentFailedWithError 'MQContract.Logging.EventIds.Lifetime.QueryResponseSubscriptionEstablishmentFailedWithError')
  - [QueryResponseSubscriptionEstablishmentSucceeded](#F-MQContract-Logging-EventIds-Lifetime-QueryResponseSubscriptionEstablishmentSucceeded 'MQContract.Logging.EventIds.Lifetime.QueryResponseSubscriptionEstablishmentSucceeded')
  - [RegisteringMiddleware](#F-MQContract-Logging-EventIds-Lifetime-RegisteringMiddleware 'MQContract.Logging.EventIds.Lifetime.RegisteringMiddleware')
  - [SettingUpInboxSubscription](#F-MQContract-Logging-EventIds-Lifetime-SettingUpInboxSubscription 'MQContract.Logging.EventIds.Lifetime.SettingUpInboxSubscription')
- [MessageChannelNullException](#T-MQContract-MessageChannelNullException 'MQContract.MessageChannelNullException')
- [MessageConversionException](#T-MQContract-MessageConversionException 'MQContract.MessageConversionException')
- [NoConnectionMatchException](#T-MQContract-NoConnectionMatchException 'MQContract.NoConnectionMatchException')
- [PingNotSupportedException](#T-MQContract-PingNotSupportedException 'MQContract.PingNotSupportedException')
- [Pipeline](#T-MQContract-Logging-EventIds-Pipeline 'MQContract.Logging.EventIds.Pipeline')
  - [AttemptingToProcessInboxMessage](#F-MQContract-Logging-EventIds-Pipeline-AttemptingToProcessInboxMessage 'MQContract.Logging.EventIds.Pipeline.AttemptingToProcessInboxMessage')
  - [DecodingServiceMessage](#F-MQContract-Logging-EventIds-Pipeline-DecodingServiceMessage 'MQContract.Logging.EventIds.Pipeline.DecodingServiceMessage')
  - [ExecutingAfterMessageDecodeMiddleware](#F-MQContract-Logging-EventIds-Pipeline-ExecutingAfterMessageDecodeMiddleware 'MQContract.Logging.EventIds.Pipeline.ExecutingAfterMessageDecodeMiddleware')
  - [ExecutingAfterMessageEncodeMiddleware](#F-MQContract-Logging-EventIds-Pipeline-ExecutingAfterMessageEncodeMiddleware 'MQContract.Logging.EventIds.Pipeline.ExecutingAfterMessageEncodeMiddleware')
  - [ExecutingBeforeMessageDecodeMiddleware](#F-MQContract-Logging-EventIds-Pipeline-ExecutingBeforeMessageDecodeMiddleware 'MQContract.Logging.EventIds.Pipeline.ExecutingBeforeMessageDecodeMiddleware')
  - [ExecutingBeforeMessageEncodeMiddleware](#F-MQContract-Logging-EventIds-Pipeline-ExecutingBeforeMessageEncodeMiddleware 'MQContract.Logging.EventIds.Pipeline.ExecutingBeforeMessageEncodeMiddleware')
  - [ExecutingSpecificAfterMessageDecodeMiddleware](#F-MQContract-Logging-EventIds-Pipeline-ExecutingSpecificAfterMessageDecodeMiddleware 'MQContract.Logging.EventIds.Pipeline.ExecutingSpecificAfterMessageDecodeMiddleware')
  - [ExecutingSpecificBeforeMessageEncodeMiddleware](#F-MQContract-Logging-EventIds-Pipeline-ExecutingSpecificBeforeMessageEncodeMiddleware 'MQContract.Logging.EventIds.Pipeline.ExecutingSpecificBeforeMessageEncodeMiddleware')
  - [ExtractingQueryResponseType](#F-MQContract-Logging-EventIds-Pipeline-ExtractingQueryResponseType 'MQContract.Logging.EventIds.Pipeline.ExtractingQueryResponseType')
  - [FilterServiceMessageInFull](#F-MQContract-Logging-EventIds-Pipeline-FilterServiceMessageInFull 'MQContract.Logging.EventIds.Pipeline.FilterServiceMessageInFull')
  - [FilteringServiceMessageByHeaders](#F-MQContract-Logging-EventIds-Pipeline-FilteringServiceMessageByHeaders 'MQContract.Logging.EventIds.Pipeline.FilteringServiceMessageByHeaders')
  - [ProcessingQueryResponse](#F-MQContract-Logging-EventIds-Pipeline-ProcessingQueryResponse 'MQContract.Logging.EventIds.Pipeline.ProcessingQueryResponse')
  - [ProcessingQueryResponseFailed](#F-MQContract-Logging-EventIds-Pipeline-ProcessingQueryResponseFailed 'MQContract.Logging.EventIds.Pipeline.ProcessingQueryResponseFailed')
  - [ProducingServiceMessage](#F-MQContract-Logging-EventIds-Pipeline-ProducingServiceMessage 'MQContract.Logging.EventIds.Pipeline.ProducingServiceMessage')
  - [QueryReplyChannelMapped](#F-MQContract-Logging-EventIds-Pipeline-QueryReplyChannelMapped 'MQContract.Logging.EventIds.Pipeline.QueryReplyChannelMapped')
  - [ReceivedInvalidQueryResponseMessage](#F-MQContract-Logging-EventIds-Pipeline-ReceivedInvalidQueryResponseMessage 'MQContract.Logging.EventIds.Pipeline.ReceivedInvalidQueryResponseMessage')
  - [ResilienceFailedToFallback](#F-MQContract-Logging-EventIds-Pipeline-ResilienceFailedToFallback 'MQContract.Logging.EventIds.Pipeline.ResilienceFailedToFallback')
  - [ResilienceRetryTriggered](#F-MQContract-Logging-EventIds-Pipeline-ResilienceRetryTriggered 'MQContract.Logging.EventIds.Pipeline.ResilienceRetryTriggered')
- [Publishing](#T-MQContract-Logging-EventIds-Publishing 'MQContract.Logging.EventIds.Publishing')
  - [AttemptingQueryResponse](#F-MQContract-Logging-EventIds-Publishing-AttemptingQueryResponse 'MQContract.Logging.EventIds.Publishing.AttemptingQueryResponse')
  - [AttemptingQueryResponseOnPubSubService](#F-MQContract-Logging-EventIds-Publishing-AttemptingQueryResponseOnPubSubService 'MQContract.Logging.EventIds.Publishing.AttemptingQueryResponseOnPubSubService')
  - [BulkPublishingMessages](#F-MQContract-Logging-EventIds-Publishing-BulkPublishingMessages 'MQContract.Logging.EventIds.Publishing.BulkPublishingMessages')
  - [ExecutingBulkPublish](#F-MQContract-Logging-EventIds-Publishing-ExecutingBulkPublish 'MQContract.Logging.EventIds.Publishing.ExecutingBulkPublish')
  - [ExecutingQuery](#F-MQContract-Logging-EventIds-Publishing-ExecutingQuery 'MQContract.Logging.EventIds.Publishing.ExecutingQuery')
  - [ExecutingQueryResponseOnInboxService](#F-MQContract-Logging-EventIds-Publishing-ExecutingQueryResponseOnInboxService 'MQContract.Logging.EventIds.Publishing.ExecutingQueryResponseOnInboxService')
  - [ExecutingQueryResponseOnPubSubService](#F-MQContract-Logging-EventIds-Publishing-ExecutingQueryResponseOnPubSubService 'MQContract.Logging.EventIds.Publishing.ExecutingQueryResponseOnPubSubService')
  - [ExecutingQueryResponseOnQueryResponseService](#F-MQContract-Logging-EventIds-Publishing-ExecutingQueryResponseOnQueryResponseService 'MQContract.Logging.EventIds.Publishing.ExecutingQueryResponseOnQueryResponseService')
  - [InboxQueryTimedOut](#F-MQContract-Logging-EventIds-Publishing-InboxQueryTimedOut 'MQContract.Logging.EventIds.Publishing.InboxQueryTimedOut')
  - [PubSubQueryResponseRecieved](#F-MQContract-Logging-EventIds-Publishing-PubSubQueryResponseRecieved 'MQContract.Logging.EventIds.Publishing.PubSubQueryResponseRecieved')
  - [PublishingMessage](#F-MQContract-Logging-EventIds-Publishing-PublishingMessage 'MQContract.Logging.EventIds.Publishing.PublishingMessage')
  - [QueryExceptionOccured](#F-MQContract-Logging-EventIds-Publishing-QueryExceptionOccured 'MQContract.Logging.EventIds.Publishing.QueryExceptionOccured')
  - [StartingQueryResponsePubSubListener](#F-MQContract-Logging-EventIds-Publishing-StartingQueryResponsePubSubListener 'MQContract.Logging.EventIds.Publishing.StartingQueryResponsePubSubListener')
  - [TransmittingInboxQuery](#F-MQContract-Logging-EventIds-Publishing-TransmittingInboxQuery 'MQContract.Logging.EventIds.Publishing.TransmittingInboxQuery')
  - [TransmittingInboxQueryFailed](#F-MQContract-Logging-EventIds-Publishing-TransmittingInboxQueryFailed 'MQContract.Logging.EventIds.Publishing.TransmittingInboxQueryFailed')
  - [TransmittingPubSubQuery](#F-MQContract-Logging-EventIds-Publishing-TransmittingPubSubQuery 'MQContract.Logging.EventIds.Publishing.TransmittingPubSubQuery')
  - [TransmittingPubSubQueryFailed](#F-MQContract-Logging-EventIds-Publishing-TransmittingPubSubQueryFailed 'MQContract.Logging.EventIds.Publishing.TransmittingPubSubQueryFailed')
  - [WaitingOnPubSubQueryResponse](#F-MQContract-Logging-EventIds-Publishing-WaitingOnPubSubQueryResponse 'MQContract.Logging.EventIds.Publishing.WaitingOnPubSubQueryResponse')
- [QueryExecutionFailedException](#T-MQContract-QueryExecutionFailedException 'MQContract.QueryExecutionFailedException')
- [QueryResponseException](#T-MQContract-QueryResponseException 'MQContract.QueryResponseException')
- [QueryTimeoutException](#T-MQContract-QueryTimeoutException 'MQContract.QueryTimeoutException')
- [SubscriptionFailedException](#T-MQContract-SubscriptionFailedException 'MQContract.SubscriptionFailedException')
- [TooManyConnectionMatchesException](#T-MQContract-TooManyConnectionMatchesException 'MQContract.TooManyConnectionMatchesException')
- [Transport](#T-MQContract-Logging-EventIds-Transport 'MQContract.Logging.EventIds.Transport')
  - [LocatedTooManyConnections](#F-MQContract-Logging-EventIds-Transport-LocatedTooManyConnections 'MQContract.Logging.EventIds.Transport.LocatedTooManyConnections')
  - [LocatedTooManyConnectionsForMapType](#F-MQContract-Logging-EventIds-Transport-LocatedTooManyConnectionsForMapType 'MQContract.Logging.EventIds.Transport.LocatedTooManyConnectionsForMapType')
  - [LocatingConnections](#F-MQContract-Logging-EventIds-Transport-LocatingConnections 'MQContract.Logging.EventIds.Transport.LocatingConnections')
  - [LocatingConnectionsForMapType](#F-MQContract-Logging-EventIds-Transport-LocatingConnectionsForMapType 'MQContract.Logging.EventIds.Transport.LocatingConnectionsForMapType')
  - [PingServiceConnection](#F-MQContract-Logging-EventIds-Transport-PingServiceConnection 'MQContract.Logging.EventIds.Transport.PingServiceConnection')
  - [UnableToLocateConnections](#F-MQContract-Logging-EventIds-Transport-UnableToLocateConnections 'MQContract.Logging.EventIds.Transport.UnableToLocateConnections')
- [UnknownResponseTypeException](#T-MQContract-UnknownResponseTypeException 'MQContract.UnknownResponseTypeException')

<a name='T-MQContract-ChannelMapper'></a>
## ChannelMapper `type`

##### Namespace

MQContract

##### Summary

Used to map channel names depending on the usage of the channel when necessary

<a name='M-MQContract-ChannelMapper-AddDefaultPublishMap-System-Func{System-String,System-Threading-Tasks-ValueTask{System-String}}-'></a>
### AddDefaultPublishMap(mapFunction) `method`

##### Summary

Add a default map function to call for publish calls

##### Returns

The current instance of the Channel Mapper

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| mapFunction | [System.Func{System.String,System.Threading.Tasks.ValueTask{System.String}}](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Func 'System.Func{System.String,System.Threading.Tasks.ValueTask{System.String}}') | A function to be called with the channel supplied expecting a mapped channel name |

<a name='M-MQContract-ChannelMapper-AddDefaultPublishSubscriptionMap-System-Func{System-String,System-Threading-Tasks-ValueTask{System-String}}-'></a>
### AddDefaultPublishSubscriptionMap(mapFunction) `method`

##### Summary

Add a default map function to call for pub/sub subscription calls

##### Returns

The current instance of the Channel Mapper

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| mapFunction | [System.Func{System.String,System.Threading.Tasks.ValueTask{System.String}}](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Func 'System.Func{System.String,System.Threading.Tasks.ValueTask{System.String}}') | A function to be called with the channel supplied expecting a mapped channel name |

<a name='M-MQContract-ChannelMapper-AddDefaultQueryMap-System-Func{System-String,System-Threading-Tasks-ValueTask{System-String}}-'></a>
### AddDefaultQueryMap(mapFunction) `method`

##### Summary

Add a default map function to call for query calls

##### Returns

The current instance of the Channel Mapper

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| mapFunction | [System.Func{System.String,System.Threading.Tasks.ValueTask{System.String}}](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Func 'System.Func{System.String,System.Threading.Tasks.ValueTask{System.String}}') | A function to be called with the channel supplied expecting a mapped channel name |

<a name='M-MQContract-ChannelMapper-AddDefaultQueryResponseMap-System-Func{System-String,System-Threading-Tasks-ValueTask{System-String}}-'></a>
### AddDefaultQueryResponseMap(mapFunction) `method`

##### Summary

Add a default map function to call for query/response response calls

##### Returns

The current instance of the Channel Mapper

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| mapFunction | [System.Func{System.String,System.Threading.Tasks.ValueTask{System.String}}](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Func 'System.Func{System.String,System.Threading.Tasks.ValueTask{System.String}}') | A function to be called with the channel supplied expecting a mapped channel name |

<a name='M-MQContract-ChannelMapper-AddDefaultQuerySubscriptionMap-System-Func{System-String,System-Threading-Tasks-ValueTask{System-String}}-'></a>
### AddDefaultQuerySubscriptionMap(mapFunction) `method`

##### Summary

Add a default map function to call for query/response subscription calls

##### Returns

The current instance of the Channel Mapper

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| mapFunction | [System.Func{System.String,System.Threading.Tasks.ValueTask{System.String}}](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Func 'System.Func{System.String,System.Threading.Tasks.ValueTask{System.String}}') | A function to be called with the channel supplied expecting a mapped channel name |

<a name='M-MQContract-ChannelMapper-AddPublishMap-System-String,System-String-'></a>
### AddPublishMap(originalChannel,newChannel) `method`

##### Summary

Add a direct map for publish calls

##### Returns

The current instance of the Channel Mapper

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| originalChannel | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | The original channel that is being used in the connection |
| newChannel | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | The channel to map it to |

<a name='M-MQContract-ChannelMapper-AddPublishMap-System-String,System-Func{System-String,System-Threading-Tasks-ValueTask{System-String}}-'></a>
### AddPublishMap(originalChannel,mapFunction) `method`

##### Summary

Add a map function for publish calls for a given channel

##### Returns

The current instance of the Channel Mapper

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| originalChannel | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | The original channel that is being used in the connection |
| mapFunction | [System.Func{System.String,System.Threading.Tasks.ValueTask{System.String}}](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Func 'System.Func{System.String,System.Threading.Tasks.ValueTask{System.String}}') | A function to be called with the channel supplied expecting a mapped channel name |

<a name='M-MQContract-ChannelMapper-AddPublishMap-System-Func{System-String,System-Boolean},System-Func{System-String,System-Threading-Tasks-ValueTask{System-String}}-'></a>
### AddPublishMap(isMatch,mapFunction) `method`

##### Summary

Add a map function call pair for publish calls

##### Returns

The current instance of the Channel Mapper

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| isMatch | [System.Func{System.String,System.Boolean}](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Func 'System.Func{System.String,System.Boolean}') | A callback that will return true if the supplied function will mape that channel |
| mapFunction | [System.Func{System.String,System.Threading.Tasks.ValueTask{System.String}}](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Func 'System.Func{System.String,System.Threading.Tasks.ValueTask{System.String}}') | A function to be called with the channel supplied expecting a mapped channel name |

<a name='M-MQContract-ChannelMapper-AddPublishSubscriptionMap-System-String,System-String-'></a>
### AddPublishSubscriptionMap(originalChannel,newChannel) `method`

##### Summary

Add a direct map for pub/sub subscription calls

##### Returns

The current instance of the Channel Mapper

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| originalChannel | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | The original channel that is being used in the connection |
| newChannel | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | The channel to map it to |

<a name='M-MQContract-ChannelMapper-AddPublishSubscriptionMap-System-String,System-Func{System-String,System-Threading-Tasks-ValueTask{System-String}}-'></a>
### AddPublishSubscriptionMap(originalChannel,mapFunction) `method`

##### Summary

Add a map function for pub/sub subscription calls

##### Returns

The current instance of the Channel Mapper

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| originalChannel | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | The original channel that is being used in the connection |
| mapFunction | [System.Func{System.String,System.Threading.Tasks.ValueTask{System.String}}](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Func 'System.Func{System.String,System.Threading.Tasks.ValueTask{System.String}}') | A function to be called with the channel supplied expecting a mapped channel name |

<a name='M-MQContract-ChannelMapper-AddPublishSubscriptionMap-System-Func{System-String,System-Boolean},System-Func{System-String,System-Threading-Tasks-ValueTask{System-String}}-'></a>
### AddPublishSubscriptionMap(isMatch,mapFunction) `method`

##### Summary

Add a map function call pair for pub/sub subscription calls

##### Returns

The current instance of the Channel Mapper

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| isMatch | [System.Func{System.String,System.Boolean}](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Func 'System.Func{System.String,System.Boolean}') | A callback that will return true if the supplied function will mape that channel |
| mapFunction | [System.Func{System.String,System.Threading.Tasks.ValueTask{System.String}}](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Func 'System.Func{System.String,System.Threading.Tasks.ValueTask{System.String}}') | A function to be called with the channel supplied expecting a mapped channel name |

<a name='M-MQContract-ChannelMapper-AddQueryMap-System-String,System-String-'></a>
### AddQueryMap(originalChannel,newChannel) `method`

##### Summary

Add a direct map for query calls

##### Returns

The current instance of the Channel Mapper

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| originalChannel | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | The original channel that is being used in the connection |
| newChannel | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | The channel to map it to |

<a name='M-MQContract-ChannelMapper-AddQueryMap-System-String,System-Func{System-String,System-Threading-Tasks-ValueTask{System-String}}-'></a>
### AddQueryMap(originalChannel,mapFunction) `method`

##### Summary

Add a map function for query calls for a given channel

##### Returns

The current instance of the Channel Mapper

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| originalChannel | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | The original channel that is being used in the connection |
| mapFunction | [System.Func{System.String,System.Threading.Tasks.ValueTask{System.String}}](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Func 'System.Func{System.String,System.Threading.Tasks.ValueTask{System.String}}') | A function to be called with the channel supplied expecting a mapped channel name |

<a name='M-MQContract-ChannelMapper-AddQueryMap-System-Func{System-String,System-Boolean},System-Func{System-String,System-Threading-Tasks-ValueTask{System-String}}-'></a>
### AddQueryMap(isMatch,mapFunction) `method`

##### Summary

Add a map function call pair for query calls

##### Returns

The current instance of the Channel Mapper

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| isMatch | [System.Func{System.String,System.Boolean}](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Func 'System.Func{System.String,System.Boolean}') | A callback that will return true if the supplied function will mape that channel |
| mapFunction | [System.Func{System.String,System.Threading.Tasks.ValueTask{System.String}}](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Func 'System.Func{System.String,System.Threading.Tasks.ValueTask{System.String}}') | A function to be called with the channel supplied expecting a mapped channel name |

<a name='M-MQContract-ChannelMapper-AddQueryResponseMap-System-String,System-String-'></a>
### AddQueryResponseMap(originalChannel,newChannel) `method`

##### Summary

Add a direct map for query/response response calls

##### Returns

The current instance of the Channel Mapper

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| originalChannel | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | The original channel that is being used in the connection |
| newChannel | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | The channel to map it to |

<a name='M-MQContract-ChannelMapper-AddQueryResponseMap-System-String,System-Func{System-String,System-Threading-Tasks-ValueTask{System-String}}-'></a>
### AddQueryResponseMap(originalChannel,mapFunction) `method`

##### Summary

Add a map function for query/response response calls

##### Returns

The current instance of the Channel Mapper

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| originalChannel | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | The original channel that is being used in the connection |
| mapFunction | [System.Func{System.String,System.Threading.Tasks.ValueTask{System.String}}](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Func 'System.Func{System.String,System.Threading.Tasks.ValueTask{System.String}}') | A function to be called with the channel supplied expecting a mapped channel name |

<a name='M-MQContract-ChannelMapper-AddQueryResponseMap-System-Func{System-String,System-Boolean},System-Func{System-String,System-Threading-Tasks-ValueTask{System-String}}-'></a>
### AddQueryResponseMap(isMatch,mapFunction) `method`

##### Summary

Add a map function call pair for query/response response calls

##### Returns

The current instance of the Channel Mapper

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| isMatch | [System.Func{System.String,System.Boolean}](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Func 'System.Func{System.String,System.Boolean}') | A callback that will return true if the supplied function will mape that channel |
| mapFunction | [System.Func{System.String,System.Threading.Tasks.ValueTask{System.String}}](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Func 'System.Func{System.String,System.Threading.Tasks.ValueTask{System.String}}') | A function to be called with the channel supplied expecting a mapped channel name |

<a name='M-MQContract-ChannelMapper-AddQuerySubscriptionMap-System-String,System-String-'></a>
### AddQuerySubscriptionMap(originalChannel,newChannel) `method`

##### Summary

Add a direct map for query/response subscription calls

##### Returns

The current instance of the Channel Mapper

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| originalChannel | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | The original channel that is being used in the connection |
| newChannel | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | The channel to map it to |

<a name='M-MQContract-ChannelMapper-AddQuerySubscriptionMap-System-String,System-Func{System-String,System-Threading-Tasks-ValueTask{System-String}}-'></a>
### AddQuerySubscriptionMap(originalChannel,mapFunction) `method`

##### Summary

Add a map function for query/response subscription calls

##### Returns

The current instance of the Channel Mapper

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| originalChannel | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | The original channel that is being used in the connection |
| mapFunction | [System.Func{System.String,System.Threading.Tasks.ValueTask{System.String}}](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Func 'System.Func{System.String,System.Threading.Tasks.ValueTask{System.String}}') | A function to be called with the channel supplied expecting a mapped channel name |

<a name='M-MQContract-ChannelMapper-AddQuerySubscriptionMap-System-Func{System-String,System-Boolean},System-Func{System-String,System-Threading-Tasks-ValueTask{System-String}}-'></a>
### AddQuerySubscriptionMap(isMatch,mapFunction) `method`

##### Summary

Add a map function call pair for query/response subscription calls

##### Returns

The current instance of the Channel Mapper

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| isMatch | [System.Func{System.String,System.Boolean}](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Func 'System.Func{System.String,System.Boolean}') | A callback that will return true if the supplied function will mape that channel |
| mapFunction | [System.Func{System.String,System.Threading.Tasks.ValueTask{System.String}}](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Func 'System.Func{System.String,System.Threading.Tasks.ValueTask{System.String}}') | A function to be called with the channel supplied expecting a mapped channel name |

<a name='T-MQContract-ConsumerRegistrationFailedException'></a>
## ConsumerRegistrationFailedException `type`

##### Namespace

MQContract

##### Summary

Thrown when the registration of a consumer failes

<a name='T-MQContract-Logging-EventIds-Consuming'></a>
## Consuming `type`

##### Namespace

MQContract.Logging.EventIds

##### Summary

Consumer related event id values.
Covers processing, acknowledgement and response handling while consuming messages.

<a name='F-MQContract-Logging-EventIds-Consuming-AcknowledgingServiceMessage'></a>
### AcknowledgingServiceMessage `constants`

##### Summary

Logged when a service message is acknowledged by the consumer.
(22409 - Information)

<a name='F-MQContract-Logging-EventIds-Consuming-CancellingSubscriptionToken'></a>
### CancellingSubscriptionToken `constants`

##### Summary

Logged when a subscription cancellation token is being cancelled (consumer shutting down or unsubscribing).
(22220 - Debug)

<a name='F-MQContract-Logging-EventIds-Consuming-ProcessingServiceMessage'></a>
### ProcessingServiceMessage `constants`

##### Summary

Logged when a service message is being processed by a consumer.
(22408 - Information)

<a name='F-MQContract-Logging-EventIds-Consuming-ProcessingServiceMessageFailed'></a>
### ProcessingServiceMessageFailed `constants`

##### Summary

Logged when processing a service message fails.
(52410 - Error)

<a name='F-MQContract-Logging-EventIds-Consuming-ReleasingMessageWait'></a>
### ReleasingMessageWait `constants`

##### Summary

Logged when a previously waiting message is released from its wait (ready to be processed).
(22412 - Information)

<a name='F-MQContract-Logging-EventIds-Consuming-ReturningErrorMessage'></a>
### ReturningErrorMessage `constants`

##### Summary

Logged when a consumed message results in returning an error response to the caller.
(42413 - Warning)

<a name='F-MQContract-Logging-EventIds-Consuming-ReturningValidResponse'></a>
### ReturningValidResponse `constants`

##### Summary

Logged when a consumed message results in returning a valid response to the caller.
(32414 - Information)

<a name='F-MQContract-Logging-EventIds-Consuming-WaitingToProcessMessage'></a>
### WaitingToProcessMessage `constants`

##### Summary

Logged when a message is queued / waiting to be processed.
(22411 - Information)

<a name='T-MQContract-ContractConnection'></a>
## ContractConnection `type`

##### Namespace

MQContract

##### Summary

The primary class for producing an instance of either an IContractConnection or an IMultiServiceContractConnection

<a name='M-MQContract-ContractConnection-Instance-MQContract-Interfaces-Service-IMessageServiceConnection,MQContract-Interfaces-Encoding-IMessageEncoder,MQContract-Interfaces-Encrypting-IMessageEncryptor,System-IServiceProvider,Microsoft-Extensions-Logging-ILogger,MQContract-ChannelMapper-'></a>
### Instance(serviceConnection,defaultMessageEncoder,defaultMessageEncryptor,serviceProvider,logger,channelMapper) `method`

##### Summary

This is the call used to create an instance of a Contract Connection which will return the Interface

##### Returns

An instance of IContractConnection

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| serviceConnection | [MQContract.Interfaces.Service.IMessageServiceConnection](#T-MQContract-Interfaces-Service-IMessageServiceConnection 'MQContract.Interfaces.Service.IMessageServiceConnection') | The service connection implementation to use for the underlying message requests. |
| defaultMessageEncoder | [MQContract.Interfaces.Encoding.IMessageEncoder](#T-MQContract-Interfaces-Encoding-IMessageEncoder 'MQContract.Interfaces.Encoding.IMessageEncoder') | A default message encoder implementation if desired.  If there is no specific encoder for a given type, this encoder would be called.  The built in default being used dotnet Json serializer. |
| defaultMessageEncryptor | [MQContract.Interfaces.Encrypting.IMessageEncryptor](#T-MQContract-Interfaces-Encrypting-IMessageEncryptor 'MQContract.Interfaces.Encrypting.IMessageEncryptor') | A default message encryptor implementation if desired.  If there is no specific encryptor |
| serviceProvider | [System.IServiceProvider](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.IServiceProvider 'System.IServiceProvider') | A service prodivder instance supplied in the case that dependency injection might be necessary |
| logger | [Microsoft.Extensions.Logging.ILogger](#T-Microsoft-Extensions-Logging-ILogger 'Microsoft.Extensions.Logging.ILogger') | An instance of a logger if logging is desired |
| channelMapper | [MQContract.ChannelMapper](#T-MQContract-ChannelMapper 'MQContract.ChannelMapper') | An instance of a ChannelMapper used to translate channels from one instance to another based on class channel attributes or supplied channels if necessary.
For example, it might be necessary for a Nats.IO instance when you are trying to read from a stored message stream that is comprised of another channel or set of channels |

##### Example

```
using MQContract.InMemory;
var serviceConnection = new Connection();
var contractConnection = ContractConnection.Instance(serviceConnection);
```

<a name='M-MQContract-ContractConnection-MappedServiceInstance-MQContract-Interfaces-Encoding-IMessageEncoder,MQContract-Interfaces-Encrypting-IMessageEncryptor,System-IServiceProvider,Microsoft-Extensions-Logging-ILogger,MQContract-ChannelMapper-'></a>
### MappedServiceInstance(defaultMessageEncoder,defaultMessageEncryptor,serviceProvider,logger,channelMapper) `method`

##### Summary

This is the call used to create an instance of a Mapped Service Contract Connection which will return the Interface

##### Returns

An instance of IMultiServiceContractConnection

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| defaultMessageEncoder | [MQContract.Interfaces.Encoding.IMessageEncoder](#T-MQContract-Interfaces-Encoding-IMessageEncoder 'MQContract.Interfaces.Encoding.IMessageEncoder') | A default message encoder implementation if desired.  If there is no specific encoder for a given type, this encoder would be called.  The built in default being used dotnet Json serializer. |
| defaultMessageEncryptor | [MQContract.Interfaces.Encrypting.IMessageEncryptor](#T-MQContract-Interfaces-Encrypting-IMessageEncryptor 'MQContract.Interfaces.Encrypting.IMessageEncryptor') | A default message encryptor implementation if desired.  If there is no specific encryptor |
| serviceProvider | [System.IServiceProvider](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.IServiceProvider 'System.IServiceProvider') | A service prodivder instance supplied in the case that dependency injection might be necessary |
| logger | [Microsoft.Extensions.Logging.ILogger](#T-Microsoft-Extensions-Logging-ILogger 'Microsoft.Extensions.Logging.ILogger') | An instance of a logger if logging is desired |
| channelMapper | [MQContract.ChannelMapper](#T-MQContract-ChannelMapper 'MQContract.ChannelMapper') | An instance of a ChannelMapper used to translate channels from one instance to another based on class channel attributes or supplied channels if necessary.
For example, it might be necessary for a Nats.IO instance when you are trying to read from a stored message stream that is comprised of another channel or set of channels |

<a name='M-MQContract-ContractConnection-MultiServiceInstance-MQContract-Interfaces-Encoding-IMessageEncoder,MQContract-Interfaces-Encrypting-IMessageEncryptor,System-IServiceProvider,Microsoft-Extensions-Logging-ILogger,MQContract-ChannelMapper-'></a>
### MultiServiceInstance(defaultMessageEncoder,defaultMessageEncryptor,serviceProvider,logger,channelMapper) `method`

##### Summary

This is the call used to create an instance of a Multi Service Contract Connection which will return the Interface

##### Returns

An instance of IMultiServiceContractConnection

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| defaultMessageEncoder | [MQContract.Interfaces.Encoding.IMessageEncoder](#T-MQContract-Interfaces-Encoding-IMessageEncoder 'MQContract.Interfaces.Encoding.IMessageEncoder') | A default message encoder implementation if desired.  If there is no specific encoder for a given type, this encoder would be called.  The built in default being used dotnet Json serializer. |
| defaultMessageEncryptor | [MQContract.Interfaces.Encrypting.IMessageEncryptor](#T-MQContract-Interfaces-Encrypting-IMessageEncryptor 'MQContract.Interfaces.Encrypting.IMessageEncryptor') | A default message encryptor implementation if desired.  If there is no specific encryptor |
| serviceProvider | [System.IServiceProvider](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.IServiceProvider 'System.IServiceProvider') | A service prodivder instance supplied in the case that dependency injection might be necessary |
| logger | [Microsoft.Extensions.Logging.ILogger](#T-Microsoft-Extensions-Logging-ILogger 'Microsoft.Extensions.Logging.ILogger') | An instance of a logger if logging is desired |
| channelMapper | [MQContract.ChannelMapper](#T-MQContract-ChannelMapper 'MQContract.ChannelMapper') | An instance of a ChannelMapper used to translate channels from one instance to another based on class channel attributes or supplied channels if necessary.
For example, it might be necessary for a Nats.IO instance when you are trying to read from a stored message stream that is comprised of another channel or set of channels |

<a name='T-MQContract-DynamicCodeNotSupportedException'></a>
## DynamicCodeNotSupportedException `type`

##### Namespace

MQContract

##### Summary

Thrown when dynamic code is not supported but a call that requires it is made

<a name='T-MQContract-Logging-EventIds'></a>
## EventIds `type`

##### Namespace

MQContract.Logging

##### Summary

Centralized well-known event id values used for structured logging across the MQContract library.

##### Remarks

Event id bands:
 - 1xxxx: Trace (hot-path diagnostic)
 - 2xxxx: Debug (state transitions)
 - 3xxxx: Information (lifecycle events)
 - 4xxxx: Warning (recoverable issues)
 - 5xxxx: Error
 - 9xxxx: Critical (invariants broken, data-loss risk)
Event id concern grouping:
 - x1xxx: Lifetime (connection, subscription, middleware registration)
 - x2xxx: Pipeline (middleware execution, message production/decoding, resilience)
 - x3xxx: Publishing (publish operations, queries, bulk publish, transmission/timeout errors)
 - x4xxx: Consuming (processing, acknowledgement, response handling)
 - x5xxx: Transport (connection discovery, pinging, connector connection issues)
 - x6xxx - x8xxx: (reserved for future concern groups)
Event id area grouping:
 - xx1xx: Connections  (connection lifecycle, discovery, pinging)
 - xx2xx: Subscriptions (subscription lifecycle, establishment, disposal)
 - xx3xx: Middleware (registration, execution)
 - xx4xx: Service Messages (production, decoding, filtering, processing attempts)
 - xx5xx: Consumer Operations (acknowledgement, response handling)
 - xx6xx: Resilience (retries, fallbacks)
 - xx7xx: Contract Messages (Pubsub, queries, query responses)
The nested classes group ids by concern (Lifetime, Pipeline, Publishing, Consuming, Transport).

<a name='T-MQContract-InvalidConsumerTypeException'></a>
## InvalidConsumerTypeException `type`

##### Namespace

MQContract

##### Summary

Thrown from a ContractConnection when an attempt to Register a given consumer through Type is not valid because the type does not implement the appropriate interface

<a name='T-MQContract-InvalidMiddlewareException'></a>
## InvalidMiddlewareException `type`

##### Namespace

MQContract

##### Summary

Thrown from a Contract Connection when an attempt is made to register a middleware that does not implement any middleware interfaces

<a name='T-MQContract-InvalidPolicyArgumentsException'></a>
## InvalidPolicyArgumentsException `type`

##### Namespace

MQContract

##### Summary

Thrown from a Resiliant Contract Connection when an attempt to create a policy is made but there is not any valid arguments

<a name='T-MQContract-InvalidQueryResponseMessageReceivedException'></a>
## InvalidQueryResponseMessageReceivedException `type`

##### Namespace

MQContract

##### Summary

Thrown when a query call message is received without proper data

<a name='T-MQContract-InvalidRetryCircuitBreakTriggersException'></a>
## InvalidRetryCircuitBreakTriggersException `type`

##### Namespace

MQContract

##### Summary

Thrown from a Resiliant Contract Connection when an attempt to create a policy is made but the retry count is higher than the circuit break count

<a name='T-MQContract-Logging-EventIds-Lifetime'></a>
## Lifetime `type`

##### Namespace

MQContract.Logging.EventIds

##### Summary

Connection / lifetime related event id values.
These ids cover creation, teardown and lifecycle operations for subscriptions, inboxes and middleware registration.

<a name='F-MQContract-Logging-EventIds-Lifetime-ClosingAllInboxes'></a>
### ClosingAllInboxes `constants`

##### Summary

Logged when all inboxes are being closed (full shutdown of inbox resources).
(31101 - Information)

<a name='F-MQContract-Logging-EventIds-Lifetime-ClosingConnection'></a>
### ClosingConnection `constants`

##### Summary

Logged when an individual connection is being closed.
(21100 - Debug)

<a name='F-MQContract-Logging-EventIds-Lifetime-ConstructingQueryResponseSubscription'></a>
### ConstructingQueryResponseSubscription `constants`

##### Summary

Logged when a query-response subscription object is being constructed.
(31210 - Information)

<a name='F-MQContract-Logging-EventIds-Lifetime-ConsumerRegistrationFailed'></a>
### ConsumerRegistrationFailed `constants`

##### Summary

Logged when consumer registration fails.
(51500 - Error)

<a name='F-MQContract-Logging-EventIds-Lifetime-CreatingPubSubSubscription'></a>
### CreatingPubSubSubscription `constants`

##### Summary

Logged when a pub/sub subscription object is being created (start of creation).
(21201 - Debug)

<a name='F-MQContract-Logging-EventIds-Lifetime-CreatingQueryResponseSubscription'></a>
### CreatingQueryResponseSubscription `constants`

##### Summary

Logged when a query-response subscription is being created.
(21209 - Debug)

<a name='F-MQContract-Logging-EventIds-Lifetime-DisposingQueryResponseSubscription'></a>
### DisposingQueryResponseSubscription `constants`

##### Summary

Logged when a query-response subscription is being disposed.
(31218 - Information)

<a name='F-MQContract-Logging-EventIds-Lifetime-EnablingMetricMiddleware'></a>
### EnablingMetricMiddleware `constants`

##### Summary

Logged when metric middleware is explicitly enabled.
(21301 - Debug)

<a name='F-MQContract-Logging-EventIds-Lifetime-EndingSubscription'></a>
### EndingSubscription `constants`

##### Summary

Logged when a subscription is ending / being disposed.
(31200 - Information)

<a name='F-MQContract-Logging-EventIds-Lifetime-EstablishingInboxSubscription'></a>
### EstablishingInboxSubscription `constants`

##### Summary

Logged when an inbox subscription establishment starts.
(21206 - Debug)

<a name='F-MQContract-Logging-EventIds-Lifetime-EstablishingNewInboxSubscription'></a>
### EstablishingNewInboxSubscription `constants`

##### Summary

Logged when establishing a brand new inbox subscription.
(21208 - Debug)

<a name='F-MQContract-Logging-EventIds-Lifetime-EstablishingPubSubServiceSubscription'></a>
### EstablishingPubSubServiceSubscription `constants`

##### Summary

Logged when establishing a pub/sub service-side subscription begins (service-backed subscription).
(21204 - Debug)

<a name='F-MQContract-Logging-EventIds-Lifetime-EstablishingPubSubServiceSubscriptionSucceeded'></a>
### EstablishingPubSubServiceSubscriptionSucceeded `constants`

##### Summary

Logged when establishing a pub/sub service subscription completes successfully.
(31205 - Information)

<a name='F-MQContract-Logging-EventIds-Lifetime-EstablishingPubSubSubscription'></a>
### EstablishingPubSubSubscription `constants`

##### Summary

Logged when the process to establish a pub/sub subscription begins.
(31202 - Information)

<a name='F-MQContract-Logging-EventIds-Lifetime-EstablishingQueryResponseServiceSubscription'></a>
### EstablishingQueryResponseServiceSubscription `constants`

##### Summary

Logged when the query-response service subscription is being established.
(31214 - Information)

<a name='F-MQContract-Logging-EventIds-Lifetime-EstablishingQueryResponseSubscription'></a>
### EstablishingQueryResponseSubscription `constants`

##### Summary

Logged when the establishment of a query-response subscription starts.
(21211 - Debug)

<a name='F-MQContract-Logging-EventIds-Lifetime-EstablishingQueryResponseSubscriptionServiceSubscription'></a>
### EstablishingQueryResponseSubscriptionServiceSubscription `constants`

##### Summary

Logged when a query-response subscription's service-side subscription is being established.
(31213 - Information)

<a name='F-MQContract-Logging-EventIds-Lifetime-EstablishingQueryResponseSubscriptionWithPubSubServiceSubscription'></a>
### EstablishingQueryResponseSubscriptionWithPubSubServiceSubscription `constants`

##### Summary

Logged when establishing a query-response subscription that also uses a pub/sub service subscription.
(31216 - Information)

<a name='F-MQContract-Logging-EventIds-Lifetime-PubSubSubscriptionEstablishmentFailed'></a>
### PubSubSubscriptionEstablishmentFailed `constants`

##### Summary

Logged when establishing a pub/sub subscription fails.
(51203 - Error)

<a name='F-MQContract-Logging-EventIds-Lifetime-QueryResponseSubscriptionDisposed'></a>
### QueryResponseSubscriptionDisposed `constants`

##### Summary

Logged when a query-response subscription has been disposed.
(31219 - Information)

<a name='F-MQContract-Logging-EventIds-Lifetime-QueryResponseSubscriptionEstablishmentFailed'></a>
### QueryResponseSubscriptionEstablishmentFailed `constants`

##### Summary

Logged when establishing a query-response subscription fails.
(51212 - Error)

<a name='F-MQContract-Logging-EventIds-Lifetime-QueryResponseSubscriptionEstablishmentFailedWithError'></a>
### QueryResponseSubscriptionEstablishmentFailedWithError `constants`

##### Summary

Logged when a query-response subscription establishment fails with an error detail.
(51217 - Error)

<a name='F-MQContract-Logging-EventIds-Lifetime-QueryResponseSubscriptionEstablishmentSucceeded'></a>
### QueryResponseSubscriptionEstablishmentSucceeded `constants`

##### Summary

Logged when a query-response subscription establishment completes successfully.
(31215 - Information)

<a name='F-MQContract-Logging-EventIds-Lifetime-RegisteringMiddleware'></a>
### RegisteringMiddleware `constants`

##### Summary

Logged when middleware is being registered with the pipeline.
(21300 - Debug)

<a name='F-MQContract-Logging-EventIds-Lifetime-SettingUpInboxSubscription'></a>
### SettingUpInboxSubscription `constants`

##### Summary

Logged when inbox subscription setup is in progress.
(31207 - Information)

<a name='T-MQContract-MessageChannelNullException'></a>
## MessageChannelNullException `type`

##### Namespace

MQContract

##### Summary

Thrown when a call is made but the system is unable to detect the channel

<a name='T-MQContract-MessageConversionException'></a>
## MessageConversionException `type`

##### Namespace

MQContract

##### Summary

Thrown when an incoming data message causes a null object return from a converter

<a name='T-MQContract-NoConnectionMatchException'></a>
## NoConnectionMatchException `type`

##### Namespace

MQContract

##### Summary

Thrown from the Mapped Connection when no connections match the search criteria making the requested action impossible to do

<a name='T-MQContract-PingNotSupportedException'></a>
## PingNotSupportedException `type`

##### Namespace

MQContract

##### Summary

Thrown from the ContractedConnection or the MappedConnection when there is no underlying service that supports the Ping call

<a name='T-MQContract-Logging-EventIds-Pipeline'></a>
## Pipeline `type`

##### Namespace

MQContract.Logging.EventIds

##### Summary

Pipeline related event id values.
These ids cover middleware execution, message production/decoding and resilience events on the message processing pipeline.

<a name='F-MQContract-Logging-EventIds-Pipeline-AttemptingToProcessInboxMessage'></a>
### AttemptingToProcessInboxMessage `constants`

##### Summary

Logged when attempting to process an inbox message (begin processing attempt).
(32405 - Information)

<a name='F-MQContract-Logging-EventIds-Pipeline-DecodingServiceMessage'></a>
### DecodingServiceMessage `constants`

##### Summary

Logged when a service message is decoded into a domain/message object.
(22402 - Information)

<a name='F-MQContract-Logging-EventIds-Pipeline-ExecutingAfterMessageDecodeMiddleware'></a>
### ExecutingAfterMessageDecodeMiddleware `constants`

##### Summary

Logged when executing 'after decode' middleware.
(22304 - Debug)

<a name='F-MQContract-Logging-EventIds-Pipeline-ExecutingAfterMessageEncodeMiddleware'></a>
### ExecutingAfterMessageEncodeMiddleware `constants`

##### Summary

Logged when executing 'after encode' middleware.
(22302 - Debug)

<a name='F-MQContract-Logging-EventIds-Pipeline-ExecutingBeforeMessageDecodeMiddleware'></a>
### ExecutingBeforeMessageDecodeMiddleware `constants`

##### Summary

Logged when executing 'before decode' middleware.
(22303 - Debug)

<a name='F-MQContract-Logging-EventIds-Pipeline-ExecutingBeforeMessageEncodeMiddleware'></a>
### ExecutingBeforeMessageEncodeMiddleware `constants`

##### Summary

Logged when executing 'before encode' middleware in the pipeline.
(22300 - Debug)

<a name='F-MQContract-Logging-EventIds-Pipeline-ExecutingSpecificAfterMessageDecodeMiddleware'></a>
### ExecutingSpecificAfterMessageDecodeMiddleware `constants`

##### Summary

Logged when executing a specific 'after decode' middleware.
(22305 - Debug)

<a name='F-MQContract-Logging-EventIds-Pipeline-ExecutingSpecificBeforeMessageEncodeMiddleware'></a>
### ExecutingSpecificBeforeMessageEncodeMiddleware `constants`

##### Summary

Logged when executing a specific 'before encode' middleware (identifies a particular middleware).
(22301 - Debug)

<a name='F-MQContract-Logging-EventIds-Pipeline-ExtractingQueryResponseType'></a>
### ExtractingQueryResponseType `constants`

##### Summary

Logged when extracting a query response CLR type from a message or headers.
(22700 - Debug)

<a name='F-MQContract-Logging-EventIds-Pipeline-FilterServiceMessageInFull'></a>
### FilterServiceMessageInFull `constants`

##### Summary

Logged when a service message is filtered by the full message content (not only headers).
(22403 - Information)

<a name='F-MQContract-Logging-EventIds-Pipeline-FilteringServiceMessageByHeaders'></a>
### FilteringServiceMessageByHeaders `constants`

##### Summary

Logged when a service message is filtered based on header values.
(22401 - Information)

<a name='F-MQContract-Logging-EventIds-Pipeline-ProcessingQueryResponse'></a>
### ProcessingQueryResponse `constants`

##### Summary

Logged when processing a query response message in the pipeline.
(22404 - Information)

<a name='F-MQContract-Logging-EventIds-Pipeline-ProcessingQueryResponseFailed'></a>
### ProcessingQueryResponseFailed `constants`

##### Summary

Logged when processing a query response fails.
(52406 - Error)

<a name='F-MQContract-Logging-EventIds-Pipeline-ProducingServiceMessage'></a>
### ProducingServiceMessage `constants`

##### Summary

Logged when a service message is produced by the pipeline (a message sent internally or to a connector).
(22400 - Information)

<a name='F-MQContract-Logging-EventIds-Pipeline-QueryReplyChannelMapped'></a>
### QueryReplyChannelMapped `constants`

##### Summary

Logged when a query reply channel has been mapped (associating a response channel for a query).
(22701 - Debug)

<a name='F-MQContract-Logging-EventIds-Pipeline-ReceivedInvalidQueryResponseMessage'></a>
### ReceivedInvalidQueryResponseMessage `constants`

##### Summary

Logged when an invalid query response message is received (invalid format or unexpected payload).
(42407 - Warning)

<a name='F-MQContract-Logging-EventIds-Pipeline-ResilienceFailedToFallback'></a>
### ResilienceFailedToFallback `constants`

##### Summary

Logged when resilience fallback could not be performed after retries.
(52601 - Error)

<a name='F-MQContract-Logging-EventIds-Pipeline-ResilienceRetryTriggered'></a>
### ResilienceRetryTriggered `constants`

##### Summary

Logged when a resilience retry is triggered for an operation in the pipeline.
(22600 - Debug)

<a name='T-MQContract-Logging-EventIds-Publishing'></a>
## Publishing `type`

##### Namespace

MQContract.Logging.EventIds

##### Summary

Publishing related event id values.
These ids cover publishing operations, queries, bulk publish and related transmission/timeout errors.

<a name='F-MQContract-Logging-EventIds-Publishing-AttemptingQueryResponse'></a>
### AttemptingQueryResponse `constants`

##### Summary

Logged when attempting a query response (publishing a response to a received query).
(23710 - Information)

<a name='F-MQContract-Logging-EventIds-Publishing-AttemptingQueryResponseOnPubSubService'></a>
### AttemptingQueryResponseOnPubSubService `constants`

##### Summary

Logged when attempting a query response that will be performed via the pub/sub service.
(33714 - Information)

<a name='F-MQContract-Logging-EventIds-Publishing-BulkPublishingMessages'></a>
### BulkPublishingMessages `constants`

##### Summary

Logged when multiple messages are being published in bulk.
(23703 - Information)

<a name='F-MQContract-Logging-EventIds-Publishing-ExecutingBulkPublish'></a>
### ExecutingBulkPublish `constants`

##### Summary

Logged when executing the internal bulk publish routine.
(23704 - Debug)

<a name='F-MQContract-Logging-EventIds-Publishing-ExecutingQuery'></a>
### ExecutingQuery `constants`

##### Summary

Logged when executing a query (publish+await response) from the publisher side.
(23705 - Information)

<a name='F-MQContract-Logging-EventIds-Publishing-ExecutingQueryResponseOnInboxService'></a>
### ExecutingQueryResponseOnInboxService `constants`

##### Summary

Logged when executing a query response on the inbox service.
(33712 - Information)

<a name='F-MQContract-Logging-EventIds-Publishing-ExecutingQueryResponseOnPubSubService'></a>
### ExecutingQueryResponseOnPubSubService `constants`

##### Summary

Logged when executing a query response on the pub/sub service.
(33713 - Information)

<a name='F-MQContract-Logging-EventIds-Publishing-ExecutingQueryResponseOnQueryResponseService'></a>
### ExecutingQueryResponseOnQueryResponseService `constants`

##### Summary

Logged when executing a query response on the dedicated query-response service.
(33711 - Information)

<a name='F-MQContract-Logging-EventIds-Publishing-InboxQueryTimedOut'></a>
### InboxQueryTimedOut `constants`

##### Summary

Logged when an inbox query has timed out waiting for a response.
(23706 - Information)

<a name='F-MQContract-Logging-EventIds-Publishing-PubSubQueryResponseRecieved'></a>
### PubSubQueryResponseRecieved `constants`

##### Summary

Logged when a pub/sub query response is received.
(23719 - Information)

<a name='F-MQContract-Logging-EventIds-Publishing-PublishingMessage'></a>
### PublishingMessage `constants`

##### Summary

Logged when a normal message publish operation is performed.
(23702 - Information)

<a name='F-MQContract-Logging-EventIds-Publishing-QueryExceptionOccured'></a>
### QueryExceptionOccured `constants`

##### Summary

Logged when a query operation throws an exception.
(53709 - Error)

<a name='F-MQContract-Logging-EventIds-Publishing-StartingQueryResponsePubSubListener'></a>
### StartingQueryResponsePubSubListener `constants`

##### Summary

Logged when starting a pub/sub listener that receives query responses.
(23715 - Information)

<a name='F-MQContract-Logging-EventIds-Publishing-TransmittingInboxQuery'></a>
### TransmittingInboxQuery `constants`

##### Summary

Logged when an inbox query is being transmitted to a remote service/connector.
(33707 - Information)

<a name='F-MQContract-Logging-EventIds-Publishing-TransmittingInboxQueryFailed'></a>
### TransmittingInboxQueryFailed `constants`

##### Summary

Logged when transmitting an inbox query failed.
(53708 - Error)

<a name='F-MQContract-Logging-EventIds-Publishing-TransmittingPubSubQuery'></a>
### TransmittingPubSubQuery `constants`

##### Summary

Logged when a pub/sub query is being transmitted.
(23716 - Information)

<a name='F-MQContract-Logging-EventIds-Publishing-TransmittingPubSubQueryFailed'></a>
### TransmittingPubSubQueryFailed `constants`

##### Summary

Logged when a pub/sub query transmission fails.
(53717 - Error)

<a name='F-MQContract-Logging-EventIds-Publishing-WaitingOnPubSubQueryResponse'></a>
### WaitingOnPubSubQueryResponse `constants`

##### Summary

Logged when awaiting a response for a pub/sub query.
(23718 - Information)

<a name='T-MQContract-QueryExecutionFailedException'></a>
## QueryExecutionFailedException `type`

##### Namespace

MQContract

##### Summary

Thrown when a query call is being made to a service that does not support query response and the listener cannot be created

<a name='T-MQContract-QueryResponseException'></a>
## QueryResponseException `type`

##### Namespace

MQContract

##### Summary

Thrown when a Query call is made and there is an error in the response

<a name='T-MQContract-QueryTimeoutException'></a>
## QueryTimeoutException `type`

##### Namespace

MQContract

##### Summary

Thrown when a query call times out waiting for the response

<a name='T-MQContract-SubscriptionFailedException'></a>
## SubscriptionFailedException `type`

##### Namespace

MQContract

##### Summary

Thrown when a Subscription has failed to be established/created

<a name='T-MQContract-TooManyConnectionMatchesException'></a>
## TooManyConnectionMatchesException `type`

##### Namespace

MQContract

##### Summary

Thrown from the Mapped Connection when more than 1 connection matches the search criteria making the requested action impossible to do

<a name='T-MQContract-Logging-EventIds-Transport'></a>
## Transport `type`

##### Namespace

MQContract.Logging.EventIds

##### Summary

Transport related event id values.
These ids cover connection discovery, pinging and issues locating connector connections.

<a name='F-MQContract-Logging-EventIds-Transport-LocatedTooManyConnections'></a>
### LocatedTooManyConnections `constants`

##### Summary

Logged when too many connections are located (unexpected multiplicity).
(55106 - Error)

<a name='F-MQContract-Logging-EventIds-Transport-LocatedTooManyConnectionsForMapType'></a>
### LocatedTooManyConnectionsForMapType `constants`

##### Summary

Logged when too many connections are located for a specific map type (unexpected multiplicity for typed lookup).
(55107 - Error)

<a name='F-MQContract-Logging-EventIds-Transport-LocatingConnections'></a>
### LocatingConnections `constants`

##### Summary

Logged when attempting to locate connections (discovery/lookup).
(25103 - Debug)

<a name='F-MQContract-Logging-EventIds-Transport-LocatingConnectionsForMapType'></a>
### LocatingConnectionsForMapType `constants`

##### Summary

Logged when locating connections for a specific map type (lookup for a typed mapping).
(25104 - Debug)

<a name='F-MQContract-Logging-EventIds-Transport-PingServiceConnection'></a>
### PingServiceConnection `constants`

##### Summary

Logged when a ping to a service connection is performed.
(25102 - Debug)

<a name='F-MQContract-Logging-EventIds-Transport-UnableToLocateConnections'></a>
### UnableToLocateConnections `constants`

##### Summary

Logged when unable to find any matching connections for a lookup.
(55105 - Error)

<a name='T-MQContract-UnknownResponseTypeException'></a>
## UnknownResponseTypeException `type`

##### Namespace

MQContract

##### Summary

Thrown when a QueryResponse type message is attempted without specifying the response type and there is no Response Type attribute for the query class.

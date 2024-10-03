<a name='assembly'></a>
# MQContract.KubeMQ

## Contents

- [AckAllQueueMessagesRequest](#T-MQContract-KubeMQ-SDK-Grpc-AckAllQueueMessagesRequest 'MQContract.KubeMQ.SDK.Grpc.AckAllQueueMessagesRequest')
  - [ChannelFieldNumber](#F-MQContract-KubeMQ-SDK-Grpc-AckAllQueueMessagesRequest-ChannelFieldNumber 'MQContract.KubeMQ.SDK.Grpc.AckAllQueueMessagesRequest.ChannelFieldNumber')
  - [ClientIDFieldNumber](#F-MQContract-KubeMQ-SDK-Grpc-AckAllQueueMessagesRequest-ClientIDFieldNumber 'MQContract.KubeMQ.SDK.Grpc.AckAllQueueMessagesRequest.ClientIDFieldNumber')
  - [RequestIDFieldNumber](#F-MQContract-KubeMQ-SDK-Grpc-AckAllQueueMessagesRequest-RequestIDFieldNumber 'MQContract.KubeMQ.SDK.Grpc.AckAllQueueMessagesRequest.RequestIDFieldNumber')
  - [WaitTimeSecondsFieldNumber](#F-MQContract-KubeMQ-SDK-Grpc-AckAllQueueMessagesRequest-WaitTimeSecondsFieldNumber 'MQContract.KubeMQ.SDK.Grpc.AckAllQueueMessagesRequest.WaitTimeSecondsFieldNumber')
- [AckAllQueueMessagesResponse](#T-MQContract-KubeMQ-SDK-Grpc-AckAllQueueMessagesResponse 'MQContract.KubeMQ.SDK.Grpc.AckAllQueueMessagesResponse')
  - [AffectedMessagesFieldNumber](#F-MQContract-KubeMQ-SDK-Grpc-AckAllQueueMessagesResponse-AffectedMessagesFieldNumber 'MQContract.KubeMQ.SDK.Grpc.AckAllQueueMessagesResponse.AffectedMessagesFieldNumber')
  - [ErrorFieldNumber](#F-MQContract-KubeMQ-SDK-Grpc-AckAllQueueMessagesResponse-ErrorFieldNumber 'MQContract.KubeMQ.SDK.Grpc.AckAllQueueMessagesResponse.ErrorFieldNumber')
  - [IsErrorFieldNumber](#F-MQContract-KubeMQ-SDK-Grpc-AckAllQueueMessagesResponse-IsErrorFieldNumber 'MQContract.KubeMQ.SDK.Grpc.AckAllQueueMessagesResponse.IsErrorFieldNumber')
  - [RequestIDFieldNumber](#F-MQContract-KubeMQ-SDK-Grpc-AckAllQueueMessagesResponse-RequestIDFieldNumber 'MQContract.KubeMQ.SDK.Grpc.AckAllQueueMessagesResponse.RequestIDFieldNumber')
- [ClientDisposedException](#T-MQContract-KubeMQ-ClientDisposedException 'MQContract.KubeMQ.ClientDisposedException')
- [Connection](#T-MQContract-KubeMQ-Connection 'MQContract.KubeMQ.Connection')
  - [#ctor(options)](#M-MQContract-KubeMQ-Connection-#ctor-MQContract-KubeMQ-ConnectionOptions- 'MQContract.KubeMQ.Connection.#ctor(MQContract.KubeMQ.ConnectionOptions)')
  - [RegisterStoredChannel(channelName)](#M-MQContract-KubeMQ-Connection-RegisterStoredChannel-System-String- 'MQContract.KubeMQ.Connection.RegisterStoredChannel(System.String)')
  - [RegisterStoredChannel(channelName,readStyle)](#M-MQContract-KubeMQ-Connection-RegisterStoredChannel-System-String,MQContract-KubeMQ-Connection-MessageReadStyle- 'MQContract.KubeMQ.Connection.RegisterStoredChannel(System.String,MQContract.KubeMQ.Connection.MessageReadStyle)')
  - [RegisterStoredChannel(channelName,readStyle,readOffset)](#M-MQContract-KubeMQ-Connection-RegisterStoredChannel-System-String,MQContract-KubeMQ-Connection-MessageReadStyle,System-Int64- 'MQContract.KubeMQ.Connection.RegisterStoredChannel(System.String,MQContract.KubeMQ.Connection.MessageReadStyle,System.Int64)')
- [ConnectionOptions](#T-MQContract-KubeMQ-ConnectionOptions 'MQContract.KubeMQ.ConnectionOptions')
  - [Address](#P-MQContract-KubeMQ-ConnectionOptions-Address 'MQContract.KubeMQ.ConnectionOptions.Address')
  - [AuthToken](#P-MQContract-KubeMQ-ConnectionOptions-AuthToken 'MQContract.KubeMQ.ConnectionOptions.AuthToken')
  - [ClientId](#P-MQContract-KubeMQ-ConnectionOptions-ClientId 'MQContract.KubeMQ.ConnectionOptions.ClientId')
  - [DefaultRPCTimeout](#P-MQContract-KubeMQ-ConnectionOptions-DefaultRPCTimeout 'MQContract.KubeMQ.ConnectionOptions.DefaultRPCTimeout')
  - [Logger](#P-MQContract-KubeMQ-ConnectionOptions-Logger 'MQContract.KubeMQ.ConnectionOptions.Logger')
  - [MaxBodySize](#P-MQContract-KubeMQ-ConnectionOptions-MaxBodySize 'MQContract.KubeMQ.ConnectionOptions.MaxBodySize')
  - [ReconnectInterval](#P-MQContract-KubeMQ-ConnectionOptions-ReconnectInterval 'MQContract.KubeMQ.ConnectionOptions.ReconnectInterval')
  - [SSLCertificate](#P-MQContract-KubeMQ-ConnectionOptions-SSLCertificate 'MQContract.KubeMQ.ConnectionOptions.SSLCertificate')
  - [SSLKey](#P-MQContract-KubeMQ-ConnectionOptions-SSLKey 'MQContract.KubeMQ.ConnectionOptions.SSLKey')
  - [SSLRootCertificate](#P-MQContract-KubeMQ-ConnectionOptions-SSLRootCertificate 'MQContract.KubeMQ.ConnectionOptions.SSLRootCertificate')
- [Event](#T-MQContract-KubeMQ-SDK-Grpc-Event 'MQContract.KubeMQ.SDK.Grpc.Event')
  - [BodyFieldNumber](#F-MQContract-KubeMQ-SDK-Grpc-Event-BodyFieldNumber 'MQContract.KubeMQ.SDK.Grpc.Event.BodyFieldNumber')
  - [ChannelFieldNumber](#F-MQContract-KubeMQ-SDK-Grpc-Event-ChannelFieldNumber 'MQContract.KubeMQ.SDK.Grpc.Event.ChannelFieldNumber')
  - [ClientIDFieldNumber](#F-MQContract-KubeMQ-SDK-Grpc-Event-ClientIDFieldNumber 'MQContract.KubeMQ.SDK.Grpc.Event.ClientIDFieldNumber')
  - [EventIDFieldNumber](#F-MQContract-KubeMQ-SDK-Grpc-Event-EventIDFieldNumber 'MQContract.KubeMQ.SDK.Grpc.Event.EventIDFieldNumber')
  - [MetadataFieldNumber](#F-MQContract-KubeMQ-SDK-Grpc-Event-MetadataFieldNumber 'MQContract.KubeMQ.SDK.Grpc.Event.MetadataFieldNumber')
  - [StoreFieldNumber](#F-MQContract-KubeMQ-SDK-Grpc-Event-StoreFieldNumber 'MQContract.KubeMQ.SDK.Grpc.Event.StoreFieldNumber')
  - [TagsFieldNumber](#F-MQContract-KubeMQ-SDK-Grpc-Event-TagsFieldNumber 'MQContract.KubeMQ.SDK.Grpc.Event.TagsFieldNumber')
- [EventReceive](#T-MQContract-KubeMQ-SDK-Grpc-EventReceive 'MQContract.KubeMQ.SDK.Grpc.EventReceive')
  - [BodyFieldNumber](#F-MQContract-KubeMQ-SDK-Grpc-EventReceive-BodyFieldNumber 'MQContract.KubeMQ.SDK.Grpc.EventReceive.BodyFieldNumber')
  - [ChannelFieldNumber](#F-MQContract-KubeMQ-SDK-Grpc-EventReceive-ChannelFieldNumber 'MQContract.KubeMQ.SDK.Grpc.EventReceive.ChannelFieldNumber')
  - [EventIDFieldNumber](#F-MQContract-KubeMQ-SDK-Grpc-EventReceive-EventIDFieldNumber 'MQContract.KubeMQ.SDK.Grpc.EventReceive.EventIDFieldNumber')
  - [MetadataFieldNumber](#F-MQContract-KubeMQ-SDK-Grpc-EventReceive-MetadataFieldNumber 'MQContract.KubeMQ.SDK.Grpc.EventReceive.MetadataFieldNumber')
  - [SequenceFieldNumber](#F-MQContract-KubeMQ-SDK-Grpc-EventReceive-SequenceFieldNumber 'MQContract.KubeMQ.SDK.Grpc.EventReceive.SequenceFieldNumber')
  - [TagsFieldNumber](#F-MQContract-KubeMQ-SDK-Grpc-EventReceive-TagsFieldNumber 'MQContract.KubeMQ.SDK.Grpc.EventReceive.TagsFieldNumber')
  - [TimestampFieldNumber](#F-MQContract-KubeMQ-SDK-Grpc-EventReceive-TimestampFieldNumber 'MQContract.KubeMQ.SDK.Grpc.EventReceive.TimestampFieldNumber')
- [IKubeMQPingResult](#T-MQContract-KubeMQ-Interfaces-IKubeMQPingResult 'MQContract.KubeMQ.Interfaces.IKubeMQPingResult')
  - [Host](#P-MQContract-KubeMQ-Interfaces-IKubeMQPingResult-Host 'MQContract.KubeMQ.Interfaces.IKubeMQPingResult.Host')
  - [ResponseTime](#P-MQContract-KubeMQ-Interfaces-IKubeMQPingResult-ResponseTime 'MQContract.KubeMQ.Interfaces.IKubeMQPingResult.ResponseTime')
  - [ServerStartTime](#P-MQContract-KubeMQ-Interfaces-IKubeMQPingResult-ServerStartTime 'MQContract.KubeMQ.Interfaces.IKubeMQPingResult.ServerStartTime')
  - [ServerUpTime](#P-MQContract-KubeMQ-Interfaces-IKubeMQPingResult-ServerUpTime 'MQContract.KubeMQ.Interfaces.IKubeMQPingResult.ServerUpTime')
  - [Version](#P-MQContract-KubeMQ-Interfaces-IKubeMQPingResult-Version 'MQContract.KubeMQ.Interfaces.IKubeMQPingResult.Version')
- [KubemqReflection](#T-MQContract-KubeMQ-SDK-Grpc-KubemqReflection 'MQContract.KubeMQ.SDK.Grpc.KubemqReflection')
  - [Descriptor](#P-MQContract-KubeMQ-SDK-Grpc-KubemqReflection-Descriptor 'MQContract.KubeMQ.SDK.Grpc.KubemqReflection.Descriptor')
- [MessageReadStyle](#T-MQContract-KubeMQ-Connection-MessageReadStyle 'MQContract.KubeMQ.Connection.MessageReadStyle')
  - [StartAtSequence](#F-MQContract-KubeMQ-Connection-MessageReadStyle-StartAtSequence 'MQContract.KubeMQ.Connection.MessageReadStyle.StartAtSequence')
  - [StartAtTime](#F-MQContract-KubeMQ-Connection-MessageReadStyle-StartAtTime 'MQContract.KubeMQ.Connection.MessageReadStyle.StartAtTime')
  - [StartAtTimeDelta](#F-MQContract-KubeMQ-Connection-MessageReadStyle-StartAtTimeDelta 'MQContract.KubeMQ.Connection.MessageReadStyle.StartAtTimeDelta')
  - [StartFromFirst](#F-MQContract-KubeMQ-Connection-MessageReadStyle-StartFromFirst 'MQContract.KubeMQ.Connection.MessageReadStyle.StartFromFirst')
  - [StartFromLast](#F-MQContract-KubeMQ-Connection-MessageReadStyle-StartFromLast 'MQContract.KubeMQ.Connection.MessageReadStyle.StartFromLast')
  - [StartNewOnly](#F-MQContract-KubeMQ-Connection-MessageReadStyle-StartNewOnly 'MQContract.KubeMQ.Connection.MessageReadStyle.StartNewOnly')
- [MessageResponseTransmissionException](#T-MQContract-KubeMQ-MessageResponseTransmissionException 'MQContract.KubeMQ.MessageResponseTransmissionException')
- [PingResult](#T-MQContract-KubeMQ-SDK-Grpc-PingResult 'MQContract.KubeMQ.SDK.Grpc.PingResult')
  - [HostFieldNumber](#F-MQContract-KubeMQ-SDK-Grpc-PingResult-HostFieldNumber 'MQContract.KubeMQ.SDK.Grpc.PingResult.HostFieldNumber')
  - [ServerStartTimeFieldNumber](#F-MQContract-KubeMQ-SDK-Grpc-PingResult-ServerStartTimeFieldNumber 'MQContract.KubeMQ.SDK.Grpc.PingResult.ServerStartTimeFieldNumber')
  - [ServerUpTimeSecondsFieldNumber](#F-MQContract-KubeMQ-SDK-Grpc-PingResult-ServerUpTimeSecondsFieldNumber 'MQContract.KubeMQ.SDK.Grpc.PingResult.ServerUpTimeSecondsFieldNumber')
  - [VersionFieldNumber](#F-MQContract-KubeMQ-SDK-Grpc-PingResult-VersionFieldNumber 'MQContract.KubeMQ.SDK.Grpc.PingResult.VersionFieldNumber')
- [PollRequest](#T-MQContract-KubeMQ-SDK-Grpc-PollRequest 'MQContract.KubeMQ.SDK.Grpc.PollRequest')
  - [AckRangeFieldNumber](#F-MQContract-KubeMQ-SDK-Grpc-PollRequest-AckRangeFieldNumber 'MQContract.KubeMQ.SDK.Grpc.PollRequest.AckRangeFieldNumber')
  - [AutoAckFieldNumber](#F-MQContract-KubeMQ-SDK-Grpc-PollRequest-AutoAckFieldNumber 'MQContract.KubeMQ.SDK.Grpc.PollRequest.AutoAckFieldNumber')
  - [ChannelFieldNumber](#F-MQContract-KubeMQ-SDK-Grpc-PollRequest-ChannelFieldNumber 'MQContract.KubeMQ.SDK.Grpc.PollRequest.ChannelFieldNumber')
  - [ClientIDFieldNumber](#F-MQContract-KubeMQ-SDK-Grpc-PollRequest-ClientIDFieldNumber 'MQContract.KubeMQ.SDK.Grpc.PollRequest.ClientIDFieldNumber')
  - [HeadersFieldNumber](#F-MQContract-KubeMQ-SDK-Grpc-PollRequest-HeadersFieldNumber 'MQContract.KubeMQ.SDK.Grpc.PollRequest.HeadersFieldNumber')
  - [RefTransactionIdFieldNumber](#F-MQContract-KubeMQ-SDK-Grpc-PollRequest-RefTransactionIdFieldNumber 'MQContract.KubeMQ.SDK.Grpc.PollRequest.RefTransactionIdFieldNumber')
  - [RequestIDFieldNumber](#F-MQContract-KubeMQ-SDK-Grpc-PollRequest-RequestIDFieldNumber 'MQContract.KubeMQ.SDK.Grpc.PollRequest.RequestIDFieldNumber')
  - [StreamRequestTypeDataFieldNumber](#F-MQContract-KubeMQ-SDK-Grpc-PollRequest-StreamRequestTypeDataFieldNumber 'MQContract.KubeMQ.SDK.Grpc.PollRequest.StreamRequestTypeDataFieldNumber')
- [PollResponse](#T-MQContract-KubeMQ-SDK-Grpc-PollResponse 'MQContract.KubeMQ.SDK.Grpc.PollResponse')
  - [ErrorFieldNumber](#F-MQContract-KubeMQ-SDK-Grpc-PollResponse-ErrorFieldNumber 'MQContract.KubeMQ.SDK.Grpc.PollResponse.ErrorFieldNumber')
  - [HeadersFieldNumber](#F-MQContract-KubeMQ-SDK-Grpc-PollResponse-HeadersFieldNumber 'MQContract.KubeMQ.SDK.Grpc.PollResponse.HeadersFieldNumber')
  - [IsErrorFieldNumber](#F-MQContract-KubeMQ-SDK-Grpc-PollResponse-IsErrorFieldNumber 'MQContract.KubeMQ.SDK.Grpc.PollResponse.IsErrorFieldNumber')
  - [MessagesFieldNumber](#F-MQContract-KubeMQ-SDK-Grpc-PollResponse-MessagesFieldNumber 'MQContract.KubeMQ.SDK.Grpc.PollResponse.MessagesFieldNumber')
  - [RefRequestIdFieldNumber](#F-MQContract-KubeMQ-SDK-Grpc-PollResponse-RefRequestIdFieldNumber 'MQContract.KubeMQ.SDK.Grpc.PollResponse.RefRequestIdFieldNumber')
  - [StreamRequestTypeDataFieldNumber](#F-MQContract-KubeMQ-SDK-Grpc-PollResponse-StreamRequestTypeDataFieldNumber 'MQContract.KubeMQ.SDK.Grpc.PollResponse.StreamRequestTypeDataFieldNumber')
  - [TransactionIdFieldNumber](#F-MQContract-KubeMQ-SDK-Grpc-PollResponse-TransactionIdFieldNumber 'MQContract.KubeMQ.SDK.Grpc.PollResponse.TransactionIdFieldNumber')
- [QueueMessage](#T-MQContract-KubeMQ-SDK-Grpc-QueueMessage 'MQContract.KubeMQ.SDK.Grpc.QueueMessage')
  - [AttributesFieldNumber](#F-MQContract-KubeMQ-SDK-Grpc-QueueMessage-AttributesFieldNumber 'MQContract.KubeMQ.SDK.Grpc.QueueMessage.AttributesFieldNumber')
  - [BodyFieldNumber](#F-MQContract-KubeMQ-SDK-Grpc-QueueMessage-BodyFieldNumber 'MQContract.KubeMQ.SDK.Grpc.QueueMessage.BodyFieldNumber')
  - [ChannelFieldNumber](#F-MQContract-KubeMQ-SDK-Grpc-QueueMessage-ChannelFieldNumber 'MQContract.KubeMQ.SDK.Grpc.QueueMessage.ChannelFieldNumber')
  - [ClientIDFieldNumber](#F-MQContract-KubeMQ-SDK-Grpc-QueueMessage-ClientIDFieldNumber 'MQContract.KubeMQ.SDK.Grpc.QueueMessage.ClientIDFieldNumber')
  - [MessageIDFieldNumber](#F-MQContract-KubeMQ-SDK-Grpc-QueueMessage-MessageIDFieldNumber 'MQContract.KubeMQ.SDK.Grpc.QueueMessage.MessageIDFieldNumber')
  - [MetadataFieldNumber](#F-MQContract-KubeMQ-SDK-Grpc-QueueMessage-MetadataFieldNumber 'MQContract.KubeMQ.SDK.Grpc.QueueMessage.MetadataFieldNumber')
  - [PolicyFieldNumber](#F-MQContract-KubeMQ-SDK-Grpc-QueueMessage-PolicyFieldNumber 'MQContract.KubeMQ.SDK.Grpc.QueueMessage.PolicyFieldNumber')
  - [TagsFieldNumber](#F-MQContract-KubeMQ-SDK-Grpc-QueueMessage-TagsFieldNumber 'MQContract.KubeMQ.SDK.Grpc.QueueMessage.TagsFieldNumber')
- [QueueMessageAttributes](#T-MQContract-KubeMQ-SDK-Grpc-QueueMessageAttributes 'MQContract.KubeMQ.SDK.Grpc.QueueMessageAttributes')
  - [DelayedToFieldNumber](#F-MQContract-KubeMQ-SDK-Grpc-QueueMessageAttributes-DelayedToFieldNumber 'MQContract.KubeMQ.SDK.Grpc.QueueMessageAttributes.DelayedToFieldNumber')
  - [ExpirationAtFieldNumber](#F-MQContract-KubeMQ-SDK-Grpc-QueueMessageAttributes-ExpirationAtFieldNumber 'MQContract.KubeMQ.SDK.Grpc.QueueMessageAttributes.ExpirationAtFieldNumber')
  - [MD5OfBodyFieldNumber](#F-MQContract-KubeMQ-SDK-Grpc-QueueMessageAttributes-MD5OfBodyFieldNumber 'MQContract.KubeMQ.SDK.Grpc.QueueMessageAttributes.MD5OfBodyFieldNumber')
  - [ReRoutedFieldNumber](#F-MQContract-KubeMQ-SDK-Grpc-QueueMessageAttributes-ReRoutedFieldNumber 'MQContract.KubeMQ.SDK.Grpc.QueueMessageAttributes.ReRoutedFieldNumber')
  - [ReRoutedFromQueueFieldNumber](#F-MQContract-KubeMQ-SDK-Grpc-QueueMessageAttributes-ReRoutedFromQueueFieldNumber 'MQContract.KubeMQ.SDK.Grpc.QueueMessageAttributes.ReRoutedFromQueueFieldNumber')
  - [ReceiveCountFieldNumber](#F-MQContract-KubeMQ-SDK-Grpc-QueueMessageAttributes-ReceiveCountFieldNumber 'MQContract.KubeMQ.SDK.Grpc.QueueMessageAttributes.ReceiveCountFieldNumber')
  - [SequenceFieldNumber](#F-MQContract-KubeMQ-SDK-Grpc-QueueMessageAttributes-SequenceFieldNumber 'MQContract.KubeMQ.SDK.Grpc.QueueMessageAttributes.SequenceFieldNumber')
  - [TimestampFieldNumber](#F-MQContract-KubeMQ-SDK-Grpc-QueueMessageAttributes-TimestampFieldNumber 'MQContract.KubeMQ.SDK.Grpc.QueueMessageAttributes.TimestampFieldNumber')
- [QueueMessagePolicy](#T-MQContract-KubeMQ-SDK-Grpc-QueueMessagePolicy 'MQContract.KubeMQ.SDK.Grpc.QueueMessagePolicy')
  - [DelaySecondsFieldNumber](#F-MQContract-KubeMQ-SDK-Grpc-QueueMessagePolicy-DelaySecondsFieldNumber 'MQContract.KubeMQ.SDK.Grpc.QueueMessagePolicy.DelaySecondsFieldNumber')
  - [ExpirationSecondsFieldNumber](#F-MQContract-KubeMQ-SDK-Grpc-QueueMessagePolicy-ExpirationSecondsFieldNumber 'MQContract.KubeMQ.SDK.Grpc.QueueMessagePolicy.ExpirationSecondsFieldNumber')
  - [MaxReceiveCountFieldNumber](#F-MQContract-KubeMQ-SDK-Grpc-QueueMessagePolicy-MaxReceiveCountFieldNumber 'MQContract.KubeMQ.SDK.Grpc.QueueMessagePolicy.MaxReceiveCountFieldNumber')
  - [MaxReceiveQueueFieldNumber](#F-MQContract-KubeMQ-SDK-Grpc-QueueMessagePolicy-MaxReceiveQueueFieldNumber 'MQContract.KubeMQ.SDK.Grpc.QueueMessagePolicy.MaxReceiveQueueFieldNumber')
- [QueueMessagesBatchRequest](#T-MQContract-KubeMQ-SDK-Grpc-QueueMessagesBatchRequest 'MQContract.KubeMQ.SDK.Grpc.QueueMessagesBatchRequest')
  - [BatchIDFieldNumber](#F-MQContract-KubeMQ-SDK-Grpc-QueueMessagesBatchRequest-BatchIDFieldNumber 'MQContract.KubeMQ.SDK.Grpc.QueueMessagesBatchRequest.BatchIDFieldNumber')
  - [MessagesFieldNumber](#F-MQContract-KubeMQ-SDK-Grpc-QueueMessagesBatchRequest-MessagesFieldNumber 'MQContract.KubeMQ.SDK.Grpc.QueueMessagesBatchRequest.MessagesFieldNumber')
- [QueueMessagesBatchResponse](#T-MQContract-KubeMQ-SDK-Grpc-QueueMessagesBatchResponse 'MQContract.KubeMQ.SDK.Grpc.QueueMessagesBatchResponse')
  - [BatchIDFieldNumber](#F-MQContract-KubeMQ-SDK-Grpc-QueueMessagesBatchResponse-BatchIDFieldNumber 'MQContract.KubeMQ.SDK.Grpc.QueueMessagesBatchResponse.BatchIDFieldNumber')
  - [HaveErrorsFieldNumber](#F-MQContract-KubeMQ-SDK-Grpc-QueueMessagesBatchResponse-HaveErrorsFieldNumber 'MQContract.KubeMQ.SDK.Grpc.QueueMessagesBatchResponse.HaveErrorsFieldNumber')
  - [ResultsFieldNumber](#F-MQContract-KubeMQ-SDK-Grpc-QueueMessagesBatchResponse-ResultsFieldNumber 'MQContract.KubeMQ.SDK.Grpc.QueueMessagesBatchResponse.ResultsFieldNumber')
- [ReceiveQueueMessagesRequest](#T-MQContract-KubeMQ-SDK-Grpc-ReceiveQueueMessagesRequest 'MQContract.KubeMQ.SDK.Grpc.ReceiveQueueMessagesRequest')
  - [ChannelFieldNumber](#F-MQContract-KubeMQ-SDK-Grpc-ReceiveQueueMessagesRequest-ChannelFieldNumber 'MQContract.KubeMQ.SDK.Grpc.ReceiveQueueMessagesRequest.ChannelFieldNumber')
  - [ClientIDFieldNumber](#F-MQContract-KubeMQ-SDK-Grpc-ReceiveQueueMessagesRequest-ClientIDFieldNumber 'MQContract.KubeMQ.SDK.Grpc.ReceiveQueueMessagesRequest.ClientIDFieldNumber')
  - [IsPeakFieldNumber](#F-MQContract-KubeMQ-SDK-Grpc-ReceiveQueueMessagesRequest-IsPeakFieldNumber 'MQContract.KubeMQ.SDK.Grpc.ReceiveQueueMessagesRequest.IsPeakFieldNumber')
  - [MaxNumberOfMessagesFieldNumber](#F-MQContract-KubeMQ-SDK-Grpc-ReceiveQueueMessagesRequest-MaxNumberOfMessagesFieldNumber 'MQContract.KubeMQ.SDK.Grpc.ReceiveQueueMessagesRequest.MaxNumberOfMessagesFieldNumber')
  - [RequestIDFieldNumber](#F-MQContract-KubeMQ-SDK-Grpc-ReceiveQueueMessagesRequest-RequestIDFieldNumber 'MQContract.KubeMQ.SDK.Grpc.ReceiveQueueMessagesRequest.RequestIDFieldNumber')
  - [WaitTimeSecondsFieldNumber](#F-MQContract-KubeMQ-SDK-Grpc-ReceiveQueueMessagesRequest-WaitTimeSecondsFieldNumber 'MQContract.KubeMQ.SDK.Grpc.ReceiveQueueMessagesRequest.WaitTimeSecondsFieldNumber')
- [ReceiveQueueMessagesResponse](#T-MQContract-KubeMQ-SDK-Grpc-ReceiveQueueMessagesResponse 'MQContract.KubeMQ.SDK.Grpc.ReceiveQueueMessagesResponse')
  - [ErrorFieldNumber](#F-MQContract-KubeMQ-SDK-Grpc-ReceiveQueueMessagesResponse-ErrorFieldNumber 'MQContract.KubeMQ.SDK.Grpc.ReceiveQueueMessagesResponse.ErrorFieldNumber')
  - [IsErrorFieldNumber](#F-MQContract-KubeMQ-SDK-Grpc-ReceiveQueueMessagesResponse-IsErrorFieldNumber 'MQContract.KubeMQ.SDK.Grpc.ReceiveQueueMessagesResponse.IsErrorFieldNumber')
  - [IsPeakFieldNumber](#F-MQContract-KubeMQ-SDK-Grpc-ReceiveQueueMessagesResponse-IsPeakFieldNumber 'MQContract.KubeMQ.SDK.Grpc.ReceiveQueueMessagesResponse.IsPeakFieldNumber')
  - [MessagesExpiredFieldNumber](#F-MQContract-KubeMQ-SDK-Grpc-ReceiveQueueMessagesResponse-MessagesExpiredFieldNumber 'MQContract.KubeMQ.SDK.Grpc.ReceiveQueueMessagesResponse.MessagesExpiredFieldNumber')
  - [MessagesFieldNumber](#F-MQContract-KubeMQ-SDK-Grpc-ReceiveQueueMessagesResponse-MessagesFieldNumber 'MQContract.KubeMQ.SDK.Grpc.ReceiveQueueMessagesResponse.MessagesFieldNumber')
  - [MessagesReceivedFieldNumber](#F-MQContract-KubeMQ-SDK-Grpc-ReceiveQueueMessagesResponse-MessagesReceivedFieldNumber 'MQContract.KubeMQ.SDK.Grpc.ReceiveQueueMessagesResponse.MessagesReceivedFieldNumber')
  - [RequestIDFieldNumber](#F-MQContract-KubeMQ-SDK-Grpc-ReceiveQueueMessagesResponse-RequestIDFieldNumber 'MQContract.KubeMQ.SDK.Grpc.ReceiveQueueMessagesResponse.RequestIDFieldNumber')
- [Request](#T-MQContract-KubeMQ-SDK-Grpc-Request 'MQContract.KubeMQ.SDK.Grpc.Request')
  - [BodyFieldNumber](#F-MQContract-KubeMQ-SDK-Grpc-Request-BodyFieldNumber 'MQContract.KubeMQ.SDK.Grpc.Request.BodyFieldNumber')
  - [CacheKeyFieldNumber](#F-MQContract-KubeMQ-SDK-Grpc-Request-CacheKeyFieldNumber 'MQContract.KubeMQ.SDK.Grpc.Request.CacheKeyFieldNumber')
  - [CacheTTLFieldNumber](#F-MQContract-KubeMQ-SDK-Grpc-Request-CacheTTLFieldNumber 'MQContract.KubeMQ.SDK.Grpc.Request.CacheTTLFieldNumber')
  - [ChannelFieldNumber](#F-MQContract-KubeMQ-SDK-Grpc-Request-ChannelFieldNumber 'MQContract.KubeMQ.SDK.Grpc.Request.ChannelFieldNumber')
  - [ClientIDFieldNumber](#F-MQContract-KubeMQ-SDK-Grpc-Request-ClientIDFieldNumber 'MQContract.KubeMQ.SDK.Grpc.Request.ClientIDFieldNumber')
  - [MetadataFieldNumber](#F-MQContract-KubeMQ-SDK-Grpc-Request-MetadataFieldNumber 'MQContract.KubeMQ.SDK.Grpc.Request.MetadataFieldNumber')
  - [ReplyChannelFieldNumber](#F-MQContract-KubeMQ-SDK-Grpc-Request-ReplyChannelFieldNumber 'MQContract.KubeMQ.SDK.Grpc.Request.ReplyChannelFieldNumber')
  - [RequestIDFieldNumber](#F-MQContract-KubeMQ-SDK-Grpc-Request-RequestIDFieldNumber 'MQContract.KubeMQ.SDK.Grpc.Request.RequestIDFieldNumber')
  - [RequestTypeDataFieldNumber](#F-MQContract-KubeMQ-SDK-Grpc-Request-RequestTypeDataFieldNumber 'MQContract.KubeMQ.SDK.Grpc.Request.RequestTypeDataFieldNumber')
  - [SpanFieldNumber](#F-MQContract-KubeMQ-SDK-Grpc-Request-SpanFieldNumber 'MQContract.KubeMQ.SDK.Grpc.Request.SpanFieldNumber')
  - [TagsFieldNumber](#F-MQContract-KubeMQ-SDK-Grpc-Request-TagsFieldNumber 'MQContract.KubeMQ.SDK.Grpc.Request.TagsFieldNumber')
  - [TimeoutFieldNumber](#F-MQContract-KubeMQ-SDK-Grpc-Request-TimeoutFieldNumber 'MQContract.KubeMQ.SDK.Grpc.Request.TimeoutFieldNumber')
- [Response](#T-MQContract-KubeMQ-SDK-Grpc-Response 'MQContract.KubeMQ.SDK.Grpc.Response')
  - [BodyFieldNumber](#F-MQContract-KubeMQ-SDK-Grpc-Response-BodyFieldNumber 'MQContract.KubeMQ.SDK.Grpc.Response.BodyFieldNumber')
  - [CacheHitFieldNumber](#F-MQContract-KubeMQ-SDK-Grpc-Response-CacheHitFieldNumber 'MQContract.KubeMQ.SDK.Grpc.Response.CacheHitFieldNumber')
  - [ClientIDFieldNumber](#F-MQContract-KubeMQ-SDK-Grpc-Response-ClientIDFieldNumber 'MQContract.KubeMQ.SDK.Grpc.Response.ClientIDFieldNumber')
  - [ErrorFieldNumber](#F-MQContract-KubeMQ-SDK-Grpc-Response-ErrorFieldNumber 'MQContract.KubeMQ.SDK.Grpc.Response.ErrorFieldNumber')
  - [ExecutedFieldNumber](#F-MQContract-KubeMQ-SDK-Grpc-Response-ExecutedFieldNumber 'MQContract.KubeMQ.SDK.Grpc.Response.ExecutedFieldNumber')
  - [MetadataFieldNumber](#F-MQContract-KubeMQ-SDK-Grpc-Response-MetadataFieldNumber 'MQContract.KubeMQ.SDK.Grpc.Response.MetadataFieldNumber')
  - [ReplyChannelFieldNumber](#F-MQContract-KubeMQ-SDK-Grpc-Response-ReplyChannelFieldNumber 'MQContract.KubeMQ.SDK.Grpc.Response.ReplyChannelFieldNumber')
  - [RequestIDFieldNumber](#F-MQContract-KubeMQ-SDK-Grpc-Response-RequestIDFieldNumber 'MQContract.KubeMQ.SDK.Grpc.Response.RequestIDFieldNumber')
  - [SpanFieldNumber](#F-MQContract-KubeMQ-SDK-Grpc-Response-SpanFieldNumber 'MQContract.KubeMQ.SDK.Grpc.Response.SpanFieldNumber')
  - [TagsFieldNumber](#F-MQContract-KubeMQ-SDK-Grpc-Response-TagsFieldNumber 'MQContract.KubeMQ.SDK.Grpc.Response.TagsFieldNumber')
  - [TimestampFieldNumber](#F-MQContract-KubeMQ-SDK-Grpc-Response-TimestampFieldNumber 'MQContract.KubeMQ.SDK.Grpc.Response.TimestampFieldNumber')
- [Result](#T-MQContract-KubeMQ-SDK-Grpc-Result 'MQContract.KubeMQ.SDK.Grpc.Result')
  - [ErrorFieldNumber](#F-MQContract-KubeMQ-SDK-Grpc-Result-ErrorFieldNumber 'MQContract.KubeMQ.SDK.Grpc.Result.ErrorFieldNumber')
  - [EventIDFieldNumber](#F-MQContract-KubeMQ-SDK-Grpc-Result-EventIDFieldNumber 'MQContract.KubeMQ.SDK.Grpc.Result.EventIDFieldNumber')
  - [SentFieldNumber](#F-MQContract-KubeMQ-SDK-Grpc-Result-SentFieldNumber 'MQContract.KubeMQ.SDK.Grpc.Result.SentFieldNumber')
- [SendQueueMessageResult](#T-MQContract-KubeMQ-SDK-Grpc-SendQueueMessageResult 'MQContract.KubeMQ.SDK.Grpc.SendQueueMessageResult')
  - [DelayedToFieldNumber](#F-MQContract-KubeMQ-SDK-Grpc-SendQueueMessageResult-DelayedToFieldNumber 'MQContract.KubeMQ.SDK.Grpc.SendQueueMessageResult.DelayedToFieldNumber')
  - [ErrorFieldNumber](#F-MQContract-KubeMQ-SDK-Grpc-SendQueueMessageResult-ErrorFieldNumber 'MQContract.KubeMQ.SDK.Grpc.SendQueueMessageResult.ErrorFieldNumber')
  - [ExpirationAtFieldNumber](#F-MQContract-KubeMQ-SDK-Grpc-SendQueueMessageResult-ExpirationAtFieldNumber 'MQContract.KubeMQ.SDK.Grpc.SendQueueMessageResult.ExpirationAtFieldNumber')
  - [IsErrorFieldNumber](#F-MQContract-KubeMQ-SDK-Grpc-SendQueueMessageResult-IsErrorFieldNumber 'MQContract.KubeMQ.SDK.Grpc.SendQueueMessageResult.IsErrorFieldNumber')
  - [MessageIDFieldNumber](#F-MQContract-KubeMQ-SDK-Grpc-SendQueueMessageResult-MessageIDFieldNumber 'MQContract.KubeMQ.SDK.Grpc.SendQueueMessageResult.MessageIDFieldNumber')
  - [SentAtFieldNumber](#F-MQContract-KubeMQ-SDK-Grpc-SendQueueMessageResult-SentAtFieldNumber 'MQContract.KubeMQ.SDK.Grpc.SendQueueMessageResult.SentAtFieldNumber')
- [StreamQueueMessagesRequest](#T-MQContract-KubeMQ-SDK-Grpc-StreamQueueMessagesRequest 'MQContract.KubeMQ.SDK.Grpc.StreamQueueMessagesRequest')
  - [ChannelFieldNumber](#F-MQContract-KubeMQ-SDK-Grpc-StreamQueueMessagesRequest-ChannelFieldNumber 'MQContract.KubeMQ.SDK.Grpc.StreamQueueMessagesRequest.ChannelFieldNumber')
  - [ClientIDFieldNumber](#F-MQContract-KubeMQ-SDK-Grpc-StreamQueueMessagesRequest-ClientIDFieldNumber 'MQContract.KubeMQ.SDK.Grpc.StreamQueueMessagesRequest.ClientIDFieldNumber')
  - [ModifiedMessageFieldNumber](#F-MQContract-KubeMQ-SDK-Grpc-StreamQueueMessagesRequest-ModifiedMessageFieldNumber 'MQContract.KubeMQ.SDK.Grpc.StreamQueueMessagesRequest.ModifiedMessageFieldNumber')
  - [RefSequenceFieldNumber](#F-MQContract-KubeMQ-SDK-Grpc-StreamQueueMessagesRequest-RefSequenceFieldNumber 'MQContract.KubeMQ.SDK.Grpc.StreamQueueMessagesRequest.RefSequenceFieldNumber')
  - [RequestIDFieldNumber](#F-MQContract-KubeMQ-SDK-Grpc-StreamQueueMessagesRequest-RequestIDFieldNumber 'MQContract.KubeMQ.SDK.Grpc.StreamQueueMessagesRequest.RequestIDFieldNumber')
  - [StreamRequestTypeDataFieldNumber](#F-MQContract-KubeMQ-SDK-Grpc-StreamQueueMessagesRequest-StreamRequestTypeDataFieldNumber 'MQContract.KubeMQ.SDK.Grpc.StreamQueueMessagesRequest.StreamRequestTypeDataFieldNumber')
  - [VisibilitySecondsFieldNumber](#F-MQContract-KubeMQ-SDK-Grpc-StreamQueueMessagesRequest-VisibilitySecondsFieldNumber 'MQContract.KubeMQ.SDK.Grpc.StreamQueueMessagesRequest.VisibilitySecondsFieldNumber')
  - [WaitTimeSecondsFieldNumber](#F-MQContract-KubeMQ-SDK-Grpc-StreamQueueMessagesRequest-WaitTimeSecondsFieldNumber 'MQContract.KubeMQ.SDK.Grpc.StreamQueueMessagesRequest.WaitTimeSecondsFieldNumber')
- [StreamQueueMessagesResponse](#T-MQContract-KubeMQ-SDK-Grpc-StreamQueueMessagesResponse 'MQContract.KubeMQ.SDK.Grpc.StreamQueueMessagesResponse')
  - [ErrorFieldNumber](#F-MQContract-KubeMQ-SDK-Grpc-StreamQueueMessagesResponse-ErrorFieldNumber 'MQContract.KubeMQ.SDK.Grpc.StreamQueueMessagesResponse.ErrorFieldNumber')
  - [IsErrorFieldNumber](#F-MQContract-KubeMQ-SDK-Grpc-StreamQueueMessagesResponse-IsErrorFieldNumber 'MQContract.KubeMQ.SDK.Grpc.StreamQueueMessagesResponse.IsErrorFieldNumber')
  - [MessageFieldNumber](#F-MQContract-KubeMQ-SDK-Grpc-StreamQueueMessagesResponse-MessageFieldNumber 'MQContract.KubeMQ.SDK.Grpc.StreamQueueMessagesResponse.MessageFieldNumber')
  - [RequestIDFieldNumber](#F-MQContract-KubeMQ-SDK-Grpc-StreamQueueMessagesResponse-RequestIDFieldNumber 'MQContract.KubeMQ.SDK.Grpc.StreamQueueMessagesResponse.RequestIDFieldNumber')
  - [StreamRequestTypeDataFieldNumber](#F-MQContract-KubeMQ-SDK-Grpc-StreamQueueMessagesResponse-StreamRequestTypeDataFieldNumber 'MQContract.KubeMQ.SDK.Grpc.StreamQueueMessagesResponse.StreamRequestTypeDataFieldNumber')
- [Subscribe](#T-MQContract-KubeMQ-SDK-Grpc-Subscribe 'MQContract.KubeMQ.SDK.Grpc.Subscribe')
  - [ChannelFieldNumber](#F-MQContract-KubeMQ-SDK-Grpc-Subscribe-ChannelFieldNumber 'MQContract.KubeMQ.SDK.Grpc.Subscribe.ChannelFieldNumber')
  - [ClientIDFieldNumber](#F-MQContract-KubeMQ-SDK-Grpc-Subscribe-ClientIDFieldNumber 'MQContract.KubeMQ.SDK.Grpc.Subscribe.ClientIDFieldNumber')
  - [EventsStoreTypeDataFieldNumber](#F-MQContract-KubeMQ-SDK-Grpc-Subscribe-EventsStoreTypeDataFieldNumber 'MQContract.KubeMQ.SDK.Grpc.Subscribe.EventsStoreTypeDataFieldNumber')
  - [EventsStoreTypeValueFieldNumber](#F-MQContract-KubeMQ-SDK-Grpc-Subscribe-EventsStoreTypeValueFieldNumber 'MQContract.KubeMQ.SDK.Grpc.Subscribe.EventsStoreTypeValueFieldNumber')
  - [GroupFieldNumber](#F-MQContract-KubeMQ-SDK-Grpc-Subscribe-GroupFieldNumber 'MQContract.KubeMQ.SDK.Grpc.Subscribe.GroupFieldNumber')
  - [SubscribeTypeDataFieldNumber](#F-MQContract-KubeMQ-SDK-Grpc-Subscribe-SubscribeTypeDataFieldNumber 'MQContract.KubeMQ.SDK.Grpc.Subscribe.SubscribeTypeDataFieldNumber')
- [Types](#T-MQContract-KubeMQ-SDK-Grpc-Request-Types 'MQContract.KubeMQ.SDK.Grpc.Request.Types')
- [Types](#T-MQContract-KubeMQ-SDK-Grpc-Subscribe-Types 'MQContract.KubeMQ.SDK.Grpc.Subscribe.Types')
- [UnableToConnectException](#T-MQContract-KubeMQ-UnableToConnectException 'MQContract.KubeMQ.UnableToConnectException')
- [kubemq](#T-MQContract-KubeMQ-SDK-Grpc-kubemq 'MQContract.KubeMQ.SDK.Grpc.kubemq')
  - [Descriptor](#P-MQContract-KubeMQ-SDK-Grpc-kubemq-Descriptor 'MQContract.KubeMQ.SDK.Grpc.kubemq.Descriptor')
- [kubemqClient](#T-MQContract-KubeMQ-SDK-Grpc-kubemq-kubemqClient 'MQContract.KubeMQ.SDK.Grpc.kubemq.kubemqClient')
  - [#ctor(channel)](#M-MQContract-KubeMQ-SDK-Grpc-kubemq-kubemqClient-#ctor-Grpc-Core-ChannelBase- 'MQContract.KubeMQ.SDK.Grpc.kubemq.kubemqClient.#ctor(Grpc.Core.ChannelBase)')
  - [#ctor(callInvoker)](#M-MQContract-KubeMQ-SDK-Grpc-kubemq-kubemqClient-#ctor-Grpc-Core-CallInvoker- 'MQContract.KubeMQ.SDK.Grpc.kubemq.kubemqClient.#ctor(Grpc.Core.CallInvoker)')
  - [#ctor()](#M-MQContract-KubeMQ-SDK-Grpc-kubemq-kubemqClient-#ctor 'MQContract.KubeMQ.SDK.Grpc.kubemq.kubemqClient.#ctor')
  - [#ctor(configuration)](#M-MQContract-KubeMQ-SDK-Grpc-kubemq-kubemqClient-#ctor-Grpc-Core-ClientBase-ClientBaseConfiguration- 'MQContract.KubeMQ.SDK.Grpc.kubemq.kubemqClient.#ctor(Grpc.Core.ClientBase.ClientBaseConfiguration)')
  - [NewInstance()](#M-MQContract-KubeMQ-SDK-Grpc-kubemq-kubemqClient-NewInstance-Grpc-Core-ClientBase-ClientBaseConfiguration- 'MQContract.KubeMQ.SDK.Grpc.kubemq.kubemqClient.NewInstance(Grpc.Core.ClientBase.ClientBaseConfiguration)')

<a name='T-MQContract-KubeMQ-SDK-Grpc-AckAllQueueMessagesRequest'></a>
## AckAllQueueMessagesRequest `type`

##### Namespace

MQContract.KubeMQ.SDK.Grpc

<a name='F-MQContract-KubeMQ-SDK-Grpc-AckAllQueueMessagesRequest-ChannelFieldNumber'></a>
### ChannelFieldNumber `constants`

##### Summary

Field number for the "Channel" field.

<a name='F-MQContract-KubeMQ-SDK-Grpc-AckAllQueueMessagesRequest-ClientIDFieldNumber'></a>
### ClientIDFieldNumber `constants`

##### Summary

Field number for the "ClientID" field.

<a name='F-MQContract-KubeMQ-SDK-Grpc-AckAllQueueMessagesRequest-RequestIDFieldNumber'></a>
### RequestIDFieldNumber `constants`

##### Summary

Field number for the "RequestID" field.

<a name='F-MQContract-KubeMQ-SDK-Grpc-AckAllQueueMessagesRequest-WaitTimeSecondsFieldNumber'></a>
### WaitTimeSecondsFieldNumber `constants`

##### Summary

Field number for the "WaitTimeSeconds" field.

<a name='T-MQContract-KubeMQ-SDK-Grpc-AckAllQueueMessagesResponse'></a>
## AckAllQueueMessagesResponse `type`

##### Namespace

MQContract.KubeMQ.SDK.Grpc

<a name='F-MQContract-KubeMQ-SDK-Grpc-AckAllQueueMessagesResponse-AffectedMessagesFieldNumber'></a>
### AffectedMessagesFieldNumber `constants`

##### Summary

Field number for the "AffectedMessages" field.

<a name='F-MQContract-KubeMQ-SDK-Grpc-AckAllQueueMessagesResponse-ErrorFieldNumber'></a>
### ErrorFieldNumber `constants`

##### Summary

Field number for the "Error" field.

<a name='F-MQContract-KubeMQ-SDK-Grpc-AckAllQueueMessagesResponse-IsErrorFieldNumber'></a>
### IsErrorFieldNumber `constants`

##### Summary

Field number for the "IsError" field.

<a name='F-MQContract-KubeMQ-SDK-Grpc-AckAllQueueMessagesResponse-RequestIDFieldNumber'></a>
### RequestIDFieldNumber `constants`

##### Summary

Field number for the "RequestID" field.

<a name='T-MQContract-KubeMQ-ClientDisposedException'></a>
## ClientDisposedException `type`

##### Namespace

MQContract.KubeMQ

##### Summary

Thrown when a call is made to an underlying KubeClient after the client has been disposed

<a name='T-MQContract-KubeMQ-Connection'></a>
## Connection `type`

##### Namespace

MQContract.KubeMQ

##### Summary

This is the MessageServiceConnection implementation for using KubeMQ

<a name='M-MQContract-KubeMQ-Connection-#ctor-MQContract-KubeMQ-ConnectionOptions-'></a>
### #ctor(options) `constructor`

##### Summary

Primary constructor to create an instance using the supplied configuration options

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| options | [MQContract.KubeMQ.ConnectionOptions](#T-MQContract-KubeMQ-ConnectionOptions 'MQContract.KubeMQ.ConnectionOptions') | The configuration options to use |

##### Exceptions

| Name | Description |
| ---- | ----------- |
| [MQContract.KubeMQ.UnableToConnectException](#T-MQContract-KubeMQ-UnableToConnectException 'MQContract.KubeMQ.UnableToConnectException') | Thrown when the initial attempt to connect fails |

<a name='M-MQContract-KubeMQ-Connection-RegisterStoredChannel-System-String-'></a>
### RegisterStoredChannel(channelName) `method`

##### Summary

Called to flag a particular channel as Stored Events when publishing or subscribing

##### Returns

The current connection to allow for chaining

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| channelName | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | The name of the channel |

<a name='M-MQContract-KubeMQ-Connection-RegisterStoredChannel-System-String,MQContract-KubeMQ-Connection-MessageReadStyle-'></a>
### RegisterStoredChannel(channelName,readStyle) `method`

##### Summary

Called to flag a particular channel as Stored Events when publishing or subscribing

##### Returns

The current connection to allow for chaining

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| channelName | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | The name of the channel |
| readStyle | [MQContract.KubeMQ.Connection.MessageReadStyle](#T-MQContract-KubeMQ-Connection-MessageReadStyle 'MQContract.KubeMQ.Connection.MessageReadStyle') | Set the message reading style when subscribing |

<a name='M-MQContract-KubeMQ-Connection-RegisterStoredChannel-System-String,MQContract-KubeMQ-Connection-MessageReadStyle,System-Int64-'></a>
### RegisterStoredChannel(channelName,readStyle,readOffset) `method`

##### Summary

Called to flag a particular channel as Stored Events when publishing or subscribing

##### Returns

The current connection to allow for chaining

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| channelName | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | The name of the channel |
| readStyle | [MQContract.KubeMQ.Connection.MessageReadStyle](#T-MQContract-KubeMQ-Connection-MessageReadStyle 'MQContract.KubeMQ.Connection.MessageReadStyle') | Set the message reading style when subscribing |
| readOffset | [System.Int64](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Int64 'System.Int64') | Set the readoffset to use for the given reading style |

<a name='T-MQContract-KubeMQ-ConnectionOptions'></a>
## ConnectionOptions `type`

##### Namespace

MQContract.KubeMQ

##### Summary

Houses the Connection Settings to use to connect to the particular instance of KubeMQ

<a name='P-MQContract-KubeMQ-ConnectionOptions-Address'></a>
### Address `property`

##### Summary

The address and port to connection to.  This can be the dns name or an ip address.
Use the format {ip/name}:{portnumber}.  Typically KubeMQ is configured to listen on 
port 50000

<a name='P-MQContract-KubeMQ-ConnectionOptions-AuthToken'></a>
### AuthToken `property`

##### Summary

The authentication token to use when connecting to the KubeMQ server

<a name='P-MQContract-KubeMQ-ConnectionOptions-ClientId'></a>
### ClientId `property`

##### Summary

The Unique Identification to be used when connecting to the KubeMQ server

<a name='P-MQContract-KubeMQ-ConnectionOptions-DefaultRPCTimeout'></a>
### DefaultRPCTimeout `property`

##### Summary

Timeout in milliseconds to use as a default for RPC calls if there is an override desired. 
Otherwise the default is 5000.

<a name='P-MQContract-KubeMQ-ConnectionOptions-Logger'></a>
### Logger `property`

##### Summary

Logging instance to use in underlying service layer

<a name='P-MQContract-KubeMQ-ConnectionOptions-MaxBodySize'></a>
### MaxBodySize `property`

##### Summary

The maximum body size in bytes configured on the KubeMQ server, default is 100MB.
If the encoded message exceeds the size, it will zip it in an attempt to transmit the 
message.  If it still fails in size, an exception will be thrown.

<a name='P-MQContract-KubeMQ-ConnectionOptions-ReconnectInterval'></a>
### ReconnectInterval `property`

##### Summary

Milliseconds to wait in between attempted reconnects to the KubeMQ server

<a name='P-MQContract-KubeMQ-ConnectionOptions-SSLCertificate'></a>
### SSLCertificate `property`

##### Summary

The SSL Certificat to use when connecting to the KubeMQ server

<a name='P-MQContract-KubeMQ-ConnectionOptions-SSLKey'></a>
### SSLKey `property`

##### Summary

The SSL Key to use when connecting to the KubeMQ server

<a name='P-MQContract-KubeMQ-ConnectionOptions-SSLRootCertificate'></a>
### SSLRootCertificate `property`

##### Summary

The SSL Root certificate to use when connecting to the KubeMQ server

<a name='T-MQContract-KubeMQ-SDK-Grpc-Event'></a>
## Event `type`

##### Namespace

MQContract.KubeMQ.SDK.Grpc

<a name='F-MQContract-KubeMQ-SDK-Grpc-Event-BodyFieldNumber'></a>
### BodyFieldNumber `constants`

##### Summary

Field number for the "Body" field.

<a name='F-MQContract-KubeMQ-SDK-Grpc-Event-ChannelFieldNumber'></a>
### ChannelFieldNumber `constants`

##### Summary

Field number for the "Channel" field.

<a name='F-MQContract-KubeMQ-SDK-Grpc-Event-ClientIDFieldNumber'></a>
### ClientIDFieldNumber `constants`

##### Summary

Field number for the "ClientID" field.

<a name='F-MQContract-KubeMQ-SDK-Grpc-Event-EventIDFieldNumber'></a>
### EventIDFieldNumber `constants`

##### Summary

Field number for the "EventID" field.

<a name='F-MQContract-KubeMQ-SDK-Grpc-Event-MetadataFieldNumber'></a>
### MetadataFieldNumber `constants`

##### Summary

Field number for the "Metadata" field.

<a name='F-MQContract-KubeMQ-SDK-Grpc-Event-StoreFieldNumber'></a>
### StoreFieldNumber `constants`

##### Summary

Field number for the "Store" field.

<a name='F-MQContract-KubeMQ-SDK-Grpc-Event-TagsFieldNumber'></a>
### TagsFieldNumber `constants`

##### Summary

Field number for the "Tags" field.

<a name='T-MQContract-KubeMQ-SDK-Grpc-EventReceive'></a>
## EventReceive `type`

##### Namespace

MQContract.KubeMQ.SDK.Grpc

<a name='F-MQContract-KubeMQ-SDK-Grpc-EventReceive-BodyFieldNumber'></a>
### BodyFieldNumber `constants`

##### Summary

Field number for the "Body" field.

<a name='F-MQContract-KubeMQ-SDK-Grpc-EventReceive-ChannelFieldNumber'></a>
### ChannelFieldNumber `constants`

##### Summary

Field number for the "Channel" field.

<a name='F-MQContract-KubeMQ-SDK-Grpc-EventReceive-EventIDFieldNumber'></a>
### EventIDFieldNumber `constants`

##### Summary

Field number for the "EventID" field.

<a name='F-MQContract-KubeMQ-SDK-Grpc-EventReceive-MetadataFieldNumber'></a>
### MetadataFieldNumber `constants`

##### Summary

Field number for the "Metadata" field.

<a name='F-MQContract-KubeMQ-SDK-Grpc-EventReceive-SequenceFieldNumber'></a>
### SequenceFieldNumber `constants`

##### Summary

Field number for the "Sequence" field.

<a name='F-MQContract-KubeMQ-SDK-Grpc-EventReceive-TagsFieldNumber'></a>
### TagsFieldNumber `constants`

##### Summary

Field number for the "Tags" field.

<a name='F-MQContract-KubeMQ-SDK-Grpc-EventReceive-TimestampFieldNumber'></a>
### TimestampFieldNumber `constants`

##### Summary

Field number for the "Timestamp" field.

<a name='T-MQContract-KubeMQ-Interfaces-IKubeMQPingResult'></a>
## IKubeMQPingResult `type`

##### Namespace

MQContract.KubeMQ.Interfaces

##### Summary

The definition for a PingResponse coming from KubeMQ that has a couple of extra properties available

<a name='P-MQContract-KubeMQ-Interfaces-IKubeMQPingResult-Host'></a>
### Host `property`

##### Summary

The host name for the server pinged

<a name='P-MQContract-KubeMQ-Interfaces-IKubeMQPingResult-ResponseTime'></a>
### ResponseTime `property`

##### Summary

How long it took the server to respond to the request

<a name='P-MQContract-KubeMQ-Interfaces-IKubeMQPingResult-ServerStartTime'></a>
### ServerStartTime `property`

##### Summary

The Server Start Time of the host that was pinged

<a name='P-MQContract-KubeMQ-Interfaces-IKubeMQPingResult-ServerUpTime'></a>
### ServerUpTime `property`

##### Summary

The Server Up Time of the host that was pinged

<a name='P-MQContract-KubeMQ-Interfaces-IKubeMQPingResult-Version'></a>
### Version `property`

##### Summary

The current version of KubeMQ running on it

<a name='T-MQContract-KubeMQ-SDK-Grpc-KubemqReflection'></a>
## KubemqReflection `type`

##### Namespace

MQContract.KubeMQ.SDK.Grpc

##### Summary

Holder for reflection information generated from SDK/Grpc/kubemq.proto

<a name='P-MQContract-KubeMQ-SDK-Grpc-KubemqReflection-Descriptor'></a>
### Descriptor `property`

##### Summary

File descriptor for SDK/Grpc/kubemq.proto

<a name='T-MQContract-KubeMQ-Connection-MessageReadStyle'></a>
## MessageReadStyle `type`

##### Namespace

MQContract.KubeMQ.Connection

##### Summary

These are the different read styles to use when subscribing to a stored Event PubSub

<a name='F-MQContract-KubeMQ-Connection-MessageReadStyle-StartAtSequence'></a>
### StartAtSequence `constants`

##### Summary

Start at message number X (this value is specified when creating the listener)

<a name='F-MQContract-KubeMQ-Connection-MessageReadStyle-StartAtTime'></a>
### StartAtTime `constants`

##### Summary

Start at time X (this value is specified when creating the listener)

<a name='F-MQContract-KubeMQ-Connection-MessageReadStyle-StartAtTimeDelta'></a>
### StartAtTimeDelta `constants`

##### Summary

Start at Time Delte X (this value is specified when creating the listener)

<a name='F-MQContract-KubeMQ-Connection-MessageReadStyle-StartFromFirst'></a>
### StartFromFirst `constants`

##### Summary

Start at the beginning

<a name='F-MQContract-KubeMQ-Connection-MessageReadStyle-StartFromLast'></a>
### StartFromLast `constants`

##### Summary

Start at the last message

<a name='F-MQContract-KubeMQ-Connection-MessageReadStyle-StartNewOnly'></a>
### StartNewOnly `constants`

##### Summary

Start from the new ones (unread ones) only

<a name='T-MQContract-KubeMQ-MessageResponseTransmissionException'></a>
## MessageResponseTransmissionException `type`

##### Namespace

MQContract.KubeMQ

##### Summary

Thrown when an error occurs sending and rpc response

<a name='T-MQContract-KubeMQ-SDK-Grpc-PingResult'></a>
## PingResult `type`

##### Namespace

MQContract.KubeMQ.SDK.Grpc

<a name='F-MQContract-KubeMQ-SDK-Grpc-PingResult-HostFieldNumber'></a>
### HostFieldNumber `constants`

##### Summary

Field number for the "Host" field.

<a name='F-MQContract-KubeMQ-SDK-Grpc-PingResult-ServerStartTimeFieldNumber'></a>
### ServerStartTimeFieldNumber `constants`

##### Summary

Field number for the "ServerStartTime" field.

<a name='F-MQContract-KubeMQ-SDK-Grpc-PingResult-ServerUpTimeSecondsFieldNumber'></a>
### ServerUpTimeSecondsFieldNumber `constants`

##### Summary

Field number for the "ServerUpTimeSeconds" field.

<a name='F-MQContract-KubeMQ-SDK-Grpc-PingResult-VersionFieldNumber'></a>
### VersionFieldNumber `constants`

##### Summary

Field number for the "Version" field.

<a name='T-MQContract-KubeMQ-SDK-Grpc-PollRequest'></a>
## PollRequest `type`

##### Namespace

MQContract.KubeMQ.SDK.Grpc

<a name='F-MQContract-KubeMQ-SDK-Grpc-PollRequest-AckRangeFieldNumber'></a>
### AckRangeFieldNumber `constants`

##### Summary

Field number for the "AckRange" field.

<a name='F-MQContract-KubeMQ-SDK-Grpc-PollRequest-AutoAckFieldNumber'></a>
### AutoAckFieldNumber `constants`

##### Summary

Field number for the "AutoAck" field.

<a name='F-MQContract-KubeMQ-SDK-Grpc-PollRequest-ChannelFieldNumber'></a>
### ChannelFieldNumber `constants`

##### Summary

Field number for the "Channel" field.

<a name='F-MQContract-KubeMQ-SDK-Grpc-PollRequest-ClientIDFieldNumber'></a>
### ClientIDFieldNumber `constants`

##### Summary

Field number for the "ClientID" field.

<a name='F-MQContract-KubeMQ-SDK-Grpc-PollRequest-HeadersFieldNumber'></a>
### HeadersFieldNumber `constants`

##### Summary

Field number for the "Headers" field.

<a name='F-MQContract-KubeMQ-SDK-Grpc-PollRequest-RefTransactionIdFieldNumber'></a>
### RefTransactionIdFieldNumber `constants`

##### Summary

Field number for the "RefTransactionId" field.

<a name='F-MQContract-KubeMQ-SDK-Grpc-PollRequest-RequestIDFieldNumber'></a>
### RequestIDFieldNumber `constants`

##### Summary

Field number for the "RequestID" field.

<a name='F-MQContract-KubeMQ-SDK-Grpc-PollRequest-StreamRequestTypeDataFieldNumber'></a>
### StreamRequestTypeDataFieldNumber `constants`

##### Summary

Field number for the "StreamRequestTypeData" field.

<a name='T-MQContract-KubeMQ-SDK-Grpc-PollResponse'></a>
## PollResponse `type`

##### Namespace

MQContract.KubeMQ.SDK.Grpc

<a name='F-MQContract-KubeMQ-SDK-Grpc-PollResponse-ErrorFieldNumber'></a>
### ErrorFieldNumber `constants`

##### Summary

Field number for the "Error" field.

<a name='F-MQContract-KubeMQ-SDK-Grpc-PollResponse-HeadersFieldNumber'></a>
### HeadersFieldNumber `constants`

##### Summary

Field number for the "Headers" field.

<a name='F-MQContract-KubeMQ-SDK-Grpc-PollResponse-IsErrorFieldNumber'></a>
### IsErrorFieldNumber `constants`

##### Summary

Field number for the "IsError" field.

<a name='F-MQContract-KubeMQ-SDK-Grpc-PollResponse-MessagesFieldNumber'></a>
### MessagesFieldNumber `constants`

##### Summary

Field number for the "Messages" field.

<a name='F-MQContract-KubeMQ-SDK-Grpc-PollResponse-RefRequestIdFieldNumber'></a>
### RefRequestIdFieldNumber `constants`

##### Summary

Field number for the "RefRequestId" field.

<a name='F-MQContract-KubeMQ-SDK-Grpc-PollResponse-StreamRequestTypeDataFieldNumber'></a>
### StreamRequestTypeDataFieldNumber `constants`

##### Summary

Field number for the "StreamRequestTypeData" field.

<a name='F-MQContract-KubeMQ-SDK-Grpc-PollResponse-TransactionIdFieldNumber'></a>
### TransactionIdFieldNumber `constants`

##### Summary

Field number for the "TransactionId" field.

<a name='T-MQContract-KubeMQ-SDK-Grpc-QueueMessage'></a>
## QueueMessage `type`

##### Namespace

MQContract.KubeMQ.SDK.Grpc

<a name='F-MQContract-KubeMQ-SDK-Grpc-QueueMessage-AttributesFieldNumber'></a>
### AttributesFieldNumber `constants`

##### Summary

Field number for the "Attributes" field.

<a name='F-MQContract-KubeMQ-SDK-Grpc-QueueMessage-BodyFieldNumber'></a>
### BodyFieldNumber `constants`

##### Summary

Field number for the "Body" field.

<a name='F-MQContract-KubeMQ-SDK-Grpc-QueueMessage-ChannelFieldNumber'></a>
### ChannelFieldNumber `constants`

##### Summary

Field number for the "Channel" field.

<a name='F-MQContract-KubeMQ-SDK-Grpc-QueueMessage-ClientIDFieldNumber'></a>
### ClientIDFieldNumber `constants`

##### Summary

Field number for the "ClientID" field.

<a name='F-MQContract-KubeMQ-SDK-Grpc-QueueMessage-MessageIDFieldNumber'></a>
### MessageIDFieldNumber `constants`

##### Summary

Field number for the "MessageID" field.

<a name='F-MQContract-KubeMQ-SDK-Grpc-QueueMessage-MetadataFieldNumber'></a>
### MetadataFieldNumber `constants`

##### Summary

Field number for the "Metadata" field.

<a name='F-MQContract-KubeMQ-SDK-Grpc-QueueMessage-PolicyFieldNumber'></a>
### PolicyFieldNumber `constants`

##### Summary

Field number for the "Policy" field.

<a name='F-MQContract-KubeMQ-SDK-Grpc-QueueMessage-TagsFieldNumber'></a>
### TagsFieldNumber `constants`

##### Summary

Field number for the "Tags" field.

<a name='T-MQContract-KubeMQ-SDK-Grpc-QueueMessageAttributes'></a>
## QueueMessageAttributes `type`

##### Namespace

MQContract.KubeMQ.SDK.Grpc

<a name='F-MQContract-KubeMQ-SDK-Grpc-QueueMessageAttributes-DelayedToFieldNumber'></a>
### DelayedToFieldNumber `constants`

##### Summary

Field number for the "DelayedTo" field.

<a name='F-MQContract-KubeMQ-SDK-Grpc-QueueMessageAttributes-ExpirationAtFieldNumber'></a>
### ExpirationAtFieldNumber `constants`

##### Summary

Field number for the "ExpirationAt" field.

<a name='F-MQContract-KubeMQ-SDK-Grpc-QueueMessageAttributes-MD5OfBodyFieldNumber'></a>
### MD5OfBodyFieldNumber `constants`

##### Summary

Field number for the "MD5OfBody" field.

<a name='F-MQContract-KubeMQ-SDK-Grpc-QueueMessageAttributes-ReRoutedFieldNumber'></a>
### ReRoutedFieldNumber `constants`

##### Summary

Field number for the "ReRouted" field.

<a name='F-MQContract-KubeMQ-SDK-Grpc-QueueMessageAttributes-ReRoutedFromQueueFieldNumber'></a>
### ReRoutedFromQueueFieldNumber `constants`

##### Summary

Field number for the "ReRoutedFromQueue" field.

<a name='F-MQContract-KubeMQ-SDK-Grpc-QueueMessageAttributes-ReceiveCountFieldNumber'></a>
### ReceiveCountFieldNumber `constants`

##### Summary

Field number for the "ReceiveCount" field.

<a name='F-MQContract-KubeMQ-SDK-Grpc-QueueMessageAttributes-SequenceFieldNumber'></a>
### SequenceFieldNumber `constants`

##### Summary

Field number for the "Sequence" field.

<a name='F-MQContract-KubeMQ-SDK-Grpc-QueueMessageAttributes-TimestampFieldNumber'></a>
### TimestampFieldNumber `constants`

##### Summary

Field number for the "Timestamp" field.

<a name='T-MQContract-KubeMQ-SDK-Grpc-QueueMessagePolicy'></a>
## QueueMessagePolicy `type`

##### Namespace

MQContract.KubeMQ.SDK.Grpc

<a name='F-MQContract-KubeMQ-SDK-Grpc-QueueMessagePolicy-DelaySecondsFieldNumber'></a>
### DelaySecondsFieldNumber `constants`

##### Summary

Field number for the "DelaySeconds" field.

<a name='F-MQContract-KubeMQ-SDK-Grpc-QueueMessagePolicy-ExpirationSecondsFieldNumber'></a>
### ExpirationSecondsFieldNumber `constants`

##### Summary

Field number for the "ExpirationSeconds" field.

<a name='F-MQContract-KubeMQ-SDK-Grpc-QueueMessagePolicy-MaxReceiveCountFieldNumber'></a>
### MaxReceiveCountFieldNumber `constants`

##### Summary

Field number for the "MaxReceiveCount" field.

<a name='F-MQContract-KubeMQ-SDK-Grpc-QueueMessagePolicy-MaxReceiveQueueFieldNumber'></a>
### MaxReceiveQueueFieldNumber `constants`

##### Summary

Field number for the "MaxReceiveQueue" field.

<a name='T-MQContract-KubeMQ-SDK-Grpc-QueueMessagesBatchRequest'></a>
## QueueMessagesBatchRequest `type`

##### Namespace

MQContract.KubeMQ.SDK.Grpc

<a name='F-MQContract-KubeMQ-SDK-Grpc-QueueMessagesBatchRequest-BatchIDFieldNumber'></a>
### BatchIDFieldNumber `constants`

##### Summary

Field number for the "BatchID" field.

<a name='F-MQContract-KubeMQ-SDK-Grpc-QueueMessagesBatchRequest-MessagesFieldNumber'></a>
### MessagesFieldNumber `constants`

##### Summary

Field number for the "Messages" field.

<a name='T-MQContract-KubeMQ-SDK-Grpc-QueueMessagesBatchResponse'></a>
## QueueMessagesBatchResponse `type`

##### Namespace

MQContract.KubeMQ.SDK.Grpc

<a name='F-MQContract-KubeMQ-SDK-Grpc-QueueMessagesBatchResponse-BatchIDFieldNumber'></a>
### BatchIDFieldNumber `constants`

##### Summary

Field number for the "BatchID" field.

<a name='F-MQContract-KubeMQ-SDK-Grpc-QueueMessagesBatchResponse-HaveErrorsFieldNumber'></a>
### HaveErrorsFieldNumber `constants`

##### Summary

Field number for the "HaveErrors" field.

<a name='F-MQContract-KubeMQ-SDK-Grpc-QueueMessagesBatchResponse-ResultsFieldNumber'></a>
### ResultsFieldNumber `constants`

##### Summary

Field number for the "Results" field.

<a name='T-MQContract-KubeMQ-SDK-Grpc-ReceiveQueueMessagesRequest'></a>
## ReceiveQueueMessagesRequest `type`

##### Namespace

MQContract.KubeMQ.SDK.Grpc

<a name='F-MQContract-KubeMQ-SDK-Grpc-ReceiveQueueMessagesRequest-ChannelFieldNumber'></a>
### ChannelFieldNumber `constants`

##### Summary

Field number for the "Channel" field.

<a name='F-MQContract-KubeMQ-SDK-Grpc-ReceiveQueueMessagesRequest-ClientIDFieldNumber'></a>
### ClientIDFieldNumber `constants`

##### Summary

Field number for the "ClientID" field.

<a name='F-MQContract-KubeMQ-SDK-Grpc-ReceiveQueueMessagesRequest-IsPeakFieldNumber'></a>
### IsPeakFieldNumber `constants`

##### Summary

Field number for the "IsPeak" field.

<a name='F-MQContract-KubeMQ-SDK-Grpc-ReceiveQueueMessagesRequest-MaxNumberOfMessagesFieldNumber'></a>
### MaxNumberOfMessagesFieldNumber `constants`

##### Summary

Field number for the "MaxNumberOfMessages" field.

<a name='F-MQContract-KubeMQ-SDK-Grpc-ReceiveQueueMessagesRequest-RequestIDFieldNumber'></a>
### RequestIDFieldNumber `constants`

##### Summary

Field number for the "RequestID" field.

<a name='F-MQContract-KubeMQ-SDK-Grpc-ReceiveQueueMessagesRequest-WaitTimeSecondsFieldNumber'></a>
### WaitTimeSecondsFieldNumber `constants`

##### Summary

Field number for the "WaitTimeSeconds" field.

<a name='T-MQContract-KubeMQ-SDK-Grpc-ReceiveQueueMessagesResponse'></a>
## ReceiveQueueMessagesResponse `type`

##### Namespace

MQContract.KubeMQ.SDK.Grpc

<a name='F-MQContract-KubeMQ-SDK-Grpc-ReceiveQueueMessagesResponse-ErrorFieldNumber'></a>
### ErrorFieldNumber `constants`

##### Summary

Field number for the "Error" field.

<a name='F-MQContract-KubeMQ-SDK-Grpc-ReceiveQueueMessagesResponse-IsErrorFieldNumber'></a>
### IsErrorFieldNumber `constants`

##### Summary

Field number for the "IsError" field.

<a name='F-MQContract-KubeMQ-SDK-Grpc-ReceiveQueueMessagesResponse-IsPeakFieldNumber'></a>
### IsPeakFieldNumber `constants`

##### Summary

Field number for the "IsPeak" field.

<a name='F-MQContract-KubeMQ-SDK-Grpc-ReceiveQueueMessagesResponse-MessagesExpiredFieldNumber'></a>
### MessagesExpiredFieldNumber `constants`

##### Summary

Field number for the "MessagesExpired" field.

<a name='F-MQContract-KubeMQ-SDK-Grpc-ReceiveQueueMessagesResponse-MessagesFieldNumber'></a>
### MessagesFieldNumber `constants`

##### Summary

Field number for the "Messages" field.

<a name='F-MQContract-KubeMQ-SDK-Grpc-ReceiveQueueMessagesResponse-MessagesReceivedFieldNumber'></a>
### MessagesReceivedFieldNumber `constants`

##### Summary

Field number for the "MessagesReceived" field.

<a name='F-MQContract-KubeMQ-SDK-Grpc-ReceiveQueueMessagesResponse-RequestIDFieldNumber'></a>
### RequestIDFieldNumber `constants`

##### Summary

Field number for the "RequestID" field.

<a name='T-MQContract-KubeMQ-SDK-Grpc-Request'></a>
## Request `type`

##### Namespace

MQContract.KubeMQ.SDK.Grpc

<a name='F-MQContract-KubeMQ-SDK-Grpc-Request-BodyFieldNumber'></a>
### BodyFieldNumber `constants`

##### Summary

Field number for the "Body" field.

<a name='F-MQContract-KubeMQ-SDK-Grpc-Request-CacheKeyFieldNumber'></a>
### CacheKeyFieldNumber `constants`

##### Summary

Field number for the "CacheKey" field.

<a name='F-MQContract-KubeMQ-SDK-Grpc-Request-CacheTTLFieldNumber'></a>
### CacheTTLFieldNumber `constants`

##### Summary

Field number for the "CacheTTL" field.

<a name='F-MQContract-KubeMQ-SDK-Grpc-Request-ChannelFieldNumber'></a>
### ChannelFieldNumber `constants`

##### Summary

Field number for the "Channel" field.

<a name='F-MQContract-KubeMQ-SDK-Grpc-Request-ClientIDFieldNumber'></a>
### ClientIDFieldNumber `constants`

##### Summary

Field number for the "ClientID" field.

<a name='F-MQContract-KubeMQ-SDK-Grpc-Request-MetadataFieldNumber'></a>
### MetadataFieldNumber `constants`

##### Summary

Field number for the "Metadata" field.

<a name='F-MQContract-KubeMQ-SDK-Grpc-Request-ReplyChannelFieldNumber'></a>
### ReplyChannelFieldNumber `constants`

##### Summary

Field number for the "ReplyChannel" field.

<a name='F-MQContract-KubeMQ-SDK-Grpc-Request-RequestIDFieldNumber'></a>
### RequestIDFieldNumber `constants`

##### Summary

Field number for the "RequestID" field.

<a name='F-MQContract-KubeMQ-SDK-Grpc-Request-RequestTypeDataFieldNumber'></a>
### RequestTypeDataFieldNumber `constants`

##### Summary

Field number for the "RequestTypeData" field.

<a name='F-MQContract-KubeMQ-SDK-Grpc-Request-SpanFieldNumber'></a>
### SpanFieldNumber `constants`

##### Summary

Field number for the "Span" field.

<a name='F-MQContract-KubeMQ-SDK-Grpc-Request-TagsFieldNumber'></a>
### TagsFieldNumber `constants`

##### Summary

Field number for the "Tags" field.

<a name='F-MQContract-KubeMQ-SDK-Grpc-Request-TimeoutFieldNumber'></a>
### TimeoutFieldNumber `constants`

##### Summary

Field number for the "Timeout" field.

<a name='T-MQContract-KubeMQ-SDK-Grpc-Response'></a>
## Response `type`

##### Namespace

MQContract.KubeMQ.SDK.Grpc

<a name='F-MQContract-KubeMQ-SDK-Grpc-Response-BodyFieldNumber'></a>
### BodyFieldNumber `constants`

##### Summary

Field number for the "Body" field.

<a name='F-MQContract-KubeMQ-SDK-Grpc-Response-CacheHitFieldNumber'></a>
### CacheHitFieldNumber `constants`

##### Summary

Field number for the "CacheHit" field.

<a name='F-MQContract-KubeMQ-SDK-Grpc-Response-ClientIDFieldNumber'></a>
### ClientIDFieldNumber `constants`

##### Summary

Field number for the "ClientID" field.

<a name='F-MQContract-KubeMQ-SDK-Grpc-Response-ErrorFieldNumber'></a>
### ErrorFieldNumber `constants`

##### Summary

Field number for the "Error" field.

<a name='F-MQContract-KubeMQ-SDK-Grpc-Response-ExecutedFieldNumber'></a>
### ExecutedFieldNumber `constants`

##### Summary

Field number for the "Executed" field.

<a name='F-MQContract-KubeMQ-SDK-Grpc-Response-MetadataFieldNumber'></a>
### MetadataFieldNumber `constants`

##### Summary

Field number for the "Metadata" field.

<a name='F-MQContract-KubeMQ-SDK-Grpc-Response-ReplyChannelFieldNumber'></a>
### ReplyChannelFieldNumber `constants`

##### Summary

Field number for the "ReplyChannel" field.

<a name='F-MQContract-KubeMQ-SDK-Grpc-Response-RequestIDFieldNumber'></a>
### RequestIDFieldNumber `constants`

##### Summary

Field number for the "RequestID" field.

<a name='F-MQContract-KubeMQ-SDK-Grpc-Response-SpanFieldNumber'></a>
### SpanFieldNumber `constants`

##### Summary

Field number for the "Span" field.

<a name='F-MQContract-KubeMQ-SDK-Grpc-Response-TagsFieldNumber'></a>
### TagsFieldNumber `constants`

##### Summary

Field number for the "Tags" field.

<a name='F-MQContract-KubeMQ-SDK-Grpc-Response-TimestampFieldNumber'></a>
### TimestampFieldNumber `constants`

##### Summary

Field number for the "Timestamp" field.

<a name='T-MQContract-KubeMQ-SDK-Grpc-Result'></a>
## Result `type`

##### Namespace

MQContract.KubeMQ.SDK.Grpc

<a name='F-MQContract-KubeMQ-SDK-Grpc-Result-ErrorFieldNumber'></a>
### ErrorFieldNumber `constants`

##### Summary

Field number for the "Error" field.

<a name='F-MQContract-KubeMQ-SDK-Grpc-Result-EventIDFieldNumber'></a>
### EventIDFieldNumber `constants`

##### Summary

Field number for the "EventID" field.

<a name='F-MQContract-KubeMQ-SDK-Grpc-Result-SentFieldNumber'></a>
### SentFieldNumber `constants`

##### Summary

Field number for the "Sent" field.

<a name='T-MQContract-KubeMQ-SDK-Grpc-SendQueueMessageResult'></a>
## SendQueueMessageResult `type`

##### Namespace

MQContract.KubeMQ.SDK.Grpc

<a name='F-MQContract-KubeMQ-SDK-Grpc-SendQueueMessageResult-DelayedToFieldNumber'></a>
### DelayedToFieldNumber `constants`

##### Summary

Field number for the "DelayedTo" field.

<a name='F-MQContract-KubeMQ-SDK-Grpc-SendQueueMessageResult-ErrorFieldNumber'></a>
### ErrorFieldNumber `constants`

##### Summary

Field number for the "Error" field.

<a name='F-MQContract-KubeMQ-SDK-Grpc-SendQueueMessageResult-ExpirationAtFieldNumber'></a>
### ExpirationAtFieldNumber `constants`

##### Summary

Field number for the "ExpirationAt" field.

<a name='F-MQContract-KubeMQ-SDK-Grpc-SendQueueMessageResult-IsErrorFieldNumber'></a>
### IsErrorFieldNumber `constants`

##### Summary

Field number for the "IsError" field.

<a name='F-MQContract-KubeMQ-SDK-Grpc-SendQueueMessageResult-MessageIDFieldNumber'></a>
### MessageIDFieldNumber `constants`

##### Summary

Field number for the "MessageID" field.

<a name='F-MQContract-KubeMQ-SDK-Grpc-SendQueueMessageResult-SentAtFieldNumber'></a>
### SentAtFieldNumber `constants`

##### Summary

Field number for the "SentAt" field.

<a name='T-MQContract-KubeMQ-SDK-Grpc-StreamQueueMessagesRequest'></a>
## StreamQueueMessagesRequest `type`

##### Namespace

MQContract.KubeMQ.SDK.Grpc

<a name='F-MQContract-KubeMQ-SDK-Grpc-StreamQueueMessagesRequest-ChannelFieldNumber'></a>
### ChannelFieldNumber `constants`

##### Summary

Field number for the "Channel" field.

<a name='F-MQContract-KubeMQ-SDK-Grpc-StreamQueueMessagesRequest-ClientIDFieldNumber'></a>
### ClientIDFieldNumber `constants`

##### Summary

Field number for the "ClientID" field.

<a name='F-MQContract-KubeMQ-SDK-Grpc-StreamQueueMessagesRequest-ModifiedMessageFieldNumber'></a>
### ModifiedMessageFieldNumber `constants`

##### Summary

Field number for the "ModifiedMessage" field.

<a name='F-MQContract-KubeMQ-SDK-Grpc-StreamQueueMessagesRequest-RefSequenceFieldNumber'></a>
### RefSequenceFieldNumber `constants`

##### Summary

Field number for the "RefSequence" field.

<a name='F-MQContract-KubeMQ-SDK-Grpc-StreamQueueMessagesRequest-RequestIDFieldNumber'></a>
### RequestIDFieldNumber `constants`

##### Summary

Field number for the "RequestID" field.

<a name='F-MQContract-KubeMQ-SDK-Grpc-StreamQueueMessagesRequest-StreamRequestTypeDataFieldNumber'></a>
### StreamRequestTypeDataFieldNumber `constants`

##### Summary

Field number for the "StreamRequestTypeData" field.

<a name='F-MQContract-KubeMQ-SDK-Grpc-StreamQueueMessagesRequest-VisibilitySecondsFieldNumber'></a>
### VisibilitySecondsFieldNumber `constants`

##### Summary

Field number for the "VisibilitySeconds" field.

<a name='F-MQContract-KubeMQ-SDK-Grpc-StreamQueueMessagesRequest-WaitTimeSecondsFieldNumber'></a>
### WaitTimeSecondsFieldNumber `constants`

##### Summary

Field number for the "WaitTimeSeconds" field.

<a name='T-MQContract-KubeMQ-SDK-Grpc-StreamQueueMessagesResponse'></a>
## StreamQueueMessagesResponse `type`

##### Namespace

MQContract.KubeMQ.SDK.Grpc

<a name='F-MQContract-KubeMQ-SDK-Grpc-StreamQueueMessagesResponse-ErrorFieldNumber'></a>
### ErrorFieldNumber `constants`

##### Summary

Field number for the "Error" field.

<a name='F-MQContract-KubeMQ-SDK-Grpc-StreamQueueMessagesResponse-IsErrorFieldNumber'></a>
### IsErrorFieldNumber `constants`

##### Summary

Field number for the "IsError" field.

<a name='F-MQContract-KubeMQ-SDK-Grpc-StreamQueueMessagesResponse-MessageFieldNumber'></a>
### MessageFieldNumber `constants`

##### Summary

Field number for the "Message" field.

<a name='F-MQContract-KubeMQ-SDK-Grpc-StreamQueueMessagesResponse-RequestIDFieldNumber'></a>
### RequestIDFieldNumber `constants`

##### Summary

Field number for the "RequestID" field.

<a name='F-MQContract-KubeMQ-SDK-Grpc-StreamQueueMessagesResponse-StreamRequestTypeDataFieldNumber'></a>
### StreamRequestTypeDataFieldNumber `constants`

##### Summary

Field number for the "StreamRequestTypeData" field.

<a name='T-MQContract-KubeMQ-SDK-Grpc-Subscribe'></a>
## Subscribe `type`

##### Namespace

MQContract.KubeMQ.SDK.Grpc

<a name='F-MQContract-KubeMQ-SDK-Grpc-Subscribe-ChannelFieldNumber'></a>
### ChannelFieldNumber `constants`

##### Summary

Field number for the "Channel" field.

<a name='F-MQContract-KubeMQ-SDK-Grpc-Subscribe-ClientIDFieldNumber'></a>
### ClientIDFieldNumber `constants`

##### Summary

Field number for the "ClientID" field.

<a name='F-MQContract-KubeMQ-SDK-Grpc-Subscribe-EventsStoreTypeDataFieldNumber'></a>
### EventsStoreTypeDataFieldNumber `constants`

##### Summary

Field number for the "EventsStoreTypeData" field.

<a name='F-MQContract-KubeMQ-SDK-Grpc-Subscribe-EventsStoreTypeValueFieldNumber'></a>
### EventsStoreTypeValueFieldNumber `constants`

##### Summary

Field number for the "EventsStoreTypeValue" field.

<a name='F-MQContract-KubeMQ-SDK-Grpc-Subscribe-GroupFieldNumber'></a>
### GroupFieldNumber `constants`

##### Summary

Field number for the "Group" field.

<a name='F-MQContract-KubeMQ-SDK-Grpc-Subscribe-SubscribeTypeDataFieldNumber'></a>
### SubscribeTypeDataFieldNumber `constants`

##### Summary

Field number for the "SubscribeTypeData" field.

<a name='T-MQContract-KubeMQ-SDK-Grpc-Request-Types'></a>
## Types `type`

##### Namespace

MQContract.KubeMQ.SDK.Grpc.Request

##### Summary

Container for nested types declared in the Request message type.

<a name='T-MQContract-KubeMQ-SDK-Grpc-Subscribe-Types'></a>
## Types `type`

##### Namespace

MQContract.KubeMQ.SDK.Grpc.Subscribe

##### Summary

Container for nested types declared in the Subscribe message type.

<a name='T-MQContract-KubeMQ-UnableToConnectException'></a>
## UnableToConnectException `type`

##### Namespace

MQContract.KubeMQ

##### Summary

Thrown when an error occurs attempting to connect to the KubeMQ server.  
Specifically this will be thrown when the Ping that is executed on each initial connection fails.

<a name='T-MQContract-KubeMQ-SDK-Grpc-kubemq'></a>
## kubemq `type`

##### Namespace

MQContract.KubeMQ.SDK.Grpc

<a name='P-MQContract-KubeMQ-SDK-Grpc-kubemq-Descriptor'></a>
### Descriptor `property`

##### Summary

Service descriptor

<a name='T-MQContract-KubeMQ-SDK-Grpc-kubemq-kubemqClient'></a>
## kubemqClient `type`

##### Namespace

MQContract.KubeMQ.SDK.Grpc.kubemq

##### Summary

Client for kubemq

<a name='M-MQContract-KubeMQ-SDK-Grpc-kubemq-kubemqClient-#ctor-Grpc-Core-ChannelBase-'></a>
### #ctor(channel) `constructor`

##### Summary

Creates a new client for kubemq

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| channel | [Grpc.Core.ChannelBase](#T-Grpc-Core-ChannelBase 'Grpc.Core.ChannelBase') | The channel to use to make remote calls. |

<a name='M-MQContract-KubeMQ-SDK-Grpc-kubemq-kubemqClient-#ctor-Grpc-Core-CallInvoker-'></a>
### #ctor(callInvoker) `constructor`

##### Summary

Creates a new client for kubemq that uses a custom `CallInvoker`.

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| callInvoker | [Grpc.Core.CallInvoker](#T-Grpc-Core-CallInvoker 'Grpc.Core.CallInvoker') | The callInvoker to use to make remote calls. |

<a name='M-MQContract-KubeMQ-SDK-Grpc-kubemq-kubemqClient-#ctor'></a>
### #ctor() `constructor`

##### Summary

Protected parameterless constructor to allow creation of test doubles.

##### Parameters

This constructor has no parameters.

<a name='M-MQContract-KubeMQ-SDK-Grpc-kubemq-kubemqClient-#ctor-Grpc-Core-ClientBase-ClientBaseConfiguration-'></a>
### #ctor(configuration) `constructor`

##### Summary

Protected constructor to allow creation of configured clients.

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| configuration | [Grpc.Core.ClientBase.ClientBaseConfiguration](#T-Grpc-Core-ClientBase-ClientBaseConfiguration 'Grpc.Core.ClientBase.ClientBaseConfiguration') | The client configuration. |

<a name='M-MQContract-KubeMQ-SDK-Grpc-kubemq-kubemqClient-NewInstance-Grpc-Core-ClientBase-ClientBaseConfiguration-'></a>
### NewInstance() `method`

##### Summary

Creates a new instance of client from given `ClientBaseConfiguration`.

##### Parameters

This method has no parameters.

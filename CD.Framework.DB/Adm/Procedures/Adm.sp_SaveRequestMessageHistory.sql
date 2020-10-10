CREATE PROCEDURE [Adm].[sp_SaveRequestMessageHistory]
	@requestmessages [Adm].[UDTT_RequestMessages] READONLY,
	@cachevaliduntil [datetimeoffset](7)
AS
INSERT INTO [Adm].[RequestMessageHistory]
(
	   [RequestId]
      ,[CacheValid]
      ,[CacheValidUntil]
      ,[InitMessage_MessageId]
      ,[ResponseMessage_MessageId]
)
SELECT
	   m.[RequestId]
      ,IIF(@cachevaliduntil > GETDATE(), 1, 0)
      ,@cachevaliduntil
      ,iniRequest.MessageId [InitMessage_MessageId]
      ,m.MessageId [ResponseMessage_MessageId]
FROM @requestmessages m
INNER JOIN [Adm].[RequestMessages] iniRequest ON iniRequest.MessageType = N'RequestCreated' AND iniRequest.RequestId = m.RequestId
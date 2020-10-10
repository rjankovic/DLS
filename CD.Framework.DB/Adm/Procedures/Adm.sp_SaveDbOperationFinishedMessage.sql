CREATE PROCEDURE [Adm].[sp_SaveDbOperationFinishedMessage]
	@requestId UNIQUEIDENTIFIER
AS

DECLARE @messageId UNIQUEIDENTIFIER = NEWID()

INSERT INTO [Adm].[RequestMessages]
(
	   [MessageId]
      ,[RequestId]
      ,[Content]
      ,[RequestForCoreType]
      ,[RequestProcessingMethod]
      ,[MessageFromId]
      ,[MessageOriginName]
      ,[MessageOriginId]
      ,[MessageFromName]
      ,[MessageToObjectId]
      ,[MessageToProjectId]
      ,[MessageToObjectName]
      ,[MessageType]
      ,[CreatedDateTime]
      ,[Project_ProjectConfigId]
	  ,[CustomerCode]
	  ,RequestFromUserId
)
SELECT
	   @messageId
      ,rm.RequestId
      ,N'' [Content]
      ,rm.[RequestForCoreType]
      ,rm.[RequestProcessingMethod]
      ,rm.[MessageFromId]
      ,rm.[MessageOriginName]
      ,rm.[MessageOriginId]
      ,rm.[MessageFromName]
      ,rm.[MessageToObjectId]
      ,rm.[MessageToProjectId]
      ,rm.[MessageToObjectName]
      ,N'DbOperationFinished' [MessageType]
      ,GETDATE() [CreatedDateTime]
      ,rm.[Project_ProjectConfigId]
	  ,rm.[CustomerCode]
	  ,rm.RequestFromUserId
FROM adm.RequestMessages rm WHERE rm.MessageType = N'RequestCreated' AND rm.RequestId = @requestId
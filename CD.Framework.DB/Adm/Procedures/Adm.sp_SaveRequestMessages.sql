CREATE PROCEDURE [Adm].[sp_SaveRequestMessages]
	@requestmessages [Adm].[UDTT_RequestMessages] READONLY,
	@attachments [Adm].[UDTT_RequestMessageAttachments] READONLY
AS
DECLARE @rc INT

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
      --,[TypeName]
      ,[Project_ProjectConfigId]
	  ,[CustomerCode]
	  ,[RequestFromUserId]
)
SELECT
	   a.[MessageId]
      ,a.[RequestId]
      ,a.[Content]
      ,a.[RequestForCoreType]
      ,a.[RequestProcessingMethod]
      ,a.[MessageFromId]
      ,a.[MessageOriginName]
      ,a.[MessageOriginId]
      ,a.[MessageFromName]
      ,a.[MessageToObjectId]
      ,a.[MessageToProjectId]
      ,a.[MessageToObjectName]
      ,a.[MessageType]
      ,a.[CreatedDateTime]
      --,a.[TypeName]
      ,a.[Project_ProjectConfigId]
	  ,a.CustomerCode
	  ,a.RequestFromUserId
FROM @requestmessages a
LEFT JOIN adm.RequestMessages rm ON a.RequestId = rm.RequestId AND a.MessageType = rm.MessageType
WHERE rm.MessageId IS NULL

SELECT @@ROWCOUNT

--INSERT INTO [Adm].[RequestMessageAttachments]
--(
--		   [AttachmentId]
--           ,[Type]
--           ,[Name]
--           ,[Uri]
--           ,[MessageId]
--           ,[OriginalRequestMessage_MessageId]
--)
--SELECT 		[AttachmentId]
--           ,[Type]
--           ,[Name]
--           ,[Uri]
--           ,[MessageId]
--           ,[OriginalRequestMessage_MessageId]
--FROM @attachments   

--RETURN @rc
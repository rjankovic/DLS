CREATE FUNCTION [Adm].[f_GetMessageByRequestAndType]
(
@requestid UNIQUEIDENTIFIER,
@messagetype NVARCHAR(50)
)
RETURNS TABLE
AS RETURN
(
SELECT [MessageId]
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
	  ,RequestFromUserId
  FROM [Adm].[RequestMessages]
  WHERE RequestId = @requestid AND MessageType = @messagetype
)
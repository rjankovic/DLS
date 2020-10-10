CREATE FUNCTION [Adm].[f_GetRespoonseForRequest]
(
@requestId UNIQUEIDENTIFIER
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
	  ,[customerCode]
	  ,[RequestFromUserId]
  FROM [Adm].[RequestMessages]
  WHERE RequestId = @requestId AND MessageType NOT IN (N'RequestAcknowledged', N'RequestCreated', /*N'Progress',*/ N'DbOperationFinished')
)
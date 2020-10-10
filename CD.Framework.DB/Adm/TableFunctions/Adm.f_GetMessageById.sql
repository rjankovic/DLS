CREATE FUNCTION [Adm].[f_GetMessageById]
(
@messageid UNIQUEIDENTIFIER
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
  WHERE MessageId = @messageid
)
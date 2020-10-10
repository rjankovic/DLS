CREATE PROCEDURE [Adm].[sp_GetNewMessagesToObject]
(
@objectId UNIQUEIDENTIFIER
)
AS 
BEGIN TRAN


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
  WHERE MessageToObjectId = @objectId 
  AND Received = 0

  UPDATE [Adm].RequestMessages SET Received = 1 
	WHERE MessageToObjectId = @objectId
	AND Received = 0

  COMMIT
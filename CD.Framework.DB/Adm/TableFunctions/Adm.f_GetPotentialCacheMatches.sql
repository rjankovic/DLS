CREATE FUNCTION [Adm].[f_GetPotentialCacheMatches]
(
@projectconfigid UNIQUEIDENTIFIER,
@coretype NVARCHAR(50)
)
RETURNS TABLE
AS RETURN
(
SELECT m.[MessageId]
      ,m.[RequestId]
      ,m.[Content]
      ,m.[RequestForCoreType]
      ,m.[RequestProcessingMethod]
      ,m.[MessageFromId]
      ,m.[MessageOriginName]
      ,m.[MessageOriginId]
      ,m.[MessageFromName]
      ,m.[MessageToObjectId]
      ,m.[MessageToProjectId]
      ,m.[MessageToObjectName]
      ,m.[MessageType]
      ,m.[CreatedDateTime]
      --,m.[TypeName]
      ,m.[Project_ProjectConfigId]
  FROM [Adm].[RequestMessageHistory] h
  INNER JOIN [Adm].RequestMessages m ON h.InitMessage_MessageId = m.MessageId
  WHERE m.MessageToProjectId = @projectconfigid AND h.CacheValid = 1 AND h.CacheValidUntil >= GETDATE()
)
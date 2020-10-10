CREATE PROCEDURE [Adm].[sp_GetActiveBroadcastMessages]
AS

SELECT
 BroadcastMessageId,
 BroadcastMessageType,
 ProjectConfigId,
 Active,
 Content
FROM [Adm].[BroadcastMessages] 
WHERE Active = 1

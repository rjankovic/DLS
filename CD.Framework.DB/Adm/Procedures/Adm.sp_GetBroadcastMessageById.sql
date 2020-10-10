CREATE PROCEDURE [Adm].[sp_GetBroadcastMessageById]
	@BroadcastMessageId UNIQUEIDENTIFIER
AS

SELECT
 BroadcastMessageId,
 BroadcastMessageType,
 ProjectConfigId,
 Active,
 Content
FROM [Adm].[BroadcastMessages] 
WHERE BroadcastMessageId = @BroadcastMessageId

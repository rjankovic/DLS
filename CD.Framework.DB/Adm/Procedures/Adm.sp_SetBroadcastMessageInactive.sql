CREATE PROCEDURE [Adm].[sp_SetBroadcastMessageInactive]
	@BroadcastMessageId UNIQUEIDENTIFIER
AS

UPDATE adm.BroadcastMessages SET Active = 0 WHERE BroadcastMessageId = @BroadcastMessageId

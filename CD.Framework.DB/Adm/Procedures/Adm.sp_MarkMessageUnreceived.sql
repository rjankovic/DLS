CREATE PROCEDURE [Adm].[sp_MarkMessageUnreceived]
	@messageId UNIQUEIDENTIFIER
AS
	UPDATE adm.RequestMessages SET Received = 0 WHERE MessageId = @messageId
	

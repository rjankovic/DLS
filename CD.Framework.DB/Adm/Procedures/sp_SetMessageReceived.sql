CREATE PROCEDURE [Adm].[sp_SetMessageReceived]
	@messageId UNIQUEIDENTIFIER
AS
	UPDATE [Adm].RequestMessages SET Received = 1 
	WHERE MessageId = @messageId
GO

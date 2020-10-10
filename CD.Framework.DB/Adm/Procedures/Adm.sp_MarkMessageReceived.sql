CREATE PROCEDURE [Adm].[sp_MarkMessageReceived]
	@messageId UNIQUEIDENTIFIER
AS
DECLARE @res INT = 0
BEGIN TRAN
	UPDATE adm.RequestMessages SET Received = 1 WHERE Received = 0 AND MessageId = @messageId
	SET @res = @@ROWCOUNT
COMMIT
SELECT @res
CREATE PROCEDURE [Adm].[sp_WriteLog]
	@messageType NVARCHAR(100),
	@message NVARCHAR(MAX),
	@stackTrace NVARCHAR(MAX) = NULL
AS
INSERT INTO Adm.Log(MessageType, [Message], StackTrace)
VALUES(@messageType, @message, @stackTrace)
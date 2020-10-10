CREATE PROCEDURE [Adm].[sp_WriteLogInfo]
	@message NVARCHAR(MAX)
AS
INSERT INTO Adm.Log(MessageType, [Message], StackTrace)
VALUES(N'0', @message, NULL)
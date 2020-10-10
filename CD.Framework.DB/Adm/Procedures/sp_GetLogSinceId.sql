CREATE PROCEDURE [Adm].[sp_GetLogSinceId](@lastSeenLogId INT)
AS
SELECT LogId, CreatedDate, MessageType, Message, StackTrace FROM adm.Log WHERE LogId > @lastSeenLogId
GO

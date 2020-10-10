CREATE PROCEDURE [Adm].[sp_InvalidateRequestCache]
	@projectconfigid UNIQUEIDENTIFIER, 
	@coretype NVARCHAR(50) = NULL
AS
UPDATE h SET h.CacheValidUntil = GETDATE(), h.CacheValid = 0 
FROM adm.RequestMessageHistory h
INNER JOIN adm.RequestMessages m ON h.InitMessage_MessageId = m.MessageId
WHERE m.RequestForCoreType = ISNULL(@coreType, m.RequestForCoreType) AND m.MessageToProjectId = @projectconfigid
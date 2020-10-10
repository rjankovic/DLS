CREATE VIEW [Adm].[vw_ServiceUsage]
	AS
SELECT
r.RequestId, 
r.CreatedDateTime,
REPLACE(REPLACE(JSON_VALUE(r.Content, '$."$type"'), N'CD.DLS.API.', N''), N', CD.DLS.API', N'') RequestType,
u.UserId UserId,
u.DisplayName UserName
FROM adm.RequestMessages r
INNER JOIN adm.Users u ON r.RequestFromUserId = u.UserId
WHERE r.MessageType = N'RequestCreated' AND RequestFromUserId <> 0

CREATE PROCEDURE [Adm].[sp_GetAbandonedRequests]
AS

;WITH candidateRequests AS(
SELECT r.MessageId, r.RequestId, r.CreatedDateTime 
FROM adm.RequestMessages r
-- the request does not have a response
LEFT JOIN adm.RequestMessages resp ON r.RequestId = resp.RequestId AND resp.MessageType <> N'RequestCreated'
-- is not waiting for other requests
LEFT JOIN adm.RequestsWaitFor wf ON wf.RequestId = r.RequestId AND wf.Active = 1
WHERE 
	r.MessageType = N'RequestCreated' 
	AND resp.MessageId IS NULL 
	AND wf.RequestsWiaitForItemId IS NULL
	-- has been received
	AND r.Received = 1
)
-- figure out the "abandon time" - either the creation of the request or the time the last of the requests
-- this request has been waiting for finished
SELECT cr.RequestId, cr.MessageId, ISNULL(MAX(awm.CreatedDateTime), cr.CreatedDateTime) CreatedDateTime
--,DATEDIFF(MINUTE, ISNULL(MAX(awm.CreatedDateTime), cr.CreatedDateTime), GETDATE())
FROM candidateRequests cr
LEFT JOIN Adm.RequestsWaitFor wf ON cr.RequestId = wf.RequestId
LEFT JOIN adm.RequestMessages awm ON awm.RequestId = wf.WaitForRequestId AND awm.MessageType <> N'RequestCreated'
GROUP BY cr.RequestId, cr.CreatedDateTime, cr.MessageId
HAVING DATEDIFF(MINUTE, ISNULL(MAX(awm.CreatedDateTime), cr.CreatedDateTime), GETDATE()) >= 15

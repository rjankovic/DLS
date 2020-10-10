CREATE PROCEDURE [Adm].[sp_GetRequestsFinishedWaiting]
AS
BEGIN
BEGIN TRAN

-- those that are waited for but have RequestProcessed will no longer be waited for
DECLARE @finishedRequests Adm.UDTT_GuidList

INSERT INTO @finishedRequests(Id)
SELECT DISTINCT WaitForRequestId
FROM adm.RequestsWaitFor wf
INNER JOIN adm.RequestMessages m ON m.RequestId = wf.WaitForRequestId
WHERE Active = 1 AND m.MessageType = N'RequestProcessed'

UPDATE wf SET Active = 0 
FROM adm.RequestsWaitFor wf
INNER JOIN @finishedRequests f ON f.Id = wf.WaitForRequestId

-- those that were dependent on the newly finished requests and are not dependent on other active requests are done waiting
SELECT DISTINCT wf.RequestId 
FROM adm.RequestsWaitFor wf
INNER JOIN @finishedRequests f ON f.Id = wf.WaitForRequestId
LEFT JOIN RequestsWaitFor others ON others.RequestId = wf.RequestId AND others.Active = 1
WHERE others.RequestsWiaitForItemId IS NULL

COMMIT
END
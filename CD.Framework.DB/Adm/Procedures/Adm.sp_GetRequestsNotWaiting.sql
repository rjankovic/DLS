CREATE PROCEDURE [Adm].[sp_GetRequestsNotWaiting]
AS
BEGIN
BEGIN TRAN

SELECT DISTINCT rm.RequestId 
FROM adm.RequestMessages rm
LEFT JOIN RequestsWaitFor wf ON rm.RequestId = wf.RequestId AND wf.Active = 1
WHERE rm.MessageType = 'RequestCreated' AND wf.RequestsWiaitForItemId IS NULL

COMMIT
END
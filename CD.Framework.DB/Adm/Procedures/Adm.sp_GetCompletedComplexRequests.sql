CREATE PROCEDURE [Adm].[sp_GetCompletedComplexRequests]
AS
BEGIN

;WITH candidates AS
(
SELECT wf.RequestId, wf.WaitForRequestId, fd.MessageId FROM adm.RequestsWaitFor wf
-- have progress responses (are complex)
INNER JOIN adm.RequestMessages prog ON wf.RequestId = prog.RequestId AND prog.MessageType = N'Progress'
-- are not completed
LEFT JOIN adm.RequestMessages progf ON wf.RequestId = progf.RequestId AND progf.MessageType = N'RequestProcessed'
-- find finished dependencies (if any)
LEFT JOIN adm.RequestMessages fd ON fd.RequestId = wf.WaitForRequestId AND fd.MessageType = N'RequestProcessed'
WHERE progf.MessageId IS NULL
)
-- requests where all dependencies are finished
SELECT DISTINCT RequestId FROM candidates c
WHERE NOT EXISTS(SELECT TOP 1 1 FROM candidates unf WHERE c.RequestId = unf.RequestId AND unf.MessageId IS NULL)

END
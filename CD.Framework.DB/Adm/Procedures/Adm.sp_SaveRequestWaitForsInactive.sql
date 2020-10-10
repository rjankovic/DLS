CREATE PROCEDURE [Adm].[sp_SaveRequestWaitForsInactive]
	@requestId UNIQUEIDENTIFIER,
	@waitForRequests [Adm].[UDTT_GuidList] READONLY
AS
INSERT INTO [Adm].[RequestsWaitFor]
(
	   [RequestId]
      ,[WaitForRequestId]
	  ,Active
)
SELECT
	@requestId
	,w.Id
	,0
FROM @waitForRequests w
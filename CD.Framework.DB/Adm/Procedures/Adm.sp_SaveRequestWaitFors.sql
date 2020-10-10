CREATE PROCEDURE [Adm].[sp_SaveRequestWaitFors]
	@requestId UNIQUEIDENTIFIER,
	@waitForRequests [Adm].[UDTT_GuidList] READONLY
AS
INSERT INTO [Adm].[RequestsWaitFor]
(
	   [RequestId]
      ,[WaitForRequestId]
)
SELECT
	@requestId
	,w.Id
FROM @waitForRequests w
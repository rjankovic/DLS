CREATE PROCEDURE [Adm].[sp_WriteLogBatch]
	@log [Adm].[UDTT_Log] READONLY
AS
INSERT INTO [Adm].[Log]
(
	   [CreatedDate]
      ,[MessageType]
      ,[Message]
      ,[StackTrace]
)
SELECT
	   l.CreatedDate
      ,l.MessageType
      ,l.[Message]
      ,l.StackTrace
FROM @log l

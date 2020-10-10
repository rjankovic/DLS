CREATE PROCEDURE [Adm].[sp_WriteUserActionLogBatch]
	@log [Adm].[UDTT_UserActionLog] READONLY
AS
INSERT INTO [Adm].[UserActionLog]
(
		[CreatedDate]
		,[EventType]
		,[UserId]
		,[ApplicationName]
		,[FrameworkElement]
		,[DataContext]
		,[ExtendedProperties]
)
SELECT
	   l.[CreatedDate]
		,l.[EventType]
		,l.[UserId]
		,l.[ApplicationName]
		,l.[FrameworkElement]
		,l.[DataContext]
		,l.[ExtendedProperties]
FROM @log l

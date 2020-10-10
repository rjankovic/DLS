CREATE PROCEDURE [Adm].[sp_WaitingTest]
	@projectConfigId UNIQUEIDENTIFIER,
	@requestId UNIQUEIDENTIFIER,
	@minutes int = 20
AS

	 DECLARE @i INT = 0
	 DECLARE @message NVARCHAR(MAX)
	 WHILE @minutes > @i
	 BEGIN
		WAITFOR DELAY '00:01' 
		SET @i = @i + 1
		SET @message = N'Waited for ' + CONVERT(NVARCHAR(10), @i) + N' minutes'
		PRINT @message
		EXEC [Adm].[sp_WriteLogInfo] @message
	 END

GO
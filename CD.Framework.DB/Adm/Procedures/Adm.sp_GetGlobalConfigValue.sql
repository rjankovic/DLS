CREATE PROCEDURE [Adm].[sp_GetGlobalConfigValue]
	@key NVARCHAR(200)
AS
SELECT [Value] FROM adm.GlobalConfig WHERE [Key] = @key
CREATE PROCEDURE [Adm].[sp_SetGlobalConfigValue]
	@key NVARCHAR(200),
	@value NVARCHAR(MAX)
AS
IF(EXISTS (SELECT 1 FROM adm.GlobalConfig WHERE [Key] = @key))
	UPDATE adm.GlobalConfig SET [Value] = @value WHERE [Key] = @key
ELSE
	INSERT INTO adm.GlobalConfig([Key], [Value])
	VALUES(@key, @value)

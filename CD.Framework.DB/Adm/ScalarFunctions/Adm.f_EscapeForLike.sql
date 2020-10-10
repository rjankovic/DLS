CREATE FUNCTION [Adm].[f_EscapeForLike]
(
	@val nvarchar(MAX)
)
RETURNS nvarchar(MAX)
AS
BEGIN
	RETURN REPLACE(REPLACE(REPLACE(REPLACE(@val, '[', '[[]'), ']', '[]]'), '_', '[_]'), '%', '[%]')
END

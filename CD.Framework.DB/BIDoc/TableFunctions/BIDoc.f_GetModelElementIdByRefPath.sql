CREATE FUNCTION [BIDoc].[f_GetModelElementIdByRefPath]
(
	@projectconfigid UNIQUEIDENTIFIER,
	@path NVARCHAR(MAX)
)
RETURNS TABLE AS RETURN
(
SELECT [ModelElementId]
FROM [BIDoc].[ModelElements] WHERE ProjectConfigId = @projectconfigid AND RefPathPrefix = CONVERT(NVARCHAR(300), LEFT(@path, 300))
	AND RefPath = @path
)

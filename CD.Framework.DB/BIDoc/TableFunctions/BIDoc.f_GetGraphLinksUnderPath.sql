/*
Includes only links originating under the path
*/
CREATE FUNCTION [BIDoc].[f_GetGraphLinksUnderPath]
(
	@projectconfigid UNIQUEIDENTIFIER,
	@graphkind NVARCHAR(50),
	@path NVARCHAR(MAX),
	@linktype NVARCHAR(50) = NULL
)
RETURNS TABLE AS RETURN
(
SELECT [BasicGraphLinkId]
      ,[LinkType]
      ,[NodeFromId]
      ,[NodeToId]
	  ,l.ExtendedProperties
FROM [BIDoc].[BasicGraphLinks] l
INNER JOIN BasicGraphNodes n ON l.NodeFromId = n.BasicGraphNodeId
INNER JOIN ModelElements e ON n.SourceElementId = e.ModelElementId
WHERE 
e.RefPathPrefix >= LEFT(@path, 300) AND e.RefPathPrefix <= LEFT(@path, 300) + N'~' AND
n.GraphKind = @graphkind AND n.ProjectConfigId = @projectconfigid AND ISNULL(@linktype, l.LinkType) = l.LinkType
AND e.RefPath >= @path AND e.RefPath <= @path + N'~'

--LEFT(e.RefPath, LEN(@path)) = @path -- e.RefPath LIKE Adm.f_EscapeForLike(@path) + '%'  ESCAPE '\'
)

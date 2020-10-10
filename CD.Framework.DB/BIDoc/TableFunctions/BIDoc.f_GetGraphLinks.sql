CREATE FUNCTION [BIDoc].[f_GetGraphLinks]
(
	@projectconfigid UNIQUEIDENTIFIER,
	@graphkind NVARCHAR(50),
	@linktype NVARCHAR(50) = NULL
)
RETURNS TABLE AS RETURN
(
SELECT BasicGraphLinkId,LinkType,NodeFromId ,NodeToId,ExtendedProperties 
FROM   
(
SELECT *,Row_number() OVER(PARTITION BY nodeFromId,NodeToID ORDER BY nodeFromId,NodeToID) rn 
FROM [BIDoc].[BasicGraphLinks] l
INNER JOIN BIDoc.BasicGraphNodes n ON l.NodeFromId = n.BasicGraphNodeId
where n.GraphKind = @graphkind and n.ProjectConfigId = @projectconfigid  AND ISNULL(@linktype, l.LinkType) = l.LinkType
) t 
WHERE  rn = 1 

)

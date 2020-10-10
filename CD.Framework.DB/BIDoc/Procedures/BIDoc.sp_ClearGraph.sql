CREATE PROCEDURE [BIDoc].[sp_ClearGraph]
	@projectconfigid UNIQUEIDENTIFIER,
	@graphkind NVARCHAR(50)
AS
--DELETE d FROM [BIDoc].GraphDocuments d
--INNER JOIN [BIDoc].[BasicGraphNodes] n ON d.GraphNode_Id = n.BasicGraphNodeId
--WHERE n.ProjectConfigId = @projectconfigid AND n.GraphKind = @graphkind

/*
WHILE ((SELECT COUNT(*) FROM [BIDoc].[BasicGraphLinks] l
INNER JOIN [BIDoc].[BasicGraphNodes] n ON l.NodeFromId = n.BasicGraphNodeId
WHERE n.ProjectConfigId = @projectconfigid AND n.GraphKind = @graphkind
)> 0
)
*/

DECLARE @rc INT = 1

WHILE @rc > 0
BEGIN

DELETE TOP (100000) l FROM [BIDoc].[BasicGraphLinks] l WITH(tablock)
INNER JOIN [BIDoc].[BasicGraphNodes] n ON l.NodeFromId = n.BasicGraphNodeId
WHERE n.ProjectConfigId = @projectconfigid AND n.GraphKind = @graphkind

SET @rc = @@ROWCOUNT
END

SET @rc = 1

WHILE @rc > 0
BEGIN

DELETE TOP (100000) FROM [BIDoc].[BasicGraphNodes] WHERE ProjectConfigId = @projectconfigid AND GraphKind = @graphkind

SET @rc = @@ROWCOUNT
END

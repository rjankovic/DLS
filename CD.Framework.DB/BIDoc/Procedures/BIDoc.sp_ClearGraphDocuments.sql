CREATE PROCEDURE [BIDoc].[sp_ClearGraphDocuments]
	@projectconfigid UNIQUEIDENTIFIER,
	@graphkind NVARCHAR(50),
	@documentType NVARCHAR(50) = NULL
AS
DELETE d FROM [BIDoc].GraphDocuments d
INNER JOIN [BIDoc].[BasicGraphNodes] n ON d.GraphNode_Id = n.BasicGraphNodeId
WHERE n.ProjectConfigId = @projectconfigid AND n.GraphKind = @graphkind
AND d.DocumentType = ISNULL(@documentType, d.DocumentType)

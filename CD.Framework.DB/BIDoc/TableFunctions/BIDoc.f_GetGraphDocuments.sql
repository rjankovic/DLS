CREATE FUNCTION [BIDoc].[f_GetGraphDocuments]
(
	@projectconfigid UNIQUEIDENTIFIER,
	@graphkind NVARCHAR(50),
	@documenttype NVARCHAR(50) = NULL
)
RETURNS TABLE AS RETURN
(
SELECT [GraphDocumentId]
      ,[Content]
      ,[DocumentType]
      ,[GraphNode_Id]
  FROM [BIDoc].[GraphDocuments] d
INNER JOIN BasicGraphNodes n ON d.GraphNode_Id = n.BasicGraphNodeId
WHERE n.GraphKind = @graphkind AND n.ProjectConfigId = @projectconfigid AND ISNULL(@documenttype, d.DocumentType) = d.DocumentType
)

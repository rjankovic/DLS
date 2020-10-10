CREATE FUNCTION [BIDoc].[f_GetGraphDocumentsForNode]
(
	@nodeid INT,
	@documenttype NVARCHAR(50) = NULL
)
RETURNS TABLE AS RETURN
(
SELECT [GraphDocumentId]
      ,[Content]
      ,[DocumentType]
      ,[GraphNode_Id]
  FROM [BIDoc].[GraphDocuments] d
  --INNER JOIN [BIDoc].f_GetModelElements e ON d.NodeRefPath = e.RefPath 
  WHERE ISNULL(@documenttype, d.DocumentType) = d.DocumentType AND d.GraphNode_Id = @nodeid
)

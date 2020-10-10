CREATE FUNCTION [BIDoc].[f_GetGraphNodes]
(
	@projectconfigid UNIQUEIDENTIFIER,
	@graphkind NVARCHAR(50)
)
RETURNS TABLE AS RETURN
(
SELECT [BasicGraphNodeId]
      ,[Name]
      ,[NodeType]
      ,[Description]
      ,[ParentId]
      ,[GraphKind]
      ,[ProjectConfigId]
      ,[SourceElementId]
      ,[TopologicalOrder]
FROM BIDoc.BasicGraphNodes 
WHERE GraphKind = @graphkind AND ProjectConfigId = @projectconfigid
)

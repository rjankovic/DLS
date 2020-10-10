CREATE PROCEDURE [Inspect].[sp_GetExtendedGraphExplorerSuggestions]
(
	@projectconfigid UNIQUEIDENTIFIER,
	@graphkind NVARCHAR(50),
	@pattern NVARCHAR(200)
)
AS
SELECT TOP 100 
	   n.[BasicGraphNodeId]
      ,n.[Name]
      ,n.[NodeType]
      ,n.Description [Description]
      ,n.[ParentId]
      ,n.[GraphKind]
      ,n.[ProjectConfigId]
      ,n.[SourceElementId]
      ,n.[TopologicalOrder]
	  ,e.[RefPath]
	  ,DIFFERENCE(@pattern, n.Name)
	  ,DIFFERENCE(@pattern, n.Description)
FROM BIDoc.BasicGraphNodes n
INNER JOIN BIDoc.ModelElements e ON n.SourceElementId = e.ModelElementId
WHERE GraphKind = @graphkind AND n.ProjectConfigId = @projectconfigid 
AND (DIFFERENCE(@pattern, n.Name) >= 3 OR DIFFERENCE(@pattern, n.Description) >= 3)
ORDER BY DIFFERENCE(@pattern, n.Name) * DIFFERENCE(@pattern, n.Description) DESC

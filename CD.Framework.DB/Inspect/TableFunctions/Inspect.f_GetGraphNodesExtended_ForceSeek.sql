CREATE FUNCTION [Inspect].[f_GetGraphNodesExtended_ForceSeek]
(
	@projectconfigid UNIQUEIDENTIFIER,
	@graphkind NVARCHAR(50)
)
RETURNS TABLE AS RETURN
(
SELECT n.[BasicGraphNodeId]
      ,n.[Name]
      ,n.[NodeType]
      ,n.[Description]
      ,n.[ParentId]
      ,n.[GraphKind]
      ,n.[ProjectConfigId]
      ,n.[SourceElementId]
      ,n.[TopologicalOrder]
	  ,e.[RefPath]
	  ,td.TypeDescription
	  ,e.Type ElementType
	  ,dp.DescriptivePath
FROM BIDoc.BasicGraphNodes n WITH (INDEX(IX_BIDoc_BasicGraphNodes_GrphKind_ProjectConfigId_RefPathIntervalStart))
INNER JOIN BIDoc.ModelElements e WITH (FORCESEEK) ON e.ModelElementId = n.SourceElementId
INNER JOIN BIDoc.ModelElementTypeDescriptions td ON td.ElementType = e.Type
LEFT JOIN BIDoc.ModelElementDescriptivePaths dp ON dp.ModelElementId = e.ModelElementId
WHERE GraphKind = @graphkind AND n.ProjectConfigId = @projectconfigid
)

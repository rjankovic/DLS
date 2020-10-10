CREATE FUNCTION [Inspect].[f_GetGraphNodesByIdExtended]
(
	@nodeIds [BIDoc].[UDTT_IdList] READONLY
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
FROM @nodeIds ni
INNER JOIN BIDoc.BasicGraphNodes n ON n.BasicGraphNodeId = ni.Id
INNER JOIN BIDoc.ModelElements e ON e.ModelElementId = n.SourceElementId
INNER JOIN BIDoc.ModelElementTypeDescriptions td ON td.ElementType = e.Type
LEFT JOIN BIDoc.ModelElementDescriptivePaths dp ON dp.ModelElementId = e.ModelElementId
)

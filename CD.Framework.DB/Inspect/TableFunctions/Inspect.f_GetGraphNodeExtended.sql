﻿CREATE FUNCTION [Inspect].[f_GetGraphNodeExtended]
(
	@nodeId INT
)
RETURNS TABLE AS RETURN
(
SELECT n.[BasicGraphNodeId]
      ,n.[Name]
      ,n.[NodeType]
      ,e.Definition [Description]
      ,n.[ParentId]
      ,n.[GraphKind]
      ,n.[ProjectConfigId]
      ,n.[SourceElementId]
      ,n.[TopologicalOrder]
	  ,e.[RefPath]
	  ,td.TypeDescription
	  ,e.Type ElementType
	  ,dp.DescriptivePath
FROM BIDoc.BasicGraphNodes n
INNER JOIN BIDoc.ModelElements e ON e.ModelElementId = n.SourceElementId
INNER JOIN BIDoc.ModelElementTypeDescriptions td ON td.ElementType = e.Type
LEFT JOIN BIDoc.ModelElementDescriptivePaths dp ON dp.ModelElementId = e.ModelElementId
WHERE BasicGraphNodeId = @nodeId
)

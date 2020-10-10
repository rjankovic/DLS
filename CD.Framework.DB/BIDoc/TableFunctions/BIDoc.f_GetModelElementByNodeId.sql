CREATE FUNCTION [BIDoc].[f_GetModelElementByNodeId]
(
	@nodeId INT
)
RETURNS TABLE AS RETURN
(
SELECT e.[ModelElementId]
      ,e.[ExtendedProperties]
      ,e.[RefPath]
      ,e.[Definition]
      ,e.[Caption]
      ,e.[Type]
      ,e.[ProjectConfigId]
  FROM [BIDoc].[ModelElements] e
  INNER JOIN BIDoc.BasicGraphNodes n ON n.SourceElementId = e.ModelElementId 
  WHERE n.BasicGraphNodeId = @nodeId
)

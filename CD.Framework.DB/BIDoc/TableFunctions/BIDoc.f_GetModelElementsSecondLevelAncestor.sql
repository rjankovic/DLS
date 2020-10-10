CREATE FUNCTION [BIDoc].[f_GetModelElementsSecondLevelAncestor]
(
	@rootId INT
)
RETURNS TABLE AS RETURN
(
SELECT DISTINCT [ModelElementId]
      ,[ExtendedProperties]
      ,[RefPath]
      ,[Definition]
      ,[Caption]
      ,[Type]
      ,[ProjectConfigId]
  FROM [BIDoc].[ModelElements] e 
  INNER JOIN BIDoc.HigherLevelElementAncestors a ON a.AncestorElementId = @rootId AND a.DetailLevel = 2 AND a.SouceElementId = e.ModelElementId
)

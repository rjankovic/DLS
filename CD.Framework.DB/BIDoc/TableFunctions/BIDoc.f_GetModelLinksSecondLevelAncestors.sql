CREATE FUNCTION [BIDoc].[f_GetModelLinksSecondLevelAncestors]
(
	@rootId INT
)
RETURNS TABLE AS RETURN
(
SELECT DISTINCT l.[ModelLinkId]
      ,l.[ElementFromId]
      ,l.[ElementToId]
      ,l.[Type]
      ,l.[ExtendedProperties]
  FROM [BIDoc].[ModelLinks] l
  INNER JOIN [BIDoc].ModelElements ef ON l.ElementFromId = ef.ModelElementId
  INNER JOIN [BIDoc].ModelElements et ON l.ElementToId = et.ModelElementId
  INNER JOIN BIDoc.HigherLevelElementAncestors afrom ON afrom.AncestorElementId = @rootId AND afrom.DetailLevel = 2 AND afrom.SouceElementId = ef.ModelElementId
  INNER JOIN BIDoc.HigherLevelElementAncestors ato ON ato.AncestorElementId = @rootId AND ato.DetailLevel = 2 AND ato.SouceElementId = et.ModelElementId
)

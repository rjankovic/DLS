CREATE FUNCTION [BIDoc].[f_GetModelLinks]
(
	@projectconfigid UNIQUEIDENTIFIER
)
RETURNS TABLE AS RETURN
(
SELECT l.[ModelLinkId]
      ,l.[ElementFromId]
      ,l.[ElementToId]
      ,l.[Type]
      ,l.[ExtendedProperties]
  FROM [BIDoc].[ModelLinks] l
  INNER JOIN [BIDoc].ModelElements e ON l.ElementFromId = e.ModelElementId
  WHERE e.ProjectConfigId = @projectconfigid
)

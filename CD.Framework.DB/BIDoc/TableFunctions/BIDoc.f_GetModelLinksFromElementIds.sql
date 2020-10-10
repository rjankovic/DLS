CREATE FUNCTION [BIDoc].[f_GetModelLinksFromElementIds]
(
	@idList [BIDoc].[UDTT_IdList] READONLY
)
RETURNS TABLE AS RETURN
(
SELECT l.[ModelLinkId]
      ,l.[ElementFromId]
      ,l.[ElementToId]
      ,l.[Type]
      ,l.[ExtendedProperties]
  FROM [BIDoc].[ModelLinks] l
  INNER JOIN @idList lst ON l.ElementFromId = lst.Id
  WHERE l.Type <> N'parent'
)

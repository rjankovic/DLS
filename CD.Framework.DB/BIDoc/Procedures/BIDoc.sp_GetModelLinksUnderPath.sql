CREATE PROCEDURE [BIDoc].[sp_GetModelLinksUnderPath]
--(
	@projectconfigid UNIQUEIDENTIFIER,
	@path NVARCHAR(MAX)
AS
/*
)
RETURNS TABLE AS RETURN
(
*/

DECLARE @elementId INT = (SELECT [ModelElementId] FROM [BIDoc].[f_GetModelElementIdByRefPath] (@projectConfigId, @path))
DECLARE 
	@intervalFrom INT,
	@intervalTo INT

SELECT @intervalFrom = e.RefPathIntervalStart, @intervalTo = e.RefPathIntervalEnd FROM BIDoc.ModelElements e WHERE e.ModelElementId = @elementId

SELECT DISTINCT l.[ModelLinkId]
      ,l.[ElementFromId]
      ,l.[ElementToId]
      ,l.[Type]
      ,l.[ExtendedProperties]
  FROM [BIDoc].[ModelLinks] l
  INNER JOIN [BIDoc].ModelElements ef ON l.ElementFromId = ef.ModelElementId
  INNER JOIN [BIDoc].ModelElements et ON l.ElementToId = et.ModelElementId
  WHERE 
  ef.RefPathIntervalStart BETWEEN @intervalFrom AND @intervalTo
  AND et.RefPathIntervalStart BETWEEN @intervalFrom AND @intervalTo
  --LEFT(ef.RefPath, LEN(@path)) = @path AND LEFT(et.RefPath, LEN(@path)) = @path --ef.RefPath LIKE  Adm.f_EscapeForLike(@path) + '%'  ESCAPE '\' AND et.RefPath LIKE Adm.f_EscapeForLike(@path) + '%'  ESCAPE '\'
	AND ef.ProjectConfigId = @projectconfigid AND et.ProjectConfigId = @projectconfigid
--)

﻿CREATE PROCEDURE [BIDoc].[sp_GetModelElementsUnderPathToChildrenOfTypesNoDef]
/*
(
*/
	@projectconfigid UNIQUEIDENTIFIER,
	@path NVARCHAR(MAX),
	@types BIDoc.UDTT_StringList READONLY
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

;
WITH elems AS(
SELECT
		e.[ModelElementId]
      ,e.[ExtendedProperties]
      ,e.[RefPath]
      ,NULL [Definition]
      ,e.[Caption]
      ,e.[Type]
      ,e.[ProjectConfigId]
  FROM [BIDoc].[ModelElements] e 
  -- the ancestors - smaller refapths, but still prefixes
  --INNER JOIN BIDoc.ModelElements ance ON ance.RefPathPrefix <= e.RefPathPrefix AND ance.RefPathPrefix + N'~' >= e.RefPathPrefix
  --AND ance.RefPath <= e.RefPath AND ance.RefPath + N'~' >= e.RefPath
  INNER JOIN @types t ON t.Value = e.Type
  WHERE 
  (
  @path = N''
  OR
  (
  e.RefPathIntervalStart BETWEEN @intervalFrom AND @intervalTo
  --e.RefPathPrefix >= LEFT(@path, 300) AND e.RefPathPrefix <= LEFT(@path, 300) + N'~' AND 
  --e.RefPath >= @path AND e.RefPath <= @path + N'~'
  )
  )
  --AND e.[Type] = @type

  UNION ALL
  -- ancestors
  SELECT e.[ModelElementId]
      ,e.[ExtendedProperties]
      ,e.[RefPath]
      ,NULL [Definition]
      ,e.[Caption]
      ,e.[Type]
      ,e.[ProjectConfigId]
  FROM elems
  INNER JOIN BIDoc.ModelLinks l ON l.Type = N'parent' AND l.ElementFromId = elems.ModelElementId
  INNER JOIN BIDoc.ModelElements e ON e.ModelElementId = l.ElementToId
  WHERE 
  (
  @path = N''
  OR
  (
  e.RefPathIntervalStart BETWEEN @intervalFrom AND @intervalTo
  --e.RefPathPrefix >= LEFT(@path, 300) AND --e.RefPathPrefix <= LEFT(@path, 300) + N'~' AND 
  --e.RefPath >= @path --AND e.RefPath <= @path + N'~'
  )
  )
)
SELECT DISTINCT * FROM elems
--)


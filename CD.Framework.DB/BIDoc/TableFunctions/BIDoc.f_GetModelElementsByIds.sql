CREATE FUNCTION [BIDoc].[f_GetModelElementsByIds]
(
	@idList [BIDoc].[UDTT_IdList] READONLY
)
RETURNS TABLE AS RETURN
(
SELECT [ModelElementId]
      ,[ExtendedProperties]
      ,[RefPath]
      ,[Definition]
      ,[Caption]
      ,[Type]
      ,[ProjectConfigId]
  FROM [BIDoc].[ModelElements] e
  INNER JOIN @idList l ON e.ModelElementId = l.Id

  UNION ALL

SELECT DISTINCT
		e.[ModelElementId]
      ,e.[ExtendedProperties]
      ,e.[RefPath]
      ,e.[Definition]
      ,e.[Caption]
      ,e.[Type]
      ,e.[ProjectConfigId]
  FROM @idList l
  INNER JOIN BIDoc.ModelLinks lnk ON lnk.ElementFromId = l.Id
  INNER JOIN BIDoc.ModelElements e ON e.ModelElementId = lnk.ElementToId
  LEFT JOIN @idList ld ON ld.Id = e.ModelElementId
  WHERE lnk.Type <> N'parent' AND ld.Id IS NULL
  
)

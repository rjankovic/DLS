CREATE PROCEDURE [BIDoc].[sp_GetModelElementsUnderPathNoDef]
--(
	@projectconfigid UNIQUEIDENTIFIER,
	@path NVARCHAR(MAX),
	@type NVARCHAR(200) = NULL
AS
--)
/*
SELECT DISTINCT [ModelElementId]
      ,[ExtendedProperties]
      ,[RefPath]
      ,NULL [Definition]
      ,[Caption]
      ,[Type]
      ,[ProjectConfigId]
  FROM [BIDoc].[ModelElements] WHERE
  LEN(@path) < 300 AND
  RefPathPrefix >= @path AND RefPathPrefix <= @path + N'~'
  AND [Type] = ISNULL(@type, [Type])

UNION ALL

SELECT DISTINCT [ModelElementId]
      ,[ExtendedProperties]
      ,[RefPath]
      ,NULL [Definition]
      ,[Caption]
      ,[Type]
      ,[ProjectConfigId]
  FROM [BIDoc].[ModelElements] WHERE
  LEN(@path) >= 300 AND
  RefPathPrefix >= LEFT(@path, 300) AND RefPathPrefix <= LEFT(@path, 300) + N'~' AND 
  RefPath >= @path AND RefPath <= @path + N'~' -- LEFT(RefPath, LEN(@path)) = @path --RefPath LIKE Adm.f_EscapeForLike(@path) + '%' ESCAPE '\' AND ProjectConfigId = @projectconfigid
  AND [Type] = ISNULL(@type, [Type])
)
*/


DECLARE @elementId INT = (SELECT [ModelElementId] FROM [BIDoc].[f_GetModelElementIdByRefPath] (@projectConfigId, @path))
DECLARE 
	@intervalFrom INT,
	@intervalTo INT

SELECT @intervalFrom = e.RefPathIntervalStart, @intervalTo = e.RefPathIntervalEnd FROM BIDoc.ModelElements e WHERE e.ModelElementId = @elementId


SELECT DISTINCT [ModelElementId]
      ,[ExtendedProperties]
      ,[RefPath]
      ,NULL [Definition]
      ,[Caption]
      ,[Type]
      ,[ProjectConfigId]
  FROM [BIDoc].[ModelElements] 
  WHERE ProjectConfigId = @projectconfigid
  AND RefPathIntervalStart BETWEEN @intervalFrom AND @intervalTo
  AND [Type] = ISNULL(@type, [Type])
  
  /*
  RefPathPrefix >= LEFT(@path, 300) AND RefPathPrefix <= LEFT(@path, 300) + N'~' AND 
  RefPath >= @path AND RefPath <= @path + N'~' -- LEFT(RefPath, LEN(@path)) = @path --RefPath LIKE Adm.f_EscapeForLike(@path) + '%' ESCAPE '\' AND ProjectConfigId = @projectconfigid
  */
  
  

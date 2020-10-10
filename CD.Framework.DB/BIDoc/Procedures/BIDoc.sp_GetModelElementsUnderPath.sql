CREATE PROCEDURE [BIDoc].[sp_GetModelElementsUnderPath]
--(
	@projectconfigid UNIQUEIDENTIFIER,
	@path NVARCHAR(MAX),
	@type NVARCHAR(200) = NULL
AS
--)
--RETURNS TABLE AS RETURN
--(

DECLARE @elementId INT = (SELECT [ModelElementId] FROM [BIDoc].[f_GetModelElementIdByRefPath] (@projectConfigId, @path))
DECLARE 
	@intervalFrom INT,
	@intervalTo INT

SELECT @intervalFrom = e.RefPathIntervalStart, @intervalTo = e.RefPathIntervalEnd FROM BIDoc.ModelElements e WHERE e.ModelElementId = @elementId


SELECT DISTINCT [ModelElementId]
      ,[ExtendedProperties]
      ,[RefPath]
      ,[Definition]
      ,[Caption]
      ,[Type]
      ,[ProjectConfigId]
  FROM [BIDoc].[ModelElements]
  WHERE ProjectConfigId = @projectconfigid
  AND RefPathIntervalStart BETWEEN @intervalFrom AND @intervalTo
  AND [Type] = ISNULL(@type, [Type])
  
  --RefPathPrefix >= LEFT(@path, 300) AND RefPathPrefix <= LEFT(@path, 300) + N'~' AND 
  --RefPath >= @path AND RefPath <= @path + N'~' -- LEFT(RefPath, LEN(@path)) = @path --RefPath LIKE Adm.f_EscapeForLike(@path) + '%' ESCAPE '\' AND ProjectConfigId = @projectconfigid
  --AND [Type] = ISNULL(@type, [Type])


--)

CREATE PROCEDURE [Inspect].[sp_GetModelElementsUnderPathDisplay]
--(
	@projectconfigid UNIQUEIDENTIFIER,
	@path NVARCHAR(MAX),
	@type NVARCHAR(200) = NULL
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

SELECT e.[ModelElementId]
      ,[RefPath]
      ,[Caption]
      ,[Type]
	  ,td.TypeDescription
	  ,dp.DescriptivePath
  FROM [BIDoc].[ModelElements] e
  INNER JOIN BIDoc.ModelElementTypeDescriptions td ON e.Type = td.ElementType
  INNER JOIN BIDoc.ModelElementDescriptivePaths dp ON e.ModelElementId = dp.ModelElementId
  WHERE
  e.ProjectConfigId = @projectconfigid
  AND e.RefPathIntervalStart BETWEEN @intervalFrom AND @intervalTo
  --e.RefPathPrefix >= LEFT(@path, 300) AND e.RefPathPrefix <= LEFT(@path, 300) + N'~' AND 
  --e.RefPath >= @path AND RefPath <= @path + N'~' -- LEFT(RefPath, LEN(@path)) = @path --RefPath LIKE Adm.f_EscapeForLike(@path) + '%' ESCAPE '\' AND ProjectConfigId = @projectconfigid
  AND e.[Type] = ISNULL(@type, [Type])
--)

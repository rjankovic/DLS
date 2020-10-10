CREATE PROCEDURE [Search].[sp_FindFulltext]
	@projectConfigId UNIQUEIDENTIFIER, 
	@pattern NVARCHAR(MAX),
	@refPathPrefix NVARCHAR(300),
	@typeFilter [BIDoc].[UDTT_StringList] READONLY
AS
	--DECLARE @pattern NVARCHAR(MAX) = N'General Ledger'


DECLARE 
	@intervalFrom INT,
	@intervalTo INT,
	@annotationPrefix BIT = 0


DECLARE @elementId INT = (SELECT [ModelElementId] FROM [BIDoc].[f_GetModelElementIdByRefPath] (@projectConfigId, @refPathPrefix))

SELECT @intervalFrom = e.RefPathIntervalStart, @intervalTo = e.RefPathIntervalEnd FROM BIDoc.ModelElements e WHERE e.ModelElementId = @elementId

DECLARE @likePattern NVARCHAR(MAX) =  N'%' + [Adm].[f_EscapeForLike](@pattern) + N'%'
DECLARE @containsPattern NVARCHAR(1000) = N'ISABOUT(' + REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(TRIM(' ()[]&!|"' from @pattern), N' ', N', '), N'(', N''), N')', N''), N'[', N''), N']', N''), N'!', N''), N'|', N''), N'&', N''), N'"', N'') + N')'

;WITH r AS(
SELECT fts.ModelElementId, fts.ElementName, fts.TypeDescription /*td.TypeDescription*/, fts.DescriptiveRootPath /*dscPth.DescriptiveRootPath*/, 
fts.BusinessFields, KEY_TBL.RANK * fts.SearchPriority ResultPriority, fts.ElementType /*e.Type*/
,fts.RefPath
--,e.RefPathPrefix

FROM Search.FulltextSearch fts 
INNER JOIN CONTAINSTABLE(Search.FulltextSearch, (ElementName, ElementNameSplit, BusinessFields), @containsPattern ) AS KEY_TBL ON fts.FulltextSearchId = KEY_TBL.[KEY]
--INNER JOIN BIDoc.ModelElementDescriptivePaths dscPth ON dscPth.ModelElementId = fts.ModelElementId
--INNER JOIN BIDoc.ModelElements e ON e.ModelElementId = fts.ModelElementId
--INNER JOIN BIDoc.ModelElementTypeDescriptions td ON td.ElementType = e.[Type]
WHERE fts.ProjectConfigId = @projectConfigId

UNION

SELECT fts.ModelElementId, fts.ElementName, fts.TypeDescription, fts.DescriptiveRootPath, 
fts.BusinessFields, LEN(@pattern) * 5 * fts.SearchPriority ResultPriority, fts.ElementType
,fts.RefPath
--,e.RefPathPrefix
FROM Search.FulltextSearch fts 
--INNER JOIN BIDoc.ModelElementDescriptivePaths dscPth ON dscPth.ModelElementId = fts.ModelElementId
--INNER JOIN BIDoc.ModelElements e ON e.ModelElementId = fts.ModelElementId
--INNER JOIN BIDoc.ModelElementTypeDescriptions td ON td.ElementType = e.[Type]
WHERE (fts.ElementName LIKE @likePattern OR fts.BusinessFields LIKE @likePattern)
AND fts.ProjectConfigId = @projectConfigId
)

SELECT TOP 200 
r.ModelElementId, r.ElementName, r.TypeDescription, r.DescriptiveRootPath, 
r.BusinessFields, r.ElementType, MAX(r.ResultPriority) ResultPriority 
FROM r 
INNER JOIN @typeFilter tf ON tf.Value = r.ElementType
LEFT JOIN BIDoc.Modelelements me ON me.ModelElementId = r.ModelElementId
WHERE (@refPathPrefix = N'' OR (@annotationPrefix = 1 AND LEFT(r.RefPath, LEN(@refPathPrefix)) = @refPathPrefix) OR (me.RefPathIntervalStart BETWEEN @intervalFrom AND @intervalTo))
GROUP BY r.ModelElementId, r.ElementName, r.TypeDescription, r.DescriptiveRootPath, 
r.BusinessFields, r.ElementType
ORDER BY MAX(r.ResultPriority) DESC

RETURN 0

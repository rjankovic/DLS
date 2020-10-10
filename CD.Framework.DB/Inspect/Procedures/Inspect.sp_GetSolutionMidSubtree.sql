CREATE PROCEDURE [Inspect].[sp_GetSolutionMidSubtree]
@projectConfigId UNIQUEIDENTIFIER,
@rootElementId INT

AS

DECLARE @pathPrefix NVARCHAR(MAX) = (SELECT [Adm].[f_EscapeForLike]((SELECT RefPath FROM bidoc.ModelElements e WHERE e.ModelElementId = @rootElementId))) + N'%'

--SELECT @pathPrefix

;

WITH elemTree AS(
	SELECT e.ModelElementId, e.Caption, e.Type, e.RefPath, 0 ParentLevel
	FROM BIDoc.ModelElements e 
	WHERE e.ProjectConfigId = @projectConfigId AND e.Type IN (
	N'CD.DLS.Model.Mssql.Ssrs.ReportElement',
	N'CD.DLS.Model.Mssql.Ssis.PackageElement',
	N'CD.DLS.Model.Mssql.Ssas.PhysicalMeasureElement',
	N'CD.DLS.Model.Mssql.Ssas.ReportCalculatedMeasureElement',
	N'CD.DLS.Model.Mssql.Ssas.CubeCalculatedMeasureElement',
	N'CD.DLS.Model.Mssql.Ssas.DimensionAttributeElement',
	N'CD.DLS.Model.Mssql.Ssas.CubeDimensionElement',
	N'CD.DLS.Model.Mssql.Db.ColumnElement'
	) AND e.RefPath LIKE @pathPrefix ESCAPE N'\'

	UNION ALL

	SELECT e.ModelElementId, e.Caption, e.Type, e.RefPath, ParentLevel + 1
	FROM elemTree t
	INNER JOIN BIDoc.ModelLinks l ON l.ElementFromId = t.ModelElementId AND l.Type = N'parent'
	INNER JOIN BIDoc.ModelElements e ON e.ModelElementId = l.ElementToId
	WHERE e.RefPath LIKE @pathPrefix ESCAPE N'\'
)
,x AS(

SELECT ModelElementId, Caption, elemTree.[Type], td.TypeDescription, MAX(ParentLevel) MaxParentLevel, RefPath
FROM elemTree
INNER JOIN BIDoc.ModelElementTypeDescriptions td ON [Type] = td.ElementType
GROUP BY ModelElementId, Caption, elemTree.[Type], td.TypeDescription, RefPath
)
SELECT ModelElementId, Caption, x.[Type], TypeDescription, MaxParentLevel, l.ElementToId ParentElementId, RefPath
FROM x
LEFT JOIN BIDoc.ModelLinks l ON l.ElementFromId = ModelElementId AND l.Type = N'parent'

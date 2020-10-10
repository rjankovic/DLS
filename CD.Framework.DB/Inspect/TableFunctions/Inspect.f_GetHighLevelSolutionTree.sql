CREATE FUNCTION Inspect.f_GetHighLevelSolutionTree
(@projectConfigId UNIQUEIDENTIFIER)
RETURNS TABLE
AS RETURN
SELECT
	[ModelElementId],
	[Caption],
	[Type], 
	[TypeDescription], 
	[MaxParentLevel], 
	[ParentElementId], 
	[RefPath]
FROM Inspect.HighLevelSolutionTrees 
WHERE ProjectConfigId = @projectConfigId

/*
WITH elemTree AS(
	SELECT e.ModelElementId, e.Caption, e.Type, e.RefPath, 0 ParentLevel
	FROM BIDoc.ModelElements e 
	WHERE e.ProjectConfigId = @projectConfigId AND e.Type IN (
	N'CD.DLS.Model.Mssql.Ssrs.ReportElement',
	N'CD.DLS.Model.Mssql.Ssis.PackageElement',
	N'CD.DLS.Model.Mssql.Ssas.CubeElement',
	N'CD.DLS.Model.Mssql.Ssas.DimensionElement',
	N'CD.DLS.Model.Mssql.Ssas.MdxScriptElement',
	N'CD.DLS.Model.Mssql.Db.ViewElement',
	N'CD.DLS.Model.Mssql.Db.SchemaTableElement',
	N'CD.DLS.Model.Mssql.Ssas.MeasureGroupElement',
	N'CD.DLS.Model.Mssql.Db.ProcedureElement',
	N'CD.DLS.Model.Mssql.Tabular.ParsedTabularTableColumn'
	)

	UNION ALL

	SELECT e.ModelElementId, e.Caption, e.Type, e.RefPath, ParentLevel + 1
	FROM elemTree t
	INNER JOIN BIDoc.ModelLinks l ON l.ElementFromId = t.ModelElementId AND l.Type = N'parent'
	INNER JOIN BIDoc.ModelElements e ON e.ModelElementId = l.ElementToId
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
*/
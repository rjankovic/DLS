CREATE FUNCTION Inspect.f_GetHighLevelTypesUnderElement
(@projectConfigId UNIQUEIDENTIFIER
,@rootElementId INT
)
RETURNS TABLE
AS RETURN

SELECT de.DescendantType AS ElementType, td.TypeDescription ,de.NodeType 
FROM BIDoc.ModelElements e
INNER JOIN BIDoc.HighLevelTypeDescendants de ON de.ParentType = e.Type
INNER JOIN BIDoc.ModelElementTypeDescriptions td ON de.DescendantType = td.ElementType
INNER JOIN BIDoc.BasicGraphNodes n ON n.SourceElementId = e.ModelElementId
WHERE e.ModelElementId = @rootElementId AND n.GraphKind = N'DataFlow'

/*SELECT DISTINCT td.ElementType, td.TypeDescription, n.NodeType 
FROM BIDoc.ModelElements e
INNER JOIN BIDoc.ModelElements se ON /*e.RefPath = N'Solution' OR*/ LEFT(se.RefPath, LEN(e.RefPathPrefix)) = e.RefPathPrefix
INNER JOIN BIDoc.ModelElementTypeDescriptions td ON se.[Type] = td.ElementType
INNER JOIN BIDoc.BasicGraphNodes n ON n.SourceElementId = se.ModelElementId
WHERE e.ModelElementId = @rootElementId AND n.GraphKind = N'DataFlow'
AND se.Type IN (
	N'CD.DLS.Model.Mssql.Ssrs.ReportElement',
	N'CD.DLS.Model.Mssql.Ssrs.TextBoxElement',
	N'CD.DLS.Model.Mssql.Ssis.PackageElement',
	N'CD.DLS.Model.Mssql.Ssas.CubeElement',
	N'CD.DLS.Model.Mssql.Ssas.DimensionElement',
	N'CD.DLS.Model.Mssql.Ssas.DimensionAttributeElement',
	N'CD.DLS.Model.Mssql.Ssas.MdxScriptElement',
	N'CD.DLS.Model.Mssql.Db.DatabaseElement',
	N'CD.DLS.Model.Mssql.Db.ViewElement',
	N'CD.DLS.Model.Mssql.Db.SchemaTableElement',
	N'CD.DLS.Model.Mssql.Db.ColumnElement',
	N'CD.DLS.Model.Mssql.Ssas.MeasureGroupElement',
	N'CD.DLS.Model.Mssql.Ssas.PhysicalMeasureElement',
	N'CD.DLS.Model.Mssql.Ssas.CalculatedMeasureElement'
	/*
	,
	N'CD.DLS.Model.Mssql.Ssas.CubeDimensionElement'
	*/
	)*/
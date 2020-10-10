CREATE PROCEDURE [Search].[sp_IndexFulltext]
@projectConfigId UNIQUEIDENTIFIER
AS

BEGIN

DELETE fts FROM Search.FullTextSearch fts
--INNER JOIN BIDoc.ModelElements e ON e.ModelElementId = fts.ModelElementId
--WHERE e.ProjectConfigId = @projectConfigId
WHERE fts.ProjectConfigId = @projectConfigId

CREATE TABLE #searchTypes (TypeName NVARCHAR(MAX), SearchPriority INT)

INSERT INTO #searchTypes
(
    TypeName,
    SearchPriority
)
VALUES 

	(N'CD.DLS.Model.Mssql.Ssrs.ReportElement', 10),	
	(N'CD.DLS.Model.Mssql.Ssrs.TextBoxElement', 5),
	(N'CD.DLS.Model.Mssql.Ssis.PackageElement', 7),
	(N'CD.DLS.Model.Mssql.Ssas.CubeElement', 3),
	(N'CD.DLS.Model.Mssql.Db.DatabaseElement', 3),
	(N'CD.DLS.Model.Mssql.Ssis.ProjectElement', 2),
	(N'CD.DLS.Model.Mssql.Ssas.DimensionElement', 7),
	--(N'CD.DLS.Model.Mssql.Ssas.MdxScriptElement', 1),
	(N'CD.DLS.Model.Mssql.Db.ViewElement', 8),
	(N'CD.DLS.Model.Mssql.Db.SchemaTableElement', 9),
	--(N'CD.DLS.Model.Mssql.Ssas.MeasureGroupElement', 6),
	(N'CD.DLS.Model.Mssql.Db.ProcedureElement', 5),
	(N'CD.DLS.Model.Mssql.Ssas.PhysicalMeasureElement', 9),
	--(N'CD.DLS.Model.Mssql.Ssas.CalculatedMeasureElement', 9),
	(N'CD.DLS.Model.Mssql.Ssas.CubeCalculatedMeasureElement', 9),
	(N'CD.DLS.Model.Mssql.Ssas.ReportCalculatedMeasureElement', 5),
	(N'CD.DLS.Model.Mssql.Ssas.DimensionAttributeElement', 8),
	(N'CD.DLS.Model.Mssql.Db.ColumnElement', 7),

	(N'CD.DLS.Model.Business.Excel.PivotTableTemplateElement', 10),

	(N'CD.DLS.Model.Mssql.Tabular.SsasTabularTableElement', 9),
	(N'CD.DLS.Model.Mssql.Tabular.SsasTabularTableColumnElement', 8),

	(N'CD.DLS.Model.Mssql.Pbi.TenantElement', 10),
	(N'CD.DLS.Model.Mssql.Pbi.ReportElement', 10),
	(N'CD.DLS.Model.Mssql.Pbi.ReportSectionElement',8),
	(N'CD.DLS.Model.Mssql.Pbi.VisualElement', 5),
	(N'CD.DLS.Model.Mssql.Pbi.ConnectionElement', 5),
	(N'CD.DLS.Model.Mssql.Pbi.PbiTableElement', 7),
	(N'CD.DLS.Model.Mssql.Pbi.PbiColumnElement', 7)




	

INSERT INTO Search.FulltextSearch(ModelElementID, ElementName, ElementNameSplit, SearchPriority,
[TypeDescription], [DescriptiveRootPath], [ElementType], ProjectConfigId, [RefPath]
)
SELECT e.ModelElementId, e.Caption, adm.f_SplitCamelCase(e.Caption), st.SearchPriority,
td.TypeDescription, dscPth.DescriptiveRootPath, e.Type, @projectConfigId, e.RefPath
FROM BIDoc.ModelElements e 
INNER JOIN #searchTypes st ON e.Type = st.TypeName
INNER JOIN BIDoc.ModelElementDescriptivePaths dscPth ON dscPth.ModelElementId = e.ModelElementId
INNER JOIN BIDoc.ModelElementTypeDescriptions td ON td.ElementType = e.[Type]
WHERE e.ProjectConfigId = @projectConfigId

;WITH assignments AS
(
SELECT DISTINCT fts.ModelElementId, 
SUBSTRING(
        (
            SELECT ', ' + fv.Value AS [text()]
            FROM Annotate.FieldValues fv
            WHERE fv.AnnotationElementId = ae.AnnotationElementId
            AND fv.Value <> N''
			FOR XML PATH ('')
        ), 2, 10000) [BusinessValues]

FROM Search.FulltextSearch AS fts
LEFT JOIN Annotate.AnnotationElements ae ON fts.ModelElementId = ae.ModelElementId
WHERE ae.IsCurrentVersion = N'1'
AND ae.ProjectConfigId = @projectConfigId
)
UPDATE fts SET BusinessFields = a.BusinessValues 
FROM Search.FulltextSearch fts
INNER JOIN assignments a ON a.ModelElementId = fts.ModelElementId

ALTER FULLTEXT CATALOG fulltext_default REBUILD

DROP TABLE #searchTypes

END
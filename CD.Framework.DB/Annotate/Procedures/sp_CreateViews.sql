CREATE PROCEDURE [Annotate].[sp_CreateViews]
	@projectConfigId UNIQUEIDENTIFIER
AS
-- IN ANNOTATIONVIEW
;WITH viewNames AS
(
	SELECT CONVERT(NVARCHAR(255), N'Default') AS ViewName

	UNION 
	
	SELECT N'Type_' + hld.DescendantType FROM [BIDoc].[HighLevelTypeDescendants] hld
	WHERE hld.ParentType = N'CD.DLS.Model.Mssql.SolutionModelElement'
)
SELECT ViewName 
INTO #viewNames
FROM viewNames

INSERT INTO Annotate.AnnotationViews
(ProjectConfigId, ViewName)
SELECT @projectConfigId, vn.ViewName
FROM #viewNames vn
LEFT JOIN Annotate.AnnotationViews vw ON vw.ProjectConfigId = @projectConfigId AND vw.ViewName = vn.ViewName
WHERE vw.ViewName IS NULL

-- IN FIELDS
IF NOT EXISTS(SELECT TOP 1 1 FROM Annotate.Fields f WHERE f.ProjectConfigId = @projectConfigId AND f.FieldName = N'Name')
BEGIN
	INSERT INTO Annotate.Fields
	(ProjectConfigId, FieldName)
	VALUES (@projectConfigId, N'Name')
END

IF NOT EXISTS(SELECT TOP 1 1 FROM Annotate.Fields f WHERE f.ProjectConfigId = @projectConfigId AND f.FieldName = N'Description')
BEGIN
	INSERT INTO Annotate.Fields
	(ProjectConfigId, FieldName)
	VALUES (@projectConfigId, N'Description')
END

--IN ANNTOTATIONVIEWFIELDS
INSERT INTO Annotate.AnnotationViewFields
(AnnotationViewId, FieldId, FieldOrder)
SELECT vw.AnnotationViewId, f.FieldId, IIF(f.FieldName = N'Name', 1, 2)
FROM Annotate.AnnotationViews vw
INNER JOIN Annotate.Fields f ON f.ProjectConfigId = vw.ProjectConfigId
LEFT JOIN Annotate.AnnotationViewFields vf ON vf.FieldId = f.FieldId AND vf.AnnotationViewId = vw.AnnotationViewId
WHERE vw.ProjectConfigId = @projectConfigId AND f.FieldName IN (N'Name', N'Description') AND vf.AnnotationViewFieldId IS NULL

DROP TABLE #viewNames
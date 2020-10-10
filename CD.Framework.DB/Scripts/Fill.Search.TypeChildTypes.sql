TRUNCATE TABLE [Search].[TypeChildTypes]
GO

INSERT INTO [Search].[TypeChildTypes]
([ParentType], [ChildType])
SELECT map.ParentType, map.ChildType
FROM   (VALUES
	(NULL, N'CD.DLS.Model.Mssql.Ssrs.ReportElement'),
	(NULL, N'CD.DLS.Model.Mssql.Ssrs.TextBoxElement'),
	(NULL, N'CD.DLS.Model.Mssql.Ssis.PackageElement'),
	(NULL, N'CD.DLS.Model.Mssql.Ssis.ProjectElement'),
	(NULL, N'CD.DLS.Model.Mssql.Db.ViewElement'),
	(NULL, N'CD.DLS.Model.Mssql.Db.SchemaTableElement'),
	(NULL, N'CD.DLS.Model.Mssql.Db.ProcedureElement'),
	(NULL, N'CD.DLS.Model.Mssql.Db.ColumnElement'),
	(NULL, N'CD.DLS.Model.Mssql.Db.DatabaseElement'),
	(NULL, N'CD.DLS.Model.Mssql.Ssas.PhysicalMeasureElement'),
	--(NULL, N'CD.DLS.Model.Mssql.Ssas.CalculatedMeasureElement'),
	(NULL, N'CD.DLS.Model.Mssql.Ssas.CubeCalculatedMeasureElement'),
	(NULL, N'CD.DLS.Model.Mssql.Ssas.ReportCalculatedMeasureElement'),
	(NULL, N'CD.DLS.Model.Mssql.Ssas.DimensionAttributeElement'),
	(NULL, N'CD.DLS.Model.Mssql.Ssas.DimensionElement'),
	(NULL, N'CD.DLS.Model.Mssql.Ssas.CubeElement'),
	(NULL, N'CD.DLS.Model.Business.Excel.PivotTableTemplateElement'),

	(N'CD.DLS.Model.Mssql.Pbi.TenantElement', N'CD.DLS.Model.Mssql.Pbi.ReportElement'),
	(N'CD.DLS.Model.Mssql.Pbi.TenantElement', N'CD.DLS.Model.Mssql.Pbi.ReportSectionElement'),
	(N'CD.DLS.Model.Mssql.Pbi.TenantElement', N'CD.DLS.Model.Mssql.Pbi.VisualElement'),
	
	(N'CD.DLS.Model.Mssql.Db.DatabaseElement', N'CD.DLS.Model.Mssql.Db.ColumnElement'),
	(N'CD.DLS.Model.Mssql.Db.DatabaseElement', N'CD.DLS.Model.Mssql.Db.SchemaTableElement'),
	(N'CD.DLS.Model.Mssql.Db.DatabaseElement', N'CD.DLS.Model.Mssql.Db.ProcedureElement'),
	(N'CD.DLS.Model.Mssql.Db.DatabaseElement', N'CD.DLS.Model.Mssql.Db.ViewElement'),
	(N'CD.DLS.Model.Mssql.Db.DatabaseElement', N'CD.DLS.Model.Mssql.Db.DatabaseElement'),
	
	(N'CD.DLS.Model.Mssql.Ssas.SsasMultidimensionalDatabaseElement', N'CD.DLS.Model.Mssql.Ssas.PhysicalMeasureElement'),
	--(N'CD.DLS.Model.Mssql.Ssas.SsasMultidimensionalDatabaseElement', N'CD.DLS.Model.Mssql.Ssas.CalculatedMeasureElement'),
	(N'CD.DLS.Model.Mssql.Ssas.SsasMultidimensionalDatabaseElement', N'CD.DLS.Model.Mssql.Ssas.CubeCalculatedMeasureElement'),
	(N'CD.DLS.Model.Mssql.Ssas.SsasMultidimensionalDatabaseElement', N'CD.DLS.Model.Mssql.Ssas.ReportCalculatedMeasureElement'),
	(N'CD.DLS.Model.Mssql.Ssas.SsasMultidimensionalDatabaseElement', N'CD.DLS.Model.Mssql.Ssas.DimensionAttributeElement'),
	(N'CD.DLS.Model.Mssql.Ssas.SsasMultidimensionalDatabaseElement', N'CD.DLS.Model.Mssql.Ssas.DimensionElement'),
	(N'CD.DLS.Model.Mssql.Ssas.SsasMultidimensionalDatabaseElement', N'CD.DLS.Model.Mssql.Ssas.CubeElement'),

	(N'CD.DLS.Model.Mssql.Tabular.SsasTabularDatabaseElement', N'CD.DLS.Model.Mssql.Tabular.SsasTabularMeasureElement'),
	(N'CD.DLS.Model.Mssql.Tabular.SsasTabularDatabaseElement', N'CD.DLS.Model.Mssql.Tabular.SsasTabularTableElement'),
	(N'CD.DLS.Model.Mssql.Tabular.SsasTabularDatabaseElement', N'CD.DLS.Model.Mssql.Tabular.SsasTabularTableColumnElement'),
	
	(N'CD.DLS.Model.Mssql.Ssis.ProjectElement', N'CD.DLS.Model.Mssql.Ssis.PackageElement'),
	(N'CD.DLS.Model.Mssql.Ssis.ProjectElement', N'CD.DLS.Model.Mssql.Ssis.ProjectElement'),
	
	(N'CD.DLS.Model.Mssql.Ssrs.FolderElement', N'CD.DLS.Model.Mssql.Ssrs.ReportElement'),
	(N'CD.DLS.Model.Mssql.Ssrs.FolderElement', N'CD.DLS.Model.Mssql.Ssrs.TextBoxElement'),

	(N'CD.DLS.Model.Business.Organization.BusinessRootElement', N'CD.DLS.Model.Business.Excel.PivotTableTemplateElement')
	) map(ParentType, ChildType)
LEFT JOIN [Search].[TypeChildTypes] ex ON ISNULL(ex.ParentType, N'') = ISNULL(map.ParentType, N'') AND ex.ChildType = map.ChildType
WHERE ex.ChildType IS NULL
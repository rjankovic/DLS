TRUNCATE TABLE [BIDoc].[HighLevelTypeDescendants]

INSERT INTO [BIDoc].[HighLevelTypeDescendants]
([ParentType], [DescendantType], [NodeType])
VALUES
(N'CD.DLS.Model.Mssql.SolutionModelElement', N'CD.DLS.Model.Mssql.Ssrs.ReportElement',N'ReportElement'),
(N'CD.DLS.Model.Mssql.SolutionModelElement', N'CD.DLS.Model.Mssql.Ssrs.TextBoxElement',N'TextBoxElement'),
(N'CD.DLS.Model.Mssql.SolutionModelElement', N'CD.DLS.Model.Mssql.Ssis.PackageElement',N'PackageElement'),
(N'CD.DLS.Model.Mssql.SolutionModelElement', N'CD.DLS.Model.Mssql.Ssas.CubeElement',N'CubeElement'),
(N'CD.DLS.Model.Mssql.SolutionModelElement', N'CD.DLS.Model.Mssql.Ssas.DimensionElement',N'DimensionElement'),
(N'CD.DLS.Model.Mssql.SolutionModelElement', N'CD.DLS.Model.Mssql.Ssas.DimensionAttributeElement',N'DimensionAttributeElement'),
(N'CD.DLS.Model.Mssql.SolutionModelElement', N'CD.DLS.Model.Mssql.Ssas.MdxScriptElement',N'MdxScriptElement'),
(N'CD.DLS.Model.Mssql.SolutionModelElement', N'CD.DLS.Model.Mssql.Db.DatabaseElement',N'DatabaseElement'),
(N'CD.DLS.Model.Mssql.SolutionModelElement', N'CD.DLS.Model.Mssql.Db.ViewElement',N'ViewElement'),
(N'CD.DLS.Model.Mssql.SolutionModelElement', N'CD.DLS.Model.Mssql.Db.SchemaTableElement',N'SchemaTableElement'),
(N'CD.DLS.Model.Mssql.SolutionModelElement', N'CD.DLS.Model.Mssql.Db.ColumnElement',N'ColumnElement'),
(N'CD.DLS.Model.Mssql.SolutionModelElement', N'CD.DLS.Model.Mssql.Ssas.MeasureGroupElement',N'MeasureGroupElement'),
(N'CD.DLS.Model.Mssql.SolutionModelElement', N'CD.DLS.Model.Mssql.Ssas.PhysicalMeasureElement',N'PhysicalMeasureElement'),
(N'CD.DLS.Model.Mssql.SolutionModelElement', N'CD.DLS.Model.Mssql.Ssas.CubeCalculatedMeasureElement',N'CubeCalculatedMeasureElement'),
(N'CD.DLS.Model.Mssql.SolutionModelElement', N'CD.DLS.Model.Mssql.Ssas.ReportCalculatedMeasureElement',N'ReportCalculatedMeasureElement'),
(N'CD.DLS.Model.Mssql.SolutionModelElement', N'CD.DLS.Model.Mssql.Pbi.TenantElement',N'TenantElement'),
(N'CD.DLS.Model.Mssql.SolutionModelElement', N'CD.DLS.Model.Mssql.Pbi.ReportElement', N'ReportElement'),
(N'CD.DLS.Model.Mssql.SolutionModelElement', N'CD.DLS.Model.Mssql.Pbi.ReportSectionElement', N'ReportSectionElement'),
(N'CD.DLS.Model.Mssql.SolutionModelElement', N'CD.DLS.Model.Mssql.Pbi.VisualElement', N'VisualElement'),
(N'CD.DLS.Model.Mssql.SolutionModelElement', N'CD.DLS.Model.Mssql.Pbi.ProjectionElement', N'ProjectionElement')


INSERT INTO [BIDoc].[HighLevelTypeDescendants]
([ParentType], [DescendantType], [NodeType])
VALUES
(N'CD.DLS.Model.Mssql.Db.ServerElement', N'CD.DLS.Model.Mssql.Db.DatabaseElement',N'DatabaseElement'),
(N'CD.DLS.Model.Mssql.Db.ServerElement', N'CD.DLS.Model.Mssql.Db.ViewElement',N'ViewElement'),
(N'CD.DLS.Model.Mssql.Db.ServerElement', N'CD.DLS.Model.Mssql.Db.SchemaTableElement',N'SchemaTableElement'),
(N'CD.DLS.Model.Mssql.Db.ServerElement', N'CD.DLS.Model.Mssql.Db.ColumnElement',N'ColumnElement')



INSERT INTO [BIDoc].[HighLevelTypeDescendants]
([ParentType], [DescendantType], [NodeType])
VALUES
(N'CD.DLS.Model.Mssql.Ssis.ServerElement', N'CD.DLS.Model.Mssql.Ssis.PackageElement',N'PackageElement')

INSERT INTO [BIDoc].[HighLevelTypeDescendants]
([ParentType], [DescendantType], [NodeType])
VALUES
(N'CD.DLS.Model.Mssql.Ssas.ServerElement', N'CD.DLS.Model.Mssql.Ssas.CubeElement',N'CubeElement'),
(N'CD.DLS.Model.Mssql.Ssas.ServerElement', N'CD.DLS.Model.Mssql.Ssas.DimensionElement',N'DimensionElement'),
(N'CD.DLS.Model.Mssql.Ssas.ServerElement', N'CD.DLS.Model.Mssql.Ssas.DimensionAttributeElement',N'DimensionAttributeElement'),
(N'CD.DLS.Model.Mssql.Ssas.ServerElement', N'CD.DLS.Model.Mssql.Ssas.MdxScriptElement',N'MdxScriptElement'),
(N'CD.DLS.Model.Mssql.Ssas.ServerElement', N'CD.DLS.Model.Mssql.Ssas.MeasureGroupElement',N'MeasureGroupElement'),
(N'CD.DLS.Model.Mssql.Ssas.ServerElement', N'CD.DLS.Model.Mssql.Ssas.PhysicalMeasureElement',N'PhysicalMeasureElement'),
(N'CD.DLS.Model.Mssql.Ssas.ServerElement', N'CD.DLS.Model.Mssql.Ssas.CubeCalculatedMeasureElement',N'CubeCalculatedMeasureElement'),
(N'CD.DLS.Model.Mssql.Ssas.ServerElement', N'CD.DLS.Model.Mssql.Ssas.ReportCalculatedMeasureElement',N'ReportCalculatedMeasureElement'),
(N'CD.DLS.Model.Mssql.Ssas.ServerElement', N'CD.DLS.Model.Mssql.Tabular.SsasTabularTableColumnElement', N'SsasTabularTableColumnElement'),
(N'CD.DLS.Model.Mssql.Ssas.ServerElement', N'CD.DLS.Model.Mssql.Tabular.SsasTabularTableElement', N'SsasTabularTableElement'),
(N'CD.DLS.Model.Mssql.Ssas.ServerElement', N'CD.DLS.Model.Mssql.Tabular.SsasTabularMeasureElement', N'SsasTabularMeasureElement')

INSERT INTO [BIDoc].[HighLevelTypeDescendants]
([ParentType], [DescendantType], [NodeType])
VALUES
(N'CD.DLS.Model.Mssql.Ssrs.ServerElement', N'CD.DLS.Model.Mssql.Ssrs.ReportElement',N'ReportElement'),
(N'CD.DLS.Model.Mssql.Ssrs.ServerElement', N'CD.DLS.Model.Mssql.Ssrs.TextBoxElement',N'TextBoxElement')

INSERT INTO [BIDoc].[HighLevelTypeDescendants]
([ParentType], [DescendantType], [NodeType])
VALUES
(N'CD.DLS.Model.Mssql.Db.DatabaseElement', N'CD.DLS.Model.Mssql.Db.DatabaseElement',N'DatabaseElement'),
(N'CD.DLS.Model.Mssql.Db.DatabaseElement', N'CD.DLS.Model.Mssql.Db.ViewElement',N'ViewElement'),
(N'CD.DLS.Model.Mssql.Db.DatabaseElement', N'CD.DLS.Model.Mssql.Db.SchemaTableElement',N'SchemaTableElement'),
(N'CD.DLS.Model.Mssql.Db.DatabaseElement', N'CD.DLS.Model.Mssql.Db.ColumnElement',N'ColumnElement')

INSERT INTO [BIDoc].[HighLevelTypeDescendants]
([ParentType], [DescendantType], [NodeType])
VALUES
(N'CD.DLS.Model.Mssql.Ssas.SsasMultidimensionalDatabaseElement', N'CD.DLS.Model.Mssql.Ssas.CubeElement',N'CubeElement'),
(N'CD.DLS.Model.Mssql.Ssas.SsasMultidimensionalDatabaseElement', N'CD.DLS.Model.Mssql.Ssas.DimensionElement',N'DimensionElement'),
(N'CD.DLS.Model.Mssql.Ssas.SsasMultidimensionalDatabaseElement', N'CD.DLS.Model.Mssql.Ssas.DimensionAttributeElement',N'DimensionAttributeElement'),
(N'CD.DLS.Model.Mssql.Ssas.SsasMultidimensionalDatabaseElement', N'CD.DLS.Model.Mssql.Ssas.MdxScriptElement',N'MdxScriptElement'),
(N'CD.DLS.Model.Mssql.Ssas.SsasMultidimensionalDatabaseElement', N'CD.DLS.Model.Mssql.Ssas.MeasureGroupElement',N'MeasureGroupElement'),
(N'CD.DLS.Model.Mssql.Ssas.SsasMultidimensionalDatabaseElement', N'CD.DLS.Model.Mssql.Ssas.PhysicalMeasureElement',N'PhysicalMeasureElement'),
(N'CD.DLS.Model.Mssql.Ssas.SsasMultidimensionalDatabaseElement', N'CD.DLS.Model.Mssql.Ssas.CubeCalculatedMeasureElement',N'CubeCalculatedMeasureElement'),
(N'CD.DLS.Model.Mssql.Ssas.SsasMultidimensionalDatabaseElement', N'CD.DLS.Model.Mssql.Ssas.ReportCalculatedMeasureElement',N'ReportCalculatedMeasureElement')

INSERT INTO [BIDoc].[HighLevelTypeDescendants]
([ParentType], [DescendantType], [NodeType])
VALUES
(N'CD.DLS.Model.Mssql.Ssrs.FolderElement', N'CD.DLS.Model.Mssql.Ssrs.ReportElement',N'ReportElement'),
(N'CD.DLS.Model.Mssql.Ssrs.FolderElement', N'CD.DLS.Model.Mssql.Ssrs.TextBoxElement',N'TextBoxElement')

INSERT INTO [BIDoc].[HighLevelTypeDescendants]
([ParentType], [DescendantType], [NodeType])
VALUES
(N'CD.DLS.Model.Mssql.Ssis.ProjectElement', N'CD.DLS.Model.Mssql.Ssis.PackageElement',N'PackageElement')

INSERT INTO [BIDoc].[HighLevelTypeDescendants]
([ParentType], [DescendantType], [NodeType])
VALUES
(N'CD.DLS.Model.Mssql.Ssis.FolderElement', N'CD.DLS.Model.Mssql.Ssis.PackageElement',N'PackageElement')

INSERT INTO [BIDoc].[HighLevelTypeDescendants]
([ParentType], [DescendantType], [NodeType])
VALUES
(N'CD.DLS.Model.Mssql.Ssis.CatalogElement', N'CD.DLS.Model.Mssql.Ssis.PackageElement',N'PackageElement')

INSERT INTO [BIDoc].[HighLevelTypeDescendants]
([ParentType], [DescendantType], [NodeType])
VALUES
(N'CD.DLS.Model.Mssql.Db.SchemaElement', N'CD.DLS.Model.Mssql.Db.ViewElement',N'ViewElement'),
(N'CD.DLS.Model.Mssql.Db.SchemaElement', N'CD.DLS.Model.Mssql.Db.SchemaTableElement',N'SchemaTableElement'),
(N'CD.DLS.Model.Mssql.Db.SchemaElement', N'CD.DLS.Model.Mssql.Db.ColumnElement',N'ColumnElement')

INSERT INTO [BIDoc].[HighLevelTypeDescendants]
([ParentType], [DescendantType], [NodeType])
VALUES
(N'CD.DLS.Model.Mssql.Ssrs.ReportElement', N'CD.DLS.Model.Mssql.Ssrs.ReportElement',N'ReportElement'),
(N'CD.DLS.Model.Mssql.Ssrs.ReportElement', N'CD.DLS.Model.Mssql.Ssrs.TextBoxElement',N'TextBoxElement')

INSERT INTO [BIDoc].[HighLevelTypeDescendants]
([ParentType], [DescendantType], [NodeType])
VALUES
(N'CD.DLS.Model.Mssql.Ssis.PackageElement', N'CD.DLS.Model.Mssql.Ssis.PackageElement',N'PackageElement')

INSERT INTO [BIDoc].[HighLevelTypeDescendants]
([ParentType], [DescendantType], [NodeType])
VALUES
(N'CD.DLS.Model.Mssql.Ssas.CubeElement', N'CD.DLS.Model.Mssql.Ssas.CubeElement',N'CubeElement'),
(N'CD.DLS.Model.Mssql.Ssas.CubeElement', N'CD.DLS.Model.Mssql.Ssas.MdxScriptElement',N'MdxScriptElement'),
(N'CD.DLS.Model.Mssql.Ssas.CubeElement', N'CD.DLS.Model.Mssql.Ssas.MeasureGroupElement',N'MeasureGroupElement'),
(N'CD.DLS.Model.Mssql.Ssas.CubeElement', N'CD.DLS.Model.Mssql.Ssas.PhysicalMeasureElement',N'PhysicalMeasureElement'),
(N'CD.DLS.Model.Mssql.Ssas.CubeElement', N'CD.DLS.Model.Mssql.Ssas.CubeCalculatedMeasureElement',N'CubeCalculatedMeasureElement'),
(N'CD.DLS.Model.Mssql.Ssas.CubeElement', N'CD.DLS.Model.Mssql.Ssas.ReportCalculatedMeasureElement',N'ReportCalculatedMeasureElement')

INSERT INTO [BIDoc].[HighLevelTypeDescendants]
([ParentType], [DescendantType], [NodeType])
VALUES
(N'CD.DLS.Model.Mssql.Ssas.DimensionElement', N'CD.DLS.Model.Mssql.Ssas.DimensionElement',N'DimensionElement'),
(N'CD.DLS.Model.Mssql.Ssas.DimensionElement', N'CD.DLS.Model.Mssql.Ssas.DimensionAttributeElement',N'DimensionAttributeElement')

INSERT INTO [BIDoc].[HighLevelTypeDescendants]
([ParentType], [DescendantType], [NodeType])
VALUES
(N'CD.DLS.Model.Mssql.Ssas.MdxScriptElement', N'CD.DLS.Model.Mssql.Ssas.MdxScriptElement',N'MdxScriptElement'),
(N'CD.DLS.Model.Mssql.Ssas.MdxScriptElement', N'CD.DLS.Model.Mssql.Ssas.CubeCalculatedMeasureElement',N'CubeCalculatedMeasureElement'),
(N'CD.DLS.Model.Mssql.Ssas.MdxScriptElement', N'CD.DLS.Model.Mssql.Ssas.ReportCalculatedMeasureElement',N'ReportCalculatedMeasureElement')

INSERT INTO [BIDoc].[HighLevelTypeDescendants]
([ParentType], [DescendantType], [NodeType])
VALUES
(N'CD.DLS.Model.Mssql.Db.ViewElement', N'CD.DLS.Model.Mssql.Db.ViewElement',N'ViewElement'),
(N'CD.DLS.Model.Mssql.Db.ViewElement', N'CD.DLS.Model.Mssql.Db.ColumnElement',N'ColumnElement')

INSERT INTO [BIDoc].[HighLevelTypeDescendants]
([ParentType], [DescendantType], [NodeType])
VALUES
(N'CD.DLS.Model.Mssql.Db.SchemaTableElement', N'CD.DLS.Model.Mssql.Db.SchemaTableElement',N'SchemaTableElement'),
(N'CD.DLS.Model.Mssql.Db.SchemaTableElement', N'CD.DLS.Model.Mssql.Db.ColumnElement',N'ColumnElement')

INSERT INTO [BIDoc].[HighLevelTypeDescendants]
([ParentType], [DescendantType], [NodeType])
VALUES
(N'CD.DLS.Model.Mssql.Ssas.MeasureGroupElement', N'CD.DLS.Model.Mssql.Ssas.PhysicalMeasureElement',N'PhysicalMeasureElement'),
(N'CD.DLS.Model.Mssql.Ssas.MeasureGroupElement', N'CD.DLS.Model.Mssql.Ssas.CubeCalculatedMeasureElement',N'CubeCalculatedMeasureElement'),
(N'CD.DLS.Model.Mssql.Ssas.MeasureGroupElement', N'CD.DLS.Model.Mssql.Ssas.ReportCalculatedMeasureElement',N'ReportCalculatedMeasureElement')

INSERT INTO [BIDoc].[HighLevelTypeDescendants]
([ParentType], [DescendantType], [NodeType])
VALUES
(N'CD.DLS.Model.Mssql.Db.ProcedureElement', N'CD.DLS.Model.Mssql.Db.ProcedureElement',N'ProcedureElement')

INSERT INTO [BIDoc].[HighLevelTypeDescendants]
([ParentType], [DescendantType], [NodeType])
VALUES
(N'CD.DLS.Model.Mssql.Tabular.SsasTabularDatabaseElement', N'CD.DLS.Model.Mssql.Tabular.SsasTabularTableColumnElement', N'SsasTabularTableColumnElement'),
(N'CD.DLS.Model.Mssql.Tabular.SsasTabularDatabaseElement', N'CD.DLS.Model.Mssql.Tabular.SsasTabularTableElement', N'SsasTabularTableElement'),
(N'CD.DLS.Model.Mssql.Tabular.SsasTabularDatabaseElement', N'CD.DLS.Model.Mssql.Tabular.SsasTabularMeasureElement', N'SsasTabularMeasureElement')


INSERT INTO [BIDoc].[HighLevelTypeDescendants]
([ParentType], [DescendantType], [NodeType])
VALUES
(N'CD.DLS.Model.Mssql.Tabular.SsasTabularTableElement', N'CD.DLS.Model.Mssql.Tabular.SsasTabularTableColumnElement', N'SsasTabularTableColumnElement'),
(N'CD.DLS.Model.Mssql.Tabular.SsasTabularTableElement', N'CD.DLS.Model.Mssql.Tabular.SsasTabularTableElement', N'SsasTabularTableElement'),
(N'CD.DLS.Model.Mssql.Tabular.SsasTabularTableElement', N'CD.DLS.Model.Mssql.Tabular.SsasTabularMeasureElement', N'SsasTabularMeasureElement')

INSERT INTO [BIDoc].[HighLevelTypeDescendants]
([ParentType], [DescendantType], [NodeType])
VALUES
(N'CD.DLS.Model.Mssql.Pbi.TenantElement', N'CD.DLS.Model.Mssql.Pbi.ReportElement', N'ReportElement'),
(N'CD.DLS.Model.Mssql.Pbi.TenantElement', N'CD.DLS.Model.Mssql.Pbi.ReportSectionElement', N'ReportSectionElement'),
(N'CD.DLS.Model.Mssql.Pbi.TenantElement', N'CD.DLS.Model.Mssql.Pbi.VisualElement', N'VisualElement'),
(N'CD.DLS.Model.Mssql.Pbi.TenantElement', N'CD.DLS.Model.Mssql.Pbi.ProjectionElement', N'ProjectionElement')


INSERT INTO [BIDoc].[HighLevelTypeDescendants]
([ParentType], [DescendantType], [NodeType])
VALUES
(N'CD.DLS.Model.Mssql.Pbi.ReportElement', N'CD.DLS.Model.Mssql.Pbi.ReportSectionElement', N'ReportSectionElement'),
(N'CD.DLS.Model.Mssql.Pbi.ReportElement', N'CD.DLS.Model.Mssql.Pbi.ReportElement', N'ReportElement'),
(N'CD.DLS.Model.Mssql.Pbi.ReportElement', N'CD.DLS.Model.Mssql.Pbi.VisualElement', N'VisualElement'),
(N'CD.DLS.Model.Mssql.Pbi.ReportElement', N'CD.DLS.Model.Mssql.Pbi.ProjectionElement', N'ProjectionElement')

--(N'CD.DLS.Model.Mssql.Pbi.ReportElement', N'CD.DLS.Model.Mssql.Pbi.ConnectionElement', N'ConnectionElement')

--INSERT INTO [BIDoc].[HighLevelTypeDescendants]
--([ParentType], [DescendantType], [NodeType])
--VALUES
--(N'CD.DLS.Model.Mssql.Pbi.ConnectionElement', N'CD.DLS.Model.Mssql.Pbi.PbiTableElement', N'TableElement')

INSERT INTO [BIDoc].[HighLevelTypeDescendants]
([ParentType], [DescendantType], [NodeType])
VALUES
(N'CD.DLS.Model.Mssql.Pbi.ReportSectionElement', N'CD.DLS.Model.Mssql.Pbi.ReportSectionElement', N'ReportSectionElement'),
(N'CD.DLS.Model.Mssql.Pbi.ReportSectionElement', N'CD.DLS.Model.Mssql.Pbi.VisualElement', N'VisualElement'),
(N'CD.DLS.Model.Mssql.Pbi.ReportSectionElement', N'CD.DLS.Model.Mssql.Pbi.ProjectionElement', N'ProjectionElement')

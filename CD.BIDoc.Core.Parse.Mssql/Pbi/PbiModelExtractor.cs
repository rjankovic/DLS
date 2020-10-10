using CD.DLS.Common.Structures;
using CD.DLS.DAL.Managers;
using CD.DLS.DAL.Objects.Extract;
using CD.DLS.Model.Interfaces;
using CD.DLS.Model.Mssql;
using CD.DLS.Model.Mssql.Pbi;
using CD.DLS.Model.Serialization;
using CD.DLS.Parse.Mssql.Db;
using CD.DLS.Parse.Mssql.Ssas;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CD.DLS.Parse.Mssql.Pbi
{
    public class PbiModelExtractor
    {
        private ProjectConfig _projectConfig;
        private Guid _extractId;
        private StageManager _stageManager;
        private GraphManager _graphManager;
        private AvailableDatabaseModelIndex sqlContext;
        private SsasServerIndex ssasServerIndex;
        private int _currentComponentId;
        private string _tenantRefPath;

        public PbiModelExtractor(ProjectConfig projectConfig, StageManager stageManager, Guid extractId, GraphManager graphManager, int currentComponentId, string tenantRefPath)
        {
            _projectConfig = projectConfig;
            _extractId = extractId;
            _stageManager = stageManager;
            _graphManager = graphManager;
            _currentComponentId = currentComponentId;
            sqlContext = new AvailableDatabaseModelIndex(_projectConfig, _graphManager);
            this.ssasServerIndex = new SsasServerIndex(_projectConfig,_graphManager);
            _tenantRefPath = tenantRefPath;
        }
               
        public void ParseModel()
        {
            SerializationHelper sh = new SerializationHelper(_projectConfig, _graphManager);

            AvailableDatabaseModelIndex sqlContext = new AvailableDatabaseModelIndex(_projectConfig, _graphManager);
            SsasServerIndex ssasServerIndex = new SsasServerIndex(_projectConfig, _graphManager);

            DatabaseIndex dbIndex = null;

            Parse.Mssql.Pbi.UrnBuilder _urnBuiler = new Parse.Mssql.Pbi.UrnBuilder();
            //var tenant2 = sh.LoadElementModel(_tenantRefPath);
            var tenant = (Tenant)_stageManager.GetExtractItems(_extractId, _currentComponentId, ExtractTypeEnum.Tenant)[0];
            //var tenantRefPath = _urnBuiler.GetTenantUrn(tenant);

            TenantElement tenantElement =  (TenantElement)sh.LoadElementModel(_tenantRefPath);

            foreach (var report in tenant.Reports)
            {
                List<PbiColumnElement> currentReportAllColumns = new List<PbiColumnElement>();

                var reportRefPath = _urnBuiler.GetReportUrn(report, tenantElement.RefPath);
                ReportElement reportElement = new ReportElement(reportRefPath, report.Name, null, tenantElement);
                foreach (var connection in report.Connections)
                {
                    var connectionRefPath = _urnBuiler.GetConnectionUrn(connection, reportElement.RefPath);
                    ConnectionElement connElement = new ConnectionElement(connectionRefPath, connection.Source, null, reportElement);
                    connElement.Type = connection.Type;
                    switch (connElement.Type)
                    {
                        case "Sql.Databases":
                        case "AnalysisServices.Databases":
                        case "analysisServicesDatabaseLive":
                            var splitConnString = connection.Source.Split('\\');
                            connElement.Database = splitConnString[1];
                            connElement.Server = splitConnString[0];
                            break;
                    }

                    reportElement.AddChild(connElement);

                    foreach (var table in connection.Tables)
                    {
                        var split = table.Name.Split('_');
                        var tableRefPath = _urnBuiler.GetTableUrn(table, connElement.RefPath);
                        switch (connElement.Type)
                        {
                            case "analysisServicesDatabaseLive":
                                PbiTableElement tableElementSsasLive = new PbiTableElement(tableRefPath, table.Name, null, connElement);
                                connElement.AddChild(tableElementSsasLive);

                                SsasTabularDatabaseIndex ssasIndex2 = (SsasTabularDatabaseIndex)ssasServerIndex.GetDatabase(connElement.Server, connElement.Database);
                                var columnIdentifier = ssasIndex2.TryResolveIdentifier(table.Name, TabularReferenceType.Table);

                                if (columnIdentifier == null)
                                {
                                    continue;
                                }

                                foreach (MssqlModelElement elem in columnIdentifier.Children)
                                {
                                    if (elem.GetType().Name == "SsasTabularTableColumnElement")
                                    {
                                        PbiColumnElement colElem = new PbiColumnElement(_urnBuiler.GetColumnUrn(elem.Caption, tableElementSsasLive.RefPath), elem.Caption, null, tableElementSsasLive);
                                        colElem.Reference = elem;
                                        tableElementSsasLive.AddChild(colElem);
                                        currentReportAllColumns.Add(colElem);
                                    }
                                }

                                break;
                            case "Sql.Databases":
                                PbiTableElement tableElement = new PbiTableElement(tableRefPath, split[1], null, connElement);
                                connElement.AddChild(tableElement);
                                dbIndex = sqlContext.GetDatabaseIndex(connElement.Server);
                                var tableColumnList = dbIndex.FindTableByObjectName(split[1], new TSql140Parser(false, SqlEngineType.Standalone), new Identifier() { Value = connElement.Database });
                                foreach (var column in table.Columns)
                                {
                                    var columnElement = (MssqlModelElement)tableColumnList.Columns.First(x => x.Identifier.GetText() == column.Name).ModelElement;

                                    var pbiColumn = new PbiColumnElement(_urnBuiler.GetColumnUrn(column, tableElement.RefPath), column.Name, column.Querry, tableElement);
                                    pbiColumn.ColumnName = column.Name;
                                    pbiColumn.Querry = column.Querry;
                                    pbiColumn.Reference = columnElement;

                                    tableElement.AddChild(pbiColumn);

                                    currentReportAllColumns.Add(pbiColumn);
                                }
                                break;

                            case "AnalysisServices.Databases":
                                PbiTableElement tableElementSsas = new PbiTableElement(tableRefPath, table.Name, null, connElement);
                                connElement.AddChild(tableElementSsas);
                                SsasDatabaseIndex ssasIndex = ssasServerIndex.GetDatabase(connElement.Server, connElement.Database);
                                try
                                {
                                    if (ssasIndex.SsasType == SsasTypeEnum.Tabular)
                                    {
                                        var tabular = ssasIndex as SsasTabularDatabaseIndex;
                                        foreach (var column in table.Columns)
                                        {
                                            var splitTabular = column.Name.Split('.');
                                            var tabularColumn = tabular.TryResolveIdentifier(splitTabular[0] + "[" + splitTabular[1] + "]", TabularReferenceType.Column);

                                            var pbiColumn = new PbiColumnElement(_urnBuiler.GetColumnUrn(column, tableElementSsas.RefPath), column.Name, column.Querry, tableElementSsas);
                                            tableElementSsas.AddChild(pbiColumn);
                                            pbiColumn.ColumnName = column.ColumnName;
                                            pbiColumn.Querry = column.Querry;
                                            pbiColumn.Reference = tabularColumn;

                                            currentReportAllColumns.Add(pbiColumn);
                                        }
                                    }
                                    else if (ssasIndex.SsasType == SsasTypeEnum.Multidimensional)
                                    {
                                        var multidim = ssasIndex as SsasMultidimensionalDatabaseIndex;
                                        foreach (var column in table.Columns)
                                        {
                                            var splitMultidim = column.Name.Split('.');
                                            var tabularColumn = multidim.TryResolveIdentifier(split[0]);
                                            tableElementSsas.AddChild(tabularColumn);
                                        }
                                    }
                                }
                                catch
                                {
                                    break;
                                }

                                break;
                        }

                    }

                }
                foreach (var reportSection in report.Sections)
                {
                    var reportSectionRefPath = _urnBuiler.GetReportSectionUrn(reportSection, reportRefPath);
                    ReportSectionElement reportSectionElement = new ReportSectionElement(reportSectionRefPath, reportSection.Displayname, null, reportElement);
                    reportElement.AddChild(reportSectionElement);

                    foreach (var visual in reportSection.Visuals)
                    {
                        var visualRefPath = _urnBuiler.GetVisualUrn(visual, reportSectionRefPath);
                        VisualElement visualElement = new VisualElement(visualRefPath, visual.Name, null, reportSectionElement);
                        visualElement.Type = visual.Type;
                        reportSectionElement.AddChild(visualElement);

                        foreach (var projection in visual.Projections)
                        {
                            var projectionRefPath = _urnBuiler.GetProjectionUrn(projection, visualRefPath);
                            ProjectionElement projectionElement = new ProjectionElement(projectionRefPath, projection.Type, projection.QueryRef, visualElement);
                            visualElement.AddChild(projectionElement);
                            MapColumnsToVisual(projectionElement, currentReportAllColumns);
                        }

                        foreach (var visualFilter in visual.Filters)
                        {
                            var visualFilterRefPath = _urnBuiler.GetFilterUrn(visualFilter, reportSectionRefPath);
                            FilterElement visualFilterElement = new FilterElement(visualFilterRefPath, visualFilter.Name, null, reportSectionElement);
                            visualElement.AddChild(visualFilterElement);
                            MapColumnsToFilter(visualFilter.Expression.Column.Property, visualFilterElement, currentReportAllColumns);
                        }
                    }

                    foreach (var sectionFilter in reportSection.Filters)
                    {
                        var sectionFilterRefPath = _urnBuiler.GetFilterUrn(sectionFilter, reportSectionRefPath);
                        FilterElement sectionFilterElement = new FilterElement(sectionFilterRefPath, sectionFilter.FilterName, null, reportSectionElement);
                        reportSectionElement.AddChild(sectionFilterElement);
                        MapColumnsToFilter(sectionFilter.Expression.Column.Property, sectionFilterElement, currentReportAllColumns);
                    }
                }

                foreach (var reportFilter in report.Filters)
                {
                    var reportFilterRefPath = _urnBuiler.GetFilterUrn(reportFilter, reportRefPath);
                    FilterElement reportFilterElement = new FilterElement(reportFilterRefPath, reportFilter.FilterName, null, reportElement);
                    reportElement.AddChild(reportFilterElement);
                    MapColumnsToFilter(reportFilter.Expression.Column.Property, reportFilterElement, currentReportAllColumns);
                }

                tenantElement.AddChild(reportElement);

            }


            var premappedModel = new Dictionary<MssqlModelElement, int>();

            foreach (var dbItem in sqlContext.GetAllPremappedIds())
            {
                if (!premappedModel.ContainsKey(dbItem.Key))
                {
                    premappedModel.Add(dbItem.Key, dbItem.Value);
                }
            }

            var ssasItems = ssasServerIndex.GetPremappedIds();
            foreach (var ssasItem in ssasItems)
            {
                if (!premappedModel.ContainsKey(ssasItem.Key))
                {
                    premappedModel.Add(ssasItem.Key, ssasItem.Value);
                }
            }

            sh.SaveModelPart(tenantElement, premappedModel, true);

        }

        public void MapColumnsToVisual(ProjectionElement projection, List<PbiColumnElement> columns)
        {
            foreach (var column in columns)
            {
                if (projection.Definition.Contains(column.Caption))
                {
                    projection.Column = column;
                }
            }
        }

        public void MapColumnsToFilter(string property, FilterElement filter, List<PbiColumnElement> columns)
        {
            foreach (var column in columns)
            {
                if (property.Contains(column.Caption))
                {
                    filter.Property = column;
                }
            }
        }

    }
}

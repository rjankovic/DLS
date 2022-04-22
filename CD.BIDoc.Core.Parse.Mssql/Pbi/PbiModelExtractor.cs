using CD.DLS.Common.Structures;
using CD.DLS.DAL.Configuration;
using CD.DLS.DAL.Managers;
using CD.DLS.DAL.Objects.Extract;
using CD.DLS.Model.Mssql;
using CD.DLS.Model.Mssql.Pbi;
using CD.DLS.Model.Mssql.Ssas;
using CD.DLS.Model.Mssql.Tabular;
using CD.DLS.Model.Serialization;
using CD.DLS.Parse.Mssql.Db;
using CD.DLS.Parse.Mssql.Ssas;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using System.Linq;

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
        private SsasTabularDatabaseIndex _connectLiveDbIndex = null;

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

                var reportRefPath = _urnBuiler.GetReportUrn(report, tenantElement);
                ReportElement reportElement = new ReportElement(reportRefPath, report.Name, null, tenantElement);
                foreach (var connection in report.Connections)
                {
                    var connectionRefPath = _urnBuiler.GetConnectionUrn(connection, reportElement.RefPath);
                    ConnectionElement connElement = new ConnectionElement(connectionRefPath, connection.Source == null ? "Connection" : connection.Source, null, reportElement);
                    connElement.Type = connection.Type;
                    if (connection.Source == null && 
                        (
                           connElement.Type == "Sql.Databases"
                           || connElement.Type == "AnalysisServices.Databases"
                           || connElement.Type == "analysisServicesDatabaseLive"
                        ))
                    {
                        DAL.Configuration.ConfigManager.Log.Warning($"Skipping connection {connection.Name} in report {report.Name} - unknown data source");
                        continue;
                    }
                    switch (connElement.Type)
                    {
                        case "Sql.Databases":
                        case "AnalysisServices.Databases":
                        case "analysisServicesDatabaseLive":
                            var splitConnString = connection.Source.Split('\\');
                            connElement.Database = splitConnString.Last();
                            connElement.Server = CD.DLS.Common.Tools.ConnectionStringTools.NormalizeServerName(string.Join("\\", splitConnString.Take(splitConnString.Length-1)));
                            break;
                    }

                    if (connElement.Type == "analysisServicesDatabaseLive")
                    {
                        _connectLiveDbIndex = ssasServerIndex.GetDatabase(connElement.Server, connElement.Database) as SsasTabularDatabaseIndex;
                    }

                    reportElement.AddChild(connElement);

                    foreach (var table in connection.Tables)
                    {
                        if (table.Name == null)
                        {
                            continue;
                        }
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
                    var reportSectionRefPath = _urnBuiler.GetReportSectionUrn(reportSection, reportElement /* reportRefPath*/);
                    ReportSectionElement reportSectionElement = new ReportSectionElement(reportSectionRefPath, reportSection.Displayname, null, reportElement);
                    reportElement.AddChild(reportSectionElement);

                    foreach (var visual in reportSection.Visuals)
                    {
                        
                        var visualRefPath = _urnBuiler.GetVisualUrn(visual, reportSectionElement);
                        VisualElement visualElement = new VisualElement(visualRefPath, visual.Name, null, reportSectionElement);
                        visualElement.Type = visual.Type;
                        reportSectionElement.AddChild(visualElement);

                        if (_connectLiveDbIndex != null)
                        {
                            //_connectLiveDbIndex.ClearTempMeasures();
                            
                            //if (visual.ExtensionMeasures.Any())
                            //{
                            //    var table = visual.ExtensionMeasures.First().TableName;
                            //    _connectLiveDbIndex.SetContextTable(table);
                            //}

                            var extensionMeasures = new List<Tuple<VisualExtensionMeasure, SsasTabularMeasureElement>>();
                            DaxScriptModelExtractor daxExtractor = new DaxScriptModelExtractor();

                            foreach (var extensionMeasure in visual.ExtensionMeasures)
                            {
                                _connectLiveDbIndex.AddLocalMeasure(extensionMeasure.TableName, extensionMeasure.MeasureName, null);

                                var measureRefPath = _urnBuiler.GetMeasureExtensionUrn(extensionMeasure.TableName, extensionMeasure.MeasureName, visualElement);
                                SsasTabularMeasureElement measureElement = new SsasTabularMeasureElement(measureRefPath, extensionMeasure.MeasureName, extensionMeasure.Expression, visualElement);
                                extensionMeasures.Add(new Tuple<VisualExtensionMeasure, SsasTabularMeasureElement>(extensionMeasure, measureElement));
                                visualElement.AddChild(measureElement);
                                _connectLiveDbIndex.AddTempMeasure(extensionMeasure.TableName, extensionMeasure.MeasureName, measureElement);
                            }

                            foreach (var measureElement in extensionMeasures)
                            {
                                _connectLiveDbIndex.SetContextTable(measureElement.Item1.TableName);
                                Dictionary<string, SsasModelElement> resultColumns;
                                var scriptModel = daxExtractor.ExtractDaxScript(measureElement.Item1.Expression, _connectLiveDbIndex, measureElement.Item2, out resultColumns);
                                if (scriptModel == null)
                                {
                                    ConfigManager.Log.Warning("Failed to parse " + measureElement.Item2.RefPath.Path);
                                    continue;
                                }
                                measureElement.Item2.AddChild(scriptModel);
                                measureElement.Item2.Reference = resultColumns.First().Value;
                            }

                            _connectLiveDbIndex.ClearLocalIndexes();
                        }

                        foreach (var projection in visual.Projections)
                        {
                            var projectionRefPath = _urnBuiler.GetProjectionUrn(projection, visualElement);
                            ProjectionElement projectionElement = new ProjectionElement(projectionRefPath, projection.Type + " - " + projection.Name, projection.QueryRef, visualElement);
                            visualElement.AddChild(projectionElement);
                            MapColumnsToVisual(projectionElement, currentReportAllColumns);
                        }

                        foreach (var visualFilter in visual.Filters)
                        {
                            var visualFilterRefPath = _urnBuiler.GetFilterUrn(visualFilter, visualElement);
                            FilterElement visualFilterElement = new FilterElement(visualFilterRefPath, visualFilter.FilterName == null ? "Filter" : visualFilter.FilterName, null, visualElement);
                            visualElement.AddChild(visualFilterElement);
                            MapColumnsToFilter(visualFilter.Reference, visualFilterElement, currentReportAllColumns);
                        }

                        if (_connectLiveDbIndex != null)
                        {
                            _connectLiveDbIndex.ClearTempMeasures();
                        }
                    }

                    foreach (var sectionFilter in reportSection.Filters)
                    {
                        var sectionFilterRefPath = _urnBuiler.GetFilterUrn(sectionFilter, reportSectionElement);
                        FilterElement sectionFilterElement = new FilterElement(sectionFilterRefPath, sectionFilter.FilterName == null ? "Filter" : sectionFilter.FilterName, null, reportSectionElement);
                        reportSectionElement.AddChild(sectionFilterElement);
                        MapColumnsToFilter(sectionFilter.Reference, sectionFilterElement, currentReportAllColumns);
                    }
                }

                foreach (var reportFilter in report.Filters)
                {
                    var reportFilterRefPath = _urnBuiler.GetFilterUrn(reportFilter, reportElement);
                    FilterElement reportFilterElement = new FilterElement(reportFilterRefPath, reportFilter.FilterName == null ? "Filter" : reportFilter.FilterName, null, reportElement);
                    reportElement.AddChild(reportFilterElement);
                    MapColumnsToFilter(reportFilter.Reference, reportFilterElement, currentReportAllColumns);
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
            if (projection == null)
            {
                return;
            }
            if (projection.Definition == null)
            {
                return;
            }
            if (_connectLiveDbIndex != null)
            {
                var resolution = _connectLiveDbIndex.TryResolveIdentifier(projection.Definition, TabularReferenceType.Column);
                if (resolution == null && projection.Definition.EndsWith(")") && projection.Definition.Contains("("))
                {
                    var d = projection.Definition;
                    var unwrappedDef = d.Substring(d.IndexOf('(') + 1).TrimEnd(')');
                    //ConfigManager.Log.Important(d);
                    //ConfigManager.Log.Important(unwrappedDef);
                    resolution = _connectLiveDbIndex.TryResolveIdentifier(unwrappedDef, TabularReferenceType.Column);
                }
                projection.Column = resolution;
            }
            else
            {
                foreach (var column in columns)
                {
                    if (projection.Definition.Contains(column.Caption))
                    {
                        projection.Column = column;
                        break;
                    }
                }
            }
        }

        public void MapColumnsToFilter(string property, FilterElement filter, List<PbiColumnElement> columns)
        {
            if (property == null)
            {
                return;
            }
            if (_connectLiveDbIndex != null)
            {
                var resolution = _connectLiveDbIndex.TryResolveIdentifier(property, TabularReferenceType.Column);
                filter.Property = resolution;
            }
            else
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
}

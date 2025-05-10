using System;
using System.Collections.Generic;
using CD.DLS.Model.Mssql;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using CD.DLS.Parse.Mssql.Db;
using CD.DLS.Common.Structures;
using CD.DLS.DAL.Managers;
using CD.DLS.DAL.Configuration;
using CD.DLS.DAL.Objects.Extract;
using CD.DLS.Model.Mssql.Tabular;
using System.Diagnostics;
using System.Linq;
using CD.DLS.Parse.Mssql.Ssas;
using CD.DLS.Model.Mssql.Ssas;
using CD.DLS.Core.Parse.Mssql.PowerQuery;
using TM = CD.DLS.Model.Mssql.Tabular;
using System.Text.RegularExpressions;

namespace CD.BIDoc.Core.Parse.Mssql.Tabular

{
    public class TabularParser
    {
        private DLS.Parse.Mssql.Ssas.UrnBuilder _urnBuilder = new DLS.Parse.Mssql.Ssas.UrnBuilder();
        private ProjectConfig _projectConfig;
        private Guid _extractId;
        private StageManager _stageManager;
        private AvailableDatabaseModelIndex _availableDatabaseModelIndex;
        private ISqlScriptModelParser _sqlScriptExtractor;

        public TabularParser(AvailableDatabaseModelIndex availableDatabaseModelIndex, ProjectConfig projectConfig, Guid extractId, StageManager stageManager)
        {
            _availableDatabaseModelIndex = availableDatabaseModelIndex;
            _projectConfig = projectConfig;
            _extractId = extractId;
            _stageManager = stageManager;
            _sqlScriptExtractor = new ScriptModelParser();


        }

        public DLS.Model.Mssql.Tabular.SsasTabularDatabaseElement Parse(int componentID, String SSASServerName, DLS.Model.Mssql.Ssas.ServerElement serverElement)
        {
            var db = ExtractTabularDatabase(componentID, serverElement);
            return db;
        }

        public TableSourceColumnList GetRelationalDbTable(String tableName, string schemaName, String dbName, string serverName)
        {
            Microsoft.SqlServer.TransactSql.ScriptDom.Identifier dbIdentifier = new Microsoft.SqlServer.TransactSql.ScriptDom.Identifier() { Value = dbName };
            var dbIdx = _availableDatabaseModelIndex.GetDatabaseIndex(serverName);


            var identifier = string.Format("[{0}].[{1}]", schemaName, tableName);
            //ConfigManager.Log.Important("Tabular Parser, parameters of getLinkedObject,identifier: " + identifier + ", dbIdentifier " + dbIdentifier.Value);
            return dbIdx.FindTableByObjectName(identifier, _sqlScriptExtractor.Parser, dbIdentifier);



        }

        private string NormalizeMQuery(string input)
        {
            //string result = Regex.Replace(input, @"\[(.*?)\]", match =>
            //{
            //    string listContent = match.Groups[1].Value;

            //    string processed = Regex.Replace(listContent, @"(?<=^|,)\s*(?!\"")([A-Za-z0-9_]+(?:\s+[A-Za-z0-9_]+)+)\s*=", m =>
            //    {
            //        string key = m.Groups[1].Value;
            //        string replacement = key.Replace(" ", "_") + " =";
            //        // Keep prefix (empty or comma), don't add leading space
            //        string prefix = m.Value.StartsWith(",") ? ", " : "";
            //        return prefix + replacement;
            //    });

            //    return "[" + processed + "]";
            //}, RegexOptions.Singleline);

            string result = Regex.Replace(input, @"\[(.*?)\]", match =>
            {
                string listContent = match.Groups[1].Value;

                // Match unquoted items (whether or not they are assignments)
                string processed = Regex.Replace(listContent, @"(?<=^|,)\s*(?!\"")([A-Za-z0-9_]+(?:\s+[A-Za-z0-9_]+)+)(?=\s*(=|,|$))", m =>
                {
                    string item = m.Groups[1].Value;
                    return item.Replace(" ", "_");
                });

                return "[" + processed + "]";
            }, RegexOptions.Singleline);
            return result;
        }

        public void ExtractTabularModel(TabularModel model, SsasTabularDatabaseElement tDatabaseElement)
        {
            var dbRefPath = tDatabaseElement.RefPath;

            foreach (var annotation in model.Annotations)
            {
                var annotationRefPath = _urnBuilder.GetUrnAnnotation(annotation.Name, dbRefPath);

                SsasTabularAnnotationElement pAnnotation = new SsasTabularAnnotationElement(annotationRefPath, annotation.Name, annotation.Value, tDatabaseElement);
                tDatabaseElement.AddChild(pAnnotation);
            }

            //ConfigManager.Log.Important("Tabular parser, Tabular Probe 6");

            Dictionary<string, SsasTabularDataSourceElement> availableDatasources = new Dictionary<string, SsasTabularDataSourceElement>();

            foreach (var dataSource in model.TabularDataSources)
            {
                //ConfigManager.Log.Important("Tabular parser : Parsing data source  " + dataSource.DSname);

                var dataSourceRefPath = _urnBuilder.GetUrnDataSource(dataSource.DSname, dbRefPath);
                SsasTabularDataSourceElement dataSourceElement = new SsasTabularDataSourceElement(dataSourceRefPath, dataSource.DSname, dataSource.Description, tDatabaseElement);

                foreach (var annotation in dataSource.Annotations)
                {
                    var annotationRefPath = _urnBuilder.GetUrnAnnotation(annotation.Name, dataSourceRefPath);
                    SsasTabularAnnotationElement pAnnotation = new SsasTabularAnnotationElement(annotationRefPath, annotation.Name, annotation.Value, dataSourceElement);
                    dataSourceElement.AddChild(pAnnotation);
                }
                dataSourceElement.ServerName = dataSource.ServerName;
                dataSourceElement.DbName = dataSource.DatabaseName;
                tDatabaseElement.AddChild(dataSourceElement);
                availableDatasources.Add(dataSourceElement.Caption, dataSourceElement);
            }

            foreach (var table in model.TabularTables)
            {
                //ConfigManager.Log.Important("Tabular parser : Parsing table  " + table.Name);

                var tableRefPath = _urnBuilder.GetUrnTable(table.Name, dbRefPath);
                SsasTabularTableElement tableElement = new SsasTabularTableElement(tableRefPath, table.Name, "Tabular table", tDatabaseElement);

                //Debug.WriteLine("Created table " + pTable.Caption);
                foreach (var annotation in table.Annotations)
                {
                    var annotationRefPath = _urnBuilder.GetUrnAnnotation(annotation.Name, tableRefPath);
                    SsasTabularAnnotationElement pAnnotation = new SsasTabularAnnotationElement(annotationRefPath, annotation.Name, annotation.Value, tableElement);
                    tableElement.AddChild(pAnnotation);
                }


                Dictionary<string, SsasTabularTableColumnElement> tableSourceColumnsToColumns = new Dictionary<string, SsasTabularTableColumnElement>();

                //DaxScriptModelExtractor columnCalculationsExtractor = new DaxScriptModelExtractor();
                //SsasTabularDatabaseIndex columnCalculationsIndex = new SsasTabularDatabaseIndex();

                Dictionary<TabularTableColumn, SsasTabularTableColumnElement> columnsToElements = new Dictionary<TabularTableColumn, SsasTabularTableColumnElement>();

                foreach (var column in table.Columns)
                {
                    //ConfigManager.Log.Important("Tabular parser, Tabular Probe 7");
                    var columnRefPath = _urnBuilder.GetUrnColumn(column.Name, tableRefPath);

                    SsasTabularTableColumnElement columnElement = new SsasTabularTableColumnElement(columnRefPath, column.Name, string.IsNullOrEmpty(column.Expression) ? column.DataType : column.Expression, tableElement);
                    columnsToElements.Add(column, columnElement);

                    if (column.ColumnType == TabularTableColumnTypeEnum.Data)
                    {
                        tableSourceColumnsToColumns.Add(column.SourceColumn, columnElement);
                    }

                    //columnCalculationsIndex.AddLocalColumn(column.Name, columnElement);

                    foreach (var annotation in column.Annotations)
                    {
                        var annotationRefPath = _urnBuilder.GetUrnAnnotation(annotation.Name, columnRefPath);
                        SsasTabularAnnotationElement pAnnotation = new SsasTabularAnnotationElement(annotationRefPath, annotation.Name, annotation.Value, tableElement);
                        columnElement.AddChild(pAnnotation);
                    }

                    //ConfigManager.Log.Important("Tabular parser, Tabular Probe 8");
                    if (column.AttributeHierarchy != null)
                    {
                        var hierarchyRefPath = _urnBuilder.GetUrnHierarchy(column.AttributeHierarchy.Name, columnRefPath);
                        SsasTabularAttributeHierarchyElement pHierarchy = new SsasTabularAttributeHierarchyElement(hierarchyRefPath, column.AttributeHierarchy.Name, "Attribute hierarchy", columnElement);
                        if (column.AttributeHierarchy != null && column.AttributeHierarchy.Annotations != null)
                        {
                            //ConfigManager.Log.Important(string.Format("Annotations: {0}", column.AttributeHierarchy.Annotations));
                            foreach (var annotation in column.AttributeHierarchy.Annotations)
                            {
                                //ConfigManager.Log.Important(string.Format("Annotation: {0}", annotation));
                                if (annotation.Value != null)
                                {
                                    var annotationRefPath = _urnBuilder.GetUrnAnnotation(annotation.Name, hierarchyRefPath);
                                    SsasTabularAnnotationElement pAnnotation = new SsasTabularAnnotationElement(annotationRefPath, annotation.Name, annotation.Value, pHierarchy);
                                    pHierarchy.AddChild(pAnnotation);
                                }
                            }
                        }

                        //ConfigManager.Log.Important("Tabular parser, Tabular Probe 9");
                        columnElement.AddChild(pHierarchy);
                    }
                    tableElement.AddChild(columnElement);
                }

                //foreach (var column in table.Columns)
                //{
                //    var columnElement = columnsToElements[column];
                //    if (column.ColumnType == TabularTableColumnTypeEnum.Calculated)
                //    {
                //        var calculatedColumnExpression = column.Expression;
                //        Dictionary<string, SsasModelElement> resultColumns;
                //        var scriptModel = columnCalculationsExtractor.ExtractDaxScript(calculatedColumnExpression, columnCalculationsIndex, columnElement, out resultColumns);
                //        if (scriptModel == null)
                //        {
                //            ConfigManager.Log.Warning("Failed to parse " + columnElement.RefPath.Path);
                //            continue;
                //        }
                //        columnElement.AddChild(scriptModel);
                //        columnElement.Reference = resultColumns.First().Value;
                //    }
                //}

                //ConfigManager.Log.Important("Tabular parser, Tabular Probe 10");

                foreach (var partition in table.Partitions)
                {
                    var partitionRefPath = _urnBuilder.GetUrnPartition(partition.Name, tableRefPath);
                    SsasTabularPartitionElement partitionElement = new SsasTabularPartitionElement(partitionRefPath, partition.Name, partition.DataSourceName, tableElement);

                    foreach (var annotation in partition.Annotations)
                    {
                        var annotationRefPath = _urnBuilder.GetUrnAnnotation(annotation.Name, partitionRefPath);
                        SsasTabularAnnotationElement pAnnotation = new SsasTabularAnnotationElement(annotationRefPath, annotation.Name, annotation.Value, partitionElement);
                        partitionElement.AddChild(pAnnotation);
                    }

                    tableElement.AddChild(partitionElement);


                    Dictionary<string, MssqlModelElement> outputColumnsFromNames = null;

                    if (partition.PartitionSourceType == TabularPartitionSourceTypeEnum.QueryPartitionSource)
                    {
                        // datasource not found
                        if (!availableDatasources.ContainsKey(partition.DataSourceName))
                        {
                            throw new Exception(string.Format("Could not find datasource [{0}] for partition [{1}] in [{2}]", partition.DataSourceName, partition.Name, table.Name));
                        }

                        var dataSource = availableDatasources[partition.DataSourceName];
                        var sqlServerName = dataSource.ServerName;
                        var sqlDbName = dataSource.DbName;

                        Identifier dbIdentifier = null;

                        bool relDbAvailable = sqlDbName != null;
                        if (relDbAvailable)
                        {
                            dbIdentifier = new Microsoft.SqlServer.TransactSql.ScriptDom.Identifier() { Value = sqlDbName };
                        }

                        var query = partition.Query; // partitionQuerySrc.QueryDefinition;

                        //ConfigManager.Log.Info(string.Format("Parsing {0} over [{1}].[{2}]", query, sqlServerName, sqlDbName));

                        var dbIndex = _availableDatabaseModelIndex.GetDatabaseIndex(sqlServerName);
                        _sqlScriptExtractor.ContextServerName = sqlServerName;

                        ConfigManager.Log.Info(string.Format("Parsing SQL partition {0} to table {1}", partitionElement.RefPath.Path, table.Name));
                        var sqlElement = _sqlScriptExtractor.ExtractScriptModel(query, partitionElement, dbIndex, dbIdentifier, out outputColumnsFromNames);
                        partitionElement.AddChild(sqlElement);

                        //ConfigManager.Log.Info(string.Format("Parsed {0} over [{1}].[{2}], output columns {3}", query, sqlServerName, sqlDbName, string.Join(", ", outputColumnsFromNames.Select(x => "[" + x.Key + "]"))));
                    }
                    else if (partition.PartitionSourceType == TabularPartitionSourceTypeEnum.MLanguagePartitionSource)
                    {
                        PowerQueryExtractor extractor = new PowerQueryExtractor();
                        foreach (var ds in availableDatasources.Values)
                        {
                            extractor.AddLocalDataSource(new PowerQuery.DataSource()
                            {
                                DataSourceName = ds.Caption,
                                ServerName = ds.ServerName,
                                DbName = ds.DbName
                            });
                        }
                        var mScript = partition.Expression;

                        var mScriptNormalized = NormalizeMQuery(mScript);
                        //Model.SolutionModelElement fakeParent = new Model.SolutionModelElement(new RefPath(), "Root");
                        Dictionary<string, DLS.Model.Mssql.PowerQuery.OperationOutputColumnElement> powerQueryColumnsFromNames;

                        ConfigManager.Log.Info(string.Format("Parsing M partition {0} to table {1}", partitionElement.RefPath.Path, table.Name));


                        var scriptModel = extractor.ExtractPowerQuery(mScriptNormalized, _availableDatabaseModelIndex, partitionElement, out powerQueryColumnsFromNames);
                        //var scriptModel = extractor.ExtractPowerQuery(mScript, _availableDatabaseModelIndex, partitionElement, out powerQueryColumnsFromNames);


                        outputColumnsFromNames = powerQueryColumnsFromNames.ToDictionary(x => x.Key, x => (MssqlModelElement)(x.Value));
                    }
                    else
                    {
                        ConfigManager.Log.Important(string.Format("Skipping partition {0} of type {1} in {2}", partition.Name, partition.PartitionSourceType, table.Name));
                        // for now
                        continue;
                    }

                    if (outputColumnsFromNames == null)
                    {
                        continue;
                    }

                    foreach (var outputColumnName in outputColumnsFromNames.Keys)
                    {
                        var partitionColumnRefPath = _urnBuilder.GetPartitionColumnUrn(outputColumnName, partitionRefPath);
                        var partitionColumnElement = new SsasTabularPartitionColumnElement(partitionColumnRefPath, outputColumnName, null, partitionElement);
                        partitionElement.AddChild(partitionColumnElement);
                        partitionColumnElement.SourceElement = outputColumnsFromNames[outputColumnName];

                        if (tableSourceColumnsToColumns.ContainsKey(outputColumnName))
                        {
                            partitionColumnElement.TargetTableColumn = tableSourceColumnsToColumns[outputColumnName];

                            //ConfigManager.Log.Info(string.Format("Table column input: {0} -> {1}", outputColumnsFromNames[outputColumnName].RefPath.Path, tableSourceColumnsToColumns[outputColumnName].RefPath.Path));

                        }
                    }

                }

                //ConfigManager.Log.Important("Tabular parser, Tabular Probe 11");
                foreach (var hierarchy in table.Hierarchies)
                {
                    var hierarchyRefPath = _urnBuilder.GetUrnHierarchy(hierarchy.Name, tableRefPath);
                    SsasTabularHierarchyElement pHierarchy = new SsasTabularHierarchyElement(hierarchyRefPath, hierarchy.Name, "Table hierarchy", tableElement);

                    foreach (var annotation in hierarchy.Annotations)
                    {
                        var annotationRefPath = _urnBuilder.GetUrnAnnotation(annotation.Name, hierarchyRefPath);
                        SsasTabularAnnotationElement pAnnotation = new SsasTabularAnnotationElement(annotationRefPath, annotation.Name, annotation.Value, pHierarchy);
                        pHierarchy.AddChild(pAnnotation);
                    }

                    tableElement.AddChild(pHierarchy);
                }

                //ConfigManager.Log.Important("Tabular parser, Tabular Probe 12");
                // measures povodne
                foreach (var measure in table.Measures)
                {
                    //      Debug.WriteLine("Measure found");
                    var measureRefPath = _urnBuilder.GetUrnMeasure(measure.Name, tableRefPath);
                    SsasTabularMeasureElement measureElement = new SsasTabularMeasureElement(measureRefPath, measure.Name, measure.Expression, tableElement);
                    tableElement.AddChild(measureElement);

                    //measureList.Add(new Tuple<TabularTableMeasure, SsasTabularMeasureElement>(measure, measureElement));
                    //Debug.WriteLine("MEASURE  " + measure.Expression);
                    foreach (var annotation in measure.Annotations)
                    {
                        var annotationRefPath = _urnBuilder.GetUrnAnnotation(annotation.Name, measureRefPath);
                        SsasTabularAnnotationElement pAnnotation = new SsasTabularAnnotationElement(annotationRefPath, annotation.Name, annotation.Value, measureElement);

                    }


                }

                tDatabaseElement.AddChild(tableElement);
            }

            SsasTabularDatabaseIndex tabularIndex = new SsasTabularDatabaseIndex(tDatabaseElement);

            // create relationships
            foreach (var relationship in model.Relationships)
            {
                var sourceColumn = tabularIndex.FindColumn(relationship.FromTable, relationship.FromColumn);
                var targetColumn = tabularIndex.FindColumn(relationship.ToTable, relationship.ToColumn);
                TM.TabularRelationshipEndCardinality fromCardinality = (TM.TabularRelationshipEndCardinality)Enum.Parse(typeof(TM.TabularRelationshipEndCardinality), relationship.FromCardinality.ToString());
                TM.TabularRelationshipEndCardinality toCardinality = (TM.TabularRelationshipEndCardinality)Enum.Parse(typeof(TM.TabularRelationshipEndCardinality), relationship.ToCardinality.ToString());
                var urn = _urnBuilder.GetUrnRelationship(relationship.Name, tDatabaseElement.RefPath);
                var relationshipElement = new SsasTabularRelationshipElement(urn, relationship.Name, null, tDatabaseElement);
                tDatabaseElement.AddChild(relationshipElement);
                relationshipElement.FromColumn = tabularIndex.FindColumn(relationship.FromTable, relationship.FromColumn);
                relationshipElement.ToColumn = tabularIndex.FindColumn(relationship.ToTable, relationship.ToColumn);
                relationshipElement.FromColumnCardinality = fromCardinality;
                relationshipElement.ToColumnCardinality = toCardinality;
                relationshipElement.IsActive = relationship.IsActive;

                foreach (var annotation in relationship.Annotations)
                {
                    var annotationRefPath = _urnBuilder.GetUrnAnnotation(annotation.Name, relationshipElement.RefPath);
                    SsasTabularAnnotationElement pAnnotation = new SsasTabularAnnotationElement(annotationRefPath, annotation.Name, annotation.Value, relationshipElement);
                    relationshipElement.AddChild(pAnnotation);
                }
            }

            // refresh the index to include the relationship
            tabularIndex = new SsasTabularDatabaseIndex(tDatabaseElement);

            // parse calculated columns and measures
            DaxScriptModelExtractor daxExtractor = new DaxScriptModelExtractor();
            foreach (var modelTable in tDatabaseElement.Tables)
            {
                var extractTable = model.TabularTables.First(x => x.Name == modelTable.Caption);
                tabularIndex.SetContextTable(extractTable.Name);

                foreach (var modelColumn in modelTable.Columns)
                {
                    var extractColumn = extractTable.Columns.First(x => x.Name == modelColumn.Caption);
                    if (extractColumn.ColumnType != TabularTableColumnTypeEnum.Calculated)
                    {
                        continue;
                    }

                    var calculatedColumnExpression = extractColumn.Expression;
                    Dictionary<string, SsasModelElement> resultColumns;
                    var scriptModel = daxExtractor.ExtractDaxScript(calculatedColumnExpression, tabularIndex, modelColumn, out resultColumns);
                    if (scriptModel == null)
                    {
                        ConfigManager.Log.Warning("Failed to parse " + modelColumn.RefPath.Path);
                        continue;
                    }
                    modelColumn.AddChild(scriptModel);
                    modelColumn.Reference = resultColumns.First().Value;

                }

                foreach (var modelMeasure in modelTable.Measures)
                {
                    var extractMeasure = extractTable.Measures.First(x => x.Name == modelMeasure.Caption);
                    var measureExpression = extractMeasure.Expression;

                    if (string.IsNullOrEmpty(measureExpression))
                    {
                        continue;
                    }

                    Dictionary<string, SsasModelElement> resultColumns;
                    var scriptModel = daxExtractor.ExtractDaxScript(measureExpression, tabularIndex, modelMeasure, out resultColumns);
                    if (scriptModel == null)
                    {
                        ConfigManager.Log.Warning("Failed to parse " + modelMeasure.RefPath.Path);
                        continue;
                    }
                    modelMeasure.AddChild(scriptModel);
                    modelMeasure.Reference = resultColumns.First().Value;
                }


            }

            tabularIndex.ClearLocalIndexes();

            //var databaseIndex = new SsasTabularDatabaseIndex(tDatabaseElement);
            //var measureExtractor = new DaxScriptModelExtractor();

            //foreach (var measure in measureList)
            //{
            //    // columnCalculationsIndex.AddLocalColumn(column.Name, columnElement);

            //    databaseIndex.ClearLocalIndexes();
            //    var measureExpression = measure.Item1.Expression;
            //    var measureElement = measure.Item2;
            //    var table = measureElement.Parent as SsasTabularTableElement;

            //    if (table != null)
            //    {
            //        foreach (var column in table.Columns)
            //        {
            //            databaseIndex.AddLocalColumn(column.Caption, column);
            //            //databaseIndex.add
            //        }
            //    }

            //    Dictionary<string, SsasModelElement> resultColumns;
            //    var scriptModel = measureExtractor.ExtractDaxScript(measureExpression, databaseIndex, measureElement, out resultColumns);
            //    if (scriptModel == null)
            //    {
            //        ConfigManager.Log.Warning("Failed to parse " + measureElement.RefPath.Path);
            //        continue;
            //    }
            //    measureElement.AddChild(scriptModel);
            //    measureElement.Reference = resultColumns.First().Value;
            //}

        }

        public SsasTabularDatabaseElement ExtractTabularDatabase(int componentID, DLS.Model.Mssql.Ssas.ServerElement serverElement)
        {
            //ConfigManager.Log.Important("Tabular parser, Tabular Probe 3");

            ConfigManager.Log.Important("Tabular parser parameters, extractID: " + _extractId + " ComponentID " + componentID);
            var database = (TabularDB)_stageManager.GetExtractItems(_extractId, componentID, ExtractTypeEnum.TabularDB)[0];

            var model = (TabularModel)_stageManager.GetExtractItems(_extractId, componentID, ExtractTypeEnum.TabularModel)[0];

            //List<Tuple<TabularTableMeasure, SsasTabularMeasureElement>> measureList = new List<Tuple<TabularTableMeasure, SsasTabularMeasureElement>>();
            //ConfigManager.Log.Important("Tabular parser, Tabular Probe 4");

            TabularDB tdb = (TabularDB)database;
            ConfigManager.Log.Important("Tabular parser : Parsing database " + database.Name);

            var dbRefPath = _urnBuilder.GetDatabaseUrn(database.Name, serverElement.RefPath);
            SsasTabularDatabaseElement tDatabaseElement = new SsasTabularDatabaseElement(dbRefPath, tdb.DBName, tdb.Description, serverElement);
            serverElement.AddChild(tDatabaseElement);

            tDatabaseElement.Collation = tdb.Collation;

            foreach (var annotation in tdb.Annotations)
            {
                var annotationRefPath = _urnBuilder.GetUrnAnnotation(annotation.Name, dbRefPath);

                SsasTabularAnnotationElement pAnnotation = new SsasTabularAnnotationElement(annotationRefPath, annotation.Name, annotation.Value, tDatabaseElement);
                tDatabaseElement.AddChild(pAnnotation);
            }

            ExtractTabularModel(model, tDatabaseElement);

            ConfigManager.Log.Important("Tabular parser, DB parsed!");

            return tDatabaseElement;
        }


    }

}

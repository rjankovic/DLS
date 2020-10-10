using System;
using System.Collections.Generic;
using System.Linq;
using CD.DLS.Model.Mssql.Ssas;
using CD.DLS.Model.Mssql;
using CD.DLS.Model.Mssql.Db;
using System.Data;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using CD.DLS.Parse.Mssql.Db;
using CD.DLS.Common.Structures;
using CD.DLS.DAL.Managers;
using CD.DLS.DAL.Configuration;
using CD.DLS.DAL.Objects.Extract;
using CD.DLS.Model.Interfaces;

namespace CD.DLS.Parse.Mssql.Ssas
{
    public class SsasModelExtractor
    {

        private UrnBuilder _urnBuilder = new UrnBuilder();
        
        private ISqlScriptModelParser _sqlExtractor;
        //private IReferrableIndex _sqlContext;
        private ProjectConfig _projectConfig;
        private Guid _extractId;
        private StageManager _stageManager;
        private GraphManager _graphManager;
        private AvailableDatabaseModelIndex _availableSqlDatabaseIndex;

        public SsasModelExtractor(/*IReferrableIndex sqlContext,*/ ProjectConfig projectConfig,
            Guid extractId, StageManager stageManager, GraphManager graphManager, AvailableDatabaseModelIndex availableSqlDatabaseIndex)
        {
            
            ScriptModelParser sqlExtractor = new ScriptModelParser();

            _sqlExtractor = sqlExtractor;
            //_sqlContext = sqlContext;
            _projectConfig = projectConfig;
            _extractId = extractId;
            _stageManager = stageManager;
            _graphManager = graphManager;
            _availableSqlDatabaseIndex = availableSqlDatabaseIndex;
        }

        #region MAPPING_STRUCTURES
        public class DataSourceViewMap
        {
            public Dictionary<string, DataSourceViewTableMap> Dictionary = new Dictionary<string, DataSourceViewTableMap>();
            public DataSourceViewTableMap this[string id]
            {
                get { return Dictionary[id]; }
                set { Dictionary[id] = value; }
            }
        }

        public class DataSourceViewTableMap
        {
            public DatasourceViewElement GraphNode { get; set; }
            public Dictionary<string, DataSourceViewColumnMap> Dictionary = new Dictionary<string, DataSourceViewColumnMap>();
            public DataSourceViewColumnMap this[string name]
            {
                get { return Dictionary[name]; }
                set { Dictionary[name] = value; }
            }
        }

        public class DataSourceViewColumnMap
        {
            // cannot use view table element - a pratition may be query - bound
            public MssqlModelElement /*DataSourceViewTableElement*/ ModelElement { get; set; }
            public Dictionary<string, MssqlModelElement> Dictionary = new Dictionary<string, MssqlModelElement>(StringComparer.OrdinalIgnoreCase);
            public MssqlModelElement this[string name]
            {
                get { return Dictionary[name]; }
                set { Dictionary[name] = value; }
            }
        }

        public class PartitionColumnMap
        {
            public MssqlModelElement ModelElement { get; set; }
            public Dictionary<string, PartitionColumnElement> Dictionary = new Dictionary<string, PartitionColumnElement>(StringComparer.OrdinalIgnoreCase);
            public PartitionColumnElement this[string name]
            {
                get { return Dictionary[name]; }
                set { Dictionary[name] = value; }
            }
        }

        public class DimensionMap
        {
            public Dictionary<string, DimensionAttributeMap> Dictionary = new Dictionary<string, DimensionAttributeMap>();
            public DimensionAttributeMap this[string id]
            {
                get { return Dictionary[id]; }
                set { Dictionary[id] = value; }
            }
        }

        public class DimensionAttributeMap
        {
            public DimensionElement ModelElement { get; set; }
            public Dictionary<string, DimensionAttributeKeyColumnMap> Dictionary = new Dictionary<string, DimensionAttributeKeyColumnMap>();
            public DimensionAttributeKeyColumnMap this[string id]
            {
                get { return Dictionary[id]; }
                set { Dictionary[id] = value; }
            }
        }

        public class DimensionAttributeKeyColumnMap
        {
            public DimensionAttributeElement ModelElement { get; set; }
            public Dictionary<string, KeyColumnElement> Dictionary = new Dictionary<string, KeyColumnElement>();
            public KeyColumnElement this[string id]
            {
                get { return Dictionary[id]; }
                set { Dictionary[id] = value; }
            }
        }

        #endregion
        private DataSourceViewMap _dsvMap = new DataSourceViewMap();
        private DimensionMap _dimensionMap = new DimensionMap();

        public List<Model.Mssql.Ssas.ServerElement> ExtractModel()
        {
            //_urnBuilder = new UrnBuilder();
            List<Model.Mssql.Ssas.ServerElement> res = new List<Model.Mssql.Ssas.ServerElement>();
            foreach (var ssasComponent in _projectConfig.SsasComponents.OrderBy(x => x.ServerName))
            {
                ConfigManager.Log.Important("Extracting SSAS DB {0} from {1}", ssasComponent.DbName, ssasComponent.ServerName);
                
                var dbExtract = (MultidimensionalDatabase)(_stageManager.GetExtractItems(_extractId, ssasComponent.SsaslDbProjectComponentId, ExtractTypeEnum.SsasMultidimensionalDatabase)[0]);
                
                Model.Mssql.Ssas.ServerElement serverElement = null;
                if (res.Count == 0 || !Common.Tools.ConnectionStringTools.AreServersNamesEqual(res.Last().Caption, ssasComponent.ServerName))
                {
                    var refPath = _urnBuilder.GetServerUrn(dbExtract.ServerID);
                    serverElement = new Model.Mssql.Ssas.ServerElement(refPath, dbExtract.ServerName);
                    res.Add(serverElement);
                }
                else
                {
                    serverElement = res.Last();
                }

                AddDatabase(ssasComponent.SsaslDbProjectComponentId, dbExtract, serverElement);
            }
            return res;
        }


        private void AddDatabase(int ssasComponentId, MultidimensionalDatabase dbExtract, Model.Mssql.Ssas.ServerElement parent)
        {
            var refPath = _urnBuilder.GetDatabaseUrn(dbExtract.DbID, parent.RefPath);
            var dbElement = new Model.Mssql.Ssas.SsasMultidimensionalDatabaseElement(refPath, dbExtract.DbName, null, parent);
            dbElement.SsasObjectID = dbExtract.DbID;
            parent.AddChild(dbElement);
            AddDataSourceViews(ssasComponentId, dbElement);
            AddDimensions(ssasComponentId, dbElement);
            AddCubes(ssasComponentId, dbElement);
        }

        public Model.Mssql.Ssas.SsasMultidimensionalDatabaseElement ExtractDatabase(int ssasComponentId, MultidimensionalDatabase dbExtract, Model.Mssql.Ssas.ServerElement parent)
        {
            var refPath = _urnBuilder.GetDatabaseUrn(dbExtract.DbID, parent.RefPath);
            var dbElement = new Model.Mssql.Ssas.SsasMultidimensionalDatabaseElement(refPath, dbExtract.DbName, null, parent);
            dbElement.SsasObjectID = dbExtract.DbID;
            parent.AddChild(dbElement);
            AddDataSourceViews(ssasComponentId, dbElement);
            AddDimensions(ssasComponentId, dbElement);
            AddCubes(ssasComponentId, dbElement);
            return dbElement;
        }

        private void AddCubes(int ssasComponentId, Model.Mssql.Ssas.SsasMultidimensionalDatabaseElement parent)
        {
            var cubeExtracts = _stageManager.GetExtractItems(_extractId, ssasComponentId, ExtractTypeEnum.SsasMultidimensionalCube).Select(x => (MultidimensionalCube)x).ToList();

            foreach (MultidimensionalCube cubeExtract in cubeExtracts)
            {
                Dictionary<string, CubeDimensionElement> cubeDimDictionary = new Dictionary<string, CubeDimensionElement>();

                var cubeUrn = _urnBuilder.GetUrn(cubeExtract, parent.RefPath);
                var cubeElement = new CubeElement(cubeUrn, cubeExtract.Name, null, parent);
                parent.AddChild(cubeElement);
                foreach (DAL.Objects.Extract.CubeDimension cubedim in cubeExtract.Dimensions)
                {
                    var cubeDimUrn = _urnBuilder.GetUrn(cubedim, cubeElement.RefPath);
                    var cubeDimElement = new CubeDimensionElement(cubeDimUrn, cubedim.Name, null, cubeElement);
                    cubeElement.AddChild(cubeDimElement);
                    var sourceDimElement = _dimensionMap[cubedim.DimensionID].ModelElement;
                    cubeDimElement.DatabaseDimension = sourceDimElement;
                    cubeDimDictionary.Add(cubedim.ID, cubeDimElement);

                    Dictionary<string, CubeDimensionAttributeElement> cubeDimensionAttributesBySourceAttributeId = new Dictionary<string, CubeDimensionAttributeElement>();

                    foreach (var sourceDimAttribute in sourceDimElement.Attributes)
                    {
                        var attrRefPath = _urnBuilder.GetCubeDimensionAttributeUrn(sourceDimAttribute, cubeDimElement.RefPath);
                        CubeDimensionAttributeElement attrElem = new CubeDimensionAttributeElement(attrRefPath, sourceDimAttribute.Caption, null, cubeDimElement);
                        cubeDimElement.AddChild(attrElem);
                        attrElem.DatabaseDimensionAttribute = sourceDimAttribute;
                        cubeDimensionAttributesBySourceAttributeId.Add(sourceDimAttribute.SsasObjectID, attrElem);
                    }

                    foreach (var hierarchy in sourceDimElement.Hierarchies)
                    {
                        var cubeHierarchyRefPath = _urnBuilder.GetCubeDimensionHierarchyUrn(hierarchy, cubeDimElement.RefPath);
                        var cubeHierarchyElement = new CubeDimensionHierarchyElement(cubeHierarchyRefPath, hierarchy.Caption, null, cubeDimElement);
                        cubeDimElement.AddChild(cubeHierarchyElement);
                        cubeHierarchyElement.DatabaseDimensionHierarchy = hierarchy;

                        foreach (var hierarchyLevel in hierarchy.Levles)
                        {
                            var cubeHierarchyLevelRefPath = _urnBuilder.GetCubeDimensionHierarchyLevelUrn(hierarchyLevel, cubeHierarchyElement.RefPath);
                            var cubeHierarchyLevelElement = new CubeDimensionHierarchyLevelElement(cubeHierarchyLevelRefPath, hierarchyLevel.Caption, null, cubeHierarchyElement);
                            cubeHierarchyElement.AddChild(cubeHierarchyLevelElement);
                            cubeHierarchyLevelElement.Ordinal = hierarchyLevel.Ordinal;
                            cubeHierarchyLevelElement.Attribute = cubeDimensionAttributesBySourceAttributeId[hierarchyLevel.Attribute.SsasObjectID];
                        }
                    }
                    
                }

                Dictionary<string, Tuple<CubeMeasureGroup, MeasureGroupElement>> measureGroupsById = new Dictionary<string, Tuple<CubeMeasureGroup, MeasureGroupElement>>();

                foreach (CubeMeasureGroup mg in cubeExtract.MeasureGroups)
                {
                    var mgUrn = _urnBuilder.GetUrn(mg, cubeElement.RefPath);
                    var mgElement = new MeasureGroupElement(mgUrn, mg.Name, null, cubeElement);
                    cubeElement.AddChild(mgElement);
                    measureGroupsById.Add(mg.ID, new Tuple<CubeMeasureGroup, MeasureGroupElement>(mg, mgElement));
                }

                foreach (CubeMeasureGroup mg in cubeExtract.MeasureGroups)
                {
                    var mgUrn = _urnBuilder.GetUrn(mg, cubeElement.RefPath);
                    var mgElement = measureGroupsById[mg.ID].Item2;

                    List<Tuple<PartitionElement, PartitionColumnMap>> partitionTableMaps = new List<Tuple<PartitionElement, PartitionColumnMap>>();

                    foreach (MeasureGroupPartition partition in mg.Partitions)
                    {
                        var partitionUrn = _urnBuilder.GetUrn(partition, mgElement.RefPath);

                        if (partition.BindingType == MeasureGroupPartitionBindingType.DsvTable /*Source is DsvTableBinding*/)
                        {
                            var partitionElement = new DsvTableBoundPartitionElement(partitionUrn, partition.Name, null, mgElement);
                            mgElement.AddChild(partitionElement);

                            //var partitionSrc = (DsvTableBinding)(partition.Source);
                            var tableMap = _dsvMap[cubeExtract.DataSourceViewID][partition.TableID];
                            PartitionColumnMap partitionMap = new PartitionColumnMap();
                            // maybe should be the source table or there could be a link from whole DSV table to DB table
                            partitionMap.ModelElement = tableMap.ModelElement;
                            foreach (var columnName in tableMap.Dictionary.Keys)
                            {
                                var ptColUrn = _urnBuilder.GetPartitionColumnUrn(columnName, partitionElement.RefPath);
                                PartitionColumnElement ptColElement = new PartitionColumnElement(ptColUrn, columnName, null, partitionElement);
                                partitionElement.AddChild(ptColElement);
                                ptColElement.Source = tableMap[columnName];
                                partitionMap[columnName] = ptColElement;
                            }

                            partitionTableMaps.Add(new Tuple<PartitionElement, PartitionColumnMap>(partitionElement, partitionMap));
                        }
                        else if (partition.BindingType == MeasureGroupPartitionBindingType.Query /*Source is QueryBinding*/)
                        {
                            var partitionElement = new QueryBoundPartitionElement(partitionUrn, partition.Name, null, mgElement);
                            mgElement.AddChild(partitionElement);

                            //var partitionQuerySrc = (QueryBinding)(partition.Source);
                            //var datasource = db.DataSources[partitionQuerySrc.DataSourceID];
                            var connString = partition.ConnectionString; // datasource.ConnectionString;
                            var dbName = _sqlExtractor.GetDbNameFromConnectionString(connString);
                            var serverName = _sqlExtractor.GetServerNameFromConnectionString(connString, mgElement.LocalhostServer());

                            Microsoft.SqlServer.TransactSql.ScriptDom.Identifier dbIdentifier = null;

                            bool relDbAvailable = dbName != null;
                            if (relDbAvailable)
                            {
                                dbIdentifier = new Microsoft.SqlServer.TransactSql.ScriptDom.Identifier() { Value = _sqlExtractor.GetDbNameFromConnectionString(connString) };
                            }

                            var query = partition.QueryDefinition; // partitionQuerySrc.QueryDefinition;
                            Dictionary<string, MssqlModelElement> outputColumnsFromNames;
                            var dbIndex = _availableSqlDatabaseIndex.GetDatabaseIndex(serverName, mgElement.LocalhostServer());
                            //_sqlContext.ContextServerName = serverName;
                            var sqlElement = _sqlExtractor.ExtractScriptModel(query, partitionElement, dbIndex, dbIdentifier, out outputColumnsFromNames);
                            partitionElement.AddChild(sqlElement);

                            var tableMap = new PartitionColumnMap() { ModelElement = sqlElement };

                            foreach (var outputColumn in outputColumnsFromNames)
                            {
                                var ptColUrn = _urnBuilder.GetPartitionColumnUrn(outputColumn.Key, partitionElement.RefPath);
                                PartitionColumnElement ptColElement = new PartitionColumnElement(ptColUrn, outputColumn.Key, null, partitionElement);
                                partitionElement.AddChild(ptColElement);
                                ptColElement.Source = outputColumn.Value;
                                tableMap[outputColumn.Key] = ptColElement;
                            }
                            partitionTableMaps.Add(new Tuple<PartitionElement, PartitionColumnMap>(partitionElement, tableMap));

                        }
                        else if (partition.BindingType == MeasureGroupPartitionBindingType.DbTable)
                        {

                            var partitionElement = new TableBoundPartitionElement(partitionUrn, partition.Name, null, mgElement);
                            mgElement.AddChild(partitionElement);

                            //var tableBinding = (TableBinding)partition.Source;
                            //var datasource = db.DataSources[tableBinding.DataSourceID];
                            var connString = partition.ConnectionString; //datasource.ConnectionString;
                            var dbName = _sqlExtractor.GetDbNameFromConnectionString(connString);

                            Microsoft.SqlServer.TransactSql.ScriptDom.Identifier dbIdentifier = null;
                            bool relDbAvailable = dbName != null;

                            if (relDbAvailable)
                            {
                                dbIdentifier = new Microsoft.SqlServer.TransactSql.ScriptDom.Identifier() { Value = _sqlExtractor.GetDbNameFromConnectionString(connString) };
                                var tableIdentifier = string.Format("[{0}].[{1}]", partition.TableBoundDbSchemaName, partition.DbTableName);
                                var serverName = _sqlExtractor.GetServerNameFromConnectionString(connString, mgElement.LocalhostServer());

                                var dbIndex = _availableSqlDatabaseIndex.GetDatabaseIndex(serverName, mgElement.LocalhostServer());
                                //_sqlContext.SetContextServer(serverName, mgElement.LocalhostServer());
                                //_sqlContext.ContextServerName = serverName;
                                var tableSource = dbIndex.FindTableByObjectName(tableIdentifier, _sqlExtractor.Parser, dbIdentifier);
                                if (tableSource != null)
                                {
                                    var tableMap = new PartitionColumnMap() { ModelElement = (MssqlModelElement)tableSource.ModelElement };
                                    partitionElement.SourceTable = (MssqlColumnScriptElement)tableMap.ModelElement;

                                    foreach (var outputColumn in tableSource.Columns)
                                    {
                                        var colName = outputColumn.Identifier.GetText();
                                        var ptColUrn = _urnBuilder.GetPartitionColumnUrn(colName, partitionElement.RefPath);
                                        PartitionColumnElement ptColElement = new PartitionColumnElement(ptColUrn, colName, null, partitionElement);
                                        partitionElement.AddChild(ptColElement);
                                        ptColElement.Source = (MssqlModelElement)outputColumn.ModelElement;
                                        tableMap[colName] = ptColElement;
                                    }
                                    partitionTableMaps.Add(new Tuple<PartitionElement, PartitionColumnMap>(partitionElement, tableMap));
                                }
                            }
                        }
                        else
                        {
                            throw new Exception("Unrecognized partition source binding type");
                        }
                        // regular relationships
                    }

                    /////////////////////////////////////////////// MG dimensions start ////////////////////////////////////////////////////////

                    HashSet<string> coveredMGDimensions = new HashSet<string>();

                    foreach (DAL.Objects.Extract.MeasureGroupDimension mgDim in mg.Dimensions)
                    {
                        if (!(mgDim is DAL.Objects.Extract.RegularMeasureGroupDimension && !(mgDim is DAL.Objects.Extract.ReferenceMeasureGroupDimension 
                            || mgDim is DAL.Objects.Extract.DegenerateMeasureGroupDimension)))
                        {
                            continue;
                        }
                        var mgDimUrn = _urnBuilder.GetUrn(mgDim, mgElement.RefPath);
                        var dimAttrMap = _dimensionMap[mgDim.DimensionID /*.Dimension.ID*/];

                        var mgDimReg = mgDim as DAL.Objects.Extract.RegularMeasureGroupDimension;
                        var mgDimRegElement = new RegularMeasureGroupDimensionElement(mgDimUrn, mgDim.DimensionName /*.Dimension.Name*/, null, mgElement);
                        mgElement.AddChild(mgDimRegElement);
                        //var mgDimSource = mgDimReg.Source;
                        //var attrs = mgDimReg.Attributes;


                        //MeasureGroupAttribute granularityAttr = null;
                        //foreach (MeasureGroupAttribute attr in attrs)
                        //{
                        //    if (attr.Type == MeasureGroupAttributeType.Granularity)
                        //    {
                        //        if (granularityAttr != null)
                        //        {
                        //            throw new Exception("Multiple granularity attributes ??");
                        //        }
                        //        granularityAttr = attr;
                        //    }
                        //}

                        //if (granularityAttr == null)
                        //{
                        //    throw new Exception("No granularity attribute ??");
                        //}

                        var keyColMap = dimAttrMap[mgDimReg.GranularityAttributeID];
                        //                        keyColMap.ModelElement.KeyColumns
                        
                        for (int i = 0; i < mgDimReg.DimensionColumnMappings.Count; i++)
                        {
                            var mapping = mgDimReg.DimensionColumnMappings[i];
                            var dimColumn = keyColMap[mapping.DimensionColumnId];
                            
                            var bindingUrn = _urnBuilder.GetColumnBindingUrn(mapping.PartitionColumnId, mapping.DimensionColumnId, mgDimRegElement.RefPath);
                            var bindingElement = new MeasureGroupDimensionColumnBindingElement(bindingUrn, mapping.PartitionColumnId, null, mgDimRegElement);
                            mgDimRegElement.AddChild(bindingElement);
                            bindingElement.DimensionAttributeKeyColumn = dimColumn;


                            foreach (var partitionTable in partitionTableMaps)
                            {
                                var tableMap = partitionTable.Item2;
                                var mgCol = tableMap[mapping.PartitionColumnId];
                                bindingElement.MeasureGroupSourceColumn = mgCol;
                                //var linkToMg = new BasicNodeLink() { NodeFrom = bindingElement, NodeTo = mgCol };
                                //bindingElement.Links.Add(linkToMg);
                            }
                        }

                        var cubeDimElement = cubeDimDictionary[mgDim.CubeDimensionID];
                        mgDimRegElement.CubeDimension = cubeDimElement;
                        mgDimRegElement.ReferenceDimensionAttribute = keyColMap.ModelElement;
                        coveredMGDimensions.Add(mgDim.CubeDimensionID);
                    }

                    // referenced relationships

                    foreach (DAL.Objects.Extract.MeasureGroupDimension mgDim in mg.Dimensions)
                    {
                        if (!(mgDim is DAL.Objects.Extract.ReferenceMeasureGroupDimension))
                        {
                            continue;
                        }
                        var mgDimUrn = _urnBuilder.GetUrn(mgDim, mgElement.RefPath);
                        var mgDimElement = new ReferencedMeasureGroupDimensionElement(mgDimUrn, mgDim.DimensionName, null, mgElement);
                        mgElement.AddChild(mgDimElement);

                        var mgDimRef = mgDim as DAL.Objects.Extract.ReferenceMeasureGroupDimension;
                        //var intAttr = mgDimRef.IntermediateGranularityAttribute;
                        //var refDimElement = cubeDimDictionary[mgDimRef.IntermediateCubeDimension.ID];
                        mgDimElement.IntermediateDimensionAttribute = _dimensionMap[mgDimRef.IntermediateGranularityAttributeDimensionID /* intAttr.Attribute.Parent.ID*/][mgDimRef.IntermediateGranularityAttributeID /*intAttr.Attribute.ID*/].ModelElement;
                        // TODO
                        //mgDimElement.ReferenceDimensionAttribute = // ....
                        mgDimElement.CubeDimension = cubeDimDictionary[mgDim.CubeDimensionID];


                        //var cubeDimNode = cubeDimDictionary[mgDim.CubeDimensionID];
                        //var cubeDimLink = new BasicNodeLink() { NodeFrom = mgDimElement, NodeTo = cubeDimNode };
                        //mgDimElement.Links.Add(cubeDimLink);

                        coveredMGDimensions.Add(mgDim.CubeDimensionID);
                        //mgDim.att
                    }

                    // M : N relationships

                    foreach (DAL.Objects.Extract.MeasureGroupDimension mgDim in mg.Dimensions)
                    {
                        if (!(mgDim is DAL.Objects.Extract.ManyToManyMeasureGroupDimension))
                        {
                            continue;
                        }
                        var mgDimUrn = _urnBuilder.GetUrn(mgDim, mgElement.RefPath);
                        var mgDimElement = new ManyToManyMeasureGroupDimensionElement(mgDimUrn, mgDim.DimensionName, null, mgElement);
                        mgElement.AddChild(mgDimElement);

                        var mgDimMN = mgDim as DAL.Objects.Extract.ManyToManyMeasureGroupDimension;

                        var intermediateMgElement = measureGroupsById[mgDimMN.MeasureGroupID].Item2;
                        mgDimElement.IntermediateMeasureGroup = intermediateMgElement;

                        var cubeDimElement = cubeDimDictionary[mgDim.CubeDimensionID];
                        mgDimElement.CubeDimension = cubeDimElement;

                        coveredMGDimensions.Add(mgDim.CubeDimensionID);
                        //mgDim.att
                    }

                    // Fact relationshisp (degenerated)
                    foreach (DAL.Objects.Extract.MeasureGroupDimension mgDim in mg.Dimensions)
                    {
                        if (!(mgDim is DAL.Objects.Extract.DegenerateMeasureGroupDimension))
                        {
                            continue;
                        }
                        var mgDimUrn = _urnBuilder.GetUrn(mgDim, mgElement.RefPath);
                        var mgDimElement = new DegenerateMeasureGroupDimensionElement(mgDimUrn, mgDim.DimensionName, null, mgElement);
                        mgElement.AddChild(mgDimElement);

                        var degen = mgDim as DAL.Objects.Extract.DegenerateMeasureGroupDimension;

                        var cubeDimElement = cubeDimDictionary[mgDim.CubeDimensionID];
                        mgDimElement.CubeDimension = cubeDimElement;

                        coveredMGDimensions.Add(mgDim.CubeDimensionID);
                    }

                    // data mining dimensions, whatever that means
                    foreach (DAL.Objects.Extract.MeasureGroupDimension mgDim in mg.Dimensions)
                    {
                        if (!(mgDim is DAL.Objects.Extract.DataMiningMeasureGroupDimension))
                        {
                            continue;
                        }
                        var mgDimUrn = _urnBuilder.GetUrn(mgDim, mgElement.RefPath);
                        var mgDimElement = new DataMiningMeasureGroupDimensionElement(mgDimUrn, mgDim.DimensionName, null, mgElement);
                        mgElement.AddChild(mgDimElement);
                        var degen = mgDim as DAL.Objects.Extract.DegenerateMeasureGroupDimension;

                        var cubeDimElement = cubeDimDictionary[mgDim.CubeDimensionID];
                        mgDimElement.CubeDimension = cubeDimElement;

                        coveredMGDimensions.Add(mgDim.CubeDimensionID);
                    }


                    // default basic handling for special MG dimensions 

                    foreach (DAL.Objects.Extract.MeasureGroupDimension mgDim in mg.Dimensions)
                    {
                        if (coveredMGDimensions.Contains(mgDim.CubeDimensionID))
                        {
                            continue;
                        }
                        throw new Exception("Measure group dimension relationship type not recognized");
                    }

                    ////////////////////////////////////// MG dimensions end //////////////////////////////////

                    ////////////////////////////////////////// Measures start /////////////////////////////////////

                    foreach (PhysicalCubeMeasure measure in mg.Measures)
                    {
                        var measureUrn = _urnBuilder.GetUrn(measure, mgElement.RefPath);
                        var measureElement = new PhysicalMeasureElement(measureUrn, measure.Name, measure.MeasureExpression, mgElement);
                        mgElement.AddChild(measureElement);

                        //var src = measure.Source.Source;
                        
                        if (measure.BindingType == PhysicalMeasureBindingType.ColumnBinding /*src is ColumnBinding*/)
                        {
                            //var colBinding = src as ColumnBinding;
                            foreach (var partitionTable in partitionTableMaps)
                            {
                                var sourceColumnElement = partitionTable.Item2[measure.ColumnID /*colBinding.ColumnID*/];
                                var mgPtSourceUrn = _urnBuilder.GetMeasurePartitionSourceUrn(partitionTable.Item1, measureElement.RefPath);
                                var mgPtSourceElement = new PhysicalMeasurePartitionSourceElement(mgPtSourceUrn, partitionTable.Item1.Caption, null, measureElement);
                                mgPtSourceElement.Source = sourceColumnElement;
                                measureElement.AddChild(mgPtSourceElement);

                                //measureElement.Source = sourceColumnElement;
                            }
                        }
                        else if (measure.BindingType == PhysicalMeasureBindingType.RowBinding)
                        {
                            //var rowBinding = src as RowBinding;
                            foreach (var partitionTable in partitionTableMaps)
                            {
                                var sourceTableColumn = partitionTable.Item2.ModelElement;
                                var mgPtSourceUrn = _urnBuilder.GetMeasurePartitionSourceUrn(partitionTable.Item1, measureElement.RefPath);
                                var mgPtSourceElement = new PhysicalMeasurePartitionSourceElement(mgPtSourceUrn, partitionTable.Item1.Caption, null, measureElement);
                                mgPtSourceElement.Source = partitionTable.Item2.ModelElement;
                                measureElement.AddChild(mgPtSourceElement);

                                //measureElement.Source = sourceTableElement;
                            }
                        }
                        //else if (src is MeasureBinding)
                        //{
                        //    // TODO
                        //}
                        //else
                        //{
                        //    throw new Exception();
                        //}
                    }


                    ///////////////////////////////////////// Measures end ////////////////////////////////////////


                }   // MGs


                AddCalculations(cubeExtract, cubeElement);


            }   // cubes
        }

        private void AddCalculations(MultidimensionalCube cube, CubeElement cubeElement)
        {
            SsasMultidimensionalDatabaseIndex cubeIndex = new SsasMultidimensionalDatabaseIndex(cubeElement, _graphManager, _projectConfig);
            MdxScriptModelExtractor mdxExtractor = new MdxScriptModelExtractor();
            mdxExtractor.ExtractCubeCalculations(cube, cubeElement, cubeIndex);



        }

        private void AddDimensions(int ssasComponentId, Model.Mssql.Ssas.SsasMultidimensionalDatabaseElement parent)
        {
            var dimExtracts = _stageManager.GetExtractItems(_extractId, ssasComponentId, ExtractTypeEnum.SsasMultidimensionalDimension).Select(x => (MultidimensionalDimension)x).ToList();

            foreach (MultidimensionalDimension dimExtract in dimExtracts)
            {
                var attributeNodeDictionary = new Dictionary<string, DimensionAttributeElement>();

                var dimUrn = _urnBuilder.GetUrn(dimExtract, parent.RefPath);
                var dimElement = new DimensionElement(dimUrn, dimExtract.Name, null, parent);
                dimElement.SsasObjectID = dimExtract.ID;
                parent.AddChild(dimElement);
                var attributeMap = new DimensionAttributeMap() { ModelElement = dimElement };
                _dimensionMap[dimExtract.ID] = attributeMap;

                //var dimSource = (DataSourceViewBinding)(dimExtract.Source);
                var dsvId = dimExtract.DataSourceViewID; //dimSource.DataSourceViewID;
                foreach (DAL.Objects.Extract.DimensionAttribute attr in dimExtract.Attributes)
                {
                    var attrUrn = _urnBuilder.GetUrn(attr, dimElement.RefPath);
                    var attrElement = new DimensionAttributeElement(attrUrn, attr.Name, null, dimElement);
                    dimElement.AddChild(attrElement);
                    attrElement.AttributeHierarchyEnabled = attr.AttributeHierarchyEnabled;
                    attrElement.SsasObjectID = attr.ID;
                    attributeNodeDictionary.Add(attr.ID, attrElement);
                    var keyColumnMap = new DimensionAttributeKeyColumnMap() { ModelElement = attrElement };
                    attributeMap[attr.ID] = keyColumnMap;

                    foreach (MultidimensionalColumnBinding keyColumn in attr.KeyColumns)
                    {
                        var colId = keyColumn.ColumnID;
                        var tableId = keyColumn.TableID;

                        var sourceElement = _dsvMap[dsvId][tableId][colId];
                        var keyColumnUrn = _urnBuilder.GetKeyColumnUrn(keyColumn, attrElement.RefPath);
                        var keyColumnElement = new KeyColumnElement(keyColumnUrn, keyColumn.ColumnID, null, attrElement);
                        attrElement.AddChild(keyColumnElement);

                        keyColumnElement.DsvColumn = sourceElement;


                        keyColumnMap[keyColumn.ColumnID] = keyColumnElement;
                    }

                    if (attr.ValueColumn != null)
                    {
                        var binding = attr.ValueColumn; //(ColumnBinding)(attr.ValueColumn.Source);
                        var colId = binding.ColumnID;
                        var tableId = binding.TableID;

                        var sourceElement = _dsvMap[dsvId][tableId][colId];
                        var valueColumnUrn = _urnBuilder.GetValueColumnUrn(binding, attrElement.RefPath);
                        var valueColumnElement = new ValueColumnElement(valueColumnUrn, binding.ColumnID, null, attrElement);
                        attrElement.AddChild(valueColumnElement);
                        valueColumnElement.DsvColumn = sourceElement;
                    }

                    if (attr.NameColumn != null)
                    {
                        var binding = attr.NameColumn; //(ColumnBinding)(attr.NameColumn.Source);
                        var colId = binding.ColumnID;
                        var tableId = binding.TableID;

                        var sourceElement = _dsvMap[dsvId][tableId][colId];
                        var nameColumnUrn = _urnBuilder.GetNameColumnUrn(binding, attrElement.RefPath);
                        var nameColumnElement = new NameColumnElement(nameColumnUrn, binding.ColumnID, null, attrElement);
                        attrElement.AddChild(nameColumnElement);
                        nameColumnElement.DsvColumn = sourceElement;
                    }

                    if (attr.CustomRollupColumn != null)
                    {
                        var binding = attr.CustomRollupColumn; //(ColumnBinding)(attr.CustomRollupColumn.Source);
                        var colId = binding.ColumnID;
                        var tableId = binding.TableID;

                        var sourceElement = _dsvMap[dsvId][tableId][colId];
                        var customRollupColumnUrn = _urnBuilder.GetCustomRollupColumnUrn(binding, attrElement.RefPath);
                        var customRollupColumnElement = new CustomRollupColumnElement(customRollupColumnUrn, binding.ColumnID, null, attrElement);
                        attrElement.AddChild(customRollupColumnElement);
                        customRollupColumnElement.DsvColumn = sourceElement;
                    }

                    if (attr.CustomRollupPropertiesColumn != null)
                    {
                        var binding = attr.CustomRollupPropertiesColumn; //(ColumnBinding)(attr.CustomRollupPropertiesColumn.Source);
                        var colId = binding.ColumnID;
                        var tableId = binding.TableID;

                        var sourceElement = _dsvMap[dsvId][tableId][colId];
                        var customRollupPropertiesColumnUrn = _urnBuilder.GetCustomRollupPropertiesColumnUrn(binding, attrElement.RefPath);
                        var customRollupPropertiesColumnElement = new CustomRollupPropertiesColumnElement(customRollupPropertiesColumnUrn, binding.ColumnID, null, attrElement);
                        attrElement.AddChild(customRollupPropertiesColumnElement);
                        customRollupPropertiesColumnElement.DsvColumn = sourceElement;
                    }

                    if (attr.UnaryOperatorColumn != null)
                    {
                        var binding = attr.UnaryOperatorColumn; //(ColumnBinding)(attr.UnaryOperatorColumn.Source);
                        var colId = binding.ColumnID;
                        var tableId = binding.TableID;

                        var sourceElement = _dsvMap[dsvId][tableId][colId];
                        var unaryOperatorColumnUrn = _urnBuilder.GetUnaryOperatorColumnUrn(binding, attrElement.RefPath);
                        var unaryOperatorColumnElement = new UnaryOperatorColumnElement(unaryOperatorColumnUrn, binding.ColumnID, null, attrElement);
                        attrElement.AddChild(unaryOperatorColumnElement);
                        unaryOperatorColumnElement.DsvColumn = sourceElement;
                    }
                    
                } // foreach attribute

                foreach (DAL.Objects.Extract.DimensionAttribute attr in dimExtract.Attributes)
                {
                    var attrMap = attributeMap[attr.ID];
                    foreach (string relatedAttributeId in attr.RelatedAttributeIDs)
                    {
                        var relatedAttributeMap = attributeMap[relatedAttributeId];
                        var relatedAttrUrn = _urnBuilder.GetRelatedAttributeUrn(relatedAttributeId, attrMap.ModelElement.RefPath);
                        var relatedAttrElement = new RelatedAttributeElement(relatedAttrUrn, relatedAttributeId, null, attrMap.ModelElement);
                        relatedAttrElement.RelatedAttribute = relatedAttributeMap.ModelElement;
                        attrMap.ModelElement.AddChild(relatedAttrElement);
                    }
                } // foreach attribute
                
                foreach (DimensionHierarchy hierarchy in dimExtract.Hierarchies)
                {
                    var hierarchyUrn = _urnBuilder.GetUrn(hierarchy, dimElement.RefPath);
                    var hierarchyElement = new HierarchyElement(hierarchyUrn, hierarchy.Name, null, dimElement);
                    hierarchyElement.SsasObjectID = hierarchy.ID;
                    dimElement.AddChild(hierarchyElement);

                    var ordinal = 1;
                    foreach (DimensionHierarchyLevel level in hierarchy.Levels)
                    {
                        var levelUrn = _urnBuilder.GetUrn(level, hierarchyElement.RefPath);
                        var levelElement = new HierarchyLevelElement(levelUrn, level.Name, null, hierarchyElement);
                        levelElement.SsasObjectID = level.ID;
                        hierarchyElement.AddChild(levelElement);

                        var sourceAttributeElement = attributeNodeDictionary[level.SourceAttributeID];
                        levelElement.Attribute = sourceAttributeElement;
                        levelElement.Ordinal = ordinal++;
                    }
                }

            } // foreach dimension
        }

        private void AddDataSourceViews(int ssasComponentId, Model.Mssql.Ssas.SsasMultidimensionalDatabaseElement parent)
        {
            var dsvExtracts = _stageManager.GetExtractItems(_extractId, ssasComponentId, ExtractTypeEnum.SsasMultidimensionalDsv).Select(x => (MultidimensionalDsv)x).ToList();

            foreach (MultidimensionalDsv dsvExtract in dsvExtracts)
            {
                var dsvRefPath = _urnBuilder.GetUrn(dsvExtract, parent.RefPath);
                var viewElement = new DatasourceViewElement(dsvRefPath, dsvExtract.Name, null, parent);
                viewElement.SsasObjectID = dsvExtract.ID;
                parent.AddChild(viewElement);
                var tableMap = new DataSourceViewTableMap() { GraphNode = viewElement };
                _dsvMap[dsvExtract.ID] = tableMap;

                var connString = dsvExtract.ConnectionString;
                var relDbName = _sqlExtractor.GetDbNameFromConnectionString(connString);
                var serverName = _sqlExtractor.GetServerNameFromConnectionString(connString, parent.LocalhostServer());
                bool relDbAvailable = relDbName != null;
                Microsoft.SqlServer.TransactSql.ScriptDom.Identifier dbIdentifier = null;
                DatabaseIndex dbIndex = null;
                if (relDbAvailable)
                {
                    dbIdentifier = new Microsoft.SqlServer.TransactSql.ScriptDom.Identifier() { Value = relDbName };
                    _sqlExtractor.ContextServerName = serverName;
                    dbIndex = _availableSqlDatabaseIndex.GetDatabaseIndex(serverName, parent.LocalhostServer());
                    //_sqlContext.SetContextServer(serverName, parent.LocalhostServer());
                    //_sqlContext.ContextServerName = serverName;
                }

                foreach (MultidimensionalDsvTable table in dsvExtract.Tables)
                {
                    var tableRefPath = _urnBuilder.GetUrn(table, viewElement.RefPath);
                    var tableElement = new DatasourceViewTableElement(tableRefPath, table.TableName, null, viewElement);
                    viewElement.AddChild(tableElement);
                    var columnMap = new DataSourceViewColumnMap() { ModelElement = tableElement };
                    tableMap[table.TableName] = columnMap;


                    Dictionary<string, MssqlModelElement> outputColumnsFromNames = new Dictionary<string, MssqlModelElement>(StringComparer.OrdinalIgnoreCase); // null;
                    
                    TableSourceColumnList tableSource = null;

                    if (relDbAvailable)
                    {
                        /* for a normal table
                                ["DbTableName"]: "FactGeneralLedger"
                                ["FriendlyName"]: "FactGeneralLedger"
                                ["TableType"]: "Table"
                                ["DbSchemaName"]: "dbo"
                                ["design-time-name"]: "c62bbd43-2994-4a31-a909-b810a00f48a3"
                        */

                        var tableType = table.TableType; // tableProps["TableType"] as string;
                        var tableName = table.DbTableName; // tableProps["DbTableName"];
                        var schemaName = table.DbSchemaName; // tableProps["DbSchemaName"];

                        if (tableType == "Table" || (tableName != null && schemaName != null))  // a real view (dbobject) can have DbTableNameSet
                        {
                            var identifier = string.Format("[{0}].[{1}]", schemaName, tableName);
                            var srcTableSource = dbIndex.FindTableByObjectName(identifier /* srcTable.Urn */, _sqlExtractor.Parser, dbIdentifier);

                            if (srcTableSource != null)
                            {
                                //var srcTableSource = _settings.DbContext.FindTableByObjectName(srcTable.Urn, _settings.DbSqlScriptExtractor.Parser, dbIdentifier);

                                outputColumnsFromNames = new Dictionary<string, MssqlModelElement>(StringComparer.OrdinalIgnoreCase);
                                foreach (var column in srcTableSource.Columns)
                                {
                                    outputColumnsFromNames.Add(((Identifier)column.Identifier).Value, (MssqlModelElement)(column.ModelElement));
                                }

                            }
                            else
                            {
                                ConfigManager.Log.Warning(string.Format("Could not find DSV source table {0} in {1}, {2} ({3})", 
                                    identifier, dbIdentifier.GetText(), dbIndex.ContextServerName, tableElement.RefPath.Path));
                            }
                        }

                        /*
                         * for a view
                            ["IsLogical"]: "True"
                            ["QueryBuilder"]: "SpecificQueryBuilder"
                            ["Description"]: ""
                            ["design-time-name"]: "766b324c-a99a-4a44-91dc-10836b904610"
                            ["QueryDefinition"]: "SELECT   GeneralLedgerId, GeneralLedgerExtractId, GeneralLedgerSourceId, GeneralLedgerOrderOwnerSourceId, GeneralLedgerDocument_DateId, GeneralLedgerCreated_DateId, \r\n                         GeneralLedgerOrderFirstInvoice_DateId, GeneralLedger_OrganizationId, GeneralLedger_EmployeeId, GeneralLedgerOrderConsultant_EmployeeId, \r\n                         GeneralLedger_AccountId, GeneralLedger_BranchId, GeneralLedger_RegionId, GeneralLedger_BrandId, GeneralLedger_ClientTypeId, GeneralLedger_DocumentId, \r\n                         GeneralLedger_ServiceId, GeneralLedger_ProjectId, GeneralLedger_CampaignId, GeneralLedger_DocumentOriginId, GeneralLedger_OrderId, \r\n                         GeneralLedger_OrderOwnerId, GeneralLedger_VATRateId, GeneralLedgerInvoiceMaturity_DateId, GeneralLedger_InvoicePaymentStatusId, \r\n                         GeneralLedger_OGMSegmentId, GeneralLedger_ReceivedOrderId, GeneralLedgerCredit, GeneralLedgerDebit, GeneralLedgerVATRate, GeneralLedgerCount, \r
        \n                         GeneralLedgerIsDeleted, GeneralLedgerSourceModifiedDate, GeneralLedgerSourceModified_DateId, GeneralLedgerDWModifiedDate, GeneralLedgerDWModified_DateId, \r\n                         GeneralLedgerIsActive, GeneralLedgerOrderOwnerRatio, GeneralLedgerCreditUpToMID, GeneralLedgerDebitUpToMID, GeneralLedgerTurnover, GeneralLedgerGP, \r\n                         GeneralLedgerRevenue, GeneralLedger_VersionId\r\nFROM         FactGeneralLedger"
                            ["FriendlyName"]: "FactGeneralLedgerQuery"
                            ["TableType"]: "View"
                            ["DbTableName"]: "FactGeneralLedgerQuery"
                         */

                        else if (tableType == "View" && (tableName == null || schemaName == null))
                        {
                            var query = table.QueryDefinition; // tableProps["QueryDefinition"] as string;

                            // TODO: get output columns
                            var sqlNode = _sqlExtractor.ExtractScriptModel(query, tableElement, dbIndex, dbIdentifier, out outputColumnsFromNames);
                            tableElement.AddChild(sqlNode);


                        }
                        else
                        {
                            throw new Exception();
                        }

                        // output column names to lowercase


                        // calculated columns
                        List<Db.ReferrableObject> columns = new List<Db.ReferrableObject>();
                        foreach (MultidimensionalDsvColumn column in table.Columns)
                        {
                            if (!string.IsNullOrEmpty(column.ComputedColumnExpression) /* .ExtendedProperties["ComputedColumnExpression"] != null*/)
                            {
                                continue;
                            }
                            if (outputColumnsFromNames.ContainsKey(column.ColumnName))
                            {
                                columns.Add(new Db.ReferrableObject()
                                {
                                    Identifier = new Identifier() { Value = column.ColumnName },
                                    ObjectContent = null,
                                    ReferenceType = Db.ScriptReferenceTypeEnum.Column,
                                    ValidSpan = null,
                                    Urn = outputColumnsFromNames[column.ColumnName].RefPath,
                                    ModelElement = outputColumnsFromNames[column.ColumnName]
                                });
                            }
                            else
                            {
                                columns.Add(new Db.ReferrableObject()
                                {
                                    Identifier = new Identifier() { Value = column.ColumnName },
                                    ObjectContent = null,
                                    ReferenceType = Db.ScriptReferenceTypeEnum.Column,
                                    ValidSpan = null,
                                    ModelElement = null,
                                    Urn = new RefPath()
                                });
                            }
                        }

                        tableSource = new TableSourceColumnList()
                        {
                            Columns = columns,
                            Identifier = new Identifier() { Value = table.TableName },
                            ModelElement = tableElement,
                            ObjectContent = null,
                            ReferenceType = Db.ScriptReferenceTypeEnum.Table,
                            ValidSpan = new ScriptSpan(0, 1000000),
                            Urn = tableElement.RefPath
                        };

                    } // if rel DB available

                    List<MultidimensionalDsvColumn> columnsSortedNoCalcFirst = new List<MultidimensionalDsvColumn>();
                    foreach (MultidimensionalDsvColumn col in table.Columns)
                    {
                        columnsSortedNoCalcFirst.Add(col);
                    }
                    columnsSortedNoCalcFirst = columnsSortedNoCalcFirst.OrderBy(x => !string.IsNullOrEmpty(x.ComputedColumnExpression)).ToList();

                    foreach (MultidimensionalDsvColumn column in columnsSortedNoCalcFirst)
                    {
                        var columnRefPath = _urnBuilder.GetUrn(column, tableElement.RefPath);
                        DatasourceViewColumnElement columnElement = new DatasourceViewColumnElement(columnRefPath, column.ColumnName, null, tableElement);
                        tableElement.AddChild(columnElement);
                        columnMap[column.ColumnName] = columnElement;
                        if (relDbAvailable)
                        {
                            var calculatedExpression = column.ComputedColumnExpression;
                            if (calculatedExpression != null)
                            {
                                var expressionNode = _sqlExtractor.ParseAndResolveOverTableSource(calculatedExpression, tableSource, columnElement);
                                columnElement.AddChild(expressionNode);
                                outputColumnsFromNames.Add(column.ColumnName, expressionNode);
                            }

                            if (outputColumnsFromNames.ContainsKey(column.ColumnName))
                            {
                                var dbColumn = outputColumnsFromNames[column.ColumnName];
                                columnElement.Source = dbColumn;
                            }
                        }
                    }

                } // table
            } // dsv
        }


    }
}

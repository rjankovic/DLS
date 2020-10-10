using CD.DLS.Common.Structures;
using CD.DLS.Common.Tools;
using CD.DLS.DAL.Configuration;
using CD.DLS.DAL.Objects.Extract;
using Microsoft.AnalysisServices;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;

namespace CD.DLS.Extract.Mssql.Ssas
{
    class MultidimensionalExtractor
    {
        private SsasDbProjectComponent _ssasComponent;
        private string _outputDirPath;
        private string _relativePathBase;
        private Manifest _manifest;
        private Server _server;
        private Database _db;

        public MultidimensionalExtractor(SsasDbProjectComponent ssasProjectComponent, string outputDirPath, string relativePathBase, Manifest manifest)
        {
            _ssasComponent = ssasProjectComponent;
            _outputDirPath = outputDirPath;
            _relativePathBase = relativePathBase;
            _manifest = manifest;
        }

        public void Extract()
        {
            _server = new Server();
            _server.Connect(string.Format("Provider=MSOLAP.5;Integrated Security=SSPI;DataSource={0}", _ssasComponent.ServerName));
            _db = _server.Databases[_ssasComponent.DbName];

            AddDatabase();
            AddDataSourceViews();
            AddDimensions();
            AddCubes();

            _server.Dispose();
            _server = null;
        }

        private void AddDatabase()
        {
            MultidimensionalDatabase dbExtract = new MultidimensionalDatabase()
            {
                DbName = _db.Name,
                DbID = _db.ID,
                ServerName = _server.Name,
                ServerID = _server.ID
            };

            var fileSerialized = dbExtract.Serialize();
            var jsonFileName = FileTools.NormalizeFileName("DB_" + dbExtract.Name + ".json");
            File.WriteAllText(Path.Combine(_outputDirPath, jsonFileName), fileSerialized);
            _manifest.Items.Add(new ManifestItem()
            {
                ComponentId = _ssasComponent.SsaslDbProjectComponentId,
                Name = jsonFileName,
                ExtractType = dbExtract.ExtractType.ToString(),
                RelativePath = Path.Combine(_relativePathBase, jsonFileName)
            });
        }

        private void AddDataSourceViews()
        {
            foreach (DataSourceView dsv in _db.DataSourceViews)
            {
                MultidimensionalDsv dsvExtract = new MultidimensionalDsv()
                {
                    ConnectionString = dsv.DataSource.ConnectionString,
                    DsvName = dsv.Name,
                    ID = dsv.ID,
                    Tables = new List<MultidimensionalDsvTable>()
                };

                foreach (DataTable table in dsv.Schema.Tables)
                {
                    var tableProps = table.ExtendedProperties;

                    MultidimensionalDsvTable tableExtract = new MultidimensionalDsvTable()
                    {
                        TableType = tableProps["TableType"] as string,
                        TableName = table.TableName,
                        DbSchemaName = tableProps["DbSchemaName"] as string,
                        DbTableName = tableProps["DbTableName"] as string,
                        QueryDefinition = (!tableProps.ContainsKey("QueryDefinition")) ? null : (tableProps["QueryDefinition"] as string),
                        Columns = new List<MultidimensionalDsvColumn>()
                    };
                    dsvExtract.Tables.Add(tableExtract);

                    foreach (DataColumn column in table.Columns)
                    {
                        tableExtract.Columns.Add(new MultidimensionalDsvColumn()
                        {
                            ColumnName = column.ColumnName,
                            ComputedColumnExpression = column.ExtendedProperties["ComputedColumnExpression"] as string
                        });
                    }
                }

                var fileSerialized = dsvExtract.Serialize();
                var jsonFileName = FileTools.NormalizeFileName("DSV_" + dsvExtract.Name + ".json");
                File.WriteAllText(Path.Combine(_outputDirPath, jsonFileName), fileSerialized);
                _manifest.Items.Add(new ManifestItem()
                {
                    ComponentId = _ssasComponent.SsaslDbProjectComponentId,
                    Name = jsonFileName,
                    ExtractType = dsvExtract.ExtractType.ToString(),
                    RelativePath = Path.Combine(_relativePathBase, jsonFileName)
                });
            }
        }

        private void AddDimensions()
        {
            foreach (Dimension dim in _db.Dimensions)
            {

                MultidimensionalDimension dimExtract = new MultidimensionalDimension()
                {
                    ID = dim.ID,
                    DimensionName = dim.Name,
                    DataSourceViewID = ((DataSourceViewBinding)dim.Source).DataSourceViewID,
                    Attributes = new List<DAL.Objects.Extract.DimensionAttribute>(),
                    Hierarchies = new List<DimensionHierarchy>()
                };
                
                foreach (Microsoft.AnalysisServices.DimensionAttribute attr in dim.Attributes)
                {
                    var attributeExtract = new DAL.Objects.Extract.DimensionAttribute()
                    {
                        ID = attr.ID,
                        KeyColumns = new List<MultidimensionalColumnBinding>(),
                        Name = attr.Name,
                        RelatedAttributeIDs = new List<string>(),
                        AttributeHierarchyEnabled = attr.AttributeHierarchyEnabled
                    };
                    dimExtract.Attributes.Add(attributeExtract);

                    
                    foreach (DataItem keyColumn in attr.KeyColumns)
                    {
                        var binding = (ColumnBinding)(keyColumn.Source);
                        var colId = binding.ColumnID;
                        var tableId = binding.TableID;
                        attributeExtract.KeyColumns.Add(new MultidimensionalColumnBinding()
                        {
                            ColumnID = colId,
                            TableID = tableId
                        });
                    }

                    if (attr.ValueColumn != null)
                    {
                        var binding = (ColumnBinding)(attr.ValueColumn.Source);
                        var colId = binding.ColumnID;
                        var tableId = binding.TableID;
                        attributeExtract.ValueColumn = new MultidimensionalColumnBinding()
                        {
                            ColumnID = colId,
                            TableID = tableId
                        };
                    }

                    if (attr.NameColumn != null)
                    {
                        var binding = (ColumnBinding)(attr.NameColumn.Source);
                        var colId = binding.ColumnID;
                        var tableId = binding.TableID;
                        attributeExtract.NameColumn = new MultidimensionalColumnBinding()
                        {
                            ColumnID = colId,
                            TableID = tableId
                        };
                    }

                    if (attr.CustomRollupColumn != null)
                    {
                        var binding = (ColumnBinding)(attr.CustomRollupColumn.Source);
                        var colId = binding.ColumnID;
                        var tableId = binding.TableID;
                        attributeExtract.CustomRollupColumn = new MultidimensionalColumnBinding()
                        {
                            ColumnID = colId,
                            TableID = tableId
                        };
                    }

                    if (attr.CustomRollupPropertiesColumn != null)
                    {
                        var binding = (ColumnBinding)(attr.CustomRollupPropertiesColumn.Source);
                        var colId = binding.ColumnID;
                        var tableId = binding.TableID;
                        attributeExtract.CustomRollupPropertiesColumn = new MultidimensionalColumnBinding()
                        {
                            ColumnID = colId,
                            TableID = tableId
                        };
                    }

                    if (attr.UnaryOperatorColumn != null)
                    {
                        var binding = (ColumnBinding)(attr.UnaryOperatorColumn.Source);
                        var colId = binding.ColumnID;
                        var tableId = binding.TableID;
                        attributeExtract.UnaryOperatorColumn = new MultidimensionalColumnBinding()
                        {
                            ColumnID = colId,
                            TableID = tableId
                        };
                    }

                    foreach (AttributeRelationship attrRel in attr.AttributeRelationships)
                    {
                        attributeExtract.RelatedAttributeIDs.Add(attrRel.AttributeID);
                    }

                } // foreach attribute
                
                foreach (Hierarchy hierarchy in dim.Hierarchies)
                {
                    var hierarchyExtract = new DimensionHierarchy()
                    {
                        ID = hierarchy.ID,
                        Name = hierarchy.Name,
                        Levels = new List<DimensionHierarchyLevel>()
                    };
                    dimExtract.Hierarchies.Add(hierarchyExtract);
                    
                    foreach (Level level in hierarchy.Levels)
                    {
                        hierarchyExtract.Levels.Add(new DimensionHierarchyLevel()
                        {
                            ID = level.ID,
                            Name = level.ID,
                            SourceAttributeID = level.SourceAttributeID
                        });
                    }
                }

                var fileSerialized = dimExtract.Serialize();
                var jsonFileName = FileTools.NormalizeFileName("Dim_" + dimExtract.Name + ".json");
                File.WriteAllText(Path.Combine(_outputDirPath, jsonFileName), fileSerialized);
                _manifest.Items.Add(new ManifestItem()
                {
                    ComponentId = _ssasComponent.SsaslDbProjectComponentId,
                    Name = jsonFileName,
                    ExtractType = dimExtract.ExtractType.ToString(),
                    RelativePath = Path.Combine(_relativePathBase, jsonFileName)
                });

            } // foreach dimension
        }

        private void AddCubes()
        {
            foreach (Cube cube in _db.Cubes)
            {
                MultidimensionalCube cubeExtract = new MultidimensionalCube()
                {
                    CubeName = cube.Name,
                    ID = cube.ID,
                    DataSourceViewID = cube.DataSourceView.ID,
                    Dimensions = new List<DAL.Objects.Extract.CubeDimension>(),
                    MdxScripts = new List<CubeMdxScript>(),
                    MeasureGroups = new List<CubeMeasureGroup>()
                };

                AddCubeDimensions(cube, cubeExtract);
                AddMeasureGroups(cube, cubeExtract);
                AddMdxScripts(cube, cubeExtract);
                
                var fileSerialized = cubeExtract.Serialize();
                var jsonFileName = FileTools.NormalizeFileName("Cube_" + cubeExtract.Name + ".json");
                File.WriteAllText(Path.Combine(_outputDirPath, jsonFileName), fileSerialized);
                _manifest.Items.Add(new ManifestItem()
                {
                    ComponentId = _ssasComponent.SsaslDbProjectComponentId,
                    Name = jsonFileName,
                    ExtractType = cubeExtract.ExtractType.ToString(),
                    RelativePath = Path.Combine(_relativePathBase, jsonFileName)
                });

            }

        }

        private void AddCubeDimensions(Cube cube, MultidimensionalCube cubeExtract)
        {
            foreach (Microsoft.AnalysisServices.CubeDimension cubedim in cube.Dimensions)
            {
                cubeExtract.Dimensions.Add(new DAL.Objects.Extract.CubeDimension()
                {
                    DimensionID = cubedim.DimensionID,
                    ID = cubedim.ID,
                    Name = cubedim.Name
                });
            }
        }


        private void AddMeasureGroups(Cube cube, MultidimensionalCube cubeExtract)
        {
            foreach (MeasureGroup mg in cube.MeasureGroups)
            {
                CubeMeasureGroup mgExtract = new CubeMeasureGroup()
                {
                    ID = mg.ID,
                    Name = mg.Name,
                    Dimensions = new List<DAL.Objects.Extract.MeasureGroupDimension>(),
                    Measures = new List<PhysicalCubeMeasure>(),
                    Partitions = new List<MeasureGroupPartition>()
                };

                foreach (Partition partition in mg.Partitions)
                {
                    MeasureGroupPartition partitionExtract = new MeasureGroupPartition()
                    {
                        ID = partition.ID,
                        Name = partition.Name
                    };

                    if (partition.Source is DsvTableBinding)
                    {

                        var partitionSrc = (DsvTableBinding)(partition.Source);
                        partitionExtract.BindingType = MeasureGroupPartitionBindingType.DsvTable;
                        partitionExtract.TableID = partitionSrc.TableID;

                    }
                    else if (partition.Source is QueryBinding)
                    {
                        var partitionQuerySrc = (QueryBinding)(partition.Source);
                        partitionExtract.BindingType = MeasureGroupPartitionBindingType.Query;
                        partitionExtract.DataSourceID = partitionQuerySrc.DataSourceID;
                        partitionExtract.ConnectionString = _db.DataSources[partitionQuerySrc.DataSourceID].ConnectionString;
                        partitionExtract.QueryDefinition = partitionQuerySrc.QueryDefinition;
                    }
                    else if (partition.Source is TableBinding)
                    {

                        var tableBinding = (TableBinding)partition.Source;
                        var datasource = _db.DataSources[tableBinding.DataSourceID];
                        var connString = datasource.ConnectionString;
                        partitionExtract.BindingType = MeasureGroupPartitionBindingType.DbTable;
                        partitionExtract.ConnectionString = connString;
                        partitionExtract.TableBoundDbSchemaName = tableBinding.DbSchemaName;
                        partitionExtract.DbTableName = tableBinding.DbTableName;
                    }
                    else
                    {
                        throw new Exception("Unrecognized partition source binding type");
                    }

                    mgExtract.Partitions.Add(partitionExtract);
                }


                HashSet<string> coveredMGDimensions = new HashSet<string>();

                foreach (Microsoft.AnalysisServices.MeasureGroupDimension mgDim in mg.Dimensions)
                {
                    if (!(mgDim is Microsoft.AnalysisServices.RegularMeasureGroupDimension && !(mgDim is Microsoft.AnalysisServices.ReferenceMeasureGroupDimension || mgDim is Microsoft.AnalysisServices.DegenerateMeasureGroupDimension)))
                    {
                        continue;
                    }

                    var mgDimReg = mgDim as Microsoft.AnalysisServices.RegularMeasureGroupDimension;
                    var attrs = mgDimReg.Attributes;
                    MeasureGroupAttribute granularityAttr = null;
                    foreach (MeasureGroupAttribute attr in attrs)
                    {
                        if (attr.Type == MeasureGroupAttributeType.Granularity)
                        {
                            if (granularityAttr != null)
                            {
                                throw new Exception("Multiple granularity attributes ??");
                            }
                            granularityAttr = attr;
                        }
                    }

                    if (granularityAttr == null)
                    {
                        throw new Exception("No granularity attribute ??");
                    }

                    List<MeasureGroupDimensionColumnMapping> columnMapping = new List<MeasureGroupDimensionColumnMapping>();
                    for (int i = 0; i < granularityAttr.KeyColumns.Count; i++)
                    {
                        var mgColumnBinding = (ColumnBinding)(granularityAttr.KeyColumns[i].Source);
                        var dimColumnBinding = (ColumnBinding)(granularityAttr.Attribute.KeyColumns[i].Source);
                        columnMapping.Add(new MeasureGroupDimensionColumnMapping()
                        {
                            PartitionColumnId = mgColumnBinding.ColumnID,
                            DimensionColumnId = dimColumnBinding.ColumnID
                        });
                    }

                    DAL.Objects.Extract.RegularMeasureGroupDimension mgRegularDimExtract = new DAL.Objects.Extract.RegularMeasureGroupDimension()
                    {
                        CubeDimensionID = mgDim.CubeDimensionID,
                        DimensionID = mgDim.Dimension.ID,
                        DimensionName  = mgDim.Dimension.Name,
                        GranularityAttributeID = granularityAttr.AttributeID,
                        DimensionColumnMappings = columnMapping
                    };
                    mgExtract.Dimensions.Add(mgRegularDimExtract);
                    coveredMGDimensions.Add(mgDim.CubeDimensionID);
                }

                // referenced relationships

                foreach (Microsoft.AnalysisServices.MeasureGroupDimension mgDim in mg.Dimensions)
                {
                    if (!(mgDim is Microsoft.AnalysisServices.ReferenceMeasureGroupDimension))
                    {
                        continue;
                    }

                    var mgDimRef = mgDim as Microsoft.AnalysisServices.ReferenceMeasureGroupDimension;
                    mgExtract.Dimensions.Add(new DAL.Objects.Extract.ReferenceMeasureGroupDimension()
                    {
                        CubeDimensionID = mgDimRef.CubeDimensionID,
                        DimensionID = mgDimRef.Dimension.ID,
                        DimensionName = mgDim.Dimension.Name,
                        IntermediateCubeDimensionID = mgDimRef.IntermediateCubeDimensionID,
                        IntermediateGranularityAttributeDimensionID = mgDimRef.IntermediateGranularityAttribute.Attribute.Parent.ID,
                        IntermediateGranularityAttributeID = mgDimRef.IntermediateGranularityAttribute.Attribute.ID /* .IntermediateGranularityAttributeID*/
                    });
                    coveredMGDimensions.Add(mgDim.CubeDimensionID);
                }

                // M : N relationships

                foreach (Microsoft.AnalysisServices.MeasureGroupDimension mgDim in mg.Dimensions)
                {
                    if (!(mgDim is Microsoft.AnalysisServices.ManyToManyMeasureGroupDimension))
                    {
                        continue;
                    }

                    var mgDimMN = mgDim as Microsoft.AnalysisServices.ManyToManyMeasureGroupDimension;
                    mgExtract.Dimensions.Add(new DAL.Objects.Extract.ManyToManyMeasureGroupDimension()
                    {
                        CubeDimensionID = mgDimMN.CubeDimensionID,
                        DimensionID = mgDimMN.Dimension.ID,
                        DimensionName = mgDim.Dimension.Name,
                        MeasureGroupID = mgDimMN.MeasureGroupID
                    });
                    coveredMGDimensions.Add(mgDim.CubeDimensionID);
                }

                // Fact relationshisp (degenerated)
                foreach (Microsoft.AnalysisServices.MeasureGroupDimension mgDim in mg.Dimensions)
                {
                    if (!(mgDim is Microsoft.AnalysisServices.DegenerateMeasureGroupDimension))
                    {
                        continue;
                    }

                    var degen = mgDim as Microsoft.AnalysisServices.DegenerateMeasureGroupDimension;
                    mgExtract.Dimensions.Add(new DAL.Objects.Extract.DegenerateMeasureGroupDimension()
                    {
                        CubeDimensionID = degen.CubeDimensionID,
                        DimensionID = degen.Dimension.ID,
                        DimensionName = mgDim.Dimension.Name
                    });
                    coveredMGDimensions.Add(mgDim.CubeDimensionID);
                }

                // data mining dimensions, whatever that means
                foreach (Microsoft.AnalysisServices.MeasureGroupDimension mgDim in mg.Dimensions)
                {
                    if (!(mgDim is Microsoft.AnalysisServices.DataMiningMeasureGroupDimension))
                    {
                        continue;
                    }
                    var mining = mgDim as Microsoft.AnalysisServices.DataMiningMeasureGroupDimension;
                    mgExtract.Dimensions.Add(new DAL.Objects.Extract.DataMiningMeasureGroupDimension()
                    {
                        CubeDimensionID = mining.CubeDimensionID,
                        DimensionID = mining.Dimension.ID,
                        DimensionName = mgDim.Dimension.Name,
                        CaseCubeDimensionID = mining.CaseCubeDimensionID
                    });
                    coveredMGDimensions.Add(mgDim.CubeDimensionID);
                }


                // default basic handling for special MG dimensions 

                foreach (Microsoft.AnalysisServices.MeasureGroupDimension mgDim in mg.Dimensions)
                {
                    if (coveredMGDimensions.Contains(mgDim.CubeDimensionID))
                    {
                        continue;
                    }
                    throw new Exception("Measure group dimension relationship type not recognized");
                }


                foreach (Measure measure in mg.Measures)
                {
                    var measureExtract = new PhysicalCubeMeasure()
                    {
                        ID = measure.ID,
                        Name = measure.Name,
                        MeasureExpression = measure.MeasureExpression
                    };

                    var src = measure.Source.Source;
                    if (src is ColumnBinding)
                    {
                        var colBinding = src as ColumnBinding;
                        measureExtract.BindingType = PhysicalMeasureBindingType.ColumnBinding;
                        measureExtract.ColumnID = colBinding.ColumnID;
                    }
                    else if (src is RowBinding)
                    {
                        measureExtract.BindingType = PhysicalMeasureBindingType.RowBinding;
                    }
                    else if (src is MeasureBinding)
                    {
                        // TODO
                    }
                    else
                    {
                        throw new Exception();
                    }

                    mgExtract.Measures.Add(measureExtract);

                }

                cubeExtract.MeasureGroups.Add(mgExtract);
            }

        }

        private void AddMdxScripts(Cube cube, MultidimensionalCube cubeExtract)
        {
            foreach (MdxScript mdxScript in cube.MdxScripts)
            {
                if (mdxScript.Commands == null)
                {
                    ConfigManager.Log.Important(string.Format("Warning: no commnads found in MDX script {0}", mdxScript.Name));
                    continue;
                }
                if (mdxScript.Commands.Count != 1)
                {
                    ConfigManager.Log.Important(string.Format("Warning: multiple commnads found in MDX script {0} - skipping the script", mdxScript.Name));
                    continue;
                }

                var command = mdxScript.Commands[0];
                cubeExtract.MdxScripts.Add(new CubeMdxScript()

                {
                    ID = mdxScript.ID,
                    Name = mdxScript.Name,
                    Command = command.Text
                });

            }
        }
    }
}

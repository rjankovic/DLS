using CD.BIDoc.Core.Interfaces;
using CD.BIDoc.Core.Model.Mssql.Tabular;
using CD.BIDoc.Core.Parse.Mssql.Db;
using CD.Framework.Common.Structures;
using CD.Framework.ORM.Managers;
using CD.Framework.ORM.Objects.Extract;
using Microsoft.AnalysisServices;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CD.BIDoc.Core.Parse.Mssql.Ssas;
using CD.BIDoc.Core.Model.Mssql;

namespace CD.BIDoc.Core.Parse.Mssql.Tabular

{
    class TabularParser
    {
        private readonly Ssas.UrnBuilder _urnBuilder = new Ssas.UrnBuilder();
        private ModelSettings _settings;
        //   private Guid _extractRequestId;
        //   private int _configComponentId;
        Dictionary<String, ParsedTabularDatabase> tdatabases;
        public List<Model.Mssql.Ssas.ServerElement> tdatabasesConverted;
        private Server _server;
        private Guid RequestID;



        private ISqlScriptModelExtractor _sqlExtractor;
        private IReferrableIndex _sqlContext;
        public TabularParser(ModelSettings settings, Db.IReferrableIndex sqlContext, Db.ISqlScriptModelExtractor sqlExtractor, Guid MainRequestID)
        {
            _sqlExtractor = sqlExtractor;
            _sqlContext = sqlContext;
            _settings = settings;
            tdatabases = new Dictionary<string, ParsedTabularDatabase>();
            RequestID = MainRequestID;


        }

        public List<Model.Mssql.Ssas.ServerElement> Parse(int ComponentID, String serverName)
        {

            extractJson(RequestID, ComponentID, serverName);
            convertDBs(serverName);
            return tdatabasesConverted;
        }

        public void convertDBs(String serverName)
        {
            _server = new Server();
            _server.Connect(string.Format("Provider=MSOLAP.5;Integrated Security=SSPI;DataSource={0}", serverName));
            tdatabasesConverted = new List<Model.Mssql.Ssas.ServerElement>();

            foreach (KeyValuePair<string, ParsedTabularDatabase> entry in tdatabases)
            {
                var refPath = _urnBuilder.GetUrn(_server);
                Model.Mssql.Ssas.ServerElement se = new Model.Mssql.Ssas.ServerElement(refPath, entry.Key);
                se.Definition = "Tabular";
                se.AddChild(entry.Value);
                tdatabasesConverted.Add(se);


            }


        }

        public TableSourceColumnList getLinkedObject(String serverName, String tableName, String relDbName)
        {
            Microsoft.SqlServer.TransactSql.ScriptDom.Identifier dbIdentifier = new Microsoft.SqlServer.TransactSql.ScriptDom.Identifier() { Value = relDbName };
            String schemaName = "dbo";
            _sqlExtractor.ContextServerName = serverName;
            _sqlContext.ContextServerName = serverName;
            var identifier = string.Format("[{0}].[{1}]", schemaName, tableName);
            return _sqlContext.FindTableByObjectName(identifier, _sqlExtractor.Parser, dbIdentifier);


        }

        public void extractJson(Guid _extractRequestId, int ComponentID, String serverName)
        {



            Debug.WriteLine("Extracting jsons");
            Debug.WriteLine("Extract request ID " + _extractRequestId);
            Debug.WriteLine("Component ID " + ComponentID);
            var databases = StageManager.GetExtractObjects(_extractRequestId, ComponentID, ExtractTypeEnum.TabularDB);
            var models = StageManager.GetExtractObjects(_extractRequestId, ComponentID, ExtractTypeEnum.TabularModel);

            Debug.WriteLine("Pocet najdenych databaz " + databases.Count);
            Debug.WriteLine("Pocet najdenych modelov " + models.Count);



            foreach (var database in databases)
            {
                TabularDB tdb = (TabularDB)database;
                Debug.WriteLine("Parsing database " + database.Name);

                var refPath = _urnBuilder.GetUrnRoot(database.Name);
                ParsedTabularDatabase tdatabase = new ParsedTabularDatabase(refPath, tdb.DBName, tdb.Description);
                tdatabase.collation = tdb.Collation;

                foreach (var annotation in tdb.Annotations)
                {
                    var annotationRefPath = _urnBuilder.GetUrnAnnotation(annotation.Name, refPath);

                    ParsedTabularAnnotation pAnnotation = new ParsedTabularAnnotation(annotationRefPath, annotation.Name, annotation.Value);
                    tdatabase.AddChild(pAnnotation);
                }



                if (!(tdatabases.ContainsKey(tdb.DBName)))
                {
                    tdatabases.Add(tdb.DBName, tdatabase);
                }
            }

            foreach (var model in models)
            {
                TabularModel tmodel = (TabularModel)model;
                var modelRefPath = _urnBuilder.GetUrnModel(tmodel.modelName, tdatabases[tmodel.DBName].RefPath);

                ParsedTabularModel pmodel = new ParsedTabularModel(modelRefPath, tmodel.modelName, tmodel.Description, tdatabases[tmodel.DBName]);

                foreach (var annotation in tmodel.Annotations)
                {
                    var annotationRefPath = _urnBuilder.GetUrnAnnotation(annotation.Name, modelRefPath);
                    ParsedTabularAnnotation pAnnotation = new ParsedTabularAnnotation(annotationRefPath, annotation.Name, annotation.Value,pmodel);
                    pmodel.AddChild(pAnnotation);
                }

                foreach (var dataSource in tmodel.TabularDataSources)
                {
                    Debug.WriteLine("Parsing data source " + dataSource.DSname);
                    var dataSourceRefPath = _urnBuilder.GetUrnDataSource(dataSource.DSname, modelRefPath);
                    ParsedTabularDataSource pDataSource = new ParsedTabularDataSource(dataSourceRefPath, dataSource.DSname, dataSource.Description,pmodel);
                    foreach (var annotation in dataSource.Annotations)
                    {
                        var annotationRefPath = _urnBuilder.GetUrnAnnotation(annotation.Name, dataSourceRefPath);
                        ParsedTabularAnnotation pAnnotation = new ParsedTabularAnnotation(annotationRefPath, annotation.Name, annotation.Value, pDataSource);
                        pDataSource.AddChild(pAnnotation);
                    }
                    pmodel.AddChild(pDataSource);
                }

                foreach (var table in tmodel.TabularTables)
                {
                    Debug.WriteLine("Pocet measures 2 " + table.measures.Count);
                    Debug.WriteLine("Parsing table " + table.Name);
                    var tableRefPath = _urnBuilder.GetUrnTable(table.Name, modelRefPath);
                    ParsedTabularTable pTable = new ParsedTabularTable(tableRefPath, table.Name, "Tabular table",pmodel);
                    Debug.WriteLine("Created table " + pTable.Caption);

                    TableSourceColumnList referencedTableObject = getLinkedObject(serverName, table.Name, tmodel.TabularDataSources[0].DSname);
                    if (referencedTableObject == null)
                    {
                        Debug.WriteLine("Prazdny");
                    }
                    else
                    {
                        Debug.WriteLine("Neprazdny");
                     //   pTable.STable=(MssqlModelElement)referencedTableObject.ModelElement;
                        
                    }

                    foreach (var annotation in table.annotations)
                    {
                        var annotationRefPath = _urnBuilder.GetUrnAnnotation(annotation.Name, tableRefPath);
                        ParsedTabularAnnotation pAnnotation = new ParsedTabularAnnotation(annotationRefPath, annotation.Name, annotation.Value,pTable);
                        pTable.AddChild(pAnnotation);
                    }

                    foreach (var column in table.columns)
                    {
                          if (!(column.Name.StartsWith("RowNumber")))
                          {


                        var columnRefPath = _urnBuilder.GetUrnColumn(column.Name, tableRefPath);
                        ParsedTabularTableColumn pColumn = new ParsedTabularTableColumn(columnRefPath, column.Name, column.DataType, pTable);
                        foreach (var annotation in column.Annotations)
                        {
                            var annotationRefPath = _urnBuilder.GetUrnAnnotation(annotation.Name, columnRefPath);
                            ParsedTabularAnnotation pAnnotation = new ParsedTabularAnnotation(annotationRefPath, annotation.Name, annotation.Value, pTable);
                            pColumn.AddChild(pAnnotation);
                        }

                        var hierarchyRefPath = _urnBuilder.GetUrnHierarchy(column.AttributeHierarchy.name, columnRefPath);
                        ParsedAttributeHierarchy pHierarchy = new ParsedAttributeHierarchy(hierarchyRefPath, column.AttributeHierarchy.name, "Attribute hierarchy", pColumn);
                        foreach (var annotation in column.AttributeHierarchy.Annotations)
                        {
                            var annotationRefPath = _urnBuilder.GetUrnAnnotation(annotation.Name, hierarchyRefPath);
                            ParsedTabularAnnotation pAnnotation = new ParsedTabularAnnotation(annotationRefPath, annotation.Name, annotation.Value, pHierarchy);
                            pHierarchy.AddChild(pAnnotation);
                        }

                        foreach (var clmn in referencedTableObject.Columns)
                        {

                            if (clmn.ModelElement.Caption.Equals(column.Name))
                            {
                                pColumn.SColumn = (MssqlModelElement)clmn.ModelElement;
                                Debug.WriteLine("Column match");
                            }
                        }
                        if (pColumn.SColumn == null)
                        {
                            Debug.WriteLine(pColumn.Caption + " Column NOT FOUND");
                        }

                        pColumn.AddChild(pHierarchy);
                        pTable.AddChild(pColumn);
                    }
                    }

                    foreach (var partition in table.partitions)
                    {
                        var partitionRefPath = _urnBuilder.GetUrnPartition(partition.name, tableRefPath);
                        ParsedPartition pPartition = new ParsedPartition(partitionRefPath, partition.name, partition.PartitionSource, pTable);
                        foreach (var annotation in partition.Annotations)
                        {
                            var annotationRefPath = _urnBuilder.GetUrnAnnotation(annotation.Name, partitionRefPath);
                            ParsedTabularAnnotation pAnnotation = new ParsedTabularAnnotation(annotationRefPath, annotation.Name, annotation.Value, pPartition);
                            pPartition.AddChild(pAnnotation);
                        }

                        pTable.AddChild(pPartition);
                    }

                 
                    foreach (var hierarchy in table.hierarchies)
                    {
                        var hierarchyRefPath = _urnBuilder.GetUrnHierarchy(hierarchy.name, tableRefPath);
                        ParsedHierarchy pHierarchy = new ParsedHierarchy(hierarchyRefPath, hierarchy.name, "Table hierarchy", pTable);

                        foreach (var annotation in hierarchy.Annotations)
                        {
                            var annotationRefPath = _urnBuilder.GetUrnAnnotation(annotation.Name, hierarchyRefPath);
                            ParsedTabularAnnotation pAnnotation = new ParsedTabularAnnotation(annotationRefPath, annotation.Name, annotation.Value, pHierarchy);
                            pHierarchy.AddChild(pAnnotation);
                        }

                        pTable.AddChild(pHierarchy);
                    }

                    // measures povodne
                    foreach (var measure in table.measures)
                    {
                        //      Debug.WriteLine("Measure found");
                        var measureRefPath = _urnBuilder.GetUrnMeasure(measure.Name, tableRefPath);
                        ParsedMeasure pMeasure = new ParsedMeasure(measureRefPath, measure.Name, measure.Expression, pTable);
                             Debug.WriteLine("MEASURE  "+measure.Expression);
                        foreach (var annotation in measure.Annotations)
                        {
                            var annotationRefPath = _urnBuilder.GetUrnAnnotation(annotation.Name, measureRefPath);
                            ParsedTabularAnnotation pAnnotation = new ParsedTabularAnnotation(annotationRefPath, annotation.Name, annotation.Value, pMeasure);
                         
                        }

                      
                        pTable.AddChild(pMeasure);
                    } 

                    pmodel.AddChild(pTable);
                }

                foreach (var relationship in tmodel.relationships)
                {
                    var relationshipRefPath = _urnBuilder.GetUrnRelationship(relationship.fromTable, relationship.toTable, modelRefPath);
                    ParsedRelationship pRelationship = new ParsedRelationship(relationshipRefPath, relationship.Name, relationship.fromTable + "->" + relationship.toTable, pmodel);
                    foreach (var annotation in relationship.Annotations)
                    {
                        var annotationRefPath = _urnBuilder.GetUrnAnnotation(annotation.Name, relationshipRefPath);
                        ParsedTabularAnnotation pAnnotation = new ParsedTabularAnnotation(annotationRefPath, annotation.Name, annotation.Value, pRelationship);
                        pRelationship.AddChild(pAnnotation);
                    }
                    pmodel.AddChild(pRelationship);
                }

                foreach (var perspective in tmodel.Perspectives)
                {
                    var perspectiveRefPath = _urnBuilder.GetUrnPerspective(perspective.Name, modelRefPath);
                    ParsedPerspective pPerspective = new ParsedPerspective(perspectiveRefPath, perspective.Name, "Tabular perspective", pmodel);

                    foreach (var table in perspective.PerspectiveTables)
                    {
                        var tableRefPath = _urnBuilder.GetUrnTable(table.Name, perspectiveRefPath);
                        ParsedTabularTable pTable = new ParsedTabularTable(tableRefPath, table.Name, "Perspective table", pPerspective);
                        TableSourceColumnList referencedTableObject = getLinkedObject(serverName, table.Name, tmodel.TabularDataSources[0].DSname);
                        if (referencedTableObject == null)
                        {
                            Debug.WriteLine("Prazdny");
                        }
                        else
                        {
                            Debug.WriteLine("Neprazdny");
                          

                        }


                        foreach (var column in table.Columns)
                        {
                            var columnRefPath = _urnBuilder.GetUrnColumn(column.Name, tableRefPath);
                            ParsedTabularTableColumn pColumn = new ParsedTabularTableColumn(columnRefPath, column.Name, "Perspective table column", pTable);
                            foreach (var annotation in column.Annotations)
                            {
                                var annotationRefPath = _urnBuilder.GetUrnAnnotation(annotation.Name, columnRefPath);
                                ParsedTabularAnnotation pAnnotation = new ParsedTabularAnnotation(annotationRefPath, annotation.Name, annotation.Value, pColumn);
                                pColumn.AddChild(pAnnotation);
                            }

                            foreach (var clmn in referencedTableObject.Columns)
                            {

                                if (clmn.ModelElement.Caption.Equals(column.Name))
                                {
                                    pColumn.SColumn = (MssqlModelElement)clmn.ModelElement;
                                    Debug.WriteLine("Column match");
                                }
                            }
                            if (pColumn.SColumn == null)
                            {
                                Debug.WriteLine(pColumn.Caption + " Column NOT FOUND");
                            }

                            pTable.AddChild(pColumn);
                        }

                      //  Debug.WriteLine("Pocet measures " + table.Measures.Count);
                  /*      foreach (var measure in table.Measures)
                        {
                      //      Debug.WriteLine("Measure found");
                            var measureRefPath = _urnBuilder.GetUrnMeasure(measure.Name, tableRefPath);
                            ParsedMeasure pMeasure = new ParsedMeasure(measureRefPath, measure.Name, measure.Measure,pTable);
                       //     Debug.WriteLine("MEASURE  "+measure.Measure);
                            foreach (var annotation in measure.Annotations)
                            {
                                var annotationRefPath = _urnBuilder.GetUrnAnnotation(annotation.Name, measureRefPath);
                                ParsedTabularAnnotation pAnnotation = new ParsedTabularAnnotation(annotationRefPath, annotation.Name, annotation.Value, pMeasure);
                                pMeasure.AddChild(pAnnotation);
                            }

                      //      pTable.AddChild(pMeasure);
                        } */

                        foreach (var hierarchy in table.Hierarchies)
                        {
                            var hierarchyRefPath = _urnBuilder.GetUrnHierarchy(hierarchy.Name, tableRefPath);
                            ParsedAttributeHierarchy pHierarchy = new ParsedAttributeHierarchy(hierarchyRefPath, hierarchy.Name, "Perspective table hierarchy", pTable);
                            foreach (var level in hierarchy.HierarchyLevels)
                            {
                                var levelRefPath = _urnBuilder.GetUrnLevel(level.Name, hierarchyRefPath);
                                ParsedHierarchyLevel pLevel = new ParsedHierarchyLevel(levelRefPath, level.Name, "Perspective table hierarchy level", pHierarchy);
                                pHierarchy.AddChild(pLevel);
                            }
                            pTable.AddChild(pHierarchy);

                            foreach (var annotation in hierarchy.Annotations)
                            {
                                var annotationRefPath = _urnBuilder.GetUrnAnnotation(annotation.Name, hierarchyRefPath);
                                ParsedTabularAnnotation pAnnotation = new ParsedTabularAnnotation(annotationRefPath, annotation.Name, annotation.Value, pHierarchy);
                                pHierarchy.AddChild(pAnnotation);
                            }
                        }

                        foreach (var annotation in table.Annotations)
                        {
                            var annotationRefPath = _urnBuilder.GetUrnAnnotation(annotation.Name, tableRefPath);
                            ParsedTabularAnnotation pAnnotation = new ParsedTabularAnnotation(annotationRefPath, annotation.Name, annotation.Value, pTable);
                            pTable.AddChild(pAnnotation);
                        }

                        pPerspective.AddChild(pTable);
                    }

                    pmodel.AddChild(pPerspective);
                }

                Debug.WriteLine("Az sem som dnes zasiel");
                tdatabases[tmodel.DBName].AddChild(pmodel);
            }


        }


    }

}

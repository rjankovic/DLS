using CD.DLS.API;
using CD.DLS.API.ModelUpdate;
using CD.DLS.Common.Structures;
using CD.DLS.DAL.Configuration;
using CD.DLS.DAL.Objects.Extract;
using CD.DLS.Model;
using CD.DLS.Model.DependencyGraph.KnowledgeBase;
using CD.DLS.Model.Interfaces;
using CD.DLS.Model.Mssql;
using CD.DLS.Model.Mssql.Db;
using CD.DLS.Parse.Mssql;
using CD.DLS.Parse.Mssql.Db;
using CD.DLS.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CD.DLS.RequestProcessor.ModelUpdate
{
    public class ParseSqlDatabaseShallowRequestProcessor : RequestProcessorBase, IRequestProcessor<ParseSqlDatabaseShallowRequest, DLSApiMessage>
    {
        public DLSApiMessage Process(ParseSqlDatabaseShallowRequest request, ProjectConfig projectConfig)
        {
            try
            {
                List<ParseSqlDatabaseObjectShallowRequest> objectRequests = new List<ParseSqlDatabaseObjectShallowRequest>();

                var dbComponent = projectConfig.DatabaseComponents.First(x => x.MssqlDbProjectComponentId == request.DbComponentId);

                ConfigManager.Log.Important(string.Format("Parsing MSSQL DB structures {0} from {1}", dbComponent.DbName, dbComponent.ServerName));

                Model.Serialization.SerializationHelper sh = new Model.Serialization.SerializationHelper(projectConfig, GraphManager);
                var solutionModel = sh.LoadElementModelToChildrenOfType(
                    string.Empty,
                    typeof(Model.Mssql.Db.ServerElement)) as SolutionModelElement;

                var premappedModel = sh.CreatePremappedModel(solutionModel);

                var existentServerElement = solutionModel.DbServers.First(x => x.Caption == dbComponent.ServerName);

                var dbElement = new Model.Mssql.Db.DatabaseElement(UrnBuilder.GetDbUrn(existentServerElement.RefPath, dbComponent.DbName), dbComponent.DbName, existentServerElement);
                existentServerElement.AddChild(dbElement);

                // need to save schemas before the parts due to race conditions
                var schemas = StageManager.GetExtractItems(request.ExtractId, request.DbComponentId, DAL.Objects.Extract.ExtractTypeEnum.SqlSchema);
                foreach (DAL.Objects.Extract.SqlSchema schema in schemas)
                {
                    var schemaElement = new SchemaElement(DbModelParserBase.RefPathFor(schema.Urn), schema.Name, dbElement);
                    dbElement.AddChild(schemaElement);
                }

                var procedures = StageManager.GetExtractItems(request.ExtractId, request.DbComponentId,
                                DAL.Objects.Extract.ExtractTypeEnum.SqlProcedure);
                var scalarUdfs = StageManager.GetExtractItems(request.ExtractId, request.DbComponentId,
                                DAL.Objects.Extract.ExtractTypeEnum.SqlScalarUdf);
                var tables = StageManager.GetExtractItems(request.ExtractId, request.DbComponentId,
                                DAL.Objects.Extract.ExtractTypeEnum.SqlTable);
                var tableTypes = StageManager.GetExtractItems(request.ExtractId, request.DbComponentId,
                                DAL.Objects.Extract.ExtractTypeEnum.SqlTableType);
                var tableUdfs = StageManager.GetExtractItems(request.ExtractId, request.DbComponentId,
                                DAL.Objects.Extract.ExtractTypeEnum.SqlTableUdf);
                var views = StageManager.GetExtractItems(request.ExtractId, request.DbComponentId,
                                DAL.Objects.Extract.ExtractTypeEnum.SqlView);

                var extractItems = procedures.Union(scalarUdfs).Union(tables).Union(tableTypes)
                    .Union(tableUdfs).Union(views);

                foreach (SmoObject extractItem in extractItems)
                {
                    //ConfigManager.Log.Important("Parsing " + extractItem.Urn);

                    var schemaElement = dbElement.SchemaByName(extractItem.SchemaName);
                    DbModelElement objectElement = ParseSqlObjectShallow(extractItem, schemaElement);

                    //sh.SaveModelPart(objectElement, premappedModel);

                }


                ReferrableIndexBuilder rib = new ReferrableIndexBuilder();
                var ix = rib.BuildIndex(new List<ServerElement>() { existentServerElement });
                ix.SetContextServer(dbComponent.ServerName);
                ix.PremappedIds = rib.ElementIdMap;


                foreach (SmoObject extractObject in extractItems)
                {
                    ConfigManager.Log.Important(string.Format("Parsing SQL scripts from {0}", extractObject.Urn));


                    var objectRefPath = DbModelParserBase.RefPathFor(extractObject.Urn).Path;

                    var schemaModel = dbElement.SchemaByName(extractObject.SchemaName);

                    var modelElement = schemaModel.Children.First(x => x.RefPath.Path == objectRefPath);

                    LocalDeepModelParser ldmp = new LocalDeepModelParser(projectConfig, dbComponent.ServerName);
                    ldmp.ExtractModel(extractObject, dbComponent.ServerName, ix, modelElement);


                }

                sh.SaveModelPart(dbElement, premappedModel);

                //objectRequests = extractItemIds.Select(x =>
                //new ParseSqlDatabaseObjectShallowRequest() {
                //    ExtractId = request.ExtractId,
                //    ExtractItemId = x.ExtractItemId,
                //    DatabaseRefPath = dbElement.RefPath.Path
                //}).ToList();

                //return new DLSApiProgressResponse()
                //{
                //    //ParallelRequests = objectRequests.Select(x => (DLSApiMessage)x).ToList(),

                //    ContinueWith = new ParseSqlDatabaseDeepRequest()
                //    {
                //        ExtractId = request.ExtractId,
                //        DbComponentId = request.DbComponentId,
                //        DbRefPath = dbElement.RefPath.Path,
                //        DbName = dbElement.DbName,
                //        ServerName = dbElement.Parent.Caption
                //    }
                //};

                return new DLSApiMessage();
            }
            catch (Exception ex1)
            {
                LogException(ex1);
                throw ex1;
            }
        }


        public DbModelElement ParseSqlObjectShallow(SmoObject extractObject, SchemaElement schemaElement)
        {
            //throw new NotImplementedException();
            if (extractObject is SqlTable)
            {
                return ParseSqlObjectShallow((SqlTable)extractObject, schemaElement);
            }
            if (extractObject is SqlTableType)
            {
                return ParseSqlObjectShallow((SqlTableType)extractObject, schemaElement);
            }
            if (extractObject is SqlView)
            {
                return ParseSqlObjectShallow((SqlView)extractObject, schemaElement);
            }
            if (extractObject is SqlScalarUdf)
            {
                return ParseSqlObjectShallow((SqlScalarUdf)extractObject, schemaElement);
            }
            if (extractObject is SqlTableUdf)
            {
                return ParseSqlObjectShallow((SqlTableUdf)extractObject, schemaElement);
            }
            if (extractObject is SqlProcedure)
            {
                return ParseSqlObjectShallow((SqlProcedure)extractObject, schemaElement);
            }

            throw new NotImplementedException();
        }

        public SchemaTableElement ParseSqlObjectShallow(SqlTable schemaTable, SchemaElement schemaElement)
        {
            var tableNode = new SchemaTableElement(DbModelParserBase.RefPathFor(schemaTable.Urn), schemaTable.ObjectName, null, schemaElement);

            foreach (var smoColumn in schemaTable.Columns)
            {
                var columnNode = new ColumnElement(DbModelParserBase.RefPathFor(smoColumn.Urn), smoColumn.ObjectName, null, tableNode);
                tableNode.AddChild(columnNode);

                columnNode.Length = smoColumn.Length;
                columnNode.Scale = smoColumn.Scale;
                columnNode.Precision = smoColumn.Precision;
                columnNode.SqlDataType = smoColumn.SqlDataType;
            }

            schemaElement.AddChild(tableNode);
            return tableNode;
        }

        public UserDefinedTableTypeElement ParseSqlObjectShallow(SqlTableType smoUdtt, SchemaElement schemaElement)
        {
            var tableTypeNode = new UserDefinedTableTypeElement(DbModelParserBase.RefPathFor(smoUdtt.Urn), smoUdtt.ObjectName, null, schemaElement);

            foreach (var smoColumn in smoUdtt.Columns)
            {
                var columnNode = new ColumnElement(DbModelParserBase.RefPathFor(smoColumn.Urn), smoColumn.ObjectName, null, tableTypeNode);
                tableTypeNode.AddChild(columnNode);

                columnNode.Length = smoColumn.Length;
                columnNode.Scale = smoColumn.Scale;
                columnNode.Precision = smoColumn.Precision;
                columnNode.SqlDataType = smoColumn.SqlDataType;
            }

            schemaElement.AddChild(tableTypeNode);
            return tableTypeNode;
        }

        public ViewElement ParseSqlObjectShallow(SqlView smoView, SchemaElement schemaElement)
        {
            var viewNode = new ViewElement(DbModelParserBase.RefPathFor(smoView.Urn), smoView.ObjectName, null, schemaElement);

            for (int i = 0; i < smoView.Columns.Count; i++)
            {
                var smoColumn = smoView.Columns[i];

                var columnNode = new ColumnElement(DbModelParserBase.RefPathFor(smoColumn.Urn), smoColumn.ObjectName, null, viewNode);

                viewNode.AddChild(columnNode);

                columnNode.Length = smoColumn.Length;
                columnNode.Scale = smoColumn.Scale;
                columnNode.Precision = smoColumn.Precision;
                columnNode.SqlDataType = smoColumn.SqlDataType;
            }

            schemaElement.AddChild(viewNode);
            return viewNode;
        }

        public ScalarUdfElement ParseSqlObjectShallow(SqlScalarUdf scalarUdf, SchemaElement schemaElement)
        {
            var udfElement = new ScalarUdfElement(DbModelParserBase.RefPathFor(scalarUdf.Urn), scalarUdf.ObjectName, null, schemaElement);
            schemaElement.AddChild(udfElement);
            return udfElement;
        }

        public TableUdfElement ParseSqlObjectShallow(SqlTableUdf tableUdf, SchemaElement schemaElement)
        {
            var udfElement = new TableUdfElement(DbModelParserBase.RefPathFor(tableUdf.Urn), tableUdf.ObjectName, null, schemaElement);
            schemaElement.AddChild(udfElement);
            return udfElement;
        }

        public ProcedureElement ParseSqlObjectShallow(SqlProcedure smoSp, SchemaElement schemaElement)
        {
            var spNode = new ProcedureElement(DbModelParserBase.RefPathFor(smoSp.Urn), smoSp.ObjectName, null, schemaElement);
            schemaElement.AddChild(spNode);
            return spNode;
        }
    }
}

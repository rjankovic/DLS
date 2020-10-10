using CD.DLS.API;
using CD.DLS.API.ModelUpdate;
using CD.DLS.Common.Structures;
using CD.DLS.DAL.Configuration;
using CD.DLS.DAL.Objects.Extract;
using CD.DLS.Model.Mssql.Db;
using CD.DLS.Model.Serialization;
using CD.DLS.Parse.Mssql.Db;
using System;

namespace CD.DLS.RequestProcessor.ModelUpdate
{
    public class ParseSqlDatabaseObjectShallowRequestProcessor : RequestProcessorBase, IRequestProcessor<ParseSqlDatabaseObjectShallowRequest, DLSApiMessage>
    {
        public DLSApiMessage Process(ParseSqlDatabaseObjectShallowRequest request, ProjectConfig projectConfig)
        {
            var extractObject = (SmoObject)StageManager.GetExtractItem(request.ExtractItemId);
            SerializationHelper sh = new SerializationHelper(projectConfig, GraphManager);
            var dbModel = (DatabaseElement)sh.LoadElementModelToChildrenOfType(request.DatabaseRefPath, typeof(SchemaElement));
            var premappedModel = sh.CreatePremappedModel(dbModel);

            ConfigManager.Log.Important("Parsing " + extractObject.Urn);

            var schemaElement = dbModel.SchemaByName(extractObject.SchemaName);
            DbModelElement objectElement = ParseSqlObjectShallow(extractObject, schemaElement);

            sh.SaveModelPart(objectElement, premappedModel);
            
            return new DLSApiMessage();
        }

        private DbModelElement ParseSqlObjectShallow(SmoObject extractObject, SchemaElement schemaElement)
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

        private SchemaTableElement ParseSqlObjectShallow(SqlTable schemaTable, SchemaElement schemaElement)
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

        private UserDefinedTableTypeElement ParseSqlObjectShallow(SqlTableType smoUdtt, SchemaElement schemaElement)
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

        private ViewElement ParseSqlObjectShallow(SqlView smoView, SchemaElement schemaElement)
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

        private ScalarUdfElement ParseSqlObjectShallow(SqlScalarUdf scalarUdf, SchemaElement schemaElement)
        {
            var udfElement = new ScalarUdfElement(DbModelParserBase.RefPathFor(scalarUdf.Urn), scalarUdf.ObjectName, null, schemaElement);
            schemaElement.AddChild(udfElement);
            return udfElement;
        }

        private TableUdfElement ParseSqlObjectShallow(SqlTableUdf tableUdf, SchemaElement schemaElement)
        {
            var udfElement = new TableUdfElement(DbModelParserBase.RefPathFor(tableUdf.Urn), tableUdf.ObjectName, null, schemaElement);
            schemaElement.AddChild(udfElement);
            return udfElement;
        }

        private ProcedureElement ParseSqlObjectShallow(SqlProcedure smoSp, SchemaElement schemaElement)
        {
            var spNode = new ProcedureElement(DbModelParserBase.RefPathFor(smoSp.Urn), smoSp.ObjectName, null, schemaElement);
            schemaElement.AddChild(spNode);
            return spNode;
        }
    }
}

using CD.DLS.DAL.Objects.Extract;
using CD.DLS.Model.Mssql.Db;
using CD.DLS.Parse.Mssql.Db;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CD.DLS.RequestProcessor.ModelUpdate
{
    class SqlShallowParser
    {
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

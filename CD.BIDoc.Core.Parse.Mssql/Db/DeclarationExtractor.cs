using CD.DLS.Model.Mssql;
using CD.DLS.Model.Mssql.Db;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CD.DLS.Parse.Mssql.Db
{
    /// <summary>
    /// Extract model element declarations from a SQL script.
    /// </summary>
    /// <remarks>
    /// Should not use SMO, just tsql fragments and nodes.
    /// </remarks>
    class DeclarationExtractor
    {
        /// <summary>
        /// Parser used to parse the script.
        /// </summary>
        private readonly TSqlParser _parser;
        /// <summary>
        /// Used to extend the model by the scripts.
        /// </summary>
        private readonly ScriptsModelExtender _extender;
        private readonly UrnBuilder _urnBuilder;

        public DeclarationExtractor(TSqlParser parser, ScriptsModelExtender extender, string serverName)
        {
            this._parser = parser;
            this._extender = extender;
            this._urnBuilder = new UrnBuilder(serverName);
        }


        public void ExtractDatabaseScriptDeclaration(DatabaseElement dbNode, string script)
        {
            using (TextReader sr = new StringReader(script))
            {
                IList<ParseError> errors = new List<ParseError>();
                var parsed = (TSqlScript)(_parser.Parse(sr, out errors));
                if (errors.Count > 0)
                {
                    // for now
                    Console.WriteLine(string.Format("Error extracting \n {0} \n\n {1}", script, errors[0].Message));
                    return;
                }
                if (parsed.Batches.Count > 1)
                {
                    throw new Exception("Too many batches in a sql script");
                }
                var batch = parsed.Batches[0];
                if (batch.Statements.Count > 1)
                {
                    throw new Exception("Too many statements in a sql script");
                }
                var stmt = batch.Statements[0];

                ExtractDatabaseStatementDeclaration(stmt, dbNode, script);

            }
        }

        public void ExtractDatabaseScriptDeclaration(DbModelElement dbObjectNode, string script)
        {
            using (TextReader sr = new StringReader(script))
            {
                IList<ParseError> errors = new List<ParseError>();
                var parsed = (TSqlScript)(_parser.Parse(sr, out errors));
                if (errors.Count > 0)
                {
                    // for now
                    Console.WriteLine(string.Format("Error extracting \n {0} \n\n {1}", script, errors[0].Message));
                    return;
                }
                if (parsed.Batches.Count > 1)
                {
                    throw new Exception("Too many batches in a sql script");
                }
                var batch = parsed.Batches[0];
                if (batch.Statements.Count > 1)
                {
                    throw new Exception("Too many statements in a sql script");
                }
                var stmt = batch.Statements[0];

                ExtractDatabaseObjectStatementDeclaration(stmt, dbObjectNode, script);

            }
        }

        private void ExtractDatabaseObjectStatementDeclaration(TSqlStatement statement, DbModelElement dbObject, string script)
        {
            if (statement is CreateTableStatement)
            {
                var createTable = statement as CreateTableStatement;
                ExtractDatabaseCreateTableStatement(createTable, (SchemaTableElement)dbObject, script);
            }
            else if (statement is AlterTableAddTableElementStatement)
            {
                var addElement = statement as AlterTableAddTableElementStatement;
                ExtractDatabaseAlterTableStatement(addElement, script, (SchemaTableElement)dbObject);
            }

            else if (statement is CreateViewStatement)
            {
                var createView = statement as CreateViewStatement;
                ExtractViewDeclaration(createView, script, (ViewElement)dbObject);
            }
            else if (statement is CreateProcedureStatement)
            {
                var createSp = statement as CreateProcedureStatement;
                ExtractSpDeclaration(createSp, script, (ProcedureElement)dbObject);
            }
            else if (statement is CreateFunctionStatement)
            {
                var createUdf = statement as CreateFunctionStatement;
                ExtractUdfDeclaration(createUdf, script, (UdfElement)dbObject);
            }
            else if (statement is CreateTypeTableStatement)
            {
                var createUdtt = statement as CreateTypeTableStatement;
                ExtractDatabaseCreateUserDefinedTableTypeStatement(createUdtt, (UserDefinedTableTypeElement)dbObject, script);
            }
        }

        private void ExtractDatabaseStatementDeclaration(TSqlStatement statement, DatabaseElement dbNode, string script)
        {
            if (statement is CreateTableStatement)
            {
                var createTable = statement as CreateTableStatement;
                ExtractDatabaseCreateTableStatement(createTable, dbNode, script);
            }
            else if (statement is AlterTableAddTableElementStatement)
            {
                var addElement = statement as AlterTableAddTableElementStatement;
                ExtractDatabaseAlterTableStatement(addElement, script, dbNode);
            }

            else if (statement is CreateViewStatement)
            {
                var createView = statement as CreateViewStatement;
                ExtractViewDeclaration(createView, script, dbNode);
            }
            else if (statement is CreateProcedureStatement)
            {
                var createSp = statement as CreateProcedureStatement;
                ExtractSpDeclaration(createSp, script, dbNode);
            }
            else if (statement is CreateFunctionStatement)
            {
                var createUdf = statement as CreateFunctionStatement;
                ExtractUdfDeclaration(createUdf, script, dbNode);
            }
            else if (statement is CreateTypeTableStatement)
            {
                var createUdtt = statement as CreateTypeTableStatement;
                ExtractDatabaseCreateUserDefinedTableTypeStatement(createUdtt, dbNode, script);
            }
        }
        private void ExtractDatabaseCreateTableStatement(CreateTableStatement createTable, DatabaseElement dbNode, string script)
        {

            var content = script; //GetSqlFragmentFromScript(script, stmt);
            var tableName = createTable.SchemaObjectName.BaseIdentifier.Value;
            //var tableSchemaName = createTable.SchemaObjectName.SchemaIdentifier == null ? "dbo" : (createTable.SchemaObjectName.SchemaIdentifier.Value);
            var schemaName = createTable.SchemaObjectName.SchemaIdentifier.Value;
            //var tableUrn = _urnBuilder.GetUrnOfTable(dbNode.DbName, schem, tableName);
            //Table smoTable = (Table)_dbIndex.FindObjectByUrn(tableUrn);

            var tableNode = dbNode.SchemaByName(schemaName).TableByName(tableName);
            ExtractDatabaseCreateTableStatement(createTable, tableNode, script);
        }


        private void ExtractDatabaseCreateTableStatement(CreateTableStatement createTable, SchemaTableElement tableNode, string script)
        {
            tableNode.Definition = script;


            int nodeCounter = 0;
            var scriptElement = ScriptModelParser.CreateElementForFragment(createTable, tableNode, true, ref nodeCounter, new ReferenceResolver());
            tableNode.SqlDefinition = scriptElement;
            tableNode.AddChild(scriptElement);


            //dg.Nodes.Add(tableNode);
            //tableNodes.Add(smoTable.Urn.Value, tableNode);
            foreach (ColumnDefinition column in createTable.Definition.ColumnDefinitions)
            {
                var columnName = column.ColumnIdentifier.Value;

                var columnElement = tableNode.GetColumnByName(columnName);
                if (columnElement == null)
                {
                    Console.WriteLine(string.Format("Could not find column {0} in table {1}, {2}", columnName, tableNode.RefPath, createTable.Definition.GetText()));
                    return;
                }

                string columnDefinition = GetSqlFragmentFromScript(script, column);
                /*var definitionStartOffset = column.StartOffset;
                var definitionLength = column.FragmentLength;*/

                columnElement.Definition = columnDefinition;
                var columnScriptElement = ScriptModelParser.CreateElementForFragment(column, scriptElement, false, ref nodeCounter, new ReferenceResolver());
                scriptElement.AddChild(columnScriptElement);
                columnElement.SqlDefinition = columnScriptElement;

                /*tableNode.DefinitionSegments.Add(new DefinitionSegment
                {
                    DefinedElement = columnElement,
                    Length = definitionLength,
                    PositionFrom = definitionStartOffset
                });*/

                // TODO: inbound links when neccessary
            }

        }

        private void ExtractDatabaseCreateUserDefinedTableTypeStatement(CreateTypeTableStatement createTableType, DatabaseElement dbNode, string script)
        {
            var content = script;
            var typeName = createTableType.Name.BaseIdentifier.Value;
            var schemaName = createTableType.Name.SchemaIdentifier.Value;
            
            var ttNode = dbNode.SchemaByName(schemaName).TableTypeByName(typeName);
            ExtractDatabaseCreateUserDefinedTableTypeStatement(createTableType, ttNode, script);
        }

        private void ExtractDatabaseCreateUserDefinedTableTypeStatement(CreateTypeTableStatement createTableType, UserDefinedTableTypeElement ttNode, string script)
        {
            int nodeCounter = 0;
            var scriptElement = ScriptModelParser.CreateElementForFragment(createTableType, ttNode, true, ref nodeCounter, new ReferenceResolver());
            ttNode.SqlDefinition = scriptElement;
            ttNode.Definition = script;
            ttNode.AddChild(scriptElement);

            foreach (ColumnDefinition column in createTableType.Definition.ColumnDefinitions)
            {
                var columnName = column.ColumnIdentifier.Value;
                var columnElement = ttNode.GetColumnByName(columnName);

                string columnDefinition = GetSqlFragmentFromScript(script, column);
                columnElement.Definition = columnDefinition;
                var columnScriptElement = ScriptModelParser.CreateElementForFragment(column, scriptElement, false, ref nodeCounter, new ReferenceResolver());
                scriptElement.AddChild(columnScriptElement);
                columnElement.SqlDefinition = columnScriptElement;

            }

        }

        private string GetSqlFragmentFromScript(string script, TSqlFragment fragment)
        {
            return script.Substring(fragment.StartOffset, fragment.FragmentLength);
        }



        private void ExtractDatabaseAlterTableStatement(AlterTableAddTableElementStatement addElement, string script, DatabaseElement dbObject)
        {
   
            foreach (ConstraintDefinition constraintDef in addElement.Definition.TableConstraints)
            {
                if (constraintDef is ForeignKeyConstraintDefinition)
                {
                    var fkDef = constraintDef as ForeignKeyConstraintDefinition;
                    if (fkDef.ConstraintIdentifier != null)
                    {
                        var name = fkDef.ConstraintIdentifier.Value;
                        var content = script; //GetSqlFragmentFromScript(script, fkDef);
                        var tableName = addElement.SchemaObjectName.BaseIdentifier.Value;
                        var schemaName = addElement.SchemaObjectName.SchemaIdentifier.Value;
                        //var tableUrn = urnBuilder.GetUrnOfTable(dbName, schemaName, tableName);
                        //Table smoTable = (Table)_dbIndex.FindObjectByUrn(tableUrn);
                        SchemaTableElement tableNode = dbObject.SchemaByName(schemaName).TableByName(tableName);// (Table)(objectsDictionary[tableUrn]);
                                                                                                                //MssqlModelElement tableNode = (dg.Nodes.FirstOrDefault(node => node.RefPath == smoTable.Urn) as MssqlModelElement);
                        ForeignKeyElement fkElement = tableNode.GetForeignKeyByName(name);

                        // Set the definition of the foreign key
                        fkElement.Definition = content;
                    }
                }
            }
        }

        private void ExtractDatabaseAlterTableStatement(AlterTableAddTableElementStatement addElement, string script, SchemaTableElement tableNode)
        {

            foreach (ConstraintDefinition constraintDef in addElement.Definition.TableConstraints)
            {
                if (constraintDef is ForeignKeyConstraintDefinition)
                {
                    var fkDef = constraintDef as ForeignKeyConstraintDefinition;
                    if (fkDef.ConstraintIdentifier != null)
                    {
                        var name = fkDef.ConstraintIdentifier.Value;
                        var content = script; //GetSqlFragmentFromScript(script, fkDef);
                                                                                                                //MssqlModelElement tableNode = (dg.Nodes.FirstOrDefault(node => node.RefPath == smoTable.Urn) as MssqlModelElement);
                        ForeignKeyElement fkElement = tableNode.GetForeignKeyByName(name);

                        // Set the definition of the foreign key
                        if (fkElement != null)
                        {
                            fkElement.Definition = content;
                        }
                    }
                }
            }
        }

        private string GetSchemaObjectNameSchema(SchemaObjectName name)
        {
            var schi = name.SchemaIdentifier;
            string schema = "dbo";
            if (schi != null)
            {
                schema = schi.Value;
            }
            return schema;
        }

        private void ExtractUdfDeclaration(CreateFunctionStatement createUdf, string script, DatabaseElement dbObject)
        {
            var name = createUdf.Name;
            string schemaName = GetSchemaObjectNameSchema(name);
            //var udfUrn = _urnBuilder.GetUrnOfUdf(dbName, GetSchemaObjectNameSchema(name), name.BaseIdentifier.Value);
            //UserDefinedFunction smoUdf = (UserDefinedFunction)_dbIndex.FindObjectByUrn(udfUrn);//(objectsDictionary[udfUrn]);
            //var schemaNode = _dbIndex.FindSchemaNode(schema);

            var udfNode = dbObject.SchemaByName(schemaName).UdfByName(name.BaseIdentifier.Value);
            ExtractUdfDeclaration(createUdf, script, udfNode);
            
        }

        private void ExtractUdfDeclaration(CreateFunctionStatement createUdf, string script, UdfElement udfNode)
        {
            udfNode.Definition = script;
            //dg.Nodes.Add(udfNode);
            _extender.Add(createUdf, udfNode);

        }

        private void ExtractSpDeclaration(CreateProcedureStatement createSp, string script, DatabaseElement dbObject)
        {
            var name = createSp.ProcedureReference.Name.BaseIdentifier.Value;
            string schemaName = GetSchemaObjectNameSchema(createSp.ProcedureReference.Name);

            //var spUrn = _urnBuilder.GetUrnOfSp(dbName, schemaName, name);
            //StoredProcedure smoSp = (StoredProcedure)_dbIndex.FindObjectByUrn(spUrn);
            ProcedureElement spNode = dbObject.SchemaByName(schemaName).ProcedureByName(name);
            ExtractSpDeclaration(createSp, script, spNode);
        }

        private void ExtractSpDeclaration(CreateProcedureStatement createSp, string script, ProcedureElement spNode)
        {
            spNode.Definition = script;
            _extender.Add(createSp, spNode);
        }

        private void ExtractViewDeclaration(CreateViewStatement createView, string script, DatabaseElement dbObject)
        {
            var name = createView.SchemaObjectName.BaseIdentifier.Value;
            //var viewSchemaName = createView.SchemaObjectName.SchemaIdentifier == null ? "dbo" : (createView.SchemaObjectName.SchemaIdentifier.Value);
            //var viewUrnSuffix = String.Format("View[@Name='{0}' and @Schema='{1}']", name, viewSchemaName);
            string schemaName = GetSchemaObjectNameSchema(createView.SchemaObjectName);
            //var viewUrn = _urnBuilder.GetUrnOfView(dbObject.DbName, schemaName, name);
            
           // var schemaNode = dbObject.
            ViewElement viewNode = dbObject.SchemaByName(schemaName).ViewByName(name);
            ExtractViewDeclaration(createView, script, viewNode);
        }

        private void ExtractViewDeclaration(CreateViewStatement createView, string script, ViewElement viewNode)
        {

            viewNode.Definition = script;

            int nodeCounter = 100;
            var scriptElement = ScriptModelParser.CreateElementForFragment(createView, viewNode, true, ref nodeCounter, new ReferenceResolver());
            viewNode.SqlDefinition = scriptElement;
            viewNode.AddChild(scriptElement);


            _extender.Add(createView, viewNode);

            var querySpec = createView.SelectStatement.QueryExpression as QuerySpecification;
            ColumnElement[] columns = viewNode.Columns.ToArray();
            for (int i = 0; i < columns.Length; i++)
            {
                var columnNode = columns[i];
                //var smoColumn = viewNode.Columns[i];
                // CREATE VIEW v AS SELECT * FROM t
                var isStarSelect = (querySpec == null || querySpec.SelectElements.Any(c => c is SelectStarExpression));
                var selectElementExpression = isStarSelect ? (columnNode.Caption) : (GetSqlFragmentFromScript(script, querySpec.SelectElements[i]));
                /*
                if (!selectElementExpression.Contains(columnNode.Caption))
                {
                    // this can happen with SELECT x.* in a view
                    throw new Exception();
                }
                */
                if (!isStarSelect)
                {
                    var columnScriptElement = ScriptModelParser.CreateElementForFragment(querySpec.SelectElements[i], columnNode, false, ref nodeCounter, new ReferenceResolver());
                    scriptElement.AddChild(columnScriptElement);
                    columnNode.SqlDefinition = columnScriptElement;
                }

                columnNode.Definition = selectElementExpression;
                //dg.Nodes.Add(columnNode);
                /*if (!isStarSelect)
                {
                    var definitionStartOffset = querySpec.SelectElements[i].StartOffset;
                    var definitionLength = querySpec.SelectElements[i].FragmentLength;
                    viewNode.DefinitionSegments.Add(new DefinitionSegment
                    {
                        DefinedElement = columnNode,
                        Length = definitionLength,
                        PositionFrom = definitionStartOffset
                    });
                }*/
            }
        }
    }
}

using CD.DLS.Model.Mssql;
using CD.DLS.Model.Mssql.Db;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using System.Collections.Generic;

namespace CD.DLS.Parse.Mssql.Db
{
    /// <summary>
    /// Builds an index of referrables from a server model.
    /// </summary>
    public class ReferrableIndexBuilder
    {
        private Dictionary<MssqlModelElement, int> _elementIdMap = new Dictionary<MssqlModelElement, int>();

        public Dictionary<MssqlModelElement, int> ElementIdMap { get { return _elementIdMap; } }
        
        public DatabaseIndex BuildIndex(List<ServerElement> serverElements)
        {
            DatabaseIndex referrableIndex = new DatabaseIndex();
            foreach (var serverElement in serverElements)
            {
                AddServerToIndex(referrableIndex, serverElement);
            }
            return referrableIndex;
        }

        private void AddToIdMap(MssqlModelElement element)
        {
            _elementIdMap[element] = element.Id;
        }

        private void AddServerToIndex(DatabaseIndex referrableIndex, ServerElement serverElement)
        {
            referrableIndex.SetContextServer(serverElement.Caption);
            //referrableIndex.ContextServerName = serverElement.Caption;
            foreach (var dbIndex in serverElement.Databases)
            {
                AddDatabaseToIndex(referrableIndex, dbIndex);
            }
        }


        public DatabaseIndex BuildIndex(List<DatabaseElement> databaseElements, string serverName)
        {
            DatabaseIndex referrableIndex = new DatabaseIndex();
            referrableIndex.SetContextServer(serverName);
            //referrableIndex.ContextServerName = serverElement.Caption;
            foreach (var dbIndex in databaseElements)
            {
                AddDatabaseToIndex(referrableIndex, dbIndex);
            }
            return referrableIndex;
        }

        private void AddDatabaseToIndex(DatabaseIndex referrableIndex, DatabaseElement dbElement) {
            AddDatabaseTablesToIndex(referrableIndex, dbElement);
            AddDatabaseViewsToIndex(referrableIndex, dbElement);
            AddDatabaseUdfsToIndex(referrableIndex, dbElement);
            AddDatabaseStoredProceduresToIndex(referrableIndex, dbElement);
            AddDatabaseTableTypesToIndex(referrableIndex, dbElement);
        }
        private void AddDatabaseTablesToIndex(DatabaseIndex index, DatabaseElement db)
        {
            foreach (SchemaTableElement table in db.Tables)
            {
                var identifier = BuildIdentifier(db.Caption, table.Parent.Caption, table.Caption);
                List<ReferrableObject> columns = new List<ReferrableObject>();
                AddToIdMap(table);
                foreach (ColumnElement column in table.Columns)
                {
                    columns.Add(new ReferrableObject()
                    {
                        Identifier = new Identifier() { Value = column.Caption },
                        ObjectContent = null,
                        ReferenceType = ScriptReferenceTypeEnum.Column,
                        ValidSpan = null,
                        Urn = column.RefPath,
                        ModelElement = column
                    });
                    AddToIdMap(column);
                }
                index.AddTableSource(new TableSourceColumnList()
                {
                    Columns = columns,
                    Identifier = identifier,
                    ObjectContent = null,
                    ReferenceType = ScriptReferenceTypeEnum.Table,
                    ValidSpan = ScriptSpan.DbWide,
                    ModelElement = table,
                    Urn = table.RefPath
                });
            }
        }
        private void AddDatabaseViewsToIndex(DatabaseIndex index, DatabaseElement db)
        {
            foreach (ViewElement view in db.Views)
            {
                AddToIdMap(view);
                var identifier = BuildIdentifier(db.Caption, view.Parent.Caption, view.Caption);
                List<ReferrableObject> columns = new List<ReferrableObject>();
                foreach (ColumnElement column in view.Columns)
                {
                    AddToIdMap(column);
                    columns.Add(new ReferrableObject()
                    {
                        Identifier = new Identifier() { Value = column.Caption },
                        ObjectContent = null,
                        ReferenceType = ScriptReferenceTypeEnum.Column,
                        ValidSpan = null,
                        Urn = column.RefPath,
                        ModelElement = column
                    });
                }
                index.AddTableSource(new TableSourceColumnList()
                {
                    Columns = columns,
                    Identifier = identifier,
                    ObjectContent = null,
                    ReferenceType = ScriptReferenceTypeEnum.Table,
                    ValidSpan = ScriptSpan.DbWide,
                    Urn = view.RefPath,
                    ModelElement = view
                });
            }
        }
        private void AddDatabaseUdfsToIndex(DatabaseIndex index, DatabaseElement db)
        {
            foreach (UdfElement function in db.ScalarUdfs)
            {
                AddToIdMap(function);
                var identifier = BuildIdentifier(db.Caption, function.Parent.Caption, function.Caption);

                
                    index.ScalarUdfsAvailable.Add(new ReferrableObject
                    {
                        Identifier = identifier,
                        ObjectContent = null,
                        ReferenceType = ScriptReferenceTypeEnum.Function,
                        ValidSpan = ScriptSpan.DbWide,
                        Urn = function.RefPath,
                        ModelElement = function
                    });
                    continue;
                
            }
            foreach (UdfElement function in db.NonScalarUdfs)
            {
                var identifier = BuildIdentifier(db.Caption, function.Parent.Caption, function.Caption);
                AddToIdMap(function);
                List<ReferrableObject> columns = new List<ReferrableObject>();
                foreach (ColumnElement column in function.Columns)
                {
                    AddToIdMap(column);
                    columns.Add(new ReferrableObject()
                    {
                        Identifier = new Identifier() { Value = column.Caption },
                        ObjectContent = null,
                        ReferenceType = ScriptReferenceTypeEnum.Column,
                        ValidSpan = null,
                        Urn = column.RefPath,
                        ModelElement = column
                    });
                }
                index.AddTableSource(new TableSourceColumnList()
                {
                    Columns = columns,
                    Identifier = identifier,
                    ObjectContent = null,
                    ReferenceType = ScriptReferenceTypeEnum.Function,
                    ValidSpan = ScriptSpan.DbWide,
                    Urn = function.RefPath,
                    ModelElement = function
                });
            }
        }
        private void AddDatabaseStoredProceduresToIndex(DatabaseIndex index, DatabaseElement db)
        {
            foreach (ProcedureElement sp in db.StoredProcedures)
            {
                AddToIdMap(sp);
                foreach (var col in sp.OutputColumns)
                {
                    AddToIdMap(col.Value);
                }
                var identifier = BuildIdentifier(db.Caption, sp.Parent.Caption, sp.Caption);

                index.StoredProceduresAvailable.Add(new ReferrableObject
                {
                    Identifier = identifier,
                    ObjectContent = null,
                    ReferenceType = ScriptReferenceTypeEnum.StoredProcedure,
                    ValidSpan = ScriptSpan.DbWide,
                    Urn = sp.RefPath,
                    ModelElement = sp
                });

            }
        }

        private void AddDatabaseTableTypesToIndex(DatabaseIndex index, DatabaseElement db)
        {
            foreach (UserDefinedTableTypeElement tableType in db.TableTypes)
            {
                AddToIdMap(tableType);
                var identifier = BuildIdentifier(db.Caption, tableType.Parent.Caption, tableType.Caption);
                List<ReferrableObject> columns = new List<ReferrableObject>();
                foreach (ColumnElement column in tableType.Columns)
                {
                    AddToIdMap(column);
                    columns.Add(new ReferrableObject()
                    {
                        Identifier = new Identifier() { Value = column.Caption },
                        ObjectContent = null,
                        ReferenceType = ScriptReferenceTypeEnum.Column,
                        ValidSpan = null,
                        Urn = column.RefPath,
                        ModelElement = column
                    });
                }
                index.AddTableType(new TableSourceColumnList()
                {
                    Columns = columns,
                    Identifier = identifier,
                    ObjectContent = null,
                    ReferenceType = ScriptReferenceTypeEnum.TableType,
                    ValidSpan = ScriptSpan.DbWide,
                    ModelElement = tableType,
                    Urn = tableType.RefPath
                });
            }
        }

        private static MultiPartIdentifier BuildIdentifier(params string[] parts)
        {
            var identifier = new MultiPartIdentifier();
            foreach(string part in parts)
                identifier.Identifiers.Add(new Identifier() { Value = part });
            return identifier;
        }

        /// <summary>
        /// For resolving over an explicit set of tables - e.g. SSAS DSV tables can have computed columns that can only reference other columns from that table.
        /// </summary>
        /// <param name="tempTableSources"></param>
        /// <returns></returns>
        public DatabaseIndex CreateExplicitDatabaseIndex(List<TableSourceColumnList> tempTableSources)
        {
            DatabaseIndex referrableIndex = new DatabaseIndex("temp");
            foreach(var tempTableSource in tempTableSources)
            {
                referrableIndex.AddTableSource(tempTableSource);
            }
            return referrableIndex;
        }
        
    }
}

using CD.DLS.Common.Structures;
using CD.DLS.Common.Tools;
using CD.DLS.DAL.Configuration;
using CD.DLS.DAL.Objects.Extract;
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System;

namespace CD.DLS.Extract_v12.Mssql.SqlDb
{
    public class SqlExtractor_v12
    {
        private MssqlDbProjectComponent _dbComponent;
        private List<ExtractObject> _extractObjects;
        //private Dictionary<string, SqlSchema> _schemaDictionary = new Dictionary<string, SqlSchema>();
        //private SqlDbStructure _dbStructure = null;
        private HashSet<string> _coveredSchemas = new HashSet<string>();
        
        private string _outputDirPath;
        private string _relativePathBase;
        private Manifest _manifest;
        private Server _server;
        
        public SqlExtractor_v12(MssqlDbProjectComponent dbProjectComponent, string outputDirPath, string relativePathBase, Manifest manifest)
        {
            _dbComponent = dbProjectComponent;
            _outputDirPath = outputDirPath;
            _relativePathBase = relativePathBase;
            _manifest = manifest;
        }

        public void Extract()
        {

            ConfigManager.Log.Important(string.Format("Extracting structures - MSSQL DB {0} from {1}", _dbComponent.DbName, _dbComponent.ServerName));

            var dbDirName = $"DB_{_dbComponent.MssqlDbProjectComponentId}_{_dbComponent.DbName}";
            dbDirName = FileTools.NormalizeFileName(dbDirName);

            _outputDirPath = Path.Combine(_outputDirPath, dbDirName);
            _relativePathBase = Path.Combine(_relativePathBase, dbDirName);

            Directory.CreateDirectory(_outputDirPath);

            ServerConnection connection = new ServerConnection(_dbComponent.ServerName);
            var server = new Server(connection);
            _server = server;
            
            var smoDb = server.Databases[_dbComponent.DbName];

            _extractObjects = new List<ExtractObject>();
            //_schemaDictionary = new Dictionary<string, SqlSchema>();

            //_dbStructure = new SqlDbStructure()
            //{
            //    DbName = _dbComponent.DbName,
            //    ServerName = _dbComponent.ServerName,
            //    ServerUrn = server.Urn.Value,
            //    DbUrn = smoDb.Urn.Value
            //};

            //ExtractDatabaseSchemas(smoDb);
            List<SmoObject> objects = new List<SmoObject>();

            objects.AddRange(ExtractDatabaseTables(smoDb));
            objects.AddRange(ExtractDatabaseViews(smoDb));
            objects.AddRange(ExtractDatabaseUdfs(smoDb));
            objects.AddRange(ExtractDatabaseStoredProcedures(smoDb));
            objects.AddRange(ExtractDatabaseTableTypes(smoDb));

            var schemaNames = objects.Select(x => x.SchemaName).Distinct();
            foreach (Schema schema in smoDb.Schemas)
            {
                if (schemaNames.Contains(schema.Name))
                {
                    objects.Add(new SqlSchema()
                    {
                        ObjectName = schema.Name,
                        SchemaName = schema.Name,
                        Urn = schema.Urn.Value
                    });
                }
            }
            
            int i = 0;
            foreach (var obj in objects)
            {
                var structuresSerialized = obj.Serialize();
                var structureFileName = FileTools.NormalizeFileName(obj.Name) + "_" + (i++).ToString() + ".json";
                File.WriteAllText(Path.Combine(_outputDirPath, structureFileName), structuresSerialized);
                _manifest.Items.Add(new ManifestItem()
                {
                    ComponentId = _dbComponent.MssqlDbProjectComponentId,
                    Name = structureFileName,
                    ExtractType = obj.ExtractType.ToString(),
                    RelativePath = Path.Combine(_relativePathBase, structureFileName)
                });
            }

            //var structuresSerialized = _dbStructure.Serialize();
            //var structureFileName = "structure.json";
            //File.WriteAllText(Path.Combine(_outputDirPath, structureFileName), structuresSerialized);
            //_manifest.Items.Add(new ManifestItem()
            //{
            //    ComponentId = _dbComponent.MssqlDbProjectComponentId,
            //    Name = structureFileName,
            //    ExtractType = _dbStructure.ExtractType.ToString(),
            //    RelativePath = Path.Combine(_relativePathBase, structureFileName)
            //});

            //ConfigManager.Log.Important(string.Format("Extracting SQL scripts - MSSQL DB {0} from {1}", _dbComponent.DbName, _dbComponent.ServerName));

            //ExtractDatabaseScripts(smoDb);
        }
        
        //private void ExtractDatabaseSchemas(Database smoDb)
        //{
        //    _dbStructure.Schemas = new List<SqlSchema>();

        //    _coveredSchemas = new HashSet<string>();
        //    _dbStructure.Schemas = new List<SqlSchema>();
        //    foreach (Schema smoSchema in smoDb.Schemas)
        //    {
        //        var isSysobject = smoSchema.IsSystemObject;
        //        if (isSysobject && smoSchema.Name != "dbo")
        //        {
        //            continue;
        //        }

        //        var schemaExtract = new SqlSchema() {
        //            ObjectName = smoSchema.Name,
        //            Urn = smoSchema.Urn.Value,
        //         ScalarUdfs = new List<SqlScalarUdf>(),
        //         Tables = new List<SqlTable>(),
        //         TableTypes = new List<SqlTableType>(),
        //         TableUdfs = new List<SqlTableUdf>(),
        //         Views = new List<SqlView>(),
        //         Procedures = new List<SqlProcedure>()
        //        };
        //        _coveredSchemas.Add(smoSchema.Name);
        //        _schemaDictionary.Add(smoSchema.Name, schemaExtract);
        //        _dbStructure.Schemas.Add(schemaExtract);
        //    }
            
        //}

        private List<SmoObject> ExtractDatabaseTables(Database smoDb)
        {
            List<SmoObject> res = new List<SmoObject>();

            foreach (Table smoTable in smoDb.Tables)
            {
                ConfigManager.Log.Important(smoTable.Urn.Value);

                if (IsSystemSchemaObject(smoTable))
                {
                    continue;
                }

                var schemaName = smoTable.Schema;
                //var dbName = smoTable.Parent.Name;
                //var serverName = smoTable.Parent.Parent.Name;
                var tableExtract = new SqlTable() {
                    ObjectName = smoTable.Name,
                    Urn = smoTable.Urn.Value,
                    Columns = new List<SqlColumn>(),
                    SchemaName = schemaName
                };
                //schema.Tables.Add(tableExtract);

                foreach (Column smoColumn in smoTable.Columns)
                {
                    tableExtract.Columns.Add(ExtractColumn(smoColumn));
                }

                tableExtract.ForeignKeys = new List<SqlForeignKey>();
                foreach (ForeignKey smoFk in smoTable.ForeignKeys)
                {
                    var fkExtract = new SqlForeignKey()
                    {
                        ObjectName = smoFk.Name,
                        Urn = smoFk.Urn.Value,
                        ReferencedTable = smoFk.ReferencedTable,
                        ReferencedTableSchema = smoFk.ReferencedTableSchema,
                        Columns = new List<SqlForeignKeyColumn>()
                    };

                    foreach (ForeignKeyColumn column in smoFk.Columns)
                    {
                        fkExtract.Columns.Add(new SqlForeignKeyColumn()
                        {
                            ColumnName = column.Name,
                            ReferencedColumnName = column.ReferencedColumn
                        });
                    }

                    tableExtract.ForeignKeys.Add(fkExtract);
                }

                var scripts = ExtractDatabaseScripts(smoTable);
                tableExtract.DefinitionScripts = scripts;

                res.Add(tableExtract);
            }
            return res;
        }
        
        private List<SmoObject> ExtractDatabaseViews(Database smoDb)
        {
            List<SmoObject> res = new List<SmoObject>();

            foreach (View smoView in smoDb.Views)
            {
                ConfigManager.Log.Important(smoView.Urn.Value);

                if (IsSystemSchemaObject(smoView))
                {
                    continue;
                }

                var schema = smoView.Schema;
                var viewExtract = new SqlView() { ObjectName = smoView.Name, Urn = smoView.Urn.Value,
                    Columns = new List<SqlColumn>(), SchemaName = schema };
                
                for (int i = 0; i < smoView.Columns.Count; i++)
                {
                    var smoColumn = smoView.Columns[i];
                    viewExtract.Columns.Add(ExtractColumn(smoColumn));   
                }

                var scripts = ExtractDatabaseScripts(smoView);
                viewExtract.DefinitionScripts = scripts;

                res.Add(viewExtract);
            }

            return res;
        }

        private List<SmoObject> ExtractDatabaseUdfs(Database smoDb)
        {
            List<SmoObject> res = new List<SmoObject>();

            foreach (UserDefinedFunction smoUdf in smoDb.UserDefinedFunctions)
            {
                ConfigManager.Log.Important(smoUdf.Urn.Value);

                if (IsSystemSchemaObject(smoUdf))
                {
                    continue;
                }

                var schema = smoUdf.Schema;

                SmoObject udfExtract;

                if (smoUdf.FunctionType == UserDefinedFunctionType.Scalar)
                {
                    udfExtract = new SqlScalarUdf() { ObjectName = smoUdf.Name,
                        Urn = smoUdf.Urn.Value, SchemaName = schema };
                }
                else
                {
                    udfExtract = new SqlTableUdf() { ObjectName = smoUdf.Name,
                        Urn = smoUdf.Urn.Value, SchemaName = schema };
                }

                var scripts = ExtractDatabaseScripts(smoUdf);
                udfExtract.DefinitionScripts = scripts;

                res.Add(udfExtract);
            }

            return res;
        }

        private List<SmoObject> ExtractDatabaseStoredProcedures(Database smoDb)
        {
            List<SmoObject> res = new List<SmoObject>();

            foreach (StoredProcedure smoSp in smoDb.StoredProcedures)
            {
                ConfigManager.Log.Important(smoSp.Urn.Value);

                if (IsSystemSchemaObject(smoSp))
                {
                    continue;
                }

                var schema = smoSp.Schema;
                var spExtract = new SqlProcedure() { ObjectName = smoSp.Name,
                    Urn = smoSp.Urn.Value, SchemaName = schema };

                var scripts = ExtractDatabaseScripts(smoSp);
                spExtract.DefinitionScripts = scripts;

                res.Add(spExtract);
            }

            return res;
        }

        private List<SmoObject> ExtractDatabaseTableTypes(Database smoDb)
        {
            List<SmoObject> res = new List<SmoObject>();

            foreach (UserDefinedTableType smoUdtt in smoDb.UserDefinedTableTypes)
            {
                ConfigManager.Log.Important(smoUdtt.Urn.Value);

                if (IsSystemSchemaObject(smoUdtt))
                {
                    continue;
                }

                var schema = smoUdtt.Schema;
                var tableTypeExtract = new SqlTableType() { ObjectName = smoUdtt.Name,
                    Urn = smoUdtt.Urn.Value, Columns = new List<SqlColumn>(), SchemaName = schema };
                
                foreach (Column smoColumn in smoUdtt.Columns)
                {
                    tableTypeExtract.Columns.Add(ExtractColumn(smoColumn));
                }

                var scripts = ExtractDatabaseScripts(smoUdtt);
                tableTypeExtract.DefinitionScripts = scripts;

                res.Add(tableTypeExtract);
            }

            return res;
        }

        private SqlColumn ExtractColumn(Column col)
        {
            return new SqlColumn()
            {
                ObjectName = col.Name,
                Length = col.DataType.MaximumLength,
                Scale = col.DataType.NumericScale,
                Precision = col.DataType.NumericPrecision,
                SqlDataType = col.DataType.SqlDataType.ToString(),
                Urn = col.Urn.Value
            };
        }


        private List<string> ExtractDatabaseScripts(SqlSmoObject obj)
        {
            List<SqlSmoObject> objectsToScript = new List<SqlSmoObject>();
            if (obj is Table)
            {
                objectsToScript = CollectObjectsToScript((Table)obj);
            }
            else if (obj is StoredProcedure)
            {
                objectsToScript = CollectObjectsToScript((StoredProcedure)obj);
            }
            else if (obj is UserDefinedFunction)
            {
                objectsToScript = CollectObjectsToScript((UserDefinedFunction)obj);
            }
            else if (obj is UserDefinedTableType)
            {
                objectsToScript = CollectObjectsToScript((UserDefinedTableType)obj);
            }
            else if (obj is View)
            {
                objectsToScript = CollectObjectsToScript((View)obj);
            }
            else
            {
                throw new NotImplementedException();
            }

            List<string> res = new List<string>();
            Scripter scrp = new Scripter(_server);

            scrp.Options.Indexes = false;
            scrp.Options.WithDependencies = false;
            scrp.Options.Triggers = false;
            scrp.Options.ScriptData = false;
            scrp.Options.SchemaQualify = true;
            scrp.Options.Permissions = false;
            scrp.Options.NoViewColumns = false;
            scrp.Options.NoFileStreamColumn = false;
            scrp.Options.NoFileGroup = true;
            scrp.Options.NoCollation = true;
            scrp.Options.NoAssemblies = false;
            scrp.Options.IncludeIfNotExists = false;
            scrp.Options.DriAllConstraints = true;
            scrp.Options.ScriptDrops = false;
            scrp.Options.ScriptSchema = true;
            scrp.Options.DriForeignKeys = true;
            scrp.Options.SchemaQualifyForeignKeysReferences = true;
            scrp.Options.Permissions = true;

            StringCollection scripts = scrp.Script(objectsToScript.Select(x => x.Urn).ToArray());

            foreach (var script in scripts)
            {
                res.Add(script);
            }
            return res;
        }

        //private List<SqlSmoObject> CollectObjectsToScript(SqlSmoObject obj)
        //{
        //    throw new NotImplementedException();
        //}

        /// <summary>
        /// Collect objects that should have the script retrieved and parsed.
        /// </summary>
        private List<SqlSmoObject> CollectObjectsToScript(Table table)
        {

            List<SqlSmoObject> objectsToScript = new List<SqlSmoObject>();

            if (!IsSystemElement(table))
                objectsToScript.Add(table);

            foreach (ForeignKey fk in table.ForeignKeys)
            {
                objectsToScript.Add(fk);
            }

            return objectsToScript;
        }

        private List<SqlSmoObject> CollectObjectsToScript(View view)
        {

            List<SqlSmoObject> objectsToScript = new List<SqlSmoObject>();

            if (!IsSystemElement(view))
                objectsToScript.Add(view);
            return objectsToScript;
        }

        private List<SqlSmoObject> CollectObjectsToScript(StoredProcedure sp)
        {

            List<SqlSmoObject> objectsToScript = new List<SqlSmoObject>();

            if (!IsSystemElement(sp))
                objectsToScript.Add(sp);

            return objectsToScript;
        }

        private List<SqlSmoObject> CollectObjectsToScript(UserDefinedFunction f)
        {

            List<SqlSmoObject> objectsToScript = new List<SqlSmoObject>();

            if (!IsSystemElement(f))
                objectsToScript.Add(f);

            return objectsToScript;
        }

        private List<SqlSmoObject> CollectObjectsToScript(UserDefinedTableType udtt)
        {

            List<SqlSmoObject> objectsToScript = new List<SqlSmoObject>();

            if (!IsSystemElement(udtt))
                objectsToScript.Add(udtt);

            return objectsToScript;
        }
        
        /// <summary>
        /// Determines whether the object is in a system schema and should not be extracted.
        /// </summary>
        private bool IsSystemSchemaObject(ScriptSchemaObjectBase obj)
        {
            return
                obj.Schema == "sys" || obj.Schema == "INFORMATION_SCHEMA";
            //!(_coveredSchemas.Contains(obj.Schema));
        }

        private bool IsSystemElement(ScriptSchemaObjectBase obj)
        {
            return obj.Schema == "sys" || obj.Schema == "INFORMATION_SCHEMA";
        }

    }
}

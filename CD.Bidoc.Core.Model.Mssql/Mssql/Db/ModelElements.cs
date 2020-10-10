using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Runtime.Serialization;
using CD.DLS.Model.Interfaces;
using CD.DLS.Model.Mssql.Tabular;

namespace CD.DLS.Model.Mssql.Db
{
    public abstract class DbModelElement : MssqlModelElement
    {        
        public DbModelElement(RefPath refPath, string caption)
            :base(refPath, caption)
        {
        }
        public DbModelElement(RefPath refPath, string caption, string definition, MssqlModelElement parent = null)
            :this(refPath, caption)
        {
            this.Parent = parent;
            this.Definition = definition;
        }

    }

    /// <summary>
    /// All that can be characterized by a (readable) script is a ScriptNode - different view mode from higher nodes
    /// </summary>
    public abstract class DbScriptedElement : DbModelElement
    {
        public DbScriptedElement(RefPath refPath, string caption, string definition, DbModelElement parentSchema)
                :base(refPath, caption, definition, parentSchema)
        { }

        [ModelLink]
        public SqlFragmentElement SqlDefinition { get; set; }

    }

    public class ScalarUdfElement : UdfElement
    {
        public ScalarUdfElement(RefPath refPath, string caption, string definition, SchemaElement parentSchema)
                : base(refPath, caption, definition, parentSchema)
        { }
    }

    public class TableUdfElement : UdfElement
    {
        public TableUdfElement(RefPath refPath, string caption, string definition, SchemaElement parentSchema)
                : base(refPath, caption, definition, parentSchema)
        { }
    }

    public class ProcedureElement : DbScriptedElement
    {
        public ProcedureElement(RefPath refPath, string caption, string definition, SchemaElement parentSchema):
            base(refPath, caption, definition, parentSchema)
        {
        }
        
        public Dictionary<string, MssqlModelElement> OutputColumns { get
            {
                var resultChild = DescendantsOfType<SqlScriptResultElement>().OrderBy(x => x.Ordinal).FirstOrDefault(); //ChildrenOfType
                if (resultChild == null)
                {
                    return new Dictionary<string, MssqlModelElement>();
                }
                return resultChild.OutputColumns;
            } }
        public List<Tuple<string, MssqlModelElement>> OutputColumnsOrdinal { get
            {
                var resultChild = DescendantsOfType<SqlScriptResultElement>().OrderBy(x => x.Ordinal).FirstOrDefault(); //ChildrenOfType
                if (resultChild == null)
                {
                    return new List<Tuple<string, MssqlModelElement>>();
                }
                return resultChild.OutputColumnsOrdinal;
            } }
    }

    public class SchemaTableElement : MssqlColumnScriptElement
    {
        public SchemaTableElement(RefPath refPath, string caption, string definition, SchemaElement parentSchema):
            base(refPath, caption, definition, parentSchema)
        {
        }

        public void AddForeignKey(ForeignKeyElement fkElement) {
            _children.Add(fkElement);
        }

        public IEnumerable<ForeignKeyElement> ForeignKeys {get{return _children.Where(n => n is ForeignKeyElement).Cast<ForeignKeyElement>();}}

        public ForeignKeyElement GetForeignKeyByName(string name)
        {
            return _children.Where(n => n is ForeignKeyElement && n.Caption == name).Cast<ForeignKeyElement>().FirstOrDefault();
        }
        
    }

    public class ForeignDbTableElement : MssqlModelElement
    {
        public ForeignDbTableElement(RefPath refPath, string caption) :
            base(refPath, caption)
        {
        }
        
    }

    public class UserDefinedTableTypeElement : MssqlColumnScriptElement
    {
        public UserDefinedTableTypeElement(RefPath refPath, string caption, string definition, SchemaElement parentSchema) :
            base(refPath, caption, definition, parentSchema)
        {
        }

        public void AddForeignKey(ForeignKeyElement fkElement)
        {
            _children.Add(fkElement);
        }
    }

    public class ForeignKeyElement : DbScriptedElement
    {
        public ForeignKeyElement(RefPath refPath, string caption, string definition, DbModelElement parentTable)
                : base(refPath, caption, definition, parentTable)
        { }

        [ModelLink]
        public ColumnElement SourceColumn { get; set; }
        [ModelLink]
        public ColumnElement TargetColumn { get; set; }
    }

    public class ViewElement : MssqlColumnScriptElement
    {
        public ViewElement(RefPath refPath, string caption, string definition, SchemaElement parentSchema)
                : base(refPath, caption, definition, parentSchema)
        { }
    }

    public class MssqlColumnScriptElement : DbScriptedElement
    {
        public MssqlColumnScriptElement(RefPath refPath, string caption, string definition, SchemaElement parentSchema)
                :base(refPath, caption, definition, parentSchema)
        { }


        public ColumnElement GetColumnByName(string name) { return (ColumnElement)_children.Where(c=>c is ColumnElement && c.Caption == name).FirstOrDefault(); }
        public IEnumerable<ColumnElement> Columns { get { return _children.Where(c => c is ColumnElement).Cast<ColumnElement>(); } }
    }

    [DataContract]
    public class DatabaseElement : DbModelElement
    {
        private string _name;
        private Dictionary<string, SchemaElement> _schemaByName;

        [DataMember]
        public string DbName
        {
            get { return _name; }
            set { _name = value; }
        }

        public IEnumerable<ScalarUdfElement> ScalarUdfs { get { return CollectSchemaObjects<ScalarUdfElement>(); } }
        public IEnumerable<TableUdfElement> NonScalarUdfs { get { return CollectSchemaObjects<TableUdfElement>(); } }
        public IEnumerable<SchemaTableElement> Tables { get { return CollectSchemaObjects<SchemaTableElement>(); } }
        public IEnumerable<ProcedureElement> StoredProcedures { get { return CollectSchemaObjects<ProcedureElement>(); } }
        public IEnumerable<ViewElement> Views { get { return CollectSchemaObjects<ViewElement>(); } }
        public IEnumerable<UserDefinedTableTypeElement> TableTypes { get { return CollectSchemaObjects<UserDefinedTableTypeElement>(); } }
        
        public DatabaseElement(RefPath refPath, string caption)
            :base(refPath, caption) { }

        public DatabaseElement(RefPath refPath, string caption, ServerElement parent)
                : base(refPath, caption, null, parent)
        {
            _name = Caption;
        }

        public SchemaTableElement TableBySchemaName(string schema, string name)
        {
            var schemaElement = SchemaByName(schema);
            return schemaElement.TableByName(name);
        }
        public SchemaElement SchemaByName(string name)
        {
            if(_schemaByName == null) {
                _schemaByName = new Dictionary<string, SchemaElement>(StringComparer.OrdinalIgnoreCase);
                foreach (var child in _children.Cast<SchemaElement>())
                {
                    _schemaByName.Add(child.Caption, child);
                }
            }
            return _schemaByName[name];
        }

        public void AddSchema(SchemaElement schema)
        {
            _children.Add(schema);
            if (_schemaByName != null)
                _schemaByName.Add(schema.Caption, schema);
        }

        private IEnumerable<T> CollectSchemaObjects<T>() where T : DbModelElement
        {
            return _children.SelectMany(n => n.Children.Where(nn => nn is T).Cast<T>());
        }
    }

    public class SchemaElement : DbModelElement
    {
        public SchemaElement(RefPath refPath, string caption)
            :base(refPath, caption) { }

        public SchemaElement(RefPath refPath, string caption, DatabaseElement parent)
                : base(refPath, caption, null, parent)
        { }


        //VD: TODO change lookups
        public ViewElement ViewByName(string s) {return ChildOfTypeByCaption<ViewElement>(s, StringComparer.OrdinalIgnoreCase);}
        public ProcedureElement ProcedureByName(string s) {return (ProcedureElement)_children.Where(n => n is ProcedureElement && StringComparer.OrdinalIgnoreCase.Compare(n.Caption, s) == 0).FirstOrDefault();}
        public SchemaTableElement TableByName(string s) {return (SchemaTableElement)_children.Where(n => n is SchemaTableElement && StringComparer.OrdinalIgnoreCase.Compare(n.Caption, s) == 0).FirstOrDefault();}
        public UserDefinedTableTypeElement TableTypeByName(string s) { return (UserDefinedTableTypeElement)_children.Where(n => n is UserDefinedTableTypeElement && StringComparer.OrdinalIgnoreCase.Compare(n.Caption, s) == 0).FirstOrDefault(); }
        public UdfElement UdfByName(string s) {return (UdfElement)_children.Where(n => n is UdfElement && StringComparer.OrdinalIgnoreCase.Compare(n.Caption, s) == 0).FirstOrDefault();}
    }

    public class ServerElement : DbModelElement
    {
        public ServerElement(RefPath refPath, string caption)
                : base(refPath, caption)
        { }

        public ServerElement(RefPath refPath, string caption, string definition, MssqlModelElement parent = null)
                : base(refPath, caption, definition, parent)
        { }

        public DatabaseElement DatabaseByCaption(string name)
        {
            return ChildOfTypeByCaption<DatabaseElement>(name);
        }

        public IEnumerable<DatabaseElement> Databases { get { return ChildrenOfType<DatabaseElement>(); } }
    }


    public abstract class UdfElement : MssqlColumnScriptElement
    {
        public UdfElement(RefPath refPath, string caption, string definition, SchemaElement parentSchema)
                : base(refPath, caption, definition, parentSchema)
        { }
        
    }

    public class ColumnElement : DbScriptedElement
    {
        public ColumnElement(RefPath refPath, string caption, string definition, DbScriptedElement parent)
                : base(refPath, caption, definition, parent)
        { }

       [DataMember]
       public int Length { get; set; }
       [DataMember]
       public int Scale { get; set; }
       [DataMember]
       public int Precision { get; set; }
       [DataMember]
       public string SqlDataType { get; set; }
    }

    //public class ProcedureOutputColumnElement : DbScriptedElement
    //{
    //    public ProcedureOutputColumnElement(RefPath refPath, string caption, string definition, DbScriptedElement parent)
    //            : base(refPath, caption, definition, parent)
    //    { }
    //}
}

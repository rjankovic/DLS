using CD.DLS.Model.Interfaces;
using CD.DLS.Model.Mssql.Db;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace CD.DLS.Model.Mssql.Ssis
{

    /// <summary>
    /// Relative or absolute (for sizes and arrow segments) position
    /// </summary>
    public class DesignPoint
    {
        public float X { get; set; }
        public float Y { get; set; }
    }

    /// <summary>
    /// An arrow connecting two DF or CF components
    /// </summary>
    public class DesignArrow
    {
        public DesignPoint Start { get; set; }
        public DesignPoint End { get; set; }
        public DesignPoint TopLeft { get; set; }
        /// <summary>
        /// Brush moves
        /// </summary>
        public List<DesignPoint> Shifts { get; set; }
        public enum PointOrientationEnum { Left, Up, Right, Down }
        public PointOrientationEnum PointOrientation
        {
            get
            {
                if (Shifts.Count == 0)
                {
                    // nasty cover-up
                    return PointOrientationEnum.Down;
                }
                var finSeg = Shifts[Shifts.Count - 1];
                if (finSeg.X < 0)
                    return PointOrientationEnum.Left;
                if (finSeg.Y < 0)
                    return PointOrientationEnum.Up;
                if (finSeg.X > 0)
                    return PointOrientationEnum.Right;
                if (finSeg.Y > 0)
                    return PointOrientationEnum.Down;

                // dtto
                return PointOrientationEnum.Down;
            }
        }
    }

    abstract public class SsisModelElement : MssqlModelElement
    {
        public SsisModelElement(RefPath refPath, string caption, string definition, SsisModelElement parent = null)
           : base(refPath, caption)
        {
            Definition = definition;
            Parent = parent;
        }

        public string LocalhostServer()
        {
            if (this is ServerElement)
            {
                return this.Caption;
            }
            return ((SsisModelElement)Parent).LocalhostServer();
        }
    }

    /// <summary>
    /// An SSIS server
    /// </summary>
    public class ServerElement : SsisModelElement
    {
        // no need for extensions
        public ServerElement(RefPath refPath, string caption, string definition, SsisModelElement parent = null)
                : base(refPath, caption, definition)
        { }
    }

    /// <summary>
    /// An SSIS catalog
    /// </summary>
    public class CatalogElement : SsisModelElement
    {
        // no need for extensions
        public CatalogElement(RefPath refPath, string caption, string definition, SsisModelElement parent)
                : base(refPath, caption, definition, parent)
        { }
    }

    /// <summary>
    /// An SSIS catalog folder
    /// </summary>
    public class FolderElement : SsisModelElement
    {
        // no need for extensions
        public FolderElement(RefPath refPath, string caption, string definition, SsisModelElement parent)
                : base(refPath, caption, definition, parent)
        { }
    }

    /// <summary>
    /// An SSIS catalog project
    /// </summary>
    public class ProjectElement : SsisModelElement
    {
        public ProjectElement(RefPath refPath, string caption, string definition, SsisModelElement parent)
                : base(refPath, caption, definition, parent)
        { }

        public IEnumerable<ProjectParameterElement> Parameters { get { return _children.Where(x => x is ProjectParameterElement).Select(x => x as ProjectParameterElement); } }

        public IEnumerable<ConnectionManagerElement> ConnectionManagers { get { return _children.Where(x => x is ConnectionManagerElement).Select(x => x as ConnectionManagerElement); } }

        public IEnumerable<PackageElement> Packages { get { return ChildrenOfType<PackageElement>(); } }
    }

    /// <summary>
    /// An executable (task or container), DF component, DF path or precedence constraint
    /// </summary>
    [DataContract]
    public class DesignBlockElement : SsisModelElement
    {
        [DataMember]
        public DesignPoint Position { get; set; }
        [DataMember]
        public DesignPoint Size { get; set; }

        public IEnumerable<DesignBlockElement> ChildBlocks
        {
            get
            {
                // all descendant are in children (by ComputeLinks) (?)
                return _children.Where(x => x.Parent == this && x is DesignBlockElement).Select(x => x as DesignBlockElement);
            }
        }
        public DesignBlockElement(RefPath refPath, string caption, string definition, SsisModelElement parent)
                : base(refPath, caption, definition, parent)
        { }
    }

    /// <summary>
    /// A package - contains executables and PCs
    /// </summary>
    public class PackageElement : DesignBlockElement
    {
        public PackageElement(RefPath refPath, string caption, string definition, SsisModelElement parent)
                : base(refPath, caption, definition, parent)
        { }
    }

    /// <summary>
    /// An executable; may contain executables and PCs or a dataflow
    /// </summary>
    [DataContract]
    public class ExecutableElement : DesignBlockElement
    {
        public ExecutableElement(RefPath refPath, string caption, string definition, SsisModelElement parent)
                : base(refPath, caption, definition, parent)
        { }

        [DataMember]
        public bool Enabled { get; set; }
    }

    /// <summary>
    /// A container; may contain executables and PCs (not a task host)
    /// </summary>
    public class ContainerElement : ExecutableElement
    {
        public ContainerElement(RefPath refPath, string caption, string definition, SsisModelElement parent)
                : base(refPath, caption, definition, parent)
        { }
    }

    /// <summary>
    /// A contol flow task, no inner blocks
    /// </summary>
    public class TaskElement : ExecutableElement
    {
        public TaskElement(RefPath refPath, string caption, string definition, SsisModelElement parent)
                : base(refPath, caption, definition, parent)
        { }
    }

    /// <summary>
    /// A precedence constraint
    /// </summary>
    [DataContract]
    public class PrecedenceConstraintElement : DesignBlockElement
    {
        public PrecedenceConstraintElement(RefPath refPath, string caption, string definition, SsisModelElement parent)
                : base(refPath, caption, definition, parent)
        { }

        [ModelLink]
        public ExecutableElement From { get; set; }
        [ModelLink]
        public ExecutableElement To { get; set; }
        [DataMember]
        public DesignArrow Arrow { get; set; }
    }

    /// <summary>
    /// An Execute SQL task
    /// </summary>
    public class SqlTaskElement : TaskElement
    {
        public SqlTaskElement(RefPath refPath, string caption, string definition, SsisModelElement parent)
                : base(refPath, caption, definition, parent)
        { }

        public Db.SqlScriptElement Script { get { return _children.FirstOrDefault(x => x is SqlScriptElement) as SqlScriptElement; } }
    }
    
    /// <summary>
    /// An Expression Task.
    /// </summary>
    public class ExpressionTaskElement : TaskElement
    {
        public ExpressionTaskElement(RefPath refPath, string caption, string definition, SsisModelElement parent)
                : base(refPath, caption, definition, parent)
        { }

    }
    
    /// <summary>
    /// An Execute package task
    /// </summary>
    public class ExecutePackageTaskElement : TaskElement
    {
        public ExecutePackageTaskElement(RefPath refPath, string caption, string definition, SsisModelElement parent)
                : base(refPath, caption, definition, parent)
        { }

        [ModelLink]
        public PackageElement Package { get; set; }
    }

    public class ExecutePackageParameterAssignmentElement : SsisModelElement
    {
        public ExecutePackageParameterAssignmentElement(RefPath refPath, string caption, string definition, SsisModelElement parent)
            : base(refPath, caption, definition, parent)
        { }

        [ModelLink]
        public ReferrableValueElement Assigned { get; set; }
    }





    /// <summary>
    /// An annotation in the control or data flow
    /// </summary>
    public class AnnotationElement : DesignBlockElement
    {
        public AnnotationElement(RefPath refPath, string caption, string definition, SsisModelElement parent)
                : base(refPath, caption, definition, parent)
        { }
    }

    [DataContract]
    public abstract class ReferrableValueElement : SsisModelElement
    {
        public ReferrableValueElement(RefPath refPath, string caption, string definition, SsisModelElement parent)
            : base(refPath, caption, definition, parent)
        { }

        [DataMember]
        public TypeCode DataType { get; set; }
        [DataMember]
        public string Value { get; set; }
        public abstract string GetExpressionReferenceName();
    }

    /// <summary>
    /// A package parameter
    /// </summary>
    public class PackageParameterElement : ReferrableValueElement
    {
        public PackageParameterElement(RefPath refPath, string caption, string definition, SsisModelElement parent)
                : base(refPath, caption, definition, parent)
        { }

        public override string GetExpressionReferenceName()
        {
            return string.Format("$Package::{0}", Caption);
        }
    }

    /// <summary>
    /// A project parameter
    /// </summary>
    public class ProjectParameterElement : ReferrableValueElement
    {
        public ProjectParameterElement(RefPath refPath, string caption, string definition, SsisModelElement parent)
                : base(refPath, caption, definition, parent)
        { }
        public override string GetExpressionReferenceName()
        {
            return string.Format("@[$Project::{0}]", Caption);
        }
    }

    /// <summary>
    /// A variable defined in some scope of the package
    /// </summary>
    public class VariableElement : ReferrableValueElement
    {
        public VariableElement(RefPath refPath, string caption, string definition, SsisModelElement parent)
                : base(refPath, caption, definition, parent)
        { }
        public override string GetExpressionReferenceName()
        {
            return string.Format("User::{0}", Caption);
        }
    }

    public class SystemVariableElement : VariableElement
    {
        public SystemVariableElement(RefPath refPath, string caption, string definition, SsisModelElement parent)
                : base(refPath, caption, definition, parent)
        { }
        public override string GetExpressionReferenceName()
        {
            return string.Format("System::{0}", Caption);
        }
    }

    /// <summary>
    /// A property of a component, task or other object
    /// </summary>
    public class PropertyElement : SsisModelElement
    {
        public PropertyElement(RefPath refPath, string caption, string definition, SsisModelElement parent)
                : base(refPath, caption, definition, parent)
        { }
        /*
        /// <summary>
        /// Mostly ExpressionNodes, can be Sql ScriptNodes or other
        /// </summary>
        public BasicDependencyNode Value { get; set; }
        */
    }

    /// <summary>
    /// An expression using variables and parameters in the current scope
    /// </summary>
    public class ExpressionElement : SsisModelElement
    {
        public ExpressionElement(RefPath refPath, string caption, string definition, SsisModelElement parent)
                : base(refPath, caption, definition, parent)
        { }
    }

    /// <summary>
    /// A reference to a variable or an parameter
    /// </summary>
    public class ExpressionReferenceElement : SsisModelElement
    {
        public ExpressionReferenceElement(RefPath refPath, string caption, string definition, SsisModelElement parent)
                : base(refPath, caption, definition, parent)
        { }
    }

    [DataContract]
    public class ConnectionManagerElement : SsisModelElement
    {

        protected string _connectionString;
        protected string _id;

        [DataMember]
        public string ConnectionString
        {
            get { return _connectionString; }
            set { _connectionString = value; }
        }

        [DataMember]
        public string ManagerId
        {
            get { return _id; }
            set { _id = value; }
        }

        [DataMember]
        public string SourceType { get; set; }

        public ConnectionManagerElement(RefPath refPath, string caption, string definition, SsisModelElement parent)
                : base(refPath, caption, definition, parent)
        { }
        
    }
    
    [DataContract]
    public class DbConnectionManagerElement : ConnectionManagerElement
    {
        public string ConnectionDbName
        {
            get
            {
                return ConnectionStringParts.GetDbName(_connectionString);
            }
        }

        public string ConnectionServerName
        {
            get
            {
                return ConnectionStringParts.GetServerName(_connectionString);
            }
        }

        public ConnectionStringParts.ProviderTypeEnum ProviderType
        {
            get
            {
                return ConnectionStringParts.GetProviderType(_connectionString);
            }
        }


        public DbConnectionManagerElement(RefPath refPath, string caption, string definition, SsisModelElement parent)
                : base(refPath, caption, definition, parent)
        { }

    }

    [DataContract]
    public class FileConnectionManagerElement : ConnectionManagerElement
    {
        [DataMember]
        public string Format { get; set; }
        [DataMember]
        public int LocaleID { get; set; }
        [DataMember]
        public int CodePage { get; set; }

        public FileConnectionManagerElement(RefPath refPath, string caption, string definition, SsisModelElement parent)
                : base(refPath, caption, definition, parent)
        { }

    }


    public class ConnectionStringParts
    {
        public enum ProviderTypeEnum { SQLNCLI, MSOLAP, Other }

        public static string GetDbName(string connectionString)
        {
            var instance = new ConnectionStringParts(connectionString);
            string dbName = instance.GetValueByPrefix("Initial Catalog");
            return dbName ?? "master";
        }

        public static string GetServerName(string connectionString)
        {
            var instance = new ConnectionStringParts(connectionString);
            string serverName = instance.GetValueByPrefix("Data Source");
            return serverName ?? "localhost";
        }

        public static ProviderTypeEnum GetProviderType(string connectionString)
        {
            var instance = new ConnectionStringParts(connectionString);
            string providerString = instance.GetValueByPrefix("Provider");

            if (providerString != null)
            {
                if (providerString.StartsWith("SQLNCLI", StringComparison.OrdinalIgnoreCase))
                {
                    return ProviderTypeEnum.SQLNCLI;
                }
                else if (providerString.StartsWith("MSOLAP", StringComparison.OrdinalIgnoreCase))
                {
                    return ProviderTypeEnum.MSOLAP;
                }

            }
            return ProviderTypeEnum.Other;
        }

        private string[] _parts;

        public ConnectionStringParts(string connectionString)
        {
            _parts = connectionString.Split(';');
        }

        public string GetValueByPrefix(string prefix)
        {
            foreach (var pt in _parts)
            {
                var trim = pt.Trim();
                if (pt.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                {
                    return pt.Split('=')[1].Trim();
                }
            }
            return null;
        }

    }
}

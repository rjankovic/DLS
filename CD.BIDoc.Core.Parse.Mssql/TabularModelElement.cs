using CD.BIDoc.Core.Interfaces;
using CD.BIDoc.Core.Model.Mssql;
using CD.Framework.ORM.Objects.Extract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CD.BIDoc.Core.Parse.Mssql
{
    abstract public class TabularModelElement : MssqlModelElement
    {
        protected TabularModelElement(RefPath refPath, string caption, string definition, TabularModelElement parent = null)
           : base(refPath, caption)
        {
            Definition = definition;
            Parent = parent;
        }
    }

    public class ParsedTabularDatabase : TabularModelElement
    {
        public string collation;
        public ParsedTabularDatabase(RefPath refPath, string caption, string definition, TabularModelElement parent = null) : base(refPath, caption, definition, parent)
        {
          
        }
    }

    public class ParsedTabularModel : TabularModelElement
    {
        public ParsedTabularModel(RefPath refPath, string caption, string definition, TabularModelElement parent = null) : base(refPath, caption, definition, parent)
        {
        }
    }

    public class ParsedTabularDataSource : TabularModelElement
    {
        public ParsedTabularDataSource(RefPath refPath, string caption, string definition, TabularModelElement parent = null) : base(refPath, caption, definition, parent)
        {
        }
    }

    public class ParsedTabularTable : TabularModelElement
    {
        public ParsedTabularTable(RefPath refPath, string caption, string definition, TabularModelElement parent = null) : base(refPath, caption, definition, parent)
        {
        }
    }

    public class ParsedTabularTableColumn : TabularModelElement
    {
        
        public ParsedTabularTableColumn(RefPath refPath, string caption, string definition, TabularModelElement parent = null) : base(refPath, caption, definition, parent)
        {
        }
    }

    public class ParsedTabularAnnotation : TabularModelElement
    {
        public ParsedTabularAnnotation(RefPath refPath, string caption, string definition, TabularModelElement parent = null) : base(refPath, caption, definition, parent)
        {
        }
    }

    public class ParsedAttributeHierarchy : TabularModelElement
    {
        public ParsedAttributeHierarchy(RefPath refPath, string caption, string definition, TabularModelElement parent = null) : base(refPath, caption, definition, parent)
        {
        }
    }

    public class ParsedPartition : TabularModelElement
    {
        public ParsedPartition(RefPath refPath, string caption, string definition, TabularModelElement parent = null) : base(refPath, caption, definition, parent)
        {
        }
    }

    public class ParsedMeasure : TabularModelElement
    {
        public ParsedMeasure(RefPath refPath, string caption, string definition, TabularModelElement parent = null) : base(refPath, caption, definition, parent)
        {
        }
    }

    public class ParsedHierarchy : TabularModelElement
    {
        public ParsedHierarchy(RefPath refPath, string caption, string definition, TabularModelElement parent = null) : base(refPath, caption, definition, parent)
        {
        }
    }

    public class ParsedRelationship : TabularModelElement
    {
        public ParsedRelationship(RefPath refPath, string caption, string definition, TabularModelElement parent = null) : base(refPath, caption, definition, parent)
        {
        }
    }

    public class ParsedPerspective : TabularModelElement
    {
        public ParsedPerspective(RefPath refPath, string caption, string definition, TabularModelElement parent = null) : base(refPath, caption, definition, parent)
        {
        }
    }

    public class ParsedHierarchyLevel : TabularModelElement
    {
        public ParsedHierarchyLevel(RefPath refPath, string caption, string definition, TabularModelElement parent = null) : base(refPath, caption, definition, parent)
        {
        }
    }
}

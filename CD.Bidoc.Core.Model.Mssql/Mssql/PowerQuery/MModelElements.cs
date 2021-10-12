using CD.DLS.Model.Interfaces;
using CD.DLS.Model.Mssql.Tabular;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Db = CD.DLS.Model.Mssql.Db;

namespace CD.DLS.Model.Mssql.PowerQuery
{
    abstract public class MModelElement : MssqlModelElement
    {
        public MModelElement(RefPath refPath, string caption, string definition, MssqlModelElement parent = null)
           : base(refPath, caption)
        {
            Definition = definition;
            Parent = parent;
        }

        public MModelElement(RefPath refPath, string caption)
           : base(refPath, caption)
        { }
    }


    [DataContract]
    public class MFragmentElement : MModelElement
    {
        public MFragmentElement(RefPath refPath, string caption, string definition, MssqlModelElement parent)
            : base(refPath, caption, definition, parent) {

        }

        [DataMember]
        public int OffsetFrom { get; set; }
        [DataMember]
        public int Length { get; set; }

        public RecordItemIdentifierElement RecordItemId { get => ChildrenOfType<RecordItemIdentifierElement>().FirstOrDefault(); }
    }

    public class PowerQueryElement : MFragmentElement
    {
        public PowerQueryElement(RefPath refPath, string caption, string definition, MssqlModelElement parent)
            : base(refPath, caption, definition, parent) { }
        
    }

    public class DataFlowLinkElement : MModelElement
    {
        public DataFlowLinkElement(RefPath refPath, string caption, string definition, MssqlModelElement parent)
            : base(refPath, caption, definition, parent) { }
        
        [ModelLink]
        public MssqlModelElement Source { get; set; }
        [ModelLink]
        public MModelElement Target { get; set; }

        public override string ToString()
        {
            return Source.RefPath.Path + " -> " + Target.RefPath.Path;
        }
    }

    public class LiteralElement : MFragmentElement
    {
        public LiteralElement(RefPath refPath, string caption, string definition, MssqlModelElement parent)
            : base(refPath, caption, definition, parent) { }
    }

    public class IdentifierElement : MFragmentElement
    {
        public IdentifierElement(RefPath refPath, string caption, string definition, MssqlModelElement parent)
            : base(refPath, caption, definition, parent) { }
    }

    public class RecordItemIdentifierElement : MFragmentElement
    {
        public RecordItemIdentifierElement(RefPath refPath, string caption, string definition, MssqlModelElement parent)
            : base(refPath, caption, definition, parent) { }

        [DataMember]
        public string ItemId { get; set; }
    }

    public class VariableReferenceElement : OperationElement
    {
        public VariableReferenceElement(RefPath refPath, string caption, string definition, MssqlModelElement parent)
            : base(refPath, caption, definition, parent) { }
    }

    public class ColumnReferenceElement : MFragmentElement
    {
        public ColumnReferenceElement(RefPath refPath, string caption, string definition, MssqlModelElement parent)
            : base(refPath, caption, definition, parent) { }

        [DataMember]
        public string ColumnName { get; set; }
    }

    public class FormulaStepElement : MFragmentElement
    {
        public FormulaStepElement(RefPath refPath, string caption, string definition, MssqlModelElement parent)
            : base(refPath, caption, definition, parent)
        {
        }

        [DataMember]
        public string VariableName { get; set; }

        public OperationElement Operation { get { return ChildrenOfType<OperationElement>().FirstOrDefault(); } }
    }

    public class OperationArgumentElement : MFragmentElement
    {
        public OperationArgumentElement(RefPath refPath, string caption, string definition, MssqlModelElement parent)
            : base(refPath, caption, definition, parent) {
        }
        
        public MFragmentElement Content { get { return ChildrenOfType<MFragmentElement>().FirstOrDefault(); } }
    }

    public class ListElement : MFragmentElement
    {
        public ListElement(RefPath refPath, string caption, string definition, MssqlModelElement parent)
            : base(refPath, caption, definition, parent)
        {
        }

        public IEnumerable<MFragmentElement> Items { get { return ChildrenOfType<MFragmentElement>().Except(ChildrenOfType<ListIndexElement>()); } }

        public ListIndexElement Index { get { return ChildrenOfType<ListIndexElement>().FirstOrDefault(); } }
    }

    public class ListIndexElement : MFragmentElement
    {
        public ListIndexElement(RefPath refPath, string caption, string definition, MssqlModelElement parent)
                : base(refPath, caption, definition, parent)
        {
        }

        public ListIndexElement InnerIndex { get { return ChildrenOfType<ListIndexElement>().FirstOrDefault(); } }
        [ModelLink]
        public MFragmentElement Content { get; set; }
    }

    public class ListAccessElement : OperationElement
    {
        public ListAccessElement(RefPath refPath, string caption, string definition, MssqlModelElement parent)
            : base(refPath, caption, definition, parent)
        {
        }

        [DataMember]
        public string ListName { get; set; }

        [ModelLink]
        public VariableReferenceElement ListFromVariable { get; set; }

        public ListIndexElement Index  { get { return ChildrenOfType<ListIndexElement>().First(); } }
    }

    public class RecordElement : MFragmentElement
    {
        public RecordElement(RefPath refPath, string caption, string definition, MssqlModelElement parent)
                : base(refPath, caption, definition, parent)
        {
        }

        public IEnumerable<RecordItemElement> Items { get { return ChildrenOfType<RecordItemElement>(); } }
    }

    public class RecordItemElement : MFragmentElement
    {
        public RecordItemElement(RefPath refPath, string caption, string definition, MssqlModelElement parent)
                       : base(refPath, caption, definition, parent)
        {
        }

        [DataMember]
        public string ItemName { get; set; }

        public MFragmentElement ItemValue { get => ChildrenOfType<MFragmentElement>().FirstOrDefault(); }
    }

    public abstract class OperationElement : MFragmentElement
    {
        public OperationElement(RefPath refPath, string caption, string definition, MssqlModelElement parent)
            : base(refPath, caption, definition, parent) { }

        public IEnumerable<OperationArgumentElement> Arguments { get { return ChildrenOfType<OperationArgumentElement>().OrderBy(x => x.OffsetFrom); } }
        public IEnumerable<DataFlowLinkElement> DataFlowLinks { get { return ChildrenOfType<DataFlowLinkElement>(); } }
        
        [DataMember]
        public string FunctionName { get; set; }

        //public virtual void CreateDataFlowLinksAndOutputColumns()
        //{
        //}

        public IEnumerable<OperationOutputColumnElement> OutputColumns
        {
            get { return ChildrenOfType<OperationOutputColumnElement>(); }
        }

    }

    public class SqlDatabaseOperationElement : OperationElement
    {
        public SqlDatabaseOperationElement(RefPath refPath, string caption, string definition, MssqlModelElement parent)
                : base(refPath, caption, definition, parent) { }

        [ModelLink]
        public Db.DatabaseElement DatabaseReference { get; set; }
    }

    public class ScalarOperationElement : OperationElement
    {
        public ScalarOperationElement(RefPath refPath, string caption, string definition, MssqlModelElement parent)
            : base(refPath, caption, definition, parent) { }

        //public DataFlowLinkElement AddDataFlowLink(MModelElement source)
        //{
        //    var linkCount = DataFlowLinks.Count();
        //    var refPath = RefPath.NamedChild("DataFlowLink", string.Format("No_{0}", linkCount + 1));
        //    DataFlowLinkElement daxDataFlowLinkElement = new DataFlowLinkElement(refPath, string.Format("DataFlowLink {0}", linkCount + 1), null, this);
        //    this.AddChild(daxDataFlowLinkElement);
        //    daxDataFlowLinkElement.Parent = this;
        //    daxDataFlowLinkElement.Source = source;
        //    daxDataFlowLinkElement.Target = this;
        //    return daxDataFlowLinkElement;
        //}
    }

    public class OperationOutputColumnElement : MFragmentElement // DaxElement
    {
        public OperationOutputColumnElement(RefPath refPath, string caption, string definition, MssqlModelElement parent)
            : base(refPath, caption, definition, parent) {

            //if (parent is DaxFragmentElement)
            //{
            //    OffsetFrom = ((DaxFragmentElement)parent).OffsetFrom;
            //    Length = ((DaxFragmentElement)parent).Length;
            //}
        }
    }

    public class TableRowOperationElement : OperationElement
    {
        public TableRowOperationElement(RefPath refPath, string caption, string definition, MssqlModelElement parent)
            : base(refPath, caption, definition, parent) { }
    }


}

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

    public class TableSplitColumnOperationElement : OperationElement
    {
        public TableSplitColumnOperationElement(RefPath refPath, string caption, string definition, MssqlModelElement parent)
            : base(refPath, caption, definition, parent) { }
    }

    public class TableDuplicateColumnOperationElement : OperationElement
    {
        public TableDuplicateColumnOperationElement(RefPath refPath, string caption, string definition, MssqlModelElement parent)
            : base(refPath, caption, definition, parent) { }
    }

    public class TableRemoveColumnsOperationElement : OperationElement
    {
        public TableRemoveColumnsOperationElement(RefPath refPath, string caption, string definition, MssqlModelElement parent)
            : base(refPath, caption, definition, parent) { }
    }

    public class TableSelectColumnsOperationElement : OperationElement
    {
        public TableSelectColumnsOperationElement(RefPath refPath, string caption, string definition, MssqlModelElement parent)
            : base(refPath, caption, definition, parent) { }
    }
    public class TableRenameColumnsOperationElement : OperationElement
    {
        public TableRenameColumnsOperationElement(RefPath refPath, string caption, string definition, MssqlModelElement parent)
            : base(refPath, caption, definition, parent) { }
    }
    public class GeneralOperationElement : OperationElement
    {
        public GeneralOperationElement(RefPath refPath, string caption, string definition, MssqlModelElement parent)
            : base(refPath, caption, definition, parent) { }
    }


    /*
let
    Source = Sql.Database("localhost", "BE"),
    dbo_GeneralService_T = Source{[Schema="dbo",Item="GeneralService_T"]}[Data],
    #"Split Column by Delimiter" = Table.SplitColumn(Table.TransformColumnTypes(dbo_GeneralService_T, {{"Source_System_Reference_Datetime", type text}}, "en-US"), "Source_System_Reference_Datetime", Splitter.SplitTextByDelimiter(",", QuoteStyle.Csv), {"Source_System_Reference_Datetime.1", "Source_System_Reference_Datetime.2"}),
    X #"Changed Type" = Table.TransformColumnTypes(#"Split Column by Delimiter",{{"Source_System_Reference_Datetime.1", type text}, {"Source_System_Reference_Datetime.2", type text}}),
    X #"Trimmed Text" = Table.TransformColumns(#"Changed Type",{{"Source_System_Code", Text.Trim, type text}}),
    #"Duplicated Column" = Table.DuplicateColumn(#"Trimmed Text", "Main_Service_ID", "Main_Service_ID - Copy"),
    #"Removed Columns" = Table.RemoveColumns(#"Duplicated Column",{"Source_System_Extract_Datetime"}),
    X #"Replaced Value" = Table.ReplaceValue(#"Removed Columns",10,20,Replacer.ReplaceValue,{"Update_Batch_ID"}),
    X #"Removed Duplicates" = Table.Distinct(#"Replaced Value", {"Main_Service_ID", "Service_Description"}),
    X #"Removed Errors" = Table.RemoveRowsWithErrors(#"Removed Duplicates", {"Service_Description"}),
    #"Removed Other Columns" = Table.SelectColumns(#"Removed Errors",{"Source_System_Reference_Datetime.2", "Source_System_Code", "SOR_GeneralService_ID", "Main_Service_ID", "Service_Description", "Service_Type", "Variable_Type", "Quantity_Unit", "Extra_Service_Configured", "Effective_Datetime", "End_Datetime", "Current_Row", "Insert_Batch_ID", "Update_Batch_ID", "Main_Service_ID - Copy"}),
    X #"Filtered Rows" = Table.SelectRows(#"Removed Other Columns", each Date.IsInPreviousYear([Effective_Datetime]))
in
    #"Filtered Rows"
     */

}

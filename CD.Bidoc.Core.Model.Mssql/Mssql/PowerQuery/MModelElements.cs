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

    public class VariableReferenceElement : MFragmentElement
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

        public IEnumerable<MFragmentElement> Items { get { return ChildrenOfType<MFragmentElement>(); } }
    }

    public abstract class OperationElement : MFragmentElement
    {
        public OperationElement(RefPath refPath, string caption, string definition, MssqlModelElement parent)
            : base(refPath, caption, definition, parent) { }

        public IEnumerable<OperationArgumentElement> Arguments { get { return ChildrenOfType<OperationArgumentElement>().OrderBy(x => x.OffsetFrom); } }
        public IEnumerable<DataFlowLinkElement> DataFlowLinks { get { return ChildrenOfType<DataFlowLinkElement>(); } }
        
        public virtual void CreateDataFlowLinksAndOutputColumns()
        {
        }

        public IEnumerable<OperationOutputColumnElement> OutputColumns
        {
            get { return ChildrenOfType<OperationOutputColumnElement>(); }
        }

        protected OperationOutputColumnElement AddOutputColumn(string name)
        {
            var refPath = RefPath.NamedChild("OutputColumn", name);
            var columnElement = new OperationOutputColumnElement(refPath, name, null, this);
            this.AddChild(columnElement);
            return columnElement;
        }
        
        public ArgumentList CollectArgumentList()
        {
            ArgumentList list = new ArgumentList();
            list.Arguments = new List<Argument>();

            foreach (var argument in Arguments)
            {
                var argumentWrap = new Argument()
                {
                    Columns = new List<ArgumentColumn>(),
                    FragmentElement = argument.Content
                };

                if (argument.Content is OperationElement && ((OperationElement)argument.Content).OutputColumns.Any())
                {
                    argumentWrap.ArgumentType = ArgumentType.Table;

                    var operation = (OperationElement)(argument.Content);

                    foreach (var column in operation.OutputColumns)
                    {
                        argumentWrap.Columns.Add(new ArgumentColumn()
                        {
                            Name = column.Caption,
                            RefereneElement = column
                        });
                    }
                }
                else if (argument.Content is VariableReferenceElement)
                {
                    argumentWrap.ArgumentType = ArgumentType.Table;

                    var variable = argument.Content.Reference as FormulaStepElement;

                    foreach (var column in variable.Operation.OutputColumns)
                    {
                        argumentWrap.Columns.Add(new ArgumentColumn()
                        {
                            Name = column.Caption,
                            RefereneElement = column
                        });
                    }
                }
                else if (argument.Content is ListElement)
                {
                    var lst = argument.Content as ListElement;
                    argumentWrap.ArgumentType = ArgumentType.List;
                }
                // scalar
                else
                {
                    argumentWrap.ArgumentType = ArgumentType.ColumnOrScalar;

                    argumentWrap.Columns.Add(new ArgumentColumn()
                    {
                        RefereneElement = argument.Content
                    });
                }

                list.Arguments.Add(argumentWrap);
            }

            return list;
        }

        private List<OperationOutputColumnElement> PassThroughTableColumns(Argument input)
        {
            if (input.ArgumentType != ArgumentType.Table)
            {
                throw new InvalidOperationException("Only table argument columns can be passed through");
            }

            List<OperationOutputColumnElement> res = new List<OperationOutputColumnElement>();
            foreach (var column in input.Columns)
            {
                if (column.RefereneElement is OperationOutputColumnElement)
                {
                    res.Add(PassThroughOutputColumn((OperationOutputColumnElement)(column.RefereneElement)));
                }
            }
            return res;
        }

        private OperationOutputColumnElement PassThroughOutputColumn(OperationOutputColumnElement inputColumn)
        {
            // lower DF link (input column -> output column)
            var columnName = inputColumn.Caption;
            var outColumn = AddOutputColumn(columnName);
            var linkCount = DataFlowLinks.Count();
            var refPath = RefPath.NamedChild("DataFlowLink", string.Format("No_{0}", linkCount + 1));
            DataFlowLinkElement daxDataFlowLinkElement = new DataFlowLinkElement(refPath, string.Format("DataFlowLink {0}", linkCount + 1), null, this);
            this.AddChild(daxDataFlowLinkElement);
            daxDataFlowLinkElement.Parent = this;
            daxDataFlowLinkElement.Source = inputColumn;
            daxDataFlowLinkElement.Target = outColumn;

            //// upper DF link (input column -> self)
            //var refPathUpper = RefPath.NamedChild("DataFlowLink", string.Format("No_{0}_Upper", linkCount + 1));
            //DaxDataFlowLinkElement daxDataFlowLinkElementUpper = new DaxDataFlowLinkElement(refPathUpper, string.Format("DataFlowLink {0} [U]", linkCount + 1), null, this);
            //this.AddChild(daxDataFlowLinkElementUpper);
            //daxDataFlowLinkElementUpper.Parent = this;
            //daxDataFlowLinkElementUpper.Source = inputColumn;
            //daxDataFlowLinkElementUpper.Target = this;

            return outColumn;
        }

        public DataFlowLinkElement AddDataFlowLink(MssqlModelElement source, OperationOutputColumnElement targetColumn)
        {
            var linkCount = DataFlowLinks.Count();
            var refPath = RefPath.NamedChild("DataFlowLink", string.Format("No_{0}", linkCount + 1));
            DataFlowLinkElement daxDataFlowLinkElement = new DataFlowLinkElement(refPath, string.Format("DataFlowLink {0}", linkCount + 1), null, this);
            this.AddChild(daxDataFlowLinkElement);
            daxDataFlowLinkElement.Parent = this;
            daxDataFlowLinkElement.Source = source;
            daxDataFlowLinkElement.Target = targetColumn;

            //// add link to self so that functions that use the table as a whole
            //var refPathUpper = RefPath.NamedChild("DataFlowLink", string.Format("No_{0}_Upper", linkCount + 1));
            //DaxDataFlowLinkElement daxDataFlowLinkElementUpper = new DaxDataFlowLinkElement(refPathUpper, string.Format("DataFlowLink {0} [U]", linkCount + 1), null, this);
            //this.AddChild(daxDataFlowLinkElementUpper);
            //daxDataFlowLinkElementUpper.Parent = this;
            //daxDataFlowLinkElementUpper.Source = source;
            //daxDataFlowLinkElementUpper.Target = this;

            return daxDataFlowLinkElement;
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

        public DataFlowLinkElement AddDataFlowLink(MModelElement source)
        {
            var linkCount = DataFlowLinks.Count();
            var refPath = RefPath.NamedChild("DataFlowLink", string.Format("No_{0}", linkCount + 1));
            DataFlowLinkElement daxDataFlowLinkElement = new DataFlowLinkElement(refPath, string.Format("DataFlowLink {0}", linkCount + 1), null, this);
            this.AddChild(daxDataFlowLinkElement);
            daxDataFlowLinkElement.Parent = this;
            daxDataFlowLinkElement.Source = source;
            daxDataFlowLinkElement.Target = this;
            return daxDataFlowLinkElement;
        }
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

    public class TableOperationElement : OperationElement
    {
        public TableOperationElement(RefPath refPath, string caption, string definition, MssqlModelElement parent)
            : base(refPath, caption, definition, parent) { }
        
        //public DaxDataFlowLinkElement AddDataFlowLink(SsasModelElement source, DaxTableOperationOutputColumnElement targetColumn)
        //{
        //    var linkCount = DataFlowLinks.Count();
        //    var refPath = RefPath.NamedChild("DataFlowLink", string.Format("No_{0}", linkCount + 1));
        //    DaxDataFlowLinkElement daxDataFlowLinkElement = new DaxDataFlowLinkElement(refPath, string.Format("DataFlowLink {0}", linkCount + 1), null, this);
        //    this.AddChild(daxDataFlowLinkElement);
        //    daxDataFlowLinkElement.Parent = this;
        //    daxDataFlowLinkElement.Source = source;
        //    daxDataFlowLinkElement.Target = targetColumn;
            
        //    // add link to self so that functions that use the table as a whole
        //    var refPathUpper = RefPath.NamedChild("DataFlowLink", string.Format("No_{0}_Upper", linkCount + 1));
        //    DaxDataFlowLinkElement daxDataFlowLinkElementUpper = new DaxDataFlowLinkElement(refPathUpper, string.Format("DataFlowLink {0} [U]", linkCount + 1), null, this);
        //    this.AddChild(daxDataFlowLinkElementUpper);
        //    daxDataFlowLinkElementUpper.Parent = this;
        //    daxDataFlowLinkElementUpper.Source = source;
        //    daxDataFlowLinkElementUpper.Target = this;

        //    return daxDataFlowLinkElement;
        //}
    }
    
    public abstract class ScalarFunctionElement : ScalarOperationElement
    {
        public ScalarFunctionElement(RefPath refPath, string caption, string definition, MssqlModelElement parent)
            : base(refPath, caption, definition, parent) { }

    }

    public class GeneralScalarFunctionElement : ScalarFunctionElement
    {
        public GeneralScalarFunctionElement(RefPath refPath, string caption, string definition, MssqlModelElement parent)
            : base(refPath, caption, definition, parent) { }

        public override void CreateDataFlowLinksAndOutputColumns()
        {
            var arguments = CollectArgumentList();
            foreach (var argument in arguments.Arguments)
            {
                AddDataFlowLink(argument.FragmentElement);
            }
        }
    }

    public abstract class TableFunctionElement : TableOperationElement
    {
        public TableFunctionElement(RefPath refPath, string caption, string definition, MssqlModelElement parent)
            : base(refPath, caption, definition, parent) { }
    }

    public class UnknownDaxTableFunctionElement : TableFunctionElement
    {
        public UnknownDaxTableFunctionElement(RefPath refPath, string caption, string definition, MssqlModelElement parent)
            : base(refPath, caption, definition, parent) { }
    }

}

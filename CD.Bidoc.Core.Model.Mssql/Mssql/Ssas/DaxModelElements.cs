using CD.DLS.Model.Interfaces;
using CD.DLS.Model.Mssql.Ssis;
using CD.DLS.Model.Mssql.Tabular;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Db = CD.DLS.Model.Mssql.Db;

namespace CD.DLS.Model.Mssql.Ssas
{
    public abstract class DaxElement : SsasModelElement
    {
        public DaxElement(RefPath refPath, string caption, string definition, MssqlModelElement parent)
            : base(refPath, caption, definition, parent)
        { }
        
    }

    [DataContract]
    public class DaxFragmentElement : DaxElement
    {
        public DaxFragmentElement(RefPath refPath, string caption, string definition, MssqlModelElement parent)
            : base(refPath, caption, definition, parent) {

            if (parent != null)
            {
                if (parent.RefPath.Path == "SSRSServer[@Name='DWH-SERVER']/Folder[@Name='/']/Folder[@Name='/TabularReports']/Report[@Name='TabularReport_LS1']/DataSet[@Name='DataSet1']/DaxScript[@Name='No_1']/FunctionCall[@Name='No_1']/Argument[@Name='No_1']")
                {

                }
            }
        }

        [DataMember]
        public int OffsetFrom { get; set; }
        [DataMember]
        public int Length { get; set; }
    }

    public class DaxScriptElement : DaxFragmentElement
    {
        public DaxScriptElement(RefPath refPath, string caption, string definition, MssqlModelElement parent)
            : base(refPath, caption, definition, parent) { }
        
    }

    public class DaxDataFlowLinkElement : DaxElement
    {
        public DaxDataFlowLinkElement(RefPath refPath, string caption, string definition, MssqlModelElement parent)
            : base(refPath, caption, definition, parent) { }
        
        [ModelLink]
        public SsasModelElement Source { get; set; }
        [ModelLink]
        public DaxElement Target { get; set; }

        public override string ToString()
        {
            return Source.RefPath.Path + " -> " + Target.RefPath.Path;
        }
    }

    public class DaxLiteralElement : DaxFragmentElement
    {
        public DaxLiteralElement(RefPath refPath, string caption, string definition, MssqlModelElement parent)
            : base(refPath, caption, definition, parent) { }
    }

    public class DaxTableReferenceElement : DaxFragmentElement
    {
        public DaxTableReferenceElement(RefPath refPath, string caption, string definition, MssqlModelElement parent)
            : base(refPath, caption, definition, parent) { }
    }

    public class DaxColumnReferenceElement : DaxFragmentElement
    {
        public DaxColumnReferenceElement(RefPath refPath, string caption, string definition, MssqlModelElement parent)
            : base(refPath, caption, definition, parent) { }

        [DataMember]
        public string TableName { get; set; }
        [DataMember]
        public string ColumnName { get; set; }
    }

    public class DaxOperationArgumentElement : DaxFragmentElement
    {
        public DaxOperationArgumentElement(RefPath refPath, string caption, string definition, MssqlModelElement parent)
            : base(refPath, caption, definition, parent) {

            if (parent is DaxFragmentElement)
            {
                OffsetFrom = ((DaxFragmentElement)parent).OffsetFrom;
                Length = ((DaxFragmentElement)parent).Length;
            }
        }
        
        public DaxFragmentElement Content { get { return ChildrenOfType<DaxFragmentElement>().FirstOrDefault(); } }
    }
    
    public abstract class DaxOperationElement : DaxFragmentElement
    {
        public DaxOperationElement(RefPath refPath, string caption, string definition, MssqlModelElement parent)
            : base(refPath, caption, definition, parent) { }

        [DataMember]
        public string Operator { get; set; }

        public IEnumerable<DaxOperationArgumentElement> Arguments { get { return ChildrenOfType<DaxOperationArgumentElement>().OrderBy(x => x.OffsetFrom); } }
        public IEnumerable<DaxDataFlowLinkElement> DataFlowLinks { get { return ChildrenOfType<DaxDataFlowLinkElement>(); } }
        
        public virtual void CreateDataFlowLinksAndOutputColumns()
        {
        }

        public IEnumerable<DaxTableOperationOutputColumnElement> OutputColumns
        {
            get { return ChildrenOfType<DaxTableOperationOutputColumnElement>(); }
        }

        protected DaxTableOperationOutputColumnElement AddOutputColumn(string name)
        {
            var refPath = RefPath.NamedChild("OutputColumn", name);
            var columnElement = new DaxTableOperationOutputColumnElement(refPath, name, null, this);
            this.AddChild(columnElement);
            return columnElement;
        }
        
        public DaxArgumentList CollectArgumentList()
        {
            DaxArgumentList list = new DaxArgumentList();
            list.Arguments = new List<DaxArgument>();

            foreach (var argument in Arguments)
            {
                var argumentWrap = new DaxArgument()
                {
                    Columns = new List<DaxArgumentColumn>(),
                    FragmentElement = argument.Content
                };

                if (argument.Content is DaxTableOperationElement || (argument.Content is DaxExpressionEvaluationFunctionElement
                    && ((DaxExpressionEvaluationFunctionElement)argument.Content).HasTableOutput))
                {
                    argumentWrap.ArgumentType = DaxArgumentType.Table;

                    var operation = (DaxOperationElement)(argument.Content);

                    foreach (var column in operation.OutputColumns)
                    {
                        argumentWrap.Columns.Add(new DaxArgumentColumn()
                        {
                            Name = column.Caption,
                            RefereneElement = column
                        });
                    }
                }
                else if (argument.Content is DaxTableReferenceElement)
                {
                    argumentWrap.ArgumentType = DaxArgumentType.Table;

                    var baseTable = argument.Content.Reference as SsasTabularTableElement;
                    var tableOperation = argument.Content.Reference as DaxTableOperationElement;

                    if (baseTable != null)
                    {
                        foreach (var column in baseTable.Columns)
                        {
                            argumentWrap.Columns.Add(new DaxArgumentColumn()
                            {
                                Name = column.Caption,
                                RefereneElement = column
                            });
                        }
                    }
                    else if (tableOperation != null)
                    {
                        foreach (var column in tableOperation.OutputColumns)
                        {
                            argumentWrap.Columns.Add(new DaxArgumentColumn()
                            {
                                Name = column.Caption,
                                RefereneElement = column
                            });
                        }
                    }
                }
                // scalar
                else
                {
                    argumentWrap.ArgumentType = DaxArgumentType.ColumnOrScalar;

                    argumentWrap.Columns.Add(new DaxArgumentColumn()
                    {
                        RefereneElement = argument.Content
                    });
                }

                list.Arguments.Add(argumentWrap);
            }

            return list;
        }
    }

    public abstract class DaxExpressionEvaluationFunctionElement : DaxOperationElement
    {
        public DaxExpressionEvaluationFunctionElement(RefPath refPath, string caption, string definition, MssqlModelElement parent)
            : base(refPath, caption, definition, parent) { }

        [DataMember]
        public bool HasTableOutput { get; set; }
        
        private DaxTableOperationOutputColumnElement PassOutputColumn(SsasModelElement inputColumn)
        {
            // lower DF link (input column -> output column)
            var columnName = inputColumn.Caption;
            if (inputColumn is DaxColumnReferenceElement)
            {
                columnName = ((DaxColumnReferenceElement)inputColumn).ColumnName;
            }
            var outColumn = AddOutputColumn(columnName);
            var linkCount = DataFlowLinks.Count();
            var refPath = RefPath.NamedChild("DataFlowLink", string.Format("No_{0}", linkCount + 1));
            DaxDataFlowLinkElement daxDataFlowLinkElement = new DaxDataFlowLinkElement(refPath, string.Format("DataFlowLink {0}", linkCount + 1), null, this);
            this.AddChild(daxDataFlowLinkElement);
            daxDataFlowLinkElement.Parent = this;
            daxDataFlowLinkElement.Source = inputColumn;
            daxDataFlowLinkElement.Target = outColumn;

            // upper DF link (input column -> self)
            var refPathUpper = RefPath.NamedChild("DataFlowLink", string.Format("No_{0}_Upper", linkCount + 1));
            DaxDataFlowLinkElement daxDataFlowLinkElementUpper = new DaxDataFlowLinkElement(refPathUpper, string.Format("DataFlowLink {0} [U]", linkCount + 1), null, this);
            this.AddChild(daxDataFlowLinkElementUpper);
            daxDataFlowLinkElementUpper.Parent = this;
            daxDataFlowLinkElementUpper.Source = inputColumn;
            daxDataFlowLinkElementUpper.Target = this;

            return outColumn;
        }

        public void SetSourceExpression(DaxElement source)
        {
            if (source is DaxTableOperationElement || source is DaxExpressionEvaluationFunctionElement && ((DaxExpressionEvaluationFunctionElement)source).HasTableOutput)
            {
                HasTableOutput = true;

                foreach (var column in ((DaxOperationElement)source).OutputColumns)
                {
                    PassOutputColumn(column);
                }
            }
            else if (source is DaxTableReferenceElement)
            {
                HasTableOutput = true;

                var baseTable = source.Reference as SsasTabularTableElement;
                var tableOperation = source.Reference as DaxOperationElement;

                if (baseTable != null)
                {
                    foreach (var column in baseTable.Columns)
                    {
                        PassOutputColumn(column);
                    }
                }
                else if (tableOperation != null)
                {
                    foreach (var column in tableOperation.OutputColumns)
                    {
                        PassOutputColumn(column);
                    }
                }
            }
            else if (source is DaxColumnReferenceElement)
            {
                HasTableOutput = true;

                PassOutputColumn((DaxColumnReferenceElement)source);
            }
            // scalar
            else
            {
                HasTableOutput = false;

                var linkCount = DataFlowLinks.Count();
                var refPath = RefPath.NamedChild("DataFlowLink", string.Format("No_{0}", linkCount + 1));
                DaxDataFlowLinkElement daxDataFlowLinkElement = new DaxDataFlowLinkElement(refPath, string.Format("DataFlowLink {0}", linkCount + 1), null, this);
                this.AddChild(daxDataFlowLinkElement);
                daxDataFlowLinkElement.Parent = this;
                daxDataFlowLinkElement.Source = source;
                daxDataFlowLinkElement.Target = this;

            }

        }

        public override void CreateDataFlowLinksAndOutputColumns()
        {
            var arguments = CollectArgumentList();
            if (arguments.Count > 0)
            {
                var sourceExpression = arguments[0].FragmentElement;
                SetSourceExpression(sourceExpression);
            }
        }
    }

    public class DaxScalarOperationElement : DaxOperationElement
    {
        public DaxScalarOperationElement(RefPath refPath, string caption, string definition, MssqlModelElement parent)
            : base(refPath, caption, definition, parent) { }

        public DaxDataFlowLinkElement AddDataFlowLink(DaxElement source)
        {
            var linkCount = DataFlowLinks.Count();
            var refPath = RefPath.NamedChild("DataFlowLink", string.Format("No_{0}", linkCount + 1));
            DaxDataFlowLinkElement daxDataFlowLinkElement = new DaxDataFlowLinkElement(refPath, string.Format("DataFlowLink {0}", linkCount + 1), null, this);
            this.AddChild(daxDataFlowLinkElement);
            daxDataFlowLinkElement.Parent = this;
            daxDataFlowLinkElement.Source = source;
            daxDataFlowLinkElement.Target = this;
            return daxDataFlowLinkElement;
        }
    }

    public class DaxTableOperationOutputColumnElement : DaxFragmentElement // DaxElement
    {
        public DaxTableOperationOutputColumnElement(RefPath refPath, string caption, string definition, MssqlModelElement parent)
            : base(refPath, caption, definition, parent) {

            if (parent is DaxFragmentElement)
            {
                OffsetFrom = ((DaxFragmentElement)parent).OffsetFrom;
                Length = ((DaxFragmentElement)parent).Length;
            }
        }
    }

    public class DaxTableOperationElement : DaxOperationElement
    {
        public DaxTableOperationElement(RefPath refPath, string caption, string definition, MssqlModelElement parent)
            : base(refPath, caption, definition, parent) { }
        
        public DaxDataFlowLinkElement AddDataFlowLink(SsasModelElement source, DaxTableOperationOutputColumnElement targetColumn)
        {
            var linkCount = DataFlowLinks.Count();
            var refPath = RefPath.NamedChild("DataFlowLink", string.Format("No_{0}", linkCount + 1));
            DaxDataFlowLinkElement daxDataFlowLinkElement = new DaxDataFlowLinkElement(refPath, string.Format("DataFlowLink {0}", linkCount + 1), null, this);
            this.AddChild(daxDataFlowLinkElement);
            daxDataFlowLinkElement.Parent = this;
            daxDataFlowLinkElement.Source = source;
            daxDataFlowLinkElement.Target = targetColumn;
            
            // add link to self so that functions that use the table as a whole
            var refPathUpper = RefPath.NamedChild("DataFlowLink", string.Format("No_{0}_Upper", linkCount + 1));
            DaxDataFlowLinkElement daxDataFlowLinkElementUpper = new DaxDataFlowLinkElement(refPathUpper, string.Format("DataFlowLink {0} [U]", linkCount + 1), null, this);
            this.AddChild(daxDataFlowLinkElementUpper);
            daxDataFlowLinkElementUpper.Parent = this;
            daxDataFlowLinkElementUpper.Source = source;
            daxDataFlowLinkElementUpper.Target = this;

            return daxDataFlowLinkElement;
        }
    }
    
    public class DaxUnaryScalarOperationElement : DaxScalarOperationElement
    {
        public DaxUnaryScalarOperationElement(RefPath refPath, string caption, string definition, MssqlModelElement parent)
            : base(refPath, caption, definition, parent) { }
    }

    public class DaxBinaryScalarOperationElement : DaxScalarOperationElement
    {
        public DaxBinaryScalarOperationElement(RefPath refPath, string caption, string definition, MssqlModelElement parent)
            : base(refPath, caption, definition, parent) { }       
    }
    
    public abstract class DaxScalarFunctionElement : DaxScalarOperationElement
    {
        public DaxScalarFunctionElement(RefPath refPath, string caption, string definition, MssqlModelElement parent)
            : base(refPath, caption, definition, parent) { }

    }

    public class GeneralDaxScalarFunctionElement : DaxScalarFunctionElement
    {
        public GeneralDaxScalarFunctionElement(RefPath refPath, string caption, string definition, MssqlModelElement parent)
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

    public abstract class DaxTableFunctionElement : DaxTableOperationElement
    {
        public DaxTableFunctionElement(RefPath refPath, string caption, string definition, MssqlModelElement parent)
            : base(refPath, caption, definition, parent) { }
    }

    public class UnknownDaxTableFunctionElement : DaxTableFunctionElement
    {
        public UnknownDaxTableFunctionElement(RefPath refPath, string caption, string definition, MssqlModelElement parent)
            : base(refPath, caption, definition, parent) { }
    }

    public class DaxLocalMeasureElement : DaxFragmentElement
    {
        public DaxLocalMeasureElement(RefPath refPath, string caption, string definition, MssqlModelElement parent)
                : base(refPath, caption, definition, parent)
        { }
    }

    public class DaxLocalVariableElement : DaxFragmentElement
    {
        public DaxLocalVariableElement(RefPath refPath, string caption, string definition, MssqlModelElement parent)
                : base(refPath, caption, definition, parent)
        { }
    }
}

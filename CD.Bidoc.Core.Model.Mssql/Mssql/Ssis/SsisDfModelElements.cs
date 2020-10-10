using CD.DLS.Model.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace CD.DLS.Model.Mssql.Ssis
{
    /// <summary>
    /// A Dataflow task, may contain DF components and paths (in the inner node)
    /// </summary>
    public class DfTaskElement : TaskElement
    {
        public DfTaskElement(RefPath refPath, string caption, string definition, SsisModelElement parent)
                : base(refPath, caption, definition, parent)
        { }

        public DfInnerElement InnerContent { get { return _children.FirstOrDefault(x => x is DfInnerElement) as DfInnerElement; } }
    }

    public class DfInnerElement : TaskElement
    {
        public DfInnerElement(RefPath refPath, string caption, string definition, SsisModelElement parent)
                : base(refPath, caption, definition, parent)
        { }
    }

    /// <summary>
    /// A dataflow component
    /// </summary>
    public class DfComponentElement : DesignBlockElement
    {
        public DfComponentElement(RefPath refPath, string caption, string definition, SsisModelElement parent)
                : base(refPath, caption, definition, parent)
        {
        }

        public IEnumerable<DfInputElement> Inputs { get { return ChildrenOfType<DfInputElement>(); } }
        public IEnumerable<DfOutputElement> Outputs { get { return ChildrenOfType<DfOutputElement>(); } }

        //public IEnumerable<DfInputOutputElement> OutputColumns
        //{ get { return _children.Where(x => x is DfInputOutputElement).Select(x => x as DfInputOutputElement); } }
    }

    public enum DfInputTypeEnum
    {
        Input, MultiInput, LeftInput, RightInput
    }

    public enum DfOutputTypeEnum
    {
        Output, ErrorOutput, MultiOutput
    }


    [DataContract]
    public class DfInputElement : SsisModelElement
    {
        public DfInputElement(RefPath refPath, string caption, string definition, SsisModelElement parent)
                : base(refPath, caption, definition, parent)
        { }

        [DataMember]
        public DfInputTypeEnum InputType { get; set; }
        [DataMember]
        public string Name { get; set; }

        public IEnumerable<DfColumnElement> Columns
        { get { return _children.Where(x => x is DfColumnElement).Select(x => x as DfColumnElement); } }
    }

    [DataContract]
    public class DfOutputElement : SsisModelElement
    {
        public DfOutputElement(RefPath refPath, string caption, string definition, SsisModelElement parent)
                : base(refPath, caption, definition, parent)
        { }

        [DataMember]
        public DfOutputTypeEnum OutputType { get; set; }
        [DataMember]
        public string Name { get; set; }

        public IEnumerable<DfColumnElement> Columns
        { get { return _children.Where(x => x is DfColumnElement).Select(x => x as DfColumnElement); } }
    }


    /// <summary>
    /// A dataflow source
    /// </summary>
    [DataContract]
    public class DfSourceElement : DfComponentElement
    {
        [DataMember]
        public string Command { get; set; }
        [DataMember]
        public string OpenRowset { get; set; }

        [ModelLink]
        public ConnectionManagerElement SourceConnection { get; set; }

        [DataMember]
        public bool IsExternalSource { get; set; }
        
        public DfSourceElement(RefPath refPath, string caption, string definition, SsisModelElement parent)
                : base(refPath, caption, definition, parent)
        { }
    }
    

    /// <summary>
    /// A dataflow destination
    /// </summary>
    public class DfDestinationElement : DfComponentElement
    {
        public DfDestinationElement(RefPath refPath, string caption, string definition, SsisModelElement parent)
                : base(refPath, caption, definition, parent)
        { }
    }

    /// <summary>
    /// A dataflow lookup
    /// </summary>
    public class DfLookupElement : DfComponentElement
    {
        public DfLookupElement(RefPath refPath, string caption, string definition, SsisModelElement parent)
                : base(refPath, caption, definition, parent)
        {
            LookupColumns = new List<DfLookupColumnElement>();
        }

        public List<DfLookupColumnElement> LookupColumns { get; set; }
    }

    public class DfDataConversionElement : DfComponentElement
    {
        public DfDataConversionElement(RefPath refPath, string caption, string definition, SsisModelElement parent)
                : base(refPath, caption, definition, parent)
        {
        }
    }

    public class DfUnionAllElement : DfComponentElement
    {
        public DfUnionAllElement(RefPath refPath, string caption, string definition, SsisModelElement parent)
                : base(refPath, caption, definition, parent)
        {
        }
    }

    public class DfUnpivotElement : DfComponentElement
    {
        public DfUnpivotElement(RefPath refPath, string caption, string definition, SsisModelElement parent)
                : base(refPath, caption, definition, parent)
        {
        }
    }

    public class DfMergeJoinElement : DfComponentElement
    {
        public DfMergeJoinElement(RefPath refPath, string caption, string definition, SsisModelElement parent)
        : base(refPath, caption, definition, parent)
        {
        }
    }

    public class DfDerivedColumnElement : DfComponentElement
    {
        public DfDerivedColumnElement(RefPath refPath, string caption, string definition, SsisModelElement parent)
        : base(refPath, caption, definition, parent)
        {
        }
    }

    public class DfLookupColumnElement : DfColumnElement
    {
        public DfLookupColumnElement(RefPath refPath, string caption, string definition, SsisModelElement parent)
            : base(refPath, caption, definition, parent)
        { }

        [ModelLink]
        public DfColumnElement InputColumn { get; set; }
    }

    public class DfLookupOutputJoinReferenceElement : SsisModelElement
    {
        public DfLookupOutputJoinReferenceElement(RefPath refPath, string caption, string definition, SsisModelElement parent)
            : base(refPath, caption, definition, parent)
        { }

        [ModelLink]
        public DfColumnElement InputJoinColumn { get; set; }

        [ModelLink]
        public DfColumnElement OutputColumn { get; set; }
    }


    public class DfUnpivotSourceReferenceElement : DfColumnElement
    {
        public DfUnpivotSourceReferenceElement(RefPath refPath, string caption, string definition, SsisModelElement parent)
            : base(refPath, caption, definition, parent)
        { }

        [ModelLink]
        public DfColumnElement TargetValueColumn { get; set; }

        [ModelLink]
        public DfColumnElement TargetPivotKeyColumn { get; set; }
    }

    /// <summary>
    /// A dataflow path connecting two components
    /// </summary>
    [DataContract]
    public class DfPathElement : DfComponentElement
    {
        public DfPathElement(RefPath refPath, string caption, string definition, SsisModelElement parent)
                : base(refPath, caption, definition, parent)
        { }

        [ModelLink]
        public DfOutputElement From { get; set; }
        [ModelLink]
        public DfInputElement To { get; set; }
        [DataMember]
        public DesignArrow Arrow { get; set; }
    }

    /// <summary>
    /// A dataflow column
    /// </summary>
    public class DfColumnElement : SsisModelElement
    {
        public DfColumnElement(RefPath refPath, string caption, string definition, SsisModelElement parent)
                : base(refPath, caption, definition, parent)
        { }

        [ModelLink]
        public MssqlModelElement ExternalSourceColumn { get; set; }

        [ModelLink]
        public MssqlModelElement ExternalDestinationColumn { get; set; }

        [ModelLink]
        public DfColumnElement SourceDfColumn { get; set; }

        [DataMember]
        public string DtsDataType { get; set; }
        [DataMember]
        public int Precision { get; set; }
        [DataMember]
        public int Scale { get; set; }
        [DataMember]
        public int Length { get; set; }
    }

    /// <summary>
    /// Dataflow aggregation - single output column from multiple inputs / vice versa
    /// </summary>
    public class DfColumnAggregationLinkElement : SsisModelElement
    {
        public DfColumnAggregationLinkElement(RefPath refPath, string caption, string definition, SsisModelElement parent)
                : base(refPath, caption, definition, parent)
        { }

        
        [ModelLink]
        public DfColumnElement SourceDfColumn { get; set; }

        [ModelLink]
        public DfColumnElement TargetDfColumn { get; set; }
    }
}

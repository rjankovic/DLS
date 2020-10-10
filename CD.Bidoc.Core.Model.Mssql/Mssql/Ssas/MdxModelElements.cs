using CD.DLS.Model.Interfaces;
using CD.DLS.Model.Mssql.Ssis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Db = CD.DLS.Model.Mssql.Db;

namespace CD.DLS.Model.Mssql.Ssas
{
    public abstract class MdxElement : SsasModelElement
    {
        public MdxElement(RefPath refPath, string caption, string definition, MssqlModelElement parent)
            : base(refPath, caption, definition, parent)
        { }
        
    }

    [DataContract]
    public class MdxFragmentElement : MdxElement
    {
        public MdxFragmentElement(RefPath refPath, string caption, string definition, MssqlModelElement parent)
            : base(refPath, caption, definition, parent) { }

        [DataMember]
        public int OffsetFrom { get; set; }
        [DataMember]
        public int Length { get; set; }
    }
    
    /// <summary>
    /// Script root
    /// </summary>
    public class MdxScriptElement : MdxFragmentElement
    {
        public MdxScriptElement(RefPath refPath, string caption, string definition, MssqlModelElement parent)
                : base(refPath, caption, definition, parent)
        { }

        public IEnumerable<MdxStatementElement> Statements { get { return ChildrenOfType<MdxStatementElement>(); } }
    }

    public class MdxStatementElement : MdxFragmentElement
    {
        public MdxStatementElement(RefPath refPath, string caption, string definition, MssqlModelElement parent)
                : base(refPath, caption, definition, parent)
        { }
    }
    
    public abstract class CalculatedMeasureElement : MdxStatementElement
    {
        public CalculatedMeasureElement(RefPath refPath, string caption, string definition, MdxElement parent)
                : base(refPath, caption, definition, parent)
        {
        }
    }

    public class CubeCalculatedMeasureElement : CalculatedMeasureElement
    {
        public CubeCalculatedMeasureElement(RefPath refPath, string caption, string definition, MdxElement parent)
                : base(refPath, caption, definition, parent)
        {
        }
    }

    public class ReportCalculatedMeasureElement : CalculatedMeasureElement
    {
        public ReportCalculatedMeasureElement(RefPath refPath, string caption, string definition, MdxElement parent)
                : base(refPath, caption, definition, parent)
        {
        }
    }

    public class CalculatedSetElement : MdxStatementElement
    {
        public CalculatedSetElement(RefPath refPath, string caption, string definition, MdxElement parent)
                : base(refPath, caption, definition, parent)
        { }
    }

    public class CalculatedMemberElement : MdxStatementElement
    {
        public CalculatedMemberElement(RefPath refPath, string caption, string definition, MdxElement parent)
                : base(refPath, caption, definition, parent)
        { }
    }

    public class ScopeStatementElement : MdxStatementElement
    {
        public ScopeStatementElement(RefPath refPath, string caption, string definition, MdxElement parent)
                : base(refPath, caption, definition, parent)
        { }
    }


}

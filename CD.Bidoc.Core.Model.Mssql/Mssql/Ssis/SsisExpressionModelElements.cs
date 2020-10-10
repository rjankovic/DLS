using CD.DLS.Model.Interfaces;
using CD.DLS.Model.Mssql.Db;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace CD.DLS.Model.Mssql.Ssis
{
    /// <summary>
    /// A fragment of an SSIS expression.
    /// </summary>
    [DataContract]
    public class SsisExpressionFragmentElement : SsisModelElement, IScriptFragmentModelElement
    {
        public SsisExpressionFragmentElement(RefPath refPath, string caption, string definition, SsisModelElement parent)
            : base(refPath, caption, definition, parent) { }

        [DataMember]
        public int OffsetFrom { get; set; }
        [DataMember]
        public int Length { get; set; }

        IModelElement IScriptFragmentModelElement.Reference
        {
            get
            {
                return Reference;
            }
        }

        IEnumerable<IScriptFragmentModelElement> IScriptFragmentModelElement.Children
        {
            get
            {
                return Children.Cast<SsisExpressionFragmentElement>();
            }
        }
    }
}

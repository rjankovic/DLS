using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CD.DLS.DAL.Objects.BusinessDictionaryIndex
{
    public class ProjectDictionaryFieldMappingItem
    {
        // f.FieldId, f.FieldName, vf.FieldOrder, REPLACE(v.ViewName, N'Type_', N'') ElementType, v.AnnotationViewId
        public int FieldId { get; set; }
        public string FieldName { get; set; }
        public int FieldOrder { get; set; }
        public string ElementType { get; set; }
        public int AnnotationViewId { get; set; }
    }
}

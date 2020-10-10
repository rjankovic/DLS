using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CD.DLS.DAL.Objects
{
    public class FulltextSearchResult
    {
        public int ModelElementId { get; set; }
        public string ElementName { get; set; }
        public string TypeDescription { get; set; }
        public string DescriptiveRootPath { get; set; }
        public string BusinessName { get; set; }
        public string BusinessFields { get; set; }
        //public int ResultPriority { get; set; }

        public string Caption { get { return string.IsNullOrWhiteSpace(BusinessFields) ? string.Format("{0} [{1}]", TypeDescription, ElementName) : string.Format("{2}: {0} [{1}]", BusinessName, ElementName, TypeDescription); } }
        
    }

    public class SearchRootElement
    {
        public int ModelElementId { get; set; }
        public string Caption { get; set; }
        public string ElementType { get; set; }
        public string RefPath { get; set; }
    }

    public class SearchParentChildTypeMapping
    {
        public string ParentType { get; set; }
        public string ChildType { get; set; }
        public string ChildTypeDescription { get; set; }
    }
    
}


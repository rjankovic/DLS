using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CD.DLS.DAL.Objects.BusinessDictionaryIndex
{
    public class ProjectOlapMeasuresLookupItem
    {
        // e.RefPath, e.ModelElementId MeasureElementId, e.Caption MeasureName
        public string RefPath { get; set; }
        public int MeasureElementId { get; set; }
        public string MeasureName { get; set; }
        public string ElementType { get; set; }
    }
}

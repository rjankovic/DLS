using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CD.DLS.DAL.Objects.BusinessDictionaryIndex
{
    public class ProjectOlapAttributesLookupItem
    {
        // e.RefPath, e.ModelElementId CubeAttributeElementId, da.ModelElementId DimensionAttributeElementId,
        // e.Caption AttributeName, cde.Caption CubeDimensionName, dde.Caption DatabaseDimensionName, dhe.Caption HierarchyName, hle.Caption HierarchyLevelName
        public string RefPath { get; set; }
        public int CubeAttributeElementId { get; set; }
        public int DimensionAttributeElementId { get; set; }
        public string AttributeName { get; set; }
        public string CubeDimensionName { get; set; }
        public string DatabaseDimensionName { get; set; }
        public string HierarchyName { get; set; }
        public string HierarchyLevelName { get; set; }
        public string ElementType { get; set; }
    }
}

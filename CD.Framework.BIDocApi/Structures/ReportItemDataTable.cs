using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CD.DLS.API.Structures
{
    public class ReportItemDataTable
    {
        public string ReportItemName { get; set; }
        public string ReportItemRefPath { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        /// <summary>
        /// Data rows
        /// </summary>
        public List<List<string>> Values { get; set; }
        public List<List<int>> NodeIds { get; set; }
    }
}

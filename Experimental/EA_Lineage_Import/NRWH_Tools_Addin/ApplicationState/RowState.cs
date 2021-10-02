using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NRWH_Tools_Addin.ApplicationState
{
    class RowState
    {
        public int Id { get; set; }
        public List<string> Values { get; set; }
        public int RowOffset { get; set; }

        public string ModifiedBy { get; set; }

        public DateTimeOffset ModifiedDate { get; set; }

        /// <summary>
        /// Changed in the worksheet
        /// </summary>
        public bool ChangedInXls { get; set; }
        public bool ChangedInEa { get; set; }

        public bool Deleted { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CD.DLS.Model.Mssql.Ssas
{

    public class DaxArgumentList
    {
        public List<DaxArgument> Arguments { get; set; }

        public DaxArgument this[int idx]
        {
            get { return Arguments[idx]; }
            set { Arguments[idx] = value; }
        }

        public int Count { get { return Arguments.Count; } }
    }

    public enum DaxArgumentType { ColumnOrScalar = 1, Table = 2 };

    public class DaxArgument
    {
        public List<DaxArgumentColumn> Columns { get; set; }
        public DaxArgumentColumn ScalarValue { get { return Columns.FirstOrDefault(); } }
        public DaxFragmentElement FragmentElement { get; set; }
        public DaxArgumentType ArgumentType { get; set; }

        public DaxArgumentColumn this[int idx]
        {
            get { return Columns[idx]; }
            set { Columns[idx] = value; }
        }
    }

    public class DaxArgumentColumn
    {
        public const string DEFAULT_NAME = "DEFAULT";
        public string Name { get; set; }
        public SsasModelElement RefereneElement { get; set; }

        public DaxArgumentColumn()
        {
            Name = DEFAULT_NAME;
        }
    }

}

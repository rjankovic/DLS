using CD.DLS.Model.Mssql;
using CD.DLS.Model.Mssql.PowerQuery;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CD.BIDoc.Core.Parse.Mssql.PowerQuery
{
    public class ArgumentList
    {
        public List<Argument> Arguments { get; set; }

        public Argument this[int idx]
        {
            get { return Arguments[idx]; }
            set { Arguments[idx] = value; }
        }

        public int Count { get { return Arguments.Count; } }
    }

    public enum ArgumentType { ColumnOrScalar = 1, Table = 2, List = 3 /*, Record = 4*/ };

    public class Argument
    {
        public List<ArgumentColumn> Columns { get; set; }
        public ArgumentColumn ScalarValue { get { return Columns.FirstOrDefault(); } }
        public MFragmentElement FragmentElement { get; set; }
        public ArgumentType ArgumentType { get; set; }

        public ArgumentColumn this[int idx]
        {
            get { return Columns[idx]; }
            set { Columns[idx] = value; }
        }
    }

    public class ArgumentColumn
    {
        public const string DEFAULT_NAME = "DEFAULT";
        public string Name { get; set; }
        public MssqlModelElement RefereneElement { get; set; }

        public ArgumentColumn()
        {
            Name = DEFAULT_NAME;
        }
    }
}

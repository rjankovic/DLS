using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CD.DLS.Parse.Mssql
{
    /// <summary>
    /// An interval in the script token stream.
    /// </summary>
    public class ScriptSpan
    {
        /// <summary>
        /// Position in the script - int.min + 1 for DB objects, int.min for server objects 
        /// </summary>
        public int TokenFrom
        {
            get
            {
                return _tokenFrom;
            }
        }

        /// <summary>
        /// Position in the script - int.max - 1 for DB ojects, int.max for server objects
        /// </summary>
        public int TokenTo
        {
            get
            {
                return _tokenTo;
            }
        }

        /// <summary>
        /// Is this a schema object?
        /// </summary>
        public bool IsDbWide
        {
            get
            {
                return _tokenFrom == int.MinValue + 1;
            }
        }

        /// <summary>
        /// Is this a server object (databases, linked servers, etc.)
        /// </summary>
        public bool IsServerWide
        {
            get

            {
                return _tokenFrom == int.MinValue;
            }
        }

        private int _tokenFrom;
        private int _tokenTo;

        /// <summary>
        /// Is the other span contained in this span?
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool Contains(ScriptSpan other)
        {
            return _tokenFrom <= other._tokenFrom && _tokenTo >= other._tokenTo;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is ScriptSpan))
            {
                return false;
            }

            var sobj = obj as ScriptSpan;
            return sobj._tokenFrom == _tokenFrom && sobj._tokenTo == _tokenTo;
        }

        /// <summary>
        /// Constructor for script-local objects
        /// </summary>
        /// <param name="frag"></param>
        public ScriptSpan(TSqlFragment frag)
        {
            _tokenFrom = frag.FirstTokenIndex;
            _tokenTo = frag.LastTokenIndex;
        }

        public ScriptSpan(int tokenFrom, int tokenTo)
        {
            this._tokenFrom = tokenFrom;
            this._tokenTo = tokenTo;
        }

        /// <summary>
        /// Constructor for schema objects
        /// </summary>
        /// <param name="dbWide"></param>
        /// <param name="serverWide"></param>
        private ScriptSpan(bool dbWide = false, bool serverWide = false)
        {
            if (dbWide)
            {
                _tokenFrom = int.MinValue + 1;
                _tokenTo = int.MaxValue - 1;
            }
            if (serverWide)
            {
                _tokenFrom = int.MinValue;
                _tokenTo = int.MaxValue;
            }
        }

        public static readonly ScriptSpan DbWide = new ScriptSpan(true, false);
        public static readonly ScriptSpan ServerWide = new ScriptSpan(false, true);
    }

}

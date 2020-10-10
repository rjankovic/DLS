using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace CD.DLS.Parse.Mssql.Db
{
    public class IdentifierComparer
    {
        private Identifier _databaseInUse;

        public IdentifierComparer(Identifier databaseInUse)
        {
            this._databaseInUse = databaseInUse;
        }

        


        private bool StringEqualCI(string a, string b)
        {
            return String.Compare(a, b,
                  StringComparison.OrdinalIgnoreCase) == 0;
        }

        public bool IdentifiersEqual(TSqlFragment a, TSqlFragment b)
        {
            if (a is Identifier)
            {
                if (b is Identifier)
                {
                    return IdentifiersEqual((Identifier)a, (Identifier)b);
                }
                else if (b is MultiPartIdentifier)
                {
                    return IdentifiersEqual((Identifier)a, (MultiPartIdentifier)b);
                }
                return false;
            }
            else if (a is MultiPartIdentifier)
            {
                if (b is Identifier)
                {
                    return IdentifiersEqual((MultiPartIdentifier)a, (Identifier)b);
                }
                else if (b is MultiPartIdentifier)
                {
                    return IdentifiersEqual((MultiPartIdentifier)a, (MultiPartIdentifier)b);
                }
                return false;
            }
            return false;
        }

        public bool IdentifiersEqual(Identifier a, Identifier b)
        {
            return StringEqualCI(a.Value, b.Value);
        }

        private bool IdentifiersEqual(MultiPartIdentifier a, Identifier b)
        {
            if (!IdentifiersEqual(a.Identifiers[a.Identifiers.Count - 1], b))
            {
                return false;
            }
            if (a.Identifiers.Count >= 2 && !StringEqualCI(a.Identifiers[a.Identifiers.Count - 2].Value, "dbo"))
            {
                return false;
            }
            if (a.Identifiers.Count >= 3 && !IdentifiersEqual(a.Identifiers[a.Identifiers.Count - 3], _databaseInUse))
            {
                return false;
            }
            return true;
        }

        private bool IdentifiersEqual(Identifier a, MultiPartIdentifier b)
        {
            return IdentifiersEqual(b, a);
        }

        public bool IdentifiersEqual(MultiPartIdentifier a, MultiPartIdentifier b)
        {
            for (int k = 0; k < Math.Min(a.Identifiers.Count, b.Identifiers.Count); k++)
            {
                if (!IdentifiersEqual(a.Identifiers[a.Identifiers.Count - 1 - k], b.Identifiers[b.Identifiers.Count - 1 - k]))
                {
                    return false;
                }
            }

            if (a.Identifiers.Count == b.Identifiers.Count)
            {
                return true;
            }

            MultiPartIdentifier longId = a;
            MultiPartIdentifier shortId = b;
            if (longId.Identifiers.Count < shortId.Identifiers.Count)
            {
                longId = b;
                shortId = a;
            }

            if (longId.Identifiers.Count >= 2 && shortId.Identifiers.Count == 1)
            {
                if (longId.Identifiers[longId.Identifiers.Count - 2].Value != "dbo")
                {
                    return false;
                }
            }
            if (longId.Identifiers.Count == 3 && shortId.Identifiers.Count <= 2)
            {
                if (!IdentifiersEqual(longId.Identifiers[0], _databaseInUse))
                {
                    return false;
                }
            }
            return true;
        }
    }
}

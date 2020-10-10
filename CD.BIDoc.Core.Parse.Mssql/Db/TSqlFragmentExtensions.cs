using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CD.DLS.Parse.Mssql.Db
{
    public static class TSqlFragmentExtensions
    {
        public static string GetText(this TSqlFragment fragment)
        {
            StringBuilder tokenText = new StringBuilder();

            if (fragment.ScriptTokenStream == null)
            {
                if (fragment is Identifier)
                {
                    return ((Identifier)fragment).Value;
                }
                else if (fragment is MultiPartIdentifier)
                {
                    bool first = true;
                    foreach (var pt in ((MultiPartIdentifier)fragment).Identifiers)
                    {
                        if (!first)
                        {
                            tokenText.Append(".");
                        }
                        tokenText.Append(pt.Value);
                        first = false;
                    }
                    return tokenText.ToString();
                }
                else throw new Exception();
            }
            for (int counter = fragment.FirstTokenIndex; counter <= fragment.LastTokenIndex; counter++)
            {
                tokenText.Append(fragment.ScriptTokenStream[counter].Text);
            }
            
            return tokenText.ToString();
        }

        public static string GetText(this TSqlFragment fragment, int startToken, int endToken)
        {
            StringBuilder tokenText = new StringBuilder();

            if (fragment.ScriptTokenStream == null)
            {
                if (fragment is Identifier)
                {
                    return ((Identifier)fragment).Value;
                }
                else if (fragment is MultiPartIdentifier)
                {
                    bool first = true;
                    foreach (var pt in ((MultiPartIdentifier)fragment).Identifiers)
                    {
                        if (!first)
                        {
                            tokenText.Append(".");
                        }
                        tokenText.Append(pt.Value);
                        first = false;
                    }
                    return tokenText.ToString();
                }
                else throw new Exception();
            }
            for (int counter = startToken; counter <= endToken; counter++)
            {
                tokenText.Append(fragment.ScriptTokenStream[counter].Text);
            }

            return tokenText.ToString();
        }

        public static string GetText(this IList<TSqlParserToken> tokenStream, int startIndex, int endIndex)
        {
            StringBuilder tokenText = new StringBuilder();
            
            for (int counter = startIndex; counter <= endIndex; counter++)
            {
                tokenText.Append(tokenStream[counter].Text);
            }

            return tokenText.ToString();
        }
    }
}

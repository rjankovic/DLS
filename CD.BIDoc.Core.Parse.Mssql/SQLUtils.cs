using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CD.DLS.Parse.Mssql
{
    /// <summary>
    /// Utility functions for SQL scripts.
    /// </summary>
    static class SQLUtils
    {
        internal static string SanitizeSqlScript(string script)
        {
            script = Regex.Replace(script, "[ \t\r\n]*", " ");
            return script;
        }

        internal static string GetNameLeftFromString(string def)
        {
            def = def.Trim();
            if (def[0] == '[')
            {
                return def.Substring(1, def.IndexOf(']') - 1);
            }
            else
            {
                return def.Substring(0, def.IndexOfAny(new char[] { ' ', '\t', '\n', '\r' }));
            }
        }

        public static string RemoveSqlParamSpecialCharacters(string str)
        {
            StringBuilder sb = new StringBuilder();
            foreach (char c in str)
            {
                if ((c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z') || c == '.' || c == '_')
                {
                    sb.Append(c);
                }
            }
            return sb.ToString();
        }

        internal static string ParametrizeSql(string query, string paramName)
        {
            bool isQuoted = false;
            bool isComment = false;
            bool isMultilineComment = false;
            int startIdx = 0;
            while (startIdx < query.Length)
            {
                var qm = query.IndexOf("?", startIdx);

                for (var p = startIdx; p < qm; p++)
                {
                    if (p > 0 && query[p - 1] == '/' && query[p] == '*')
                    {
                        isMultilineComment = true;
                        isComment = true;
                    }
                    if (p > 0 && query[p - 1] == '*' && query[p] == '/')
                    {
                        isMultilineComment = false;
                        isComment = false;
                    }
                    if (p > 0 && query[p - 1] == '-' && query[p] == '-')
                    {
                        isComment = true;
                    }
                    if (query[p] == '\n' && !isMultilineComment)
                    {
                        isComment = false;
                    }

                    if (!isComment)
                    {
                        if (query[p] == '\'')
                        {
                            isQuoted = !isQuoted;
                        }
                    }
                }
                if (qm > 0)
                {

                    if (query[qm - 1] == '\\' /*questinable*/ || isQuoted  /*questionable end*/ || isComment)
                    {
                        startIdx = qm + 1;
                        continue;
                    }
                }
                // there can be input params mapped even if not used (incorrect package (?))
                if (qm == -1)
                {
                    return query;
                }
                var qr = query.Remove(qm, 1);
                var qf = qr.Insert(qm, paramName);
                return qf;
            }
            return query;
            // or maybe
            //throw new Exception();
        }


    }
}

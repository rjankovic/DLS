using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CD.DLS.Parse.Mssql.Ssas
{
    public static class MdxHelper
    {
        public static string WrapIdentifier(string identifier, out List<string> incrementalPrefixes)
        {
            StringBuilder sb = new StringBuilder();
            bool isBrackets = false;
            int pos = 0;
            bool addEndingBrackets = false;
            incrementalPrefixes = new List<string>();
            while (pos < identifier.Length)
            {
                bool segmentStart = false;

                var ch = identifier[pos];

                if (pos == 0 || (pos > 0 && !isBrackets && identifier[pos - 1] == '.'))
                {
                    segmentStart = true;
                    if (pos > 0)
                    {
                        incrementalPrefixes.Add(sb.ToString().TrimEnd('.'));
                    }
                }
                if (!isBrackets && ch == '[')
                {
                    isBrackets = true;
                }
                if (isBrackets && ch == ']')
                {
                    isBrackets = false;
                }
                if (segmentStart && ch != '[')
                {
                    sb.Append('[');
                    addEndingBrackets = true;
                }
                if (ch == '.' && addEndingBrackets)
                {
                    sb.Append(']');
                    addEndingBrackets = false;
                }
                sb.Append(ch);
                pos++;
            }
            if (addEndingBrackets)
            {
                sb.Append(']');
            }
            return sb.ToString();
        }

        public static string WrapIdentifier(string identifier)
        {
            var dummyPrefixes = new List<string>();
            return WrapIdentifier(identifier, out dummyPrefixes);
        }
    }
}

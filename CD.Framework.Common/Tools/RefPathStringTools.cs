using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CD.DLS.Common.Tools
{
    public partial class RefPathStringTools
    {
        private string[] partsOfRefPath;
        private string srvName;
        private string dbName;
        private string schema;
        private string table;
        private string column;

        public string GetConnStringByRefPath(string refPath)
        {
            ParseRefPath(refPath);
            string ConnString = "Server=" + srvName + ";Database=" + dbName + ";Trusted_Connection=True;";
            return ConnString;
        }

        public string GetSchemaTable(string refPath)
        {
            ParseRefPath(refPath);
            string schemaTable = "[" + schema + "].[" + table + "]";
            return schemaTable;
        }

        public string GetColumn(string refPath)
        {
            ParseRefPath(refPath);
            return column;
        }

        private void ParseRefPath(string refPath)
        {
            partsOfRefPath = Regex.Split(refPath, @"\w+\[\@\w+\='");
            int c = partsOfRefPath.Count();
            int i = 1;
            while ( i < c)
            {
                partsOfRefPath[i] = Regex.Replace(partsOfRefPath[i], @"('\]\/|'\])", "");
                i++;
            }
            srvName = partsOfRefPath[1];
            dbName = partsOfRefPath[2];
            schema = partsOfRefPath[3];
            table = partsOfRefPath[4];
            if (c > 5)
            {
                column = partsOfRefPath[5];
            }
            else
            {
                column = "";
            }        
        }

    }
}

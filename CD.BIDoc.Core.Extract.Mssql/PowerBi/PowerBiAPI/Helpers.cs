
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace CD.DLS.Extract.PowerBi.PowerBiAPI
{
    public static class Helpers
    {
        public static string StreamToString(this Stream stream)
        {
            
            using (StreamReader reader = new StreamReader(stream))
            {
                string text = reader.ReadToEnd();
                return text;
            }
        }

        public static string ResponseToString(this HttpWebResponse webResponse)
        {
            using (Stream data = webResponse.GetResponseStream())
            {
                return data.StreamToString();
            }
        }

        public static string ResponseToString(this HttpResponseMessage webResponse)
        {
            using (Stream data = webResponse.Content.ReadAsStreamAsync().Result)
            {
                return data.StreamToString();
            }
        }

        public static long CopyStream(Stream input, Stream output)
        {
            byte[] buffer = new byte[32768];
            int read;
            while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
            {
                output.Write(buffer, 0, read);
            }

            return input.Length;
        }

       

        public static PBIDataType ToPBIDataType(this Type dataType)
        {
            switch(dataType.Name)
            {
                case "Boolean": return PBIDataType.Boolean;
                case "Byte": return PBIDataType.Int64;
                case "Char": return PBIDataType.String;
                case "DateTime": return PBIDataType.DateTime;
                case "Decimal": return PBIDataType.Double;
                case "Double": return PBIDataType.Double;
                case "Guid": return PBIDataType.String;
                case "Int16": return PBIDataType.Int64;
                case "Int32": return PBIDataType.Int64;
                case "Int64": return PBIDataType.Int64;
                case "SByte": return PBIDataType.Int64;
                case "Single": return PBIDataType.Int64;
                case "String": return PBIDataType.String;
                case "UInt16": return PBIDataType.Int64;
                case "UInt32": return PBIDataType.Int64;
                case "UInt64": return PBIDataType.Int64;
                default: throw new NotSupportedException(string.Format("Datatype '{0}' is not supported in Power BI!", dataType.Name));
            }
        }

       

        public static void WriteWarning(string message, params string[] args)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(message, args);
            Console.ResetColor();
        }
    }
}

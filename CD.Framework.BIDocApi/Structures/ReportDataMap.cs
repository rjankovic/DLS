using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CD.DLS.API.Structures
{
    public class ReportDataMap
    {
        public ReportElementAbsolutePosition RootElementPosition { get; set; }
        public List<ReportItemDataTable> ReportItemDataTables { get; set; }

        public string Serialize()
        {
            JsonSerializerSettings settings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All
            };
            return JsonConvert.SerializeObject(this, settings);
        }

        public static ReportDataMap Deserialize(string serialized)
        {
            JsonSerializerSettings settings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All
            };
            return JsonConvert.DeserializeObject<ReportDataMap>(serialized, settings);
        }

        public DataSet GetDataSet()
        {
            DataSet ds = new DataSet();
            foreach (var table in ReportItemDataTables)
            {
                DataTable dt = new DataTable(table.ReportItemName);
                ds.Tables.Add(dt);
                for (int i = 0; i < table.Width; i++)
                {
                    dt.Columns.Add((table.Values[0].Count < i + 1 ? "" : table.Values[0][i]) + "_" + i.ToString());
                }
                for (int i = 1; i < table.Height; i++)
                {
                    var dr = dt.NewRow();
                    for (int j = 0; j < table.Values[i].Count; j++)
                    {
                        dr[j] = table.Values[i][j];
                    }
                    dt.Rows.Add(dr);
                }
            }

            return ds;
        }
    }

}

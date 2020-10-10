using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CD.DLS.DAL.Objects.Extract
{
    public enum ExtractTypeEnum {
        SsisPackage = 1,
        SsisProjectConnectionManager = 2,
        SsisProjectsParameters = 3,
        SsisProjectFile = 4,
        SsisPackageFile = 8,
        //SsrsFile = 5,
        TabularDB = 6,
        TabularModel = 7,
        //SqlDbStructure = 8,
        //SqlDbScript = 9,
        SsasMultidimensionalDsv = 10,
        SsasMultidimensionalDimension = 11,
        SsasMultidimensionalCube = 12,
        SsasMultidimensionalDatabase = 13,

        SqlTable = 14,
        SqlView = 15,
        SqlScalarUdf = 16,
        SqlTableUdf = 17,
        SqlTableType = 18,
        SqlProcedure = 19,
        SqlSchema = 20,

        SsrsReport = 21,
        SsrsSharedDataSet = 22,
        SsrsSharedDataSource = 23,
        SsrsFolder = 24,

        Tenant = 25,
        PowerBiReport = 26,
        PowerBiConnection = 27,
        PowerBiSection = 28,
        PowerBiTable = 29,
        PowerBiVisual = 30,
        PowerBiColumn = 31,
        PowerBiFilter = 32,
        PowerBiFilterExpression = 33,
        PowerBiProjection = 34,

        NA = 100
    }

    public abstract class ExtractObject
    {
        public string Serialize()
        {
            JsonSerializerSettings settings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All
            };
            return JsonConvert.SerializeObject(this, settings);
        }

        public static ExtractObject Deserialize(string serialized)
        {
            JsonSerializerSettings settings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All
            };
            return JsonConvert.DeserializeObject<ExtractObject>(serialized, settings);
        }

        public abstract ExtractTypeEnum ExtractType { get; }
        public abstract string Name { get; }
        public int ExtractItemId { get; set; }
    }

}

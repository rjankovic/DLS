using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CD.DLS.DAL.Objects.Extract
{
    
    #region SSRS

    public enum SsrsItemTypeEnum { Report = 1, SharedDataSet = 2, SharedDataSource = 3, Folder = 4 }

    /*
            SsrsReport = 21,
        SsrsSharedDataSet = 22,
        SsrsSharedDataSource = 23,
        SsrsFolder = 24

     */

    public abstract class SsrsItem : ExtractObject
    {
        //public override ExtractTypeEnum ExtractType => ExtractTypeEnum.SsrsFile;

        public string FileName { get; set; }
        public string FullPath { get; set; }
        public string Content { get; set; }
        public string ID { get; set; }
        //public string Path { get; set; }
        //public string Name { get; set; }
        //public string Definition { get; set; }
        //public string TypeName { get; set; }
        public string DataSourceConnectionString { get; set; }
        public string DataSourceExtension { get; set; }

        public SsrsItemTypeEnum ItemType { get; set; }

        public override string Name => FileName;
    }

    public class SsrsReport : SsrsItem
    {
        public override ExtractTypeEnum ExtractType => ExtractTypeEnum.SsrsReport;
    }

    public class SsrsSharedDataSet : SsrsItem
    {
        public override ExtractTypeEnum ExtractType => ExtractTypeEnum.SsrsSharedDataSet;
    }

    public class SsrsSharedDataSource : SsrsItem
    {
        public override ExtractTypeEnum ExtractType => ExtractTypeEnum.SsrsSharedDataSource;
    }

    public class SsrsFolder : SsrsItem
    {
        public override ExtractTypeEnum ExtractType => ExtractTypeEnum.SsrsFolder;
    }

    #endregion


}

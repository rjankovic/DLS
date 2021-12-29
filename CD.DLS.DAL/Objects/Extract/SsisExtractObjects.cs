using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CD.DLS.DAL.Objects.Extract
{

    #region SSIS
    public abstract class SsisFile : ExtractObject
    {
        public string FileName { get; set; }
        public string FullPath { get; set; }
        public string Content { get; set; }

        public override string Name => FileName;
    }


    public class SsisProjectFile : SsisFile
    {
        public override ExtractTypeEnum ExtractType => ExtractTypeEnum.SsisProjectFile;
    }

    public class SsisParametersFile : SsisFile
    {
        public override ExtractTypeEnum ExtractType => ExtractTypeEnum.SsisParametersFile;
    }

    public class SsisPackageFile : SsisFile
    {
        public override ExtractTypeEnum ExtractType => ExtractTypeEnum.SsisPackageFile;
    }

    public class SsisConnectionManagerFile : SsisFile
    {
        public override ExtractTypeEnum ExtractType => ExtractTypeEnum.SsisConnectionManagerFile;
    }

    //public class SsisProjectConnectionManager : ExtractObject
    //{
    //    public override ExtractTypeEnum ExtractType => ExtractTypeEnum.SsisProjectConnectionManager;

    //    public override string Name => ConnectionManager.Name;

    //    public SsisConnectionManager ConnectionManager { get; set; }
    //}

    //public class SsisProjectParameters : ExtractObject
    //{
    //    public override ExtractTypeEnum ExtractType => ExtractTypeEnum.SsisProjectsParameters;

    //    public List<SsisParameter> Parameters { get; set; }

    //    public override string Name => "ProjectParameters";
    //}


    #endregion
    

}

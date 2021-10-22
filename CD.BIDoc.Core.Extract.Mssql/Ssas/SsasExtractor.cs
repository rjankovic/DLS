
using CD.DLS.Common.Structures;
using CD.DLS.Common.Tools;
using CD.DLS.DAL.Configuration;
using CD.DLS.DAL.Objects.Extract;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace CD.DLS.Extract.Mssql.Ssas
{
    class SsasExtractor
    {
        private SsasDbProjectComponent _ssasComponent;
        private string _outputDirPath;
        private string _relativePathBase;
        private Manifest _manifest;

        public SsasExtractor(SsasDbProjectComponent ssasProjectComponent, string outputDirPath, string relativePathBase, Manifest manifest)
        {
            _ssasComponent = ssasProjectComponent;
            _outputDirPath = outputDirPath;
            _relativePathBase = relativePathBase;
            _manifest = manifest;

        }

        public Boolean IsTabular(SsasDbProjectComponent currentProject)
        {
            var serverName = currentProject.ServerName;
            Microsoft.AnalysisServices.Server _server = new Microsoft.AnalysisServices.Server();
            _server.Connect(string.Format("Provider=MSOLAP.8;Integrated Security=SSPI;DataSource={0}", currentProject.ServerName));

            if (_server.ServerMode == Microsoft.AnalysisServices.ServerMode.Tabular)
            {
                return true;
            }

            else { return false; }

        }

        public void Extract()
        {
            var dbDirName = $"DB_{_ssasComponent.SsaslDbProjectComponentId}_{_ssasComponent.DbName}";
            dbDirName = FileTools.NormalizeFileName(dbDirName);

            _outputDirPath = Path.Combine(_outputDirPath, dbDirName);
            _relativePathBase = Path.Combine(_relativePathBase, dbDirName);

            Directory.CreateDirectory(_outputDirPath);

            ConfigManager.Log.Important(string.Format("Extracting SSAS DB {0} from {1}", _ssasComponent.DbName, _ssasComponent.ServerName));

          
            if (IsTabular(_ssasComponent))
            {
                ConfigManager.Log.Important("Tabular detected");
                var tabularExtractor = new TabularExtractor(_ssasComponent, _outputDirPath, _relativePathBase, _manifest);
                tabularExtractor.Extract();
            }
            else
            {
                ConfigManager.Log.Important("OLAP detected");
                var multidimensionalExtractor = new MultidimensionalExtractor(_ssasComponent, _outputDirPath, _relativePathBase, _manifest);
                 multidimensionalExtractor.Extract();
            }

        }
        
    }
}

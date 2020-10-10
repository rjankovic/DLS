using CD.DLS.Common.Structures;
using CD.DLS.DAL.Configuration;
using CD.DLS.DAL.Misc;
using CD.DLS.DAL.Objects.Extract;
using System;
using System.IO;
using System.Linq;

namespace CD.DLS.Extract_v12
{
    class Program
    {
        private static string _outputFolder;
        private static string _configJson;
        private static int _componentId;
        private static Guid _extractId;

        static void Main(string[] args)
        {
            try
            {
                _outputFolder = args[0];
                _configJson = File.ReadAllText(args[1]);
                _componentId = int.Parse(args[3]);
                _extractId = Guid.Parse(args[2]);

                var workDirName = _extractId.ToString();
                var workDirPath = Path.Combine(_outputFolder, workDirName);
                var extractFolder = Directory.CreateDirectory(workDirPath);
                var manifestPath = Path.Combine(workDirPath, "manifest.json");
                ManualConfigManager mcm = new ManualConfigManager();
                ConfigManager.SetCustomConfigManager(mcm);
                mcm.ApplicationClass = ApplicationClassEnum.Service;
                mcm.DeploymentMode = DeploymentModeEnum.Azure;
                mcm.Log = new ConsoleLogger("DLS Extract");

                ConfigManager.Log.Info("Extractor v12 starting");

                //ConfigManager.ApplicationClass = ApplicationClassEnum.Service;
                var manifest = Manifest.Deserialize(File.ReadAllText(manifestPath));
                var projectConfig = ProjectConfig.Deserialize(_configJson);


                var ssisComponent = projectConfig.SsisComponents.FirstOrDefault(x => x.SsisProjectComponentId == _componentId);
                var sqlComponent = projectConfig.DatabaseComponents.FirstOrDefault(x => x.MssqlDbProjectComponentId == _componentId);

                if (sqlComponent != null)
                {
                    var sqlDirName = "MSSQLDB";
                    var mssqlDbDirPath = Path.Combine(_outputFolder, workDirName, sqlDirName);
                    var sqlExtractor = new Extract_v11.Mssql.SqlDb.SqlExtractor_v11(sqlComponent, mssqlDbDirPath, sqlDirName, manifest);
                    sqlExtractor.Extract();

                }
                else if (ssisComponent != null)
                {
                    var ssisDirName = "SSIS";
                    var ssisDirPath = Path.Combine(_outputFolder, workDirName, ssisDirName);
                    var extractor = new Extract_v11.Mssql.Ssis.SsisExtractor_v11(ssisComponent, ssisDirPath, ssisDirName, manifest);
                    extractor.Extract();

                }
                else
                {
                    throw new Exception("Component " + _componentId.ToString() + " not found");
                }

                var manifestSerialized = manifest.Serialize();
                File.WriteAllText(Path.Combine(workDirPath, "manifest.json"), manifestSerialized);

            }
            catch (Exception ex)
            {
                ShowException(ex);
                //throw;
                Console.ReadLine();
            }

        }
        
        public static void ShowException(Exception ex)
        {
            ConfigManager.Log.Important(ex.Message);
            if (ex.InnerException != null)
            {
                ConfigManager.Log.Important(ex.InnerException.Message);
            }
            ConfigManager.Log.Important(ex.StackTrace);
        }

    }
}

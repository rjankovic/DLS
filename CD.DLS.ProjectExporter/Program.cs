using CD.DLS.DAL.Configuration;
using CD.DLS.DAL.Engine;
using CD.DLS.DAL.Managers;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CD.DLS.ProjectExporter
{
    class Program
    {
        static void Main(string[] args)
        {
            ManualConfigManager mcm = new ManualConfigManager();
            mcm.ApplicationClass = ApplicationClassEnum.Service;
            mcm.DeploymentMode = DeploymentModeEnum.Azure;
            ConfigManager.SetCustomConfigManager(mcm);
            
            NetBridge nb = new NetBridge(true);
            var cs = ConfigurationManager.ConnectionStrings["dlsConnection"].ConnectionString;
            nb.SetConnectionString(cs);

            var pcm = new ProjectConfigManager(nb);
            var operation = ConfigurationManager.AppSettings["operation"];
            var projectConfigId = new Guid(ConfigurationManager.AppSettings["projectConfigId"]);

            switch (operation)
            {
                case "exportProjectConfig":
                    var config = pcm.GetProjectConfig(projectConfigId);
                    var ser = config.Serialize();
                    File.WriteAllText("projectconfig.json", ser);
                    break;
                default:
                    Console.WriteLine("Operation " + operation + " not implemented");
                    Console.ReadLine();
                    break;
            }

        }
    }
}

using System;
using CD.DLS.Common.Structures;
using CD.DLS.DAL.Configuration;
using CD.DLS.DAL.Engine;
using CD.DLS.DAL.Managers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CD.DLS.Tests.ExtractTests
{
    [TestClass]
    public class ConfigSerialization
    {
        private static ProjectConfigManager _pcm;

        [ClassInitialize]
        public static void ClassInitialize(TestContext tc)
        {
            ConfigManager.SetCustomConfigManager(new StandardConfigManager());
            ManualConfigManager mcm = new ManualConfigManager();

            mcm.CustomerDatabaseConnectionString = "Data Source=DESKTOP-KUT8BE5;Initial Catalog=CDFramework;Integrated Security=True";
            mcm.ApplicationClass = ApplicationClassEnum.Service;
            mcm.DeploymentMode = DeploymentModeEnum.Azure;

            ConfigManager.SetCustomConfigManager(mcm);

            var cs = ConfigManager.CustomerDatabaseConnectionString;
            NetBridge nb = new NetBridge(true);
            nb.SetConnectionString(cs);
            _pcm = new ProjectConfigManager(nb);
            
        }


        [TestMethod]
        public void SerializeManpower()
        {
            var config = _pcm.GetProjectConfig(new Guid("7D41A042-E44D-4AD3-89E2-71B10620E4C9"));
            var ser = config.Serialize();
            var deser = ProjectConfig.Deserialize(ser);
        }
    }
}

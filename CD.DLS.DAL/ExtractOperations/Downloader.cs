using CD.DLS.DAL.Configuration;
using CD.DLS.DAL.Engine;
using CD.DLS.DAL.Managers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CD.DLS.DAL.ExtractOperations
{
    public static class Downloader
    {
        public static void DownloadConfig(Guid projectId, string targetPath)
        {
            var connectionString = ConfigManager.UploaderConnectionString;
            var nb = new NetBridge(false, false);
            nb.SetConnectionString(connectionString);

            var pcm = new ProjectConfigManager(nb);
            var projectConfig = pcm.GetProjectConfig(projectId);
            var serialized = projectConfig.Serialize();
            File.WriteAllText(targetPath, serialized);
            
        }
    }
}

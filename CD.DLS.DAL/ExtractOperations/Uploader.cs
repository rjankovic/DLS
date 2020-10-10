using CD.DLS.API;
using CD.DLS.API.ModelUpdate;
using CD.DLS.DAL.Configuration;
using CD.DLS.DAL.Engine;
using CD.DLS.DAL.Managers;
using CD.DLS.DAL.Objects.Extract;
using CD.DLS.DAL.Receiver;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace CD.DLS.DAL.ExtractOperations
{
    public class UploadEventArgs : EventArgs
    {
        public int UploadPercentage { get; set; }
    }

    public delegate void UploadEventHandler(object sender, UploadEventArgs e);

    public static class Uploader
    {
        public static event UploadEventHandler UploadProgress;
        public static event UploadEventHandler DataUploaded;

        private static StageManager _stageManager = null; //= new StageManager();
        private static int run = 0;
        private static int percentage;

        public async static Task UploadExtract(string zipPath, Receiver.IReceiver receiver, NetBridge netBridge, string customerCode)
        {
            _stageManager = new StageManager(netBridge);
            var tempPath = Path.GetTempPath();
            var dirName = Path.GetFileNameWithoutExtension(zipPath);
            var tempDir = Path.Combine(tempPath, dirName);
            if (Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, true);
            }

            Directory.CreateDirectory(tempDir);
            ConfigManager.Log.Important($"Unpackig extract {zipPath}");
            ZipFile.ExtractToDirectory(zipPath, tempDir);

            var manifestPath = Path.Combine(tempDir, "manifest.json");
            var manifestText = File.ReadAllText(manifestPath);
            var manifest = Manifest.Deserialize(manifestText);
            
            _stageManager.CreateNewExtract(manifest);

            var extractId = manifest.ExtractId;
            foreach (var manifestItem in manifest.Items)
            {
                ConfigManager.Log.Important($"Reading {manifestItem.RelativePath}");
                var filePath = Path.Combine(tempDir, manifestItem.RelativePath);
                var deser = ExtractObject.Deserialize(File.ReadAllText(filePath));               
                _stageManager.SaveExtractItem(deser, extractId, manifestItem.ComponentId);
                run = run + 1;
                percentage = (run * 100) / manifest.Items.Count();
                if (UploadProgress != null)
                {
                    UploadProgress(null, new UploadEventArgs() { UploadPercentage = percentage });
                }               
            }            
            if(DataUploaded != null)
            {
                DataUploaded(null, new UploadEventArgs());
            }

            run = 0;

            ConfigManager.Log.Important($"Sending service request");
            var requestMessage = Helpers.CreateRequest(receiver, manifest.ProjectConfig.ProjectConfigId, customerCode);
            requestMessage.MessageToObjectId = ConfigManager.ServiceReceiverId;
            requestMessage.RequestForCoreType = Common.Interfaces.CoreTypeEnum.BIDoc;
            UpdateModelRequest request = new UpdateModelRequest()
            {
                ExtractId = manifest.ExtractId
            };
            requestMessage.Content = request.Serialize();
            await receiver.PostMessageNoResponse(requestMessage);                      
        }
    }
}

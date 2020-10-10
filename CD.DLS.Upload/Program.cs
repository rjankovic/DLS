using CD.DLS.Common.Storage.FileSystem;
using CD.DLS.DAL.Configuration;
using CD.DLS.DAL.Engine;
using CD.DLS.DAL.Misc;
using CD.DLS.DAL.Receiver;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace CD.DLS.Upload
{
    class Program
    {
        private static string[] _args;
        private static IReceiver _receiver;
        private static NetBridge _netBridge;
        private static string _customerCode;

        static void Main(string[] args)
        {
            try
            {
                ConfigManager.ApplicationClass = ApplicationClassEnum.Daemon;
                _args = args;

                _netBridge = new NetBridge(true, false);
                _netBridge.SetConnectionString(ConfigManager.UploaderConnectionString);
                ConfigManager.Log = new DbLogger(new DAL.Managers.LogManager(_netBridge));
                
                _customerCode = args[1];
                _receiver = new HttpReceiver(_customerCode);
                
                var mode = args[0];
                //if (ConfigManager.ApplicationClass == ApplicationClassEnum.Client)
                //{
                //    ConfigManager.Log = new ConsoleLogger("DLS Uploader");
                //}
                //else
                //{
                //    ConfigManager.ApplicationClass = ApplicationClassEnum.Daemon;
                //}

                switch (mode)
                {
                    case "up":
                        var task1 = UploadExtract();
                        task1.Wait();
                        break;
                    case "upx":
                        var task = FindUploadClear();
                        task.Wait();
                        break;
                    case "cfg":
                        DownloadConfig();
                        break;
                    default:
                        throw new Exception($"Unrecognized command {mode}");
                }
            }
            catch (Exception ex)
            {
                ShowException(ex);
                throw;
            }
            
        }

        private static void DownloadConfig()
        {
            var projectId = Guid.Parse(_args[2]);
            var targetPath = _args[3];
            DAL.ExtractOperations.Downloader.DownloadConfig(projectId, targetPath);
        }

        private async static Task UploadExtract()
        {
            string zipPath = _args[2];

            await DAL.ExtractOperations.Uploader.UploadExtract(zipPath, _receiver, _netBridge, _customerCode);
        }

        private static Task FindUploadClear()
        {
            var folderPath = _args[2];

            ConfigManager.Log.Info(string.Format("Looking for zip uploads in {0}", folderPath));
            var dir = new DirectoryInfo(folderPath);
            var files = dir.GetFiles("*.zip");
            if (files.Length == 0)
            {
                throw new Exception(string.Format("Could not find any ZIP files in {0}", folderPath));
            }

            var zip = files.OrderByDescending(x => x.CreationTime).First();
            ConfigManager.Log.Info(string.Format("Uploading file {0}", zip.FullName));
            
            var tsk = DAL.ExtractOperations.Uploader.UploadExtract(zip.FullName, _receiver, _netBridge, _customerCode);

            return tsk;
            //foreach (var zipToDelete in files)
            //{
            //    File.Delete(zipToDelete.FullName);
            //}
        }

        public static void ShowException(Exception ex)
        {
            Console.WriteLine(ex.Message);
            if (ex.InnerException != null)
            {
                Console.WriteLine(ex.InnerException.Message);
            }
            Console.WriteLine(ex.StackTrace);
        }
    }
}

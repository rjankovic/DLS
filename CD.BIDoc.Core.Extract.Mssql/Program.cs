using CD.DLS.Common.Structures;
using CD.DLS.DAL.Configuration;
using CD.DLS.DAL.Engine;
using CD.DLS.DAL.Misc;
using CD.DLS.DAL.Objects.Extract;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Windows.Forms;

namespace CD.DLS.Extract.Mssql
{
    class Program
    {
        private const string V12_PATH = "v12\\CD.DLS.Extract_v12.exe";
        private const string V11_PATH = "v11\\CD.DLS.Extract_v11.exe";
        
        private static string _outputFolder;
        private static string _configJson;
        
        static void Main(string[] args)
        {
            _outputFolder = args[0];
            _configJson = File.ReadAllText(args[1]);

            var v12FullPath = Path.Combine(Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), V12_PATH));
            var v11FullPath = Path.Combine(Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), V11_PATH));

            var projectConfig = ProjectConfig.Deserialize(_configJson);

            var manifest = new Manifest()
            {
                ExecutedBy = System.Security.Principal.WindowsIdentity.GetCurrent().Name,
                ExtractId = Guid.NewGuid(),
                ExtractStart = DateTime.UtcNow,
                ProjectConfig = projectConfig,
                Items = new List<ManifestItem>()
            };

            var workDirName = manifest.ExtractId.ToString();
            var workDirPath = Path.Combine(_outputFolder, workDirName);
            var extractFolder = Directory.CreateDirectory(workDirPath);
            //ConfigManager.ApplicationClass = ApplicationClassEnum.Client;
            ConfigManager.DeploymentMode = DeploymentModeEnum.OnPremises;
            ConfigManager.ApplicationClass = ApplicationClassEnum.Service;
            ConfigManager.Log = new ConsoleLogger("DLS Extractor");
            // to prevent RefreshBrokerConnection
            
            try
            {
                string relativePathBase;


                

                /**/
                var sqlDirName = "MSSQLDB";
                var mssqlDbDirPath = Path.Combine(_outputFolder, workDirName, sqlDirName);
                Directory.CreateDirectory(mssqlDbDirPath);
                relativePathBase = sqlDirName; // Path.Combine(workDirName, sqlDirName);
                
                foreach (var sqlComponent in projectConfig.DatabaseComponents)
                {
                    // remove later
                    continue;

                    string serverName = sqlComponent.ServerName;
                    int serverVersion = FindServerVersion(serverName);
                    if (serverVersion >= 13)
                    {
                        var sqlExtractor = new SqlDb.SqlExtractor(sqlComponent, mssqlDbDirPath, relativePathBase, manifest);
                        sqlExtractor.Extract();
                    }
                    else
                    {
                        SaveManifest(workDirPath, manifest);
                        RunProcess(v12FullPath, new string[] { args[0], args[1], manifest.ExtractId.ToString(), sqlComponent.MssqlDbProjectComponentId.ToString() });
                        manifest = LoadManifest(workDirPath);
                    }
                }
                var ssisDirName = "SSIS";
                var ssisDirPath = Path.Combine(_outputFolder, workDirName, ssisDirName);
                Directory.CreateDirectory(ssisDirPath);
                relativePathBase = ssisDirName; // Path.Combine(workDirName, ssisDirName);

                foreach (var ssisComponent in projectConfig.SsisComponents)
                {
                    string serverName = ssisComponent.ServerName;
                    int serverVersion = FindServerVersion(serverName);
                    if (serverVersion >= 13)
                    {
                        var extractor = new Ssis.SsisExtractor(ssisComponent, ssisDirPath, relativePathBase, manifest);
                        extractor.Extract();
                    }
                    else if (serverVersion >= 12)
                    {
                        SaveManifest(workDirPath, manifest);
                        RunProcess(v12FullPath, new string[] { args[0], args[1], manifest.ExtractId.ToString(), ssisComponent.SsisProjectComponentId.ToString() });
                        manifest = LoadManifest(workDirPath);
                    }
                    else
                    {
                        SaveManifest(workDirPath, manifest);
                        RunProcess(v11FullPath, new string[] { args[0], args[1], manifest.ExtractId.ToString(), ssisComponent.SsisProjectComponentId.ToString() });
                        manifest = LoadManifest(workDirPath);
                    }
                }

                var ssasDirName = "SSAS";
                var ssasDirPath = Path.Combine(_outputFolder, workDirName, ssasDirName);
                Directory.CreateDirectory(ssasDirPath);
                relativePathBase = ssasDirName; // Path.Combine(workDirName, ssasDirName);

                foreach (var ssasComponent in projectConfig.SsasComponents)
                {
                    var extractor = new Ssas.SsasExtractor(ssasComponent, ssasDirPath, relativePathBase, manifest);
                    extractor.Extract();
                }
                /**/

                var ssrsDirName = "SSRS";
                var ssrsDirPath = Path.Combine(_outputFolder, workDirName, ssrsDirName);
                Directory.CreateDirectory(ssrsDirPath);
                relativePathBase = ssrsDirName; // Path.Combine(workDirName, ssrsDirName);

                foreach (var ssrsComponent in projectConfig.SsrsComponents)
                {
                    var extractor = new Ssrs.SsrsExtractor(ssrsComponent, ssrsDirPath, relativePathBase, manifest);
                    extractor.Extract();
                }

                var powerBiDirName = "PBI";
                var powerBiDirPath = Path.Combine(_outputFolder, workDirName, powerBiDirName);
                Directory.CreateDirectory(powerBiDirPath);
                relativePathBase = powerBiDirName; // Path.Combine(workDirName, powerBiDirName);

                for (int i = 0; i < projectConfig.PowerBiComponents.Count; i++)
                {
                    //MessageBox.Show(Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location),projectConfig.ProjectConfigId.ToString(),powerBicomponent.ApplicationID));
                    var credentialsFilePath = Path.Combine(System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "config", projectConfig.ProjectConfigId.ToString() + ".credentials");
                    Credentials credentials = JsonConvert.DeserializeObject<Credentials>(File.ReadAllText(credentialsFilePath));
                    Credential credential = credentials.FindCredential(projectConfig.PowerBiComponents[i].PowerBiProjectComponentId, "PowerBi");
                    var extractor = new PowerBi.PowerBiExtractor(projectConfig.PowerBiComponents[i], relativePathBase, powerBiDirPath, manifest, credential.Username, credential.Password);
                    extractor.Extract();
                }
            }
            catch (Exception ex)
            {
                ShowException(ex);
                throw;
                //Console.ReadLine();
            }

            var zipPath = workDirPath + ".zip";
            ConfigManager.Log.Important($"Compressing the extract \"{zipPath}\"");
            var manifestSerialized = manifest.Serialize();
            File.WriteAllText(Path.Combine(workDirPath, "manifest.json"), manifestSerialized);
            ZipFile.CreateFromDirectory(workDirPath, zipPath, CompressionLevel.Optimal, false);

        }

        private static void SaveManifest(string workDirPath, Manifest manifest)
        {
            var manifestSerialized = manifest.Serialize();
            File.WriteAllText(Path.Combine(workDirPath, "manifest.json"), manifestSerialized);
        }

        private static Manifest LoadManifest(string workDirPath)
        {
            var json = File.ReadAllText(Path.Combine(workDirPath, "manifest.json"));
            return Manifest.Deserialize(json);
        }

        private static void RunProcess(string fullPath, string[] args)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.CreateNoWindow = false;
            startInfo.UseShellExecute = false;
            startInfo.FileName = fullPath;
            startInfo.WindowStyle = ProcessWindowStyle.Normal;
            startInfo.WorkingDirectory = Path.GetTempPath(); // Path.GetDirectoryName(extractDirPath);
            startInfo.Arguments = string.Format(string.Join(" ", args));
            ConfigManager.Log.Important("Executing " + fullPath, startInfo.Arguments);
            startInfo.RedirectStandardError = true;
            startInfo.RedirectStandardOutput = true;
            using (Process exeProcess = Process.Start(startInfo))
            {
                //var stdOutput = exeProcess.StandardOutput.ReadToEnd();
                //ConfigManager.Log.Important(stdOutput);

                //var stdError = exeProcess.StandardError.ReadToEnd();
                //ConfigManager.Log.Error(stdError);

                
                exeProcess.OutputDataReceived += ExeProcess_OutputDataReceived;
                exeProcess.BeginOutputReadLine();
                exeProcess.BeginErrorReadLine();

                exeProcess.WaitForExit();

                exeProcess.OutputDataReceived -= ExeProcess_OutputDataReceived;

                if (exeProcess.ExitCode != 0)
                {
                    ConfigManager.Log.Error("Extractor returned an unexpected exit code: " + exeProcess.ExitCode.ToString());
                    throw new Exception();
                }

            }
            
        }

        private static void ExeProcess_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            ConfigManager.Log.Info(e.Data);
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

        public static int FindServerVersion(string serverName)
        {
            var connectionString = String.Format(@"Data Source={0};Integrated Security=True", serverName);
            var nb = new NetBridge(true);
            nb.SetConnectionString(connectionString);
            DataTable tab = new DataTable();
            tab = nb.ExecuteSelectStatement("SELECT CONVERT(VARCHAR(128), SERVERPROPERTY ('productversion'))");
            string version;
            version = tab.Rows[0][0].ToString();
            version = version.Split('.')[0];
            int res = int.Parse(version);
            ConfigManager.Log.Important("Server " + serverName + " version: " + res.ToString());
            return res;
        }
    }
}
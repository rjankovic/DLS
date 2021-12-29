//using System.Collections.Generic;
//using System.IO;
//using System;
//using System.Reflection;
//using System.Linq;
//using CD.DLS.Model.Mssql;
//using CD.DLS.Model.Mssql.Ssis;
//using System.Diagnostics;
//using CD.DLS.Common.Structures;
//using CD.DLS.DAL.Managers;
//using CD.DLS.Model.Interfaces;
//using CD.DLS.DAL.Configuration;
//using CD.DLS.Parse.Mssql.Db;
//using Microsoft.SqlServer.TransactSql.ScriptDom;

//namespace CD.DLS.Parse.Mssql.Ssis
//{
//    /// <summary>
//    /// Main entry point for parsing the SSIS model extracted by CD.BIDoc.Core.Extract.Mssql.Ssis.SsisExtractor 
//    /// </summary>
//    public class SsisModelParser
//    {
//        //private Db.IReferrableIndex _sqlContext;
//        private AvailableDatabaseModelIndex _availableDatabaseIndex;
//        private Db.ISqlScriptModelParser _sqlExtractor;
//        private ProjectConfig _projectConfig;
//        private Guid _extractId;
//        private StageManager _stageManager;

//        public SsisModelParser(AvailableDatabaseModelIndex availableDatabaseModelIndex, ProjectConfig projectConfig, Guid extractId, StageManager stageManager)
//        {
//            _availableDatabaseIndex = availableDatabaseModelIndex;
            
//            _sqlExtractor = new ScriptModelParser();

//            //_sqlExtractor = sqlExtractor;
//            _projectConfig = projectConfig;
//            _extractId = extractId;
//            _stageManager = stageManager;
//        }
        
//        /// <summary>
//        /// Extracts model for all projects specified by the filter on the server.
//        /// </summary>
//        /// <returns>The server element.</returns>
//        public List<ServerElement> ParsetModel()
//        {
//            List<ServerElement> res = new List<ServerElement>();

//            var groupByServer = _projectConfig.SsisComponents.GroupBy(x => x.ServerName);
//            foreach (var serverGrp in groupByServer)
//            {
//                var serverName = serverGrp.Key;
//                var rp = new RefPath(string.Format("IntegrationServices[@Name='{0}']", serverName));
//                var serverElement = new ServerElement(rp, serverName, rp.Path);
//                res.Add(serverElement);
                
//                var catalogRp = rp.Child("Catalog");

//                var catalogElement = new CatalogElement(catalogRp, "SSIS Catalog", catalogRp.Path, serverElement);
//                serverElement.AddChild(catalogElement);

//                var grpByFolder = serverGrp.GroupBy(x => x.FolderName);
//                foreach (var folderGrp in grpByFolder)
//                {
//                    var folderName = folderGrp.Key;
//                    //var folder = catalog.Folders[folderName];
//                    var folderRp = catalogRp.NamedChild("Folder", folderName);
//                    var folderElement = new FolderElement(folderRp, folderName, folderRp.Path, catalogElement);
//                    catalogElement.AddChild(folderElement);

//                    foreach(var projectComponent in folderGrp)
//                    {
//                        ConfigManager.Log.Important(string.Format("Extracting SSIS project {0} from {1}", projectComponent.ProjectName, projectComponent.ServerName));

//                        SsisXmlProvider definitionSearcher = new SsisXmlProvider(_extractId, projectComponent.SsisProjectComponentId, _stageManager);
//                        ProjectModelParser pml = new ProjectModelParser(definitionSearcher, _availableDatabaseIndex, _projectConfig, _extractId, _stageManager);
//                        pml.ParseProject(projectComponent.SsisProjectComponentId, folderElement);
//                    }
//                }
//            }
//            /**/

//            return res;
//        }

//        private void ExeProcess_ErrorDataReceived(object sender, DataReceivedEventArgs e)
//        {
//            ConfigManager.Log.Error(e.Data);
//        }

//        private void ExeProcess_OutputDataReceived(object sender, DataReceivedEventArgs e)
//        {
//            ConfigManager.Log.Important(e.Data);
//        }
        
//    }
//}

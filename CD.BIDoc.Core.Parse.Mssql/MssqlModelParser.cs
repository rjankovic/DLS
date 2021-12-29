//using CD.DLS.Parse.Mssql.Db;
//using CD.DLS.Parse.Mssql.Ssas;
//using CD.DLS.Parse.Mssql.Ssis;
//using CD.DLS.Parse.Mssql.Ssrs;
//using CD.DLS.Model.Mssql;
//using Microsoft.SqlServer.TransactSql.ScriptDom;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.IO;
//using System.Reflection;
//using System.Diagnostics;
//using CD.DLS.Model.Interfaces;
//using CD.DLS.Common.Structures;
//using CD.DLS.DAL.Managers;
//using CD.DLS.DAL.Configuration;
//using CD.DLS.Serialization;
//using CD.DLS.Model.Serialization;
//using CD.DLS.Model;
//using System.Windows;
//using CD.DLS.Parse.Mssql.Pbi;

//namespace CD.DLS.Parse.Mssql
//{
//    public class MssqlModelParser
//    {
//        private StageManager _stageManager;
//        private GraphManager _graphManager;
//        private ProjectConfig _projectConfig;

//        public MssqlModelParser(ProjectConfig projectConfig, StageManager stageManager, GraphManager graphManager)
//        {
//            _projectConfig = projectConfig;
//            _stageManager = stageManager;
//            _graphManager = graphManager;
//        }

//        public MssqlModelElement ParseAll(Guid extractId)
//        {
            
//            List<Model.Mssql.Db.ServerElement> dbServers = null;
//            List<Model.Mssql.Ssas.ServerElement> ssasServers = null;
//            List<Model.Mssql.Ssis.ServerElement> ssisServers = null;
//            List<Model.Mssql.Ssrs.ServerElement> ssrsServers = null;
//            //List<Model.Mssql.Agent.ServerElement> agentServers = null;
//            List<Model.Mssql.Pbi.TenantElement> pbiTenants = null;

//            _graphManager.ClearModel(_projectConfig.ProjectConfigId, Guid.Empty);
            
//            var solutionModel = new SolutionModelElement(new RefPath(""), "Solution");
//            SerializationHelper serializationHelper = new SerializationHelper(_projectConfig, _graphManager);

//            var elementIdMap = serializationHelper.SaveModelPart(solutionModel, new Dictionary<MssqlModelElement, int>());

            
//            //IDbModelExtractSettings dbSettings = settings.GetSettings<IDbModelExtractSettings>();

//            ShallowModelParser smil = new ShallowModelParser(_projectConfig, extractId, _stageManager);
//            dbServers = smil.ParseModel();

//            ScriptModelParser sqlExtractor = new ScriptModelParser();
//            DeepModelParser deepExtractor = new DeepModelParser(sqlExtractor, _projectConfig, extractId, _stageManager);
//            deepExtractor.ExtractModel(dbServers);
            
//            foreach (var dbServer in dbServers)
//            {
//                solutionModel.AddChild(dbServer);
//                dbServer.Parent = solutionModel;

//                var dbServerElementIdMap = serializationHelper.SaveModelPart(dbServer, elementIdMap);
//                foreach (var dbServerMapItem in dbServerElementIdMap)
//                {
//                    elementIdMap.Add(dbServerMapItem.Key, dbServerMapItem.Value);
//                }
//            }

//            var availableRelationalDatabasesIndex = new AvailableDatabaseModelIndex(_projectConfig, _graphManager);

//            ReferrableIndexBuilder srib = new ReferrableIndexBuilder();
//            var sqlEnvironment = srib.BuildIndex(dbServers);
//            SsasServerIndex ssasEnvironment = new SsasServerIndex(_projectConfig, _graphManager);
            
//            SsisIndex ssisIndex = null;

//            //ISsasModelExtractSettings ssasSettings = settings.GetSettings<ISsasModelExtractSettings>();
//            if (_projectConfig.SsasComponents.Any())
//            {
//                //var ssasExtractSettings = new SsasModelExtractSettings(ssasSettings.ServerName, ssasSettings.DbFilter, sqlEnvironment, sqlExtractor, ssasSettings.Log);
//                SsasModelExtractor ssasExtractor = new SsasModelExtractor(
//                    _projectConfig, extractId, _stageManager, _graphManager, 
//                    availableRelationalDatabasesIndex);
//                ssasServers = ssasExtractor.ExtractModel();
//                ssasEnvironment = new SsasServerIndex(ssasServers, _projectConfig, _graphManager);
//                if (ssasServers != null)
//                {
//                    foreach (var ssasServer in ssasServers)
//                    {
//                        ssasServer.Parent = solutionModel;
//                        solutionModel.AddChild(ssasServer);

//                        var ssasServerElementIdMap = serializationHelper.SaveModelPart(ssasServer, elementIdMap);
//                        foreach (var ssasServerMapItem in ssasServerElementIdMap)
//                        {
//                            elementIdMap.Add(ssasServerMapItem.Key, ssasServerMapItem.Value);
//                        }

//                    }
//                }
//            }
            
//            //ISsisModelExtractSettings ssisSettings = settings.GetSettings<ISsisModelExtractSettings>();
//            if (_projectConfig.SsisComponents.Any())
//            {
//                var ssisExtractor = new SsisModelParser(availableRelationalDatabasesIndex, _projectConfig, extractId /*sqlEnvironment*/, _stageManager);
//                ssisServers = ssisExtractor.ParsetModel();
//                var ssisIndexBuilder = new Ssis.SsisIndexBuilder();
//                ssisIndex = ssisIndexBuilder.BuildPackagesIndex(ssisServers);
//                if (ssisServers != null)
//                {
//                    foreach (var ssisServer in ssisServers)
//                    {
//                        ssisServer.Parent = solutionModel;
//                        solutionModel.AddChild(ssisServer);

//                        var ssisServerElementIdMap = serializationHelper.SaveModelPart(ssisServer, elementIdMap);
//                        foreach (var ssisServerMapItem in ssisServerElementIdMap)
//                        {
//                            elementIdMap.Add(ssisServerMapItem.Key, ssisServerMapItem.Value);
//                        }
//                    }
//                }
//            }

//            //ISsrsModelExtractSettings ssrsSettings = settings.GetSettings<ISsrsModelExtractSettings>();
//            if (_projectConfig.SsrsComponents.Any())
//            {
//                //var ssrsExtractSettings = new SsrsModelExtractSettings(ssrsSettings.ServiceUrl, ssrsSettings.ExecutionServiceUrl, ssrsSettings.FolderFilter, sqlEnvironment, sqlExtractor,
//                //    ssasEnvironment, new MdxScriptModelExtractor(ssasSettings), ssrsSettings.ServerName, ssrsSettings.Log);
//                var mdxExtractor = new MdxScriptModelExtractor();
//                var ssrsExtractor = new SsrsModelExtractor(availableRelationalDatabasesIndex, ssasEnvironment, mdxExtractor, _projectConfig, extractId, _stageManager);
//                ssrsServers = ssrsExtractor.ExtractModel();
//                if (ssrsServers != null)
//                {
//                    foreach (var ssrsServer in ssrsServers)
//                    {
//                        ssrsServer.Parent = solutionModel;
//                        solutionModel.AddChild(ssrsServer);

//                        var ssrsServerElementIdMap = serializationHelper.SaveModelPart(ssrsServer, elementIdMap);
//                        foreach (var ssrsServerMapItem in ssrsServerElementIdMap)
//                        {
//                            elementIdMap.Add(ssrsServerMapItem.Key, ssrsServerMapItem.Value);
//                        }
//                    }
//                }

//                //var reportLising = ssrsExtractor.ExtractReportList();

//            }

            
//            if(_projectConfig.PowerBiComponents.Any())
//            {
//                var pbiExtractor = new PbiModelExtractor(_projectConfig, _stageManager, extractId, _graphManager,0,null);
//                pbiExtractor.ParseModel();

//                if (pbiTenants != null)
//                {
//                    foreach (var pbitenant in pbiTenants)
//                    {
//                        pbitenant.Parent = solutionModel;
//                        solutionModel.AddChild(pbitenant);

//                        var pbiTenantElementIdmap = serializationHelper.SaveModelPart(pbitenant, elementIdMap);
//                        foreach (var pbiTenantMapItem in pbiTenantElementIdmap)
//                        {
//                            elementIdMap.Add(pbiTenantMapItem.Key, pbiTenantMapItem.Value);
//                        }
//                    }
//                }

//            }
            
         
//            /*
//            //IAgentModelExtractSettings agentSettings = settings.GetSettings<IAgentModelExtractSettings>();
//            if (settings.Config.MssqlAgentComponents.Any())
//            {
//                //var agentExtractSettings = new AgentModelExtractor.AgentModelExtractSettings(agentSettings.ServerName, agentSettings.JobFilter, sqlEnvironment, sqlExtractor, ssisSettings != null, ssisIndex);
//                var agentExtractor = new AgentModelExtractor(settings, sqlExtractor, sqlEnvironment, ssisIndex);
//                agentServers = agentExtractor.ExtractModel();
//                if (agentServers != null)
//                {
//                    foreach (var agentServer in agentServers)
//                    {
//                        agentServer.Parent = solutionModel;
//                        solutionModel.AddChild(agentServer);
//                    }
//                }
//            }
//            */
//            return solutionModel;
//        }

        

//    }
//}

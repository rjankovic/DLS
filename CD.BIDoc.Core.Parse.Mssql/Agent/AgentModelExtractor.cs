//using CD.DLS.Parse.Mssql.Db;
//using CD.DLS.Interfaces;
//using CD.DLS.Model.Mssql.Agent;
//using Microsoft.SqlServer.Management.IntegrationServices;
//using Microsoft.SqlServer.Management.Smo;
//using Microsoft.SqlServer.Management.Smo.Agent;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace CD.DLS.Parse.Mssql.Agent
//{
//    public class AgentModelExtractor
//    {
//    //    public class AgentModelExtractSettings
//    //    {

//    //        public readonly string ServerName;
//    //        public IAgentJobFilter jobFilter;
//    //        public readonly Db.IReferrableIndex DbContext;
//    //        public readonly Db.ISqlScriptModelExtractor DbSqlScriptExtractor;
//    //        public readonly bool IncludeSsis;
//    //        public readonly Ssis.SsisIndex SsisIndex;


//    //        public AgentModelExtractSettings(
//    //            string serverName
//    //            , IAgentJobFilter jobFilter
//    //            , Db.IReferrableIndex dbContextIndex
//    //            , Db.ISqlScriptModelExtractor dbScriptExtractor
//    //            , bool includeSsis
//    //            , Ssis.SsisIndex ssisIndex)
//    //        {
//    //            ServerName = serverName;
//    //            this.jobFilter = jobFilter;
//    //            DbContext = dbContextIndex;
//    //            DbSqlScriptExtractor = dbScriptExtractor;
//    //            IncludeSsis = includeSsis;
//    //            SsisIndex = ssisIndex;
//    //        }
//    //    }

//        private ModelSettings _settings;
//        private UrnBuilder _urnBuilder;


//        //public AgentModelExtractSettings Settings
//        //{
//        //    set
//        //    {
//        //        _settings = value;
//        //    }
//        //}

//        private Server _server;
//        private IntegrationServices _ssis = null;
//        private string _ssisUrn = null;

//        private ISqlScriptModelParser _sqlExtractor;
//        private IReferrableIndex _sqlContext;
//        private Ssis.SsisIndex _ssisIndex;

//        public AgentModelExtractor(ModelSettings settings, ISqlScriptModelParser sqlExtractor, IReferrableIndex sqlContext, Ssis.SsisIndex ssisIndex)
//        {
//            _settings = settings;
//            _sqlExtractor = sqlExtractor;
//            _sqlContext = sqlContext;
//            _ssisIndex = ssisIndex;
//        }

//        public List<ServerElement> ExtractModel()
//        {
//            var grpByServer = _settings.Config.MssqlAgentComponents.GroupBy(x => x.ServerName);
//            List<ServerElement> res = new List<ServerElement>();

//            foreach (var serverGrp in grpByServer)
//            {
//                var serverName = serverGrp.Key;
//                //Console.WriteLine("Building SQL Agent Dependency graph");
//                _server = new Server(serverName);
//                var agent = _server.JobServer;
//                var agentElement = new Model.Mssql.Agent.ServerElement(new RefPath(agent.Urn), agent.Name);

//                try
//                {
//                    _ssis = new IntegrationServices(_server);
//                    _ssisUrn = _ssis.Urn;
//                    _urnBuilder = new UrnBuilder(_ssis);
//                }
//                catch
//                {
//                    // SSIS not found on the server
//                    throw;
//                }


//                foreach (var job in serverGrp)
//                {
//                    //if (job.)
//                    //{
//                    //}


//                    //foreach (var jobTuple in _settings.jobFilter.EnumerateJobs(agent))
//                    //{
//                        _settings.Log.Important(string.Format("Extracting job {0} from {1}", job.JobName, job.ServerName));
//                        AddJob(agent.Jobs[job.JobName], agentElement);

//                    //}
//                }

//                _server = null;
//                res.Add(agentElement);
//            }
//            return res;
//        }

//        private void AddJob(Job job, Model.Mssql.Agent.ServerElement agentElement)
//        {
//            var jobElement = new JobElement(new RefPath(job.Urn), job.Name, null, agentElement);
//            agentElement.AddChild(jobElement);
            
//            Dictionary<int, StepElement> idsToSteps = new Dictionary<int, StepElement>();
//            foreach (JobStep step in job.JobSteps)
//            {
//                StepElement stepElement = null;
//                switch (step.SubSystem)
//                {
//                    case AgentSubSystem.ActiveScripting:
//                        stepElement = new ActiveScriptingStepElement(new RefPath(step.Urn), step.Name, null, jobElement);
//                        break;
//                    case AgentSubSystem.AnalysisCommand:
//                        stepElement = new AnalysisCommandStepElement(new RefPath(step.Urn), step.Name, null, jobElement);
//                        break;
//                    case AgentSubSystem.AnalysisQuery:
//                        stepElement = new AnalysisQueryStepElement(new RefPath(step.Urn), step.Name, null, jobElement);
//                        break;
//                    case AgentSubSystem.CmdExec:
//                        stepElement = new CmdExecStepElement(new RefPath(step.Urn), step.Name, null, jobElement);
//                        break;
//                    case AgentSubSystem.Distribution:
//                        stepElement = new DistributionStepElement(new RefPath(step.Urn), step.Name, null, jobElement);
//                        break;
//                    case AgentSubSystem.LogReader:
//                        stepElement = new LogReaderStepElement(new RefPath(step.Urn), step.Name, null, jobElement);
//                        break;
//                    case AgentSubSystem.Merge:
//                        stepElement = new MergeStepElement(new RefPath(step.Urn), step.Name, null, jobElement);
//                        break;
//                    case AgentSubSystem.PowerShell:
//                        stepElement = new PowerShellStepElement(new RefPath(step.Urn), step.Name, null, jobElement);
//                        break;
//                    case AgentSubSystem.QueueReader:
//                        stepElement = new QueueReaderStepElement(new RefPath(step.Urn), step.Name, null, jobElement);
//                        break;
//                    case AgentSubSystem.Snapshot:
//                        stepElement = new SnapshotStepElement(new RefPath(step.Urn), step.Name, null, jobElement);
//                        break;
//                    case AgentSubSystem.Ssis:
//                        stepElement = new SsisStepElement(new RefPath(step.Urn), step.Name, null, jobElement);
//                        BuildSsisStep((SsisStepElement)stepElement, step);
//                        break;
//                    case AgentSubSystem.TransactSql:
//                        stepElement = new TsqlStepElement(new RefPath(step.Urn), step.Name, null, jobElement);
//                        BuildTsqlStep((TsqlStepElement)stepElement, step);
//                        break;
//                }
//                idsToSteps.Add(step.ID, stepElement);
//                jobElement.AddChild(stepElement);
//            }

//            jobElement.StartStep = idsToSteps[job.StartStepID];

//            foreach (JobStep step in job.JobSteps)
//            {
//                var stepElement = idsToSteps[step.ID];
//                var onSuccessActionElement = new OnStepSuccessElement(_urnBuilder.GetOnStepSuccessUrn(stepElement), "On Success", null, stepElement);
//                var onFailureActionElement = new OnStepFailureElement(_urnBuilder.GetOnStepFailureUrn(stepElement), "On Failure", null, stepElement);
//                stepElement.AddChild(onSuccessActionElement);
//                stepElement.AddChild(onFailureActionElement);

//                switch (step.OnSuccessAction)
//                {
//                    case StepCompletionAction.GoToNextStep:
//                        var followingStep = idsToSteps[step.ID + 1];
//                        onSuccessActionElement.GoToStep = followingStep;
//                        break;
//                    case StepCompletionAction.GoToStep:
//                        var goToStep = idsToSteps[step.OnSuccessStep];
//                        onSuccessActionElement.GoToStep = goToStep;
//                        break;
//                }
//                switch (step.OnFailAction)
//                {
//                    case StepCompletionAction.GoToNextStep:
//                        var followingStep = idsToSteps[step.ID + 1];
//                        onFailureActionElement.GoToStep = followingStep;
//                        break;
//                    case StepCompletionAction.GoToStep:
//                        var goToStep = idsToSteps[step.OnFailStep];
//                        onFailureActionElement.GoToStep = goToStep;
//                        break;
//                }
//            }
//        }

//        private void BuildTsqlStep(TsqlStepElement stepElement, JobStep step)
//        {
//            var dbIdentifier = new Microsoft.SqlServer.TransactSql.ScriptDom.Identifier() { Value = step.DatabaseName };

//            var server = step.Server;
//            _sqlContext.ContextServerName = server;
//            var sqlNode = _sqlExtractor.ExtractScriptModel(step.Command, stepElement, _sqlContext, dbIdentifier);
//            stepElement.AddChild(sqlNode);
//        }

//        private void BuildSsisStep(SsisStepElement stepElement, JobStep step)
//        {
//            if (_ssis == null)
//            {
//                return;
//            }

//            var packageRefPath = _urnBuilder.GetPackageRefPath(step);
//            var packageElement = _ssisIndex.TryGetDefiningElementByRefPath(packageRefPath);
//            if (packageElement == null)
//            {
//                return;
//            }
            
//            stepElement.SsisPackageElement = (Model.Mssql.Ssis.PackageElement)packageElement;
//        }

//    }
//}

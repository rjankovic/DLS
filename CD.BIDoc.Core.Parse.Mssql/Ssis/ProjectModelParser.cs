using System.Collections.Generic;
using System;
using System.Xml;
using System.Linq;
using CD.DLS.Model.Mssql.Ssis;
using CD.DLS.Model.Mssql;
using CD.DLS.DAL.Managers;
using CD.DLS.DAL.Objects.Extract;
using CD.DLS.Common.Structures;
using CD.DLS.DAL.Configuration;
using CD.DLS.Parse.Mssql.Db;
using CD.BIDoc.Core.Parse.Mssql.Ssis;

namespace CD.DLS.Parse.Mssql.Ssis
{
    /// <summary>
    /// Parses a previously extracted SSIS project
    /// </summary>
    public class ProjectModelParser
    {
        private readonly UrnBuilder _urnBuilder = new UrnBuilder();
        private readonly SsisXmlProvider _definitionSearcher;
        //private readonly Db.IReferrableIndex _dbIndex;
        private AvailableDatabaseModelIndex _availableDatabaseModelIndex;
        private readonly Db.ISqlScriptModelParser _sqlScriptExtractor;
        private readonly ExpressionModelExtractor _expressionExtractor;
        private readonly Dictionary<string, Db.TableSourceColumnList> _tempTablesAvailable = new Dictionary<string, Db.TableSourceColumnList>(StringComparer.OrdinalIgnoreCase);
        private int _configComponentId;
        private ProjectConfig _projectConfig;
        private Guid _extractId;
        private StageManager _stageManager;

        public ProjectModelParser(SsisXmlProvider definitionSearcher, 
            AvailableDatabaseModelIndex availableDatabaseModelIndex, 
            ProjectConfig projectConfig, Guid extractId, StageManager stageManager)
        {
            this._definitionSearcher = definitionSearcher;
            //this._dbIndex = dbIndex;
            _availableDatabaseModelIndex = availableDatabaseModelIndex;
            _sqlScriptExtractor = new ScriptModelParser();
            _expressionExtractor = new ExpressionModelExtractor(_urnBuilder);
            _projectConfig = projectConfig;
            _extractId = extractId;
            _stageManager = stageManager;
        }

        ///// <summary>
        ///// Creates a model of the SSIS project and stores it in the FolderElement
        ///// </summary>
        ///// <param name="extractRequestId"></param>
        ///// <param name="configComponentId"></param>
        ///// <param name="folder"></param>
        //public void ParseProject(int configComponentId, FolderElement folder)
        //{
        //    _configComponentId = configComponentId;
        //    var component = _projectConfig.SsisComponents.First(x => x.SsisProjectComponentId == configComponentId);
        //    var projectUrn = _urnBuilder.GetProjectUrn(folder, component.ProjectName);
        //    var projectElement = new ProjectElement(projectUrn, component.ProjectName, projectUrn.Path, folder);
        //    folder.AddChild(projectElement);

        //    SsisIndex referrables = new SsisIndex();

        //    var parametersFiles = _stageManager.GetExtractItems(_extractId, _configComponentId, DLS.DAL.Objects.Extract.ExtractTypeEnum.SsisProjectsParameters);
        //    var parametersFile = (SsisProjectParameters)(parametersFiles[0]);

        //    foreach (var parameter in parametersFile.Parameters)
        //    {
        //        var parameterElement = AddProjectParameter(parameter, projectElement);

        //        var parameterReferenceName = parameterElement.GetExpressionReferenceName();
        //        referrables.Add(parameterReferenceName, parameterReferenceName, parameterElement, projectElement);
        //    }

        //    ConnectionIndex projectConnections = new ConnectionIndex();
        //    var connectionFiles = _stageManager.GetExtractItems(_extractId, _configComponentId, ExtractTypeEnum.SsisProjectConnectionManager);

        //    foreach (SsisProjectConnectionManager connectionManager in connectionFiles)
        //    {
        //        var conMgr = AddConnectionManager(connectionManager.ConnectionManager, projectElement, referrables);
        //        projectConnections.Add(conMgr);
        //    }

        //    Dictionary<string, string> packageNamesToUrns = new Dictionary<string, string>();
        //    List<Tuple<ExecutePackageTaskElement, string>> execPackageTasksPackageNames = new List<Tuple<ExecutePackageTaskElement, string>>();
        //    List<PackageElement> packageNodes = new List<PackageElement>();

        //    var packageFiles = _stageManager.GetExtractItems(_extractId, _configComponentId, ExtractTypeEnum.SsisPackage);

        //    foreach (var package in packageFiles)
        //    {
        //        var urn = _urnBuilder.GetPackageUrn(projectElement, package.Name);
        //        packageNamesToUrns.Add(package.Name, urn.Path);
        //    }

        //    //var packageFilter = _settings.PackageFilter;
        //    HashSet<string> failedPackageLoads = new HashSet<string>();
        //    foreach (SsisPackage package in packageFiles)
        //    {
        //        ConfigManager.Log.Info(string.Format("Parsing SSIS package {0}", package.Name));
        //        var pkgNode = AddPackage(package, projectElement, execPackageTasksPackageNames, referrables, projectConnections);
        //        packageNodes.Add(pkgNode);
        //    }

        //    foreach (var execTask in execPackageTasksPackageNames)
        //    {
        //        if (execTask.Item2 == string.Empty)
        //        {
        //            continue;
        //        }
        //        if(!packageNamesToUrns.ContainsKey(execTask.Item2.ToLower()))
        //        {
        //            continue;
        //        }
        //        var urn = packageNamesToUrns[execTask.Item2.ToLower()];
        //        if (failedPackageLoads.Contains(urn))
        //        {
        //            continue;
        //        }
        //        var targetPackage = packageNodes.First(x => x.RefPath.Path == urn);
        //        execTask.Item1.Package = targetPackage;
        //    }
        //}

        public void ParsePackage(int configComponentId, ProjectElement projectElementShallow, 
            PackageElement packageElement, SsisPackage packageExtract)
        {
            ParsePackage(configComponentId, projectElementShallow, packageElement, 
                packageExtract, projectElementShallow.ConnectionManagers.ToList());
        }

        public void ParsePackage(int configComponentId, ProjectElement projectElementShallow,
            PackageElement packageElement, SsisPackage packageExtract, List<ConnectionManagerElement> connectionManagers)
        {
            _configComponentId = configComponentId;
            var component = _projectConfig.SsisComponents.First(x => x.SsisProjectComponentId == configComponentId);

            ConnectionIndex projectConnections = new ConnectionIndex();
            var connectionFiles = _stageManager.GetExtractItems(_extractId, _configComponentId, ExtractTypeEnum.SsisProjectConnectionManager);

            foreach (var conMgr in connectionManagers)
            {
                projectConnections.Add(conMgr);
            }

            Dictionary<string, string> packageNamesToUrns = new Dictionary<string, string>();
            List<Tuple<ExecutePackageTaskElement, string>> execPackageTasksPackageNames = new List<Tuple<ExecutePackageTaskElement, string>>();
            List<PackageElement> packageNodes = new List<PackageElement>();

            //var packageFiles = _stageManager.GetExtractItems(_extractId, _configComponentId, ExtractTypeEnum.SsisPackage);

            foreach (var package in projectElementShallow.Packages)
            {
                var urn = package.RefPath; // _urnBuilder.GetPackageUrn(projectElement, package.Name);
                packageNamesToUrns.Add(package.Caption.ToLower() /* .Name*/, urn.Path);
            }

            //var packageFilter = _settings.PackageFilter;
            HashSet<string> failedPackageLoads = new HashSet<string>();
            //foreach (SsisPackage package in packageFiles)
            //{
            ConfigManager.Log.Info(string.Format("Parsing SSIS package {0}", packageExtract.PackageName));
            PackageElement pkgNode = null;
            try
            {
                pkgNode = ParsePackageDeep(packageElement, packageExtract, projectElementShallow,
                    execPackageTasksPackageNames, new SsisIndex() /* for now */,
                    projectConnections);
            }
            catch (Exception ex)
            {
                ConfigManager.Log.Error(ex.Message);
                ConfigManager.Log.Error(ex.StackTrace);
                if (ex.InnerException != null)
                {
                    ConfigManager.Log.Error(ex.InnerException.Message);
                    ConfigManager.Log.Error(ex.InnerException.StackTrace);
                }
            }
            packageNodes.Add(pkgNode);
            //}

            foreach (var execTask in execPackageTasksPackageNames)
            {
                if (execTask.Item2 == string.Empty)
                {
                    continue;
                }
                if (!packageNamesToUrns.ContainsKey(execTask.Item2.ToLower()))
                {
                    continue;
                }
                var urn = packageNamesToUrns[execTask.Item2.ToLower()];
                if (failedPackageLoads.Contains(urn))
                {
                    continue;
                }
                var targetPackage = projectElementShallow.Packages.First(x => x.RefPath.Path == urn);
                execTask.Item1.Package = targetPackage;
            }
        }

        public ProjectElement ParseProjectShallow(int configComponentId, FolderElement folder, SsisXmlProvider xmlProvider)
        {
            _configComponentId = configComponentId;
            var component = _projectConfig.SsisComponents.First(x => x.SsisProjectComponentId == configComponentId);
            var projectUrn = _urnBuilder.GetProjectUrn(folder, component.ProjectName);
            var projectElement = new ProjectElement(projectUrn, component.ProjectName, projectUrn.Path, folder);
            folder.AddChild(projectElement);

            SsisIndex referrables = new SsisIndex();

            //var parametersFiles = _stageManager.GetExtractItems(_extractId, _configComponentId, DLS.DAL.Objects.Extract.ExtractTypeEnum.SsisProjectsParameters);
            //var parametersFile = (SsisProjectParameters)(parametersFiles[0]);
            var project = xmlProvider.Project;

            foreach (var parameter in project.ProjectParameters)
            {
                var parameterElement = AddProjectParameter(parameter, projectElement);

                var parameterReferenceName = parameterElement.GetExpressionReferenceName();
                referrables.Add(parameterReferenceName, parameterReferenceName, parameterElement, projectElement);
            }

            ConnectionIndex projectConnections = new ConnectionIndex();
            //var connectionFiles = _stageManager.GetExtractItems(_extractId, _configComponentId, ExtractTypeEnum.SsisProjectConnectionManager);

            foreach (var connectionManager in project.ProjectConnectionManagers)
            {
                var conMgr = AddConnectionManager(connectionManager, projectElement, referrables);
                projectConnections.Add(conMgr);
            }

            return projectElement;
        }

        private ProjectParameterElement AddProjectParameter(SsisParameter parameter, ProjectElement projectElement)
        {
            String definition = "";// _definitionSearcher.GetProjectParamDefinition(parameter.ID);
            var urn = _urnBuilder.GetParameterUrn(projectElement, parameter);
            var id = parameter.ID;

            var parameterElement = new ProjectParameterElement(urn, parameter.Name, definition, projectElement);
            projectElement.AddChild(parameterElement);
            //parameterElement.DataType = (TypeCode)(Enum.Parse(typeof(TypeCode), parameter.DataType));
            parameterElement.Value = parameter.Value.ToString();

            return parameterElement;
        }

        private PackageParameterElement AddPackageParameter(SsisParameter parameter, PackageElement packageElement, string packageId)
        {
            var paramDefinition = parameter.XmlDefinition; // _definitionSearcher.GetPackageParameterDefinition(parameter.ID, packageId);
            var paramUrn = _urnBuilder.GetParameterUrn(packageElement, parameter);

            var parameterElement = new PackageParameterElement(paramUrn, parameter.Name, paramDefinition, packageElement);
            packageElement.AddChild(parameterElement);
            //parameterElement.DataType = (TypeCode)(Enum.Parse(typeof(TypeCode), parameter.DataType));
            parameterElement.Value = string.Empty;
            if (parameter.Value != null)
            {
                parameterElement.Value = parameter.Value.ToString();
            }

            return parameterElement;
        }

        private ConnectionManagerElement AddConnectionManager(SsisConnectionManager manager, SsisModelElement parent, SsisIndex referrables, string packageId = null)
        {
            var creation = manager.CreationName;
            string definition = manager.XmlDefinition; // null;
            XmlElement containingElement;
            //if (parent is ProjectElement)
            //{
            //    definition = manager.XmlDefinition; // _definitionSearcher.GetProjectConnectionManagerDefinition(manager.ID, out containingElement);
            //}
            //else
            //{
            //    definition = _definitionSearcher.GetPackageConnectionManagerDefinition(manager.ID, packageId, out containingElement);
            //}

            ConnectionManagerElement conMgr = null;
            if (creation == "OLEDB" || creation == "ODBC")
            {
                conMgr = new DbConnectionManagerElement(_urnBuilder.GetConnectionManagerUrn(parent, manager.Name), manager.Name, definition, parent);
                conMgr.SourceType = creation;
                conMgr.ConnectionString = manager.ConnectionString;
            }
            else if (creation == "FLATFILE")
            {
                var flatConMgr = new FileConnectionManagerElement(_urnBuilder.GetConnectionManagerUrn(parent, manager.Name), manager.Name, definition, parent);
                flatConMgr.ConnectionString = manager.ConnectionString;
                //var objectData = (XmlElement)(containingElement.GetElementsByTagName("DTS:ObjectData")[0]);
                //var innerXml = (XmlElement)(objectData.GetElementsByTagName("DTS:ConnectionManager")[0]);
                
                //var localeId = innerXml.GetAttribute("DTS:LocaleID");
                //var codePage = innerXml.GetAttribute("DTS:CodePage");
                //var format = innerXml.GetAttribute("DTS:Format");
                //flatConMgr.CodePage = int.Parse(codePage);
                //flatConMgr.LocaleID = int.Parse(localeId);
                //flatConMgr.Format = format;
                
                flatConMgr.SourceType = creation;

                conMgr = flatConMgr;
            }
            else
            {
                conMgr = new ConnectionManagerElement(_urnBuilder.GetConnectionManagerUrn(parent, manager.Name), manager.Name, definition, parent);
                conMgr.ConnectionString = manager.ConnectionString;
                conMgr.SourceType = creation;
            }
            parent.AddChild(conMgr);
            conMgr.ManagerId = manager.ID;
            AddExpressions(manager.Properties, referrables, conMgr);

            return conMgr;
        }
        
            private PackageElement AddPackage(SsisPackage package, ProjectElement projectElement,
            List<Tuple<ExecutePackageTaskElement, string>> execPackageTasksPackageNames, SsisIndex projectReferrables, ConnectionIndex projectConnections)
        {

            // referenced by IDs
            ConnectionIndex packageConnections = new ConnectionIndex(projectConnections);

            var refPath = _urnBuilder.GetPackageUrn(projectElement, package.PackageName);// packageInfo.Urn;
            PackageElement packageElement = new PackageElement(refPath, package.PackageName, null /* definition*/, projectElement);
            projectElement.AddChild(packageElement);
            ParsePackageDeep(packageElement, package, projectElement,
                execPackageTasksPackageNames, projectReferrables, projectConnections);
            
            return packageElement;
        }

        private PackageElement ParsePackageDeep(PackageElement packageElement, SsisPackage package, ProjectElement projectElement,
List<Tuple<ExecutePackageTaskElement, string>> execPackageTasksPackageNames, SsisIndex projectReferrables, ConnectionIndex projectConnections)
        {

            // referenced by IDs
            ConnectionIndex packageConnections = new ConnectionIndex(projectConnections);

            //var refPath = _urnBuilder.GetPackageUrn(projectElement, package.PackageName);// packageInfo.Urn;
            //XmlElement definitionXml = null;
            //var definition = _definitionSearcher.GetPackageDefinition(package.Executable.ID, out definitionXml);
            //var layout = _definitionSearcher.GetPackageNodeLayout(package.Executable.ID);

            packageElement.Position = package.Layout.TopLeft;
            packageElement.Size = package.Layout.Size; // layout.Size;

            _tempTablesAvailable.Clear();

            SsisIndex packageReferrables = new SsisIndex(projectReferrables);

            // Parameters
            foreach (var parameter in package.Parameters)
            {
                var parameterElement = AddPackageParameter(parameter, packageElement, package.Executable.ID);
                packageReferrables.Add(parameterElement.GetExpressionReferenceName(), parameter.ID, parameterElement, packageElement);
            }

            // Variables
            AddVariables(package.Executable, packageReferrables, packageElement);

            // Connection managers
            foreach (SsisConnectionManager conMag in package.ConnectionManagers)
            {
                // these are covered at the project level
                if (conMag.Scope == SsisConnectionManagerScope.Project)
                {
                    continue;
                }
                var conMagElement = AddConnectionManager(conMag, packageElement, packageReferrables, package.Executable.ID);
                packageConnections.Add(conMagElement);
                packageConnections.AddWithRefId(conMag.RefId, conMagElement);
            }

            // Executables
            Dictionary<string, SsisModelElement> executablesToNodes = new Dictionary<string, SsisModelElement>();
            var topolExecutables = TopologicalOrder(package.Executable.Children, package.Executable.PrecedenceConstraints);
            foreach (var executable in topolExecutables /*package.Executables*/)
            {
                var execNode = AddExecutable(executable, package, packageElement, packageReferrables, packageConnections, execPackageTasksPackageNames);
                if (execNode != null)
                {
                    executablesToNodes.Add(((SsisExecutable)executable).RefId, execNode);
                }
            }

            AddPrecedenceConstraints(package.Executable.PrecedenceConstraints, executablesToNodes, packageElement /*, definitionXml*/, package);

            AddExpressions(package.Executable.Properties, packageReferrables, packageElement);

            return packageElement;
        }

        private void AddVariables(SsisExecutable container, SsisIndex referrables, SsisModelElement parent)
        {
            foreach (var variable in container.Variables)
            {
                // parent's variables are also present in childs' scope
                if (referrables.ContainsName(variable.QualifiedName))
                {
                    continue;
                }
                //var qn = variable.QualifiedName;
                var ns = variable.Namespace;
                //  rest is covered
                if (new string[] { /*"System",*/ "$Package", "$Project" }.Contains(ns))
                {
                    continue;
                }

                ReferrableValueElement variableElement = null;
                // system variable
                if (ns == "System")
                {
                    var varUrn = _urnBuilder.GetVariableUrn(parent, variable.QualifiedName);
                    variableElement = new SystemVariableElement(varUrn, variable.Name, variable.QualifiedName, parent);
                    /**/
                }
                else
                {
                    var variableDefinition = variable.XmlDefinition; // _definitionSearcher.GetVariableDefinition(variable.ID, containerXmlElement);
                    var varUrn = _urnBuilder.GetVariableUrn(parent, variable.QualifiedName);
                    variableElement = new VariableElement(varUrn, variable.Name, variableDefinition, parent);
                }
                // TODO one or the other - name of expression reference name
                if (referrables.ContainsName(variableElement.GetExpressionReferenceName()))
                {
                    continue;
                }
                //variableElement.DataType = ((TypeCode)(Enum.Parse(typeof(TypeCode), variable.DataType)));
                variableElement.Value = variable.Value.ToString();

                //TODO: Variaiable expression

                referrables.Add(variableElement.GetExpressionReferenceName(), variable.ID, variableElement, parent);

                parent.AddChild(variableElement);
            }
        }

        private void AddExpressions(IEnumerable<SsisProperty> properties, SsisIndex referrables, SsisModelElement element)
        {
            // Expressions are supported on:
            // - package
            // - connection manager

            //if (_settings.ExtractExpressions)
            //{
                foreach (SsisProperty property in properties)
                {
                    string expression = property.Expression;    
                    AddExpression(expression, referrables, element, property.Name);
                }
            //}
        }

        /// <summary>
        /// Adds a SSIS expression element to an existing element, if expression extraction is enabled.
        /// If expression extraction is disabled, or if expression is null, does nothing.
        /// </summary>
        /// <param name="expression">SSIS expression.</param>
        /// <param name="referrables">Referrables for the expression.</param>
        /// <param name="element">Containing element.</param>
        private void AddExpression(string expression, SsisIndex referrables, SsisModelElement element, string expressionName = null)
        {
            if (expression != null /*&& _settings.ExtractExpressions*/)
            {
                var expressionModel = _expressionExtractor.ExtractExpressionModel(expression, referrables, element, expressionName);
                element.AddChild(expressionModel);
            }
        }
        
        private class SqlParameterMapping
        {
            public string ParameterName { get; set; }
            public string VariableName { get; set; }
            public SsisParameterDirection Direction { get; set; }
        }

        private ExecutableElement AddExecutable(SsisExecutable executable, SsisPackage package, SsisModelElement parentNode, 
            SsisIndex referrables, ConnectionIndex connections, List<Tuple<ExecutePackageTaskElement, string>> execPackageTasksPackageNames)
        {
            // a single task is also a DtsContainer
            
            string designRefPath = null;
            XmlElement definitionElement = null;
            var definition = executable.XmlDefinition; // _definitionSearcher.GetExecutableDefinition(executable.ID, package.Executable.ID, out designRefPath, out definitionElement);
            //string definition = null;
            var nodeLayout = executable.Layout; // _definitionSearcher.GetContainerDesign(package.Executable.ID, designRefPath);
            //var nodeLayoutXml = _definitionSearcher.GetContainerDesignXml(package.Container.ID, designRefPath);
            var refPath = _urnBuilder.GetExecutableUrn(parentNode, executable.Name, executable.ID);

            ExecutableElement resNode = null;

            // single task
            if (executable is SsisTask)
            {
                var host = executable as SsisTask;
                //var innerObj = host.InnerObject;

                if (host is SsisDfTask)
                {
                    var pipe = ((SsisDfTask)host).Components;
                    var paths = ((SsisDfTask)host).Paths;
                    resNode = new DfTaskElement(refPath, host.Name, definition, parentNode);
                    SsisIndex containerReferrables = new SsisIndex(referrables);
                    AddVariables(host, containerReferrables, resNode);
                    SsisDfModelParser dfExtractor = new SsisDfModelParser(_definitionSearcher, _availableDatabaseModelIndex, _sqlScriptExtractor);
                    try
                    {
                        dfExtractor.ParseDataFlow(pipe, paths, resNode, definitionElement, designRefPath, package, containerReferrables, connections, _tempTablesAvailable, (SsisDfTask)host);
                    }
                    catch (Exception ex)
                    {
                        ConfigManager.Log.Error(string.Format("Failed to parse dataflow task in {0}: {1}{2}{3}", resNode.RefPath.Path, ex.Message, Environment.NewLine, ex.StackTrace));
                    }
                }
                else if (host is SsisExecutePackageTask /* host.CreationName.EndsWith(".ExecutePackageTask", StringComparison.Ordinal)*/)
                {
                    var execTask = (SsisExecutePackageTask)host;
                    var packageName = execTask.PackageName; // host.GetPropertyValue("PackageName");
                    ExecutePackageTaskElement execNode = new ExecutePackageTaskElement(refPath, host.Name, definition, parentNode);
                    execPackageTasksPackageNames.Add(new Tuple<ExecutePackageTaskElement, string>(execNode, packageName));
                    resNode = execNode;
                    //AddVariables(con, referrables, resNode, definitionElement);

                    var paramAssigns = execTask.ParameterAssignments; // _definitionSearcher.GetExecPackageParameterAssignments(definitionElement);

                    HashSet<string> assignedParams = new HashSet<string>();
                    foreach (var paramAssignment in paramAssigns)
                    {
                        if (assignedParams.Contains(paramAssignment.ParamName))
                        {
                            continue;
                        }
                        var assignmentRefPath = _urnBuilder.GetParameterAssignmentUrn(execNode /*parentNode*/, paramAssignment.ParamName);
                        var asgNode = new ExecutePackageParameterAssignmentElement(assignmentRefPath, paramAssignment.ParamName, paramAssignment.Definition, execNode);
                        execNode.AddChild(asgNode);
                        // asgNode.Links.Add(new BasicNodeLink() { NodeFrom = asgNode, NodeTo = referrables[paramAssignment.ReferrableName].Item1 });

                        asgNode.Assigned = referrables.GetNodeByName(paramAssignment.ReferrableName);
                        assignedParams.Add(paramAssignment.ParamName);
                    }
                }
                //TODO: Can we reference Microsoft.SqlServer.ExpressionTask.dll ?
                else if (host.CreationName.StartsWith("Microsoft.SqlServer.Dts.Tasks.ExpressionTask.ExpressionTask"))
                {
                    resNode = new ExpressionTaskElement(refPath, host.Name, definition, parentNode);
                    string expression = host.GetPropertyValue("Expression");
                    AddExpression(expression, referrables, resNode);
                }
                else
                {
                    //var type = host. innerObj.GetType().FullName;
                    //if (!(innerObj is Microsoft.SqlServer.Dts.Runtime.Task))
                    //{
                    //    return null;
                    //}
                    //var task = ((Microsoft.SqlServer.Dts.Runtime.Task)innerObj);

                    ///**/
                    if (host is SsisSqlTask)
                    {
                        var sqlTask = host as SsisSqlTask;

                        var sqlConId = sqlTask.ConnectionID;
                        DbConnectionManagerElement sqlConNode = null;
                        bool connectionFound = connections.TryGetDbConnectionManager(sqlConId, out sqlConNode);
                        //var conString = sqlConNode.conn

                        resNode = new SqlTaskElement(refPath, host.Name, definition, parentNode);
                        //AddVariables(con, referrables, resNode, definitionElement);

                        if (connectionFound)
                        {
                            var source = sqlTask.StatementSource;
                            var sourceParametrized = source;

                            Dictionary<string, ReferrableValueElement> paramMapping = new Dictionary<string, ReferrableValueElement>();
                            // set the DB used in the query according to the initial catalog of the connection manager
                            var dbIdentifier = new Microsoft.SqlServer.TransactSql.ScriptDom.Identifier() { Value = sqlConNode.ConnectionDbName };
                            //var dbIdentifier = _sqlScriptExtractor.GetServerNameFromConnectionString()

                            var serverName = sqlConNode.ConnectionServerName;
                            
                            var dbIndex = _availableDatabaseModelIndex.GetDatabaseIndex(serverName, parentNode.LocalhostServer());
                            
                            var variablesIndex = new Db.AddPresetVariablesReferrableIndex(dbIndex);

                            var sourceType = sqlTask.StatementSourceType;
                            List<SqlParameterMapping> mappings = new List<SqlParameterMapping>();

                            foreach (SsisParameterMapping binding in sqlTask.Parameters)
                            {
                                var parName = binding.Name; //.ParameterName;
                                var dir = binding.Direction; // .ParameterDirection;
                                var variableName = binding.Variablename; // .DtsVariableName;
                                mappings.Add(new SqlParameterMapping()
                                {
                                    ParameterName = parName.ToString(),
                                    VariableName = variableName,
                                    Direction = dir
                                });
                            }

                            foreach (var mapping in mappings.OrderBy(x => x.ParameterName))
                            {
                                if (sourceType == "DirectInput")
                                {
                                    var queryParName = string.Format("@param_{0}", mapping.ParameterName);
                                    sourceParametrized = SQLUtils.ParametrizeSql(sourceParametrized, queryParName);

                                    ReferrableValueElement variableNode;
                                    if (referrables.TryGetNodeByName(mapping.VariableName, out variableNode))
                                    {
                                        paramMapping.Add(queryParName, variableNode);
                                        variablesIndex.AddPresetVariable(queryParName, variableNode);
                                    }
                                }
                                else
                                {
                                    // TODO: SQL from variable
                                }
                            }
                            foreach (var tempTable in _tempTablesAvailable.Values)
                            {
                                variablesIndex.AddTable(tempTable);
                            }

                            Dictionary<string, MssqlModelElement> resultColumns = new Dictionary<string, MssqlModelElement>();
                            Dictionary<string, Db.TableSourceColumnList> tempTablesCreated = new Dictionary<string, Db.TableSourceColumnList>();

                            //_settings.RelationalDbFactory.Visitor -- inject parameters (variables) to visitor
                            if (sourceType == "DirectInput")
                            {
                                var sqlNode = _sqlScriptExtractor.ExtractScriptModel(sourceParametrized, resNode, variablesIndex, dbIdentifier, out resultColumns, out tempTablesCreated);
                                resNode.AddChild(sqlNode);
                            }

                            variablesIndex.ClearPresetTables();

                            if (tempTablesCreated != null)
                            {
                                foreach (var newTempTable in tempTablesCreated)
                                {
                                    _tempTablesAvailable[newTempTable.Key] = newTempTable.Value;
                                }
                            }
                        }

                    }
                    else
                    {
                        resNode = new TaskElement(refPath, host.Name, definition, parentNode);
                        //AddVariables(con, referrables, resNode, definitionElement);
                        
                    }
                }
            }
            // sequence/loop/...
            else
            {

                resNode = new ContainerElement(refPath, executable.Name, definition, parentNode);

                SsisIndex containerReferrables = new SsisIndex(referrables);
                AddVariables(executable, containerReferrables, resNode);

                List<SsisExecutable> exs;
                List<SsisPrecedenceConstraint> precedenceConstraints;
                GetExecutables(executable, out exs, out precedenceConstraints);
                var topolExs = TopologicalOrder(exs, precedenceConstraints);

                Dictionary<string, SsisModelElement> executablesToNodes = new Dictionary<string, SsisModelElement>();

                if (exs != null)
                {
                    foreach (SsisExecutable e in topolExs /*exs*/)
                    {
                        var execNode = AddExecutable(e, package, resNode, containerReferrables, connections, execPackageTasksPackageNames);
                        if (execNode != null)
                        {
                            executablesToNodes.Add(e.RefId, execNode);
                        }
                    }

                    AddPrecedenceConstraints(precedenceConstraints, executablesToNodes, resNode, package);
                }

            }

            //if (resNode != null)
            //{
            parentNode.AddChild(resNode);

            resNode.Size = nodeLayout.Size;
            resNode.Position = nodeLayout.TopLeft;
            resNode.Enabled = executable.Enabled;

            return resNode;
            //}

        }

        private void GetExecutables(SsisExecutable con, out List<SsisExecutable> exs, out List<SsisPrecedenceConstraint> precedenceConstraints)
        {
            exs = con.Children;
            precedenceConstraints = con.PrecedenceConstraints;
        }

        private void AddPrecedenceConstraints(List<SsisPrecedenceConstraint> constraints, Dictionary<string, SsisModelElement> executables, SsisModelElement parent, SsisPackage package)
        {
            foreach (var constraint in constraints)
            {
                var id = constraint.ID;
                if (!(executables.ContainsKey(constraint.PrecedenceExecutableID /* ((DtsContainer)(constraint.PrecedenceExecutable)).ID*/) && executables.ContainsKey(constraint.ConstrainedExecutableID /*((DtsContainer)(constraint.ConstrainedExecutable)).ID*/)))
                {
                    continue;
                }

                var fromNode = executables[constraint.PrecedenceExecutableID];
                var toNode = executables[constraint.ConstrainedExecutableID];

                string layoutRefId = null;
                var definition = constraint.XmlDefinition; // _definitionSearcher.GetPrecedenceConstraintDefinition(constraint.ID, containerDefinitionXml, out layoutRefId);
                var urn = _urnBuilder.GetPrecedenceConstraintUrn(parent, constraint.Name);
                var arrow = constraint.DesignArrow; //_definitionSearcher.GetPrecedenceConstraintArrow(package.Executable.ID, layoutRefId);

                var constraintElement = new PrecedenceConstraintElement(urn, constraint.Name, definition, parent);
                parent.AddChild(constraintElement);
                constraintElement.From = (ExecutableElement)fromNode;
                constraintElement.To = (ExecutableElement)toNode;
                constraintElement.Arrow = arrow;

            }
        }

        // executablesToNodes.Add(((DtsContainer)e).ID, execNode);

        private List<SsisExecutable> TopologicalOrder(List<SsisExecutable> executables, List<SsisPrecedenceConstraint> constraints)
        {
            List<SsisExecutable> res = new List<SsisExecutable>();
            
            Dictionary<string, List<string>> precedingExecutables = new Dictionary<string, List<string>>();
            Dictionary<string, SsisExecutable> executablesById = new Dictionary<string, SsisExecutable>();
            foreach (var exec in executables)
            {
                var execId = exec.RefId; // exec.ID;
                precedingExecutables.Add(execId, new List<string>());
                executablesById[execId] = exec;
            }

            foreach (var pc in constraints)
            {
                var pcId = pc.ConstrainedExecutableID; // .ID; // pc.ConstrainedExecutableID;
                precedingExecutables[pcId].Add(pc.PrecedenceExecutableID /*pc.PrecedenceExecutableID*/);
            }

            while (precedingExecutables.Any(x => x.Value.Count == 0))
            {
                var freeExecs = precedingExecutables.Where(x => x.Value.Count == 0).Select(x => executablesById[x.Key]).ToList();
                foreach (var freeEx in freeExecs)
                {
                    res.Add(freeEx);
                    foreach (var pe in precedingExecutables.Values)
                    {
                        if (pe.Contains(freeEx.RefId /*freeEx.ID*/))
                        {
                            pe.Remove(freeEx.RefId /*freeEx.ID*/);
                        }
                    }
                    var freeExId = freeEx.RefId /*freeEx.ID*/;
                    precedingExecutables.Remove(freeExId);
                }
            }

            // a cycle (if any) - add all remaining executables
            foreach (var pe in precedingExecutables)
            {
                res.Add(executablesById[pe.Key]);
            }

            return res;
        }


    }
}

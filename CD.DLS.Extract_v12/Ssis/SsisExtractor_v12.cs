using CD.DLS.Common.Structures;
using CD.DLS.Common.Tools;
using CD.DLS.DAL.Configuration;
using CD.DLS.DAL.Engine;
using CD.DLS.DAL.Managers;
using CD.DLS.DAL.Objects.Extract;
using Ionic.Zip;
using Microsoft.SqlServer.Dts.Pipeline.Wrapper;
using Microsoft.SqlServer.Dts.Runtime;
using Microsoft.SqlServer.Dts.Tasks.ExecuteSQLTask;
using Microsoft.SqlServer.Management.IntegrationServices;
using Microsoft.SqlServer.Management.Smo;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace CD.DLS.Extract_v12.Mssql.Ssis
{
    public class SsisExtractor_v12
    {
        private SsisProjectComponent _ssisProject;
        private string _outputDirPath;
        private string _relativePathBase;
        private Manifest _manifest;

        public SsisExtractor_v12(SsisProjectComponent ssisProjectComponent, string outputDirPath, string relativePathBase, Manifest manifest)
        {
            _ssisProject = ssisProjectComponent;
            _outputDirPath = outputDirPath;
            _relativePathBase = relativePathBase;
            _manifest = manifest;
        }

        public void Extract()
        {
            var projDirName = $"Project_{_ssisProject.SsisProjectComponentId}_{_ssisProject.ProjectName}";
            projDirName = FileTools.NormalizeFileName(projDirName);

            _outputDirPath = Path.Combine(_outputDirPath, projDirName);
            _relativePathBase = Path.Combine(_relativePathBase, projDirName);

            Directory.CreateDirectory(_outputDirPath);


            var serverName = _ssisProject.ServerName;
            var folderName = _ssisProject.FolderName;
            var projectName = _ssisProject.ProjectName;

            var server = new Server(serverName);
            var ssis = new IntegrationServices(server);

            if (ssis.Catalogs.Count != 1)
            {
                throw new Exception(string.Format("The SSIS server {0} is expected to contain exactly 1 catalog.", serverName));
            }
            var catalog = ssis.Catalogs["SSISDB"];
            var folder = catalog.Folders[folderName];
            var projectInfo = folder.Projects[projectName];
            //var bytes = projectInfo.GetProjectBytes();
            //   pml.ExtractProject(project, projectInfo, folderElement, _sqlContext);

            ConfigManager.Log.Important(string.Format("Extracting SSIS project {0} from {1}", _ssisProject.ProjectName, _ssisProject.ServerName));


            var ssisFiles = GetSsisCatalogProjectFiles(serverName, folderName, projectName);

            foreach (var file in ssisFiles)
            {
                var fileSerialized = file.Serialize();
                var jsonFileName = "File_" + file.Name + ".json";
                File.WriteAllText(Path.Combine(_outputDirPath, jsonFileName), fileSerialized);
                _manifest.Items.Add(new ManifestItem()
                {
                    ComponentId = _ssisProject.SsisProjectComponentId,
                    Name = jsonFileName,
                    ExtractType = file.ExtractType.ToString(),
                    RelativePath = Path.Combine(_relativePathBase, jsonFileName)
                });

                //StageManager.SaveExtract(file, _requestId, _ssisProject.SsisProjectComponentId);
            }

            var bytes = projectInfo.GetProjectBytes();

            using (var project = Project.OpenProject(new MemoryStream(bytes)))
            {
                ExtractProject(project, projectInfo);
                //var pkg2 = existingProject.PackageItems["LoadHelios_DimEmployee.dtsx"].Package; //.Execute(.... todo....)
            }


        }


        private void ExtractProject(Project project, ProjectInfo projectInfo)
        {
            /*
            var paramsProj = projectInfo.Parameters;

            foreach (Microsoft.SqlServer.Management.IntegrationServices.ParameterInfo param in paramsProj)
            {
                param.
            }
            */
            SsisProjectParameters parameters = new SsisProjectParameters() { Parameters = new List<SsisParameter>() };
            foreach (Microsoft.SqlServer.Management.IntegrationServices.ParameterInfo parameter in projectInfo.Parameters)
            {
                var param = ExtractProjectInfoParameter(parameter);
                parameters.Parameters.Add(param);
            }

            var fileSerialized = parameters.Serialize();
            var jsonFileName = "Parameters_" + parameters.Name + ".json";
            File.WriteAllText(Path.Combine(_outputDirPath, jsonFileName), fileSerialized);
            _manifest.Items.Add(new ManifestItem()
            {
                ComponentId = _ssisProject.SsisProjectComponentId,
                Name = jsonFileName,
                ExtractType = parameters.ExtractType.ToString(),
                RelativePath = Path.Combine(_relativePathBase, jsonFileName)
            });

            //StageManager.SaveExtract(parameters, _requestId, _ssisProject.SsisProjectComponentId);
            parameters = null;

            foreach (var connectionManager in project.ConnectionManagerItems)
            {
                var connMag = ExtractConnectionManager(connectionManager.ConnectionManager);
                var projConnMag = new SsisProjectConnectionManager() { ConnectionManager = connMag };

                fileSerialized = projConnMag.Serialize();
                jsonFileName = "CM_" + projConnMag.Name + ".json";
                File.WriteAllText(Path.Combine(_outputDirPath, jsonFileName), fileSerialized);
                _manifest.Items.Add(new ManifestItem()
                {
                    ComponentId = _ssisProject.SsisProjectComponentId,
                    Name = jsonFileName,
                    ExtractType = projConnMag.ExtractType.ToString(),
                    RelativePath = Path.Combine(_relativePathBase, jsonFileName)
                });
                //StageManager.SaveExtract(projConnMag, _requestId, _ssisProject.SsisProjectComponentId);
            }


            foreach (var packageItem in project.PackageItems)
            {
                var package = packageItem.Package;
                var packageInfo = projectInfo.Packages[packageItem.StreamName];

                ConfigManager.Log.Important(string.Format("Package {0}", package.Name));
                ExtractPackage(package, packageInfo);
            }

        }

        private void ExtractPackage(Package package, Microsoft.SqlServer.Management.IntegrationServices.PackageInfo packageInfo)
        {
            SsisPackage pkg = new SsisPackage() { Urn = packageInfo.Urn, PackageName = packageInfo.Name };
            pkg.Parameters = new List<SsisParameter>();

            foreach (Microsoft.SqlServer.Dts.Runtime.Parameter param in package.Parameters)
            {
                pkg.Parameters.Add(ExtractParameter(param));
            }
            pkg.ConnectionManagers = new List<SsisConnectionManager>();
            foreach (var connMag in package.Connections)
            {
                pkg.ConnectionManagers.Add(ExtractConnectionManager(connMag));
            }


            var container = ExtractDtsContainer(package, package);
            pkg.Container = container;

            var fileSerialized = pkg.Serialize();
            var jsonFileName = "Package_" + pkg.Name + ".json";
            File.WriteAllText(Path.Combine(_outputDirPath, jsonFileName), fileSerialized);
            _manifest.Items.Add(new ManifestItem()
            {
                ComponentId = _ssisProject.SsisProjectComponentId,
                Name = jsonFileName,
                ExtractType = pkg.ExtractType.ToString(),
                RelativePath = Path.Combine(_relativePathBase, jsonFileName)
            });
            //StageManager.SaveExtract(pkg, _requestId, _ssisProject.SsisProjectComponentId);
        }

        public DLS.DAL.Objects.Extract.DtsContainer ExtractDtsContainer(Microsoft.SqlServer.Dts.Runtime.DtsContainer container, Package package)
        {
            //host.CreationName.StartsWith("SSIS.ExecutePackageTask", StringComparison.Ordinal)
            var creationName = container.CreationName;
            DLS.DAL.Objects.Extract.DtsContainer res = null;

            if (container is TaskHost)
            {
                var taskHost = container as TaskHost;

                if (creationName.StartsWith("SSIS.Pipeline", StringComparison.Ordinal))
                {
                    res = new SsisDfTask()
                    {
                        Children = new List<DLS.DAL.Objects.Extract.DtsContainer>(),
                        CreationName = container.CreationName,
                        Enabled = !container.Disable,
                        //Expressions = new List<SsisExpression>(),
                        Name = container.Name,
                        Properties = new List<SsisProperty>(),
                        Variables = new List<SsisVariable>(),
                        PrecedenceConstraints = new List<SsisPrecedenceConstraint>(),
                        Components = new List<SsisDfComponent>()
                    };
                    var pipe = (MainPipe)(taskHost.InnerObject);
                    ExtractDataflowComponents(pipe, (SsisDfTask)res);
                    ExtractDataflowPaths(pipe, (SsisDfTask)res);
                }
                else if (creationName.StartsWith("SSIS.ExecutePackageTask", StringComparison.Ordinal))
                {
                    res = new SsisExecuteSsisPackabeTask()
                    {
                        Children = new List<DLS.DAL.Objects.Extract.DtsContainer>(),
                        CreationName = container.CreationName,
                        Enabled = !container.Disable,
                        //Expressions = new List<SsisExpression>(),
                        Name = container.Name,
                        Properties = new List<SsisProperty>(),
                        PrecedenceConstraints = new List<SsisPrecedenceConstraint>(),
                        Variables = new List<SsisVariable>()
                    };

                }
                else if (creationName.StartsWith("Microsoft.SqlServer.Dts.Tasks.ExpressionTask.ExpressionTask", StringComparison.Ordinal))
                {
                    res = new SsisExpressionTask()
                    {
                        Children = new List<DLS.DAL.Objects.Extract.DtsContainer>(),
                        CreationName = container.CreationName,
                        Enabled = !container.Disable,
                        //Expressions = new List<SsisExpression>(),
                        Name = container.Name,
                        Properties = new List<SsisProperty>(),
                        PrecedenceConstraints = new List<SsisPrecedenceConstraint>(),
                        Variables = new List<SsisVariable>()
                    };
                }
                else
                {
                    var innerObj = taskHost.InnerObject;
                    if (!(innerObj is Microsoft.SqlServer.Dts.Runtime.Task))
                    {
                        return null;
                    }
                    var task = ((Microsoft.SqlServer.Dts.Runtime.Task)innerObj);

                    ///**/
                    if (task is ExecuteSQLTask)
                    {
                        var sqlTask = task as ExecuteSQLTask;
                        var connectionName = sqlTask.Connection;
                        var connectionId = sqlTask.GetConnectionID(package.Connections, connectionName);
                        var parameterBindings = ExtractParameterMappings(sqlTask.ParameterBindings);
                        var statementSource = sqlTask.SqlStatementSource;
                        var statementSourcetype = sqlTask.SqlStatementSourceType.ToString();
                        res = new SsisSqlTask()
                        {
                            Children = new List<DLS.DAL.Objects.Extract.DtsContainer>(),
                            CreationName = container.CreationName,
                            Enabled = !container.Disable,
                            //Expressions = new List<SsisExpression>(),
                            Name = container.Name,
                            Properties = new List<SsisProperty>(),
                            Variables = new List<SsisVariable>(),
                            PrecedenceConstraints = new List<SsisPrecedenceConstraint>(),
                            StatementSource = statementSource,
                            StatementSourceType = statementSourcetype,
                            Parameters = parameterBindings,
                            ConnectionID = connectionId,
                            ConnectionName = connectionName
                        };
                    }
                }

                if (res == null)
                {
                    res = new SsisTask()
                    {
                        Children = new List<DLS.DAL.Objects.Extract.DtsContainer>(),
                        CreationName = container.CreationName,
                        Enabled = !container.Disable,
                        //Expressions = new List<SsisExpression>(),
                        Name = container.Name,
                        Properties = new List<SsisProperty>(),
                        PrecedenceConstraints = new List<SsisPrecedenceConstraint>(),
                        Variables = new List<SsisVariable>()
                    };
                }

                res.Properties = ExtractProperties(taskHost.Properties, taskHost);

            }
            else if (res == null)
            {
                res = new DLS.DAL.Objects.Extract.DtsContainer()
                {
                    Children = new List<DLS.DAL.Objects.Extract.DtsContainer>(),
                    CreationName = container.CreationName,
                    Enabled = !container.Disable,
                    //Expressions = new List<SsisExpression>(),
                    Name = container.Name,
                    Properties = new List<SsisProperty>(),
                    PrecedenceConstraints = new List<SsisPrecedenceConstraint>(),
                    Variables = new List<SsisVariable>()
                };

            }

            /*
            if (container is IDTSPropertiesProvider)
            {
                res.Expressions = ExtractExpressions(container as IDTSPropertiesProvider);
            }
            */

            Executables executables = null;
            PrecedenceConstraints precedenceConstraints = null;

            if (container is Microsoft.SqlServer.Dts.Runtime.Sequence)
            {
                var typed = ((Microsoft.SqlServer.Dts.Runtime.Sequence)container);
                executables = typed.Executables;
                precedenceConstraints = typed.PrecedenceConstraints;
            }
            else if (container is ForEachLoop)
            {
                var typed = ((Microsoft.SqlServer.Dts.Runtime.ForEachLoop)container);
                executables = typed.Executables;
                precedenceConstraints = typed.PrecedenceConstraints;
            }
            else if (container is ForLoop)
            {
                var typed = ((Microsoft.SqlServer.Dts.Runtime.ForLoop)container);
                executables = typed.Executables;
                precedenceConstraints = typed.PrecedenceConstraints;
            }
            else if (container is Package)
            {
                var typed = ((Microsoft.SqlServer.Dts.Runtime.Package)container);
                executables = typed.Executables;
                precedenceConstraints = typed.PrecedenceConstraints;
            }


            if (executables != null)
            {
                foreach (var exec in executables)
                {
                    var extract = ExtractDtsContainer((Microsoft.SqlServer.Dts.Runtime.DtsContainer)exec, package);
                    res.Children.Add(extract);
                }
            }

            if (precedenceConstraints != null)
            {
                res.PrecedenceConstraints = ExtractPrecedenceConstraints(precedenceConstraints);
            }

            res.Variables = ExtractVariables(container);
            res.ID = container.ID;

            return res;
        }

        private void ExtractDataflowPaths(MainPipe pipe, SsisDfTask res)
        {
            res.Paths = new List<SsisDfPath>();
            foreach (IDTSPath100 path in pipe.PathCollection)
            {
                res.Paths.Add(new SsisDfPath()
                {
                    Name = path.Name,
                    ID = path.ID,
                    IdString = path.IdentificationString,
                    SourceComponentIdString = path.StartPoint.Component.IdentificationString,
                    TargetComponentIdString = path.EndPoint.Component.IdentificationString,
                    SourceIdString = path.StartPoint.IdentificationString,
                    TargetIdString = path.EndPoint.IdentificationString
                });
            }
        }

        private List<SsisPrecedenceConstraint> ExtractPrecedenceConstraints(PrecedenceConstraints precedenceConstraints)
        {
            List<SsisPrecedenceConstraint> res = new List<SsisPrecedenceConstraint>();
            foreach (var pc in precedenceConstraints)
            {
                res.Add(new SsisPrecedenceConstraint()
                {
                    ID = pc.ID,
                    Name = pc.Name,
                    ConstrainedExecutableID = ((Microsoft.SqlServer.Dts.Runtime.DtsContainer)(pc.ConstrainedExecutable)).ID,
                    PrecedenceExecutableID = ((Microsoft.SqlServer.Dts.Runtime.DtsContainer)(pc.PrecedenceExecutable)).ID
                });
            }
            return res;
        }

        private void ExtractDataflowComponents(MainPipe pipe, SsisDfTask task)
        {
            task.Components = new List<SsisDfComponent>();

            var components = pipe.ComponentMetaDataCollection;
            foreach (object componentObj in components)
            {
                var extr = new SsisDfComponent();

                var fullName = componentObj.GetType().FullName;
                extr.ObjectType = fullName;

                if (!(componentObj is IDTSComponentMetaData100))
                {
                    continue;
                }

                IDTSComponentMetaData100 component = (IDTSComponentMetaData100)(componentObj);
                extr.Name = component.Name;
                extr.IdString = component.IdentificationString;
                extr.ID = component.ID;
                extr.ClassId = component.ComponentClassID;
                extr.Contract = component.ContactInfo;
                var contractBase = extr.Contract.Split(';')[0];
                extr.ContractBase = contractBase;
                extr.Properties = new List<SsisProperty>();

                extr.Connections = new List<SsisRuntimeConnection>();
                foreach (IDTSRuntimeConnection100 connection in component.RuntimeConnectionCollection)
                {
                    extr.Connections.Add(new SsisRuntimeConnection()
                    {
                        Name = connection.Name,
                        ID = connection.ID,
                        IdentificationString = connection.IdentificationString,
                        ConnectionManagerID = connection.ConnectionManagerID
                    });
                }

                var properties = component.CustomPropertyCollection;
                extr.Properties = ExtractProperties(properties);

                IDTSInputCollection100 inputs = component.InputCollection;
                IDTSOutputCollection100 outputs = component.OutputCollection;


                extr.Outputs = new List<SsisDfOutput>();
                foreach (IDTSOutput100 output in outputs)
                {
                    var extOutput = new SsisDfOutput() { Description = output.Description, ID = output.ID, Name = output.Name, Columns = new List<DfColumn>(), ExternalColumns = new List<DfColumn>(), IsErrorOutput = output.IsErrorOut, IdString = output.IdentificationString };

                    foreach (IDTSOutputColumn100 outCol in output.OutputColumnCollection)
                    {
                        extOutput.Columns.Add(new DfColumn()
                        {
                            CodePage = outCol.CodePage,
                            DataType = outCol.DataType.ToString(),
                            Description = outCol.Description,
                            ID = outCol.ID,
                            Length = outCol.Length,
                            MappedColumnID = outCol.MappedColumnID,
                            Name = outCol.Name,
                            Precision = outCol.Precision,
                            Scale = outCol.Scale,
                            ExternalColumnID = outCol.ExternalMetadataColumnID,
                            CustomProperties = ExtractProperties(outCol.CustomPropertyCollection),
                            LineageID = outCol.LineageID,
                            IdentificationString = outCol.IdentificationString
                        });
                    }

                    foreach (IDTSExternalMetadataColumn100 externalCol in output.ExternalMetadataColumnCollection)
                    {
                        extOutput.ExternalColumns.Add(new DfColumn()
                        {
                            CodePage = externalCol.CodePage,
                            DataType = externalCol.DataType.ToString(),
                            Description = externalCol.Description,
                            ID = externalCol.ID,
                            Length = externalCol.Length,
                            MappedColumnID = externalCol.MappedColumnID,
                            Name = externalCol.Name,
                            Precision = externalCol.Precision,
                            Scale = externalCol.Scale,
                            CustomProperties = ExtractProperties(externalCol.CustomPropertyCollection),
                            IdentificationString = externalCol.IdentificationString
                        });
                    }

                    extr.Outputs.Add(extOutput);
                }

                extr.Inputs = new List<SsisDfInput>();
                foreach (IDTSInput100 input in inputs)
                {
                    var extInput = new SsisDfInput() { Description = input.Description, ID = input.ID, Name = input.Name, Columns = new List<DfColumn>(), ExternalColumns = new List<DfColumn>(), IdString = input.IdentificationString };

                    foreach (IDTSInputColumn100 inCol in input.InputColumnCollection)
                    {
                        extInput.Columns.Add(new DfColumn()
                        {
                            CodePage = inCol.CodePage,
                            DataType = inCol.DataType.ToString(),
                            Description = inCol.Description,
                            ID = inCol.ID,
                            Length = inCol.Length,
                            MappedColumnID = inCol.MappedColumnID,
                            Name = inCol.Name,
                            Precision = inCol.Precision,
                            Scale = inCol.Scale,
                            ExternalColumnID = inCol.ExternalMetadataColumnID,
                            CustomProperties = ExtractProperties(inCol.CustomPropertyCollection),
                            LineageID = inCol.LineageID,
                            IdentificationString = inCol.IdentificationString
                        });
                    }

                    foreach (IDTSExternalMetadataColumn100 externalCol in input.ExternalMetadataColumnCollection)
                    {
                        extInput.ExternalColumns.Add(new DfColumn()
                        {
                            CodePage = externalCol.CodePage,
                            DataType = externalCol.DataType.ToString(),
                            Description = externalCol.Description,
                            ID = externalCol.ID,
                            Length = externalCol.Length,
                            MappedColumnID = externalCol.MappedColumnID,
                            Name = externalCol.Name,
                            Precision = externalCol.Precision,
                            Scale = externalCol.Scale,
                            CustomProperties = ExtractProperties(externalCol.CustomPropertyCollection),
                            IdentificationString = externalCol.IdentificationString
                        });
                    }

                    extr.Inputs.Add(extInput);
                }

                task.Components.Add(extr);
            }
        }

        private List<SsisProperty> ExtractProperties(IDTSCustomPropertyCollection100 properties)
        {
            var res = new List<SsisProperty>();

            foreach (IDTSCustomProperty100 property in properties)
            {
                var val = property.Value;
                if (val != null)
                {
                    res.Add(new SsisProperty() { Name = property.Name, Value = val.ToString() });
                }
            }
            return res;
        }

        //private List<SsisExpression> ExtractExpressions(IDTSPropertiesProvider provider)
        //{

        //    List<SsisExpression> res = new List<SsisExpression>();

        //    foreach (DtsProperty property in provider.Properties)
        //    {
        //        string expression = provider.GetExpression(property.Name);
        //        SsisExpression ex = new SsisExpression()
        //        {
        //            ExpressionName = property.Name,
        //            Value = expression
        //        };
        //        res.Add(ex);
        //    }

        //    return res;
        //}

        private List<SsisProperty> ExtractProperties(DtsProperties properties, object provider)
        {
            List<SsisProperty> props = new List<SsisProperty>();
            foreach (DtsProperty p in properties)
            {
                try
                {
                    if (p.Get)
                    {
                        string expression = null;

                        if (provider is IDTSPropertiesProvider)
                        {

                            expression = ((IDTSPropertiesProvider)provider).GetExpression(p.Name);

                        }
                        props.Add(new SsisProperty()
                        {
                            Name = p.Name,
                            Value = p.GetValue(provider) as string,
                            Expression = expression
                        });
                    }
                    //p.GetExpression
                }
                catch (Exception ex)
                {
                    ConfigManager.Log.Important(string.Format("-- Failed to extract property {0}: {1}", p.Name, ex.Message));
                    continue;
                }
            }
            return props;
        }

        private List<SsisVariable> ExtractVariables(Microsoft.SqlServer.Dts.Runtime.DtsContainer container)
        {
            List<SsisVariable> res = new List<SsisVariable>();

            foreach (Variable variable in container.Variables)
            {
                string value = string.Empty;
                try
                {
                    value = variable.Value.ToString();
                }
                catch (Exception ex)
                {
                    ConfigManager.Log.Important(string.Format("-- Failed to extract variable {0}: {1}", variable.Name, ex.Message));
                    continue;
                }
                res.Add(new SsisVariable()
                {
                    DataType = variable.DataType.ToString(),
                    ID = variable.ID,
                    Name = variable.Name,
                    QualifiedName = variable.QualifiedName,
                    Value = variable.Value.ToString(),
                    Namespace = variable.Namespace
                });
            }

            return res;
        }

        private SsisParameter ExtractParameter(Microsoft.SqlServer.Dts.Runtime.Parameter parameter)
        {
            SsisParameter res = new SsisParameter()
            {
                DataType = parameter.DataType.ToString(),
                Name = parameter.Name,
                Value = parameter.Value.ToString(),
                ID = parameter.ID
            };
            return res;
        }

        private SsisParameter ExtractProjectInfoParameter(Microsoft.SqlServer.Management.IntegrationServices.ParameterInfo parameter)
        {
            SsisParameter res = new SsisParameter()
            {
                DataType = parameter.DataType.ToString(),
                Name = parameter.Name,
                Value = parameter.DefaultValue == null ? parameter.DesignDefaultValue.ToString() : parameter.DefaultValue.ToString(),
                ID = parameter.Id.ToString()
            };

            //int charIndex = res.Value.IndexOf(@"\");
            res.Value = res.Value.Replace("\\", @"\");

            return res;
        }

        private SsisConnectionManager ExtractConnectionManager(ConnectionManager manager)
        {
            SsisConnectionManager res = new SsisConnectionManager()
            {
                ConnectionString = manager.ConnectionString,
                CreationName = manager.CreationName,
                ID = manager.ID,
                Name = manager.Name,
                Scope = (SsisConnectionManagerScope)(Enum.Parse(typeof(SsisConnectionManagerScope), manager.Scope.ToString()))
            };

            res.Properties = ExtractProperties(manager.Properties, manager);
            return res;
        }

        private List<SsisParameterMapping> ExtractParameterMappings(IDTSParameterBindings bindings)
        {
            List<SsisParameterMapping> res = new List<SsisParameterMapping>();
            foreach (IDTSParameterBinding binding in bindings)
            {
                res.Add(new SsisParameterMapping()
                {
                    Direction = (SsisParameterDirection)binding.ParameterDirection,
                    Name = binding.ParameterName.ToString(),
                    Variablename = binding.DtsVariableName
                });
            }
            return res;
        }


        private List<ExtractObject> GetSsisCatalogProjectFiles(string serverName, string folderName, string projectName)
        {
            var connectionString = String.Format(@"Data Source={0};Integrated Security=True", serverName);
            SqlCommand cmd = new SqlCommand(connectionString);

            //NetBridge nb = new NetBridge(connectionString);

            NetBridge nb = new NetBridge(true);

            nb.SetConnectionString(connectionString);


            //Dictionary<SsisProjectComponent, List<TextFile>> res = new Dictionary<SsisProjectComponent, List<TextFile>>();
            var tempFolder = Path.GetTempPath();

            var subFolder = Path.Combine(tempFolder, "BIDoc", projectName);
            if (Directory.Exists(subFolder))
            {
                Directory.Delete(subFolder, true);
            }
            Directory.CreateDirectory(subFolder);
            var zipName = projectName + ".zip";
            var projContent = nb.ExecuteProcedureScalar("[SSISDB].[catalog].[get_project]", new Dictionary<string, object>()
      {{"folder_name", folderName}, {"project_name", projectName}});
            //_settings.Projects[0]
            var zipPath = Path.Combine(subFolder, zipName);
            File.WriteAllBytes(zipPath, (byte[])projContent);

            Ionic.Zip.ZipFile zip = new ZipFile(zipPath);
            List<ExtractObject> files = new List<ExtractObject>();
            foreach (ZipEntry entry in zip.Entries)
            {
                string content;
                using (MemoryStream ms = new MemoryStream())
                {
                    entry.Extract(ms);
                    ms.Position = 0;
                    StreamReader sr = new StreamReader(ms);
                    content = sr.ReadToEnd();
                }
                //XmlDocument doc = new XmlDocument();
                //XmlDocument designDoc = null;
                //doc.LoadXml(content);
                //Dictionary<string, XmlElement> nodeLayoutXmls = new Dictionary<string, XmlElement>();

                if (entry.FileName.EndsWith(".dtsx"))
                {
                    files.Add(new SsisPackageFile() { FileName = entry.FileName, FullPath = Path.Combine(folderName, projectName, entry.FileName), Content = content });
                }
                else
                {
                    files.Add(new SsisProjectFile() { FileName = entry.FileName, FullPath = Path.Combine(folderName, projectName, entry.FileName), Content = content });
                }
            }

            return files;
        }
    }
}


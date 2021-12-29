using CD.DLS.Model.Mssql.Ssis;
using CD.DLS.DAL.Managers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml;
using CD.BIDoc.Core.Parse.Mssql.Ssis;
using CD.DLS.DAL.Objects.SsisDiagram;
using CD.DLS.DAL.Objects.Extract;

namespace CD.DLS.Parse.Mssql.Ssis
{
    public class SsisXmlProvider
    {
        public class SsisFile
        {
            public string FileName { get; set; }
            public string FullPath { get; set; }
            public string Content { get; set; }
            public XmlDocument XmlContent { get; set; }
            public XmlDocument DesignTimeProperties { get; set; }
            public Dictionary<string, ElementLayoutDesign> NodeLayoutDesigns { get; set; }
            public Dictionary<string, XmlElement> NodeLayoutDesignXmls { get; set; }
            public Dictionary<string, DesignArrow> EdgeLayouts { get; set; }
        }

        //public class ElementLayoutDesign
        //{
        //    public DesignPoint TopLeft { get; set; }
        //    public DesignPoint Size { get; set; }
        //}

        private Dictionary<string, SsisFile> _files = new Dictionary<string, SsisFile>();

        private SsisProject _project;

        public SsisProject Project { get => _project; }

        private XmlDocument DesignTimeProperties { get; set; }
        private Dictionary<string, ElementLayoutDesign> NodeLayoutDesigns { get; set; }
        private Dictionary<string, DesignArrow> EdgeLayouts { get; set; }
        private Dictionary<string, XmlElement> NodeLayoutDesignXmls { get; set; }

        public SsisXmlProvider(Guid extractId, int configComponentId, StageManager _stageManager, DAL.Objects.Extract.SsisPackageFile packageExtract = null, bool loadPackages = true)
        {
            List<DAL.Objects.Extract.SsisPackageFile> packageFileList = new List<DAL.Objects.Extract.SsisPackageFile>();

            if (loadPackages)
            {
                if (packageExtract == null)
                {
                    packageFileList = _stageManager.GetExtractItems(extractId, configComponentId, DLS.DAL.Objects.Extract.ExtractTypeEnum.SsisPackageFile).Select(x => (DLS.DAL.Objects.Extract.SsisPackageFile)x).ToList();
                }
                else
                {
                    packageFileList = new List<DAL.Objects.Extract.SsisPackageFile>() { packageExtract };
                }
            }
            _project = new SsisProject()
            {
                Packages = new List<SsisPackage>(),
                ProjectConnectionManagers = new List<SsisConnectionManager>(),
                ProjectParameters = new List<SsisParameter>()
            };

            var projectFileList = _stageManager.GetExtractItems(extractId, configComponentId, DLS.DAL.Objects.Extract.ExtractTypeEnum.SsisProjectFile).Select(x => (DLS.DAL.Objects.Extract.SsisProjectFile)x).ToList();

            //var fileList = GetSsisCatalogProjectFiles(serverName, folderName, projectName);

            var manifest = projectFileList.First(x => x.Name == "@Project.manifest");
            XmlDocument manifestXml = new XmlDocument();
            manifestXml.LoadXml(manifest.Content);

            var managers = _stageManager.GetExtractItems(extractId, configComponentId, DLS.DAL.Objects.Extract.ExtractTypeEnum.SsisConnectionManagerFile).Select(x => (DLS.DAL.Objects.Extract.SsisConnectionManagerFile)x).ToList();
            foreach (var mng in managers)
            {
                LoadProjectConnectionManager(mng);
                //var fileName = mng.GetAttribute("SSIS:Name");
                //var file = projectFileList.First(x => Uri.UnescapeDataString(x.FileName) == fileName);
                //XmlDocument fileXml = new XmlDocument();
                //fileXml.LoadXml(file.Content);
                //var objectId = ((XmlElement)(fileXml.GetElementsByTagName("DTS:ConnectionManager")[0])).GetAttribute("DTS:DTSID");
                //_files.Add(objectId, new SsisFile() { Content = file.Content, FileName = file.Name, XmlContent = fileXml });
            }

            var paramsFile = _stageManager.GetExtractItems(extractId, configComponentId, DLS.DAL.Objects.Extract.ExtractTypeEnum.SsisParametersFile).Select(x => (DLS.DAL.Objects.Extract.SsisParametersFile)x).FirstOrDefault();
            if (paramsFile != null)
            {
                LoadProjectParameters(paramsFile);
            }

            foreach (var package in packageFileList)
            {
                LoadPackage(package);
            }


            //var packages = ((XmlElement)(manifestXml.GetElementsByTagName("SSIS:Packages")[0])).GetElementsByTagName("SSIS:Package");
            //foreach (XmlElement pkg in packages)
            //{
            //    var fileName = pkg.GetAttribute("SSIS:Name");
            //    var file = packageFileList.FirstOrDefault(x => Uri.UnescapeDataString(x.FileName) == fileName);
            //    if (file == null)
            //    {
            //        continue;
            //    }
            //    XmlDocument fileXml = new XmlDocument();
            //    fileXml.LoadXml(file.Content);
            //    var objectId = ((XmlElement)(fileXml.GetElementsByTagName("DTS:Executable")[0])).GetAttribute("DTS:DTSID");
            //    var newFile = new SsisFile() { Content = file.Content, FileName = file.Name, XmlContent = fileXml };
            //    _files.Add(objectId, newFile);

            //    XmlDocument designDoc = null;
            //    XmlDocument doc = fileXml;
            //        Dictionary<string, ElementLayoutDesign> nodeLayouts = new Dictionary<string, ElementLayoutDesign>();
            //        Dictionary<string, DesignArrow> edgeLayouts = new Dictionary<string, DesignArrow>();
            //        Dictionary<string, XmlElement> nodeLayoutXmls = new Dictionary<string, XmlElement>();
            //        var designProps = doc.GetElementsByTagName("DTS:DesignTimeProperties");
            //        if (designProps.Count > 0)
            //        {
            //            var designContent = (designProps[0] as XmlElement).InnerText;
            //            designDoc = new XmlDocument();
            //            designDoc.LoadXml(designContent);

            //            var layoutNodes = designDoc.GetElementsByTagName("NodeLayout");
            //            foreach (XmlElement node in layoutNodes)
            //            {
            //                nodeLayouts[node.GetAttribute("Id")] = GetNodeLayoutDesign(node);
            //                nodeLayoutXmls[node.GetAttribute("Id")] = node;
            //            }

            //            var containerLayoutNodes = designDoc.GetElementsByTagName("ContainerLayout");
            //            foreach (XmlElement node in containerLayoutNodes)
            //            {
            //                nodeLayouts[node.GetAttribute("Id")] = GetNodeLayoutDesign(node);
            //                nodeLayoutXmls[node.GetAttribute("Id")] = node;
            //            }

            //            var edgeLayoutNodes = designDoc.GetElementsByTagName("EdgeLayout");
            //            foreach (XmlElement node in edgeLayoutNodes)
            //            {
            //                edgeLayouts[node.GetAttribute("Id")] = ParseDesignArrow(node);
            //            }

            //        }

            //    newFile.DesignTimeProperties = designDoc;
            //    newFile.NodeLayoutDesigns = nodeLayouts;
            //    newFile.EdgeLayouts = edgeLayouts;
            //    newFile.NodeLayoutDesignXmls = nodeLayoutXmls;

            //    //files.Add(new SsisFile() { FileName = entry.FileName, FullPath = Path.Combine(folderName, projectName, entry.FileName), Content = content, XmlContent = doc, 
            //    // DesignTimeProperties = designDoc, NodeLayoutDesigns = nodeLayouts, EdgeLayouts = edgeLayouts, NodeLayoutDesignXmls = nodeLayoutXmls });
                

            //}

            ////var managers = ((XmlElement)(manifestXml.GetElementsByTagName("SSIS:ConnectionManagers")[0])).GetElementsByTagName("SSIS:ConnectionManager");
            ////foreach (XmlElement mng in managers)
            ////{
            ////    var fileName = mng.GetAttribute("SSIS:Name");
            ////    var file = projectFileList.First(x => Uri.UnescapeDataString(x.FileName) == fileName);
            ////    XmlDocument fileXml = new XmlDocument();
            ////    fileXml.LoadXml(file.Content);
            ////    var objectId = ((XmlElement)(fileXml.GetElementsByTagName("DTS:ConnectionManager")[0])).GetAttribute("DTS:DTSID");
            ////    _files.Add(objectId, new SsisFile() { Content = file.Content, FileName = file.Name, XmlContent = fileXml });
            ////}

            //var paramsFile = projectFileList.First(x => x.Name == "Project.params");

            //XmlDocument paramsXml = new XmlDocument();
            //paramsXml.LoadXml(paramsFile.Content);

            //_files.Add("@Project.params", new SsisFile() { Content = paramsFile.Content, FileName = paramsFile.Name, XmlContent = paramsXml });

            //var olps = fileList.Where(x => x.FileName.ToLower().Contains("ExportOLAP".ToLower())).ToList();

            //_files = fileList.ToDictionary(x => Uri.UnescapeDataString(x.FileName), x => x, StringComparer.OrdinalIgnoreCase);
        }

        private void LoadPackage(SsisPackageFile packageFile)
        {
            SsisPackage pkg = new SsisPackage();
            pkg.FileName = packageFile.FileName;

            XmlDocument xml = new XmlDocument();
            xml.LoadXml(packageFile.Content);

            LoadPackageNodeDesigns(xml.DocumentElement);

            var connectionManagersRoot = xml.GetElementsByTagName("DTS:ConnectionManagers");
            if(connectionManagersRoot.Count > 0)
            {
                var connMgrsRoot = connectionManagersRoot[0];
                foreach (XmlElement conMgr in connMgrsRoot.ChildNodes)
                {
                    var connMgrLoaded = LoadConnectionmanger(conMgr);
                    pkg.ConnectionManagers.Add(connMgrLoaded);
                }
            }

            pkg.PackageName = packageFile.Name;

            var parametersRootList = xml.GetElementsByTagName("DTS:PackageParameters");
            
            if (parametersRootList.Count > 0)
            {
                var parametersRoot = (XmlElement)parametersRootList[0];
                var parameters = parametersRoot.ChildNodes;
                foreach (XmlElement par in parameters)
                {
                    SsisParameter packageParam = LoadPackageParameter(par);
                    pkg.Parameters.Add(packageParam);
                }
            }




            pkg.XmlDefinition = xml.OuterXml;
            pkg.Executable = LoadSsisExecutable(xml.DocumentElement);

            pkg.Project = _project;
            _project.Packages.Add(pkg);
            return;
        }

        private void LoadPackageNodeDesigns(XmlElement xml)
        {
            Dictionary<string, ElementLayoutDesign> nodeLayouts = new Dictionary<string, ElementLayoutDesign>();
            Dictionary<string, DesignArrow> edgeLayouts = new Dictionary<string, DesignArrow>();
            Dictionary<string, XmlElement> nodeLayoutXmls = new Dictionary<string, XmlElement>();
            XmlDocument designDoc = null;
            var designProps = xml.GetElementsByTagName("DTS:DesignTimeProperties");
            if (designProps.Count > 0)
            {
                var designContent = (designProps[0] as XmlElement).InnerText;
                designDoc = new XmlDocument();
                designDoc.LoadXml(designContent);

                var layoutNodes = designDoc.GetElementsByTagName("NodeLayout");
                foreach (XmlElement node in layoutNodes)
                {
                    nodeLayouts[node.GetAttribute("Id")] = GetNodeLayoutDesign(node);
                    nodeLayoutXmls[node.GetAttribute("Id")] = node;
                }

                var containerLayoutNodes = designDoc.GetElementsByTagName("ContainerLayout");
                foreach (XmlElement node in containerLayoutNodes)
                {
                    nodeLayouts[node.GetAttribute("Id")] = GetNodeLayoutDesign(node);
                    nodeLayoutXmls[node.GetAttribute("Id")] = node;
                }

                var edgeLayoutNodes = designDoc.GetElementsByTagName("EdgeLayout");
                foreach (XmlElement node in edgeLayoutNodes)
                {
                    edgeLayouts[node.GetAttribute("Id")] = ParseDesignArrow(node);
                }

            }

            DesignTimeProperties = designDoc;
            NodeLayoutDesigns = nodeLayouts;
            EdgeLayouts = edgeLayouts;
            NodeLayoutDesignXmls = nodeLayoutXmls;
        }



        private SsisExecutable LoadSsisExecutable(XmlElement xml)
        {
            SsisExecutable exec = new SsisExecutable();

            exec.ID = GetDtsId(xml);
            exec.Name = GetObjectName(xml);
            exec.RefId = GetRefId(xml);
            exec.CreationName = GetCreationName(xml);
            exec.Enabled = !GetDisabled(xml);
            exec.Layout = NodeLayoutDesigns[exec.ID];
            exec.Variables = GetVariables(xml);

            return exec;
        }

        private List<SsisVariable> GetVariables(XmlElement xml)
        {
            throw new NotImplementedException();
        }

        private void LoadProjectConnectionManager(SsisConnectionManagerFile conmgrFile)
        {
            XmlDocument xml = new XmlDocument();
            xml.LoadXml(conmgrFile.Content);
            var connManager = LoadConnectionmanger(xml.DocumentElement);
            connManager.Scope = SsisConnectionManagerScope.Project;
            _project.ProjectConnectionManagers.Add(connManager);
        }

        private string GetRefId(XmlElement xml)
        {
            return xml.GetAttribute("DTS:refId");
        }

        private string GetDtsId(XmlElement xml)
        {
            return xml.GetAttribute("DTS:DTSID");
        }

        private string GetCreationName(XmlElement xml)
        {
            return xml.GetAttribute("DTS:CreationName");
        }

        private string GetObjectName(XmlElement xml)
        {
            return xml.GetAttribute("DTS:ObjectName");
        }

        private bool GetDisabled(XmlElement xml)
        {
            bool disabled = false;
            var parsed = bool.TryParse(xml.GetAttribute("DTS:Disabled"), out disabled);
            if (!parsed)
            {
                disabled = false;
            }
            return disabled;
        }

        private SsisConnectionManager LoadConnectionmanger(XmlElement xml)
        {
            /*
    <DTS:ConnectionManager DTS:CreationName="OLEDB" DTS:DTSID="{7103331F-8BA6-4930-86F1-9D0B55ABC68B}" DTS:ObjectName="BE" DTS:refId="Package.ConnectionManagers[BE]">
      <DTS:ObjectData>
        <DTS:ConnectionManager DTS:ConnectionString="data source=tdchbi01.ita.itadel.dk;initial catalog=BE;provider=SQLNCLI11.1;integrated security=SSPI" />
      </DTS:ObjectData>
    </DTS:ConnectionManager>
             */
            //xml.
            //DTS:CreationName="OLEDB" DTS:DTSID="{7103331F-8BA6-4930-86F1-9D0B55ABC68B}" DTS:ObjectName="BE" DTS:refId="Package.ConnectionManagers[BE]
            SsisConnectionManager mgr = new SsisConnectionManager()
            {
                RefId = GetRefId(xml),
                ID = GetDtsId(xml),
                XmlDefinition = xml.OuterXml,
                CreationName = GetCreationName(xml),
                ConnectionString = ((XmlElement)(xml.GetElementsByTagName("DTS:ObjectData")[0].FirstChild)).GetAttribute("DTS:ConnectionString"),
                Name = GetObjectName(xml),
                Scope = SsisConnectionManagerScope.Package
            };

            return mgr;
        }

        private SsisParameter LoadProjectParameter(XmlElement xml)
        {
            var props = LoadPropertiesWithSSISPrefix(xml);

            var param = new SsisParameter()
            {
                DataType = "",
                Name = xml.GetAttribute("SSIS:Name"),
                Sensitive = bool.Parse(props["Sensitive"]),
                Value = props["Value"],
                XmlDefinition = xml.OuterXml
            };

            return param;
        }

        private SsisParameter LoadPackageParameter(XmlElement xml)
        {
            /*
<DTS:PackageParameter DTS:CreationName="" DTS:DataType="3" DTS:Description="" DTS:DTSID="{037F6792-5D61-4349-95DE-74129519C9D9}" DTS:ObjectName="BatchID" DTS:Required="0" DTS:Sensitive="0">
      <DTS:Property DTS:DataType="3" DTS:Name="ParameterValue">0</DTS:Property>
    </DTS:PackageParameter>
             */

            var param = new SsisParameter()
            {
                DataType = "",
                Name = GetObjectName(xml),
                Sensitive = bool.Parse(xml.GetAttribute("DTS:Sensitive")),
                //Value = props["Value"],
                XmlDefinition = xml.OuterXml
            };

            var props = xml.GetElementsByTagName("DTS:Property");
            if (props.Count == 0)
            {
                return param;
            }

            var prop = (XmlElement)(props[0]);
            if (prop.GetAttribute("DTS:Name") != "ParameterValue")
            {
                return param;
            }

            if (!param.Sensitive)
            {
                param.Value = prop.Value;
            }
            
            return param;
        }

        private void LoadProjectParameters(SsisParametersFile file)
        {
            XmlDocument xml = new XmlDocument();
            xml.LoadXml(file.Content);
            var pars = xml.GetElementsByTagName("SSIS:Parameter");
            foreach (XmlElement param in pars)
            {
                var loaded = LoadProjectParameter(param);
                _project.ProjectParameters.Add(loaded);
            }
        }

        private Dictionary<string, string> LoadPropertiesWithSSISPrefix(XmlElement xml)
        {
            Dictionary<string, string> res = new Dictionary<string, string>();

            var propsElements = xml.GetElementsByTagName("SSIS:Properties");
            if (propsElements.Count == 0)
            {
                return res;
            }

            var propsElement = propsElements[0];
            var props = ((XmlElement)propsElement).GetElementsByTagName("SSIS:Property");
            foreach (XmlElement prop in props)
            {
                res.Add(prop.GetAttribute("SSIS:Name"), prop.Value);
            }

            return res;
        }


        public string GetPackageParameterDefinition(string parameterId, string packageId)
        {
            var packageFile = _files[packageId].XmlContent;
            var pars = ((XmlElement)(packageFile.GetElementsByTagName("DTS:PackageParameters")[0]));
            foreach (XmlElement p in pars.GetElementsByTagName("DTS:PackageParameter"))
            {
                var id = p.GetAttribute("DTS:DTSID");
                if (id == parameterId)
                {
                    return p.OuterXml;
                }
            }
            throw new Exception();
        }

        public string GetVariableDefinition(string variableId, XmlElement containerXmlContent)
        {
            //var packageFile = _files[package.ID].XmlContent;
            var vars = ((XmlElement)(containerXmlContent.GetElementsByTagName("DTS:Variables")[0]));
            foreach (XmlElement v in vars.GetElementsByTagName("DTS:Variable"))
            {
                var id = v.GetAttribute("DTS:DTSID");
                if (id == variableId)
                {
                    return v.OuterXml;
                }
            }

            var traceUpNode = containerXmlContent.ParentNode;
            while(traceUpNode != null)
            {
                if (traceUpNode.Name == "DTS:Executable" && traceUpNode is XmlElement)
                {
                    return GetVariableDefinition(variableId, (XmlElement)traceUpNode);
                }
                else
                {
                    traceUpNode = traceUpNode.ParentNode;
                }
            }
            throw new Exception();
        }

        public string GetPrecedenceConstraintDefinition(string constraintId, XmlElement containerXml, out string refId)
        {
            XmlElement pcs = null;
            // inner containers have constraints too and constraints appear after inner executables
            foreach (var node in containerXml.ChildNodes)
            {
                if (!(node is XmlElement))
                {
                    continue;
                }
                var elem = node as XmlElement;
                if (elem.Name == "DTS:PrecedenceConstraints")
                {
                    pcs = elem;
                }
            }

            foreach (XmlElement pc in pcs.GetElementsByTagName("DTS:PrecedenceConstraint"))
            {
                var id = pc.GetAttribute("DTS:DTSID");
                if (id == constraintId)
                {
                    refId = pc.GetAttribute("DTS:refId");
                    return pc.OuterXml;
                }
            }
            throw new Exception();
        }

        public DesignArrow GetPrecedenceConstraintArrow(string packageId, string layoutRefId)
        {
            var packageFile = _files[packageId];
            if (packageFile.EdgeLayouts.ContainsKey(layoutRefId))
            {
                return packageFile.EdgeLayouts[layoutRefId];
            }
            return new DesignArrow() { TopLeft = new DesignPoint() { X = 0, Y = 0 }, Shifts = new List<DesignPoint>() };
        }

        public DesignArrow GetDfPathArrow(string packageId, string layoutRefId)
        {
            return GetPrecedenceConstraintArrow(packageId, layoutRefId);
        }

        public string GetDfPathDefinition(XmlElement flowXml, string startIdEnding, string endIdEnding, out string fullLayoutRefId)
        {
            XmlElement paths = null;
            foreach (var childElem in flowXml.ChildNodes)
            {
                if (!(childElem is XmlElement))
                {
                    continue;
                }
                var childTyped = childElem as XmlElement;
                if (childTyped.Name != "paths")
                {
                    continue;
                }
                paths = childTyped;
            }
            string res = null;
            string fullLayoutRes = null;
            foreach (XmlElement path in paths.GetElementsByTagName("path"))
            {
                var startId = path.GetAttribute("startId");
                var endId = path.GetAttribute("endId");
                if (startId.EndsWith(startIdEnding) && endId.EndsWith(endIdEnding))
                {
                    fullLayoutRes = path.GetAttribute("refId");
                    if (res != null)
                    {
                        throw new Exception();
                    }
                    res = path.OuterXml;
                }
            }
            if (res != null)
            {
                fullLayoutRefId = fullLayoutRes;
                return res;
            }
            throw new Exception();
        }

        public string GetPackageConnectionManagerDefinition(string managerId, string packageId, out XmlElement xmlElement)
        {
            var packageFile = _files[packageId].XmlContent;
            var cons = ((XmlElement)(packageFile.GetElementsByTagName("DTS:ConnectionManagers")[0]));
            string res = null;
            foreach (XmlElement c in cons.GetElementsByTagName("DTS:ConnectionManager"))
            {
                var id = c.GetAttribute("DTS:DTSID");
                if (id == managerId)
                {
                    /*
                    if (res != null)
                    {
                        throw new Exception();
                    }
                    res = c.OuterXml;
                    */
                    xmlElement = c;
                    return c.OuterXml;
                }
            }
            /*
            if (res != null)
            {
                return res;
            }
            */
            throw new Exception();
        }

        public string GetProjectConnectionManagerDefinition(string connManagerId, out XmlElement xmlElement)
        {
            var text = _files[connManagerId].Content;
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(text);
            xmlElement = (XmlElement)doc.DocumentElement;
            return text;
        }

        public string GetProjectParamDefinition(string parameterId)
        {
            var projFile = _files["@Project.params"];
            var pars = projFile.XmlContent.GetElementsByTagName("SSIS:Parameter");
            foreach (XmlElement p in pars)
            {
                var props = p.GetElementsByTagName("SSIS:Property");
                foreach (XmlElement pp in props)
                {
                    var name = pp.GetAttribute("SSIS:Name");
                    if (name == "ID")
                    {
                        if (pp.InnerText == parameterId)
                        {
                            return p.OuterXml;
                        }
                    }
                }
            }
            throw new Exception();
        }

        public string GetPackageDefinition(string packageId, out XmlElement xmlElement)
        {
            var file = _files[packageId];
            xmlElement = file.XmlContent.DocumentElement;
            return file.Content;
        }

        public string GetExecutableDefinition(string executableId, string packageId, out string refPath, out XmlElement element)
        {
            var file = _files[packageId];
            var doc = file.XmlContent;
            var execs = doc.GetElementsByTagName("DTS:Executable");
            foreach (XmlElement exec in execs)
            {
                if (exec.GetAttribute("DTS:DTSID") == executableId)
                {
                    refPath = exec.GetAttribute("DTS:refId");
                    element = exec;
                    return exec.OuterXml;
                }
            }
            refPath = null;
            throw new Exception();
        }

        public ElementLayoutDesign GetContainerDesign(string containingPackageId, string designRefPath)
        {
            var file = _files[containingPackageId];
            if (file.NodeLayoutDesigns.ContainsKey(designRefPath))
            {
                return file.NodeLayoutDesigns[designRefPath];
            }
            else
            {
                DAL.Configuration.ConfigManager.Log.Warning($"Could not find the layout info for {designRefPath}");
                return new ElementLayoutDesign() { TopLeft = new DAL.Objects.SsisDiagram.DesignPoint(), Size = new DAL.Objects.SsisDiagram.DesignPoint() };
            }
        }

        //public XmlElement GetContainerDesignXml(string containingPackageId, string designRefPath)
        //{
        //    var file = _files[containingPackageId];
        //    return file.NodeLayoutDesignXmls[designRefPath];
        //}

        public string GetDfComponentDefinition(XmlElement flowXml, string parentLayoutRefPath, string componentIdString, out XmlElement componentDefinitionXml)
        {
            var layoutRefPath = parentLayoutRefPath + "\\" + componentIdString;
            foreach (XmlElement n in flowXml.GetElementsByTagName("component"))
            {
                var refId = n.GetAttribute("refId");
                if (refId == layoutRefPath)
                {
                    componentDefinitionXml = n;
                    return n.OuterXml;
                }
            }
            throw new Exception();
        }

        public enum AccessMode { SqlCommand, SqlCommandVariable, OpenRowset, OpenRowsetVariable }

        public AccessMode GetAccessMode(XmlElement componentDefinition)
        {
            var properties = (XmlElement)(componentDefinition.GetElementsByTagName("properties")[0]);

            foreach (XmlElement n in properties.GetElementsByTagName("property"))
            {
                var name = n.GetAttribute("name");
                if (name == "AccessMode")
                {
                    var value = n.InnerText;
                    switch (value)
                    {
                        case "0":
                            return AccessMode.OpenRowset;
                        case "1": return AccessMode.OpenRowsetVariable;
                        case "2": return AccessMode.SqlCommand;
                        case "3": return AccessMode.SqlCommandVariable;
                        default: throw new Exception("Unknown access mode " + value);
                    }
                }
            }
            throw new Exception("Access Mode not found");
        }

        public class ParamAssignment
        {
            public string Definition { get; set; }
            public string ParamName { get; set; }
            public string ReferrableName { get; set; }
        }

        public List<ParamAssignment> GetExecPackageParameterAssignments(XmlElement defElement)
        {
            List<ParamAssignment> res = new List<ParamAssignment>();
            var execPackageNode = defElement.GetElementsByTagName("ExecutePackageTask")[0] as XmlElement;
            foreach (XmlElement asgElem in execPackageNode.GetElementsByTagName("ParameterAssignment"))
            {
                var paramNode = asgElem.GetElementsByTagName("ParameterName")[0] as XmlElement;
                var referrableNode = asgElem.GetElementsByTagName("BindedVariableOrParameterName")[0] as XmlElement;
                res.Add(new ParamAssignment() { Definition = asgElem.OuterXml, ParamName = paramNode.InnerText, ReferrableName = referrableNode.InnerText });
            }
            return res;
        }

        public ElementLayoutDesign GetPackageNodeLayout(string packageId)
        {
            var file = _files[packageId];
            if (file.EdgeLayouts.Count == 0)
            {
                return new ElementLayoutDesign();
            }
            var xDim = file.NodeLayoutDesigns.Values.Where(x => x.TopLeft != null && x.Size != null).Max(x => x.TopLeft.X + x.Size.X);
            var yDim = file.NodeLayoutDesigns.Values.Where(x => x.TopLeft != null && x.Size != null).Max(x => x.TopLeft.Y + x.Size.Y);
            return new ElementLayoutDesign() { Size = new DesignPoint() { X = xDim, Y = yDim }, TopLeft = new DesignPoint() { X = 0, Y = 0 } };
        }

        private ElementLayoutDesign GetNodeLayoutDesign(XmlElement designNode)
        {
            var topLeftAttr = designNode.GetAttribute("TopLeft");
            var sizeAttr = designNode.GetAttribute("Size");

            DesignPoint topLeftPoint = ParseDesignPoint(topLeftAttr);
            DesignPoint sizePoint = ParseDesignPoint(sizeAttr);
            return new ElementLayoutDesign() { TopLeft = topLeftPoint, Size = sizePoint };
        }

        private DesignPoint ParseDesignPoint(string attrVal)
        {
            CultureInfo westernCulture = new CultureInfo("en-US");
            if (attrVal.Contains("NaN"))
            {
                return null;
            }
            if (attrVal == "{x:Null}")
            {
                return new DesignPoint() { X = 0, Y = 0 };
            }
            var split = attrVal.Split(',');
            var x = float.Parse(split[0], westernCulture);
            var y = float.Parse(split[1], westernCulture);
            return new DesignPoint() { X = x, Y = y };
        }

        private DesignArrow ParseDesignArrow(XmlElement edgeLayout)
        {
            DesignArrow arw = new DesignArrow();
            var elCurve = (edgeLayout.GetElementsByTagName("EdgeLayout.Curve")[0]) as XmlElement;
            var curve = (elCurve.GetElementsByTagName("mssgle:Curve")[0]) as XmlElement;
            var startPoint = ParseDesignPoint(curve.GetAttribute("Start"));
            var endPoint = ParseDesignPoint(curve.GetAttribute("End"));
            var endConnector = ParseDesignPoint(curve.GetAttribute("EndConnector"));
            var topLeftAttr = edgeLayout.GetAttribute("TopLeft");
            DesignPoint topLeftPoint = ParseDesignPoint(topLeftAttr);

            arw.TopLeft = topLeftPoint;
            arw.TopLeft = startPoint;
            //arw.End = endConnector;
            arw.Shifts = new List<DesignPoint>();

            DesignPoint currentPoint = startPoint;
            var segmentsElem = (curve.GetElementsByTagName("mssgle:Curve.Segments")[0]) as XmlElement;
            foreach (XmlElement segmCollection in segmentsElem.GetElementsByTagName("mssgle:SegmentCollection"))
            {
                foreach (var segment in segmCollection.ChildNodes)
                {
                    if (!(segment is XmlElement))
                    {
                        continue;
                    }

                    var segmentTyped = segment as XmlElement;
                    if (segmentTyped.Name == "mssgle:LineSegment")
                    {
                        var newPoint = ParseDesignPoint(segmentTyped.GetAttribute("End"));
                        arw.Shifts.Add(new DesignPoint() { X = newPoint.X - currentPoint.X, Y = newPoint.Y - currentPoint.Y });
                        currentPoint = newPoint;
                    }
                    else if (segmentTyped.Name == "mssgle:CubicBezierSegment")
                    {
                        //TODO: bezier segments?
                        var newPoint = ParseDesignPoint(segmentTyped.GetAttribute("Point2"));
                        arw.Shifts.Add(new DesignPoint() { X = newPoint.X - currentPoint.X, Y = newPoint.Y - newPoint.Y });
                        currentPoint = newPoint;
                        newPoint = ParseDesignPoint(segmentTyped.GetAttribute("Point3"));
                        arw.Shifts.Add(new DesignPoint() { X = newPoint.X - currentPoint.X, Y = newPoint.Y - newPoint.Y });
                        currentPoint = newPoint;
                    }
                }
            }
            arw.Shifts.Add(new DesignPoint() { X = endConnector.X - currentPoint.X, Y = endConnector.Y - currentPoint.Y });

            return arw;
        }

        public string GetDfInnerDefinition(XmlElement outerTaskElem, out XmlElement innerXml)
        {
            innerXml = (XmlElement)(outerTaskElem.GetElementsByTagName("pipeline")[0]);
            return innerXml.OuterXml;
        }
        
        internal string GetDfComponentOutputDefinition(XmlElement componentDefinitionXml, string idString, out XmlElement definitionXml)
        {
            string componentPath = componentDefinitionXml.GetAttribute("refId");
            var basePath = componentPath.Substring(0, componentPath.LastIndexOf("\\") + 1);
            var outputPath = basePath + idString;
            foreach (XmlElement output in componentDefinitionXml.GetElementsByTagName("output"))
            {
                var itemPath = output.GetAttribute("refId");
                if (itemPath == outputPath)
                {
                    definitionXml = output;
                    return output.OuterXml;
                }
            }
            throw new Exception();
        }

        internal string GetDfOutputColumnDefinition(XmlElement outputDefinitionXml, string idString)
        {
            string outputPath = outputDefinitionXml.GetAttribute("refId");
            var basePath = outputPath.Substring(0, outputPath.LastIndexOf("\\") + 1);
            var columnPath = basePath + idString;
            foreach (XmlElement outputColumn in outputDefinitionXml.GetElementsByTagName("outputColumn"))
            {
                var itemPath = outputColumn.GetAttribute("refId");
                if (itemPath == columnPath)
                {
                    return outputColumn.OuterXml;
                }
            }
            throw new Exception();
        }

        internal string GetDfComponentInputDefinition(XmlElement componentDefinitionXml, string identificationString, out XmlElement inputDefinitionXml)
        {
            string componentPath = componentDefinitionXml.GetAttribute("refId");
            var basePath = componentPath.Substring(0, componentPath.LastIndexOf("\\") + 1);
            var inputPath = basePath + identificationString;
            foreach (XmlElement input in componentDefinitionXml.GetElementsByTagName("input"))
            {
                var itemPath = input.GetAttribute("refId");
                if (itemPath == inputPath)
                {
                    inputDefinitionXml = input;
                    return input.OuterXml;
                }
            }
            throw new Exception();
        }

        internal string GetDfInputColumnDefinition(XmlElement inputDefinitionXml, string identificationString)
        {
            string inputPath = inputDefinitionXml.GetAttribute("refId");
            var basePath = inputPath.Substring(0, inputPath.LastIndexOf("\\") + 1);
            var columnPath = basePath + identificationString;
            foreach (XmlElement inputColumn in inputDefinitionXml.GetElementsByTagName("inputColumn"))
            {
                var itemPath = inputColumn.GetAttribute("refId");
                if (itemPath == columnPath)
                {
                    return inputColumn.OuterXml;
                }
            }
            throw new Exception();
        }
    }
}

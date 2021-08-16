using CD.DLS.API;
using CD.DLS.API.ModelUpdate;
using CD.DLS.Common.Structures;
using CD.DLS.DAL.Objects.Extract;
using CD.DLS.Model.Interfaces;
using CD.DLS.Model.Mssql;
using CD.DLS.Model.Serialization;
using CD.DLS.Parse.Mssql.Ssas;
using System.Collections.Generic;
using System.Linq;
using CD.DLS.Model.Mssql.Ssis;
using CD.DLS.Model;

namespace CD.DLS.RequestProcessor.ModelUpdate
{
    public class ParseSsisProjectsRequestProcessor : RequestProcessorBase, IRequestProcessor<ParseSsisProjectsRequest, DLSApiProgressResponse>
    {
        public DLSApiProgressResponse Process(ParseSsisProjectsRequest request, ProjectConfig projectConfig)
        {
            List<DLSApiMessage> parseProjectRequests = new List<DLSApiMessage>();

            SerializationHelper serializationHelper = new SerializationHelper(projectConfig, GraphManager);
            var solutionElement = (SolutionModelElement)serializationHelper.LoadElementModelToChildrenOfType("", typeof(SolutionModelElement));
            var premappedIds = serializationHelper.CreatePremappedModel(solutionElement);

            var groupByServer = projectConfig.SsisComponents.GroupBy(x => x.ServerName);
            foreach (var serverGrp in groupByServer)
            {
                var serverName = serverGrp.Key;
                var rp = new RefPath(string.Format("IntegrationServices[@Name='{0}']", serverName));
                var serverElement = new ServerElement(rp, serverName, rp.Path);
                //res.Add(serverElement);

                serverElement.Parent = solutionElement;
                solutionElement.AddChild(serverElement);

                var catalogRp = rp.Child("Catalog");

                var catalogElement = new CatalogElement(catalogRp, "SSIS Catalog", catalogRp.Path, serverElement);
                serverElement.AddChild(catalogElement);

                var grpByFolder = serverGrp.GroupBy(x => x.FolderName);
                foreach (var folderGrp in grpByFolder)
                {
                    var folderName = folderGrp.Key;
                    //var folder = catalog.Folders[folderName];
                    var folderRp = catalogRp.NamedChild("Folder", folderName);
                    var folderElement = new FolderElement(folderRp, folderName, folderRp.Path, catalogElement);
                    catalogElement.AddChild(folderElement);
                    
                    foreach (var projectComponent in folderGrp)
                    {
                        parseProjectRequests.Add(new ParseSsisProjectShallowRequest()
                        {
                            ExtractId = request.ExtractId,
                            SsisComponentId = projectComponent.SsisProjectComponentId,
                            FolderRefPath = folderElement.RefPath.Path,
                            ServerRefPath = serverElement.RefPath.Path
                        });
                    }
                }

                serializationHelper.SaveModelPart(serverElement, premappedIds);
            }

            GraphManager.SetRefPathIntervals(projectConfig.ProjectConfigId, RequestId);

            return new DLSApiProgressResponse()
            {
                //ContinuationsWaitForDb = true,
                ParallelRequests = parseProjectRequests,
                ContinueWith = new ParseSsrsComponentsRequest()
                {
                    ExtractId = request.ExtractId
                }
            };
            
        }
    }
}

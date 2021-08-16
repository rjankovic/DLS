using CD.DLS.API;
using CD.DLS.API.ModelUpdate;
using CD.DLS.Common.Structures;
using CD.DLS.DAL.Objects.Extract;
using CD.DLS.Model.Serialization;
using System.Collections.Generic;
using CD.DLS.Model.Mssql.Ssis;
using CD.DLS.Parse.Mssql.Ssis;
using CD.DLS.DAL.Configuration;

namespace CD.DLS.RequestProcessor.ModelUpdate
{
    public class ParseSsisProjectShallowRequestProcessor : RequestProcessorBase, IRequestProcessor<ParseSsisProjectShallowRequest, DLSApiProgressResponse>
    {
        public DLSApiProgressResponse Process(ParseSsisProjectShallowRequest request, ProjectConfig projectConfig)
        {
            //List<DLSApiMessage> parsePackageRequests = new List<DLSApiMessage>();

            var urnBuilder = new UrnBuilder();
            SerializationHelper serializationHelper = new SerializationHelper(projectConfig, GraphManager);
            var folderElement = (FolderElement)serializationHelper.LoadElementModel(request.FolderRefPath);
            var premappedIds = serializationHelper.CreatePremappedModel(folderElement);

            var xmlProvider = new SsisXmlProvider(request.ExtractId, request.SsisComponentId, 
                StageManager, null, false);
            var dbIndex = new Parse.Mssql.Db.AvailableDatabaseModelIndex(projectConfig, GraphManager);
            ProjectModelParser modelParser = new ProjectModelParser(xmlProvider, dbIndex, projectConfig,
                request.ExtractId, StageManager);

            var projectModel = modelParser.ParseProjectShallow(request.SsisComponentId, folderElement);

            
            var packages = StageManager.GetExtractItems(request.ExtractId, request.SsisComponentId, ExtractTypeEnum.SsisPackage);

            foreach (SsisPackage package in packages)
            {
                //parsePackageRequests.Add(new ParseSsisPackageShallowRequest()
                //{
                //    ExtractId = request.ExtractId,
                //    ProjectRefPath = projectModel.RefPath.Path,
                //    PackageExractItemId = package.ExtractItemId
                //});

                //var projectElement = (ProjectElement)serializationHelper.LoadElementModel(request.ProjectRefPath);
                //var premappedIds = serializationHelper.CreatePremappedModel(projectElement);

                //var package = (SsisPackage)StageManager.GetExtractItem(request.PackageExractItemId);

                ConfigManager.Log.Important("Parsing " + package.Urn + " shallow");

                var refPath = urnBuilder.GetPackageUrn(projectModel, package.Name);// packageInfo.Urn;
                PackageElement packageElement = new PackageElement(refPath, package.Name, null /* definition*/, projectModel);
                projectModel.AddChild(packageElement);

                //serializationHelper.SaveModelPart(packageElement, premappedIds);
            }

            serializationHelper.SaveModelPart(projectModel, premappedIds);

            GraphManager.SetRefPathIntervals(projectConfig.ProjectConfigId, RequestId);

            return new DLSApiProgressResponse()
            {
                //ContinuationsWaitForDb = true,
                //ParallelRequests = parsePackageRequests,
                ContinueWith = new ParseSsisProjectDeepRequest()
                {
                    ExtractId = request.ExtractId,
                    SsisComponentId = request.SsisComponentId,
                    ProjectRefPath = projectModel.RefPath.Path,
                    ServerRefPath = request.ServerRefPath,
                    FolderRefPath = request.FolderRefPath
                }
            };
            
        }
    }
}

using CD.DLS.API;
using CD.DLS.API.ModelUpdate;
using CD.DLS.Common.Structures;
using CD.DLS.DAL.Objects.Extract;
using CD.DLS.Model.Serialization;
using CD.DLS.Model.Mssql.Ssis;
using CD.DLS.Parse.Mssql.Ssis;
using CD.BIDoc.Core.Parse.Mssql.Ssis;

namespace CD.DLS.RequestProcessor.ModelUpdate
{
    //public class ParseSsisPackageShallowRequestProcessor : RequestProcessorBase, IRequestProcessor<ParseSsisPackageShallowRequest, DLSApiMessage>
    //{
    //    public DLSApiMessage Process(ParseSsisPackageShallowRequest request, ProjectConfig projectConfig)
    //    {
    //        SerializationHelper serializationHelper = new SerializationHelper(projectConfig, GraphManager);
    //        var projectElement = (ProjectElement)serializationHelper.LoadElementModel(request.ProjectRefPath);
    //        var premappedIds = serializationHelper.CreatePremappedModel(projectElement);

    //        var package = (SsisPackage)StageManager.GetExtractItem(request.PackageExractItemId);

    //        var urnBuilder = new UrnBuilder();
    //        var refPath = urnBuilder.GetPackageUrn(projectElement, package.Name);// packageInfo.Urn;
    //        PackageElement packageElement = new PackageElement(refPath, package.Name, null /* definition*/, projectElement);
    //        projectElement.AddChild(packageElement);

    //        serializationHelper.SaveModelPart(packageElement, premappedIds);
            
    //        return new DLSApiMessage();
    //    }
    //}
}

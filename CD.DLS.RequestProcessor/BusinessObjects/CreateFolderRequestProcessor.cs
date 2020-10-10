using CD.DLS.API;
using CD.DLS.API.BusinessObjects;
using CD.DLS.Common.Structures;
using CD.DLS.DAL.Configuration;
using CD.DLS.Model;
using CD.DLS.Model.Business;
using CD.DLS.Model.Business.Organization;
using CD.DLS.Model.Interfaces;
using CD.DLS.Model.Mssql;
using CD.DLS.Model.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CD.DLS.RequestProcessor.BusinessObjects
{
    class CreateFolderRequestProcessor : BusinessRequestProcessorBase, IRequestProcessor<CreateFolderRequest, CreateFolderRequestResponse>
    {
        public CreateFolderRequestResponse Process(CreateFolderRequest request, ProjectConfig projectConfig)
        {
            try
            {
                SerializationHelper serializationHelper = new SerializationHelper(projectConfig, GraphManager);

                var parentFolder = (BusinessFolderElement)(serializationHelper.LoadElementModel(
                    request.ParentFolderRefPath));
                var premappedIds = serializationHelper.CreatePremappedModel(parentFolder);

                RefPath folderRefPath = parentFolder.RefPath.NamedChild("Folder", request.FolderName);
                BusinessFolderElement folderElement = new BusinessFolderElement(folderRefPath, request.FolderName, null, parentFolder);
                parentFolder.AddChild(folderElement);

                serializationHelper.SaveModelPart(folderElement, premappedIds);

                return new CreateFolderRequestResponse() { FolderRefPath = folderRefPath.Path };
            }
            catch (Exception ex)
            {
                LogException(ex);
                throw;
            }
        }
    }
}

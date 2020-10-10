using CD.DLS.API;
using CD.DLS.API.BusinessObjects;
using CD.DLS.API.Structures;
using CD.DLS.Common.Structures;
using CD.DLS.DAL.Identity;
using CD.DLS.DAL.Managers;
using CD.DLS.DAL.Objects.Inspect;
using CD.DLS.DAL.Receiver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CD.DLS.Clients.Controls.DataManagers
{
    class BusinessObjectsTreeManager
    {
        private IReceiver _receiver;
        private Guid _serviceReceiverId;
        private ProjectConfig _config;
        private ServiceHelper _serviceHelper;
        private GraphManager _graphManager;
        private InspectManager _inspectManager;
        
        public BusinessObjectsTreeManager(IReceiver receiver, Guid serviceReceiverId, ProjectConfig config)
        {
            _receiver = receiver;
            _serviceReceiverId = serviceReceiverId;
            _config = config;
            _serviceHelper = new ServiceHelper(receiver, serviceReceiverId, config);
            _graphManager = new GraphManager();
            _inspectManager = new InspectManager();
        }

        public List<ElementTreeListItem> ListBusinessTree()
        {
            var res = _inspectManager.GetBusinessTree(_config.ProjectConfigId);

            
            var identity = IdentityProvider.GetCurrentUser().Identity;

            // the user's business folder does not exist yet - create it now
            if (!res.Any(x => x.Caption == identity))
            {
                //CreateFolder(identity, "Business");
                _serviceHelper.PostRequest<CreateFolderRequestResponse>(new CreateFolderRequest() { FolderName = identity, ParentFolderRefPath = "Business" }).Wait();
                res = _inspectManager.GetBusinessTree(_config.ProjectConfigId);
            }

            var identityFolder = res.First(x => x.Caption == identity);
            identityFolder.Alias = "My Artefacts";

            var resFiltered = res.Where(x => x.RefPath.StartsWith("Business/SharedFolder") || x.RefPath.Contains("Name='" + identity + "'")).ToList();

            return resFiltered;
        }

        public void RenameElement(int elementId, string newName)
        {
            _graphManager.RenameModelElement(elementId, newName);
        }

        public async Task<ElementTreeListItem> CreateFolder(string folderName, ElementTreeListItem parentFolder)
        {
            var resp = await _serviceHelper.PostRequest<CreateFolderRequestResponse>(new CreateFolderRequest() { FolderName = folderName, ParentFolderRefPath = parentFolder.RefPath });

            var elementId = _graphManager.GetModelElementIdByRefPath(_config.ProjectConfigId, resp.FolderRefPath);
            var elementModel = _graphManager.GetModelElementById(elementId);

            return new ElementTreeListItem()
            {
                Alias = folderName,
                Caption = folderName,
                MaxParentLevel = parentFolder.MaxParentLevel + 1,
                ModelElementId = elementModel.Id,
                ParentElementId = parentFolder.ModelElementId,
                RefPath = elementModel.RefPath,
                Type = parentFolder.Type,
                TypeDescription = parentFolder.TypeDescription
            };
        }

        public async Task DeleteElement(ElementTreeListItem obj)
        {
            var t = Task.Factory.StartNew(() => { _graphManager.ClearModelPartWithAggregations(_config.ProjectConfigId, obj.RefPath); });
            await t;
            return;
        }

        public async Task<PivotTableStructure> GetPivotTableTemplate(string refPath)
        {
            var request = new GetPivotTableTemplateRequest() { PivotTableTemplateRefPath = refPath };
            var resp = await _serviceHelper.PostRequest(request);
            return resp.Structure;
        }

        public async Task<ElementTreeListItem> SavePivotTableTemplate(PivotTableStructure structure, ElementTreeListItem parentFolder, string templateName)
        {
            var request = new SavePivotTableTemplateRequest() { FolderRefPath = parentFolder.RefPath, PivotTableName = templateName, Structure = structure };
            var resp = await _serviceHelper.PostRequest(request);

            var elementId = _graphManager.GetModelElementIdByRefPath(_config.ProjectConfigId, resp.PivotTableTemplateRefPath);
            var elementModel = _graphManager.GetModelElementById(elementId);

            return new ElementTreeListItem()
            {
                Alias = templateName,
                Caption = templateName,
                MaxParentLevel = parentFolder.MaxParentLevel + 1,
                ModelElementId = elementModel.Id,
                ParentElementId = parentFolder.ModelElementId,
                RefPath = elementModel.RefPath,
                Type = elementModel.Type,
                TypeDescription = "Pivot Table Template"
            };
        }
    }
}

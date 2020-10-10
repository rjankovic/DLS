using CD.DLS.API;
using CD.DLS.API.BusinessObjects;
using CD.DLS.Common.Structures;
using CD.DLS.Model;
using CD.DLS.Model.Business;
using CD.DLS.Model.Business.Excel;
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
    class SavePivotTableTemplateRequestProcessor : BusinessRequestProcessorBase, IRequestProcessor<SavePivotTableTemplateRequest, SavePivotTableTemplateRequestResponse>
    {
        public SavePivotTableTemplateRequestResponse Process(SavePivotTableTemplateRequest request, ProjectConfig projectConfig)
        {
            try
            {
                SerializationHelper serializationHelper = new SerializationHelper(projectConfig, GraphManager);
                var folderElement = (BusinessFolderElement)(serializationHelper.LoadElementModel(request.FolderRefPath));
                var premappedIds = serializationHelper.CreatePremappedModel(folderElement);
                var pivotTableElement = PivotTableStructureToModel(request.Structure, folderElement, request.PivotTableName);
                serializationHelper.SaveModelPart(pivotTableElement, premappedIds);
                return new SavePivotTableTemplateRequestResponse() { PivotTableTemplateRefPath = pivotTableElement.RefPath.Path };
            }
            catch (Exception ex)
            {
                LogException(ex);
                throw;
            }
        }
    }
}

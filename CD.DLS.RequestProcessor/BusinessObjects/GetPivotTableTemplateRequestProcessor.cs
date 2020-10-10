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
    class GetPivotTableTemplateRequestProcessor : BusinessRequestProcessorBase, IRequestProcessor<GetPivotTableTemplateRequest, GetPivotTableTemplateRequestResponse>
    {
        public GetPivotTableTemplateRequestResponse Process(GetPivotTableTemplateRequest request, ProjectConfig projectConfig)
        {
            try
            {
                SerializationHelper serializationHelper = new SerializationHelper(projectConfig, GraphManager);
                var pivotTableElement = (PivotTableTemplateElement)(serializationHelper.LoadElementModel(request.PivotTableTemplateRefPath));
                var convertedStructure = pivotTableElement.PivotTableStructure;
                return new GetPivotTableTemplateRequestResponse() { Structure = convertedStructure };
            }
            catch (Exception ex)
            {
                LogException(ex);
                throw;
            }

        }
    }
}

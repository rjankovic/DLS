using CD.DLS.API;
using CD.DLS.API.Render;
using CD.DLS.Common.Structures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CD.DLS.RequestProcessor.Render
{
    class RenderReportRequestProcessor : RequestProcessorBase, IRequestProcessor<RenderReportRequest, RenderReportResponse>
    {
        /*
    exports
    http://localhost/reportserver/Pages/ReportViewer.aspx?%2fReports%2f07+-+Perm+Dashboard&rs:Format=csv

    http://localhost/reportserver/Pages/ReportViewer.aspx?%2fReports%2f07+-+Perm+Dashboard&rs:Format=excel

    http://localhost/ReportServer?%2fReports%2f08+-+Bonuses&DateYearMonth=%5BDate%5D.%5BYear%20-%20Month%5D.%5BMonth%5D.%26%5B2017%5D%26%5B3%5D&BranchBranchLevel1Name=%5BEmployee%20Internal%5D.%5BEmployee%20Home%20Branch%20Level1%20Name%5D.%26%5BBrno%20Staffing%5D&OrderConsultantEmployeeRegionName=%5BOrder%20Consultant%5D.%5BEmployee%20Region%20Name%5D.%5BAll%5D&OrderConsultantEmployeeCOPositionName=%5BOrder%20Consultant%5D.%5BEmployee%20CO%20Position%20Detail%20Name%5D.%5BAll%5D&rs:Format=csv

    ssrs service
    http://localhost/ReportServer/reportservice2010.asmx

    // nested folders
    %2fReports%2fVetrotech%2fPortal+page
 */


        public RenderReportResponse Process(RenderReportRequest request, ProjectConfig projectConfig)
        {
            throw new NotImplementedException();
        }
    }
}

using CD.DLS.Serialization;
using CD.DLS.API;
using CD.DLS.Common.Structures;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CD.DLS.Parse.Mssql.Ssas;
using System.Data.SqlClient;
using CD.DLS.Parse.Mssql;
using CD.DLS.DAL.Managers;

namespace CD.DLS.Operations
{
    internal class ListSsrsReportsRequestProcessor : BIDocRequestProcessor<ListSsrsReportsRequest>
    {
        public ListSsrsReportsRequestProcessor(BIDocCore core) : base(core)
        {
        }

        public override ProcessingResult ProcessRequest(ListSsrsReportsRequest request, ProjectConfig projectConfig)
        {
            var attachments = new List<Attachment>();
            var reportElements = GraphManager.GetModelElementsOfType(projectConfig.ProjectConfigId, typeof(Model.Mssql.Ssrs.ReportElement))
                .Select(x => new Model.Mssql.Ssrs.ReportElement(new Interfaces.RefPath(x.RefPath), x.Caption, x.Definition, null)).ToList();
            var reportTree = MssqlModelExtractor.ListReports(new Interfaces.ModelSettings() { Config = projectConfig, Log = _core.Log } /* new ExtractSettingsProvider(projectConfig, _core.Log)*/, reportElements);

            if (reportTree == null)
            {
                return null;
            }
            ListSsrsReportsRequestResponse result = new ListSsrsReportsRequestResponse() { RootFolder = (ListSsrsReportsRequestResponse .SsrsFolder)reportTree };
            var stringResult = result.Serialize();
        
            return new ProcessingResult()
            {
                Content = stringResult,
                Attachments = attachments
            };
        }
    }
}

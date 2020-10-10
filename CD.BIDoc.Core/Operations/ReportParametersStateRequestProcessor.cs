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
using System.Net;
using System.Web.Services.Protocols;
using CD.DLS.Parse.Mssql;
using CD.DLS.Model.Mssql.Ssrs;
using CD.DLS.Interfaces;
using CD.DLS.Model.Mssql;
using System.Xml;
using Microsoft.SqlServer.ReportExecution2005;

namespace CD.DLS.Operations
{
    internal class ReportParametersStateRequestProcessor : BIDocRequestProcessor<ReportParametersStateRequest>
    {
        
        public ReportParametersStateRequestProcessor(BIDocCore core) : base(core)
        {
        }

        public override ProcessingResult ProcessRequest(ReportParametersStateRequest request, ProjectConfig projectConfig)
        {
            var attachments = new List<Attachment>();
            var es = new ReportExecutionService();
            es.Credentials = CredentialCache.DefaultCredentials;
            SsrsProjectComponent ssrsComponent = null;
            if (projectConfig.SsrsComponents.Count == 1)
            {
                ssrsComponent = projectConfig.SsrsComponents.First();
            }
            else
            {
                ssrsComponent = projectConfig.SsrsComponents.First(x => x.SsrsProjectComponentId == request.SsrsComponentId);
            }
            // Set the base Web service URL of the source server  
            es.Url = ssrsComponent.SsrsExecutionServiceUrl; // projectConfig.SsrsExecutionServiceUrl;

            // Render arguments
            string reportPath = request.ReportPath;




            //string reportElementUrn = MssqlModelExtractor.GetReportElementUrnPath(request.ReportPath, new ExtractSettingsProvider(projectConfig, _core.Log));
            //ReportElement reportElement = null;

            //_core.Log.Important("Converting to DB format");
            //using (var dbContext = new CDFrameworkContext())
            //{
            //    BIDocModelStored modelStored = new BIDocModelStored(dbContext, projectConfig.ProjectConfigId, reportElementUrn);
            //    int reportElementId = dbContext.ModelElements.First(x => x.RefPath == reportElementUrn).Id;

            //    FromBIDocModelConverter converterFrom = new FromBIDocModelConverter(modelStored);
            //    IReflectionHelper reflection = new JsonReflectionHelper(new Model.Mssql.ModelActivator());
            //    Model.Mssql.ModelConverter converterTo = new Model.Mssql.ModelConverter(reflection);
            //    MssqlModelElement convertedModel = converterFrom.Convert(converterTo, reportElementId);

            //    reportElement = (ReportElement)(convertedModel);
            //}


            string historyID = null;
            
            // Prepare report parameter.
            ParameterValue[] parameters = new ParameterValue[request.ParameterValues.Count];
            for (int i = 0; i < request.ParameterValues.Count; i++)
            {
                var val = request.ParameterValues[i];
                parameters[i] = new ParameterValue() { Name = val.ParameterName, Value = val.ParameterValue };
            }
            
            ExecutionInfo execInfo = new ExecutionInfo();
            ExecutionHeader execHeader = new ExecutionHeader();

            es.ExecutionHeaderValue = execHeader;

            execInfo = es.LoadReport(reportPath, historyID);
            
            es.SetExecutionParameters(parameters, "en-us");

            var execInfoUpdated = es.GetExecutionInfo();

            //TODO fix
            /*
            ReportParametersState paramsState = new ReportParametersState();
            paramsState.Parameters = new List<ReportParameter>();
            foreach (var param in execInfoUpdated.Parameters)
            {
                //var modelParam = reportElement.Parameters.First(x => x.Caption == param.Name);
                //if(modelParam.)
                paramsState.Parameters.Add(param);
            }

            Uri attachmentUri = null;
            var stateSerialized = paramsState.Serialize();
            using (var ms = Tools.Tools.GenerateStreamFromString(stateSerialized))
            {
                    attachmentUri = _core.StorageProvider.Save(ms);
            }
            
            attachments.Add(new Attachment() { Uri = attachmentUri, Type = AttachmentTypeEnum.JSON, AttachmentId = Guid.NewGuid() });
            */
            var requestResult = new ReportParametersStateRequestResponse();
            var stringResult = requestResult.Serialize();
            
            return new ProcessingResult()
            {
                Content = stringResult,
                Attachments = attachments
            };
            
        }
        
    }
}

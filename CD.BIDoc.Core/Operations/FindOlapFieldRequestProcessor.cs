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
using CD.DLS.DAL.Managers;
using CD.DLS.DAL.Objects.BIDoc;

namespace CD.DLS.Operations
{
    internal class FindOlapFieldRequestProcessor : BIDocRequestProcessor<FindOlapFieldRequest>
    {
        public FindOlapFieldRequestProcessor(BIDocCore core) : base(core)
        {
        }

        public override ProcessingResult ProcessRequest(FindOlapFieldRequest request, ProjectConfig projectConfig)
        {
            var attachments = new List<Attachment>();

            string foundPath = null;
            int foundModelElementId = 0;
            var cubePath = UrnBuilder.GetCubeRefPath(request.ConnectionString, request.CubeName);
            /*
            InspectManager.FindOlapField(projectConfig.ProjectConfigId, cubePath, request.FieldName, 
                out foundPath, out foundModelElementId);
            */
            int dfNodeId = -1;
            if (foundPath != null)
            {
                dfNodeId = GraphManager.GetGraphNodeId(projectConfig.ProjectConfigId, foundPath, DependencyGraphKind.DataFlow);
            }

            FindOlapFieldRequestResponse resp = new FindOlapFieldRequestResponse()
            {
                RefPath = foundPath,
                ModelElementId = foundModelElementId,
                DataFlowNodeId = dfNodeId
            };

            string stringResult = null;
            stringResult = resp.Serialize();



            //var cubePath = UrnBuilder.GetCubeRefPath(request.ConnectionString, request.CubeName);
            //FindOlapFieldRequestResponse result;
            //using (var db = new CDFrameworkContext())
            //{
            //    var query = db.Database.SqlQuery<FindOlapFieldRequestResponse>("Inspect.sp_FindOlapField @cubePath, @fieldName", 
            //        new SqlParameter("@cubePath", cubePath), new SqlParameter("@fieldName", request.FieldName));
            //    result = query.FirstOrDefault();
            //}
            
            return new ProcessingResult()
            {
                Content = stringResult,
                Attachments = attachments
            };
        }
    }
}

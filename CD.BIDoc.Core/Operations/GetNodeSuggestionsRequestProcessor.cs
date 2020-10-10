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
using CD.DLS.Tools;
using CD.DLS.DAL.Managers;
using CD.DLS.DAL.Objects.BIDoc;
//using CD.Framework.EF.Entities;

namespace CD.DLS.Operations
{
    internal class GetNodeSuggestionsRequestProcessor : BIDocRequestProcessor<GetNodeSuggestionsRequest>
    {
        public GetNodeSuggestionsRequestProcessor(BIDocCore core) : base(core)
        {
        }

        public override ProcessingResult ProcessRequest(GetNodeSuggestionsRequest request, ProjectConfig projectConfig)
        {
            List<BIDocGraphInfoNodeExtended> suggesions = new List<BIDocGraphInfoNodeExtended>();
            if (request.ExtendedSearch)
            {
                suggesions = InspectManager.GetExtendedGraphExplorerSuggestions(projectConfig.ProjectConfigId, DependencyGraphKind.DataFlowTransitive);
            }
            else
            {
                suggesions = InspectManager.GetGraphExplorerSuggestions(projectConfig.ProjectConfigId, DependencyGraphKind.DataFlowTransitive);
            }

            GetNodeSuggestionsRequestResponse result = new GetNodeSuggestionsRequestResponse();
            result.Suggestions = suggesions.Select(x => NodeDeclarationConverter.ToNodeDeclaration(x)).ToList();
            
            var stringResult = result.Serialize();

            return new ProcessingResult()
            {
                Content = stringResult,
                Attachments = new List<Attachment>()
            };
        }
        
    }
}

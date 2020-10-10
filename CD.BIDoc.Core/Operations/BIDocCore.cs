using CD.DLS.Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CD.DLS.Common.Structures;
using System.Threading;
using CD.DLS.API;
using CD.DLS.Parse.Mssql.Ssis;

namespace CD.DLS.Operations
{
    public class ProcessingResult
    {
        public string Content { get; set; }
        public List<Attachment> Attachments { get; set; }
    }

    /// <summary>
    /// Entry point from the framework to the core.
    /// </summary>
    public class BIDocCore : CoreBase
    {
        private readonly List<BIDocRequestProcessor> _requestProcessors;


        public BIDocCore()
        {
            _requestProcessors = new List<BIDocRequestProcessor>
            {
                new ExtractMetadataRequestProcessor(this),
                new CreateGraphRequestProcessor(this),
                new GetBasicDGRequestProcessor(this),
                new GetMssqlDocumentsRequestProcessor(this),
                new GetSsisDocumentsRequestProcessor(this),
                new NullRequestProcessor(this),
                new FindOlapFieldRequestProcessor(this),
                new GetElementLineageInfoRequestProcessor(this),
                new SaveBusinessDictionaryEntryRequestProcessor(this),
                new ListSsrsReportsRequestProcessor(this),
                new RenderReportRequestProcessor(this),
                new GetReportItemPositionsRequestProcessor(this),
                new GetNodeRefPathByIdRequestProcessor(this),
                new ReportParametersStateRequestProcessor(this),
                new ListReportElementsRequestProcessor(this),
                new GetNodeDescendantsRequestProcessor(this),
                new GetNodeSuggestionsRequestProcessor(this),
                new GetDataFlowBetweenGroupsRequestProcessor(this),
                new GetModelElementIdByRefPathRequestProcessor(this),
                new LineageDetailRequestProcessor(this),
                new ElementTechViewRequestProcessor(this)
            };
        }

        public override CoreTypeEnum CoreType
        {
            get
            {
                return CoreTypeEnum.BIDoc;
            }
        }

        public override Task<RequestMessage> ProcessMessage(RequestMessage input)
        {
            var requestDeser = DLSApiMessage.Deserialize(input.Content);

            return Task.Factory.StartNew(() =>
            {

                var resp = ProcessRequest(requestDeser);

                if(resp.Attachments == null)
                {
                    resp.Attachments = new List<Attachment>();
                }

                var respMsg = new RequestMessage()
                {
                    MessageFromId = Id,
                    MessageFromName = Name,
                    MessageId = Guid.NewGuid(),
                    MessageOriginId = Id,
                    MessageOriginName = Name,
                    MessageToProjectId = input.MessageToProjectId, // Guid.Empty,
                    MessageToObjectId = input.MessageFromId,
                    MessageToObjectName = input.MessageFromName,
                    MessageType = MessageTypeEnum.RequestProcessed,
                    CreatedDateTime = DateTimeOffset.Now,
                    RequestForCoreType = input.RequestForCoreType,
                    RequestId = input.RequestId,
                    Content = resp.Content,
                    Attachments = resp.Attachments
                };

                foreach (var att in respMsg.Attachments)
                {
                    att.MessageId = respMsg.MessageId;
                }

                return respMsg;
            });
        }

        public ProcessingResult ProcessRequest(DLSApiMessage request)
        {
            //TODO: VD: catching the exception and rethrowing makes debugging harder
            //TODO: RJ: ...but the exception must be logged before the application crashes

//#if vdfalse
            try
            {
//#endif
            ProcessingResult resp = null;

                foreach(BIDocRequestProcessor processor in _requestProcessors)
                {
                    if (processor.CanProcess(request))
                    {
                        resp = processor.ProcessRequest(request, _projectConfig);
                        break;
                    }
                }

                if(resp == null)
                {
                    throw new Exception("Core cannot process request");
                }
                GC.Collect();
                return resp;
//#if vdfalse
            }
            catch (Exception ex)
            {
                _log.Error(ex.Message);
                _log.Error(ex.StackTrace);
                if (ex.InnerException != null)
                {
                    _log.Error(ex.InnerException.Message);
                }
                throw;
            }
//#endif
        }

        public override void Init(Guid id, string name, ILogger log, ProjectConfig projectConfig)
        {
            base.Init(id, name, log, projectConfig);
            //NetBridge.ConnectionInfoMessage += NetBridge_ConnectionInfoMessage;
        }

        //private void NetBridge_ConnectionInfoMessage(object sender, System.Data.SqlClient.SqlInfoMessageEventArgs e)
        //{
        //    _log.Important(e.Message);
        //}
    }
}

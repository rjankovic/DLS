using CD.DLS.API;
using CD.DLS.API.ModelUpdate;
using CD.DLS.Common.Interfaces;
using CD.DLS.Common.Structures;
using CD.DLS.DAL.Configuration;
using CD.DLS.DAL.Engine;
using CD.DLS.DAL.Managers;
using CD.DLS.DAL.Receiver;
using Microsoft.Azure.WebJobs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CD.DLS.RequestProcessor
{
    public class MessageProcessor
    {
        private IReceiver _receiver;
        private ILogger _log;
        private ICollector<string> _topicCollector;

        public ICollector<string> TopicCollector { get { return _topicCollector; } set { _topicCollector = value; } }

        //TODO: in Azure, set the ConfigManager's ServedCustomerCode beforehands
        public MessageProcessor(string customerCode)
        {
            ConfigManager.Log.Info("Starting processor for customer " + customerCode);
            _receiver = new HttpReceiver(customerCode);
            _log = ConfigManager.Log;

        }
        
        public MessageProcessor(Receiver receiver)
        {
            _receiver = receiver;
            _log = ConfigManager.Log;

        }

        private Dictionary<string, RequestManager> _requestManagers = new Dictionary<string, RequestManager>();

        private RequestManager GetRequestManager(RequestMessage message)
        {
            if (ConfigManager.DeploymentMode == DeploymentModeEnum.Azure)
            {
                if (!_requestManagers.ContainsKey(message.CustomerCode))
                {
                    var nb = new NetBridge(true);
                    nb.SetConnectionString(ConfigManager.GetCustomerDatabaseConnectionString(message.CustomerCode));
                    var rm = new RequestManager(nb);
                    _requestManagers[message.CustomerCode] = rm;
                }
                return _requestManagers[message.CustomerCode];
            }
            else
            {
                if (!_requestManagers.ContainsKey(string.Empty))
                {
                    _requestManagers[string.Empty] = new RequestManager();
                }
                return _requestManagers[string.Empty];
            }
        }

        public async Task ProcessAsync(RequestMessage message)
        {
            ConfigManager.Log.Important(string.Format("Processing request {0} for project {1}", message.Content, message.MessageToProjectId));

            //_receiver.SetAzureTargetTopicClientBasedOnCustomerCode(message.CustomerCode);
            var rm = GetRequestManager(message);

            // new request - wait for response
            if (message.MessageType == MessageTypeEnum.RequestCreated)
            {
                RequestMessage response = await ProcessNewRequestAsync(message);
                if (response != null)
                {
                    _receiver.PostMessageNoResponse(response);
                    if (response.MessageType == MessageTypeEnum.RequestProcessed)
                    {
                        CheckCompletedComplexRequests(message);
                        CheckFinishedWaits(rm);
                    }
                    else if (response.MessageType == MessageTypeEnum.Progress)
                    {
                        SendContinuationRequests(response);
                    }
                }
            }
            // acknowledgement is nice, but lets ignore it for now
            else if (message.MessageType == MessageTypeEnum.RequestAcknowledged)
            {
                return;
            }
            // response from core - pass it through to the waiting task
            else if (message.MessageType == MessageTypeEnum.RequestProcessed)
            {
                return;
            }
            // no immediate reaction to progress messages
            else if (message.MessageType == MessageTypeEnum.Progress)
            {
                return;
            }
            else if (message.MessageType == MessageTypeEnum.DbOperationFinished)
            {
                SendContinuationRequests(message);
                CheckCompletedComplexRequests(message);
                CheckFinishedWaits(rm);

            }
            else
            {
                var error = "Unrecognized message: " + message.Serialize();
                ConfigManager.Log.Error(error);
                ConfigManager.Log.FlushMessages();
                throw new Exception(error);
            }
        }

        /// <summary>
        /// Request processing for the web - multipart requests not supported
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public async Task<RequestMessage> ProcessShortAsync(RequestMessage message)
        {
            ConfigManager.Log.Important(string.Format("Processing request {0} for project {1}", message.Content, message.MessageToProjectId));

            //_receiver.SetAzureTargetTopicClientBasedOnCustomerCode(message.CustomerCode);
            var rm = GetRequestManager(message);

            if (message.MessageType != MessageTypeEnum.RequestCreated)
            {
                throw new Exception("Only 'RequestCreated' requests are supported");
            }
            RequestMessage response = await ProcessNewRequestAsync(message);
            
            if (response == null)
            {
                throw new Exception("Failed to process the request, see log for details");
            }

            /*
            var saved = rm.SaveRequestMessage(message);

            if (!saved)
            {
                throw new Exception("Failed to save the request response");
            }
            */

            if (response.MessageType != MessageTypeEnum.RequestProcessed)
            {
                throw new Exception(string.Format("Unexpected response type: {0}", response.MessageType.ToString()));
            }

            rm.SaveRequestMessage(response);

            return response;
        }


        public void ProcessSync(RequestMessage message)
        {
            ConfigManager.Log.Important(string.Format("Processing request {0} for project {1}", message.Content, message.MessageToProjectId));
            //SaveMessage(message);

            //_receiver.SetAzureTargetTopicClientBasedOnCustomerCode(message.CustomerCode);
            var rm = GetRequestManager(message);

            // new request - wait for response
            if (message.MessageType == MessageTypeEnum.RequestCreated)
            {
                RequestMessage response = ProcessNewRequest(message);
                if (response != null)
                {
                    _receiver.PostMessageNoResponse(response);
                    if (response.MessageType == MessageTypeEnum.RequestProcessed)
                    {
                        CheckCompletedComplexRequests(message);
                        CheckFinishedWaits(rm);
                    }
                    else if (response.MessageType == MessageTypeEnum.Progress)
                    {
                        SendContinuationRequests(response);
                    }
                }
            }
            // acknowledgement is nice, but lets ignore it for now
            else if (message.MessageType == MessageTypeEnum.RequestAcknowledged)
            {
                return;
            }
            // response from core - pass it through to the waiting task
            else if (message.MessageType == MessageTypeEnum.RequestProcessed)
            {
                return;
            }
            else if (message.MessageType == MessageTypeEnum.DbOperationFinished)
            {

                SendContinuationRequests(message);
                CheckCompletedComplexRequests(message);
                CheckFinishedWaits(rm);

            }
            else
            {
                throw new Exception();
            }
        }

        /// <summary>
        /// Send continuation messages for progress response or DbOperationFinished response.
        /// </summary>
        /// <param name="message"></param>
        private void SendContinuationRequests(RequestMessage message)
        {
            ConfigManager.Log.Important("Sending continuation requests for message " + message.Content + "(" + message.MessageType.ToString() + ")");

            DLSApiProgressResponse progressResponse = null;
            var rm = GetRequestManager(message);

            if (message.MessageType == MessageTypeEnum.DbOperationFinished)
            {
                var progressMessage = rm.GetProgressForRequest(message.RequestId);
                progressResponse = (DLSApiProgressResponse)DLSApiMessage.Deserialize(progressMessage.Content);
            }
            else if (message.MessageType == MessageTypeEnum.Progress)
            {
                progressResponse = (DLSApiProgressResponse)DLSApiMessage.Deserialize(message.Content);
            }
            else
            {
                throw new Exception();
            }

            // if the message originates from DbOperationFinished, ContinuationsWatForDb is ignored
            if (message.MessageType == MessageTypeEnum.Progress && progressResponse.ContinuationsWaitForDb)
            {
                return;
            }

            ConfigManager.Log.Important("Not waiting for DB");

            if (progressResponse.ContinueWith == null && !progressResponse.ParallelRequests.Any())
            {
                var creationMessage = rm.GetCreationForRequest(message.RequestId);
                SendRequestFinished(creationMessage);
            }

            List<RequestMessage> messagesToFireImmediately = new List<RequestMessage>();

            if (progressResponse.ParallelRequests.Any())
            {
                foreach (var parallel in progressResponse.ParallelRequests)
                {
                    ConfigManager.Log.Important("Parallel request  " + parallel.Serialize());

                    var newRequest = Helpers.CreateRequest(_receiver, message.MessageToProjectId, message.CustomerCode);
                    newRequest.RequestForCoreType = CoreTypeEnum.BIDoc;
                    newRequest.MessageToObjectId = ConfigManager.ServiceReceiverId;
                    newRequest.Content = parallel.Serialize();
                    var rc = rm.SaveRequestMessage(newRequest);
                    if (rc)
                    {
                        ConfigManager.Log.Important("Added immediate request from parallel");
                        messagesToFireImmediately.Add(newRequest);
                    }
                }
            }

            if (progressResponse.ContinueWith != null)
            {
                //ConfigManager.Log.Important("1");
                var newRequest = Helpers.CreateRequest(_receiver, message.MessageToProjectId, message.CustomerCode);
                //ConfigManager.Log.Important("2");
                newRequest.RequestForCoreType = CoreTypeEnum.BIDoc;
                //ConfigManager.Log.Important("3");
                newRequest.MessageToObjectId = ConfigManager.ServiceReceiverId;
                //ConfigManager.Log.Important("4");
                newRequest.Content = progressResponse.ContinueWith.Serialize();
                ConfigManager.Log.Important("Created continuation request after progress response: " + newRequest.Serialize());
                var rc = rm.SaveRequestMessage(newRequest);
                if (rc)
                {
                    ConfigManager.Log.Important("Saved continuation request: " + newRequest.Serialize());
                }
                else
                {
                    ConfigManager.Log.Important("Could not save continuation request: " + newRequest.Serialize());
                }
                // no parallels - fire immediately
                if (!progressResponse.ParallelRequests.Any() && rc)
                {
                    ConfigManager.Log.Important("Firing continuation request");
                    messagesToFireImmediately.Add(newRequest);
                }
                // wait for parallels
                else
                {
                    rm.SaveRequestWaitFor(newRequest.RequestId, messagesToFireImmediately.Select(x => x.RequestId).ToList());
                    rm.SaveRequestWaitForInactive(message.RequestId, new List<Guid>() { newRequest.RequestId });
                }
            }

            // send immediate continuations to self
            
            rm.SaveRequestWaitForInactive(message.RequestId, messagesToFireImmediately.Select(x => x.RequestId).ToList());

            foreach (var req in messagesToFireImmediately)
            {
                ConfigManager.Log.Important("Firing continuation request " + req.Content);

                var creationMessage = rm.GetCreationForRequest(req.RequestId);

                PostMessageToServiceBusCollector(creationMessage);
                /*
                if (ConfigManager.DeploymentMode == DeploymentModeEnum.Azure && ConfigManager.ApplicationClass == ApplicationClassEnum.Service && TopicCollector != null)
                {
                    PostMessageToServiceBusCollector(creationMessage);
                }
                else
                {
                    _sender.PostMessageToServiceBus(creationMessage);
                }
                */
            }

        }


        public void PostMessageToServiceBusCollector(RequestMessage message)
        {
            // (N'RequestProcessed','Timeout','Error','IsBusy')
            var busMessage = new ServiceBusMessage();
            busMessage.MessageType = ServiceBusMessageTypeEnum.RequestMessage;
            ConfigManager.Log.Info("posting message " + message.ToString());
            ConfigManager.Log.Info("posting message " + message.Serialize());
            if (message.MessageType == MessageTypeEnum.RequestProcessed || message.MessageType == MessageTypeEnum.Error || message.MessageType == MessageTypeEnum.IsBusy)
            {
                busMessage.MessageType = ServiceBusMessageTypeEnum.ResponseMessage;
                busMessage.ResponseToRequestId = message.RequestId;
            }
            busMessage.MessageId = message.MessageId;
            busMessage.TargetId = message.MessageToObjectId;
            busMessage.CustomerCode = message.CustomerCode;

            var busMessageSerialized = busMessage.Serialize();
            TopicCollector.Add(busMessageSerialized);
        }

        private void SendRequestFinished(RequestMessage input)
        {
            var respMsg = new RequestMessage()
            {
                MessageFromId = _receiver.Id,
                MessageFromName = _receiver.Name,
                MessageId = Guid.NewGuid(),
                MessageOriginId = _receiver.Id,
                MessageOriginName = _receiver.Name,
                MessageToProjectId = input.MessageToProjectId, // Guid.Empty,
                MessageToObjectId = input.MessageFromId,
                MessageToObjectName = input.MessageFromName,
                MessageType = MessageTypeEnum.RequestProcessed,
                CreatedDateTime = DateTimeOffset.Now,
                RequestForCoreType = input.RequestForCoreType,
                RequestId = input.RequestId,
                Content = new DLSApiMessage().Serialize(), // resp.Serialize(),
                Attachments = new List<Attachment>(),
                CustomerCode = input.CustomerCode,
                RequestFromUserId = input.RequestFromUserId
            };

            foreach (var att in respMsg.Attachments)
            {
                att.MessageId = respMsg.MessageId;
            }
            _receiver.PostMessageNoResponse(respMsg);


            CheckCompletedComplexRequests(input);
        }

        private void CheckFinishedWaits(RequestManager requestManager)
        {
            var unleashedRequests = requestManager.GetRequestsFinishedWaiting();

            // send unleashed requests to self
            
            foreach (var req in unleashedRequests)
            {
                var creationMessage = requestManager.GetCreationForRequest(req);
                var progressMessage = requestManager.GetProgressForRequest(req);
                if (progressMessage != null)
                {
                    SendRequestFinished(creationMessage);
                }
                else
                {
                    PostMessageToServiceBusCollector(creationMessage);
                    /*
                    if (ConfigManager.ApplicationClass == ApplicationClassEnum.Service && ConfigManager.DeploymentMode == DeploymentModeEnum.Azure && TopicCollector != null)
                    {
                        PostMessageToServiceBusCollector(creationMessage);
                    }
                    else
                    {
                        _sender.PostMessageToServiceBus(creationMessage);
                    }
                    */
                }
            }
        }

        private void CheckCompletedComplexRequests(RequestMessage inResponseToMessage)
        {
            var rm = GetRequestManager(inResponseToMessage);
            var completedRequests = rm.GetCompletedComplexRequests(inResponseToMessage.RequestId);

            foreach (var completedRequestId in completedRequests)
            {
                var createdRequest = rm.GetCreationForRequest(completedRequestId);
                SendRequestFinished(createdRequest);
            }
        }


        public async Task<RequestMessage> ProcessNewRequestAsync(RequestMessage request)
        {
            var resp = Helpers.CreateResponse(request);
            resp.MessageType = MessageTypeEnum.RequestAcknowledged;
            if (request.RequestForCoreType == CoreTypeEnum.ManagementApi)
            {
                ProcessManagmentRequest(request);
                _log.Important("Request processed");
                return resp;
            }

            _log.Important(string.Format("Processing request {0} ({1}) by {2}", request.MessageId, request.Content, request.RequestForCoreType.ToString()));
            try
            {
                var res = await ProcessRequest(request);

                _log.Important("Request processed");
                return res;
            }
            catch (Exception ex)
            {
                resp.MessageType = MessageTypeEnum.Error;
                _log.Important(string.Format("Request {0} failed.", request.RequestId));
                _log.Error(ex.Message);
                _log.Error(ex.StackTrace);
                if (ex.InnerException != null)
                {
                    _log.Error(ex.InnerException.Message);
                    _log.Error(ex.InnerException.StackTrace);
                    if (ex.InnerException.InnerException != null)
                    {
                        _log.Error(ex.InnerException.InnerException.Message);
                        _log.Error(ex.InnerException.InnerException.StackTrace);

                    }
                }
                GC.Collect();
                return resp;
            }
        }

        public RequestMessage ProcessNewRequest(RequestMessage request)
        {
            var resp = Helpers.CreateResponse(request);
            resp.MessageType = MessageTypeEnum.RequestAcknowledged;
            if (request.RequestForCoreType == CoreTypeEnum.ManagementApi)
            {
                ProcessManagmentRequest(request);
                _log.Important("Request processed");
                return resp;
            }

            _log.Important(string.Format("Processing request {0} ({1}) by {2}", request.MessageId, request.Content, request.RequestForCoreType.ToString()));
            try
            {
                var res = ProcessRequestSync(request);

                _log.Important("Request processed");
                return res;
            }
            catch (Exception ex)
            {
                resp.MessageType = MessageTypeEnum.Error;
                _log.Important(string.Format("Request {0} failed.", request.RequestId));
                _log.Error(ex.Message);
                _log.Error(ex.StackTrace);
                if (ex.InnerException != null)
                {
                    _log.Error(ex.InnerException.Message);
                }
                GC.Collect();
                return resp;
            }
        }

        public Task<RequestMessage> ProcessRequest(RequestMessage input)
        {
            var requestDeser = DLSApiMessage.Deserialize(input.Content);

            var connString = ConfigManager.GetCustomerDatabaseConnectionString(input.CustomerCode);
            var nb = new NetBridge(true);
            nb.SetConnectionString(connString);
            var pcm = new ProjectConfigManager(nb);

            var projectConfig = pcm.GetProjectConfig(input.MessageToProjectId);

            return Task.Factory.StartNew(() =>
            {
                var resp = FindProcessorAndProcess(requestDeser, projectConfig, input);

                var respMsg = new RequestMessage()
                {
                    MessageFromId = _receiver.Id,
                    MessageFromName = _receiver.Name,
                    MessageId = Guid.NewGuid(),
                    MessageOriginId = _receiver.Id,
                    MessageOriginName = _receiver.Name,
                    MessageToProjectId = input.MessageToProjectId, // Guid.Empty,
                    MessageToObjectId = input.MessageFromId,
                    MessageToObjectName = input.MessageFromName,
                    MessageType = resp is DLSApiProgressResponse ? MessageTypeEnum.Progress : MessageTypeEnum.RequestProcessed,
                    CreatedDateTime = DateTimeOffset.Now,
                    RequestForCoreType = input.RequestForCoreType,
                    RequestId = input.RequestId,
                    Content = resp.Serialize(),
                    Attachments = new List<Attachment>(),
                    CustomerCode = input.CustomerCode,
                    RequestFromUserId = input.RequestFromUserId
                };

                foreach (var att in respMsg.Attachments)
                {
                    att.MessageId = respMsg.MessageId;
                }

                //if (!(resp is DLSApiProgressResponse))
                //{
                //    CheckWaitingMessagesAfterRequestProcessed(input.RequestId, respMsg);
                //}
                //else
                //{
                //    var progressResponse = resp as DLSApiProgressResponse;
                //    SendContinuationRequests(progressResponse, respMsg);
                //}

                return respMsg;
            });
        }


        public RequestMessage ProcessRequestSync(RequestMessage input)
        {
            var requestDeser = DLSApiMessage.Deserialize(input.Content);

            var connString = ConfigManager.GetCustomerDatabaseConnectionString(input.CustomerCode);
            var nb = new NetBridge(true);
            nb.SetConnectionString(connString);
            var pcm = new ProjectConfigManager(nb);

            var projectConfig = pcm.GetProjectConfig(input.MessageToProjectId);

            //return Task.Factory.StartNew(() =>
            //{
            var resp = FindProcessorAndProcess(requestDeser, projectConfig, input);

            var respMsg = new RequestMessage()
            {
                MessageFromId = _receiver.Id,
                MessageFromName = _receiver.Name,
                MessageId = Guid.NewGuid(),
                MessageOriginId = _receiver.Id,
                MessageOriginName = _receiver.Name,
                MessageToProjectId = input.MessageToProjectId, // Guid.Empty,
                MessageToObjectId = input.MessageFromId,
                MessageToObjectName = input.MessageFromName,
                MessageType = resp is DLSApiProgressResponse ? MessageTypeEnum.Progress : MessageTypeEnum.RequestProcessed,
                CreatedDateTime = DateTimeOffset.Now,
                RequestForCoreType = input.RequestForCoreType,
                RequestId = input.RequestId,
                Content = resp.Serialize(),
                Attachments = new List<Attachment>(),
                CustomerCode = input.CustomerCode
            };

            foreach (var att in respMsg.Attachments)
            {
                att.MessageId = respMsg.MessageId;
            }

            //if (!(resp is DLSApiProgressResponse))
            //{
            //    CheckWaitingMessagesAfterRequestProcessed(input.RequestId, respMsg);
            //}
            //else
            //{
            //    var progressResponse = resp as DLSApiProgressResponse;
            //    SendContinuationRequests(progressResponse, respMsg);
            //}

            return respMsg;
            //});
        }
        
        private DLSApiMessage FindProcessorAndProcess(DLSApiMessage message, ProjectConfig projectConfig, RequestMessage requestMessage)
        {
            {
                var msgType = message.GetType().BaseType;
                if (msgType.IsGenericType)
                {
                    var genType = msgType.GetGenericTypeDefinition();
                    if (genType == typeof(DLSApiRequest<>))
                    {

                    }
                }

                var responseType = message.GetType().BaseType.GetGenericArguments()[0];
                var classes = Assembly.GetExecutingAssembly().GetTypes().Where(x => x.IsClass);
                foreach (var t in classes)
                {
                    var parameterlessConstructor = t.GetConstructor(Type.EmptyTypes);
                    if (parameterlessConstructor == null)
                    {
                        continue;
                    }
                    foreach (Type intType in t.GetInterfaces())
                    {
                        var genericProcessorType = typeof(IRequestProcessor<,>);
                        //var genericProcessorMethod = genericProcessorType.method
                        if (intType.IsGenericType && intType.GetGenericTypeDefinition()
                            == genericProcessorType)
                        {
                            var requestType = intType.GenericTypeArguments[0];
                            if (requestType.IsAssignableFrom(message.GetType()))
                            {
                                var interfaceMethod = intType.GetMethod("Process");

                                var processorInstance = parameterlessConstructor.Invoke(Type.EmptyTypes);
                                var initMethod = processorInstance.GetType().GetMethod("Init", new Type[] { typeof(RequestMessage) });
                                initMethod.Invoke(processorInstance, new object[] { requestMessage });

                                var map = t.GetInterfaceMap(interfaceMethod.DeclaringType);
                                var index = Array.IndexOf(map.InterfaceMethods, interfaceMethod);

                                if (index == -1)
                                {
                                    //this should literally be impossible
                                }

                                var instanceMethod = map.TargetMethods[index];
                                var processingResult = instanceMethod.Invoke(processorInstance, new object[] { message, projectConfig });
                                return (DLSApiMessage)processingResult;
                            }
                        }
                    }
                }

                throw new NotImplementedException();
            }
        }

        private void ProcessManagmentRequest(RequestMessage request)
        {
            //var deser = ManagementMessage.Deserialize(request.Content);
            //if (deser is UpdateConfigRequest)
            //{
            //    _projectConfig = ConfigManager.GetProjectConfig(_projectConfig.ProjectConfigId);

            //}
        }

    }
}

using CD.DLS.Common.Structures;
using CD.DLS.DAL.Configuration;
using CD.DLS.DAL.Engine;
using CD.DLS.DAL.Managers;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;
using Nito.AsyncEx;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CD.DLS.DAL.Receiver
{
    public class Receiver : IDisposable, IReceiver
    {
        private Guid _id;
        private string _name;
        private SqlDependency _messageDependency;

        public Guid Id { get { return _id; } }
        public string Name { get { return _name; } }
        //topicClient = new TopicClient(ServiceBusConnectionString, topicName);
        private TopicClient _targetTopicClient = null;
        private Dictionary<string, TopicClient> _topicClientsPerTopic = new Dictionary<string, TopicClient>();
        private MessageReceiver _subscriptionClient = null;
        private RequestManager _requestManager = null;
        HashSet<Guid> _broadcastMessagesReceived = new HashSet<Guid>();

        private TopicClient GetTopicClient(string topicName)
        {
            if (!_topicClientsPerTopic.ContainsKey(topicName))
            {
                _topicClientsPerTopic[topicName] = new TopicClient(Configuration.ConfigManager.ServiceBusConnectionString, topicName);
            }
            return _topicClientsPerTopic[topicName];
        }

        private Dictionary<string, RequestManager> _requestManagersByCustomer = new Dictionary<string, RequestManager>();

        private RequestManager GetCustomerRequestManager(string customerCode)
        {
            if (!_requestManagersByCustomer.ContainsKey(customerCode))
            {
                var nb = new NetBridge(true);
                nb.SetConnectionString(ConfigManager.GetCustomerDatabaseConnectionString(customerCode));
                var rm = new RequestManager(nb);
                _requestManagersByCustomer[customerCode] = rm;
            }

            return _requestManagersByCustomer[customerCode];
        }

        struct WaitingDependency
        {
            public Guid RequestId;
            public AsyncManualResetEvent Signal;
        }
        private Dictionary<string, WaitingDependency> _waitingDependencies = new Dictionary<string, WaitingDependency>();

        public Receiver(Guid id, string name, string subscriptionTopicName = null, string subscriptionName = null, RequestManager requestManager = null, bool senderOnly = false)
        {
            _id = id;
            _name = name;
            if (requestManager == null)
            {
                if (ConfigManager.DeploymentMode == DeploymentModeEnum.OnPremises || ConfigManager.ApplicationClass == ApplicationClassEnum.Client || ConfigManager.ApplicationClass == ApplicationClassEnum.WebClient)
                {
                    _requestManager = new RequestManager();
                }
            }
            else
            {
                _requestManager = requestManager;
            }

            if (ConfigManager.DeploymentMode == DeploymentModeEnum.Azure)
            {
                if (ConfigManager.ApplicationClass == ApplicationClassEnum.Service)
                {
                    // cannot set it now - the service serves all
                    _targetTopicClient = null;
                }
                else if (ConfigManager.ApplicationClass == ApplicationClassEnum.Client || ConfigManager.ApplicationClass == ApplicationClassEnum.WebClient)
                {
                    _targetTopicClient = GetTopicClient(ConfigManager.ServiceBusServiceTopicName); // new TopicClient(Configuration.ConfigManager.ServiceBusConnectionString, ConfigManager.ServiceBusServiceTopicName);
                }
            }

            if (!senderOnly)
            {
                if (Configuration.ConfigManager.QueueMode == Configuration.QueueModeEnum.ServiceBroker)
                {
                    _messageDependency = _requestManager.GetMessageHandle(_id);
                    _messageDependency.OnChange += BrokerMessagesReceived;
                }
                else if (Configuration.ConfigManager.QueueMode == Configuration.QueueModeEnum.AzureTopic)
                {
                    if (subscriptionName == null)
                    {
                        if (ConfigManager.ApplicationClass == ApplicationClassEnum.Client)
                        {
                            subscriptionName = Configuration.ConfigManager.ServiceBusCustomerTopicSubscription;
                        }
                        else if(ConfigManager.ApplicationClass == ApplicationClassEnum.Service)
                        {
                            subscriptionName = ConfigManager.ServiceBusServiceTopicSubscription;
                        }
                    }
                    if (subscriptionTopicName == null)
                    {
                        if (ConfigManager.ApplicationClass == ApplicationClassEnum.Client || ConfigManager.ApplicationClass == ApplicationClassEnum.WebClient)
                        {
                            subscriptionTopicName = Configuration.ConfigManager.ServiceBusCustomerTopicName;
                        }
                        else if (ConfigManager.ApplicationClass == ApplicationClassEnum.Service)
                        {
                            subscriptionTopicName = ConfigManager.ServiceBusServiceTopicName;
                        }
                    }
                    var serviceBusConnectionString = Configuration.ConfigManager.ServiceBusConnectionString;

                    _subscriptionClient = new MessageReceiver(serviceBusConnectionString, EntityNameHelper.FormatSubscriptionPath(subscriptionTopicName, subscriptionName), ReceiveMode.PeekLock);
                    
                    //_subscriptionClient = new SubscriptionClient(serviceBusConnectionString, subscriptionTopicName, subscriptionName);
                    RegisterServiceBusOnMessageHandlerAndReceiveMessages();
                }
            }
        }

        /// <summary>
        /// For the service's target switching
        /// </summary>
        /// <param name="topicName"></param>
        public void SetAzureTargetTopicClient(string topicName)
        {
            //if (_targetTopicClient != null)
            //{
            //    _targetTopicClient.CloseAsync().Wait();
            //}
            _targetTopicClient = GetTopicClient(topicName); // new TopicClient(Configuration.ConfigManager.ServiceBusConnectionString, topicName);
        }

        public void SetAzureTargetTopicClientBasedOnCustomerCode(string customerCode)
        {
            var topicName = ConfigManager.GetServiceBusCustomerTopicName(customerCode);
            //if (_targetTopicClient != null)
            //{
            //    _targetTopicClient.CloseAsync().Wait();
            //}
            _targetTopicClient = GetTopicClient(topicName); // new TopicClient(Configuration.ConfigManager.ServiceBusConnectionString,  topicName);
        }

        private void RegisterServiceBusOnMessageHandlerAndReceiveMessages()
        {
            // Configure the message handler options in terms of exception handling, number of concurrent messages to deliver, etc.
            var messageHandlerOptions = new MessageHandlerOptions(ServiceBusExceptionReceivedHandler)
            {
                // Maximum number of concurrent calls to the callback ProcessMessagesAsync(), set to 1 for simplicity.
                // Set it according to how many messages the application wants to process in parallel.
                MaxConcurrentCalls = 10,

                // Indicates whether MessagePump should automatically complete the messages after returning from User Callback.
                // False below indicates the Complete will be handled by the User Callback as in `ProcessMessagesAsync` below.
                AutoComplete = false
            };

            // Register the function that processes messages.
            _subscriptionClient.RegisterMessageHandler(ProcessServiceBusMessagesAsync, messageHandlerOptions);
            ConfigManager.Log.Important(string.Format("Listening for requests in {0}", _subscriptionClient.Path /* TopicPath, _subscriptionClient.SubscriptionName*/));
        }

        private Task ServiceBusExceptionReceivedHandler(ExceptionReceivedEventArgs exceptionReceivedEventArgs)
        {
            ConfigManager.Log.Error($"Message handler encountered an exception {exceptionReceivedEventArgs.Exception}.");
            var context = exceptionReceivedEventArgs.ExceptionReceivedContext;
            ConfigManager.Log.Error("Exception context for troubleshooting:");
            ConfigManager.Log.Error($"- Endpoint: {context.Endpoint}");
            ConfigManager.Log.Error($"- Entity Path: {context.EntityPath}");
            ConfigManager.Log.Error($"- Executing Action: {context.Action}");
            return Task.CompletedTask;
        }

        private async Task ProcessServiceBusMessagesAsync(Message message, CancellationToken token)
        {

            ServiceBusMessage busMessage;
            
            var body = Encoding.UTF8.GetString(message.Body);

            busMessage = ServiceBusMessage.Deserialize(body);
            
            // Process the message.
            //Console.WriteLine($"Received message: SequenceNumber:{message.SystemProperties.SequenceNumber} Body:{body}");
            
            //try
            //{
            //}
            //catch
            //{
            //    await _subscriptionClient.CompleteAsync(message.SystemProperties.LockToken);
            //    throw;
            //}
            
            // this message is not for me, leave it be
            if (busMessage.TargetId != _id && busMessage.MessageType != ServiceBusMessageTypeEnum.ServiceLog && busMessage.MessageType != ServiceBusMessageTypeEnum.Broadcast)
            {
                await _subscriptionClient.AbandonAsync(message.SystemProperties.LockToken);
                return;
            }

            if (busMessage.MessageType != ServiceBusMessageTypeEnum.Broadcast)
            {
                await _subscriptionClient.CompleteAsync(message.SystemProperties.LockToken);
            }
            else
            {
                if (busMessage.CustomerCode == null)
                {
                    await _subscriptionClient.CompleteAsync(message.SystemProperties.LockToken);
                    return;
                }
                RequestManager rm = GetCustomerRequestManager(busMessage.CustomerCode);
                var broadcastMsg = rm.GetBroadcastMessageById(busMessage.MessageId);
                if (broadcastMsg == null)
                {
                    await _subscriptionClient.CompleteAsync(message.SystemProperties.LockToken);
                    return;
                }
                if (broadcastMsg.Active == false)
                {
                    await _subscriptionClient.CompleteAsync(message.SystemProperties.LockToken);
                    return;
                }
                if (_broadcastMessagesReceived.Contains(busMessage.MessageId))
                {
                    return;
                }
                _broadcastMessagesReceived.Add(busMessage.MessageId);

                BroadcastMessageReceived?.Invoke(broadcastMsg);

                return;
            }

            ConfigManager.Log.Important("Received SB message " + busMessage.MessageId + " from " + _subscriptionClient.Path + " as a " + ConfigManager.ApplicationClass.ToString() + " (" + _id.ToString() + ")");


            //await _subscriptionClient.CompleteAsync(message.SystemProperties.LockToken);
            
            //if (busMessage.MessageType != ServiceBusMessage.MessageTypeEnum.ServiceLog)
            //{
            //    // Complete the message so that it is not received again.
            //    // This can be done only if the subscriptionClient is created in ReceiveMode.PeekLock mode (which is the default).
            //    try
            //    {
            //        await _subscriptionClient.CompleteAsync(message.SystemProperties.LockToken);
            //    }
            //    catch
            //    {

            //    }
            //}

            //////////////
            //return;
            //////////////

            if (busMessage.MessageType == ServiceBusMessageTypeEnum.ResponseMessage)
            {
                var dependencyId = busMessage.ResponseToRequestId.ToString();
                if (!_waitingDependencies.ContainsKey(dependencyId))
                {
                    return;
                }
                var waitingDependency = _waitingDependencies[dependencyId];
                waitingDependency.Signal.Set();
            }
            else if (busMessage.MessageType == ServiceBusMessageTypeEnum.RequestMessage)
            {
                // var connString = ConfigManager.GetCustomerDatabaseConnectionString(busMessage.CustomerCode);
                // var nb = new NetBridge(true);
                // nb.SetConnectionString(connString);
                // new RequestManager(nb);
                RequestManager rm = GetCustomerRequestManager(busMessage.CustomerCode);
                var newMessage = rm.GetMessageById(busMessage.MessageId);
                MessageReceived?.Invoke(newMessage);

                //List<RequestMessage> newMessages = rm.GetNewMessagesToObject(_id);
                ////_messageDependency = _requestManager.GetMessageHandle(_id);
                //foreach (var newMessage in newMessages)
                //{
                //    MessageReceived?.Invoke(newMessage);
                //}
            }
            else if (busMessage.MessageType == ServiceBusMessageTypeEnum.ServiceLog)
            {
                Configuration.ConfigManager.Log.Info("Message from service: " + busMessage.Content);
            }
            else
            {
                throw new NotImplementedException();
            }


            // Note: Use the cancellationToken passed as necessary to determine if the subscriptionClient has already been closed.
            // If subscriptionClient has already been closed, you can choose to not call CompleteAsync() or AbandonAsync() etc.
            // to avoid unnecessary exceptions.
        }



        public async Task<RequestMessage> PostMessage(RequestMessage message)
        {
            message.MessageFromId = this.Id;
            message.MessageFromName = this.Name;
            RequestManager rm = _requestManager;
            if (ConfigManager.DeploymentMode == DeploymentModeEnum.Azure || ConfigManager.ApplicationClass == ApplicationClassEnum.Service)
            {
                //var nb = new NetBridge(true);
                //nb.SetConnectionString(ConfigManager.GetCustomerDatabaseConnectionString(message.CustomerCode));
                //rm = new RequestManager(nb);
                rm = GetCustomerRequestManager(message.CustomerCode);
            }

            rm.SaveRequestMessage(message);
            
            var semaphore = new SemaphoreSlim(0, 1);

            string dependencyId = null;
            if (Configuration.ConfigManager.QueueMode == Configuration.QueueModeEnum.ServiceBroker)
            {
                var respHandle = rm.GetResponseHandle(message);
                dependencyId = respHandle.Id;
                respHandle.OnChange += BrokerResponseReceived;    
            }
            // the will handle itself in ProcessServiceBusMessagesAsync
            // but need to post the message to the service bus
            else if (Configuration.ConfigManager.QueueMode == Configuration.QueueModeEnum.AzureTopic)
            {
                await PostMessageToServiceBus(message);
                dependencyId = message.RequestId.ToString();
            }

            var waitingDependency = new WaitingDependency() { RequestId = message.RequestId, Signal = new AsyncManualResetEvent(false) };
            _waitingDependencies.Add(dependencyId, waitingDependency);

            await waitingDependency.Signal.WaitAsync();

            RequestMessage response = rm.GetResponseForRequest(message.RequestId);
            rm.SetMessageReceived(response.MessageId);

            return response;
        }


        public async void PostBroadcastMessageToServiceBus(BroadcastMessage broadcastMessage, string customerCode)
        {
            var busMessage = new ServiceBusMessage();
            busMessage.MessageType = ServiceBusMessageTypeEnum.Broadcast;
            busMessage.MessageId = broadcastMessage.BroadcastMessageId;
            busMessage.Content = broadcastMessage.Serialize();
            busMessage.CustomerCode = customerCode;
            var busMessageSerialized = busMessage.Serialize();
            var busMessageWrap = new Message(Encoding.UTF8.GetBytes(busMessageSerialized));

            var topicName = ConfigManager.GetServiceBusCustomerTopicName(customerCode);
            var targetTopicClient = GetTopicClient(topicName);
            await targetTopicClient.SendAsync(busMessageWrap);
        }

        public async Task PostMessageNoResponse(RequestMessage message)
        {
            message.MessageFromId = this.Id;
            message.MessageFromName = this.Name;
            bool rc;
            if (ConfigManager.DeploymentMode == DeploymentModeEnum.OnPremises || ConfigManager.ApplicationClass == ApplicationClassEnum.Client || ConfigManager.ApplicationClass == ApplicationClassEnum.WebClient)
            {
                rc = _requestManager.SaveRequestMessage(message);
            }
            else
            {
                //var nb = new NetBridge(true);
                //nb.SetConnectionString(ConfigManager.GetCustomerDatabaseConnectionString(message.CustomerCode));
                //var rm = new RequestManager(nb);
                var rm = GetCustomerRequestManager(message.CustomerCode);
                rc = rm.SaveRequestMessage(message);
            }
            if (Configuration.ConfigManager.QueueMode == Configuration.QueueModeEnum.AzureTopic && rc)
            {
                await PostMessageToServiceBus(message);
            }
        }

        public async Task PostMessageToServiceBus(RequestMessage message)
        {
            // (N'RequestProcessed','Timeout','Error','IsBusy')
            var busMessage = new ServiceBusMessage();
            busMessage.MessageType = ServiceBusMessageTypeEnum.RequestMessage;
            if (message.MessageType == MessageTypeEnum.RequestProcessed || message.MessageType == MessageTypeEnum.Error || message.MessageType == MessageTypeEnum.IsBusy)
            {
                busMessage.MessageType = ServiceBusMessageTypeEnum.ResponseMessage;
                busMessage.ResponseToRequestId = message.RequestId;
            }
            busMessage.MessageId = message.MessageId;
            busMessage.TargetId = message.MessageToObjectId;
            busMessage.CustomerCode = message.CustomerCode;

            var busMessageSerialized = busMessage.Serialize();
            ConfigManager.Log.Important("Posting SB message " + busMessage.MessageId + " to " + _targetTopicClient.TopicName);
            var busMessageWrap = new Message(Encoding.UTF8.GetBytes(busMessageSerialized));
            int retries = 5;
            while (true)
            {
                try
                {
                    await _targetTopicClient.SendAsync(busMessageWrap); //.Wait();
                    break;
                }
                catch (ServiceBusCommunicationException)
                {
                    if (retries < 1)
                    {
                        throw;
                    }
                    retries--;
                    continue;
                }
                catch (ServiceBusTimeoutException)
                {
                    if (retries < 1)
                    {
                        throw;
                    }
                    retries--;
                    continue;
                }
            }
            ConfigManager.Log.Important("Message sent");

        }

        public void SendServiceBusMessage(string busMessageSerialized)
        {
            var busMessageWrap = new Message(Encoding.UTF8.GetBytes(busMessageSerialized));
            _targetTopicClient.SendAsync(busMessageWrap);
        }

        public delegate void MessageHandler(RequestMessage message);
        public event MessageHandler MessageReceived;

        public delegate void BroadcastMessageHandler(BroadcastMessage message);
        public event BroadcastMessageHandler BroadcastMessageReceived;


        private void BrokerResponseReceived(object sender,
   SqlNotificationEventArgs e)
        {
            var dependency = sender as SqlDependency;

            dependency.OnChange -= BrokerResponseReceived;
            if (!_waitingDependencies.ContainsKey(dependency.Id))
            {
                return;
            }
            var waitingDependency = _waitingDependencies[dependency.Id];
            waitingDependency.Signal.Set();
        }

        
        private void BrokerMessagesReceived(object sender,
   SqlNotificationEventArgs e)
        {
            var dependency = sender as SqlDependency;
            dependency.OnChange -= BrokerResponseReceived;

            if (_waitingDependencies.ContainsKey(dependency.Id))
            {
                BrokerResponseReceived(sender, e);
            }

            List<RequestMessage> newMessages = _requestManager.GetNewMessagesToObject(_id);
            _messageDependency = _requestManager.GetMessageHandle(_id);
            foreach (var message in newMessages)
            {
                MessageReceived?.Invoke(message);
            }

            _messageDependency = _requestManager.GetMessageHandle(_id);
            _messageDependency.OnChange += BrokerMessagesReceived;
        }

        public void Dispose()
        {
            //if (_targetTopicClient != null)
            //{
            //    _targetTopicClient.CloseAsync();
            //}
            foreach (var topicClient in _topicClientsPerTopic.Values)
            {
                if (!topicClient.IsClosedOrClosing)
                {
                    topicClient.CloseAsync();
                }
            }
            _requestManager = null;
            _requestManagersByCustomer.Clear();
            CloseSubscriptionMessageReceiver();
        }

        public void CloseSubscriptionMessageReceiver()
        {
            if (_subscriptionClient != null)
            {
                _subscriptionClient.CloseAsync().Wait();
            }
        }
    }
}

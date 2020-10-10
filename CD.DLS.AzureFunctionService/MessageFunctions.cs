using CD.DLS.DAL.Configuration;
using CD.DLS.DAL.Engine;
using CD.DLS.DAL.Managers;
using CD.DLS.DAL.Misc;
using CD.DLS.DAL.Receiver;
using CD.DLS.RequestProcessor;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Azure.WebJobs.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using System;

namespace CD.DLS.AzureFunctionService
{
    public static class MessageFunctions
    {
        [FunctionName("ProcessMessage")]
        public static void Run([ServiceBusTrigger("requestmessagestopic", "requestmessagesubscription", AccessRights.Manage, Connection = "sbConnection")]string mySbMsg, TraceWriter log, 
            [ServiceBus("requestmessagestopic", AccessRights.Manage, Connection = "sbConnection", EntityType = EntityType.Topic)] ICollector<string> topicCollector)
        {

            try
            {
                //log.Info("Received SB message " + mySbMsg);

                ConfigManager.ApplicationClass = ApplicationClassEnum.Service;

                ServiceBusMessage busMessage;
                log.Info("Deserializing message " + mySbMsg);
                
                
                //topicCollector.Add("Hello world");
                //topicCollector.Add("Hi again");
                try
                {
                    busMessage = ServiceBusMessage.Deserialize(mySbMsg);
                }
                catch
                {
                    return;
                }
                //return;

                var dbConnstring = ConfigManager.GetCustomerDatabaseConnectionString(busMessage.CustomerCode);
                var nbrg = new NetBridge(true);
                nbrg.SetConnectionString(dbConnstring);
                
                var logManager = new LogManager(nbrg);
                //var logger = new AzureFunctionLogger(log);
                //logger.DbLogger = new DbLogger(logManager);
                ConfigManager.Log = new DbLogger(logManager); //logger;

                ConfigManager.Log.Info("KV: " + ConfigManager.GetAzureKeyVaultBaseAddress(busMessage.CustomerCode));
                ConfigManager.Log.Info("DLS Service Received SB message " + mySbMsg);
                //ConfigManager.Log.Info("Connstring: " + dbConnstring);

                //var dlsLogger = (EventLogger)ConfigManager.Log;
                //dlsLogger.LogEvent += (s, e) =>
                //{
                //    if (e.LogType == Common.Interfaces.LogTypeEnum.Error)
                //    {
                //        log.Error(e.Message + Environment.NewLine + e.StackTrace, null, "DLS Service");
                //    }
                //    else
                //    {
                //        log.Info(e.Message, "DLS Service");
                //    }
                //};

                //ConfigManager.Log.Important("Received SB message " + busMessage.MessageId);

                //// this message is not for me, leave it be
                if (busMessage.MessageType == ServiceBusMessageTypeEnum.ServiceLog)
                {
                    ConfigManager.Log.FlushMessages();
                    return;
                }


                //////////////
                //return;
                //////////////


                if (busMessage.MessageType == ServiceBusMessageTypeEnum.ResponseMessage)
                {
                    ConfigManager.Log.Important("Received reponse message " + mySbMsg + " - will not be processed by this function");
                }
                else if (busMessage.MessageType == ServiceBusMessageTypeEnum.RequestMessage)
                {
                    var connString = ConfigManager.GetCustomerDatabaseConnectionString(busMessage.CustomerCode);
                    var nb = new NetBridge(true);
                    nb.SetConnectionString(connString);
                    RequestManager rm = new RequestManager(nb);

                    // if this message has not been received by someone else
                    if (!rm.MarkMessageReceived(busMessage.MessageId))
                    {
                        ConfigManager.Log.Important(string.Format("Not processing message {0} - it has already been received", busMessage.MessageId));
                        ConfigManager.Log.FlushMessages();
                        return;
                    }

                    var newMessage = rm.GetMessageById(busMessage.MessageId);

                    RequestProcessor.MessageProcessor mp = new RequestProcessor.MessageProcessor(busMessage.CustomerCode);
                    mp.TopicCollector = topicCollector;

                    //ConfigManager.Log.Important("Processing request...");
                    //mp.ProcessSync(newMessage); //.Wait();

                    //await mp.ProcessAsync(newMessage); //.Wait();
                    mp.ProcessSync(newMessage); 

                    //MessageReceived?.Invoke(newMessage);

                    //List<RequestMessage> newMessages = rm.GetNewMessagesToObject(_id);
                    ////_messageDependency = _requestManager.GetMessageHandle(_id);
                    //foreach (var newMessage in newMessages)
                    //{
                    //    MessageReceived?.Invoke(newMessage);
                    //}
                }
                else if (busMessage.MessageType == ServiceBusMessageTypeEnum.ServiceLog)
                {
                    //Configuration.ConfigManager.Log.Info("Message from service: " + busMessage.Content);
                    ConfigManager.Log.Important("Received reponse message " + mySbMsg + " - will not be processed by this function");
                }
                else
                {
                    throw new NotImplementedException();
                }

                ConfigManager.Log.FlushMessages();
            }
            catch (Exception ex)
            {
                log.Error(ex.Message + Environment.NewLine + ex.StackTrace, ex, "DLS Service");
                ConfigManager.Log.FlushMessages();
                throw;
            }

        }


    }
}

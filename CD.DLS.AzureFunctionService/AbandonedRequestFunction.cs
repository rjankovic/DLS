using System;
using System.Collections.Generic;
using CD.DLS.Common.Structures;
using CD.DLS.DAL.Configuration;
using CD.DLS.DAL.Engine;
using CD.DLS.DAL.Managers;
using CD.DLS.DAL.Misc;
using CD.DLS.DAL.Receiver;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;

namespace CD.DLS.AzureFunctionService
{
    public static class AbandonedRequestFunction
    {
        /// <summary>
        /// Find all abandonec requests for each customer every 10 minutes
        /// </summary>
        /// <param name="myTimer"></param>
        /// <param name="log"></param>
        [FunctionName("AbandonedRequestFunction")]
        public static void Run([TimerTrigger("0 */10 * * * *")]TimerInfo myTimer, TraceWriter log)
        {
            log.Info($"C# Timer trigger function executed at: {DateTime.Now}");

            try
            {
                //log.Info("Received SB message " + mySbMsg);

                ConfigManager.ApplicationClass = ApplicationClassEnum.Service;

                var customerListString = ConfigManager.CustomerList;

                log.Info("Customer list: " + (customerListString == null ? "null" : customerListString));

                var customerList = new List<string>(customerListString.Split(','));

                foreach (var customerCode in customerList)
                {
                    var dbConnstring = ConfigManager.GetCustomerDatabaseConnectionString(customerCode);
                    var nbrg = new NetBridge(true);
                    nbrg.SetConnectionString(dbConnstring);

                    var logManager = new LogManager(nbrg);
                    var requestManager = new RequestManager(nbrg);
                    ConfigManager.Log = new DbLogger(logManager);

                    string msg = null;

                    msg = "Checking abandoned requests for customer " + customerCode;
                    log.Info(msg);
                    ConfigManager.Log.Info(msg);
                    ConfigManager.Log.FlushMessages();

                    var abandonedRequests = requestManager.GetAbandonedRequests();
                    var receiver = new HttpReceiver(customerCode);
                    
                    foreach (var abandonedRequest in abandonedRequests)
                    {
                        var message = requestManager.GetMessageById(abandonedRequest.MessageId);
                        msg = string.Format("Resending message {0} to {2}: {1}", message.MessageId, message.Content, customerCode);
                        log.Info(msg);
                        ConfigManager.Log.Info(msg);
                        ConfigManager.Log.FlushMessages();

                        requestManager.MarkMessageUnreceived(abandonedRequest.MessageId);
                        try
                        {
                            var task = receiver.PostMessageNoResponse(message); // .PostMessageToServiceBus(message);
                            log.Info("Message " + message.MessageId.ToString() + " resent");
                            task.Wait();
                            log.Info("Message " + message.MessageId.ToString() + " reprocessed");
                        }
                        catch (Exception ex)
                        {
                            var errMsg = ex.Message + Environment.NewLine + ex.StackTrace + (ex.InnerException == null ? "" : (Environment.NewLine + ex.InnerException.Message + Environment.NewLine + ex.InnerException.StackTrace));
                            log.Error(errMsg);
                            ConfigManager.Log.Error(errMsg);
                            ConfigManager.Log.FlushMessages();
                            continue;
                        }
                    }

                }

            }
            catch (Exception ex)
            {
                log.Error(ex.Message + Environment.NewLine + ex.StackTrace, ex, "DLS Service");
                ConfigManager.Log.Error(ex.Message + Environment.NewLine + ex.StackTrace, ex, "DLS Service");
                ConfigManager.Log.FlushMessages();
                throw;
            }
        }


    }
}

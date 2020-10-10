using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using CD.DLS.Common.Structures;
using CD.DLS.DAL.Configuration;
using CD.DLS.DAL.Engine;
using CD.DLS.DAL.Managers;
using CD.DLS.DAL.Misc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;

namespace CD.DLS.AzureFunctionService
{
    public static class HttpProcessMessage
    {
        [FunctionName("HttpProcessMessage")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)]HttpRequestMessage req, TraceWriter log)
        {
            try
            {
                ConfigManager.ApplicationClass = ApplicationClassEnum.Service;

                var kvp = req.GetQueryNameValuePairs();
                log.Info(string.Format("HTTP request processor invoked: {0}", kvp.Select(x => string.Format("{0}: {1}", x.Key, x.Value)), "; "));
                
                // parse query parameter
                string customerCode = req.GetQueryNameValuePairs()
                    .FirstOrDefault(q => string.Compare(q.Key, "customer", true) == 0)
                    .Value;

                Guid messageId = Guid.Parse(req.GetQueryNameValuePairs()
                    .FirstOrDefault(q => string.Compare(q.Key, "message", true) == 0)
                    .Value);

                var connString = ConfigManager.GetCustomerDatabaseConnectionString(customerCode);
                var nb = new NetBridge(true);
                nb.SetConnectionString(connString);
                var nbrg = new NetBridge(true);
                nbrg.SetConnectionString(connString);

                var logManager = new LogManager(nbrg);
                ConfigManager.Log = new DbLogger(logManager);
                
                RequestManager rm = new RequestManager(nb);

                // if this message has not been received by someone else
                if (!rm.MarkMessageReceived(messageId))
                {
                    var error = string.Format("Not processing message {0} - it has already been received", messageId);
                    ConfigManager.Log.Important(error);
                    ConfigManager.Log.FlushMessages();
                    return req.CreateResponse(HttpStatusCode.BadRequest, error);
                }

                var newMessage = rm.GetMessageById(messageId);

                RequestMessage response;
                try
                {
                    RequestProcessor.MessageProcessor mp = new RequestProcessor.MessageProcessor(newMessage.CustomerCode);
                    //response = mp.ProcessSync(newMessage); //
                    //response = await mp.ProcessShortAsync(newMessage);

                    //response = await mp.ProcessShortAsync(newMessage);

                    await mp.ProcessAsync(newMessage);
                    response = rm.GetResponseForRequest(newMessage.RequestId);

                    // don't send the content directly, let it be read from DB
                    response.Content = null;
                    ConfigManager.Log.FlushMessages();
                }
                catch (Exception ex)
                {
                    var msg = string.Format("Failed to process message {0}: {1} {2} {3}", newMessage.MessageId, ex.Message, Environment.NewLine, ex.StackTrace);
                    log.Error(msg);
                    log.Flush();
                    ConfigManager.Log.Error(msg);
                    ConfigManager.Log.FlushMessages();
                    return req.CreateErrorResponse(HttpStatusCode.InternalServerError, msg);
                }

                return req.CreateResponse(HttpStatusCode.OK, response);
            }
            catch (Exception outerEx)
            {
                var msg = string.Format("Failed to process message: {0} {1} {2} {3}", outerEx.Message, Environment.NewLine, outerEx.StackTrace, (outerEx.InnerException == null ? "" : outerEx.InnerException.Message));
                log.Error(msg);
                log.Flush();
                ConfigManager.Log.Error(msg);
                ConfigManager.Log.FlushMessages();
                return req.CreateErrorResponse(HttpStatusCode.InternalServerError, msg);
            }
        }
    }
}
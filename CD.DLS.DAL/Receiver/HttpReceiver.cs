using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using CD.DLS.Common.Structures;
using CD.DLS.DAL.Configuration;
using CD.DLS.DAL.Engine;
using CD.DLS.DAL.Identity;
using CD.DLS.DAL.Managers;

namespace CD.DLS.DAL.Receiver
{
    public class HttpReceiver : IReceiver
    {
        private Dictionary<string, RequestManager> _requestManagersByCustomer = new Dictionary<string, RequestManager>();

        private Guid _id;
        public Guid Id
        {
            get { return _id; }
        }

        private string _name;
        public string Name { get { return _name; } }

        private string _httpFuncctionKey;
        private string _httpFunctionAddress;
        private string _customerCode;

        public HttpReceiver(string customerCode)
        {
            _customerCode = customerCode;
            _id = Guid.NewGuid();
            _name = "HTTP Receiver";
            if (ConfigManager.ApplicationClass == ApplicationClassEnum.Daemon)
            {
                _httpFunctionAddress = ConfigManager.AzureFunctionApi;
                _httpFuncctionKey = ConfigManager.AzureFunctionKey;
            }
            else
            {
                _httpFunctionAddress = IdentityProvider.GetKeyVaultSecret(StandardConfigManager.DLS_AZURE_FUNCTION_API, _customerCode); // ConfigManager.AzureFunctionApi;
                var azureSecretname = ConfigManager.AzureFunctionSecretName;
                _httpFuncctionKey = IdentityProvider.GetKeyVaultSecret(azureSecretname, _customerCode);
            }


        }

        private RequestManager GetCustomerRequestManager(string customerCode)
        {
            if (!_requestManagersByCustomer.ContainsKey(customerCode))
            {
                var nb = new NetBridge(true);
                if (ConfigManager.ApplicationClass == ApplicationClassEnum.Daemon)
                {
                    nb.SetConnectionString(ConfigManager.UploaderConnectionString);
                }
                else
                {
                    nb.SetConnectionString(ConfigManager.GetCustomerDatabaseConnectionString(customerCode));
                }
                var rm = new RequestManager(nb);
                _requestManagersByCustomer[customerCode] = rm;
            }

            return _requestManagersByCustomer[customerCode];
        }

        public async Task<RequestMessage> PostMessage(RequestMessage message)
        {
            // https://dlsfunctionsservice.azurewebsites.net/api/HttpProcessMessage?code=xcUC21HjzGmpa5/AC/6lJ5XEmMpq/UysCaDksGL2/MGoVXdDTnLF1g==

            message.MessageFromId = this.Id;
            message.MessageFromName = this.Name;
            var rm = GetCustomerRequestManager(message.CustomerCode);

            rm.SaveRequestMessage(message);

            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            var httpFuncctionKeyEncoded = HttpUtility.UrlEncode(_httpFuncctionKey);
            var customerCodeEncoded = HttpUtility.UrlEncode(message.CustomerCode);
            var messageIdEncoded = HttpUtility.UrlEncode(message.MessageId.ToString());

            var url = string.Format("{0}?code={1}&customer={2}&message={3}", _httpFunctionAddress, httpFuncctionKeyEncoded, customerCodeEncoded, messageIdEncoded);
            ConfigManager.Log.Important("Calling url " + url);
            var request = (HttpWebRequest)WebRequest.Create(url);
            //var response = request.GetResponse(); //await request.GetResponseAsync(); //await request.GetResponseAsync();

            var response = await request.GetResponseAsync(); //await request.GetResponseAsync();

            var stream = response.GetResponseStream();
            var reader = new StreamReader(stream);
            var serialized = reader.ReadToEnd();

            var responseMessageDeserialized = RequestMessage.Deserialize(serialized);
            var respFromDb = rm.GetMessageById(responseMessageDeserialized.MessageId);


            return respFromDb;
        }

        public async Task PostMessageNoResponse(RequestMessage message)
        {
            var resp = await PostMessage(message);
            return;
        }
    }
}

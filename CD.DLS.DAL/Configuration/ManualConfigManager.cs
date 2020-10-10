using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CD.DLS.Common.Interfaces;

namespace CD.DLS.DAL.Configuration
{
    public class ManualConfigManager : IConfigManager
    {
        public DeploymentModeEnum DeploymentMode { get; set; }

        public ApplicationClassEnum ApplicationClass { get; set; }

        public Guid ServiceReceiverId { get; set; }

        public bool ServiceRunsInConsole { get; set; }

        public QueueModeEnum QueueMode { get; set; }

        public string CustomerDatabaseConnectionString { get; set; }

        public string AadInstance { get; set; }

        public string AzureTenant { get; set; }

        public string AadClientId { get; set; }

        public string AadRedirectUri { get; set; }

        public string MsGraphResourceId { get; set; }

        public string MsGraphApiVersion { get; set; }

        public string CustomerCode { get; set; }

        public string ServiceBusConnectionString { get; set; }

        public string ServiceBusServiceTopicName { get; set; }

        public string ServiceBusServiceTopicSubscription { get; set; }

        public string ServiceBusCustomerTopicName { get; set; }

        public string ServiceBusCustomerTopicSubscription { get; set; }

        public string AzureKeyVaultBaseAddress { get; set; }

        public ILogger Log { get; set; }

        public string UploaderConnectionString { get; set; }

        public string StorageAccountConnectionString { get; set; }

        public string StorageAwaitedRequestsTableName { get; set; }

        public string ExtractorPath { get; set; }

        public string ServiceLogPath { get; set; }

        public DevModeStatus DevMode { get; set; }

        private ClientClassEnum _clientClass = ClientClassEnum.General;

        public ClientClassEnum ClientClass { get { return _clientClass; } set { _clientClass = value; } }

        public int ServiceTimeout { get { return 600; } }

        public string GetAzureKeyVaultBaseAddress(string customerCode)
        {
            throw new NotImplementedException();
        }

        public string GetCustomerDatabaseConnectionString(string customerCode)
        {
            throw new NotImplementedException();
        }

        public string GetServiceBusCustomerTopicName(string customerCode)
        {
            throw new NotImplementedException();
        }

        public void SetConfigValue(string key, string value)
        {
            throw new NotImplementedException();
        }

        public string KeyVaultReaderPath
        {
            get { return null; }
        }

        public string CustomerList { get; set; }

        public string AadClientSecret { get; set; }

        public string AzureFunctionApi { get; set; }

        public string AzureFunctionSecretName { get { return "HttpFunctionKey"; } }

        public bool LogInitialized { get { return false; } }

        public string AzureFunctionKey { get; set; }
    }
}

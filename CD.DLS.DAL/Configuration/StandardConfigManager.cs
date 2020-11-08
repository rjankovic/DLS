using CD.DLS.Common.Interfaces;
using CD.DLS.Common.Tools;
using CD.DLS.DAL.Misc;
using System;
using System.Collections.Generic;
using System.Threading;

namespace CD.DLS.DAL.Configuration
{
    /// <summary>
    /// Central configuration provider - connection strings, sensitive and other values. Triggers login and AAD requests when needed.
    /// </summary>
    public class StandardConfigManager : IConfigManager
    {
        private DeploymentModeEnum _deploymentMode = DeploymentModeEnum.NA;
        private Dictionary<string, string> _ConfigValues = new Dictionary<string, string>();
        private ApplicationClassEnum _applicationClass = ApplicationClassEnum.NA;
        private Dictionary<int, ILogger> _logByThread = new Dictionary<int, ILogger>();


        private string GetEnvirionmentVariable(string name)
        {
            return Environment.GetEnvironmentVariable(name, EnvironmentVariableTarget.Process);
        }

        public const string DLS_DEPLOYMENT_MODE = "DeploymentMode";
        public const string DLS_APPLICATION_CLASS = "ApplicationClass";
        public const string KV_DLS_CUSTOMER_CONNECTION_STRING = "CustomerDatabaseConnectionString";
        public const string DLS_UPLOADER_CONNECTION_STRING = "UploaderConnectionString";
        public const string DLS_SERVICE_BUS_CONNECTION_STRING = "ServiceBusConnectionString";
        public const string DLS_STORAGE_CONNECTION_STRING = "StorageConnectionString";
        public const string DLS_AWAIT_REQUESTS_TABLE = "AwaitedRequestsTable";
        public const string DLS_SERVICE_IN_CONSOLE = "ServiceRunsInConsole";
        public const string DLS_SERVICERECEIVERID = "ServiceReceiverId";
        public const string DLS_SERVICE_TIMEOUT = "ServiceTimeout";
        public const string DLS_EXTRACTOR_PATH = "ExtractorPath";
        public const string DLS_SERVICE_LOG_PATH = "ServiceLogPath";
        public const string DLS_DEV_MODE = "DevMode";

        public const string KV_DLS_ASB_CUSTOMER_TOPIC = "ServiceBusCustomerTopicName";
        public const string KV_DLS_ASB_CUSTOMER_SUBSCRIPTION = "ServiceBusCustomerTopicSubscription";
        public const string DLS_ASB_SERVICE_TOPIC = "ServiceBusServiceTopicName";
        public const string DLS_ASB_SERVICE_SUBSCRIPTION = "ServiceBusServiceTopicSubscription";

        public const string DLS_AAD_INSTANCE = "AadInstance";
        public const string DLS_AZURE_TENANT = "AzureTenant";
        public const string DLS_AAD_CLIENTID = "AadClientId";
        public const string DLS_AAD_REDIRECT_URI = "AadRedirectUri";
        public const string DLS_GRAPH_RESOURCE_ID = "MsGraphResourceId";
        public const string DLS_GRAPH_API_VERSION = "MsGraphApiVersion";
        public const string DLS_KEY_VAULT_READER_PATH = "KeyVaultReaderPath";
        public const string DLS_CUSTOMER_LIST = "CustomerList";
        public const string DLS_AAD_CLIENT_SECRET = "AadClientSecret";
        public const string DLS_AZURE_FUNCTION_API = "AzureFunctionApi";
        public const string DLS_AZURE_FUNCTION_SECRET_NAME = "AzureFunctionSecretName";
        public const string DLS_AZURE_FUNCTION_KEY = "HttpFunctionKey";

        public const string DLS_DEFAULT_CUSTOMER = "DefaultCustomer";

        /// <summary>
        /// Is this an on-premises or cloud instance of the framework
        /// </summary>
        public DeploymentModeEnum DeploymentMode
        {
            get {
                if (_deploymentMode == DeploymentModeEnum.NA)
                {
                    var configVal = GetConfiguredValueFromRegistryOrAppConfig(DLS_DEPLOYMENT_MODE);
                    try
                    {
                        _deploymentMode = (DeploymentModeEnum)Enum.Parse(typeof(DeploymentModeEnum), configVal);
                    }
                    catch
                    {
                        throw new Exception(string.Format("Failed to parse deployment mode from config {0}: '{1}'", DLS_DEPLOYMENT_MODE, configVal));
                    }
                }
                return _deploymentMode;
                //return DeploymentModeEnum.Azure;
            }

            set { _deploymentMode = value; }
        }

        /// <summary>
        /// Is the currently running process a service, a client or else?
        /// </summary>
        public ApplicationClassEnum ApplicationClass
        {
            get
            {
                if (_applicationClass == ApplicationClassEnum.NA)
                {
                    return ApplicationClassEnum.Client;
                    /*
                    var configVal = GetConfiguredValueFromRegistryOrEnvironment(DLS_APPLICATION_CLASS);
                    _applicationClass = (ApplicationClassEnum)Enum.Parse(typeof(ApplicationClassEnum), configVal);
                    */
                }
                return _applicationClass;
            }
            set
            {
                _applicationClass = value;
            }
        }

        /// <summary>
        /// The ObjectTo to whicht to direct request messages from the clients
        /// </summary>
        public Guid ServiceReceiverId
        {
            get
            {
                return Guid.Parse(GetConfiguredValueFromRegistryOrAppConfig(DLS_SERVICERECEIVERID));
            }
        }

        /// <summary>
        /// Does the service run in a console window? (Debug mode for on-premises deployments)
        /// </summary>
        public bool ServiceRunsInConsole
        {
            get
            {
                var val = GetConfiguredValueFromRegistryOrAppConfig(DLS_SERVICE_IN_CONSOLE);
                return bool.Parse(val);
            }
        }

        /// <summary>
        /// Timeout for Azure Functions execution
        /// </summary>
        public int ServiceTimeout
        {
            get
            {
                var val = GetConfiguredValueFromRegistryOrAppConfig(DLS_SERVICE_TIMEOUT);
                return int.Parse(val);
            }
        }

        /// <summary>
        /// Are message queues provided by SQL Service Broker or Azure Service Bus Topics?
        /// </summary>
        public QueueModeEnum QueueMode
        {
            get { return DeploymentMode == DeploymentModeEnum.Azure ? QueueModeEnum.AzureTopic : QueueModeEnum.ServiceBroker; }
        }

        /// <summary>
        /// The connection to the currently logged in user's organization database (Azure or on-premises)
        /// </summary>
        public string CustomerDatabaseConnectionString
        {
            get {
                if (ApplicationClass == ApplicationClassEnum.Service && DeploymentMode == DeploymentModeEnum.Azure)
                {
                    throw new InvalidOperationException("A service should not use this property without specifying the customer code, as it serves all the customers simultaneously");
                }
                return GetSensitiveCustomerValue(KV_DLS_CUSTOMER_CONNECTION_STRING);
            }
        }

        /// <summary>
        /// Database connection string for a specific customer
        /// </summary>
        /// <param name="customerCode"></param>
        /// <returns></returns>
        public string GetCustomerDatabaseConnectionString(string customerCode)
        {
            return GetSensitiveCustomerValue(KV_DLS_CUSTOMER_CONNECTION_STRING, customerCode);
        }

        /// <summary>
        /// Http root for login requests
        /// </summary>
        public string AadInstance
        {
            get { return GetConfiguredValueFromRegistryOrAppConfig(DLS_AAD_INSTANCE); }
            //get { return "https://login.microsoftonline.com/{0}" /*GetSensitiveCustomerValue(DLS_AAD_INSTANCE)*/; }
        }

        /// <summary>
        /// Azure tennant ID in which the service runs
        /// </summary>
        public string AzureTenant
        {
            //get { return "lukasmatejovskycleverdecisi.onmicrosoft.com"; }
            get { return GetConfiguredValueFromRegistryOrAppConfig(DLS_AZURE_TENANT); }
        }

        /// <summary>
        /// client ID of the AAD-registered client application
        /// </summary>
        public virtual string AadClientId
        {
            get { return GetSensitiveValueFromRegistryOrAppConfig(DLS_AAD_CLIENTID); }
        }

        /// <summary>
        /// Redirect URI of the AAD-registered client application
        /// </summary>
        public string AadRedirectUri
        {
            //get { return "9RLBIRBujkzMW2FpjWaqiZ0qwqAB3mD7SqueAzWZnDG6TRpLmknnEZAkqWUFO3CIh50Weafd8hiFjExrWH6JpPDXE1Tt8oH+wOOOovPJgWoiMLFP5yiS6bhulypB8g2tGmMm4ljSwmyjlLVEOjaa8FTUVvprJK3ML/msT98TCi8=";  }
            get { return GetConfiguredValueFromRegistryOrAppConfig(DLS_AAD_REDIRECT_URI); }
        }

        /// <summary>
        /// Microsoft Graph base URI (for AAD queries)
        /// </summary>
        public string MsGraphResourceId
        {
            //get { return "https://graph.microsoft.com"; }
            get { return GetConfiguredValueFromRegistryOrAppConfig(DLS_GRAPH_RESOURCE_ID); }
        }

        /// <summary>
        /// MS Graph API Version
        /// </summary>
        public string MsGraphApiVersion
        {
            get { return GetConfiguredValueFromRegistryOrAppConfig(DLS_GRAPH_API_VERSION); }
        }

        //public  string MsGraphApiEndpoint
        //{
        //    get { return GetConfiguredValueFromRegistryOrEnvironment("MsGraphApiEndpoint"); }
        //}

        /// <summary>
        /// The current customer's identifier
        /// </summary>
        public virtual string CustomerCode
        {
            get
            {
                return Identity.IdentityProvider.GetCurrentUser().GetCustomerCode();
            }
        }

        /// <summary>
        /// Azure Service Bus connection
        /// </summary>
        public string ServiceBusConnectionString
        {
            get { return GetConfiguredValueFromRegistryOrAppConfig(DLS_SERVICE_BUS_CONNECTION_STRING); }
        }

        /// <summary>
        /// The service Bus Topic to whicth to send client requests
        /// </summary>
        public string ServiceBusServiceTopicName
        {
            get { return GetConfiguredValueFromRegistryOrAppConfig(DLS_ASB_SERVICE_TOPIC); }
        }

        /// <summary>
        /// The service's subscription name for it's incoming messages
        /// </summary>
        public string ServiceBusServiceTopicSubscription
        {
            get { return GetConfiguredValueFromRegistryOrAppConfig(DLS_ASB_SERVICE_SUBSCRIPTION); }
        }

        /// <summary>
        /// The Azure Service Bus topic for service responses and announcements towards the clients
        /// </summary>
        public string ServiceBusCustomerTopicName
        {
            get { return GetSensitiveCustomerValue(KV_DLS_ASB_CUSTOMER_TOPIC); }
        }

        public string GetServiceBusCustomerTopicName(string customerCode)
        {
            return GetSensitiveCustomerValue(KV_DLS_ASB_CUSTOMER_TOPIC, customerCode);
        }

        /// <summary>
        /// The subscription name for service responses
        /// </summary>
        public string ServiceBusCustomerTopicSubscription
        {
            get { return GetSensitiveCustomerValue(KV_DLS_ASB_CUSTOMER_SUBSCRIPTION); }
        }

        /// <summary>
        /// "cust1,cust2,cust3" available only to the service
        /// </summary>
        public string CustomerList
        {
            get { return GetConfiguredValueFromRegistryOrAppConfig(DLS_CUSTOMER_LIST); }
        }

        /// <summary>
        /// The Key Vault address of the currently logged in user's organization
        /// </summary>
        public string AzureKeyVaultBaseAddress
        {
            get {
                if (ApplicationClass == ApplicationClassEnum.Service && DeploymentMode == DeploymentModeEnum.Azure)
                {
                    throw new InvalidOperationException("A service should not use this property without specifying the customer code, as it serves all the customers simultaneously");
                }
                return GetAzureKeyVaultBaseAddress(CustomerCode);
            }
        }

        /// <summary>
        /// The Key Vault address of a given customer
        /// </summary>
        /// <param name="customerCode"></param>
        /// <returns></returns>
        public string GetAzureKeyVaultBaseAddress(string customerCode)
        {
            return $"https://{customerCode}dlscustkv.vault.azure.net";
        }

        /// <summary>
        /// All-purpose log
        /// </summary>
        public ILogger Log
        {
            get {
                try
                {
                    var threadId = Thread.CurrentThread.ManagedThreadId;

                    if (!_logByThread.ContainsKey(threadId))
                    {
                        if (ApplicationClass == ApplicationClassEnum.Service && DeploymentMode == DeploymentModeEnum.Azure)
                        {
                            _logByThread[threadId] = new EventLogger();
                        }
                        else if (ApplicationClass == ApplicationClassEnum.Service)
                        {
                            _logByThread[threadId] = new FileLogger("DLS Service", ServiceLogPath);
                        }
                        else
                        {
                            _logByThread[threadId] = new DbLogger();
                        }
                    }
                    return _logByThread[threadId];
                }
                catch
                {
                    return new ConsoleLogger("Backup Log");
                }
            }
            set
            {
                var threadId = Thread.CurrentThread.ManagedThreadId;
                _logByThread[threadId] = value;
            }
        }

        public string UploaderConnectionString
        {
            get { return GetConfiguredValueFromRegistryOrAppConfig(DLS_UPLOADER_CONNECTION_STRING); }
        }

        /// <summary>
        /// This is not sensitive - it is only used by the service internally for request completion notification
        /// </summary>
        public string StorageAccountConnectionString
        {
            get { return GetConfiguredValueFromRegistryOrAppConfig(DLS_STORAGE_CONNECTION_STRING); }
        }

        public string StorageAwaitedRequestsTableName
        {
            get { return GetConfiguredValueFromRegistryOrAppConfig(DLS_AWAIT_REQUESTS_TABLE); }
        }

        public string ExtractorPath
        {
            get { return GetConfiguredValueFromRegistryOrAppConfig(DLS_EXTRACTOR_PATH); }
        }


        public string ServiceLogPath
        {
            get { return GetConfiguredValueFromRegistryOrAppConfig(DLS_SERVICE_LOG_PATH); }
        }

        public DevModeStatus DevMode
        {
            get {
                var configVal = GetConfiguredValueFromRegistryOrAppConfig(DLS_DEV_MODE);
                if (configVal == null)
                {
                    return DevModeStatus.Production;
                }
                return (DevModeStatus)(Enum.Parse(typeof(DevModeStatus), configVal));
            }
        }

        private ClientClassEnum _clientClass = ClientClassEnum.General;

        /// <summary>
        /// General client or Excel?
        /// </summary>
        public ClientClassEnum ClientClass { get { return _clientClass; } set { _clientClass = value; } }

        public string KeyVaultReaderPath
        {
            get {
                var val = GetConfiguredValueFromRegistryOrAppConfig(DLS_KEY_VAULT_READER_PATH);
                return val;
            }
        }

        public string AadClientSecret
        {
            get { return GetConfiguredValueFromRegistryOrAppConfig(DLS_AAD_CLIENT_SECRET); }
        }

        public string AzureFunctionApi
        {
            get
            {
                if (ApplicationClass == ApplicationClassEnum.Daemon)
                {
                    return GetConfiguredValueFromRegistryOrAppConfig(DLS_AZURE_FUNCTION_API);
                }
                else
                {
                    return GetSensitiveCustomerValue(DLS_AZURE_FUNCTION_API);
                }
            }
        }

        public string AzureFunctionSecretName
        {
            get
            {
                return "HttpFunctionKey";
            }
        }
        
        protected virtual string GetConfiguredValueFromRegistryOrAppConfig(string key)
        {
            if (_ConfigValues.ContainsKey(key))
            {
                return _ConfigValues[key];
            }

            string configValue;
            if (ApplicationClass == ApplicationClassEnum.Client || ApplicationClass == ApplicationClassEnum.Daemon)
            {
                configValue = Registry.GetConfigValue(key);
            }
            else if (DeploymentMode == DeploymentModeEnum.OnPremises && ApplicationClass == ApplicationClassEnum.Service)
            {
                configValue = Registry.GetConfigValue(key);
            }
            else if (ApplicationClass == ApplicationClassEnum.Service)
            {
                configValue = System.Configuration.ConfigurationManager.AppSettings[key];
            }
            else
            {
                throw new Exception($"The configuration value for {key} was not found");
            }

            _ConfigValues[key] = configValue;
            return configValue;

            
            /*
            var registryRes = Registry.GetConfigValue(key);
            if (registryRes != null)
            {
                _ConfigValues[key] = registryRes;
                return registryRes;
            }
            else
            {
                var enviroRes = Environment.GetEnvironmentVariable(key);
                _ConfigValues[key] = enviroRes;
                return enviroRes;
            }
            */

        }

        private  string GetSensitiveValueFromRegistryOrAppConfig(string key)
        {
            var encryptedValue = GetConfiguredValueFromRegistryOrAppConfig(key);
            return StringCipher.Decrypt(encryptedValue);
        }

        private string GetSensitiveCustomerValue(string key, string customerCode = null)
        {
            int tries = 3;
            while (tries > 0)
            {
                try
                {
                    string configValue = null;

                    switch (DeploymentMode)
                    {
                        case DeploymentModeEnum.Azure:
                            if (customerCode == null)
                            {
                                configValue = Identity.IdentityProvider.GetKeyVaultSecret(key);
                                return configValue;
                                //return task1.WaitAndUnwrapException();
                            }
                            else
                            {
                                configValue = Identity.IdentityProvider.GetKeyVaultSecret(key, customerCode);
                                return configValue;
                                //return task2.WaitAndUnwrapException();
                            }
                        case DeploymentModeEnum.OnPremises:
                            configValue = Registry.GetConfigValue(key);
                            return configValue;
                        default:
                            throw new NotImplementedException();
                    }
                }
                catch (Exception ex)
                {
                    tries--;
                    if (tries > 0)
                    {
                        continue;
                    }
                    else
                    {
                        throw new Exception(string.Format("Failed to retrieve secret {0} for {1}: {2}; {3}{4}", key, customerCode, ex.Message, Environment.NewLine, ex.StackTrace));
                    }
                }
            }
            throw new Exception(string.Format("Failed to retrieve secret {0} for {1}", key, customerCode));
        }

        public void SetConfigValue(string key, string value)
        {
            _ConfigValues[key] = value;
        }

        public bool LogInitialized { get { return _logByThread.ContainsKey(Thread.CurrentThread.ManagedThreadId); } }

        public string AzureFunctionKey
        {
            get { return GetConfiguredValueFromRegistryOrAppConfig(DLS_AZURE_FUNCTION_KEY);}
        }
    }
}

using CD.DLS.Common.Interfaces;
using CD.DLS.Common.Tools;
using CD.DLS.DAL.Misc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CD.DLS.DAL.Configuration
{
    public enum DeploymentModeEnum { OnPremises, Azure, NA }
    public enum QueueModeEnum { ServiceBroker, AzureTopic }
    public enum ApplicationClassEnum { Service, Client, WebClient, Daemon, NA }
    public enum ClientClassEnum { General, Excel }
    public enum DevModeStatus { Debug, Production }

    /// <summary>
    /// A wrapper for StandardConfigurationManager, other implementations can be inserted
    /// </summary>
    public static class ConfigManager
    {
        private static IConfigManager _innerConfigManager = new StandardConfigManager();
        
        public static void SetCustomConfigManager(IConfigManager manager)
        {
            _innerConfigManager = manager;
        }

        public static void UseDefaultConfigManager()
        {
            _innerConfigManager = new StandardConfigManager();
        }


        
        /// <summary>
        /// Is this an on-premises or cloud instance of the framework
        /// </summary>
        public static DeploymentModeEnum DeploymentMode
        {
            get {
                return _innerConfigManager.DeploymentMode;
            }
        }

        /// <summary>
        /// Is the currently running process a service, a client or else?
        /// </summary>
        public static ApplicationClassEnum ApplicationClass
        {
            get
            {
                return _innerConfigManager.ApplicationClass;
            }
            set
            {
                _innerConfigManager.ApplicationClass = value;
            }
        }

        /// <summary>
        /// The ObjectTo to whicht to direct request messages from the clients
        /// </summary>
        public static Guid ServiceReceiverId
        {
            get
            {
                return _innerConfigManager.ServiceReceiverId;
            }
        }
        
        /// <summary>
        /// Does the service run in a console window? (Debug mode for on-premises deployments)
        /// </summary>
        public static bool ServiceRunsInConsole
        {
            get
            {
                return _innerConfigManager.ServiceRunsInConsole;
            }
        }

        /// <summary>
        /// Are message queues provided by SQL Service Broker or Azure Service Bus Topics?
        /// </summary>
        public static QueueModeEnum QueueMode
        {
            get { return _innerConfigManager.QueueMode; }
        }
        
        /// <summary>
        /// The connection to the currently logged in user's organization database (Azure or on-premises)
        /// </summary>
        public static string CustomerDatabaseConnectionString
        {
            get {
                return _innerConfigManager.CustomerDatabaseConnectionString;
            }
        }

        /// <summary>
        /// Database connection string for a specific customer
        /// </summary>
        /// <param name="customerCode"></param>
        /// <returns></returns>
        public static string GetCustomerDatabaseConnectionString(string customerCode)
        {
            return _innerConfigManager.GetCustomerDatabaseConnectionString(customerCode);
        }
        
        /// <summary>
        /// Http root for login requests
        /// </summary>
        public static string AadInstance
        {
            get { return _innerConfigManager.AadInstance; }
        }

        /// <summary>
        /// Azure tennant ID in which the service runs
        /// </summary>
        public static string AzureTenant
        {
            get { return _innerConfigManager.AzureTenant; }
        }

        /// <summary>
        /// client ID of the AAD-registered client application
        /// </summary>
        public static string AadClientId
        {
            get { return _innerConfigManager.AadClientId; }
        }

        /// <summary>
        /// client secret of the AAD-registered client application
        /// </summary>
        public static string AadClientSecret
        {
            get { return _innerConfigManager.AadClientSecret; }
        }

        /// <summary>
        /// Redirect URI of the AAD-registered client application
        /// </summary>
        public static string AadRedirectUri
        {
            get { return _innerConfigManager.AadRedirectUri; }
        }
        
        /// <summary>
        /// Microsoft Graph base URI (for AAD queries)
        /// </summary>
        public static string MsGraphResourceId
        {
            get { return _innerConfigManager.MsGraphResourceId; }
        }

        /// <summary>
        /// MS Graph API Version
        /// </summary>
        public static string MsGraphApiVersion
        {
            get { return _innerConfigManager.MsGraphApiVersion; }
        }
        

            /// <summary>
            /// The current customer's identifier
            /// </summary>
        public static string CustomerCode
        {
            get
            {
                return _innerConfigManager.CustomerCode;
            }
        }

        /// <summary>
        /// Azure Service Bus connection
        /// </summary>
        public static string ServiceBusConnectionString
        {
            get { return _innerConfigManager.ServiceBusConnectionString; }
        }

        /// <summary>
        /// The service Bus Topic to whicth to send client requests
        /// </summary>
        public static string ServiceBusServiceTopicName
        {
            get { return _innerConfigManager.ServiceBusServiceTopicName; }
        }

        /// <summary>
        /// The service's subscription name for it's incoming messages
        /// </summary>
        public static string ServiceBusServiceTopicSubscription
        {
            get { return _innerConfigManager.ServiceBusServiceTopicSubscription; }
        }

        /// <summary>
        /// The Azure Service Bus topic for service responses and announcements towards the clients
        /// </summary>
        public static string ServiceBusCustomerTopicName
        {
            get { return _innerConfigManager.ServiceBusCustomerTopicName; }
        }

        /// <summary>
        /// The Azure Service Bus topic for service responses and announcements towards the clients of a specified customer
        /// </summary>
        /// <param name="customerCode"></param>
        public static string GetServiceBusCustomerTopicName(string customerCode)
        {
            return _innerConfigManager.GetServiceBusCustomerTopicName(customerCode);
        }

        /// <summary>
        /// The subscription name for service responses
        /// </summary>
        public static string ServiceBusCustomerTopicSubscription
        {
            get { return _innerConfigManager.ServiceBusCustomerTopicSubscription; }
        }

        /// <summary>
        /// The Key Vault address of the currently logged in user's organization
        /// </summary>
        public static string AzureKeyVaultBaseAddress
        {
            get {
                return _innerConfigManager.AzureKeyVaultBaseAddress;
            }
        }

        /// <summary>
        /// The Key Vault address of a given customer
        /// </summary>
        /// <param name="customerCode"></param>
        /// <returns></returns>
        public static string GetAzureKeyVaultBaseAddress(string customerCode)
        {
            return _innerConfigManager.GetAzureKeyVaultBaseAddress(customerCode);
        }

        /// <summary>
        /// Restricted database access for the uploader
        /// </summary>
        /// <returns></returns>
        public static string UploaderConnectionString
        {
            get { return _innerConfigManager.UploaderConnectionString; }
        }


        /// <summary>
        /// Connection string to the service's central Azure Storage account
        /// </summary>
        public static string StorageAccountConnectionString
        {
            get { return _innerConfigManager.StorageAccountConnectionString; }
        }


        /// <summary>
        /// An Azure table containing the requests finalizing in a DB whose notifications need to be sent to the Azure topic upon finishing
        /// </summary>
        public static string StorageAwaitedRequestsTableName
        {
            get { return _innerConfigManager.StorageAwaitedRequestsTableName; }
        }

        /// <summary>
        /// Path to the extractor executable
        /// </summary>
        public static string ExtractorPath
        {
            get { return _innerConfigManager.ExtractorPath; }
        }

        /// <summary>
        /// Path to the service's log file
        /// </summary>
        public static string ServiceLogPath
        {
            get { return _innerConfigManager.ServiceLogPath; }
        }

        /// <summary>
        /// Debug / production mode indicator
        /// </summary>
        public static DevModeStatus DevMode
        {
            get { return _innerConfigManager.DevMode; }
        }

        /// <summary>
        /// All-purpose log
        /// </summary>
        public static ILogger Log
        {
            get { return _innerConfigManager.Log; }
            set { _innerConfigManager.Log = value;  }
        }


        public static bool LogInitialized
        {
            get { return _innerConfigManager.LogInitialized; }
        }

        /// <summary>
        /// Timeout for service execution in seconds
        /// </summary>
        public static int ServiceTimeout
        {
            get { return _innerConfigManager.ServiceTimeout; }
        }

        public static void SetConfigValue(string key, string value)
        {
            _innerConfigManager.SetConfigValue(key, value);
        }

        public static string KeyVaultReaderPath
        {
            get { return _innerConfigManager.KeyVaultReaderPath; }
        }

        /// <summary>
        /// "cust1,cust2,cust3" available only to the service
        /// </summary>
        public static string CustomerList
        {
            get { return _innerConfigManager.CustomerList; }
        }

        /// <summary>
        /// Excel or standard client?
        /// </summary>
        public static ClientClassEnum ClientClass
        {
            get { return _innerConfigManager.ClientClass; }
            set { _innerConfigManager.ClientClass = value; }
        }

        /// <summary>
        /// URL of the Azure function HTTP request processor
        /// </summary>
        public static string AzureFunctionApi { get { return _innerConfigManager.AzureFunctionApi; } }

        /// <summary>
        /// KV secret used as the application key for the Azure HTTP function
        /// </summary>
        public static string AzureFunctionSecretName { get { return _innerConfigManager.AzureFunctionSecretName; } }

        public static string AzureFunctionKey
        {
            get { return _innerConfigManager.AzureFunctionKey; }
        }

    }
}

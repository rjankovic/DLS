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
    public interface IConfigManager
    {
        /// <summary>
        /// Is this an on-premises or cloud instance of the framework
        /// </summary>
        DeploymentModeEnum DeploymentMode { get; }

        /// <summary>
        /// Is the currently running process a service, a client or else?
        /// </summary>
        ApplicationClassEnum ApplicationClass { get; set; }

        /// <summary>
        /// General client or Excel?
        /// </summary>
        ClientClassEnum ClientClass { get; set; }

        /// <summary>
        /// The ObjectTo to whicht to direct request messages from the clients
        /// </summary>
        Guid ServiceReceiverId { get; }

        /// <summary>
        /// Does the service run in a console window? (Debug mode for on-premises deployments)
        /// </summary>
        bool ServiceRunsInConsole { get; }

        /// <summary>
        /// Are message queues provided by SQL Service Broker or Azure Service Bus Topics?
        /// </summary>
        QueueModeEnum QueueMode { get; }

        /// <summary>
        /// The connection to the currently logged in user's organization database (Azure or on-premises)
        /// </summary>
        string CustomerDatabaseConnectionString { get; }

        /// <summary>
        /// Database connection string for a specific customer
        /// </summary>
        /// <param name="customerCode"></param>
        /// <returns></returns>
        string GetCustomerDatabaseConnectionString(string customerCode);

        /// <summary>
        /// Restricted database access for the uploader
        /// </summary>
        /// <returns></returns>
        string UploaderConnectionString { get; }

        /// <summary>
        /// Http root for login requests
        /// </summary>
        string AadInstance { get; }

        /// <summary>
        /// Azure tennant ID in which the service runs
        /// </summary>
        string AzureTenant { get; }

        /// <summary>
        /// client ID of the AAD-registered client application
        /// </summary>
        string AadClientId { get; }

        /// <summary>
        /// client secret of the AAD-registered client application
        /// </summary>
        string AadClientSecret { get; }
        
        /// <summary>
        /// Redirect URI of the AAD-registered client application
        /// </summary>
        string AadRedirectUri { get; }
        
        /// <summary>
        /// Microsoft Graph base URI (for AAD queries)
        /// </summary>
        string MsGraphResourceId { get; }

        /// <summary>
        /// MS Graph API Version
        /// </summary>
        string MsGraphApiVersion { get; }

        /// <summary>
        /// The current customer's identifier
        /// </summary>
        string CustomerCode { get; }

        /// <summary>
        /// Azure Service Bus connection
        /// </summary>
        string ServiceBusConnectionString { get; }

        /// <summary>
        /// The service Bus Topic to whicth to send client requests
        /// </summary>
        string ServiceBusServiceTopicName { get; }

        /// <summary>
        /// The service's subscription name for it's incoming messages
        /// </summary>
        string ServiceBusServiceTopicSubscription { get; }

        /// <summary>
        /// The Azure Service Bus topic for service responses and announcements towards the clients
        /// </summary>
        string ServiceBusCustomerTopicName { get; }

        /// <summary>
        /// The Azure Service Bus topic for service responses and announcements towards the clients of a specified customer
        /// </summary>
        /// <param name="customerCode"></param>
        string GetServiceBusCustomerTopicName(string customerCode);

        /// <summary>
        /// The subscription name for service responses
        /// </summary>
        string ServiceBusCustomerTopicSubscription { get; }

        /// <summary>
        /// Connection string to the service's central Azure Storage account
        /// </summary>
        string StorageAccountConnectionString { get; }

        /// <summary>
        /// An Azure table containing the requests finalizing in a DB whose notifications need to be sent to the Azure topic upon finishing
        /// </summary>
        string StorageAwaitedRequestsTableName { get; }

        /// <summary>
        /// The Key Vault address of the currently logged in user's organization
        /// </summary>
        string AzureKeyVaultBaseAddress { get; }

        /// <summary>
        /// The Key Vault address of a given customer
        /// </summary>
        /// <param name="customerCode"></param>
        /// <returns></returns>
        string GetAzureKeyVaultBaseAddress(string customerCode);
        
        /// <summary>
        /// Path to the extractor executable
        /// </summary>
        string ExtractorPath { get; }

        /// <summary>
        /// Path to the service's log file
        /// </summary>
        string ServiceLogPath { get; }

        /// <summary>
        /// Debug / production mode indicator
        /// </summary>
        DevModeStatus DevMode { get; }

        /// <summary>
        /// All-purpose log
        /// </summary>
        ILogger Log { get; set; }

        /// <summary>
        /// Timeout for service execution in seconds
        /// </summary>
        int ServiceTimeout { get; }

        /// <summary>
        /// List of customers, available only to the service "cust1,cust2,cust3"
        /// </summary>
        string CustomerList { get; }

        void SetConfigValue(string key, string value);

        string KeyVaultReaderPath { get; }

        /// <summary>
        /// URL of the Azure function HTTP request processor
        /// </summary>
        string AzureFunctionApi { get; }

        /// <summary>
        /// KV secret used as the application key for the Azure HTTP function
        /// </summary>
        string AzureFunctionSecretName { get; }

        bool LogInitialized { get; }

        /// <summary>
        /// The Azure HTTP function key
        /// </summary>
        string AzureFunctionKey { get; }

    }
}

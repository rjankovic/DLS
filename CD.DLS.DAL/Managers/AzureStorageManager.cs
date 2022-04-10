using CD.DLS.DAL.Configuration;
using Microsoft.Azure.CosmosDB.Table;
using Microsoft.Azure.Storage;
using System;
using System.Collections.Generic;

namespace CD.DLS.DAL.Mamangers
{

    public class AwaitedRequestsTableItem : TableEntity
    {
        public AwaitedRequestsTableItem(string custoemrCode, Guid requestId)
        {
            this.PartitionKey = CustomerCode;
            this.RowKey = requestId.ToString().ToLower();
        }

        public AwaitedRequestsTableItem()
        {

        }

        private string _customerCode;
        private Guid _requestId;

        public string CustomerCode
        {
            get {
                return _customerCode;
            }
            set {
                _customerCode = value;
                PartitionKey = _customerCode;
            }
        }
        public Guid RequestId
        {
            get
            {
                return _requestId;
            }
            set
            {
                _requestId = value;
                RowKey = _requestId.ToString().ToLower();
            }
        }
    }

    public class AzureStorageManager
    {
        private CloudStorageAccount _storageAccount;
        private CloudTableClient _tableClient;
        private CloudTable _awaitedRequestsTable;

        public AzureStorageManager()
        {
            // Retrieve the storage account from the connection string.
            _storageAccount = CloudStorageAccount.Parse(ConfigManager.StorageAccountConnectionString);

            // Create the table client.
            _tableClient = _storageAccount.CreateCloudTableClient();
            // Retrieve a reference to the table.
            _awaitedRequestsTable = _tableClient.GetTableReference(ConfigManager.StorageAwaitedRequestsTableName);
        }

        public IEnumerable<AwaitedRequestsTableItem> GetAwaitedRequests()
        {
            TableQuery<AwaitedRequestsTableItem> query = new TableQuery<AwaitedRequestsTableItem>();
            
            return _awaitedRequestsTable.ExecuteQuery(query);
        }

        public void InsertAwaitedRequest(AwaitedRequestsTableItem item)
        {
            // Create the TableOperation object that inserts the entity.
            TableOperation insertOperation = TableOperation.Insert(item);

            // Execute the insert operation.
            _awaitedRequestsTable.Execute(insertOperation);
        }

        public void DeleteAwaitedRequest(AwaitedRequestsTableItem item)
        {

            // Create a retrieve operation that expects an entity.
            TableOperation retrieveOperation = TableOperation.Retrieve<AwaitedRequestsTableItem>(item.CustomerCode, item.RequestId.ToString().ToLower());

            // Execute the operation.
            TableResult retrievedResult = _awaitedRequestsTable.Execute(retrieveOperation);

            // Assign the result to an entity
            AwaitedRequestsTableItem deleteEntity = (AwaitedRequestsTableItem)retrievedResult.Result;

            // Create the Delete TableOperation.
            if (deleteEntity != null)
            {
                TableOperation deleteOperation = TableOperation.Delete(deleteEntity);

                // Execute the operation.
                _awaitedRequestsTable.Execute(deleteOperation);
            }
        }




    }
}

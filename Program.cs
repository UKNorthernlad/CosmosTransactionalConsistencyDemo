// https://learn.microsoft.com/en-us/azure/cosmos-db/nosql/database-transactions-optimistic-concurrency#optimistic-concurrency-control

// dotnet add package Newtonsoft.Json
// dotnet add package Microsoft.Azure.Cosmos

// # Create a resource group
// az group create --name demo --location swedencentral

// # Create a CosmosDB account with standard (not serverless) pricing
// az cosmosdb create --name democosmosdb99 --resource-group demo --locations regionName=swedencentral --default-consistency-level Strong --kind GlobalDocumentDB

// # Create a database in the CosmosDB account
// az cosmosdb sql database create --account-name democosmosdb99 --resource-group demo --name test

// # Create a container in the database
// az cosmosdb sql container create --account-name democosmosdb99 --resource-group demo --database-name test --name mydata --partition-key-path /accountid

using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;

namespace CosmosDBUpdateTest
{
    internal class Program
    {
        private static String accountName = "democosmosdb99";
        private static String databaseName = "test";
        private static String containerName = "mydata";
        private static String partitionKeyName = "/accountid";
        private static int transactionItemsCount = 2;

        private static readonly string EndpointUri = "https://democosmosdb99.documents.azure.com:443/";
        private static readonly string PrimaryKey = "";

        private static CosmosClient cosmosClient;
        private static Database database;
        private static Container container;

        static async Task Main(string[] args)
        {
            cosmosClient = new CosmosClient(EndpointUri, PrimaryKey);
            await CreateDatabaseAsync();
            await CreateContainerAsync();

            while (true)
            {
                Console.WriteLine("");
                Console.WriteLine("1. Create database, container and inital item");
                Console.WriteLine("2. Async Create Batch 1");
                Console.WriteLine("3. Exit");
                Console.Write("Please select an option: ");

                string choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":

                        await AddBalanceToContainerAsync();
                        break;
                    case "2":
                        await AddTransactionsToContainerAsync();
                        break;
                    case "3":
                        Console.WriteLine("Exiting...");
                        return;
                    default:
                        Console.WriteLine("Invalid choice. Try again.");
                        break;
                }
            }
        }

        private static async Task CreateDatabaseAsync()
        {
            database = await cosmosClient.CreateDatabaseIfNotExistsAsync(databaseName);
            Console.WriteLine("Created Database: {0}\n", database.Id);
        }

        private static async Task CreateContainerAsync()
        {
            container = await database.CreateContainerIfNotExistsAsync(containerName, partitionKeyName);
            Console.WriteLine("Created Container: {0}\n", container.Id);
        }

        private static async Task AddBalanceToContainerAsync()
        {
            BalanceItem item = new BalanceItem { id = "111111", accountid= "111111", documenttype = "balance", balance = 100 };
            ItemResponse<BalanceItem> response = await container.CreateItemAsync(item, new PartitionKey("111111"));
            Console.WriteLine("Created Item: {0}\n", response.Resource.id);
        }

        private static async Task AddTransactionsToContainerAsync()
        {
            int total;
            // Get current balance
            ItemResponse<BalanceItem> balanceResponse = await container.ReadItemAsync<BalanceItem>("111111", new PartitionKey("111111"));
            BalanceItem item = balanceResponse.Resource;

            Console.WriteLine("Current balance: " + item.balance);
            total = item.balance;

            TransactionalBatch batch = container.CreateTransactionalBatch(new PartitionKey("111111"));

            for (int i = 0; i < transactionItemsCount; i++)
            {
                String guid = Guid.NewGuid().ToString();
                batch.CreateItem(new TransactionItem { id = guid, accountid = "111111", documenttype = "transaction", transaction = 100 });
                total = total + 100;
            }

            item.balance = total;

            Console.WriteLine("Adding new transaction items and updating balance to: " + total);
            batch.UpsertItem(item,new TransactionalBatchItemRequestOptions() { IfMatchEtag = balanceResponse.ETag});

            Thread.Sleep(15000);

            TransactionalBatchResponse response = await batch.ExecuteAsync();

            if (response.IsSuccessStatusCode)
            {
                
                Console.WriteLine("Update successful");
            }
            else
            {
                Console.WriteLine("Error: " + (int)response.StatusCode + " " + response.StatusCode);
            }

            //
        }
    }
}

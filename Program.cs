using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;

namespace CosmosDbDemo {
    public class Program {

        /// The Azure Cosmos DB endpoint for running this GetStarted sample.
        private string EndpointUrl = "https://gkcosmosdbpj.documents.azure.com:443/";

        /// The primary key for the Azure DocumentDB account.
        private string PrimaryKey = "uxxSGMYogGo8SdF7aDOrBmHQtWfaTaEEOmlOm9795QAXXJpCOrDQiOEYJMIPeRMQafNRxcmK8OK48BzXadgLyw==";

        // The Cosmos client instance
        private CosmosClient cosmosClient;

        // The database we will create
        private Database database;

        // The container we will create.
        private Container container;

        // The name of the database and container we will create
        private string databaseId = "FamilyDatabase";
        private string containerId = "FamilyContainer";

         static void Main (string[] args) {
            try {
                Console.WriteLine ("Beginning operations...\n");
                Program p = new Program ();
                 Task task= p.GetStartedDemoAsync ();
                 task.Wait();

            } catch (CosmosException de) {
                Exception baseException = de.GetBaseException ();
                Console.WriteLine ("{0} error occurred: {1}", de.StatusCode, de);
            } catch (Exception e) {
                Console.WriteLine ("Error: {0}", e);
            } finally {
                Console.WriteLine ("End of demo, press any key to exit.");
                Console.ReadKey ();
            }
        }
        public async Task GetStartedDemoAsync () {
            // Create a new instance of the Cosmos Client
            this.cosmosClient = new CosmosClient (EndpointUrl, PrimaryKey);
            await this.CreateDatabaseAsync ();
            await this.CreateContainerAsync ();
            //await this.AddItemsToContainerAsync ();
            await this.QueryItemsAsync ();
        }
         private async Task CreateDatabaseAsync () {
            // Create a new database
            this.database = await this.cosmosClient.CreateDatabaseIfNotExistsAsync (databaseId);
            Console.WriteLine ("Created Database: {0}\n", this.database.Id);
        }

         private async Task CreateContainerAsync () {
            // Create a new container
            this.container = await this.database.CreateContainerIfNotExistsAsync (containerId, "/LastName");
            Console.WriteLine ("Created Container: {0}\n", this.container.Id);
        }

          private async Task AddItemsToContainerAsync () {
            // Create a family object for the Andersen family
            Family andersenFamily = new Family {
                Id = "Andersen.1",
                LastName = "Andersen",
                Parents = new Parent[] {
                new Parent { FirstName = "Thomas" },
                new Parent { FirstName = "Mary Kay" }
                },
                Children = new Child[] {
                new Child {
                FirstName = "Henriette Thaulow",
                Gender = "female",
                Grade = 5,
                Pets = new Pet[] {
                new Pet { GivenName = "Fluffy" }
                }
                }
                },
                Address = new Address { State = "WA", County = "King", City = "Seattle" },
                IsRegistered = false
            };

            try {
                // Create an item in the container representing the Andersen family. Note we provide the value of the partition key for this item, which is "Andersen".
                ItemResponse<Family> andersenFamilyResponse = await this.container.CreateItemAsync<Family> (andersenFamily, new PartitionKey (andersenFamily.LastName));
                // Note that after creating the item, we can access the body of the item with the Resource property of the ItemResponse. We can also access the RequestCharge property to see the amount of RUs consumed on this request.
                Console.WriteLine ("Created item in database with id: {0} Operation consumed {1} RUs.\n", andersenFamilyResponse.Resource.Id, andersenFamilyResponse.RequestCharge);
            } catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.Conflict) {
                Console.WriteLine ("Item in database with id: {0} already exists\n", andersenFamily.Id);
            }
        }   

         private async Task QueryItemsAsync () {
            var sqlQueryText = "SELECT * FROM c WHERE c.LastName = 'Andersen'";

            Console.WriteLine ("Running query: {0}\n", sqlQueryText);

            var queryDefinition = new QueryDefinition (sqlQueryText);
            var queryResultSetIterator = this.container.GetItemQueryIterator<Family> (queryDefinition);

            var families = new List<Family> ();

            while (queryResultSetIterator.HasMoreResults) {
                FeedResponse<Family> currentResultSet = await queryResultSetIterator.ReadNextAsync ();
                foreach (Family family in currentResultSet) {
                    families.Add (family);
                    Console.WriteLine ("\tRead {0}\n", family);
                }
            }
        }     

    }
}

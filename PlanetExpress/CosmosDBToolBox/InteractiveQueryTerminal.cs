using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using Microsoft.Azure.Management.CosmosDB.Fluent;
using Microsoft.WindowsAzure.Storage.Table;
using MongoDB.Driver;
using PlanetExpress.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlanetExpress
{
    class InteractiveQueryTerminal
    {

        /// <summary>
        /// CURRENTLY UNDER DEVELOPMENT...
        /// </summary>

        string shellType;

        public InteractiveQueryTerminal(ICosmosDBAccount account = null, DocumentClient documentClient = null, CloudTableClient tableClient = null, MongoClient mongoClient = null) {
            this.shellType = account.Tags["defaultExpirience"] ?? "Global DocumentDB";
        }

        //static void Main(String[] args) {
          //  Environment.SetEnvironmentVariable("AZURE_AUTH_LOCATION", @"C:\Users\phbennet\Desktop\azureauth.properties");
           // AccountUtils au = new AccountUtils(@"C:\Users\phbennet\Desktop\azureauth.properties");
            /*string pathToMongo = @"C:\Program Files\MongoDB\Server\3.4\bin\mongo.exe";
            string connection = @"mongodb://phb-east-mongo-test:jhQwwBYQwCyTt3IJIV7jltm6C1CGsCPs82M8uDdvx1nTjQHNtxRr0Bi1Xk5RYCcTxGn9f5kejrZuRnzZ9cwT8w==@phb-east-mongo-test.documents.azure.com:10255/?ssl=true&replicaSet=globaldb";
            LaunchMongoShell(pathToMongo, connection);
            Console.WriteLine("Done with mongoshell.");*/

            /*Environment.SetEnvironmentVariable("AZURE_AUTH_LOCATION", @"C:\Users\phbennet\Desktop\azureauth.properties");
            Console.WriteLine("creating account ops class...");
            AccountOperationsAsync ops = new AccountOperationsAsync(@"C:\Users\phbennet\Desktop\azureauth.properties");
            Console.WriteLine("getting database account...");
            Task<ICosmosDBAccount> accountTask = ops.SelectAccountByName("bot-test-phb");
            ICosmosDBAccount account = accountTask.Result;
            DocumentDBToolBox toolbox = new DocumentDBToolBox(account);
            DataExplorerDocument(toolbox.client, "SELECT * FROM c", "bot", "botcoll");
            MongoDBUtils toolbox = new MongoDBUtils();

            string pathToGremlin = @"C:\Program Files\apache-tinkerpop-gremlin-console-3.3.0-bin\apache-tinkerpop-gremlin-console-3.3.0\bin\gremlin.bat";
            string pathToRS = @"C:\Program Files\apache-tinkerpop-gremlin-console-3.3.0-bin\apache-tinkerpop-gremlin-console-3.3.0\conf\remote-secure.yaml";
            LaunchGremlinConsole(account, pathToGremlin, pathToRS, "graphdb", "Persons");*/
          //  DocumentDBUtils docdb = new DocumentDBUtils(au.SelectAccountByName("documentb-test-phb").Result);
            //PartitionKey key = GetPartitionKey(docdb.client, "newDB", "newColl").Result;
            //Console.WriteLine(key.ToString());
         //   DeleteByQuery(docdb.client, "newDB", "newColl");
        //    Console.ReadLine();
       // }

        /*
        ********************************** Michael's workflow
        */

        /// <summary>
        /// Deletes all documents returned by a query. This can be used to delete all documents in a collectionby leaving
        /// the query string parameter blank as the default value is set to SELECT * FROM c. 
        /// </summary>
        /// <param name="client"> DocumentClient instance that establish a valid programatic connection to the database </param>
        /// <param name="databaseId"> string that is the name of the database you'd like to delete items from </param>
        /// <param name="collectionId"> string that is the name of the collection you'd like to delete items from </param>
        /// <param name="queryString"> string that defines the DocumentDB SQL query to use as matching criteria for the delete </param>
        public static async void DeleteByQuery(DocumentClient client, string databaseId, string collectionId, string queryString = "SELECT * FROM c")
        {
            try
            {
                // create the queryable that will retrieve documents to delete, the default value for query
                // string is SELECT * FROM c which yields all documents. You can change this value to any 
                // valid query to select documents that meet a certain criteria 
                IQueryable<Document> documents = client.CreateDocumentQuery<Document>(UriFactory.CreateDocumentCollectionUri(databaseId, collectionId),
                    queryString,
                    new FeedOptions { EnableCrossPartitionQuery = true });

                // loop throug each document in the response
                foreach (Document doc in documents)
                {
                    await client.DeleteDocumentAsync(doc.SelfLink,
                        new RequestOptions
                        {
                            // change type, arg for GetPropertyValue<>() to the type of your partition key and the 
                            // attribute name of your partition key
                            PartitionKey = new PartitionKey(doc.GetPropertyValue<string>("page"))
                        });
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("An error occured while trying to delete one or more document(s)...");
                Console.WriteLine(" --- ERROR MESSAGE ---");
                Console.WriteLine(e.Message);
                Console.WriteLine(" --- STACKTRACE ---");
                Console.WriteLine(e.StackTrace);
            }
        }

        public static async void DataExplorerDocument(DocumentClient client, string queryString, string databaseId, string collectionId)
        {
            double totalConsumedRU = 0;
            int itemCount = 0;
            //stop watch is unreliable...
            Stopwatch w = new Stopwatch();
            w.Start();

            IQueryable<object> values = client.CreateDocumentQuery<object>(UriFactory.CreateDocumentCollectionUri(databaseId, collectionId),
                queryString,
                new FeedOptions { EnableCrossPartitionQuery = true });
            w.Stop();

            using (IDocumentQuery<dynamic> queryable = values.AsDocumentQuery())
            {
                while (queryable.HasMoreResults)
                {
                    FeedResponse<dynamic> response = await queryable.ExecuteNextAsync<dynamic>();
                    totalConsumedRU += response.RequestCharge;
                    itemCount += Convert.ToInt32(response.ResponseHeaders["x-ms-item-count"]);
                }
            }

            Console.WriteLine("**********************************************");
            Console.WriteLine("RU consumed: " + totalConsumedRU);
            Console.WriteLine("Items returned in Query: " + itemCount);
            Console.WriteLine("Elapsed Time: " + w.ElapsedMilliseconds);
            Console.WriteLine("**********************************************");
            foreach (var item in values)
            {
                Console.WriteLine(item);
            }
        }

        public void DataExplorerTable(CloudTableClient tableClient, string query) {}

        public void DataExplorerMongo(MongoClient mongoClient, string databaseName, string collectionName, string query) {}

        public static void LaunchMongoShell(string pathToMongoEXE, string connectionString) {
            Process mongoShell = new Process();
            mongoShell.StartInfo = new ProcessStartInfo(pathToMongoEXE, connectionString)
            {
                UseShellExecute = false
            };
            mongoShell.Start();
            mongoShell.WaitForExit();
        }

        public static void LaunchGremlinConsole(ICosmosDBAccount account, string pathToGremlinBAT, string pathToRemoteSecureYAML, string databaseName, string collectionName) {
            ModifyRemoteSecureYAML(account, pathToRemoteSecureYAML, databaseName, collectionName);
            Console.WriteLine("Check: " + pathToRemoteSecureYAML);
            Process gremlinConsole = new Process();
            gremlinConsole.StartInfo = new ProcessStartInfo(pathToGremlinBAT)
            {
                UseShellExecute = false,
                WorkingDirectory = pathToGremlinBAT.Substring(0, (pathToGremlinBAT.Length - 15))
            };
            Console.WriteLine("**IMPORTANT** Once the Gremlin Console has loaded please run the following command > :remote connect tinkerpop.server conf/remote-secure.yaml");
            Console.WriteLine("**IMPORTANT** To exit the Gremlin Console, use the command > :x ");
            gremlinConsole.Start();
            gremlinConsole.WaitForExit();
        }

        public static void ModifyRemoteSecureYAML(ICosmosDBAccount account, string pathToRemoteSecureYAML, string databaseName, string collectionName) {
            string host = @"hosts: [" + account.Name + @".graphs.azure.com]";
            string port = @"port: 443";
            string userName = @"username: /dbs/" + databaseName + @"/colls/" + collectionName;
            string password = @"password: " + account.ListKeys().PrimaryMasterKey;
            string serializer = @"serializer: { className: org.apache.tinkerpop.gremlin.driver.ser.GraphSONMessageSerializerV1d0, config: { serializeResultToString: true }}";
            string[] lines = File.ReadLines(pathToRemoteSecureYAML).ToArray();
            lines[26] = host;
            lines[27] = port;
            lines[28] = userName;
            lines[29] = password;
            lines[32] = serializer;
            Console.WriteLine("Modifying remote-secure.yaml");
            File.WriteAllLines(pathToRemoteSecureYAML, lines);
            Console.Write("Finished");
        }
    }
}

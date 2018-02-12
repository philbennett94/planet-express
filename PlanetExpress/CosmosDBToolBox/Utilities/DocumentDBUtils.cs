using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Management.CosmosDB.Fluent;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace PlanetExpress.Utilities
{
    /// <summary>
    /// The DocumentDBUtils class is used to perform tasks on CosmosDB accounts defined with the "SQL api". The class
    /// is used in the PlanetExpress project to provide support for automated testing and configuration functionality 
    /// within the DocumentDBWorkflows class.
    /// </summary>
    class DocumentDBUtils
    {
        // A document client used to perform tasks on the databases and collections within database account defined in the constructor
        public DocumentClient client;
        // A dctionary containing information on databases and their collections contained in the database account defined in the constructor
        public Dictionary<string, string[]> databaseInformation;

        /// <summary>
        /// Constructor for the DocumentDBUtils class that populates the arrtibutes client, and database information.
        /// </summary>
        /// <param name="account"> A valid cosmos db account object that can be used to initialize client and database information. </param>
        public DocumentDBUtils(ICosmosDBAccount account)
        {
            this.client = CreateClientTB(account);
            this.databaseInformation = GetDatabaseInformation();
        }

        /// <summary>
        /// Gathers information on the database account including database names and collection names.
        /// </summary>
        /// <returns> A Dictionary string, string[] databaseInformation or null </returns>
        public Dictionary<string, string[]> GetDatabaseInformation() {
            try
            {
                Dictionary<string, string[]> databaseInformation = new Dictionary<string, string[]>();
                Task<FeedResponse<Database>> databasesTask = GetDatabaseNamesTB(this.client);
                FeedResponse<Database> databases = databasesTask.Result;
                foreach (Database db in databases)
                {
                    databaseInformation[db.Id] = GetCollectionNamesTB(this.client, db);
                }
                return databaseInformation;
            }
            catch (Exception e)
            {
                Console.WriteLine("Something went wrong while trying to gather database information.");
                Console.WriteLine(e.Message);
                return null;
            }
        }

        /// <summary>
        /// Updates the database information dictionary when a database or collection is created.
        /// </summary>
        /// <param name="databaseName"> The name of the database to be added to the dictionary of database information. </param>
        /// <param name="collectionName"> optional - The name of the collection to be added to the dictionary of database information. </param>
        public void UpdateDatabaseInformation(string databaseName, string collectionName = null)
        {
            try
            {
                if (collectionName == null && !(this.databaseInformation.Keys.Contains(databaseName)))
                {
                    this.databaseInformation.Add(databaseName, new string[1]);
                }
                else
                {
                    string[] collectionNames = new string[this.databaseInformation[databaseName].Length + 1];
                    collectionNames[0] = collectionName;
                    for (int i = 0; i < collectionNames.Length; i++)
                    {
                        collectionNames[i] = this.databaseInformation[databaseName][i - 1];
                    }
                    this.databaseInformation[databaseName] = collectionNames;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("There was an error while updating the database information.");
                Console.WriteLine(e.Message);
            }
        }

        /// <summary>
        /// Removes database or collection information from the database on delete.
        /// </summary>
        /// <param name="databaseName"> The name of the database to be removed from the dictionary of database information. </param>
        /// <param name="collectionName"> optional - The name of the collection to be removed from the dictionary of database information. </param>
        public void RemoveDatabaseInformation(string databaseName, string collectionName = null)
        {
            try
            {
                if (collectionName == null)
                {
                    this.databaseInformation.Remove(databaseName);
                }
                else
                {
                    string[] collectionNames = new string[this.databaseInformation[databaseName].Length - 1];
                    int i = 0;
                    foreach (string name in this.databaseInformation[databaseName])
                    {
                        if (name != collectionName)
                        {
                            collectionNames[i] = name;
                            i++;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("There was an error deleteing the database information");
                Console.WriteLine(e.Message);
            }
        }

        /// <summary>
        /// Initialize the document client, return null if there is an exception
        /// </summary>
        /// <param name="account"> A cosmos db account that the client will connect to. </param>
        /// <returns> A valid document client or null. </returns>
        public DocumentClient CreateClientTB(ICosmosDBAccount account) {
            DocumentClient client;
            try
            {
                client = new DocumentClient(new Uri(account.DocumentEndpoint), account.ListKeys().PrimaryMasterKey);
                return client;
            }
            catch (Exception e)
            {
                Console.WriteLine("Something went wrong while creating the document client. Make sure you have provided the correct account.");
                Console.WriteLine(e.Message);
                return null;
            }
        }

        /// <summary>
        /// Creates a Database within a database account.
        /// </summary>
        /// <param name="client"> A valid document client that can be used to access a database. </param>
        /// <param name="databaseId"> The name of the database that will be created. </param>
        /// <returns></returns>
        public async Task<Database> CreateDatabaseTB(DocumentClient client, string databaseId) {
            Database database;
            try
            {
                database = await client.CreateDatabaseIfNotExistsAsync(new Database { Id = databaseId });
                UpdateDatabaseInformation(databaseId);
                return database;
            }
            catch (ArgumentNullException ane)
            {
                Console.WriteLine("Something went wrong while creating the database object. Please ensure you have provided a databaseId.");
                Console.WriteLine(ane.Message);
                return null;
            }
            catch (DocumentClientException dce)
            {
                Console.WriteLine("Something went wrong while creating the database object. Please ensure your DocumentClient is valid.");
                Console.WriteLine("The status code of your request was: {0}.", (int)dce.StatusCode);
                Console.WriteLine(dce.Message);
                return null;
            }
            catch (AggregateException ae)
            {
                Console.WriteLine("An aggregate exception was thrown. For more information please see the message(s) below.");
                foreach (var exception in ae.InnerExceptions)
                {
                    Console.WriteLine(exception.Message);
                }
                return null;
            }

        }

        /// <summary>
        /// Deletes database within a database account.
        /// </summary>
        /// <param name="client"> A valid document client that can be used to access a database. </param>
        /// <param name="databaseId"> The name of the database to delete. </param>
        public async void DeleteDatabaseTB(DocumentClient client, string databaseId) {
            try
            {
                await client.DeleteDatabaseAsync(UriFactory.CreateDatabaseUri(databaseId));
                RemoveDatabaseInformation(databaseId);
            }
            catch (ArgumentNullException ane)
            {
                Console.WriteLine("Something went wrong while deleting the database object. Please ensure you have provided a databaseId and client.");
                Console.WriteLine(ane.Message);
            }
            catch (DocumentClientException dce)
            {
                Console.WriteLine("Something went wrong while deleting the database object. Please ensure your DocumentClient is valid.");
                int code = (int)dce.StatusCode;
                if (code == 404)
                {
                    Console.WriteLine("NotFound - This means the resource you tried to delete did not exist.");
                }
                else
                {
                    Console.WriteLine("The status code of your request was: {0}.", code);
                }
                Console.WriteLine(dce.Message);
            }
        }

        /// <summary>
        /// Creates a collection inside of a database
        /// </summary>
        /// <param name="client"> A valid document client that can be used to access a database. </param>
        /// <param name="databaseId"> The name of the database in which the collection will be created. </param>
        /// <param name="collectionId"> The name of the collection that will be created. </param>
        /// <param name="partitionKey"> optional - The name of the partition key for the collection. </param>
        /// <param name="throughput"> optional - The throughput (RU's) assigned to the collection. </param>
        public async void CreateDocumentCollectionTB(DocumentClient client, string databaseId, string collectionId, string partitionKey = null, int throughput = 1000) {
            try
            {
                DocumentCollection collection = new DocumentCollection();
                collection.Id = collectionId;
                if (partitionKey != null)
                {
                    collection.PartitionKey.Paths.Add(@"/" + partitionKey);
                }
                await client.CreateDocumentCollectionIfNotExistsAsync(
                    UriFactory.CreateDatabaseUri(databaseId),
                    collection,
                    new RequestOptions { OfferThroughput = throughput });
                UpdateDatabaseInformation(databaseId, collectionId);
            }
            catch (ArgumentNullException ane)
            {
                Console.WriteLine("Something went wrong while creating the collection. Please ensure you have provided a the necesary parameters to run the method.");
                Console.WriteLine(ane.Message);
            }
            catch (DocumentClientException dce)
            {
                Console.WriteLine("Something went wrong while creating the collection. Please ensure your DocumentClient is valid.");
                int code = (int)dce.StatusCode;
                switch (code) {
                    case 400:
                        Console.WriteLine("BadRequest - This means something was wrong with the request supplied. It is likely that an id was not supplied for the new collection.");
                        break;
                    case 403:
                        Console.WriteLine("Forbidden - This means you attempted to exceed your quota for collections. Contact support to have this quota increased.");
                        break;
                    default:
                        Console.WriteLine("The status code of your request was: {0}.", code);
                        break;
                }
                Console.WriteLine(dce.Message);
            }
            catch (AggregateException ae)
            {
                Console.WriteLine("An aggregate exception was thrown. For more information please see the message(s) below.");
                foreach (var exception in ae.InnerExceptions)
                {
                    Console.WriteLine(exception.Message);
                }
            }
        }

        /// <summary>
        /// Deletes a collection within a database.
        /// </summary>
        /// <param name="client"> A valid document client that can be used to access a database. </param>
        /// <param name="databaseId"> The name of the database that holds the collection to delete. </param>
        /// <param name="collectionId"> The name of the collection to delete. </param>
        public async void DeleteDocumentCollectionTB(DocumentClient client, string databaseId, string collectionId) {
            try
            {
                await client.DeleteDocumentCollectionAsync(UriFactory.CreateDocumentCollectionUri(databaseId, collectionId));
                RemoveDatabaseInformation(databaseId, collectionId);
            }
            catch (ArgumentNullException ane)
            {
                Console.WriteLine("Something went wrong while deleting the collection. Please ensure you have provided a the necesary parameters to run the method.");
                Console.WriteLine(ane.Message);
            }
            catch (DocumentClientException dce)
            {
                Console.WriteLine("Something went wrong while deleting the collection. Please ensure your DocumentClient is valid, and that the resource exists.");
                int code = (int)dce.StatusCode;
                if (code == 404)
                {
                    Console.WriteLine("NotFound - This means the resource you tried to delete did not exist.");
                }
                else {
                    Console.WriteLine("The status code of your request was: {0}.", code);
                }
                Console.WriteLine(dce.Message);
            }
        }

        /// <summary>
        /// Returns the database feed, an iterable collection of database objects, used for getting the names of databases in a certain account.
        /// </summary>
        /// <param name="client"> A valid document client that can be used to access a database. </param>
        /// <returns> A future containing a feed response that can be used to access database names within an account. </returns>
        public async Task<FeedResponse<Database>> GetDatabaseNamesTB(DocumentClient client) {
            try
            {
                return await client.ReadDatabaseFeedAsync();
            }
            catch (DocumentClientException dce)
            {
                Console.WriteLine("Something went wrong while reading the database feed. Please ensure that your DocumentClient is valid.");
                int code = (int)dce.StatusCode;
                if (code == 429)
                {
                    Console.WriteLine("TooManyRequests - This means you have exceeded the number of request units per second. Consult the DocumentClientException.RetryAfter value to see how long you should wait before retrying this operation.");
                }
                else {
                    Console.WriteLine("The status code of your request was: {0}.", code);
                }
                Console.WriteLine(dce.Message);
                return null;
            }
        }

        /// <summary>
        /// Gets the names of all collections associated with a database.
        /// </summary>
        /// <param name="client">  A valid document client that can be used to access a database. </param>
        /// <param name="database"> A valid cosmos db database object. </param>
        /// <returns> An array of strings containing the names of the collections or null if their are no collections in the database. </returns>
        public string[] GetCollectionNamesTB(DocumentClient client, Database database) {
            try
            {
                int i = 0;
                List<DocumentCollection> collectionsList = client.CreateDocumentCollectionQuery((String)database.SelfLink).ToList();
                string[] collectionNames = new string[collectionsList.Count()];
                foreach (DocumentCollection dc in collectionsList)
                {
                    collectionNames[i] = dc.Id;
                    i++;
                }
                return collectionNames;
            }
            catch (Exception e)
            {
                Console.WriteLine("Something went wrong while trying to gather collection names. Please ensure that all parameters are valid.");
                Console.WriteLine(e.Message);
                return null;
            }
        }

        /// <summary>
        /// Used to insert one or more json documents into a database.
        /// </summary>
        /// <param name="client"> A valid document client that can be used to access a databse </param>
        /// <param name="databaseId"> The name of the database in which the document will be insterted </param>
        /// <param name="collectionId"> The name of the collection in which the document will be inserted </param>
        /// <param name="files"> An array of file paths that denote the location of json files to be inserted into the collection </param>
        /// <param name="loop"> The number of times to insert the files specified by the parameter [files] </param>
        public async void InsertOneOrManyDocumentsTB(DocumentClient client, string databaseId, string collectionId, string[] files, int loop) {
            try
            {

                List<object> fileJSON = files.Select(f => CreateJSONObjectFromFileTB(f)).ToList();
                if (loop > 1)
                {
                    List<object> fileBSONCopy = new List<object>(fileJSON);
                    while (loop > 1)
                    {
                        fileJSON.AddRange(fileBSONCopy);
                        loop--;
                    }
                }
                foreach (object file in fileJSON)
                {
                    await client.CreateDocumentAsync(
                        UriFactory.CreateDocumentCollectionUri(databaseId, collectionId), file);
                }
            }
            catch (ArgumentNullException ane)
            {
                Console.WriteLine("Please ensure that the list of filepaths provided are valid.");
                Console.WriteLine(ane.Message);
            }
            catch (DocumentClientException dce)
            {
                int code = (int)dce.StatusCode;
                switch (code)
                {
                    case 400:
                        Console.WriteLine("BadRequest - This means something was wrong with the document supplied. It is likely that disableAutomaticIdGeneration was true and an id was not supplied.");
                        break;
                    case 403:
                        Console.WriteLine("Forbidden - This likely means the collection in to which you were trying to create the document is full.");
                        break;
                    case 409:
                        Console.WriteLine("Conflict - This means a Document with an id matching the id field of document already existed");
                        break;
                    case 413:
                        Console.WriteLine("RequestEntityTooLarge - This means the Document exceeds the current max entity size. Consult documentation for limits and quotas.");
                        break;
                    case 429:
                        Console.WriteLine("TooManyRequests - This means you have exceeded the number of request units per second. Consult the DocumentClientException.RetryAfter value to see how long you should wait before retrying this operation.");
                        break;
                    default:
                        Console.WriteLine("The status code of your request was: {0}.", code);
                        break;
                }
                Console.WriteLine(dce.Message);
            }
            catch (AggregateException ae) {
                Console.WriteLine("An aggregate exception was thrown. For more information please see the message(s) below.");
                foreach (var exception in ae.InnerExceptions) {
                    Console.WriteLine(exception.Message);
                }
            }
        }

        /// <summary>
        /// Used to deserialize a json file so that it can inserted into a collection
        /// </summary>
        /// <param name="pathToFile"> path to the valid json file that you want to add to your collection </param>
        /// <returns> A valid jason object that can be inserted into a collection </returns>
        public object CreateJSONObjectFromFileTB(string pathToFile)
        {
            try
            {
                StreamReader streamReader = new StreamReader(pathToFile);
                JsonTextReader jsonTextReader = new JsonTextReader(streamReader);
                JsonSerializer jsonSerializer = new JsonSerializer();
                return jsonSerializer.Deserialize(jsonTextReader);
            }
            catch (Exception e)
            {
                Console.WriteLine("Please make sure the file you have provided is a valid JSON document.");
                Console.WriteLine(e.Message);
                return null;
            }
        }

        /// <summary>
        /// Deletes all documents returned by a query. This can be used to delete all documents in a collectionby leaving
        /// the query string parameter blank as the default value is set to SELECT * FROM c. 
        /// </summary>
        /// <param name="client"> DocumentClient instance that establish a valid programatic connection to the database </param>
        /// <param name="databaseId"> string that is the name of the database you'd like to delete items from </param>
        /// <param name="collectionId"> string that is the name of the collection you'd like to delete items from </param>
        /// <param name="queryString"> string that defines the DocumentDB SQL query to use as matching criteria for the delete </param>
        public async void DeleteByQuery(DocumentClient client, string databaseId, string collectionId, string partitionKey, string queryString = "SELECT * FROM c")
        {
            try
            {
                IQueryable<Document> documents = client.CreateDocumentQuery<Document>(UriFactory.CreateDocumentCollectionUri(databaseId, collectionId),
                    queryString,
                    new FeedOptions { EnableCrossPartitionQuery = true });

                foreach (Document doc in documents)
                {
                    await client.DeleteDocumentAsync(doc.SelfLink,
                        new RequestOptions
                        {
                            // change type arg for GetPropertyValue<>() to the type of your partition key
                            PartitionKey = new PartitionKey(doc.GetPropertyValue<string>(partitionKey))
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
    }
}

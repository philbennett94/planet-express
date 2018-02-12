using Microsoft.Azure.Management.CosmosDB.Fluent;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Authentication;

namespace PlanetExpress.Utilities
{

    class MongoDBUtils : DocumentDBUtils
    {
        public MongoClient mongoClient;
        public string connectionString;

        /// <summary>
        /// constuctor for MongoDBUtils.
        /// </summary>
        /// <param name="account"> valid cosmos db account. </param>
        public MongoDBUtils(ICosmosDBAccount account) : base(account)
        {
            this.connectionString = FormConnectionStringTB(account);
            this.mongoClient = CreateClientTB();
        }

        /// <summary>
        /// Creates a mongoDB connection string for an account.
        /// </summary>
        /// <param name="account"> a Cosmos DB account object </param>
        /// <returns> the connection string of the resource or null. </returns>
        public string FormConnectionStringTB(ICosmosDBAccount account)
        {
            try
            {
                string userName = account.Name;
                string password = account.ListKeys().PrimaryMasterKey;
                string host = account.DocumentEndpoint.Split('/')[2].Split(':')[0];
                return String.Format("mongodb://{0}:{1}@{2}:10255/?ssl=true&replicaSet=globaldb", userName, password, host);
            } catch(Exception e) {
                Console.WriteLine("Something went wrong while trying to create a MongoDB connection string.");
                Console.WriteLine(e.Message);
                return null;
            }
        }

        /// <summary>
        /// Creates an instance of the MongoClient class for use in database and collection operations
        /// </summary>
        /// <returns> MongoClient or null</returns>
        public MongoClient CreateClientTB()
        {
            try
            {
                MongoClientSettings settings = MongoClientSettings.FromUrl(new MongoUrl(this.connectionString));
                settings.SslSettings = new SslSettings() { EnabledSslProtocols = SslProtocols.Tls12 };
                return new MongoClient(settings);
            }
            catch(Exception e) {
                Console.WriteLine("Something went wrong while trying to create the MongoClient. Please ensure you are trying to access a MongoDB account.");
                Console.WriteLine(e.Message);
                return null;
            }
        }

        /// <summary>
        /// Deletes a database from a mongo db account.
        /// </summary>
        /// <param name="client"> a valid mongo client. </param>
        /// <param name="databaseName"> the name of the database. </param>
        public async void DeleteDatabaseTB(MongoClient client, string databaseName)
        {
            try
            {
                await client.DropDatabaseAsync(databaseName);
                RemoveDatabaseInformation(databaseName);
            }
            catch (Exception e) {
                Console.WriteLine("Something went wrong while trying to drop the database. Please ensure your mongo client is valid, and that you have provided the correct database name.");
                Console.WriteLine(e.Message);
            }
        }

        /// <summary>
        /// Selects a mongo database for use in database related activities.
        /// </summary>
        /// <param name="client"> a vlaid mongo client. </param>
        /// <param name="dbName"> the name of the database. </param>
        /// <returns></returns>
        public IMongoDatabase GetDatabaseTB(MongoClient client, string dbName)
        {
            try
            {
                IMongoDatabase database = client.GetDatabase(dbName);
                return database;
            }
            catch (Exception e) {
                Console.WriteLine("Something went wrong while trying to select the mongo db database. Please make sure you have provided a valid mongo client and database name.");
                Console.WriteLine(e.Message);
                return null;
            }
        }

        /// <summary>
        /// Inserts one or more doucments into the a collection.
        /// </summary>
        /// <param name="client"> a valid mongo client. </param>
        /// <param name="databaseName"> the name of the database. </param>
        /// <param name="collectionName"> the name of the collection in which documents will be inserted. </param>
        /// <param name="files"> a list of file paths separated by a comma. </param>
        /// <param name="loop"> the amount of times you would like the files/list of files to be inserted. </param>
        public async void InsertOneOrManyDocumentsTB(MongoClient client, string databaseName, string collectionName, string[] files, int loop)
        {
            try
            {
                List<BsonDocument> fileBSON = files.Select(f => CreateBSONObjectFromFileTB(f)).ToList();
                if (loop > 1)
                {
                    List<BsonDocument> fileBSONCopy = new List<BsonDocument>(fileBSON);
                    while (loop > 1)
                    {
                        fileBSON.AddRange(fileBSONCopy);
                        loop--;
                    }
                }
                IMongoDatabase database = client.GetDatabase(databaseName);
                IMongoCollection<object> collection = database.GetCollection<object>(collectionName);
                await collection.InsertManyAsync(fileBSON);
            }
            catch (Exception e) {
                Console.WriteLine("Something went wrong while trying to insert one or more documents.");
                Console.WriteLine("Ensure the following variables were passed to the method, and that they are correct: MongoClient client, string databaseName, string collectionName, List<object> files");
                Console.WriteLine(e.Message);
            }
        }

        /// <summary>
        /// Creates a collection within a mongo db database
        /// </summary>
        /// <param name="database">MongoDatabase object. </param>
        /// <param name="databaseName"> the name of the database. </param>
        /// <param name="collectionName"> the name of the collection to create. </param>
        public async void CreateCollectionTB(IMongoDatabase database, string databaseName ,string collectionName)
        {
            try
            {
                await database.CreateCollectionAsync(collectionName);
                UpdateDatabaseInformation(databaseName, collectionName);
            }
            catch (Exception e) {
                Console.WriteLine("Something went wrong while creating the mongo collection. Please ensure the following inputs have been provided and are valid: IMongoDatabase database, string collectionName");
                Console.WriteLine(e.Message);
            }
        }

        /// <summary>
        /// Deletes a Mongo DB collection
        /// </summary>
        /// <param name="database"> MongoDatabase object. </param>
        /// <param name="databaseName"> the name of the database. </param>
        /// <param name="collectionName"> the name of the collection to delete. </param>
        public async void DropCollection(IMongoDatabase database, string databaseName, string collectionName)
        {
            try
            {
                await database.DropCollectionAsync(collectionName);
                RemoveDatabaseInformation(databaseName, collectionName);
            }
            catch (Exception e) {
                Console.WriteLine("Something went wrong while creating the mongo collection. Please ensure the following inputs have been provided and are valid: IMongoDatabase database, string collectionName");
                Console.WriteLine(e.Message);
            }
        }

        /// <summary>
        /// Reads in json document and converts it to Bson so that it can be insterted into MongoDB.
        /// </summary>
        /// <param name="pathToFile"> path to file on local machine</param>
        /// <returns> BsonDocument, or null </returns>
        public BsonDocument CreateBSONObjectFromFileTB(string pathToFile)
        {
            try
            {
                string docString = File.ReadAllText(pathToFile);
                return BsonDocument.Parse(docString);
            }
            catch (Exception e)
            {
                Console.WriteLine("Please make sure the file you have provided is a valid JSON document.");
                Console.WriteLine(e.Message);
                return null;
            }
        }

        /// <summary>
        /// Delete Many async example. The delete operation matches documents on a field called message id. 
        /// The field is a top level attribute in the document.
        /// </summary>
        /// <param name="collection" type="IMongoCollection<TDocument>"> TDocument -> BsonDocument </param>
        public static async void DeleteManyAsyncWrapper(IMongoCollection<BsonDocument> collection)
        {
            await collection.DeleteManyAsync(Builders<BsonDocument>.Filter.Eq("messageid", "1"));
        }
    }
}

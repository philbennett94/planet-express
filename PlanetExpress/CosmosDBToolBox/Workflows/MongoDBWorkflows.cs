using PlanetExpress.Interfaces;
using PlanetExpress.Utilities;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Management.CosmosDB.Fluent;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlanetExpress.Workflows
{
    class MongoDBWorkflows : IDatabaseWorkflow
    {
        public MongoDBUtils mongodb;
        public MongoDBWorkflows(ICosmosDBAccount account)
        {
            this.mongodb = new MongoDBUtils(account);
        }

        /// <summary>
        /// creates a mongo db database and prompts for necesary inputs.
        /// </summary>
        public void CreateDatabaseWF()
        {
            string databaseName = GeneralUtils.PromptInput("Please enter a name for your new database. It cannot be blank");
            Task<Database> newdbTask = this.mongodb.CreateDatabaseTB(this.mongodb.client, databaseName);
            Database newdb = newdbTask.Result;
            if (newdb == null)
            {
                Console.WriteLine("The database could not be created. It may already exist. Please check the console above for potential errors.");
            }
            else
            {
                Console.WriteLine("The database {0} was created.", newdb.Id);
            }
        }

        /// <summary>
        /// deletes a mongo db database and prompts for necesary inputs.
        /// </summary>
        public void DeleteDatabaseWF()
        {
            string databaseName = GeneralUtils.PromptInput("Please enter the name of the database you want to drop, it cannot be blank");
            this.mongodb.DeleteDatabaseTB(this.mongodb.mongoClient, databaseName);
            Console.WriteLine("The delete process has begun and runs asynchronously in the background. Potential errors may show in the console at anytime.");
        }

        /// <summary>
        /// lists databases and prompts for necesary inputs.
        /// </summary>
        public void ListDatabasesWF()
        {
            string[] headers = { "Database Names" };
            List<string[]> lines = GeneralUtils.CreateMenuHeader(headers);
            foreach (string dbName in this.mongodb.databaseInformation.Keys)
            {
                lines.Add(new string[] { dbName });
            }
            Console.WriteLine(GeneralUtils.PadElementsInLines(lines));
        }

        /// <summary>
        /// creates a collection for a mongo database and prompts for necesary inputs.
        /// </summary>
        public void CreateCollectionWF()
        {
            string databaseName = GeneralUtils.PromptInput("Please enter the name of the database for which you want to create a collection");
            string collectionName = GeneralUtils.PromptInput("Please enter a name for the new collection. This cannot be blank");
            IMongoDatabase database = this.mongodb.GetDatabaseTB(this.mongodb.mongoClient, databaseName);
            this.mongodb.CreateCollectionTB(database, databaseName, collectionName);
            Console.WriteLine("The collection is being created. This process occurs asynchronously in the background and usually completes quickly. Errors realted to this process may be written to the console at anytime.");
        }

        /// <summary>
        /// deletes a collection for a mongo db database and prompts for necesary inputs.
        /// </summary>
        public void DeleteCollectionWF()
        {
            string databaseName = GeneralUtils.PromptInput("Please enter the name of the database that contains the collection you would like to delete");
            string collectionName = GeneralUtils.PromptInput("Please enter the name of the collection you would like to delete in " + databaseName);
            IMongoDatabase database = this.mongodb.GetDatabaseTB(this.mongodb.mongoClient, databaseName);
            this.mongodb.DropCollection(database, databaseName, collectionName);
        }

        /// <summary>
        /// lists all collections contained within a mongo db database and prompts for necesary inputs.
        /// </summary>
        public void ListCollectionsWF()
        {
            string databaseName = GeneralUtils.PromptInput("Please enter the name of the database for which you would like to list collections");
            string[] headers = { ("Collections in Database: " + databaseName) };
            List<string[]> lines = GeneralUtils.CreateMenuHeader(headers);
            foreach (string coll in this.mongodb.databaseInformation[databaseName])
            {
                lines.Add(new string[] { coll });
            }
            Console.WriteLine(GeneralUtils.PadElementsInLines(lines));
        }

        /// <summary>
        /// inserts one or many documents into a mongo db database.
        /// </summary>
        public void InsertOneOrManyWF()
        {
            string databaseName = GeneralUtils.PromptInput("Please enter the name of the database that holds the collection you want to insert documents into. It cannot be blank");
            string collectionName = GeneralUtils.PromptInput("Please enter the name of the collection you want to insert documents into");
            string[] fileNames = GeneralUtils.PromptInput("Please enter one or more absolute paths to JSON files separated only by a comma").Split(',');
            string loop = GeneralUtils.PromptInput("How many times would you like to insert these files? Enter 1 or more");
            int loopInt;
            bool result = Int32.TryParse(loop, out loopInt);
            if (result)
            {
                this.mongodb.InsertOneOrManyDocumentsTB(this.mongodb.mongoClient, databaseName, collectionName, fileNames, loopInt);
            }
            else
            {
                this.mongodb.InsertOneOrManyDocumentsTB(this.mongodb.mongoClient, databaseName, collectionName, fileNames, 1);
            }
            
            Console.WriteLine("The insert process has begun and will occur asynchronously in the background. Errors related to this process may write to the console at any time");
        }
    }
}

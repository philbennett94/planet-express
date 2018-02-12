using PlanetExpress.Utilities;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Management.CosmosDB.Fluent;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PlanetExpress.Interfaces;

namespace PlanetExpress.Workflows
{
    class DocumentDBWorkflows : IDatabaseWorkflow
    {
        public DocumentDBUtils docdb;
        public DocumentDBWorkflows(ICosmosDBAccount account) {
            this.docdb = new DocumentDBUtils(account);
        }

        /// <summary>
        /// Creates a document db database and prompts for necesary inputs.
        /// </summary>
        public void CreateDatabaseWF()
        {
            string databaseName = GeneralUtils.PromptInput("Please enter a name for your new database. It cannot be blank");
            Task<Database> newdbTask = this.docdb.CreateDatabaseTB(this.docdb.client, databaseName);
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
        /// Deletes a document db database and prompts for necesary inputs.
        /// </summary>
        public void DeleteDatabaseWF()
        {
            string databaseName = GeneralUtils.PromptInput("Please enter the name of the database you want to delete. It cannot be blank");
            this.docdb.DeleteDatabaseTB(this.docdb.client, databaseName);
            Console.WriteLine("Deleting the database, this process occurs in the background and the change may take a few minutes to reflect in the portal.");
            Console.WriteLine("You may see errors related to this process in the terminal.");
        }

        /// <summary>
        /// Lists databases and prompts for necesary inputs.
        /// </summary>
        public void ListDatabasesWF()
        {
            string[] headers = { "Database Names" };
            List<string[]> lines = GeneralUtils.CreateMenuHeader(headers);
            foreach (string dbName in this.docdb.databaseInformation.Keys)
            {
                lines.Add(new string[] { dbName });
            }
            Console.WriteLine(GeneralUtils.PadElementsInLines(lines));
        }

        /// <summary>
        /// creates a collection in a database and prompts for necesary inputs.
        /// </summary>
        public void CreateCollectionWF()
        {
            string databaseName = GeneralUtils.PromptInput("Please enter the name of the database where the collection will reside");
            string collectionName = GeneralUtils.PromptInput("Please enter a name for your new collection");
            string partitionKey = GeneralUtils.OptionalArgumentHelper(GeneralUtils.PromptInput("Please enter a parition key. Press enter without typing anything to ccreate a collection without a partition key"));
            string throughput = GeneralUtils.OptionalArgumentHelper(GeneralUtils.PromptInput("Please enter the throughput for the collection. Press enter without typing anything to use the default throughput of 1000 RU"));
            if (int.TryParse(throughput, out int ru))
            {
                this.docdb.CreateDocumentCollectionTB(this.docdb.client, databaseName, collectionName, GeneralUtils.OptionalArgumentHelper(partitionKey), ru);
            }
            else
            {
                Console.WriteLine("The throughput value you provided could not be parsed to an integer. Creating collection with 1000 RU throughput");
                this.docdb.CreateDocumentCollectionTB(this.docdb.client, databaseName, collectionName, partitionKey, 1000);
            }
        }

        /// <summary>
        /// deletes a collection in a database and prompts for necesary inputs.
        /// </summary>
        public void DeleteCollectionWF()
        {
            string databaseName = GeneralUtils.PromptInput("Please enter the name of the database that holds the collection you want to delete. It cannot be blank");
            string collectionName = GeneralUtils.PromptInput("Please enter the name of the collection you want to delete");
            this.docdb.DeleteDocumentCollectionTB(this.docdb.client, databaseName, collectionName);
            Console.WriteLine("The delete process has been initiated it may take a few minutes to reflect in the portal. If errors occure they may be logged in the console at any time.");
        }

        /// <summary>
        /// lists collections in a database and prompts for necesary inputs.
        /// </summary>
        public void ListCollectionsWF()
        {
            string databaseName = GeneralUtils.PromptInput("Please enter the name of the database for which you would like to list collections");
            string[] headers = { ("Collections in Database: " + databaseName) };
            List<string[]> lines = GeneralUtils.CreateMenuHeader(headers);
            foreach (string coll in this.docdb.databaseInformation[databaseName]) {
                lines.Add(new string[] { coll });
            }
            Console.WriteLine(GeneralUtils.PadElementsInLines(lines));
        }

        /// <summary>
        /// inserts one or many documents into a database and prompts for necesary inputs.
        /// </summary>
        public void InsertOneOrManyWF()
        {
            string databaseName = GeneralUtils.PromptInput("Please enter the name of the database that holds the collection you want to insert documents into. It cannot be blank");
            string collectionName = GeneralUtils.PromptInput("Please enter the name of the collection you want to insert documents into");
            string[] fileNames = GeneralUtils.PromptInput("Please enter one or more absolute paths to JSON files separated only by a comma").Split(',');
            string loop = GeneralUtils.PromptInput("How many times would you like to insert these files? Enter 1 or more");
            int loopInt;
            bool result = Int32.TryParse(loop, out loopInt);
            Console.WriteLine("loop: " + loopInt);
            if (result)
            {
                this.docdb.InsertOneOrManyDocumentsTB(this.docdb.client, databaseName, collectionName, fileNames, loopInt);
            }
            else
            {
                this.docdb.InsertOneOrManyDocumentsTB(this.docdb.client, databaseName, collectionName, fileNames, 1);
            }
            Console.WriteLine("The insert process has begun and will occur asynchronously in the background. Errors related to this process may write to the console at any time");
        }
    }
}

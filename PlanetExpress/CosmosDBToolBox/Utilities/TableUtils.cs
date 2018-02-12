using Microsoft.Azure.Management.CosmosDB.Fluent;
using Microsoft.Azure.CosmosDB.Table;
using Microsoft.Azure.Storage;
using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlanetExpress.Utilities
{
    class TableUtils : DocumentDBUtils
    {
        public string connectionString;
        public CloudTableClient tableClient;

        /// <summary>
        /// Create a TableUtils class for use with CosmosDB Table API operations/resources
        /// </summary>
        /// <param name="account"> a valid cosmos db account. </param>
        public TableUtils(ICosmosDBAccount account) : base (account) {
            this.connectionString = FormConnectionString(account);
            CloudStorageAccount storageAccount = CreateStorageAccountTB();
            this.tableClient = CreateClientTB(storageAccount);
        }

        /// <summary>
        /// Creates a connection string used to access the table storage account
        /// </summary>
        /// <param name="account"> valid cosmos db account. </param>
        /// <returns> connection string or null if error</returns>
        public string FormConnectionString(ICosmosDBAccount account) {
            try
            {
                string name = account.Name;
                string key = account.ListKeys().PrimaryMasterKey;
                string endpoint = "https://" + name + ".table.cosmosdb.azure.com:443/";
                return String.Format("DefaultEndpointsProtocol=https;AccountName={0};AccountKey={1};TableEndpoint={2};", name, key, endpoint);
            }
            catch (Exception e)
            {
                Console.WriteLine("Something went wrong while trying to create a Storage account connection string.");
                Console.WriteLine(e.Message);
                return null;
            }
        }

        /// <summary>
        /// Creates a cloud storage account for use with table operations
        /// </summary>
        /// <returns> returns a cloud storage account or null if error</returns>
        public CloudStorageAccount CreateStorageAccountTB()
        {
            try
            {
                CloudStorageAccount storageAccount = CloudStorageAccount.Parse(this.connectionString);
                return storageAccount;
            }
            catch (ArgumentNullException ane)
            {
                Console.WriteLine("The connection string is null or empty.");
                Console.WriteLine(ane.Message);
                return null;
            }
            catch (FormatException fex)
            {
                Console.WriteLine("The connection string is not a valid connection string.");
                Console.WriteLine(fex.Message);
                return null;
            }
            catch (ArgumentException aex) {
                Console.WriteLine("The connectionString cannot be parsed.");
                Console.WriteLine(aex.Message);
                return null;
            }
        }

        /// <summary>
        /// Creates a table client used to interact with the table database
        /// </summary>
        /// <param name="storageAccount"> valid cloud storage account. </param>
        /// <returns> tableClient or null if error. </returns>
        public CloudTableClient CreateClientTB(CloudStorageAccount storageAccount)
        {
            try
            {
                CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
                return tableClient;
            }
            catch (Exception e) {
                Console.WriteLine("Something went wrong while trying to create the table client. Please make sure your Cloud Storage account is configured correctly.");
                Console.WriteLine(e.Message);
                return null;
            }
        }

        /// <summary>
        /// Selects a cloud table for use in table operations
        /// </summary>
        /// <param name="tableName"> name of the table to use. </param>
        /// <returns> CloudTable or null if error. </returns>
        public CloudTable GetTable(string tableName)
        {
            try
            {
                CloudTable table = this.tableClient.GetTableReference(tableName);
                return table;
            }
            catch (Exception e) {
                Console.WriteLine("Something went wrong while selecting the cloudTable. Please ensure the following parameters are correct: CloudTableClient tableClient, string tableName");
                Console.WriteLine(e.Message);
                return null;
            }
        }

        /// <summary>
        /// Creates a table if the table does not already exist
        /// </summary>
        /// <param name="table"> name of table to create. </param>
        public async void CreateTable(CloudTable table) {
            try
            {
                await table.CreateIfNotExistsAsync();
            }
            catch (Exception e) {
                Console.WriteLine("Something went wrong while trying to create the table. Ensure your CloudTable reference is correct.");
                Console.WriteLine(e.Message);
            }
        }

        /// <summary>
        /// Deletes a cloudtable 
        /// </summary>
        /// <param name="table"> name of table to delete. </param>
        public async void DeleteTable(CloudTable table) {
            try
            {
                await table.DeleteIfExistsAsync();
            }
            catch (Exception e) {
                Console.WriteLine("Something went wrong while trying to delete the table. Ensure your CloudTable reference is correct.");
                Console.WriteLine(e.Message);
            }
        }

        /// <summary>
        /// Inserts one or more entities into the table
        /// </summary>
        /// <param name="table"> name of table to insert entities into. </param>
        /// <param name="loop"> number of times you want to insert. </param>
        public async void InsertOneOrMany(CloudTable table, int loop) {
            try
            {
                List<TestEntity> entities = CreateEntityList(loop);
                TableBatchOperation batchOperation = new TableBatchOperation();
                Console.WriteLine("length of list: " + entities.Count);
                foreach (TestEntity entity in entities)
                {
                    batchOperation.Insert(entity);
                }
                await table.ExecuteBatchAsync(batchOperation);
            }
            catch (Exception e) {
                Console.WriteLine("Something went wrong while attempting to insert one or more documents.");
                Console.WriteLine(e.Message);
            }
        }

        /// <summary>
        /// Create a list of entities to be inserted into a table
        /// </summary>
        /// <param name="count"></param>
        /// <returns></returns>
        public List<TestEntity> CreateEntityList(int count)
        {
            List<TestEntity> payload = new List<TestEntity>();
            Random rand = new Random();
            string id = GenerateRandomString(10, rand);
            while (count > 0)
            {
                int pageCount = rand.Next(700);
                string firstName = GenerateRandomString(7, rand);
                string lastName = GenerateRandomString(15, rand);
                string title = GenerateRandomString(5, rand);
                TestEntity t = new TestEntity(id, firstName, lastName, title, pageCount);
                Console.WriteLine(t.ToString());
                payload.Add(t);
                count--;
            }
            return payload;
        }

        /// <summary>
        /// Generates a random string.
        /// </summary>
        /// <param name="len"> length of the random string to create. </param>
        /// <param name="rand"> an instance of Random for random number generation. </param>
        /// <returns> random string. </returns>
        public string GenerateRandomString(int len, Random rand)
        {
            const string alphabet = "abcdefghijklmnopqrstuvwxyz";
            var chars = Enumerable.Range(0, len)
                .Select(x => alphabet[rand.Next(0, alphabet.Length)]);
            return new string(chars.ToArray());
        }
    }
}

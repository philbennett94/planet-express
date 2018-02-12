using PlanetExpress.Interfaces;
using PlanetExpress.Utilities;
using Microsoft.Azure.Management.CosmosDB.Fluent;
using Microsoft.Azure.CosmosDB.Table;
using System;

namespace PlanetExpress.Workflows
{
    class TableWorkflows : DocumentDBWorkflows, IDatabaseWorkflow
    {
        TableUtils tabu;
        
        /// <summary>
        /// Constructor. Database creation, deletion, list, and list collections are all inherited from DocumentDBWorkflows class.
        /// </summary>
        /// <param name="account"> a valid cosmos db account name with table set as the default expirience. </param>
        public TableWorkflows(ICosmosDBAccount account) : base(account)
        {
            this.tabu = new TableUtils(account); 
        }

        /// <summary>
        /// Creates a table and prompts for necesary inputs.
        /// </summary>
        public new void CreateCollectionWF()
        {
            string tableName = GeneralUtils.PromptInput("Please enter a name for your new table");
            CloudTable table = this.tabu.GetTable(tableName);
            this.tabu.CreateTable(table);
            this.docdb.UpdateDatabaseInformation(tableName);
            Console.WriteLine("The table is being created in the background. Errors can appear at anytime in the console if they arise. This process is usually quick.");
        }

        /// <summary>
        /// Deletes a table and prompts for necesary inputs.
        /// </summary>
        public new void DeleteCollectionWF()
        {
            string tableName = GeneralUtils.PromptInput("Please enter the name of the table you want to delete");
            CloudTable table = this.tabu.GetTable(tableName);
            this.tabu.DeleteTable(table);
            this.docdb.RemoveDatabaseInformation("TablesDB", tableName);
            Console.WriteLine("The table is being deleted in the background. Errors can appear at anytime in the console if they arise. This process is usually quick.");
        }

        /// <summary>
        /// inserts one or more entities into a table and prompts for necesary inputs.
        /// </summary>
        public new void InsertOneOrManyWF()
        {
            string tableName = GeneralUtils.PromptInput("Please enter the name of the table to which entities will be written");
            CloudTable table = this.tabu.GetTable(tableName);
            string loopStr = GeneralUtils.PromptInput("Please enter the number of entities you would like written to the table");
            int loop;
            if (Int32.TryParse(loopStr, out loop))
            {
                this.tabu.InsertOneOrMany(table, loop);
            }
            else
            {
                Console.WriteLine("Your count could not be parsed to an int... inserting 5 entities to table {0}.", tableName);
                this.tabu.InsertOneOrMany(table, 5);
            }
        }
    }
}

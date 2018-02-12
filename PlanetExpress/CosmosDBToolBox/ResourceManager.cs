using Microsoft.Azure.Management.CosmosDB.Fluent;
using PlanetExpress.Workflows;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PlanetExpress
{
    class ResourceManager
    {
        public AccountWorkflows accountWorkflow;
        public Dictionary<string, DocumentDBWorkflows> docdbWorkflows;
        public Dictionary<string, MongoDBWorkflows> mongodbWorkflows;
        public Dictionary<string, GraphWorkflows> graphWorkflows;
        public Dictionary<string, TableWorkflows> tableWorkflows;

        /// <summary>
        /// Constructor. ResourceManager runs the worflows against resources.
        /// </summary>
        /// <param name="pathToAuth"> path to a valid azure auth file which defines credentials for AAD file. </param>
        public ResourceManager(string pathToAuth)
        {
            this.accountWorkflow = new AccountWorkflows(pathToAuth);
            this.docdbWorkflows   = new Dictionary<string, DocumentDBWorkflows>();
            this.mongodbWorkflows = new Dictionary<string, MongoDBWorkflows>();
            this.graphWorkflows   = new Dictionary<string, GraphWorkflows>();
            this.tableWorkflows   = new Dictionary<string, TableWorkflows>();
        }

        /// <summary>
        /// Checks for an existing account.
        /// </summary>
        /// <param name="accountName"> an account name to reference for the check. </param>
        /// <returns> a char denoting the default expirience of the account or n if the account isn't found. </returns>
        public Char CheckForExistingWorkFlow(string accountName)
        {
            bool doc = this.docdbWorkflows.Keys.Contains(accountName);
            bool mongo = this.mongodbWorkflows.Keys.Contains(accountName);
            bool graph = this.graphWorkflows.Keys.Contains(accountName);
            bool table = this.tableWorkflows.Keys.Contains(accountName);
            if (doc == true) { return 'd'; }
            else if (mongo == true) { return 'm'; }
            else if (graph == true) { return 'g'; }
            else if (table == true) { return 't'; }
            else { return 'n'; }
        }

        /// <summary>
        /// creates a doc db workflows class.
        /// </summary>
        /// <param name="account"> cosmos db account, prefereably with SQL API (doc db) set as the default expirience. </param>
        /// <param name="name"> name of the account. </param>
        /// <returns> an instance of DocumentDBWorkflows</returns>
        public DocumentDBWorkflows GenerateDocDBWF(ICosmosDBAccount account, string name)
        {
            DocumentDBWorkflows wf = new DocumentDBWorkflows(account);
            docdbWorkflows.Add(name, wf);
            return wf;
        }

        /// <summary>
        /// creates a mongo db workflows class.
        /// </summary>
        /// <param name="account"> cosmos db account, prefereably with MONGO DB API (mongo db) set as the default expirience. </param>
        /// <param name="name"> name of the account. </param>
        /// <returns> an instance of MongoDBWorkflows</returns>
        public MongoDBWorkflows GenerateMongoDBWF(ICosmosDBAccount account, string name)
        {
            MongoDBWorkflows wf = new MongoDBWorkflows(account);
            mongodbWorkflows.Add(name, wf);
            return wf;
        }

        /// <summary>
        /// creates a graph workflows class.
        /// </summary>
        /// <param name="account"> cosmos db account, prefereably with GRAPH API (graph database) set as the default expirience. </param>
        /// <param name="name"> name of the account. </param>
        /// <returns> an instance of GraphWorkflows</returns>
        public GraphWorkflows GenerateGraphWF(ICosmosDBAccount account, string name)
        {
            GraphWorkflows wf = new GraphWorkflows(account);
            graphWorkflows.Add(name, wf);
            return wf;
        }

        /// <summary>
        /// creates a table workflows class.
        /// </summary>
        /// <param name="account"> cosmos db account, prefereably with TABLE API (table store) set as the default expirience. </param>
        /// <param name="name"> name of the account. </param>
        /// <returns> an instance of TableWorkflows</returns>
        public TableWorkflows GenerateTableWF(ICosmosDBAccount account, string name)
        {
            TableWorkflows wf = new TableWorkflows(account);
            tableWorkflows.Add(name, wf);
            return wf;
        }

        /// <summary>
        /// executes account level operations.
        /// </summary>
        /// <param name="wf"> an instance of account workflows. </param>
        /// <param name="op"> the operation to complete. </param>
        public void RunAccount(AccountWorkflows wf, int op)
        {
            switch (op)
            {
                case 0: wf.CreateDatabaseAccountWF(); break;
                case 1: wf.DeleteDatabaseAccountWF(); break;
                case 2: wf.ListDatabaseAccountsWF(); break;
                case 3:wf.ListDatabaseAccountInformationWF(); break;
            }
        }

        /// <summary>
        /// runs database level operations.
        /// </summary>
        /// <param name="wf"> instance of workflows. </param>
        /// <param name="op"> the operation to complete. </param>
        public void RunDoc(DocumentDBWorkflows wf, int op)
        {
            switch (op)
            {
                case 0: wf.CreateDatabaseWF(); break;
                case 1: wf.DeleteDatabaseWF(); break;
                case 2: wf.ListDatabasesWF(); break;
                case 3: wf.CreateCollectionWF(); break;
                case 4: wf.DeleteCollectionWF(); break;
                case 5: wf.ListCollectionsWF(); break;
                case 6: wf.InsertOneOrManyWF(); break;
            }
        }

        /// <summary>
        /// runs database level operations.
        /// </summary>
        /// <param name="wf"> instance of workflows. </param>
        /// <param name="op"> the operation to complete. </param>
        public void RunMongo(MongoDBWorkflows wf, int op)
        {
            switch (op)
            {
                case 0: wf.CreateDatabaseWF(); break;
                case 1: wf.DeleteDatabaseWF(); break;
                case 2: wf.ListDatabasesWF(); break;
                case 3: wf.CreateCollectionWF(); break;
                case 4: wf.DeleteCollectionWF(); break;
                case 5: wf.ListCollectionsWF(); break;
                case 6: wf.InsertOneOrManyWF(); break;
            }
        }

        /// <summary>
        /// runs database level operations.
        /// </summary>
        /// <param name="wf"> instance of workflows. </param>
        /// <param name="op"> the operation to complete. </param>
        public void RunGraph(GraphWorkflows wf, int op)
        {
            switch (op)
            {
                case 0: wf.CreateDatabaseWF(); break;
                case 1: wf.DeleteDatabaseWF(); break;
                case 2: wf.ListDatabasesWF(); break;
                case 3: wf.CreateCollectionWF(); break;
                case 4: wf.DeleteCollectionWF(); break;
                case 5: wf.ListCollectionsWF(); break;
                case 6: wf.InsertOneOrManyWF(); break;
            }
        }

        /// <summary>
        /// runs database level operations.
        /// </summary>
        /// <param name="wf"> instance of workflows. </param>
        /// <param name="op"> the operation to complete. </param>
        public void RunTable(TableWorkflows wf, int op)
        {
            switch (op)
            {
                case 0: wf.CreateDatabaseWF(); break;
                case 1: wf.DeleteDatabaseWF(); break;
                case 2: wf.ListDatabasesWF(); break;
                case 3: wf.CreateCollectionWF(); break;
                case 4: wf.DeleteCollectionWF(); break;
                case 5: wf.ListCollectionsWF(); break;
                case 6: wf.InsertOneOrManyWF(); break;
            }
        }
    }
}

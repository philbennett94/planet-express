using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PlanetExpress.Interfaces;
using Microsoft.Azure.Management.CosmosDB.Fluent;
using Microsoft.Azure.Documents.Client;
using PlanetExpress.Utilities;

namespace PlanetExpress.Workflows
{
    class GraphWorkflows : DocumentDBWorkflows, IDatabaseWorkflow
    {
        /// <summary>
        /// Constructor. This class extends DocumentDBWorkflows for functionality, but uses a modified document client that is better suited for graph.
        /// </summary>
        /// <param name="account"></param>
        public GraphWorkflows(ICosmosDBAccount account) : base (account) {
            this.docdb.client = CreateClientTB(account);
        }

        /// <summary>
        /// creates a graph client.
        /// </summary>
        /// <param name="account"> a valid cosmos db account with graph as the default expirience. </param>
        /// <returns> document client suitable for use with the graph api. </returns>
        public DocumentClient CreateClientTB(ICosmosDBAccount account)
        {
            string authkey = account.ListKeys().PrimaryMasterKey;
            Uri endpoint = new Uri(account.DocumentEndpoint);
            return new DocumentClient(endpoint, authkey, new ConnectionPolicy
            {
                ConnectionMode = ConnectionMode.Direct,
                ConnectionProtocol = Protocol.Tcp
            });
        }
    }
}

using Microsoft.Azure.Management.CosmosDB.Fluent;
using Microsoft.Azure.Management.CosmosDB.Fluent.Models;
using Microsoft.Azure.Management.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent.Core;
using Microsoft.Azure.Management.ResourceManager.Fluent.Models;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.Rest.Azure.Authentication;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace PlanetExpress.Utilities
{
    class AccountUtils
    {
        public string subscriptionId;
        public string clientId;
        public string clientSecret;
        public string tenantId;
        public string apiAccessToken;
        public Dictionary<string, string[]> databaseAccountInformation;
        public IAzure azure;
        public IResourceManagementClient resourceManagementClient;

        /// <summary>
        /// Creates an instance of the AccountUtils class.
        /// </summary>
        /// <param name="pathToAzureAuthProperties"> string path to a local, valid, azureauth.properties file. </param>
        public AccountUtils(string pathToAzureAuthProperties)
        {
            List<string> fileList = File.ReadLines(pathToAzureAuthProperties).ToList();
            this.subscriptionId   = fileList[0].Split('=')[1];
            this.clientId         = fileList[1].Split('=')[1];
            this.clientSecret     = fileList[2].Split('=')[1];
            this.tenantId         = fileList[3].Split('=')[1];
            Task<string> tokenTask = GetAccessToken();
            this.apiAccessToken    = tokenTask.Result;
            this.databaseAccountInformation = GenerateDatabaseAccountInformation();
            this.azure = GenerateAzureClient();
            Task<IResourceManagementClient> resourceManagementTask = GenerateResourceManagementClient(this.tenantId, this.clientId, this.clientSecret, this.subscriptionId);
            this.resourceManagementClient                          = resourceManagementTask.Result;
        }

        /// <summary>
        /// Makes a rest api call to gathers azure account resource info, then parses it and returns a dictionary.
        /// </summary>
        /// <returns> dictionary containing database account information. </returns>
        private Dictionary<string, string[]> GenerateDatabaseAccountInformation() {
            string uri = @"https://management.azure.com/subscriptions/" + this.subscriptionId + @"/resources?api-version=2017-05-10";
            Task<string> responseJSONTask = MakeAzureRESTAPICall(uri, this.apiAccessToken);
            string responseJSON = responseJSONTask.Result;
            return ParseDatabaseAccounts(responseJSON);
        }

        /// <summary>
        /// parses a json string of azure account resource information into a dictionary containing cosmos db account information.
        /// </summary>
        /// <param name="jsonResponse"> databse account information </param>
        /// <returns> dictionary of account information. </returns>
        public Dictionary<string, string[]> ParseDatabaseAccounts(String jsonResponse)
        {
            JObject jsonObject = JObject.Parse(jsonResponse);
            Dictionary<string, string[]> databaseAccountInfo = new Dictionary<string, string[]>();
            foreach (var entry in jsonObject["value"])
            {
                string type = (string)entry["type"];
                if (type == "Microsoft.DocumentDb/databaseAccounts" || type == "Microsoft.DocumentDB/databaseAccounts")
                {
                    string id = (string)entry["id"];
                    string resourceGroup = id.Split('/')[4];
                    string name = (string)entry["name"];
                    string location = (string)entry["location"];
                    string dbModel;
                    if (entry["tags"]["defaultExperience"] != null)
                    {
                        dbModel = (string)entry["tags"]["defaultExperience"];
                    }
                    else
                    {
                        dbModel = "DocumentDB";
                    }
                    databaseAccountInfo[name] = new string[] { id, resourceGroup, type, location, dbModel };
                }
            }
            return databaseAccountInfo;
        }

        /// <summary>
        /// Creates an azure account client for azure account level operations.
        /// </summary>
        /// <returns> Azure account management client. </returns>
        public IAzure GenerateAzureClient()
        {
            var credentials = SdkContext.AzureCredentialsFactory.FromFile(Environment.GetEnvironmentVariable("AZURE_AUTH_LOCATION"));
            IAzure azure = Azure
                       .Configure()
                       .WithLogLevel(HttpLoggingDelegatingHandler.Level.Basic)
                       .Authenticate(credentials)
                       .WithDefaultSubscription();
            return azure;
        }

        /// <summary>
        /// Creates a dictionary of cosmos db account level information.
        /// </summary>
        /// <param name="account"> an existing cosmos db account. </param>
        /// <returns> a dictionary of account level attributes used for operations against the account and its resources. </returns>
        public Dictionary<string, string> GetAccountConnectionInformation(ICosmosDBAccount account)
        {
            DatabaseAccountListKeysResultInner databaseAccountListKeysResult = account.ListKeys();
            return new Dictionary<string, string>
            {
                {"Endpoint" , account.DocumentEndpoint},
                { "Primary Key", databaseAccountListKeysResult.PrimaryMasterKey },
                { "Secondary Key", databaseAccountListKeysResult.SecondaryMasterKey },
                { "Primary Read-only Key", databaseAccountListKeysResult.PrimaryReadonlyMasterKey},
                { "Secondary Read-only Key", databaseAccountListKeysResult.SecondaryReadonlyMasterKey}
            };
        }

        /// <summary>
        /// Adds database account information to the master dicionary for database account management.
        /// </summary>
        /// <param name="account"> a vlaid cosmos db account. </param>
        /// <returns> array containing account resource group and account name. </returns>
        public string[] AddDatabaseAccountInformation(ICosmosDBAccount account)
        {
            string name = account.Name;
            string resourceGroupName = account.ResourceGroupName;
            string dbModel;
            if (account.Tags.Keys.Contains("defaultExperience"))
            {
                dbModel = account.Tags["defaultExperience"];
            }
            else
            {
                dbModel = "DocumentDB";
            }
            this.databaseAccountInformation.Add(name, new string[] { account.Id, resourceGroupName, account.Type, account.RegionName, dbModel });
            return new string[] { resourceGroupName, name };
        }

        /// <summary>
        /// Removes information for a database account from the master dictionary. 
        /// </summary>
        /// <param name="accountName"> name of the database account to remove. </param>
        public void RemoveDatabaseAccountInformation(string accountName)
        {
            if (this.databaseAccountInformation.Keys.Contains(accountName))
            {
                this.databaseAccountInformation.Remove(accountName);
            }
            else
            {
                Console.WriteLine("Database account not found. It may have already been removed.");
            }
        }

        /// <summary>
        /// updates the defualt expirience of an item in the master dictionary.
        /// </summary>
        /// <param name="database"> name of the database. </param>
        /// <param name="defaultExpirience"> default expirience or new ex[pirience. </param>
        /// <returns> string denoting the default expirience of a database. </returns>
        public string UpdateDefaultExpirienceInformation(string database, string defaultExpirience)
        {
            this.databaseAccountInformation[database][4] = defaultExpirience;
            return defaultExpirience;
        }

        /// <summary>
        /// Creates an azure resource management client.
        /// </summary>
        /// <param name="tenantId"> tenant id of AAD application. </param>
        /// <param name="clientId"> client id of AAD application. </param>
        /// <param name="clientSecret"> password of AAD application. </param>
        /// <param name="subscriptionId"> subcription id which the AAD application is registered under. </param>
        /// <returns> async task resource management client. </returns>
        public async Task<IResourceManagementClient> GenerateResourceManagementClient(string tenantId, string clientId, string clientSecret, string subscriptionId)
        {
            var serviceCredentials = await ApplicationTokenProvider.LoginSilentAsync(tenantId, clientId, clientSecret);
            IResourceManagementClient resourceClient = new ResourceManagementClient(serviceCredentials);
            resourceClient.SubscriptionId = subscriptionId;
            return resourceClient;
        }

        /// <summary>
        /// Gets the access token for use when making REST API calls.
        /// </summary>
        /// <returns> api access token. </returns>
        public async Task<string> GetAccessToken()
        {
            string authContextURL = "https://login.windows.net/" + this.tenantId;
            AuthenticationContext authenticationContext = new AuthenticationContext(authContextURL);
            ClientCredential credential = new ClientCredential(clientId: this.clientId, clientSecret: this.clientSecret);
            AuthenticationResult result = await authenticationContext.AcquireTokenAsync(resource: "https://management.azure.com/", clientCredential: credential);
            if (result == null)
            {
                throw new InvalidOperationException("Failed to obtain token...");
            }
            string token = result.AccessToken;
            return token;
        }

        /// <summary>
        /// make an azure rest api call defined by string URI. 
        /// </summary>
        /// <param name="URI"> api call to make. </param>
        /// <param name="token"> token that authenticates the call. </param>
        /// <returns> async task string -> result of api call. </returns>
        public static async Task<string> MakeAzureRESTAPICall(string URI, String token)
        {
            string result;
            Uri uri = new Uri(String.Format(URI));
            HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(uri);
            httpWebRequest.Headers.Add(HttpRequestHeader.Authorization, "Bearer " + token);
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "GET";
            try
            {
                WebResponse httpResponse = await httpWebRequest.GetResponseAsync();
                using (StreamReader streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    result = streamReader.ReadToEnd();
                }
                return result;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message); 
                return null;
            }
        }

        /// <summary>
        /// Creates a cosmos db account. All params with default values will be autogenerated if no value is provided.
        /// </summary>
        /// <param name="resourceName"> name of the resource to create. </param>
        /// <param name="provisioningRegion"> region in which to provision the resource. </param>
        /// <param name="resourceGroupName"> name of resource group to assign the resource to. </param>
        /// <param name="databaseModel"> the api of the new cosmos db resource (SQL, mongo, table, graph...). </param>
        /// <returns> new cosmos db account </returns>
        public async Task<ICosmosDBAccount> CreateCosmosDBAccount(string resourceName = null, string provisioningRegion = null, string resourceGroupName = null, string databaseModel = null)
        {
            string rName        = resourceName ?? SdkContext.RandomResourceName("docDb", 10);
            Region pRegion      = YieldRegion(provisioningRegion);
            string dbModel      = databaseModel ?? DatabaseAccountKind.GlobalDocumentDB;

            try
            {
                Console.WriteLine("Creating the database account [{0}]. This process can take several minutes...", rName);
                if (resourceGroupName == null)
                {
                    string rgName = SdkContext.RandomResourceName("docDbToolBox", 10);
                    ICosmosDBAccount cosmosDBAccount = await this.azure.CosmosDBAccounts.
                        Define(rName)
                        .WithRegion(pRegion)
                        .WithNewResourceGroup(rgName)
                        .WithKind(dbModel)
                        .WithSessionConsistency()
                        .WithWriteReplication(pRegion)
                        .CreateAsync();
                    return cosmosDBAccount;
                }
                else
                {
                    ICosmosDBAccount cosmosDBAccount = await this.azure.CosmosDBAccounts.Define(rName)
                        .WithRegion(pRegion)
                        .WithExistingResourceGroup(resourceGroupName)
                        .WithKind(dbModel)
                        .WithSessionConsistency()
                        .WithWriteReplication(pRegion)
                        .CreateAsync();
                    return cosmosDBAccount;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Something went wrong... creating a default resource");
                Console.WriteLine(e.Message);
                return await CreateCosmosDBAccount();
            }
        }

        /// <summary>
        /// sets the default expirience (api) of a cosmos db resource.
        /// </summary>
        /// <param name="defaultExpirience"> the new default expirience tag value (docdc, mongodb, graph, table...). </param>
        /// <param name="resourceGroup"> the name of the resource group that contains the cosmos resource. </param>
        /// <param name="resourceName"> the name of the resource for which the default expirience will be changed. </param>
        public async void SetDefaultExpirience(string defaultExpirience, string resourceGroup, string resourceName)
        {
            try
            {
                GenericResourceInner genericResourceI = await this.resourceManagementClient.Resources.GetAsync(resourceGroup, "Microsoft.DocumentDB", "", "databaseAccounts", resourceName, "2016-03-31");
                genericResourceI.Tags.Add("defaultExperience", defaultExpirience);
                await this.resourceManagementClient.Resources.CreateOrUpdateAsync(resourceGroup, "Microsoft.DocumentDB", "", "databaseAccounts", resourceName, "2016-03-31", genericResourceI);
            }
            catch (Exception e)
            {
                Console.WriteLine("Something went wrong while trying to set the defaultExpirience tag... set the tag through the portal or retry.");
                Console.WriteLine(e.Message);
                Console.WriteLine("The default expirience was not changed for the resource [{0}].", resourceName);
            }
        }

        /// <summary>
        /// Deletes a cosmos db account.
        /// </summary>
        /// <param name="cosmosDbAccount"> a valid/existing cosmos db account. </param>
        public async void DeleteDatabaseAccount(ICosmosDBAccount cosmosDbAccount)
        {
            try
            {
                string name = cosmosDbAccount.Name;
                string id   = cosmosDbAccount.Id;
                Console.WriteLine("Deleting the database account [{0}]. This process can take several minutes...", cosmosDbAccount.Name);
                await this.azure.CosmosDBAccounts.DeleteByIdAsync(cosmosDbAccount.Id);
            }
            catch (Exception e)
            {
                Console.WriteLine("Azure encountered an error, but it is likely that your account was still deleted. Please confirm through portal.");
                Console.WriteLine(e.Message);
            }
        }

        /// <summary>
        /// Get a cosmos db account to make requests against.
        /// </summary>
        /// <param name="accountName"> account name of an existing account. </param>
        /// <returns> async task -> yields CosmosDBAccount </returns>
        public async Task<ICosmosDBAccount> SelectAccountByName(string accountName)
        {
            return await this.azure.CosmosDBAccounts.GetByIdAsync(this.databaseAccountInformation[accountName][0]);
        }

        /// <summary>
        /// returns an azure region based on a human readable string representation of the region (what's shown in the az portal). 
        /// </summary>
        /// <param name="region"> human readable name of the region. </param>
        /// <returns> azure Region object. </returns>
        public Region YieldRegion(string region) {
            try
            {
                Dictionary<string, Region> azureRegions = new Dictionary<string, Region> {
                    { "East US", Region.USEast },
                    { "East US 2", Region.USEast2},
                    { "Central US", Region.USCentral},
                    { "North Central US", Region.USNorthCentral},
                    { "South Central US", Region.USSouthCentral},
                    { "West Central US", Region.USWestCentral},
                    { "West US", Region.USWest},
                    { "West US 2", Region.USWest2},
                    { "Canada East", Region.CanadaEast},
                    { "Canada Central", Region.CanadaCentral}
                };
                return azureRegions[region];
            }
            catch (Exception e)
            {
                Console.WriteLine("Could not convert the region provided. Yielding default region East US.");
                Console.WriteLine(e.Message);
                return Region.USEast;
            }
        }
    }
}

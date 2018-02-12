using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PlanetExpress.Utilities;
using Microsoft.Azure.Management.CosmosDB.Fluent;

namespace PlanetExpress.Workflows
{
    class AccountWorkflows
    {
        public AccountUtils acctUtil;
        public AccountWorkflows(string pathToAzureAuth) {
            this.acctUtil = new AccountUtils(pathToAzureAuth);
        }

        /// <summary>
        /// Creates a cosmos db database account and prompts for necesary inputs.
        /// </summary>
        public void CreateDatabaseAccountWF() {
            Console.WriteLine("The following prompts are optional. Press <enter>, without typing a response, when prompted to yield the default settings for an account.");
            string resourceName = GeneralUtils.PromptInput("Please enter a name for the account");
            string provisioningRegion = GeneralUtils.PromptInput("Please enter an azure region where the account will be located");
            string resourceGroupName = GeneralUtils.PromptInput("Please enter the name of a resource group");
            string databaseModel = GeneralUtils.PromptInput("Please enter the database model. Enter MonogoDB, or DocumentDB for all other database models");
            string databaseExpirience = GeneralUtils.PromptInput("Please enter the default expirience for your Database. Enter one of the following... DocumentDB, MongoDB, Graph, Table");
            Task<ICosmosDBAccount> newAccountInfoTask = this.acctUtil.CreateCosmosDBAccount(
                GeneralUtils.OptionalArgumentHelper(resourceName),
                GeneralUtils.OptionalArgumentHelper(provisioningRegion),
                GeneralUtils.OptionalArgumentHelper(resourceGroupName),
                GeneralUtils.OptionalArgumentHelper(databaseModel)
            );
            string[] newAccountInfo = this.acctUtil.AddDatabaseAccountInformation(newAccountInfoTask.Result);
            this.acctUtil.SetDefaultExpirience(databaseExpirience, newAccountInfo[0], newAccountInfo[1]);
            string dbexp =  this.acctUtil.UpdateDefaultExpirienceInformation(newAccountInfo[1], databaseExpirience);
            Console.WriteLine("The database account {0} was created with the default expirience {1}. Changes in default expirience may not immediately be reflected in the portal.", newAccountInfo[1], dbexp);
        }

        /// <summary>
        /// Deletes a database account and prompts for necesary inputs.
        /// </summary>
        public void DeleteDatabaseAccountWF() {
            string name = GeneralUtils.PromptInput("Please enter the name of a database account");
            Task<ICosmosDBAccount> accountTask = this.acctUtil.SelectAccountByName(name);
            ICosmosDBAccount account = accountTask.Result;
            this.acctUtil.DeleteDatabaseAccount(account);
            acctUtil.RemoveDatabaseAccountInformation(name);
            Console.WriteLine("The database account {0} was removed. This change may not be reflected in the portal for several minutes.", name);
            Console.WriteLine("Please wait a minute for this to operation to go through before moving on to the next operation, otherwise azure will not process the request.");
        }

        /// <summary>
        /// Lists database account information and prompts for necesary inputs.
        /// </summary>
        public void ListDatabaseAccountInformationWF()
        {
            string name = GeneralUtils.PromptInput("Please enter the name of a database account");
            Task<ICosmosDBAccount> accountTask = this.acctUtil.SelectAccountByName(name);
            ICosmosDBAccount account = accountTask.Result;
            Dictionary<string, string> accountConnectionInformation = this.acctUtil.GetAccountConnectionInformation(account);
            string[] headers = { "Account Connection Information" };
            List<string[]> lines = GeneralUtils.CreateMenuHeader(headers);
            foreach (string connectionItem in accountConnectionInformation.Keys)
            {
                lines.Add(new string[] { connectionItem + " : " + accountConnectionInformation[connectionItem] });
            }
            Console.WriteLine(GeneralUtils.PadElementsInLines(lines));
        }

        /// <summary>
        /// Lists database accounts and prompts for necesary inputs.
        /// </summary>
        public void ListDatabaseAccountsWF()
        {
            string[] headers = { "Databse Account Name", "Database Model", "Database Location", "Database Resource Group" };
            List<string[]> lines = GeneralUtils.CreateMenuHeader(headers);
            foreach (string databaseAccountName in acctUtil.databaseAccountInformation.Keys)
            {
                lines.Add(new string[]
                {
                    databaseAccountName,
                    acctUtil.databaseAccountInformation[databaseAccountName][4],
                    acctUtil.databaseAccountInformation[databaseAccountName][3],
                    acctUtil.databaseAccountInformation[databaseAccountName][1] });
            }
            Console.WriteLine(GeneralUtils.PadElementsInLines(lines));
        }
    }
}

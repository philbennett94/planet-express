using System;
using System.Threading.Tasks;
using PlanetExpress.Utilities;
using Microsoft.Azure.Management.CosmosDB.Fluent;
using PlanetExpress.Workflows;

namespace PlanetExpress
{
    class Program
    {
        String[] planet =
        {
            @"                             `. ___                          ",
            @"                    __,' __`.                _..----....____ ",
            @"        __...--.'``;.   ,.   ;``--..__     .'    ,-._    _.-'",
            @"  _..-''-------'   `'   `'   `'     O ``-''._   (,;') _,'    ",
            @",'________________                          \`-._`-','       ",
            @" `._              ```````````------...___   '-.._'-:         ",
            @"    ```--.._      ,.                     ````--...__\-.      ",
            @"            `.--. `-`                       ____    |  |`    ",
            @"              `. `.                       ,'`````.  ;  ;`    ",
            @"                `._`.        __________   `.      \'__/`     ",
            @"                   `-:._____/______/___/____`.     \  `      ",
            @"                               |       `._    `.    \        ",
            @"                               `._________`-.   `.   `.___   ",
            @"                                             SSt  `------'   "
        };
        String[] headline =
        {
            @" _____  _                  _     ______                              ",
            @"|  __ \| |                | |   |  ____|                             ",
            @"| |__) | | __ _ _ __   ___| |_  | |__  __  ___ __  _ __ ___  ___ ___ ",
            @"|  ___/| |/ _` | '_ \ / _ \ __| |  __| \ \/ / '_ \| '__/ _ \/ __/ __|",
            @"| |    | | (_| | | | |  __/ |_  | |____ >  <| |_) | | |  __/\__ \__ \",
            @"|_|    |_|\__,_|_| |_|\___|\__| |______/_/\_\ .__/|_|  \___||___/___/",
            @"                                            | |  Automation Tools",
            @"                                            |_|  Azure CosmosDB"
        };
        string[] greetingMenu =
        {
            "Manage Accounts",
            "Manage Resources",
            "Help",
            "Exit"
        };

        string[] accountOperationsMenu = 
        {
            "Create a Database Account",
            "Delete a database account",
            "List All Database Accounts",
            "List Database Account Information",
            "Help",
            "Back",
            "Exit"
        };

        string[] databaseOperationsMenu =
        {
            "Create a Database",
            "Delete a Database",
            "List All Databases",
            "Create a Collection",
            "Delete a Collection",
            "List All Collections",
            "Populate a collection with data",
            "Help",
            "Back",
            "Exit"
        };

        /// <summary>
        /// Run the program.
        /// </summary>
        static void Main(String[] args)
        {
           Program p = new Program();
            p.Run();
        }

        /// <summary>
        /// Run the program logic. Serve menus, execute requests, etc...
        /// </summary>
        public void Run() {
            Console.ForegroundColor = ConsoleColor.Blue;
            PrintArray(planet);
            Console.ForegroundColor = ConsoleColor.Green;
            PrintArray(headline);
            Console.ForegroundColor = ConsoleColor.White;
            String pathToAzureAuth = GeneralUtils.PromptInput("Please enter the file path to youre azureauth.properties file (if you don't know what that is press enter and you will be directed to a setup guide)");
            String envCheck = CheckEnvironment(pathToAzureAuth);
            if (envCheck == "OK")
            {
                ResourceManager manager = new ResourceManager(pathToAzureAuth);
                bool go = true;
                int menuChoice = 0;
                while (go == true)
                {
                    int innerChoice;
                    if (menuChoice == 0)
                    {
                        innerChoice = MenuExec(greetingMenu);
                        if (innerChoice == 0)
                        {
                            menuChoice = 1;
                        }
                        else if (innerChoice == 1)
                        {
                            menuChoice = 2;
                        }
                        else if (innerChoice == 2)
                        {
                            Help();
                        }
                        else if (innerChoice == 3)
                        {
                            Environment.Exit(0);
                        }
                        else
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("Not a valid menu choice, try again...");
                            Console.ForegroundColor = ConsoleColor.White;
                        }
                    }
                    else if (menuChoice == 1)
                    {
                        innerChoice = MenuExec(accountOperationsMenu);
                        if (innerChoice == 6)
                        {
                            Environment.Exit(0);
                        }
                        else if (innerChoice == 5)
                        {
                            menuChoice = 0;
                        }
                        else if (innerChoice == 4)
                        {
                            Help();
                        }
                        else if (innerChoice >= 0 && innerChoice < 4)
                        {
                            InteractAccount(manager, innerChoice);
                        }
                        else
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("Not a valid menu choice, try again...");
                            Console.ForegroundColor = ConsoleColor.White;
                        }
                    }
                    else if (menuChoice == 2)
                    {
                        innerChoice = MenuExec(databaseOperationsMenu);
                        if (innerChoice == 9)
                        {
                            Environment.Exit(0);
                        }
                        else if (innerChoice == 8)
                        {
                            menuChoice = 0;
                        }
                        else if (innerChoice == 7)
                        {
                            Help();
                        }
                        else if (innerChoice >= 0 && innerChoice < 7)
                        {
                            InteractResource(manager, innerChoice);
                        }
                        else
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("Not a valid menu choice, try again...");
                            Console.ForegroundColor = ConsoleColor.White;
                        }
                    }
                }
            }
            else {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Please ensure that you have properly configured an AZURE_AUTH_LOCATION environment variable.");
                Console.WriteLine("Follow the steps in the linked article under section \"Set up authentication\" to properly generate the AZURE-AUTH_LOCATION environment variable: https://docs.microsoft.com/en-us/dotnet/azure/dotnet-sdk-azure-get-started?view=azure-dotnet");
                Console.WriteLine("Once your environment is properly configured, please run the Toolbox again. Press enter to exit.");
                Console.ReadLine();
            }
        }

        /// <summary>
        /// checks to make sure environment variables are set/ can be set correctly.
        /// </summary>
        /// <param name="pathToAzureAuth"> file path to an azureauth.properties file. </param>
        /// <returns> string denoting whether or not the env var is set. </returns>
        public String CheckEnvironment(String pathToAzureAuth) {
            Console.WriteLine("Checking for required environment variable AZURE_AUTH_LOCATION@Location {0}...", pathToAzureAuth);
            Environment.SetEnvironmentVariable("AZURE_AUTH_LOCATION", pathToAzureAuth);
            if (Environment.GetEnvironmentVariable("AZURE_AUTH_LOCATION") != null)
            {
                return "OK";
            }
            else {
                return "AZURE_AUTH_LOCATION ENVIRONMENT VARIABLE IS MISSING...";
            }
        }

        /// <summary>
        /// print the help menu.. 
        /// </summary>
        public void Help() {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Welcome to Planet Express. This program was created to help automate common tasks related to CosmosDB and provide examples for working with Cosmos resources in C#.");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("--------------------");
            Console.WriteLine("Dependencies: ");
            Console.WriteLine("--------------------");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("\t Planet Express requires that an azureauth.properties file be present for some functionality.");
            Console.WriteLine("\t Please select the first time user option in the start up menu for help generating this file.");
            Console.WriteLine("\t For more information about azure authentication please visit: https://docs.microsoft.com/en-us/dotnet/azure/dotnet-sdk-azure-get-started?view=azure-dotnet");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("--------------------");
            Console.WriteLine("Functionality: ");
            Console.WriteLine("--------------------");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("\t > Manage CosmosDB resources (Databses, Collections => list, create, delete).");
            Console.WriteLine("\t > Ingest data (with time and size tracking).");
            Console.WriteLine("\t > Scale up and down.");
            Console.WriteLine("\t > Query data (with time and size tracking).");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("--------------------");
            Console.WriteLine("Menu Items: ");
            Console.WriteLine("--------------------");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("\t Top Level Menu: ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("\t > [0]: Manage Accounts... Interact with Cosmos DB account level objects");
            Console.WriteLine("\t > [1]: Manage Resources... Interact with Cosmos DB resource level objects");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("\t\t Manage Accounts Menu: ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("\t\t > [0]: Create a Database Account... creates a database account with any database model except cassandra");
            Console.WriteLine("\t\t > [1]: Delete a database account... deletes a database account");
            Console.WriteLine("\t\t > [2]: List All Database Accounts... lists all cosmos db accounts for a given azure subscription");
            Console.WriteLine("\t\t > [3]: List Database Account Information... lists account keys and endpoint for a given account");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("\t\t Manage Resources Menu: ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("\t\t > [0]: Create a Database... create a database within a given database account");
            Console.WriteLine("\t\t > [1]: Delete a Database... delete a database within a given database account");
            Console.WriteLine("\t\t > [2]: List All Databases... list all databases within a given database account");
            Console.WriteLine("\t\t > [3]: Create a Collection... create a collection within a given database");
            Console.WriteLine("\t\t > [4]: Delete a Collection... delete a collection within a given database");
            Console.WriteLine("\t\t > [5]: List All Collections... list all collections within a given database");
            Console.WriteLine("\t\t > [6]: Populate a collection with data... insert one or more items into a given database");
        }

        /// <summary>
        /// prints an array where each item is on its own line.
        /// </summary>
        /// <param name="inArray"> array or strings. </param>
        public void PrintArray(String[] inArray)
        {
            foreach (String s in inArray) {
                Console.WriteLine(s);
            }
        }

        /// <summary>
        /// prints a menu from an array of menu items, and pairs each item with an integer to support user choice. The user choice is captured or rejected.
        /// </summary>
        /// <param name="menuItems"> an array of strings where each item is a selection on the menu. </param>
        /// <returns> user choice as an integer, or recurse. </returns>
        public int MenuExec(String[] menuItems)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Please select an action from the menu by entering the corresponding number when prompted.");
            Console.ForegroundColor = ConsoleColor.White;
            for (int i = 0; i < menuItems.Length; i++) {
                Console.WriteLine("[" + i + "]: " + menuItems[i]);
            }
            String selection = GeneralUtils.PromptInput("Please enter your selection");
            int iSelection;
            if (int.TryParse(selection, out iSelection))
            {
                return iSelection;
            }
            else
            {
                Console.WriteLine("Your input could not be cast to an integer and is therefore invalid. Please try again...");
                return MenuExec(menuItems);
            }
        }

        /// <summary>
        /// run an account level operation.
        /// </summary>
        /// <param name="manager"> an instance of resource manager. </param>
        /// <param name="op"> the menu choice which denotes the operation to be completed. </param>
        public void InteractAccount(ResourceManager manager, int op)
        {
            try
            {
                manager.RunAccount(manager.accountWorkflow, op);
            }
            catch (Exception e)
            {
                Console.WriteLine("Something went wrong while executing an account level operation...");
                Console.WriteLine(e.Message);
            }
            
        }

        /// <summary>
        /// run a resource level operation.
        /// </summary>
        /// <param name="manager"> an instance of resource manager. </param>
        /// <param name="op"> the menu choice which denotes the operation to be completed. </param>
        public void InteractResource(ResourceManager manager, int op)
        {
            try
            {
                string accountName = GeneralUtils.PromptInput("Please enter the name of the database account that contains the resources you want to interact with");
                char lookUp = manager.CheckForExistingWorkFlow(accountName);
                if (lookUp != 'n')
                {
                    switch (lookUp)
                    {
                        case 'd': manager.RunDoc(manager.docdbWorkflows[accountName], op); break;
                        case 'm': manager.RunMongo(manager.mongodbWorkflows[accountName], op); break;
                        case 'g': manager.RunGraph(manager.graphWorkflows[accountName], op); break;
                        case 't': manager.RunTable(manager.tableWorkflows[accountName], op); break;
                    }
                }
                else
                {
                    string model = manager.accountWorkflow.acctUtil.databaseAccountInformation[accountName][4];
                    Task<ICosmosDBAccount> accountTask = manager.accountWorkflow.acctUtil.SelectAccountByName(accountName);
                    ICosmosDBAccount account = accountTask.Result;
                    switch (model)
                    {
                        case "DocumentDB":
                            DocumentDBWorkflows doc = manager.GenerateDocDBWF(account, accountName);
                            manager.RunDoc(doc, op);
                            break;
                        case "MongoDB":
                            MongoDBWorkflows mongo = manager.GenerateMongoDBWF(account, accountName);
                            manager.RunMongo(mongo, op);
                            break;
                        case "Graph":
                            GraphWorkflows graph = manager.GenerateGraphWF(account, accountName);
                            manager.RunGraph(graph, op);
                            break;
                        case "Table":
                            TableWorkflows table = manager.GenerateTableWF(account, accountName);
                            manager.RunTable(table, op);
                            break;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Something went wrong while trying to process the request...");
                Console.WriteLine(e.Message);
            }
        }
    }
}
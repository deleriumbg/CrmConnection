using log4net;
using Microsoft.Xrm.Sdk;
using System;
using System.Reflection;


namespace CRMConnection
{
    class Program
    {
        static IOrganizationService _service;
        private static readonly ILog _log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        static void Main(string[] args)
        {
            _log.Info("Initializing connection...");

            try
            {
                var crmContext = new Connection();
                _service = crmContext.Connect();

                if (crmContext.IsConnected)
                {
                    Console.WriteLine("Connection Established Successfully...");
                    _log.Info("Connection Established Successfully...");

                    while (true)
                    {
                        //Get all active contact Full name with application ID (example in UOP system: A815280) and Student ID (example in UOP system: S8152) using each query style
                        Console.WriteLine("-----------------------------------------------");
                        Console.WriteLine("| Choose a method to execute your query:      |\n" +
                                          "| For FetchXML press 1                        |\n" +
                                          "| For Query Expression press 2                |\n" +
                                          "| For Early Bound entities using LINQ press 3 |\n" +
                                          "| To Exit press 4                             |");
                        Console.WriteLine("-----------------------------------------------");
                        var userInput = Console.ReadLine();
                        var queries = new Queries(crmContext);

                        switch (userInput)
                        {
                            case "1":
                                //Executing the query using FetchXML
                                queries.FetchXmlQuery();
                                break;
                            case "2":
                                //Executing the query using QueryExpression
                                queries.QueryExpressionQuery();
                                break;
                            case "3":
                                //Executing the query using Early Bound and LINQ
                                queries.EarlyBoundLinqQuery();
                                break;
                            case "4":
                                //Exiting program
                                _log.Info("Exiting program due to user request");
                                return;
                            default:
                                Console.WriteLine("Invalid user input");
                                break;
                        }
                    }
                }
                else
                {
                    Console.WriteLine("Failed to Establish Connection!");
                    _log.Error("Failed to Establish Connection!");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception caught in main method - {ex.Message}");
                _log.Error($"Exception caught in main method - {ex.Message}");
            }
        }
    }
}

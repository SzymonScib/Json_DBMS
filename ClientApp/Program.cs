using Serilog;
using QueryEngine;

namespace ClientApp
{
    class Program
    {
        static void Main(string[] args){
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .CreateLogger();

            var logger = Log.Logger;

            string storagePath;
            string configFilePath = "config.txt";

            if (File.Exists(configFilePath)){
                storagePath = File.ReadAllText(configFilePath).Trim();
                if (string.IsNullOrEmpty(storagePath))
                {
                    logger.Error("Config file is empty. Please provide a storage path.");
                    storagePath = PromptForStoragePath();
                    File.WriteAllText(configFilePath, storagePath);
                }
            }
            else if (args.Length == 0){
                
                storagePath = PromptForStoragePath();
                File.WriteAllText(configFilePath, storagePath);
            }
            else{
                storagePath = args[0];
            }

            if(!Directory.Exists(storagePath)){
                Directory.CreateDirectory(storagePath);
                Console.WriteLine($"Created storage directory at {storagePath}");
            }

            var queryEngine = new SqlQueryEngine(storagePath, logger);

            while(true){
                Console.Write("sql> ");
                var query = Console.ReadLine();
                if (string.IsNullOrEmpty(query)) continue;
                if (query.ToLower() == "exit") break;

                var result = queryEngine.ExecuteQuery(query);
                Console.WriteLine(result);
            }

            Log.Information("Shutting  down");

            
        }

        private static string PromptForStoragePath(){
            Console.WriteLine("Please provide a storage path");
            Console.Write("Storage Path: ");
            string storagePath = Console.ReadLine();

            if (string.IsNullOrEmpty(storagePath))
            {
                Log.Logger.Error("No storage path provided");
                Environment.Exit(1);
            }

            return storagePath;
        }
    }
}
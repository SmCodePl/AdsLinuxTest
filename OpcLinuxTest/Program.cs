using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpcLinuxTest;


Host.CreateDefaultBuilder(args)
        .UseSystemd()
        .ConfigureServices((ctx,services) => ServiceRegistration.AddInfrastructure(services))
        .ConfigureAppConfiguration((hostingContext, config) =>
        {
            config.Sources.Clear();// Clear all default config sources 
            config.AddJsonFile("appSettings.json"); // Use Appsettings Configuration
            //config.AddEnvironmentVariables("ENV_"); // Use Environment variables
                    //config.AddCommandLine(args); // Use Command Line
                    //config.AddStaticRoutesXmlConfiguration(); // Overriding settings with StaticRoutes.Xml 
        })
        .ConfigureLogging(logging => 
        {
            logging.ClearProviders();
            logging.AddConsole();
        })
        .Build()
        .Run();


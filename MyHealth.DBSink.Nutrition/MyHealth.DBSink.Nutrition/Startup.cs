using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MyHealth.Common;
using MyHealth.DBSink.Nutrition;
using MyHealth.DBSink.Nutrition.Functions;
using MyHealth.DBSink.Nutrition.Services;
using System.IO;

[assembly: FunctionsStartup(typeof(Startup))]
namespace MyHealth.DBSink.Nutrition
{
    public class Startup : FunctionsStartup
    {
        private static ILogger _logger;

        public override void Configure(IFunctionsHostBuilder builder)
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

            builder.Services.AddSingleton<IConfiguration>(config);
            _ = builder.Services.AddLogging();
            _logger = new LoggerFactory().CreateLogger(nameof(CreateNutritionDocument));

            builder.Services.AddSingleton(sp =>
            {
                IConfiguration config = sp.GetService<IConfiguration>();
                return new CosmosClient(config["CosmosDBConnectionString"]);
            });

            builder.Services.AddSingleton<IServiceBusHelpers>(sp =>
            {
                IConfiguration config = sp.GetService<IConfiguration>();
                return new ServiceBusHelpers(config["ServiceBusConnectionString"]);
            });
            builder.Services.AddScoped<INutritionDbService, NutritionDbService>();
        }
    }
}

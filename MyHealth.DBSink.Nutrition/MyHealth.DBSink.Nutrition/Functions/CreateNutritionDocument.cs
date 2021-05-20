using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MyHealth.Common;
using MyHealth.DBSink.Nutrition.Services;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;
using mdl = MyHealth.Common.Models;

namespace MyHealth.DBSink.Nutrition.Functions
{
    public class CreateNutritionDocument
    {
        private readonly IConfiguration _configuration;
        private readonly INutritionDbService _nutritionDbService;
        private readonly IServiceBusHelpers _serviceBusHelpers;

        public CreateNutritionDocument(
            IConfiguration configuration,
            INutritionDbService nutritionDbService,
            IServiceBusHelpers serviceBusHelpers)
        {
            _configuration = configuration;
            _nutritionDbService = nutritionDbService;
            _serviceBusHelpers = serviceBusHelpers;
        }

        [FunctionName(nameof(CreateNutritionDocument))]
        public async Task Run([ServiceBusTrigger("myhealthnutritiontopic", "myhealthnutritionsubscription", Connection = "ServiceBusConnectionString")] string mySbMsg, ILogger logger)
        {
            try
            {
                var nutritionDocument = JsonConvert.DeserializeObject<mdl.Nutrition>(mySbMsg);

                logger.LogInformation($"Checking database for existing Food Log record dated {nutritionDocument.NutritionDate}");
                var existingNutritionLog = await _nutritionDbService.RetrieveNutritionEnvelope(nutritionDocument.NutritionDate);
                if (existingNutritionLog == null)
                {
                    logger.LogInformation($"No existing record for {nutritionDocument.NutritionDate}. Adding new log to database");
                    await _nutritionDbService.AddNutritionDocument(nutritionDocument);
                }
                else
                {
                    logger.LogInformation($"Food log for date {nutritionDocument.NutritionDate} exists. Attempting to update record with latest values");
                    await _nutritionDbService.ReplaceNutritionDocument(existingNutritionLog);
                }

                logger.LogInformation($"Nutrition document with {nutritionDocument.NutritionDate} has been persisted");
            }
            catch (Exception ex)
            {
                logger.LogError($"Exception thrown in {nameof(CreateNutritionDocument)}: {ex}", ex);
                await _serviceBusHelpers.SendMessageToQueue(_configuration["ExceptionQueue"], ex);
                throw ex;
            }
        }
    }
}

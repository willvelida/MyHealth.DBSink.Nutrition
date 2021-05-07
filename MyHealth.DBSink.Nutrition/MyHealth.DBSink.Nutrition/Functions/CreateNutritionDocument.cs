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

                await _nutritionDbService.AddNutritionDocument(nutritionDocument);
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

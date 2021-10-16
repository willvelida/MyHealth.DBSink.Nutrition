using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MyHealth.Common;
using MyHealth.DBSink.Nutrition.Services.Interfaces;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;
using mdl = MyHealth.Common.Models;

namespace MyHealth.DBSink.Nutrition.Functions
{
    public class CreateNutritionDocument
    {
        private readonly IConfiguration _configuration;
        private readonly INutritionService _nutritionService;
        private readonly IServiceBusHelpers _serviceBusHelpers;

        public CreateNutritionDocument(
            IConfiguration configuration,
            INutritionService nutritionService,
            IServiceBusHelpers serviceBusHelpers)
        {
            _configuration = configuration;
            _nutritionService = nutritionService;
            _serviceBusHelpers = serviceBusHelpers;
        }

        [FunctionName(nameof(CreateNutritionDocument))]
        public async Task Run([ServiceBusTrigger("myhealthnutritiontopic", "myhealthnutritionsubscription", Connection = "ServiceBusConnectionString")] string mySbMsg, ILogger logger)
        {
            try
            {
                var nutrition = JsonConvert.DeserializeObject<mdl.Nutrition>(mySbMsg);
                var nutritionEnvelope = _nutritionService.MapNutritionToNutritionEnvelope(nutrition);
                await _nutritionService.AddNutritionDocument(nutritionEnvelope);
                logger.LogInformation($"Nutrition document with {nutritionEnvelope.Date} has been persisted");
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

using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MyHealth.Common;
using MyHealth.DBSink.Nutrition.Mappers;
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
        private readonly INutritionEnvelopeMapper _nutritionEnvelopeMapper;
        private readonly IServiceBusHelpers _serviceBusHelpers;

        public CreateNutritionDocument(
            IConfiguration configuration,
            INutritionDbService nutritionDbService,
            INutritionEnvelopeMapper nutritionEnvelopeMapper,
            IServiceBusHelpers serviceBusHelpers)
        {
            _configuration = configuration;
            _nutritionDbService = nutritionDbService;
            _nutritionEnvelopeMapper = nutritionEnvelopeMapper;
            _serviceBusHelpers = serviceBusHelpers;
        }

        [FunctionName(nameof(CreateNutritionDocument))]
        public async Task Run([ServiceBusTrigger("myhealthnutritiontopic", "myhealthnutritionsubscription", Connection = "ServiceBusConnectionString")] string mySbMsg, ILogger logger)
        {
            try
            {
                var nutrition = JsonConvert.DeserializeObject<mdl.Nutrition>(mySbMsg);
                var nutritionEnvelope = _nutritionEnvelopeMapper.MapNutritionToNutritionEnvelope(nutrition);
                await _nutritionDbService.AddNutritionDocument(nutritionEnvelope);
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

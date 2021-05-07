using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;
using mdl = MyHealth.Common.Models;

namespace MyHealth.DBSink.Nutrition.Services
{
    public class NutritionDbService : INutritionDbService
    {
        private readonly CosmosClient _cosmosClient;
        private readonly Container _myHealthContainer;
        private readonly IConfiguration _configuration;

        public NutritionDbService(
            CosmosClient cosmosClient,
            IConfiguration configuration)
        {
            _configuration = configuration;
            _cosmosClient = cosmosClient;
            _myHealthContainer = _cosmosClient.GetContainer(_configuration["DatabaseName"], _configuration["ContainerName"]);
        }

        public async Task AddNutritionDocument(mdl.Nutrition nutrition)
        {
            try
            {
                ItemRequestOptions itemRequestOptions = new ItemRequestOptions
                {
                    EnableContentResponseOnWrite = false
                };

                mdl.NutritionEnvelope nutritionEnvelope = new mdl.NutritionEnvelope
                {
                    Id = Guid.NewGuid().ToString(),
                    Nutrition = nutrition,
                    DocumentType = "Nutrition"
                };

                await _myHealthContainer.CreateItemAsync(
                    nutritionEnvelope,
                    new PartitionKey(nutritionEnvelope.DocumentType),
                    itemRequestOptions);

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}

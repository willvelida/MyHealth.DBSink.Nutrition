using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using MyHealth.Common.Models;
using MyHealth.DBSink.Nutrition.Repository.Interfaces;
using System;
using System.Threading.Tasks;

namespace MyHealth.DBSink.Nutrition.Repository
{
    public class NutritionRepository : INutritionRepository
    {
        private readonly CosmosClient _cosmosClient;
        private readonly Container _myHealthContainer;
        private readonly IConfiguration _configuration;

        public NutritionRepository(
            CosmosClient cosmosClient,
            IConfiguration configuration)
        {
            _configuration = configuration;
            _cosmosClient = cosmosClient;
            _myHealthContainer = _cosmosClient.GetContainer(_configuration["DatabaseName"], _configuration["ContainerName"]);
        }

        public async Task CreateNutrition(NutritionEnvelope nutritionEnvelope)
        {
            try
            {
                ItemRequestOptions itemRequestOptions = new ItemRequestOptions
                {
                    EnableContentResponseOnWrite = false
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

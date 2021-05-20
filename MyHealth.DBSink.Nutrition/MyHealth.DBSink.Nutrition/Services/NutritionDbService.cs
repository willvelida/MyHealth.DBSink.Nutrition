using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using MyHealth.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
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

        public async Task ReplaceNutritionDocument(NutritionEnvelope existingNutritionEnvelope)
        {
            try
            {
                ItemRequestOptions itemRequestOptions = new ItemRequestOptions
                {
                    EnableContentResponseOnWrite = false
                };

                await _myHealthContainer.ReplaceItemAsync(
                    existingNutritionEnvelope,
                    existingNutritionEnvelope.Id,
                    new PartitionKey(existingNutritionEnvelope.DocumentType),
                    itemRequestOptions);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<NutritionEnvelope> RetrieveNutritionEnvelope(string nutritionDate)
        {
            try
            {
                QueryDefinition query = new QueryDefinition("SELECT * FROM Records c WHERE c.DocumentType = 'Nutrition' AND c.Nutrition.NutritionDate = @nutritionLogDate")
                    .WithParameter("@nutritionLogDate", nutritionDate);
                List<NutritionEnvelope> nutritionEnvelopes = new List<NutritionEnvelope>();

                FeedIterator<NutritionEnvelope> feedIterator = _myHealthContainer.GetItemQueryIterator<NutritionEnvelope>(query);

                while (feedIterator.HasMoreResults)
                {
                    FeedResponse<NutritionEnvelope> queryResponse = await feedIterator.ReadNextAsync();
                    nutritionEnvelopes.AddRange(queryResponse.Resource);
                }

                return nutritionEnvelopes.FirstOrDefault();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}

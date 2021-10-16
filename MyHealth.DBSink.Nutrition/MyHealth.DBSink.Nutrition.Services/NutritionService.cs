using MyHealth.DBSink.Nutrition.Repository.Interfaces;
using MyHealth.DBSink.Nutrition.Services.Interfaces;
using System;
using System.Threading.Tasks;
using mdl = MyHealth.Common.Models;

namespace MyHealth.DBSink.Nutrition.Services
{
    public class NutritionService : INutritionService
    {
        private readonly INutritionRepository _nutritionRepository;

        public NutritionService(INutritionRepository nutritionRepository)
        {
            _nutritionRepository = nutritionRepository;
        }

        public async Task AddNutritionDocument(mdl.NutritionEnvelope nutrition)
        {
            try
            {
                await _nutritionRepository.CreateNutrition(nutrition);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public mdl.NutritionEnvelope MapNutritionToNutritionEnvelope(mdl.Nutrition nutrition)
        {
            if (nutrition == null)
                throw new Exception("No Nutrition Envelope to Map!");

            mdl.NutritionEnvelope nutritionEnvelope = new mdl.NutritionEnvelope
            {
                Id = Guid.NewGuid().ToString(),
                Nutrition = nutrition,
                DocumentType = "Nutrition",
                Date = nutrition.NutritionDate
            };

            return nutritionEnvelope;
        }
    }
}

using System.Threading.Tasks;
using mdl = MyHealth.Common.Models;

namespace MyHealth.DBSink.Nutrition.Services
{
    public interface INutritionDbService
    {
        /// <summary>
        /// Adds a new NutritionEnvelope object to the database.
        /// </summary>
        /// <param name="nutrition"></param>
        /// <returns></returns>
        Task AddNutritionDocument(mdl.Nutrition nutrition);

        /// <summary>
        /// Retrieves an existing NutritionEnvelope document from the database.
        /// </summary>
        /// <param name="nutritionDate"></param>
        /// <returns></returns>
        Task<mdl.NutritionEnvelope> RetrieveNutritionEnvelope(string nutritionDate);

        /// <summary>
        /// Replaces an existing Nutrition Document in the database.
        /// </summary>
        /// <param name="existingNutritionEnvelope"></param>
        /// <returns></returns>
        Task ReplaceNutritionDocument(mdl.NutritionEnvelope existingNutritionEnvelope);
    }
}

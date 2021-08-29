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
        Task AddNutritionDocument(mdl.NutritionEnvelope nutrition);
    }
}

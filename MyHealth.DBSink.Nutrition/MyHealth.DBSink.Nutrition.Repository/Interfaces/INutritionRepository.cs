using MyHealth.Common.Models;
using System.Threading.Tasks;

namespace MyHealth.DBSink.Nutrition.Repository.Interfaces
{
    public interface INutritionRepository
    {
        Task CreateNutrition(NutritionEnvelope nutritionEnvelope);
    }
}

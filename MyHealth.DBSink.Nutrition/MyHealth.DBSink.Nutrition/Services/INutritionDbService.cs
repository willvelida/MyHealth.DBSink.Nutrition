using System.Threading.Tasks;
using mdl = MyHealth.Common.Models;

namespace MyHealth.DBSink.Nutrition.Services
{
    public interface INutritionDbService
    {
        Task AddNutritionDocument(mdl.Nutrition nutrition);
    }
}

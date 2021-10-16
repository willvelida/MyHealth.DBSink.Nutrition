using System.Threading.Tasks;
using mdl = MyHealth.Common.Models;

namespace MyHealth.DBSink.Nutrition.Services.Interfaces
{
    public interface INutritionService
    {
        Task AddNutritionDocument(mdl.NutritionEnvelope nutrition);
        mdl.NutritionEnvelope MapNutritionToNutritionEnvelope(mdl.Nutrition nutrition);
    }
}

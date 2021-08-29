using MyHealth.Common.Models;
using System;
using System.Collections.Generic;
using System.Text;
using mdl = MyHealth.Common.Models;

namespace MyHealth.DBSink.Nutrition.Mappers
{
    public class NutritionEnvelopeMapper : INutritionEnvelopeMapper
    {
        public NutritionEnvelope MapNutritionToNutritionEnvelope(mdl.Nutrition nutrition)
        {
            if (nutrition == null)
                throw new Exception("No Nutrition Envelope to Map!");

            mdl.NutritionEnvelope nutritionEnvelope = new NutritionEnvelope
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

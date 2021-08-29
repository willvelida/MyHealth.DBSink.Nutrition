using System;
using System.Collections.Generic;
using System.Text;
using mdl = MyHealth.Common.Models;

namespace MyHealth.DBSink.Nutrition.Mappers
{
    public interface INutritionEnvelopeMapper
    {
        mdl.NutritionEnvelope MapNutritionToNutritionEnvelope(mdl.Nutrition nutrition);
    }
}

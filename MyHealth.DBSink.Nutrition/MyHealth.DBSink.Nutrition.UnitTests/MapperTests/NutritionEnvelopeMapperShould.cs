using AutoFixture;
using FluentAssertions;
using FluentAssertions.Execution;
using MyHealth.DBSink.Nutrition.Mappers;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using mdl = MyHealth.Common.Models;

namespace MyHealth.DBSink.Nutrition.UnitTests.MapperTests
{
    public class NutritionEnvelopeMapperShould
    {
        private NutritionEnvelopeMapper _sut;

        public NutritionEnvelopeMapperShould()
        {
            _sut = new NutritionEnvelopeMapper();
        }

        [Fact]
        public void ThrowExceptionWhenIncomingNutritionObjectIsNull()
        {
            Action nutritionEnvelopeMapperAction = () => _sut.MapNutritionToNutritionEnvelope(null);

            nutritionEnvelopeMapperAction.Should().Throw<Exception>().WithMessage("No Nutrition Envelope to Map!");
        }

        [Fact]
        public void MapNutritionToNutritionEnvelopeCorrectly()
        {
            var fixture = new Fixture();
            var testNutrition = fixture.Create<mdl.Nutrition>();
            testNutrition.NutritionDate = "2021-08-28";

            var expectedNutritionEnvelope = _sut.MapNutritionToNutritionEnvelope(testNutrition);

            using (new AssertionScope())
            {
                expectedNutritionEnvelope.Should().BeOfType<mdl.NutritionEnvelope>();
                expectedNutritionEnvelope.Nutrition.Should().Be(testNutrition);
                expectedNutritionEnvelope.DocumentType.Should().Be("Nutrition");
                expectedNutritionEnvelope.Date.Should().Be(testNutrition.NutritionDate);
            }
        }
    }
}

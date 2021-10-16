using AutoFixture;
using FluentAssertions;
using FluentAssertions.Execution;
using Moq;
using MyHealth.DBSink.Nutrition.Repository.Interfaces;
using MyHealth.DBSink.Nutrition.Services;
using System;
using System.Threading.Tasks;
using Xunit;
using mdl = MyHealth.Common.Models;

namespace MyHealth.DBSink.Nutrition.UnitTests.ServicesTests
{
    public class NutritionServiceShould
    {
        private Mock<INutritionRepository> _mockNutritionRepository;

        private NutritionService _sut;

        public NutritionServiceShould()
        {
            _mockNutritionRepository = new Mock<INutritionRepository>();

            _sut = new NutritionService(_mockNutritionRepository.Object);
        }

        [Fact]
        public async Task AddNutritionDocumentWhenCreateItemAsyncIsCalled()
        {
            // Arrange
            var fixutre = new Fixture();
            mdl.NutritionEnvelope testNutritionDocument = fixutre.Create<mdl.NutritionEnvelope>();

            // Act
            Func<Task> serviceAction = async () => await _sut.AddNutritionDocument(testNutritionDocument);

            // Assert
            await serviceAction.Should().NotThrowAsync<Exception>();
        }

        [Fact]
        public async Task ThrowExceptionWhenCreateItemAsyncCallFails()
        {
            // Arrange
            var fixutre = new Fixture();
            mdl.NutritionEnvelope testNutritionDocument = fixutre.Create<mdl.NutritionEnvelope>();

            _mockNutritionRepository.Setup(x => x.CreateNutrition(It.IsAny<mdl.NutritionEnvelope>())).Throws(new Exception());

            // Act
            Func<Task> serviceAction = async () => await _sut.AddNutritionDocument(testNutritionDocument);

            // Assert
            await serviceAction.Should().ThrowAsync<Exception>();
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

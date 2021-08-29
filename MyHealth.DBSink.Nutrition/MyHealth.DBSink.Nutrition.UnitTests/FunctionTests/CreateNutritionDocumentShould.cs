using AutoFixture;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using MyHealth.Common;
using MyHealth.DBSink.Nutrition.Functions;
using MyHealth.DBSink.Nutrition.Mappers;
using MyHealth.DBSink.Nutrition.Services;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;
using Xunit;
using mdl = MyHealth.Common.Models;

namespace MyHealth.DBSink.Nutrition.UnitTests.FunctionTests
{
    public class CreateNutritionDocumentShould
    {
        private Mock<ILogger> _mockLogger;
        private Mock<IConfiguration> _mockConfiguration;
        private Mock<INutritionDbService> _mockNutritionDbService;
        private Mock<INutritionEnvelopeMapper> _mockNutritionEnvelopeMapper;
        private Mock<IServiceBusHelpers> _mockServiceBusHelpers;

        private CreateNutritionDocument _func;

        public CreateNutritionDocumentShould()
        {
            _mockConfiguration = new Mock<IConfiguration>();
            _mockLogger = new Mock<ILogger>();
            _mockConfiguration.Setup(x => x["ServiceBusConnectionString"]).Returns("ServiceBusConnectionString");
            _mockNutritionDbService = new Mock<INutritionDbService>();
            _mockNutritionEnvelopeMapper = new Mock<INutritionEnvelopeMapper>();
            _mockServiceBusHelpers = new Mock<IServiceBusHelpers>();

            _func = new CreateNutritionDocument(
                _mockConfiguration.Object,
                _mockNutritionDbService.Object,
                _mockNutritionEnvelopeMapper.Object,
                _mockServiceBusHelpers.Object);
        }

        [Fact]
        public async Task AddActivityDocumentSuccessfullyWhenExistingNutritionDocumentIsNotFound()
        {
            // Arrange
            var fixture = new Fixture();
            var testNutrition = fixture.Create<mdl.Nutrition>();
            var testNutritionEnvelope = fixture.Create<mdl.NutritionEnvelope>();
            var testActivityDocumentString = JsonConvert.SerializeObject(testNutrition);

            _mockNutritionEnvelopeMapper.Setup(x => x.MapNutritionToNutritionEnvelope(It.IsAny<mdl.Nutrition>())).Returns(testNutritionEnvelope);
            _mockNutritionDbService.Setup(x => x.AddNutritionDocument(It.IsAny<mdl.NutritionEnvelope>())).Returns(Task.CompletedTask);

            // Act
            await _func.Run(testActivityDocumentString, _mockLogger.Object);

            // Assert
            _mockNutritionEnvelopeMapper.Verify(x => x.MapNutritionToNutritionEnvelope(It.IsAny<mdl.Nutrition>()), Times.Once);
            _mockNutritionDbService.Verify(x => x.AddNutritionDocument(It.IsAny<mdl.NutritionEnvelope>()), Times.Once);
            _mockServiceBusHelpers.Verify(x => x.SendMessageToQueue(It.IsAny<string>(), It.IsAny<Exception>()), Times.Never);
        }       

        [Fact]
        public async Task CatchAndLogErrorWhenAddActivityDocumentThrowsException()
        {
            // Arrange
            var fixture = new Fixture();
            var testNutrition = fixture.Create<mdl.Nutrition>();
            var testNutritionEnvelope = fixture.Create<mdl.NutritionEnvelope>();
            var testActivityDocumentString = JsonConvert.SerializeObject(testNutrition);

            _mockNutritionEnvelopeMapper.Setup(x => x.MapNutritionToNutritionEnvelope(It.IsAny<mdl.Nutrition>())).Returns(testNutritionEnvelope);
            _mockNutritionDbService.Setup(x => x.AddNutritionDocument(It.IsAny<mdl.NutritionEnvelope>())).ThrowsAsync(It.IsAny<Exception>());

            // Act
            Func<Task> responseAction = async () => await _func.Run(testActivityDocumentString, _mockLogger.Object);

            // Assert
            _mockNutritionDbService.Verify(x => x.AddNutritionDocument(It.IsAny<mdl.NutritionEnvelope>()), Times.Never);
            await responseAction.Should().ThrowAsync<Exception>();
            _mockServiceBusHelpers.Verify(x => x.SendMessageToQueue(It.IsAny<string>(), It.IsAny<Exception>()), Times.Once);
        }
    }
}

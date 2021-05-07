using FluentAssertions;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Moq;
using MyHealth.DBSink.Nutrition.Services;
using MyHealth.DBSink.Nutrition.UnitTests.TestHelpers;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using mdl = MyHealth.Common.Models;

namespace MyHealth.DBSink.Nutrition.UnitTests.ServicesTests
{
    public class NutritionDbServiceShould
    {
        private Mock<CosmosClient> _mockCosmosClient;
        private Mock<Container> _mockContainer;
        private Mock<IConfiguration> _mockConfiguration;

        private NutritionDbService _sut;

        public NutritionDbServiceShould()
        {
            _mockCosmosClient = new Mock<CosmosClient>();
            _mockContainer = new Mock<Container>();
            _mockCosmosClient.Setup(x => x.GetContainer(It.IsAny<string>(), It.IsAny<string>())).Returns(_mockContainer.Object);
            _mockConfiguration = new Mock<IConfiguration>();
            _mockConfiguration.Setup(x => x["DatabaseName"]).Returns("db");
            _mockConfiguration.Setup(x => x["ContainerName"]).Returns("col");

            _sut = new NutritionDbService(_mockCosmosClient.Object, _mockConfiguration.Object);
        }

        [Fact]
        public async Task AddNutritionDocumentWhenCreateItemAsyncIsCalled()
        {
            // Arrange
            mdl.Nutrition testNutritionDocument = new mdl.Nutrition
            {
                Carbs = 100
            };


            _mockContainer.SetupCreateItemAsync<mdl.Nutrition>();

            // Act
            Func<Task> serviceAction = async () => await _sut.AddNutritionDocument(testNutritionDocument);

            // Assert
            await serviceAction.Should().NotThrowAsync<Exception>();
            _mockContainer.Verify(x => x.CreateItemAsync(
                It.IsAny<mdl.NutritionEnvelope>(),
                It.IsAny<PartitionKey>(),
                It.IsAny<ItemRequestOptions>(),
                It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task ThrowExceptionWhenCreateItemAsyncCallFails()
        {
            // Arrange
            mdl.Nutrition testNutritionDocument = new mdl.Nutrition
            {
                Carbs = 100
            };


            _mockContainer.SetupCreateItemAsync<mdl.Nutrition>();
            _mockContainer.Setup(x => x.CreateItemAsync(
                It.IsAny<mdl.NutritionEnvelope>(),
                It.IsAny<PartitionKey>(),
                It.IsAny<ItemRequestOptions>(),
                It.IsAny<CancellationToken>())).ThrowsAsync(new Exception());

            // Act
            Func<Task> serviceAction = async () => await _sut.AddNutritionDocument(testNutritionDocument);

            // Assert
            await serviceAction.Should().ThrowAsync<Exception>();
        }
    }
}

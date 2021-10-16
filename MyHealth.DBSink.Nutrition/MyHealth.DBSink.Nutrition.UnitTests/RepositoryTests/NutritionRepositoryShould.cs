using AutoFixture;
using FluentAssertions;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Moq;
using MyHealth.DBSink.Nutrition.Repository;
using MyHealth.DBSink.Nutrition.UnitTests.TestHelpers;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using mdl = MyHealth.Common.Models;

namespace MyHealth.DBSink.Nutrition.UnitTests.RepositoryTests
{
    public class NutritionRepositoryShould
    {
        private Mock<CosmosClient> _mockCosmosClient;
        private Mock<Container> _mockContainer;
        private Mock<IConfiguration> _mockConfiguration;

        private NutritionRepository _sut;

        public NutritionRepositoryShould()
        {
            _mockCosmosClient = new Mock<CosmosClient>();
            _mockContainer = new Mock<Container>();
            _mockCosmosClient.Setup(x => x.GetContainer(It.IsAny<string>(), It.IsAny<string>())).Returns(_mockContainer.Object);
            _mockConfiguration = new Mock<IConfiguration>();
            _mockConfiguration.Setup(x => x["DatabaseName"]).Returns("db");
            _mockConfiguration.Setup(x => x["ContainerName"]).Returns("col");

            _sut = new NutritionRepository(_mockCosmosClient.Object, _mockConfiguration.Object);
        }

        [Fact]
        public async Task AddNutritionDocumentWhenCreateItemAsyncIsCalled()
        {
            // Arrange
            var fixutre = new Fixture();
            mdl.NutritionEnvelope testNutritionDocument = fixutre.Create<mdl.NutritionEnvelope>();

            _mockContainer.SetupCreateItemAsync<mdl.NutritionEnvelope>();

            // Act
            Func<Task> serviceAction = async () => await _sut.CreateNutrition(testNutritionDocument);

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
            var fixutre = new Fixture();
            mdl.NutritionEnvelope testNutritionDocument = fixutre.Create<mdl.NutritionEnvelope>();

            _mockContainer.SetupCreateItemAsync<mdl.Nutrition>();
            _mockContainer.Setup(x => x.CreateItemAsync(
                It.IsAny<mdl.NutritionEnvelope>(),
                It.IsAny<PartitionKey>(),
                It.IsAny<ItemRequestOptions>(),
                It.IsAny<CancellationToken>())).ThrowsAsync(new Exception());

            // Act
            Func<Task> serviceAction = async () => await _sut.CreateNutrition(testNutritionDocument);

            // Assert
            await serviceAction.Should().ThrowAsync<Exception>();
        }
    }
}

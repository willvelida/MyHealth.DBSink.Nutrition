using AutoFixture;
using FluentAssertions;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Moq;
using MyHealth.DBSink.Nutrition.Services;
using MyHealth.DBSink.Nutrition.UnitTests.TestHelpers;
using System;
using System.Collections.Generic;
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
            var fixutre = new Fixture();
            mdl.Nutrition testNutritionDocument = fixutre.Create<mdl.Nutrition>();

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
            var fixutre = new Fixture();
            mdl.Nutrition testNutritionDocument = fixutre.Create<mdl.Nutrition>();

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

        [Fact]
        public async Task RetrieveNutritionDocumentWhenGetItemQueryIteratorIsCalled()
        {
            // Arrange
            List<mdl.NutritionEnvelope> nutritionEnvelopes = new List<mdl.NutritionEnvelope>();
            mdl.NutritionEnvelope nutritionEnvelope = new mdl.NutritionEnvelope
            {
                Id = Guid.NewGuid().ToString(),
                DocumentType = "Test",
                Nutrition = new mdl.Nutrition
                {
                    NutritionDate = "2021-05-07"
                }
            };
            nutritionEnvelopes.Add(nutritionEnvelope);

            _mockContainer.SetupItemQueryIteratorMock(nutritionEnvelopes);
            _mockContainer.SetupItemQueryIteratorMock(new List<int> { nutritionEnvelopes.Count });

            // Act
            var response = await _sut.RetrieveNutritionEnvelope(nutritionEnvelope.Nutrition.NutritionDate);

            // Assert
            Assert.Equal(nutritionEnvelope.Id, response.Id);
            Assert.Equal(nutritionEnvelope.DocumentType, response.DocumentType);
            Assert.Equal(nutritionEnvelope.Nutrition.NutritionDate, response.Nutrition.NutritionDate);
        }

        [Fact]
        public async Task ReturnNullWhenNoExistingFoodRecordsForAGivenDateAreFound()
        {
            // Arrange
            var emptyNutritionEnvelopeList = new List<mdl.NutritionEnvelope>();

            var getFoodLogs = _mockContainer.SetupItemQueryIteratorMock(emptyNutritionEnvelopeList);
            getFoodLogs.feedIterator.Setup(x => x.HasMoreResults).Returns(false);
            _mockContainer.SetupItemQueryIteratorMock(new List<int>() { 0 });

            // Act
            var response = await _sut.RetrieveNutritionEnvelope("2021-05-01");

            // Act
            Assert.Null(response);
        }

        [Fact]
        public async Task ThrowExceptionWhenGetItemQueryIteratorFails()
        {
            // Arrange
            _mockContainer.Setup(x => x.GetItemQueryIterator<mdl.NutritionEnvelope>(
                It.IsAny<QueryDefinition>(),
                It.IsAny<string>(),
                It.IsAny<QueryRequestOptions>()))
                .Throws(new Exception());

            // Act
            Func<Task> responseAction = async () => await _sut.RetrieveNutritionEnvelope("2021-05-01");

            // Assert
            await responseAction.Should().ThrowAsync<Exception>();
        }

        [Fact]
        public async Task ReplaceNutritionDocumentWhenReplaceItemAsyncIsCalled()
        {
            var fixutre = new Fixture();
            mdl.NutritionEnvelope testNutritionEnvelopeDocument = fixutre.Create<mdl.NutritionEnvelope>();

            _mockContainer.SetupReplaceItemAsync<mdl.NutritionEnvelope>();

            // Act
            Func<Task> serviceAction = async () => await _sut.ReplaceNutritionDocument(testNutritionEnvelopeDocument);

            // Assert
            await serviceAction.Should().NotThrowAsync<Exception>();
            _mockContainer.Verify(x => x.ReplaceItemAsync(
                It.IsAny<mdl.NutritionEnvelope>(),
                It.IsAny<string>(),
                It.IsAny<PartitionKey>(),
                It.IsAny<ItemRequestOptions>(),
                It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task ThrowExceptionWhenReplaceItemAsyncFails()
        {
            // Arrange
            _mockContainer.Setup(x => x.ReplaceItemAsync(
                It.IsAny<mdl.NutritionEnvelope>(),
                It.IsAny<string>(),
                It.IsAny<PartitionKey>(),
                It.IsAny<ItemRequestOptions>(),
                It.IsAny<CancellationToken>()))
                .Throws(new Exception());

            // Act
            Func<Task> responseAction = async () => await _sut.ReplaceNutritionDocument(new mdl.NutritionEnvelope());

            // Act
            await responseAction.Should().ThrowAsync<Exception>();
        }
    }
}

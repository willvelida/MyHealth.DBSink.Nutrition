using Microsoft.Azure.Cosmos;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace MyHealth.DBSink.Nutrition.UnitTests.TestHelpers
{
    public static class CosmosExtensions
    {
        public static Mock<ItemResponse<T>> SetupCreateItemAsync<T>(this Mock<Container> containerMock)
        {
            var itemResponseMock = new Mock<ItemResponse<T>>();

            containerMock
                .Setup(x => x.CreateItemAsync(
                    It.IsAny<T>(),
                    It.IsAny<PartitionKey>(),
                    It.IsAny<ItemRequestOptions>(),
                    It.IsAny<CancellationToken>()))
                .Callback((T item, PartitionKey? partitionKey, ItemRequestOptions opts, CancellationToken ct) => itemResponseMock.Setup(x => x.Resource).Returns(null))
                .ReturnsAsync((T item, PartitionKey? partitionKey, ItemRequestOptions opts, CancellationToken ct) => itemResponseMock.Object);

            return itemResponseMock;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Aoraki.Web.Contracts;
using Aoraki.Web.Models.Entities;
using Aoraki.Web.Services;
using Azure;
using Azure.Data.Tables;
using FluentAssertions;
using Moq;
using Xunit;

namespace Aoraki.Web.Tests.Services;

public class BlogrollServiceTests
{
    private static BlogrollService ConstructService(IStorageFactory? storageFactory = null)
    {
        storageFactory ??= new Mock<IStorageFactory>().Object;
        return new BlogrollService(storageFactory);
    }

    [Fact]
    public async Task GetTotalEntryCountAsync_ShouldReturnValidResult()
    {
        var storageFactory = new Mock<IStorageFactory>();
        var tableClient = new Mock<TableClient>();
        var tableResults = new Mock<AsyncPageable<BlogrollBlog>>();
        storageFactory.Setup(x => x.GetTableClient(It.IsAny<string>())).Returns(tableClient.Object);

        tableClient.Setup(x => x.QueryAsync<BlogrollBlog>(
                It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()))
            .Returns(tableResults.Object);

        tableResults.Setup(x => x.GetAsyncEnumerator(It.IsAny<CancellationToken>()))
            .Returns(new List<BlogrollBlog> { new(), new() }.ToAsyncEnumerable().GetAsyncEnumerator());

        var service = ConstructService(storageFactory.Object);

        var result = await service.GetTotalEntryCountAsync(CancellationToken.None);
        result.Should().Be(2);

        tableClient.Verify(x => x.QueryAsync<BlogrollBlog>(
                It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetEntriesAsync_ShouldReturnValidResult()
    {
        var storageFactory = new Mock<IStorageFactory>();
        var tableClient = new Mock<TableClient>();
        var tableResults = new Mock<AsyncPageable<BlogrollBlog>>();
        storageFactory.Setup(x => x.GetTableClient(It.IsAny<string>())).Returns(tableClient.Object);

        tableClient.Setup(x => x.QueryAsync<BlogrollBlog>(
                It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()))
            .Returns(tableResults.Object);

        tableResults.Setup(x => x.GetAsyncEnumerator(It.IsAny<CancellationToken>()))
            .Returns(new List<BlogrollBlog>
                {
                    new()
                    {
                        Title = "aaa",
                        Timestamp = new DateTimeOffset(2020, 1, 1, 1, 1, 1, TimeSpan.Zero)
                    },
                    new()
                    {
                        Title = "bbb",
                        Timestamp = new DateTimeOffset(2021, 1, 1, 1, 1, 1, TimeSpan.Zero)
                    },
                    new()
                    {
                        Title = "ccc",
                        Timestamp = new DateTimeOffset(2022, 1, 1, 1, 1, 1, TimeSpan.Zero)
                    },
                    new()
                    {
                        Title = "ddd",
                        Timestamp = new DateTimeOffset(2023, 1, 1, 1, 1, 1, TimeSpan.Zero)
                    }
                }
                .ToAsyncEnumerable().GetAsyncEnumerator());

        var service = ConstructService(storageFactory.Object);

        var result = (await service.GetEntriesAsync(1, 2, CancellationToken.None)).ToList();
        result.Should().HaveCount(2);
        result[0].Title.Should().Be("ccc");
        result[1].Title.Should().Be("bbb");

        tableClient.Verify(x => x.QueryAsync<BlogrollBlog>(
                It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
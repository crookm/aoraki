using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.ServiceModel.Syndication;
using System.Threading;
using System.Threading.Tasks;
using Aoraki.Web.Contracts;
using Aoraki.Web.Models.Entities;
using Aoraki.Web.Services;
using Azure;
using Azure.Data.Tables;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace Aoraki.Web.Tests.Services;

public class JournalServiceTests
{
    private static JournalService ConstructService(IStorageFactory? storageFactory = null,
        ICanonicalService? canonicalService = null)
    {
        storageFactory ??= new Mock<IStorageFactory>().Object;
        canonicalService ??= new Mock<ICanonicalService>().Object;
        return new JournalService(storageFactory, canonicalService);
    }

    [Fact]
    public async Task GetTotalPostCountAsync_ShouldReturnValidResult()
    {
        var storageFactory = new Mock<IStorageFactory>();
        var tableClient = new Mock<TableClient>();
        var tableResults = new Mock<AsyncPageable<BlogPost>>();
        storageFactory.Setup(x => x.GetTableClient(It.IsAny<string>())).Returns(tableClient.Object);

        tableClient.Setup(x => x.QueryAsync(
                It.IsAny<Expression<Func<BlogPost, bool>>>(), It.IsAny<int?>(), It.IsAny<IEnumerable<string>>(),
                It.IsAny<CancellationToken>()))
            .Returns(tableResults.Object);

        tableResults.Setup(x => x.GetAsyncEnumerator(It.IsAny<CancellationToken>()))
            .Returns(new List<BlogPost> { new(), new() }.ToAsyncEnumerable().GetAsyncEnumerator());

        var service = ConstructService(storageFactory.Object);

        var result = await service.GetTotalPostCountAsync(false, CancellationToken.None);
        result.Should().Be(2);

        tableClient.Verify(x => x.QueryAsync(
                It.IsAny<Expression<Func<BlogPost, bool>>>(), It.IsAny<int?>(), It.IsAny<IEnumerable<string>>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetPostBySlugAsync_ShouldReturnNull_WhenNotFound()
    {
        var storageFactory = new Mock<IStorageFactory>();
        var tableClient = new Mock<TableClient>();
        storageFactory.Setup(x => x.GetTableClient(It.IsAny<string>())).Returns(tableClient.Object);
        tableClient.Setup(x => x.GetEntityAsync<BlogPost>(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new RequestFailedException(404, string.Empty));

        var service = ConstructService(storageFactory.Object);

        var result = await service.GetPostBySlugAsync("a", "b", true, CancellationToken.None);
        result.Should().BeNull();

        tableClient.Verify(x => x.GetEntityAsync<BlogPost>(
                "a", "b", It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetPostBySlugAsync_ShouldReturnNull_WhenNotPublished()
    {
        var storageFactory = new Mock<IStorageFactory>();
        var tableClient = new Mock<TableClient>();
        storageFactory.Setup(x => x.GetTableClient(It.IsAny<string>())).Returns(tableClient.Object);
        tableClient.Setup(x => x.GetEntityAsync<BlogPost>(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Response.FromValue(new BlogPost { Published = null }, null!));

        var service = ConstructService(storageFactory.Object);

        var result = await service.GetPostBySlugAsync("a", "b", false, CancellationToken.None);
        result.Should().BeNull();

        tableClient.Verify(x => x.GetEntityAsync<BlogPost>(
                "a", "b", It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetPostBySlugAsync_ShouldReturnNull_WhenPublishedLater()
    {
        var storageFactory = new Mock<IStorageFactory>();
        var tableClient = new Mock<TableClient>();
        storageFactory.Setup(x => x.GetTableClient(It.IsAny<string>())).Returns(tableClient.Object);
        tableClient.Setup(x => x.GetEntityAsync<BlogPost>(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Response.FromValue(new BlogPost { Published = DateTimeOffset.MaxValue }, null!));

        var service = ConstructService(storageFactory.Object);

        var result = await service.GetPostBySlugAsync("a", "b", false, CancellationToken.None);
        result.Should().BeNull();

        tableClient.Verify(x => x.GetEntityAsync<BlogPost>(
                "a", "b", It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetPostBySlugAsync_ShouldReturnValidResult()
    {
        var storageFactory = new Mock<IStorageFactory>();
        var tableClient = new Mock<TableClient>();
        storageFactory.Setup(x => x.GetTableClient(It.IsAny<string>())).Returns(tableClient.Object);
        tableClient.Setup(x => x.GetEntityAsync<BlogPost>(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Response.FromValue(new BlogPost { Published = DateTimeOffset.MinValue, Title = "z" }, null!));

        var service = ConstructService(storageFactory.Object);

        var result = await service.GetPostBySlugAsync("a", "b", false, CancellationToken.None);
        result.Should().NotBeNull();
        result!.Title.Should().Be("z");

        tableClient.Verify(x => x.GetEntityAsync<BlogPost>(
                "a", "b", It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetPostsAsync_ShouldReturnValidResult()
    {
        var storageFactory = new Mock<IStorageFactory>();
        var tableClient = new Mock<TableClient>();
        var tableResults = new Mock<AsyncPageable<BlogPost>>();
        storageFactory.Setup(x => x.GetTableClient(It.IsAny<string>())).Returns(tableClient.Object);

        tableClient.Setup(x => x.QueryAsync(
                It.IsAny<Expression<Func<BlogPost, bool>>>(), It.IsAny<int?>(), It.IsAny<IEnumerable<string>>(),
                It.IsAny<CancellationToken>()))
            .Returns(tableResults.Object);

        tableResults.Setup(x => x.GetAsyncEnumerator(It.IsAny<CancellationToken>()))
            .Returns(new List<BlogPost>
                {
                    new()
                    {
                        Title = "aaa",
                        Published = new DateTimeOffset(2020, 1, 1, 1, 1, 1, TimeSpan.Zero)
                    },
                    new()
                    {
                        Title = "bbb",
                        Published = new DateTimeOffset(2021, 1, 1, 1, 1, 1, TimeSpan.Zero)
                    },
                    new()
                    {
                        Title = "ccc",
                        Published = new DateTimeOffset(2022, 1, 1, 1, 1, 1, TimeSpan.Zero)
                    },
                    new()
                    {
                        Title = "ddd",
                        Published = new DateTimeOffset(2023, 1, 1, 1, 1, 1, TimeSpan.Zero)
                    }
                }
                .ToAsyncEnumerable().GetAsyncEnumerator());

        var service = ConstructService(storageFactory.Object);

        var result = (await service.GetPostsAsync(1, 2, true, CancellationToken.None)).ToList();
        result.Should().HaveCount(2);
        result[0].Title.Should().Be("ccc");
        result[1].Title.Should().Be("bbb");

        tableClient.Verify(x => x.QueryAsync(
                It.IsAny<Expression<Func<BlogPost, bool>>>(), It.IsAny<int?>(), It.IsAny<IEnumerable<string>>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetPostsArchiveAsync_ShouldReturnValidResult()
    {
        var storageFactory = new Mock<IStorageFactory>();
        var tableClient = new Mock<TableClient>();
        var tableResults = new Mock<AsyncPageable<BlogPost>>();
        storageFactory.Setup(x => x.GetTableClient(It.IsAny<string>())).Returns(tableClient.Object);

        tableClient.Setup(x => x.QueryAsync(
                It.IsAny<Expression<Func<BlogPost, bool>>>(), It.IsAny<int?>(), It.IsAny<IEnumerable<string>>(),
                It.IsAny<CancellationToken>()))
            .Returns(tableResults.Object);

        tableResults.Setup(x => x.GetAsyncEnumerator(It.IsAny<CancellationToken>()))
            .Returns(new List<BlogPost>
                {
                    new()
                    {
                        Title = "aaa",
                        Published = new DateTimeOffset(2020, 1, 1, 1, 1, 1, TimeSpan.Zero)
                    },
                    new()
                    {
                        Title = "bbb",
                        Published = new DateTimeOffset(2021, 8, 1, 1, 1, 1, TimeSpan.Zero)
                    },
                    new()
                    {
                        Title = "ccc",
                        Published = new DateTimeOffset(2021, 1, 1, 1, 1, 1, TimeSpan.Zero)
                    }
                }
                .ToAsyncEnumerable().GetAsyncEnumerator());

        var service = ConstructService(storageFactory.Object);

        var result = await service.GetPostsArchiveAsync(CancellationToken.None);
        result.Keys.Should().HaveCount(2);
        result.Should().ContainKeys(2020, 2021);
        result[2020].Should().HaveCount(1)
            .And.Subject.First().Title.Should().Be("aaa");
        result[2021].Should().HaveCount(2);
        result[2021][0].Title.Should().Be("bbb");
        result[2021][1].Title.Should().Be("ccc");

        tableClient.Verify(x => x.QueryAsync(
                It.IsAny<Expression<Func<BlogPost, bool>>>(), It.IsAny<int?>(), It.IsAny<IEnumerable<string>>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetPostsFeedItemsAsync_ShouldReturnValidResult()
    {
        var urlHelper = new Mock<IUrlHelper>();
        var canonical = new Mock<ICanonicalService>();
        var storageFactory = new Mock<IStorageFactory>();
        var tableClient = new Mock<TableClient>();
        var tableResults = new Mock<AsyncPageable<BlogPost>>();
        storageFactory.Setup(x => x.GetTableClient(It.IsAny<string>())).Returns(tableClient.Object);

        tableClient.Setup(x => x.QueryAsync(
                It.IsAny<Expression<Func<BlogPost, bool>>>(), It.IsAny<int?>(), It.IsAny<IEnumerable<string>>(),
                It.IsAny<CancellationToken>()))
            .Returns(tableResults.Object);

        tableResults.Setup(x => x.GetAsyncEnumerator(It.IsAny<CancellationToken>()))
            .Returns(new List<BlogPost>
                {
                    new()
                    {
                        PartitionKey = "z",
                        RowKey = "y",
                        Title = "aaa",
                        Content = "[abc]()",
                        Published = new DateTimeOffset(2020, 1, 1, 1, 1, 1, TimeSpan.Zero)
                    }
                }
                .ToAsyncEnumerable().GetAsyncEnumerator());

        canonical.Setup(x => x.CanonicaliseUrl(It.IsAny<string>())).Returns("https://example.com");

        var service = ConstructService(storageFactory.Object, canonical.Object);

        var result = await service.GetPostsFeedItemsAsync(urlHelper.Object, "abc", null, CancellationToken.None);
        var item = result.Should().HaveCount(1).And.Subject.First();
        item.Id.Should().Be("abc;id=z+y");
        item.Title.Text.Should().Be("aaa");
        item.Links.Should().Contain(x => x.Uri.Host == "example.com");
        item.PublishDate.Year.Should().Be(2020);
        item.LastUpdatedTime.Year.Should().Be(2020);

        var content = item.Content.Should().BeOfType<TextSyndicationContent>().Subject;
        content.Type.Should().Be("html");
        content.Text.Should().Be("<p><a href=\"\">abc</a></p>\n");

        tableClient.Verify(x => x.QueryAsync(
                It.IsAny<Expression<Func<BlogPost, bool>>>(), It.IsAny<int?>(), It.IsAny<IEnumerable<string>>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
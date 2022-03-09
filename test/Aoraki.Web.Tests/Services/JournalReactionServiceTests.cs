using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Aoraki.Web.Contracts;
using Aoraki.Web.Models;
using Aoraki.Web.Models.Entities;
using Aoraki.Web.Services;
using Azure;
using Azure.Data.Tables;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using Xunit;

namespace Aoraki.Web.Tests.Services;

public class JournalReactionServiceTests
{
    private static JournalReactionService ConstructService(IStorageFactory? storageFactory = null,
        IJournalService? journalService = null)
    {
        storageFactory ??= new Mock<IStorageFactory>().Object;
        journalService ??= new Mock<IJournalService>().Object;
        return new JournalReactionService(storageFactory, journalService);
    }

    [Fact]
    public async Task GetReactionsAsync_ShouldReturnNull_WhereNoReactionsFound()
    {
        var reactTableClient = new Mock<TableClient>();
        reactTableClient.Setup(x =>
                x.GetEntityAsync<BlogPostReactions>(
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IEnumerable<string>>(),
                    It.IsAny<CancellationToken>()))
            .ThrowsAsync(new RequestFailedException(StatusCodes.Status404NotFound, "not found"));

        var storageFactory = new Mock<IStorageFactory>();
        storageFactory.Setup(x => x.GetTableClient(Constants.TableNameBlogReact)).Returns(reactTableClient.Object);

        var service = ConstructService(storageFactory.Object);

        var results = await service.GetReactionsAsync("2012", "aaa", CancellationToken.None);
        results.Should().BeNull();

        reactTableClient.Verify(x => x.GetEntityAsync<BlogPostReactions>(
                "2012", "aaa", It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetReactionsAsync_ShouldReturnValidResult()
    {
        var response = new Mock<Response<BlogPostReactions>>();
        response.SetupGet(x => x.Value).Returns(new BlogPostReactions
        {
            PartitionKey = "2012",
            RowKey = "aaa",
            ReactLike = 1,
            ReactUseful = 2,
            ReactOutdated = 3,
            ReactEducational = 4
        });

        var reactTableClient = new Mock<TableClient>();
        reactTableClient.Setup(x => x.GetEntityAsync<BlogPostReactions>(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IEnumerable<string>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(response.Object);

        var storageFactory = new Mock<IStorageFactory>();
        storageFactory.Setup(x => x.GetTableClient(Constants.TableNameBlogReact)).Returns(reactTableClient.Object);

        var service = ConstructService(storageFactory.Object);

        var results = await service.GetReactionsAsync("2012", "aaa", CancellationToken.None);
        results.Should().NotBeNull()
            .And.ContainKeys(Enum.GetValues<Reaction>());

        results![Reaction.Like].Should().Be(1);
        results[Reaction.Useful].Should().Be(2);
        results[Reaction.Outdated].Should().Be(3);
        results[Reaction.Educational].Should().Be(4);

        reactTableClient.Verify(x => x.GetEntityAsync<BlogPostReactions>(
                "2012", "aaa", It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task PostReactionAsync_ShouldReturnFalse_WhenPostDoesNotExist()
    {
        var journalService = new Mock<IJournalService>();
        journalService.Setup(x => x.GetPostBySlugAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((BlogPost)null);

        var service = ConstructService(journalService: journalService.Object);

        var result =
            await service.PostReactionAsync("1111", "abc", "1.1.1.1", Reaction.Like, 0, CancellationToken.None);
        result.Should().BeFalse();

        journalService.Verify(x => x.GetPostBySlugAsync(
                "1111", "abc", false, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task PostReactionAsync_ShouldReturnFalse_WhenAuditRecordExistsAlready()
    {
        var journalService = new Mock<IJournalService>();
        journalService.Setup(x => x.GetPostBySlugAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BlogPost { PartitionKey = "1111", RowKey = "abc", });

        var response = new Mock<Response<BlogPostReactionAudit>>();
        var reactAuditTableClient = new Mock<TableClient>();
        reactAuditTableClient.Setup(x => x.GetEntityAsync<BlogPostReactionAudit>(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IEnumerable<string>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(response.Object);

        var storageFactory = new Mock<IStorageFactory>();
        storageFactory.Setup(x => x.GetTableClient(Constants.TableNameBlogReactAudit))
            .Returns(reactAuditTableClient.Object);

        var service = ConstructService(storageFactory.Object, journalService.Object);

        var result =
            await service.PostReactionAsync("1111", "abc", "1.1.1.1", Reaction.Like, 0, CancellationToken.None);
        result.Should().BeFalse();

        journalService.Verify(x => x.GetPostBySlugAsync(
                "1111", "abc", false, It.IsAny<CancellationToken>()),
            Times.Once);
        reactAuditTableClient.Verify(x => x.GetEntityAsync<BlogPostReactionAudit>(
                "1111_abc", "1.1.1.1", It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task PostReactionAsync_ShouldReturnFalse_WhenRecordCannotBeUpdatedInThreeTries()
    {
        var journalService = new Mock<IJournalService>();
        journalService.Setup(x => x.GetPostBySlugAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BlogPost { PartitionKey = "1111", RowKey = "abc", });

        var reactAuditTableClient = new Mock<TableClient>();
        reactAuditTableClient.Setup(x => x.GetEntityAsync<BlogPostReactionAudit>(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IEnumerable<string>>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new RequestFailedException(StatusCodes.Status404NotFound, "not found"));

        var reactResponse = new Mock<Response<BlogPostReactions>>();
        reactResponse.SetupGet(x => x.Value).Returns(new BlogPostReactions());
        var reactTableClient = new Mock<TableClient>();
        reactTableClient.Setup(x => x.GetEntityAsync<BlogPostReactions>(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(reactResponse.Object);
        reactTableClient.Setup(x => x.UpdateEntityAsync(
                It.IsAny<BlogPostReactions>(), It.IsAny<ETag>(), It.IsAny<TableUpdateMode>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new RequestFailedException(StatusCodes.Status412PreconditionFailed, "oop"));

        var storageFactory = new Mock<IStorageFactory>();
        storageFactory.Setup(x => x.GetTableClient(Constants.TableNameBlogReactAudit))
            .Returns(reactAuditTableClient.Object);
        storageFactory.Setup(x => x.GetTableClient(Constants.TableNameBlogReact))
            .Returns(reactTableClient.Object);

        var service = ConstructService(storageFactory.Object, journalService.Object);

        var result =
            await service.PostReactionAsync("1111", "abc", "1.1.1.1", Reaction.Like, 0, CancellationToken.None);
        result.Should().BeFalse();

        reactTableClient.Verify(x => x.UpdateEntityAsync(
                It.IsAny<BlogPostReactions>(), It.IsAny<ETag>(), It.IsAny<TableUpdateMode>(),
                It.IsAny<CancellationToken>()),
            Times.Exactly(3));
    }

    [Fact]
    public async Task PostReactionAsync_ShouldReturnTrue_WhenRequestIsValidOnExistingRecord()
    {
        var journalService = new Mock<IJournalService>();
        journalService.Setup(x => x.GetPostBySlugAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BlogPost { PartitionKey = "1111", RowKey = "abc", });

        var reactAuditTableClient = new Mock<TableClient>();
        reactAuditTableClient.Setup(x => x.GetEntityAsync<BlogPostReactionAudit>(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IEnumerable<string>>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new RequestFailedException(StatusCodes.Status404NotFound, "not found"));

        var reactResponse = new Mock<Response<BlogPostReactions>>();
        reactResponse.SetupGet(x => x.Value).Returns(new BlogPostReactions());
        var reactTableClient = new Mock<TableClient>();
        reactTableClient.Setup(x => x.GetEntityAsync<BlogPostReactions>(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(reactResponse.Object);

        var storageFactory = new Mock<IStorageFactory>();
        storageFactory.Setup(x => x.GetTableClient(Constants.TableNameBlogReactAudit))
            .Returns(reactAuditTableClient.Object);
        storageFactory.Setup(x => x.GetTableClient(Constants.TableNameBlogReact))
            .Returns(reactTableClient.Object);

        var service = ConstructService(storageFactory.Object, journalService.Object);

        var result =
            await service.PostReactionAsync("1111", "abc", "1.1.1.1", Reaction.Like, 0, CancellationToken.None);
        result.Should().BeTrue();

        reactTableClient.Verify(x => x.UpdateEntityAsync(
                It.Is<BlogPostReactions>(y => y.ReactLike == 1),
                It.IsAny<ETag>(), It.IsAny<TableUpdateMode>(), It.IsAny<CancellationToken>()),
            Times.Once);

        reactAuditTableClient.Verify(x => x.AddEntityAsync(
                It.Is<BlogPostReactionAudit>(y =>
                    y.PartitionKey == "1111_abc" && y.RowKey == "1.1.1.1" && y.Reaction == Reaction.Like),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task PostReactionAsync_ShouldReturnTrue_WhenRequestIsValidOnNewRecord()
    {
        var journalService = new Mock<IJournalService>();
        journalService.Setup(x => x.GetPostBySlugAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BlogPost { PartitionKey = "1111", RowKey = "abc", });

        var reactAuditTableClient = new Mock<TableClient>();
        reactAuditTableClient.Setup(x => x.GetEntityAsync<BlogPostReactionAudit>(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IEnumerable<string>>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new RequestFailedException(StatusCodes.Status404NotFound, "not found"));

        var reactTableClient = new Mock<TableClient>();
        reactTableClient.Setup(x => x.GetEntityAsync<BlogPostReactions>(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new RequestFailedException(StatusCodes.Status404NotFound, "not found"));

        var storageFactory = new Mock<IStorageFactory>();
        storageFactory.Setup(x => x.GetTableClient(Constants.TableNameBlogReactAudit))
            .Returns(reactAuditTableClient.Object);
        storageFactory.Setup(x => x.GetTableClient(Constants.TableNameBlogReact))
            .Returns(reactTableClient.Object);

        var service = ConstructService(storageFactory.Object, journalService.Object);

        var result =
            await service.PostReactionAsync("1111", "abc", "1.1.1.1", Reaction.Like, 0, CancellationToken.None);
        result.Should().BeTrue();

        reactTableClient.Verify(x => x.UpdateEntityAsync(
                It.IsAny<BlogPostReactions>(), It.IsAny<ETag>(), It.IsAny<TableUpdateMode>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        reactTableClient.Verify(x => x.AddEntityAsync(
                It.Is<BlogPostReactions>(y => y.PartitionKey == "1111" && y.RowKey == "abc" && y.ReactLike == 1),
                It.IsAny<CancellationToken>()),
            Times.Once);

        reactAuditTableClient.Verify(x => x.AddEntityAsync(
                It.Is<BlogPostReactionAudit>(y =>
                    y.PartitionKey == "1111_abc" && y.RowKey == "1.1.1.1" && y.Reaction == Reaction.Like),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
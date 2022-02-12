using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Aoraki.Web.Contracts;
using Aoraki.Web.Controllers;
using Aoraki.Web.Models;
using Aoraki.Web.Models.Entities;
using Aoraki.Web.Models.ViewModels;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace Aoraki.Web.Tests.Controllers;

public class JournalControllerTests
{
    private static JournalController ConstructController(IJournalService? journalService = null,
        IJournalReactionService? reactionService = null)
    {
        journalService ??= new Mock<IJournalService>().Object;
        reactionService ??= new Mock<IJournalReactionService>().Object;
        return new JournalController(journalService, reactionService);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(int.MinValue)]
    public async Task Index_ShouldRedirectToFirstPage_WhenPageTooLow(int requestedPage)
    {
        var journalService = new Mock<IJournalService>();
        journalService.Setup(x => x.GetTotalPostCountAsync(It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(JournalController.EntriesPerPage * 10);

        var controller = ConstructController(journalService.Object);

        var result = await controller.Index(page: requestedPage, CancellationToken.None);
        result.Should().BeOfType<RedirectToActionResult>()
            .Which.RouteValues.Should().Contain(x => x.Key == "page" && (int)x.Value! == 1);
    }

    [Theory]
    [InlineData(1, 2)]
    [InlineData(5, 10)]
    [InlineData(10, int.MaxValue)]
    public async Task Index_ShouldRedirectToLastPage_WhenPageTooHigh(int totalPages, int requestedPage)
    {
        var journalService = new Mock<IJournalService>();
        journalService.Setup(x => x.GetTotalPostCountAsync(It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(JournalController.EntriesPerPage * totalPages);

        var controller = ConstructController(journalService.Object);

        var result = await controller.Index(page: requestedPage, CancellationToken.None);
        result.Should().BeOfType<RedirectToActionResult>()
            .Which.RouteValues.Should().Contain(x => x.Key == "page" && (int)x.Value! == totalPages);
    }

    [Fact]
    public async Task Index_ShouldReturnValidResult()
    {
        const int totalPages = 12;
        const int currentPage = 4;

        var journalService = new Mock<IJournalService>();
        journalService.Setup(x => x.GetTotalPostCountAsync(It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(JournalController.EntriesPerPage * totalPages);
        journalService
            .Setup(x => x.GetPostsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<BlogPost> { new() { Content = "hello" } });

        var controller = ConstructController(journalService.Object);
        var result = await controller.Index(currentPage, CancellationToken.None);
        var view = result.Should().BeOfType<ViewResult>().Subject;
        var model = view.Model.Should().BeOfType<JournalIndexViewModel>().Subject;

        model.Pagination.CurrentPage.Should().Be(currentPage);
        model.Pagination.TotalPages.Should().Be(totalPages);
        model.Posts.First().Content.Should().Be("hello");

        journalService.Verify(
            x => x.GetPostsAsync(
                (currentPage - 1) * JournalController.EntriesPerPage, JournalController.EntriesPerPage,
                false, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Archive_ShouldReturnValidResult()
    {
        var journalService = new Mock<IJournalService>();
        journalService.Setup(x => x.GetPostsArchiveAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<int, List<BlogPostArchive>>
                { [2020] = new() { new BlogPostArchive { Title = "aaa" } } });

        var controller = ConstructController(journalService.Object);

        var result = await controller.Archive();
        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        var model = viewResult.Model.Should().BeOfType<Dictionary<int, List<BlogPostArchive>>>().Subject;
        model.Should().ContainKey(2020);
        model[2020].First().Title.Should().Be("aaa");

        journalService.Verify(x => x.GetPostsArchiveAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Read_ShouldReturnNotFound_WhenPostNotFound()
    {
        var controller = ConstructController();
        var result = await controller.Read("1", "aaa", CancellationToken.None);
        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task Read_ShouldReturnValidResult()
    {
        const string year = "2021";
        const string slug = "hello-world";

        var journalService = new Mock<IJournalService>();
        journalService
            .Setup(x => x.GetPostBySlugAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BlogPost { Content = "haha" });

        var controller = ConstructController(journalService.Object);

        var result = await controller.Read(year, slug, CancellationToken.None);
        var view = result.Should().BeOfType<ViewResult>().Subject;
        var model = view.Model.Should().BeOfType<JournalPostReadViewModel>().Subject;
        model.Post.Content.Should().Be("haha");

        journalService.Verify(x => x.GetPostBySlugAsync(
                year, slug, false, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ReadPlainText_ShouldReturnNotFound_WhenPostNotFound()
    {
        var controller = ConstructController();
        var result = await controller.ReadPlaintext("1", "aaa", CancellationToken.None);
        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task ReadPlainText_ShouldReturnValidResult()
    {
        const string year = "2021";
        const string slug = "hello-world";

        var journalService = new Mock<IJournalService>();
        journalService
            .Setup(x => x.GetPostBySlugAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BlogPost { Content = "[haha]()" });

        var controller = ConstructController(journalService.Object);

        var result = await controller.ReadPlaintext(year, slug, CancellationToken.None);
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeOfType<string>()
            .Which.Trim().Should().Be("haha");

        journalService.Verify(x => x.GetPostBySlugAsync(
                year, slug, false, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ReadMarkdown_ShouldReturnNotFound_WhenPostNotFound()
    {
        var controller = ConstructController();
        var result = await controller.ReadMarkdown("1", "aaa", CancellationToken.None);
        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task ReadMarkdown_ShouldReturnValidResult()
    {
        const string year = "2021";
        const string slug = "hello-world";

        var journalService = new Mock<IJournalService>();
        journalService
            .Setup(x => x.GetPostBySlugAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BlogPost { Content = "[haha]()" });

        var controller = ConstructController(journalService.Object);

        var result = await controller.ReadMarkdown(year, slug, CancellationToken.None);
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeOfType<string>()
            .Which.Trim().Should().Be("[haha]()");

        journalService.Verify(x => x.GetPostBySlugAsync(
                year, slug, false, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ReadJson_ShouldReturnNotFound_WhenPostNotFound()
    {
        var controller = ConstructController();
        var result = await controller.ReadJson("1", "aaa", CancellationToken.None);
        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task ReadJson_ShouldReturnValidResult()
    {
        const string year = "2021";
        const string slug = "hello-world";

        var journalService = new Mock<IJournalService>();
        journalService
            .Setup(x => x.GetPostBySlugAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BlogPost { Content = "[haha]()" });

        var controller = ConstructController(journalService.Object);

        var result = await controller.ReadJson(year, slug, CancellationToken.None);
        var jsonResult = result.Should().BeOfType<JsonResult>().Subject;
        jsonResult.Value.Should().BeOfType<BlogPost>()
            .Which.Content.Should().Be("[haha]()");

        journalService.Verify(x => x.GetPostBySlugAsync(
                year, slug, false, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task AtomFeed_ShouldBeValidAtomFeed()
    {
        var journalService = new Mock<IJournalService>();
        journalService.Setup(x => x.GetPostsFeedItemsAsync(
                It.IsAny<IUrlHelper>(), It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[]
            {
                new SyndicationItem
                {
                    Id = "aaa",
                    Title = new TextSyndicationContent("ppp", TextSyndicationContentKind.Plaintext),
                    Content = new TextSyndicationContent("www", TextSyndicationContentKind.Plaintext),
                    Links = { new SyndicationLink(new Uri("https://example.com")) },
                    PublishDate = DateTimeOffset.MinValue,
                    LastUpdatedTime = DateTimeOffset.MinValue
                }
            });

        var controller = ConstructController(journalService.Object);

        var result = await controller.AtomFeed(CancellationToken.None);
        var contentResult = result.Should().BeOfType<ContentResult>().Subject;
        contentResult.ContentType.Should().Be("application/atom+xml; charset=utf-8");
        var content = contentResult.Content.Should().NotBeNullOrWhiteSpace().And.Subject;

        await using var contentStream = new MemoryStream(Encoding.UTF8.GetBytes(content));
        using var reader = XmlReader.Create(contentStream);

        var atomSerializer = new Atom10FeedFormatter<SyndicationFeed>();
        atomSerializer.CanRead(reader).Should().BeTrue();

        journalService.Verify(x => x.GetPostsFeedItemsAsync(
                It.IsAny<IUrlHelper>(), Constants.SiteFeedBaseId, It.IsAny<int?>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task AtomFeed_ShouldReturnValidResult()
    {
        var journalService = new Mock<IJournalService>();
        journalService.Setup(x => x.GetPostsFeedItemsAsync(
                It.IsAny<IUrlHelper>(), It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[]
            {
                new SyndicationItem
                {
                    Id = "aaa",
                    Title = new TextSyndicationContent("ppp", TextSyndicationContentKind.Plaintext),
                    Content = new TextSyndicationContent("www", TextSyndicationContentKind.Plaintext),
                    Links = { new SyndicationLink(new Uri("https://example.com")) },
                    PublishDate = DateTimeOffset.MinValue,
                    LastUpdatedTime = DateTimeOffset.MinValue
                }
            });

        var controller = ConstructController(journalService.Object);

        var result = await controller.AtomFeed(CancellationToken.None);
        var contentResult = result.Should().BeOfType<ContentResult>().Subject;
        contentResult.ContentType.Should().Be("application/atom+xml; charset=utf-8");
        var content = contentResult.Content.Should().NotBeNullOrWhiteSpace().And.Subject;

        await using var contentStream = new MemoryStream(Encoding.UTF8.GetBytes(content));
        using var reader = XmlReader.Create(contentStream);

        var feed = SyndicationFeed.Load(reader);
        feed.Id.Should().Be(Constants.SiteFeedBaseId);
        feed.Title.Text.Should().Be(Constants.SiteTitle);
        feed.Links.Should()
            .Contain(x => x.RelationshipType == "alternate" && x.Uri == new Uri(Constants.SiteBaseUrl));
        feed.Items.ToList().Should().HaveCountGreaterThan(0)
            .And.Subject.First().Id.Should().Be("aaa");

        journalService.Verify(x => x.GetPostsFeedItemsAsync(
                It.IsAny<IUrlHelper>(), Constants.SiteFeedBaseId, It.IsAny<int?>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task RssFeed_ShouldBeValidAtomFeed()
    {
        var journalService = new Mock<IJournalService>();
        journalService.Setup(x => x.GetPostsFeedItemsAsync(
                It.IsAny<IUrlHelper>(), It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[]
            {
                new SyndicationItem
                {
                    Id = "aaa",
                    Title = new TextSyndicationContent("ppp", TextSyndicationContentKind.Plaintext),
                    Content = new TextSyndicationContent("www", TextSyndicationContentKind.Plaintext),
                    Links = { new SyndicationLink(new Uri("https://example.com")) },
                    PublishDate = DateTimeOffset.MinValue,
                    LastUpdatedTime = DateTimeOffset.MinValue
                }
            });

        var controller = ConstructController(journalService.Object);

        var result = await controller.RssFeed(CancellationToken.None);
        var contentResult = result.Should().BeOfType<ContentResult>().Subject;
        contentResult.ContentType.Should().Be("application/rss+xml; charset=utf-8");
        var content = contentResult.Content.Should().NotBeNullOrWhiteSpace().And.Subject;

        await using var contentStream = new MemoryStream(Encoding.UTF8.GetBytes(content));
        using var reader = XmlReader.Create(contentStream);

        var rssSerializer = new Rss20FeedFormatter<SyndicationFeed>();
        rssSerializer.CanRead(reader).Should().BeTrue();

        journalService.Verify(x => x.GetPostsFeedItemsAsync(
                It.IsAny<IUrlHelper>(), Constants.SiteFeedBaseId, It.IsAny<int?>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task RssFeed_ShouldReturnValidResult()
    {
        var journalService = new Mock<IJournalService>();
        journalService.Setup(x => x.GetPostsFeedItemsAsync(
                It.IsAny<IUrlHelper>(), It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[]
            {
                new SyndicationItem
                {
                    Id = "aaa",
                    Title = new TextSyndicationContent("ppp", TextSyndicationContentKind.Plaintext),
                    Content = new TextSyndicationContent("www", TextSyndicationContentKind.Plaintext),
                    Links = { new SyndicationLink(new Uri("https://example.com")) },
                    PublishDate = DateTimeOffset.MinValue,
                    LastUpdatedTime = DateTimeOffset.MinValue
                }
            });

        var controller = ConstructController(journalService.Object);

        var result = await controller.RssFeed(CancellationToken.None);
        var contentResult = result.Should().BeOfType<ContentResult>().Subject;
        contentResult.ContentType.Should().Be("application/rss+xml; charset=utf-8");
        var content = contentResult.Content.Should().NotBeNullOrWhiteSpace().And.Subject;

        await using var contentStream = new MemoryStream(Encoding.UTF8.GetBytes(content));
        using var reader = XmlReader.Create(contentStream);

        var feed = SyndicationFeed.Load(reader);
        feed.Id.Should().Be(Constants.SiteFeedBaseId);
        feed.Title.Text.Should().Be(Constants.SiteTitle);
        feed.Links.Should()
            .Contain(x => x.RelationshipType == "alternate" && x.Uri == new Uri(Constants.SiteBaseUrl));
        feed.Items.ToList().Should().HaveCountGreaterThan(0)
            .And.Subject.First().Id.Should().Be("aaa");

        journalService.Verify(x => x.GetPostsFeedItemsAsync(
                It.IsAny<IUrlHelper>(), Constants.SiteFeedBaseId, It.IsAny<int?>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task SetupSyndicationFeed_ShouldReturnValidResult()
    {
        var journalService = new Mock<IJournalService>();
        journalService.Setup(x => x.GetPostsFeedItemsAsync(
                It.IsAny<IUrlHelper>(), It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[]
            {
                new SyndicationItem
                {
                    Id = "aaa",
                    Title = new TextSyndicationContent("ppp", TextSyndicationContentKind.Plaintext),
                    Content = new TextSyndicationContent("www", TextSyndicationContentKind.Plaintext),
                    Links = { new SyndicationLink(new Uri("https://example.com")) },
                    PublishDate = DateTimeOffset.MinValue,
                    LastUpdatedTime = DateTimeOffset.MinValue
                }
            });

        var controller = ConstructController(journalService.Object);

        var result = await controller.SetupSyndicationFeed(CancellationToken.None);
        result.Id.Should().Be(Constants.SiteFeedBaseId);
        result.Title.Text.Should().Be(Constants.SiteTitle);
        result.Links.Should()
            .Contain(x => x.RelationshipType == "alternate" && x.Uri == new Uri(Constants.SiteBaseUrl));
        result.Items.ToList().Should().HaveCountGreaterThan(0)
            .And.Subject.First().Id.Should().Be("aaa");

        journalService.Verify(x => x.GetPostsFeedItemsAsync(
                It.IsAny<IUrlHelper>(), Constants.SiteFeedBaseId, It.IsAny<int?>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
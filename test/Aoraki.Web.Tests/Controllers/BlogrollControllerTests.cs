using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Aoraki.Web.Contracts;
using Aoraki.Web.Controllers;
using Aoraki.Web.Models.Entities;
using Aoraki.Web.Models.ViewModels;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace Aoraki.Web.Tests.Controllers;

public class BlogrollControllerTests
{
    private static BlogrollController ConstructController(IBlogrollService? blogrollService = null)
    {
        blogrollService ??= new Mock<IBlogrollService>().Object;
        return new BlogrollController(blogrollService);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(int.MinValue)]
    public async Task Index_ShouldRedirectToFirstPage_WhenPageTooLow(int requestedPage)
    {
        var blogrollService = new Mock<IBlogrollService>();
        blogrollService.Setup(x => x.GetTotalEntryCountAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(BlogrollController.EntriesPerPage * 10);

        var controller = ConstructController(blogrollService.Object);

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
        var blogrollService = new Mock<IBlogrollService>();
        blogrollService.Setup(x => x.GetTotalEntryCountAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(BlogrollController.EntriesPerPage * totalPages);

        var controller = ConstructController(blogrollService.Object);

        var result = await controller.Index(page: requestedPage, CancellationToken.None);
        result.Should().BeOfType<RedirectToActionResult>()
            .Which.RouteValues.Should().Contain(x => x.Key == "page" && (int)x.Value! == totalPages);
    }

    [Fact]
    public async Task Index_ShouldReturnValidResult()
    {
        const int totalPages = 12;
        const int currentPage = 4;

        var blogrollService = new Mock<IBlogrollService>();
        blogrollService.Setup(x => x.GetTotalEntryCountAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(BlogrollController.EntriesPerPage * totalPages);
        blogrollService.Setup(x => x.GetEntriesAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<BlogrollBlog>
            {
                new()
                {
                    PartitionKey = "1",
                    RowKey = "3633b64a-20d2-44cf-bf62-d4813d97a3d6",
                    Timestamp = new DateTimeOffset(637800799516505234, TimeSpan.FromHours(13)),
                    Title = "aaa",
                    Description = "bbb",
                    Url = "https://example.com"
                }
            });

        var controller = ConstructController(blogrollService.Object);

        var result = await controller.Index(currentPage, CancellationToken.None);
        var view = result.Should().BeOfType<ViewResult>().Subject;
        var model = view.Model.Should().BeOfType<BlogrollIndexViewModel>().Subject;

        model.Pagination.CurrentPage.Should().Be(currentPage);
        model.Pagination.TotalPages.Should().Be(totalPages);
        model.Blogs.First().Title.Should().Be("aaa");

        blogrollService.Verify(x => x.GetEntriesAsync(
                (currentPage - 1) * BlogrollController.EntriesPerPage, BlogrollController.EntriesPerPage,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
using System;
using System.Reflection;
using System.Threading.Tasks;
using Aoraki.Web.Contracts;
using Aoraki.Web.Data.Context;
using Aoraki.Web.Data.Models;
using Aoraki.Web.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

// ReSharper disable HeapView.BoxingAllocation
// ReSharper disable MethodHasAsyncOverload
namespace Aoraki.Web.Tests.Services
{
    public class JournalPostServiceTests
    {
        private readonly ICanonicalService _canonical = new CanonicalService();

        private static DbContextOptions<AorakiDbContext> CreateDbContext(string methodName) =>
            new DbContextOptionsBuilder<AorakiDbContext>()
                .UseInMemoryDatabase($"AorakiUnitTest-{methodName}")
                .Options;

        // ReSharper disable once InconsistentNaming
        public static object[][] PublishedPostsExpression_ShouldBeAccurate_Data =>
            new[]
            {
                new object[] { DateTime.UtcNow, true },
                new object[] { DateTime.UtcNow - TimeSpan.FromSeconds(1), true },
                new object[] { DateTime.UtcNow - TimeSpan.FromDays(12), true },
                new object[] { DateTime.UtcNow - TimeSpan.FromDays(999), true },
                new object[] { DateTime.UtcNow + TimeSpan.FromDays(1), false },
                new object[] { DateTime.UtcNow + TimeSpan.FromDays(999), false },
            };
        
        [Theory]
        [MemberData(nameof(PublishedPostsExpression_ShouldBeAccurate_Data))]
        public void PublishedPostsExpression_ShouldBeAccurate(DateTime publishedTime, bool expectedResult) =>
            JournalPostService.PublishedPostsExpression.Compile()(new JournalPost{Published = publishedTime})
                .Should().Be(expectedResult);
        
        [Fact]
        public async Task GetTotalPostCountAsync_ShouldProduceAccurateCount_WithOnlyPublishedPosts()
        {
            await using var context = new AorakiDbContext(
                CreateDbContext(MethodBase.GetCurrentMethod()?.DeclaringType?.FullName));
            context.JournalPosts.Add(new JournalPost
            {
                Published = DateTime.UtcNow - TimeSpan.FromDays(10)
            });
            context.JournalPosts.Add(new JournalPost
            {
                Published = DateTime.UtcNow
            });
            await context.SaveChangesAsync();

            var service = new JournalPostService(context, _canonical);
            var result = await service.GetTotalPostCountAsync();

            result.Should().Be(2, because: "all posts are published in the past");
        }
        
        [Fact]
        public async Task GetTotalPostCountAsync_ShouldProduceAccurateCount_WithSomeUnpublishedPosts()
        {
            await using var context = new AorakiDbContext(
                CreateDbContext(MethodBase.GetCurrentMethod()?.DeclaringType?.FullName));
            context.JournalPosts.Add(new JournalPost
            {
                Published = DateTime.UtcNow - TimeSpan.FromDays(10)
            });
            context.JournalPosts.Add(new JournalPost
            {
                Published = null
            });
            await context.SaveChangesAsync();

            var service = new JournalPostService(context, _canonical);
            var result = await service.GetTotalPostCountAsync();

            result.Should().Be(1, because: "one of two posts are not yet published");
        }
        
        [Fact]
        public async Task GetTotalPostCountAsync_ShouldProduceAccurateCount_WithNoPublishedPosts()
        {
            await using var context = new AorakiDbContext(
                CreateDbContext(MethodBase.GetCurrentMethod()?.DeclaringType?.FullName));
            context.JournalPosts.Add(new JournalPost
            {
                Published = null
            });
            context.JournalPosts.Add(new JournalPost
            {
                Published = null
            });
            await context.SaveChangesAsync();

            var service = new JournalPostService(context, _canonical);
            var result = await service.GetTotalPostCountAsync();

            result.Should().Be(0, because: "no posts are published");
        }
        
        [Fact]
        public async Task GetTotalPostCountAsync_ShouldProduceAccurateCount_WithOnlyFuturePublishedPosts()
        {
            await using var context = new AorakiDbContext(
                CreateDbContext(MethodBase.GetCurrentMethod()?.DeclaringType?.FullName));
            context.JournalPosts.Add(new JournalPost
            {
                Published = DateTime.UtcNow + TimeSpan.FromDays(100)
            });
            context.JournalPosts.Add(new JournalPost
            {
                Published = DateTime.UtcNow + TimeSpan.FromDays(1)
            });
            await context.SaveChangesAsync();

            var service = new JournalPostService(context, _canonical);
            var result = await service.GetTotalPostCountAsync();

            result.Should().Be(0, because: "all posts are published in the future (scheduled)");
        }
        
        [Fact]
        public async Task GetTotalPostCountAsync_ShouldProduceAccurateCount_WithSomeFuturePublishedPosts()
        {
            await using var context = new AorakiDbContext(
                CreateDbContext(MethodBase.GetCurrentMethod()?.DeclaringType?.FullName));
            context.JournalPosts.Add(new JournalPost
            {
                Published = DateTime.UtcNow - TimeSpan.FromDays(10)
            });
            context.JournalPosts.Add(new JournalPost
            {
                Published = DateTime.UtcNow
            });
            context.JournalPosts.Add(new JournalPost
            {
                Published = DateTime.UtcNow + TimeSpan.FromDays(1)
            });
            await context.SaveChangesAsync();

            var service = new JournalPostService(context, _canonical);
            var result = await service.GetTotalPostCountAsync();

            result.Should().Be(2, because: "one of three posts are scheduled for the future");
        }

        [Fact]
        public async Task GetPostByIdAsync_ShouldFindPost_WhereExistsAndPublished()
        {
            await using var context = new AorakiDbContext(
                CreateDbContext(MethodBase.GetCurrentMethod()?.DeclaringType?.FullName));
            context.JournalPosts.Add(new JournalPost
            {
                Id = 12,
                Published = DateTime.UtcNow
            });
            await context.SaveChangesAsync();
            
            var service = new JournalPostService(context, _canonical);
            var result = await service.GetPostByIdAsync(12, false);

            result.Id.Should().Be(12);
        }
        
        [Fact]
        public async Task GetPostByIdAsync_ShouldFindPost_WhereExistsAndUnPublishedAllowed()
        {
            await using var context = new AorakiDbContext(
                CreateDbContext(MethodBase.GetCurrentMethod()?.DeclaringType?.FullName));
            context.JournalPosts.Add(new JournalPost
            {
                Id = 12,
                Published = null
            });
            await context.SaveChangesAsync();
            
            var service = new JournalPostService(context, _canonical);
            var result = await service.GetPostByIdAsync(12, true);

            result.Id.Should().Be(12);
        }
        
        [Fact]
        public async Task GetPostByIdAsync_ShouldNotFindPost_WhereNotExists()
        {
            await using var context = new AorakiDbContext(
                CreateDbContext(MethodBase.GetCurrentMethod()?.DeclaringType?.FullName));
            context.JournalPosts.Add(new JournalPost
            {
                Id = 12,
                Published = DateTime.UtcNow
            });
            await context.SaveChangesAsync();
            
            var service = new JournalPostService(context, _canonical);
            var result = await service.GetPostByIdAsync(13, false);

            result.Should().BeNull();
        }
        
        [Fact]
        public async Task GetPostByIdAsync_ShouldNotFindPost_WhereExistsNotPublished()
        {
            await using var context = new AorakiDbContext(
                CreateDbContext(MethodBase.GetCurrentMethod()?.DeclaringType?.FullName));
            context.JournalPosts.Add(new JournalPost
            {
                Id = 12,
                Published = null
            });
            await context.SaveChangesAsync();
            
            var service = new JournalPostService(context, _canonical);
            var result = await service.GetPostByIdAsync(12, false);

            result.Should().BeNull();
        }
        
        [Fact]
        public async Task GetPostBySlugAsync_ShouldFindPost_WhereExistsAndPublished()
        {
            await using var context = new AorakiDbContext(
                CreateDbContext(MethodBase.GetCurrentMethod()?.DeclaringType?.FullName));
            context.JournalPosts.Add(new JournalPost
            {
                Slug = "aaa",
                Published = DateTime.UtcNow
            });
            await context.SaveChangesAsync();
            
            var service = new JournalPostService(context, _canonical);
            var result = await service.GetPostBySlugAsync("aaa", false);

            result.Slug.Should().Be("aaa");
        }
        
        [Fact]
        public async Task GetPostBySlugAsync_ShouldFindPost_WhereExistsAndUnPublishedAllowed()
        {
            await using var context = new AorakiDbContext(
                CreateDbContext(MethodBase.GetCurrentMethod()?.DeclaringType?.FullName));
            context.JournalPosts.Add(new JournalPost
            {
                Slug = "aaa",
                Published = null
            });
            await context.SaveChangesAsync();
            
            var service = new JournalPostService(context, _canonical);
            var result = await service.GetPostBySlugAsync("aaa", true);

            result.Slug.Should().Be("aaa");
        }
        
        [Fact]
        public async Task GetPostBySlugAsync_ShouldNotFindPost_WhereNotExists()
        {
            await using var context = new AorakiDbContext(
                CreateDbContext(MethodBase.GetCurrentMethod()?.DeclaringType?.FullName));
            
            var service = new JournalPostService(context, _canonical);
            var result = await service.GetPostBySlugAsync("aaa", false);

            result.Should().BeNull();
        }
        
        [Fact]
        public async Task GetPostBySlugAsync_ShouldNotFindPost_WhereExistsNotPublished()
        {
            await using var context = new AorakiDbContext(
                CreateDbContext(MethodBase.GetCurrentMethod()?.DeclaringType?.FullName));
            context.JournalPosts.Add(new JournalPost
            {
                Slug = "aaa",
                Published = null
            });
            await context.SaveChangesAsync();
            
            var service = new JournalPostService(context, _canonical);
            var result = await service.GetPostBySlugAsync("aaa", false);

            result.Should().BeNull();
        }

        [Fact]
        public async Task GetPostsAsync_ShouldGetAllPublishedPostsInOrder_UnpublishedNotAllowed()
        {
            await using var context = new AorakiDbContext(
                CreateDbContext(MethodBase.GetCurrentMethod()?.DeclaringType?.FullName));
            context.JournalPosts.Add(new JournalPost
            {
                Slug = "unpublished",
                Published = null
            });
            context.JournalPosts.Add(new JournalPost
            {
                Slug = "oldest",
                Published = DateTime.UtcNow - TimeSpan.FromDays(2)
            });
            context.JournalPosts.Add(new JournalPost
            {
                Slug = "newest",
                Published = DateTime.UtcNow
            });
            await context.SaveChangesAsync();
            
            var service = new JournalPostService(context, _canonical);
            var result = await service.GetPostsAsync(0, int.MaxValue, allowUnpublished: false);

            result.Should()
                .HaveCount(2, because: "only two of three posts are published")
                .And.BeInDescendingOrder(x => x.Published);
        }
        
        [Fact]
        public async Task GetPostsAsync_ShouldGetAllPublishedPosts_UnpublishedAllowed()
        {
            await using var context = new AorakiDbContext(
                CreateDbContext(MethodBase.GetCurrentMethod()?.DeclaringType?.FullName));
            context.JournalPosts.Add(new JournalPost
            {
                Slug = "unpublished",
                Published = null
            });
            context.JournalPosts.Add(new JournalPost
            {
                Slug = "oldest",
                Published = DateTime.UtcNow - TimeSpan.FromDays(2)
            });
            context.JournalPosts.Add(new JournalPost
            {
                Slug = "newest",
                Published = DateTime.UtcNow
            });
            await context.SaveChangesAsync();
            
            var service = new JournalPostService(context, _canonical);
            var result = await service.GetPostsAsync(0, int.MaxValue, allowUnpublished: true);

            result.Should()
                .HaveCount(3, because: "unpublished posts should be shown");
        }
        
        [Fact]
        public async Task GetPostsAsync_ShouldPaginateAccurately()
        {
            await using var context = new AorakiDbContext(
                CreateDbContext(MethodBase.GetCurrentMethod()?.DeclaringType?.FullName));
            context.JournalPosts.Add(new JournalPost
            {
                Slug = "unpublished",
                Published = null
            });
            context.JournalPosts.Add(new JournalPost
            {
                Slug = "oldest",
                Published = DateTime.UtcNow - TimeSpan.FromDays(2)
            });
            context.JournalPosts.Add(new JournalPost
            {
                Slug = "newest",
                Published = DateTime.UtcNow
            });
            await context.SaveChangesAsync();
            
            var service = new JournalPostService(context, _canonical);
            var result1 = await service.GetPostsAsync(0, 2, allowUnpublished: false);
            var result2 = await service.GetPostsAsync(2, 2, allowUnpublished: false);
            var result3 = await service.GetPostsAsync(1, 1, allowUnpublished: false);
            var result4 = await service.GetPostsAsync(0, 2, allowUnpublished: true);
            var result5 = await service.GetPostsAsync(2, 2, allowUnpublished: true);

            result1.Should().HaveCount(2, because: "there should be enough results to fill this page");
            result2.Should().HaveCount(0, because: "there should be no more results available");
            result3.Should().HaveCount(1, because: "there should be enough posts to have results on this page with this take");
            result4.Should().HaveCount(2, because: "there should be enough results including unpublished to fill this page");
            result5.Should().HaveCount(1, because: "there should be enough results including unpublished to partially fill this page");
        }

        [Fact]
        public async Task GetPostsArchiveAsync_ShouldGetAllPublishedPostsInOrder()
        {
            await using var context = new AorakiDbContext(
                CreateDbContext(MethodBase.GetCurrentMethod()?.DeclaringType?.FullName));
            context.JournalPosts.Add(new JournalPost
            {
                Slug = "unpublished",
                Published = null
            });
            context.JournalPosts.Add(new JournalPost
            {
                Slug = "2010 post 1",
                Published = new DateTime(2010, 05, 10)
            });
            context.JournalPosts.Add(new JournalPost
            {
                Slug = "2010 post 2",
                Published = new DateTime(2010, 10, 10)
            });
            context.JournalPosts.Add(new JournalPost
            {
                Slug = "2020 post 1",
                Published = new DateTime(2020, 10, 10)
            });
            await context.SaveChangesAsync();
            
            var service = new JournalPostService(context, _canonical);
            var result = await service.GetPostsArchiveAsync();

            result.Should()
                .HaveCount(2, because: "there should be two year groups for 2010 and 2020")
                .And.ContainKeys(2010, 2020);
            result.Keys.Should().BeInDescendingOrder();
            result[2010].Should()
                .HaveCount(2, because: "there are two posts from 2010")
                .And.BeInDescendingOrder(x => x.Published);
            result[2020].Should().HaveCount(1, because: "there is only one post from 2020");
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.ServiceModel.Syndication;
using System.Threading.Tasks;
using Aoraki.Web.Contracts;
using Aoraki.Web.Data.Context;
using Aoraki.Web.Data.Models;
using Aoraki.Web.Extensions;
using Aoraki.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Aoraki.Web.Services
{
    public class JournalPostService : IJournalPostService
    {
        private readonly AorakiDbContext _db;
        private readonly ICanonicalService _canonical;

        public JournalPostService(AorakiDbContext db, ICanonicalService canonical)
        {
            _db = db;
            _canonical = canonical;
        }

        private readonly Expression<Func<JournalPost, bool>> PublishedPostsExpression =
            post => post.Published != null && post.Published <= DateTime.UtcNow;

        public async Task<int> CreatePostAsync(JournalPost post)
        {
            var inserted = _db.JournalPosts.Add(post);
            await _db.SaveChangesAsync();
            return inserted.Entity.Id;
        }

        public async Task UpdatePostAsync(JournalPost post)
        {
            _db.Update(post);
            await _db.SaveChangesAsync();
        }

        public async Task<int> GetTotalPostCountAsync()
        {
            return await _db.JournalPosts
                .AsQueryable()
                .CountAsync(PublishedPostsExpression);
        }

        public async Task<JournalPost> GetPostByIdAsync(int id, bool allowUnpublished = false)
        {
            var filter = _db.JournalPosts.Where(post => post.Id == id);
            if (!allowUnpublished)
                filter = filter.Where(PublishedPostsExpression);
            return await filter.FirstOrDefaultAsync();
        }

        public async Task<JournalPost> GetPostBySlugAsync(string slug, bool allowUnpublished = false)
        {
            var filter = _db.JournalPosts.Where(post => post.Slug == slug);
            if (!allowUnpublished)
                filter = filter.Where(PublishedPostsExpression);
            return await filter.FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<JournalPost>> GetPostsAsync(int skip, int take, bool allowUnpublished = false)
        {
            var filter = _db.JournalPosts.AsQueryable();
            if (!allowUnpublished)
                filter = filter.Where(PublishedPostsExpression);
            return await filter
                .OrderByDescending(post => post.Published)
                .Skip(skip).Take(take)
                .ToListAsync();
        }

        public async Task<Dictionary<int, List<JournalArchivePost>>> GetPostsArchiveAsync()
        {
            return (await _db.JournalPosts
                .Where(PublishedPostsExpression)
                .Select(post => new JournalArchivePost
                {
                    Slug = post.Slug,
                    Title = post.Title,
                    Published = post.Published.Value
                })
                .OrderByDescending(post => post.Published)
                .ToListAsync())
                .GroupBy(post => post.Published.Year)
                .ToDictionary(post => post.Key, post => post.ToList());
        }

        public async Task<List<SyndicationItem>> GetPostsFeedItemsAsync(IUrlHelper urlHelper, string baseId, int? page = null)
        {
            return (await GetPostsAsync(0, int.MaxValue, allowUnpublished: false))
                .Select(post =>
                {
                    var item = new SyndicationItem
                    {
                        Id = baseId + $";id={post.Id}",
                        Title = new TextSyndicationContent(post.Title, TextSyndicationContentKind.Plaintext),
                        Content = new TextSyndicationContent(post.ToHtml(), TextSyndicationContentKind.Html),
                        PublishDate = new DateTimeOffset(post.Published.Value, TimeSpan.Zero),
                        LastUpdatedTime = new DateTimeOffset(post.Published.Value, TimeSpan.Zero),
                    };
                    item.Links.Add(
                        new SyndicationLink(
                            new Uri(_canonical.CanonicaliseUrl(urlHelper.Action("Read", "Journal", new { year = post.Published?.Year, slug = post.Slug })))));
                    return item;
                })
                .ToList();
        }
    }
}
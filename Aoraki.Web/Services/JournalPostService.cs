using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Aoraki.Web.Contracts;
using Aoraki.Web.Data.Context;
using Aoraki.Web.Data.Models;
using Aoraki.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace Aoraki.Web.Services
{
    public class JournalPostService : IJournalPostService
    {
        private readonly AorakiDbContext _db;

        public JournalPostService(AorakiDbContext db)
        {
            _db = db;
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
    }
}
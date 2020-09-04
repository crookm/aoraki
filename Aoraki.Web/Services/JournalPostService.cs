using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Aoraki.Web.Contracts;
using Aoraki.Web.Models;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Aoraki.Web.Services
{
    public class JournalPostService : IJournalPostService
    {
        private readonly IJournalSettings _journalSettings;
        private readonly IMongoCollection<JournalPost> _postCollection;

        public JournalPostService(IJournalSettings journalSettings)
        {
            _journalSettings = journalSettings;

            var client = new MongoClient(_journalSettings.DbConnection);
            var database = client.GetDatabase(_journalSettings.DbName);
            _postCollection = database.GetCollection<JournalPost>(_journalSettings.DbPostsCollection);
        }

        public async Task<JournalPost> GetPostAsync(string slug)
        {
            var filter = GetPublishedPostsFilter()
                         & Builders<JournalPost>.Filter.Eq(p => p.Slug, slug);
            return await _postCollection.Find(filter).FirstOrDefaultAsync();
        }

        public async Task<int> GetTotalPostCountAsync()
        {
            return (int) await _postCollection.CountDocumentsAsync(GetPublishedPostsFilter());
        }

        public async Task<IEnumerable<JournalPost>> GetPostsAsync(int skip, int take)
        {
            return await _postCollection.Find(GetPublishedPostsFilter())
                .SortByDescending(p => p.Published)
                .Skip(skip).Limit(take)
                .ToListAsync();
        }
        
        public async Task<Dictionary<int, List<JournalArchivePost>>> GetPostsArchiveAsync()
        {
            return (await _postCollection.Find(GetPublishedPostsFilter())
                .Project(p => new JournalArchivePost
                {
                    Slug = p.Slug,
                    Title = p.Title,
                    Published = p.Published
                })
                .SortByDescending(p => p.Published)
                .ToListAsync())
                .GroupBy(p => p.Published.Year, p => p)
                .ToDictionary(p => p.Key, p => p.ToList());
        }

        private static FilterDefinition<JournalPost> GetPublishedPostsFilter()
        {
            return Builders<JournalPost>.Filter
                .Where(p => p.Published != null && p.Published <= DateTime.UtcNow);
        }
    }
}
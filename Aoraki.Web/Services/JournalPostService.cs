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

        public async Task<string> CreatePostAsync(JournalPost post)
        {
            var id = ObjectId.GenerateNewId().ToString();
            await _postCollection.InsertOneAsync(new JournalPost
            {
                Id = id,
                Title = post.Title,
                Slug = post.Slug,
                Tags = post.Tags,
                Created = post.Created,
                Published = post.Published,
                Content = post.Content
            });

            return id;
        }

        public async Task UpdatePostAsync(string id, JournalPost post)
        {
            var filter = Builders<JournalPost>.Filter.Eq(p => p.Id, id);
            var update = Builders<JournalPost>.Update
                .Set(p => p.Title, post.Title)
                .Set(p => p.Slug, post.Slug)
                .Set(p => p.Tags, post.Tags)
                .Set(p => p.Published, post.Published)
                .Set(p => p.Content, post.Content);
            await _postCollection.UpdateOneAsync(filter, update);
        }
        
        public async Task<int> GetTotalPostCountAsync()
        {
            return (int) await _postCollection.CountDocumentsAsync(GetPublishedPostsFilter());
        }

        public async Task<JournalPost> GetPostByIdAsync(string id, bool allowUnpublished = false)
        {
            var filter = Builders<JournalPost>.Filter.Eq(p => p.Id, id);
            if (!allowUnpublished)
                filter &= GetPublishedPostsFilter();
            return await _postCollection.Find(filter).FirstOrDefaultAsync();
        }

        public async Task<JournalPost> GetPostBySlugAsync(string slug, bool allowUnpublished = false)
        {
            var filter = Builders<JournalPost>.Filter.Eq(p => p.Slug, slug);
            if (!allowUnpublished)
                filter &= GetPublishedPostsFilter();
            return await _postCollection.Find(filter).FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<JournalPost>> GetPostsAsync(int skip, int take, bool allowUnpublished = false)
        {
            var filter = Builders<JournalPost>.Filter.Empty;
            if (!allowUnpublished)
                filter &= GetPublishedPostsFilter();
            return await _postCollection.Find(filter)
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
                    Published = p.Published.Value
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
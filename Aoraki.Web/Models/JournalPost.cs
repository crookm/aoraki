using System;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Aoraki.Web.Models
{
    [Bind(
        nameof(JournalPost.Title),
        nameof(JournalPost.Slug),
        nameof(JournalPost.Tags),
        nameof(JournalPost.Lead),
        nameof(JournalPost.Content))]
    public class JournalPost
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        [BsonElement("title")]
        public string Title { get; set; }
        [BsonElement("slug")]
        public string Slug { get; set; }
        [BsonElement("tags")]
        public string[] Tags { get; set; }
        [BsonElement("created")]
        public DateTime Created { get; set; }
        [BsonElement("published")]
        public DateTime? Published { get; set; }
        [BsonElement("lead")]
        public string Lead { get; set; }
        [BsonElement("content")]
        public string Content { get; set; }
    }
}
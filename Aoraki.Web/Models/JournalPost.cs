using System;
using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Aoraki.Web.Models
{
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
        public DateTime Published { get; set; }
        [BsonElement("content")]
        public string Content { get; set; }
    }
}
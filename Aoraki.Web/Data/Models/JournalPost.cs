using System;
using Microsoft.AspNetCore.Mvc;

namespace Aoraki.Web.Data.Models
{
    [Bind(
        nameof(JournalPost.Title),
        nameof(JournalPost.Slug),
        nameof(JournalPost.Lead),
        nameof(JournalPost.Content))]
    public class JournalPost
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Slug { get; set; }
        public string Lead { get; set; }
        public string Content { get; set; }
        public DateTime Created { get; set; }
        public DateTime? Published { get; set; }
    }
}
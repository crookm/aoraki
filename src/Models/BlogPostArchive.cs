using System;

namespace Aoraki.Web.Models
{
    public class BlogPostArchive
    {
        public string Slug { get; set; }
        public string Title { get; set; }
        public DateTimeOffset Published { get; set; }
    }
}
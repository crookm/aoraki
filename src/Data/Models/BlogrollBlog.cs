using System;

namespace Aoraki.Web.Data.Models
{
    public class BlogrollBlog
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Url { get; set; }
        public string Description { get; set; }
        public DateTimeOffset Created { get; set; } = DateTimeOffset.UtcNow;
    }
}
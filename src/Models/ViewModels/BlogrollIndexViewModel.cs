using System.Collections.Generic;
using Aoraki.Web.Data.Models;

namespace Aoraki.Web.Models.ViewModels
{
    public class BlogrollIndexViewModel
    {
        public Pagination Pagination { get; set; }
        public IEnumerable<BlogrollBlog> Blogs { get; set; }
    }
}
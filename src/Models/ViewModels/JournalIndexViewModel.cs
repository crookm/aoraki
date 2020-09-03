using System.Collections.Generic;
using Aoraki.Web.Models.Entities;

namespace Aoraki.Web.Models.ViewModels
{
    public class JournalIndexViewModel
    {
        public Pagination Pagination { get; set; }
        public IEnumerable<BlogPost> Posts { get; set; }
    }
}
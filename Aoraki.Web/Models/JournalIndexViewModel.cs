using System.Collections.Generic;

namespace Aoraki.Web.Models
{
    public class JournalIndexViewModel
    {
        public Pagination Pagination { get; set; }
        public IEnumerable<JournalPost> Posts { get; set; }
    }
}
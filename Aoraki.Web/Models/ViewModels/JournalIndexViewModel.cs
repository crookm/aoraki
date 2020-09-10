using System.Collections.Generic;

namespace Aoraki.Web.Models.ViewModels
{
    public class JournalIndexViewModel
    {
        public Pagination Pagination { get; set; }
        public IEnumerable<JournalPost> Posts { get; set; }
    }
}
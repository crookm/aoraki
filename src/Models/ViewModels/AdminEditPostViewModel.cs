using Aoraki.Web.Data.Models;

namespace Aoraki.Web.Models.ViewModels
{
    public class AdminEditPostViewModel
    {
        public bool IsPublished { get; set; }
        public string Tags { get; set; }
        public JournalPost Post { get; set; }
    }
}
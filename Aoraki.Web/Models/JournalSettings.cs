using Aoraki.Web.Contracts;

namespace Aoraki.Web.Models
{
    public class JournalSettings : IJournalSettings
    {
        public string DbConnection { get; set; }
        public string DbName { get; set; }
        public string DbPostsCollection { get; set; }
    }
}
using System.Collections.Generic;
using System.Threading.Tasks;
using Aoraki.Web.Models;

namespace Aoraki.Web.Contracts
{
    public interface IJournalPostService
    {
        Task<JournalPost> GetPostAsync(string slug);
        Task<int> GetTotalPostCountAsync();
        Task<IEnumerable<JournalPost>> GetPostsAsync(int skip, int take);
        Task<Dictionary<int, List<JournalArchivePost>>> GetPostsArchiveAsync();
    }
}
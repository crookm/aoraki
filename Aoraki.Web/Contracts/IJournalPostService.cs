using System.Collections.Generic;
using System.Threading.Tasks;
using Aoraki.Web.Models;

namespace Aoraki.Web.Contracts
{
    public interface IJournalPostService
    {
        Task<string> CreatePostAsync(JournalPost post);
        Task UpdatePostAsync(string id, JournalPost post);

        Task<int> GetTotalPostCountAsync();
        Task<JournalPost> GetPostByIdAsync(string id, bool allowUnpublished = false);
        Task<JournalPost> GetPostBySlugAsync(string slug, bool allowUnpublished = false);
        Task<IEnumerable<JournalPost>> GetPostsAsync(int skip, int take, bool allowUnpublished = false);
        Task<Dictionary<int, List<JournalArchivePost>>> GetPostsArchiveAsync();
    }
}
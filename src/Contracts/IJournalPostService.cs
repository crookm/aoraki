using System.Collections.Generic;
using System.ServiceModel.Syndication;
using System.Threading.Tasks;
using Aoraki.Web.Data.Models;
using Aoraki.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace Aoraki.Web.Contracts
{
    public interface IJournalPostService
    {
        Task<int> CreatePostAsync(JournalPost post);
        Task UpdatePostAsync(JournalPost post);

        Task<int> GetTotalPostCountAsync();
        Task<JournalPost> GetPostByIdAsync(int id, bool allowUnpublished = false);
        Task<JournalPost> GetPostBySlugAsync(string slug, bool allowUnpublished = false);
        Task<IEnumerable<JournalPost>> GetPostsAsync(int skip, int take, bool allowUnpublished = false);
        Task<Dictionary<int, List<JournalArchivePost>>> GetPostsArchiveAsync();
        Task<List<SyndicationItem>> GetPostsFeedItemsAsync(IUrlHelper urlHelper, string baseId, int? page = null);
    }
}
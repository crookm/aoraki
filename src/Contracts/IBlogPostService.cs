using System.Collections.Generic;
using System.ServiceModel.Syndication;
using System.Threading;
using System.Threading.Tasks;
using Aoraki.Web.Models;
using Aoraki.Web.Models.Entities;
using Microsoft.AspNetCore.Mvc;

namespace Aoraki.Web.Contracts
{
    public interface IBlogPostService
    {
        Task<int> GetTotalPostCountAsync(CancellationToken token = default);

        Task<BlogPost> GetPostBySlugAsync(string year, string slug, bool allowUnpublished = false,
            CancellationToken token = default);

        Task<IEnumerable<BlogPost>> GetPostsAsync(int skip, int take, bool allowUnpublished = false,
            CancellationToken token = default);

        Task<Dictionary<int, List<BlogPostArchive>>> GetPostsArchiveAsync(CancellationToken token = default);

        Task<List<SyndicationItem>> GetPostsFeedItemsAsync(IUrlHelper urlHelper, string baseId, int? page = null,
            CancellationToken token = default);
    }
}
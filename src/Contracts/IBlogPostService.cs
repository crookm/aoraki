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
        /// <summary>
        /// Get the total number of posts
        /// </summary>
        /// <param name="allowUnpublished">Specify if the count should include posts which are not published, defaults to false</param>
        /// <param name="token">A cancellation token</param>
        /// <returns>The total number of blog posts</returns>
        Task<int> GetTotalPostCountAsync(bool allowUnpublished = false, CancellationToken token = default);

        /// <summary>
        /// Get a blog post by its year and slug
        /// </summary>
        /// <remarks>The slug may be re-used over years, so it must be specified</remarks>
        /// <param name="year">The year to look for the post with the matching slug</param>
        /// <param name="slug">The slug to look up the post by</param>
        /// <param name="allowUnpublished">Specify if unpublished posts should be retrieved</param>
        /// <param name="token">A cancellation token</param>
        /// <returns>The specified blog post, or null if it does not exist</returns>
        Task<BlogPost?> GetPostBySlugAsync(string year, string slug, bool allowUnpublished = false,
            CancellationToken token = default);

        /// <summary>
        /// Get a paged list of blog posts
        /// </summary>
        /// <param name="skip">The number of entries to skip from the start</param>
        /// <param name="take">The number of entries to retrieve</param>
        /// <param name="allowUnpublished">Specify if unpublished posts should be included in the results</param>
        /// <param name="token">A cancellation token</param>
        /// <returns>A collection of blog posts</returns>
        Task<IEnumerable<BlogPost>> GetPostsAsync(int skip, int take, bool allowUnpublished = false,
            CancellationToken token = default);

        /// <summary>
        /// Get all published posts, grouped by year
        /// </summary>
        /// <param name="token">A cancellation token</param>
        /// <returns>A collection of blog posts grouped by year</returns>
        Task<Dictionary<int, List<BlogPostArchive>>> GetPostsArchiveAsync(CancellationToken token = default);

        /// <summary>
        /// Get a paged list of blog posts in syndicated feed format
        /// </summary>
        /// <param name="urlHelper">The URL helper from MVC which allows routes to be figured out</param>
        /// <param name="baseId">The base identifier of the RSS/ATOM feed, which should never change</param>
        /// <param name="page">The page of results to display</param>
        /// <param name="token">A cancellation token</param>
        /// <returns>A collection of blog posts in syndicated feed format</returns>
        Task<List<SyndicationItem>> GetPostsFeedItemsAsync(IUrlHelper urlHelper, string baseId, int? page = null,
            CancellationToken token = default);
    }
}
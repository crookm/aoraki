using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Aoraki.Web.Models.Entities;

namespace Aoraki.Web.Contracts;

public interface IBlogrollService
{
    /// <summary>
    /// Get the total number of published blogroll blog entries
    /// </summary>
    /// <param name="token">A cancellation token</param>
    /// <returns>The total number of blogroll entries</returns>
    Task<int> GetTotalEntryCountAsync(CancellationToken token = default);

    /// <summary>
    /// Get a paged list of blogroll blog entries
    /// </summary>
    /// <param name="skip">The number of entries to skip from the start</param>
    /// <param name="take">The number of entries to retrieve</param>
    /// <param name="token">A cancellation token</param>
    /// <returns>A collection of blogroll entries, limited by the specified parameters</returns>
    Task<IEnumerable<BlogrollBlog>> GetEntriesAsync(int skip, int take, CancellationToken token = default);
}
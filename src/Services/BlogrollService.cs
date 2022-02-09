using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Aoraki.Web.Contracts;
using Aoraki.Web.Models.Entities;
using Azure.Data.Tables;

namespace Aoraki.Web.Services;

public class BlogrollService : IBlogrollService
{
    private readonly TableClient _tableClient;

    public BlogrollService(IStorageFactory storageFactory)
    {
        _tableClient = storageFactory.GetTableClient("blogroll");
    }

    public async Task<int> GetTotalEntryCountAsync(CancellationToken token = default)
    {
        var query = _tableClient.QueryAsync<BlogrollBlog>(select: new[] { nameof(BlogrollBlog.RowKey) },
            cancellationToken: token);
        return await query.CountAsync(token);
    }

    public async Task<IEnumerable<BlogrollBlog>> GetEntriesAsync(int skip, int take, CancellationToken token = default)
    {
        var query = _tableClient.QueryAsync<BlogrollBlog>(cancellationToken: token);
        return await query.OrderByDescending(blog => blog.Timestamp)
            .Skip(skip).Take(take)
            .ToListAsync(token);
    }
}
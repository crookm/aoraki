using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Threading;
using System.Threading.Tasks;
using Aoraki.Web.Contracts;
using Aoraki.Web.Extensions;
using Aoraki.Web.Models;
using Aoraki.Web.Models.Entities;
using Azure;
using Azure.Data.Tables;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Aoraki.Web.Services;

public class BlogPostService : IBlogPostService
{
    private readonly ICanonicalService _canonical;
    private readonly TableClient _tableClient;

    public BlogPostService(IStorageFactory storageFactory, ICanonicalService canonical)
    {
        _canonical = canonical;
        _tableClient = storageFactory.GetTableClient("blogposts");
    }

    public async Task<int> GetTotalPostCountAsync(bool allowUnpublished = false, CancellationToken token = default)
    {
        var query = _tableClient.QueryAsync<BlogPost>(
            post => allowUnpublished || post.Published <= DateTimeOffset.Now, null,
            new[] { nameof(BlogPost.Published) }, token);

        return await query.CountAsync(token);
    }

    public async Task<BlogPost?> GetPostBySlugAsync(string year, string slug, bool allowUnpublished = false,
        CancellationToken token = default)
    {
        try
        {
            var post = await _tableClient.GetEntityAsync<BlogPost>(year, slug, cancellationToken: token);
            return allowUnpublished || post.Value.Published <= DateTimeOffset.Now
                ? post.Value
                : null;
        }
        catch (RequestFailedException e) when (e.Status == StatusCodes.Status404NotFound)
        {
            return null;
        }
    }

    public async Task<IEnumerable<BlogPost>> GetPostsAsync(int skip, int take, bool allowUnpublished = false,
        CancellationToken token = default)
    {
        var query = _tableClient.QueryAsync<BlogPost>(
            post => allowUnpublished || post.Published <= DateTimeOffset.Now, cancellationToken: token);

        return await query.OrderByDescending(post => post.Published)
            .Skip(skip).Take(take)
            .ToListAsync(token);
    }

    public async Task<Dictionary<int, List<BlogPostArchive>>> GetPostsArchiveAsync(
        CancellationToken token = default)
    {
        var query = _tableClient.QueryAsync<BlogPost>(post => post.Published <= DateTimeOffset.Now, null,
            new[] { nameof(BlogPost.RowKey), nameof(BlogPost.Title), nameof(BlogPost.Published) }, token);
        return (await query.Select(post => new BlogPostArchive
                {
                    Slug = post.RowKey,
                    Title = post.Title,
                    Published = post.Published ?? DateTimeOffset.MinValue
                })
                .OrderByDescending(post => post.Published)
                .ToListAsync(token))
            .GroupBy(post => post.Published.Year)
            .ToDictionary(post => post.Key, post => post.ToList());
    }

    public async Task<List<SyndicationItem>> GetPostsFeedItemsAsync(IUrlHelper urlHelper, string baseId,
        int? page = null, CancellationToken token = default)
    {
        return (await GetPostsAsync(0, int.MaxValue, allowUnpublished: false, token))
            .Select(post =>
            {
                var item = new SyndicationItem
                {
                    Id = baseId + $";id={post.PartitionKey}+{post.RowKey}",
                    Title = new TextSyndicationContent(post.Title, TextSyndicationContentKind.Plaintext),
                    Content = new TextSyndicationContent(post.ToHtml(), TextSyndicationContentKind.Html),
                    PublishDate = post.Published ?? DateTimeOffset.MinValue,
                    LastUpdatedTime = post.Published ?? DateTimeOffset.MinValue
                };
                item.Links.Add(
                    new SyndicationLink(
                        new Uri(_canonical.CanonicaliseUrl(urlHelper.Action("Read", "Journal",
                            new { year = post.Published?.Year, slug = post.RowKey })))));
                return item;
            })
            .ToList();
    }
}
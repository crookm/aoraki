using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Aoraki.Web.Contracts;
using Aoraki.Web.Extensions;
using Aoraki.Web.Models;
using Aoraki.Web.Models.Entities;
using Azure;
using Azure.Data.Tables;
using Microsoft.AspNetCore.Http;

namespace Aoraki.Web.Services;

public class JournalReactionService : IJournalReactionService
{
    private readonly IJournalService _journalService;
    private readonly TableClient _reactTableClient;
    private readonly TableClient _reactAuditTableClient;

    public JournalReactionService(IStorageFactory storageFactory, IJournalService journalService)
    {
        _journalService = journalService;
        _reactTableClient = storageFactory.GetTableClient("blogreact");
        _reactAuditTableClient = storageFactory.GetTableClient("blogreactaudit");
    }

    public async Task<bool> PostReactionAsync(string year, string slug, string ipAddress, Reaction reaction,
        int attempts = 0, CancellationToken token = default)
    {
        attempts += 1;
        var post = await _journalService.GetPostBySlugAsync(year, slug, token: token);
        if (post == null) return false;

        year = post.PartitionKey;
        slug = post.RowKey;

        try
        {
            await _reactAuditTableClient.GetEntityAsync<BlogPostReactionAudit>($"{year}_{slug}", ipAddress,
                cancellationToken: token);
            return false;
        }
        catch (RequestFailedException e) when (e.Status == StatusCodes.Status404NotFound)
        {
            // The specified IP address has not yet reacted to this post, continue
        }

        try
        {
            var postReaction =
                await _reactTableClient.GetEntityAsync<BlogPostReactions>(year, slug, cancellationToken: token);
            postReaction.Value.ApplyReaction(reaction);
            await _reactTableClient.UpdateEntityAsync(postReaction.Value, postReaction.Value.ETag,
                cancellationToken: token);
        }
        catch (RequestFailedException e) when (e.Status == StatusCodes.Status412PreconditionFailed)
        {
            // The row been modified after it had been loaded
            if (attempts > 3) return false;
            return await PostReactionAsync(year, slug, ipAddress, reaction, attempts, token);
        }
        catch (RequestFailedException e) when (e.Status == StatusCodes.Status404NotFound)
        {
            // There is not yet a record of reactions for this post
            await _reactTableClient.AddEntityAsync(new BlogPostReactions
            {
                PartitionKey = year,
                RowKey = slug
            }.ApplyReaction(reaction), token);
        }

        await _reactAuditTableClient.AddEntityAsync(new BlogPostReactionAudit
        {
            PartitionKey = $"{year}_{slug}",
            RowKey = ipAddress,
            Reaction = reaction
        }, token);

        return true;
    }

    public async Task<Dictionary<Reaction, int>?> GetReactionsAsync(string year, string slug,
        CancellationToken token = default)
    {
        try
        {
            var reactions =
                (await _reactTableClient.GetEntityAsync<BlogPostReactions>(year, slug, cancellationToken: token))
                .Value;
            return new Dictionary<Reaction, int>
            {
                [Reaction.Like] = reactions.ReactLike,
                [Reaction.Useful] = reactions.ReactUseful,
                [Reaction.Outdated] = reactions.ReactOutdated,
                [Reaction.Educational] = reactions.ReactEducational
            };
        }
        catch (RequestFailedException e) when (e.Status == StatusCodes.Status404NotFound)
        {
            return null;
        }
    }
}
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Aoraki.Web.Models;

namespace Aoraki.Web.Contracts;

public interface IJournalReactionService
{
    /// <summary>
    /// Submit a reaction to a journal post
    /// </summary>
    /// <remarks>A single IP address may only react to each unique post once</remarks>
    /// <param name="year">The year of the post to react to</param>
    /// <param name="slug">The slug of the post to react to</param>
    /// <param name="ipAddress">The IP address of the user reacting, may be v4 or v6</param>
    /// <param name="reaction">The reaction to post</param>
    /// <param name="attempts">The current number of attempts to save the reaction, this method is recursive and will stop after three attempts</param>
    /// <param name="token">A cancellation token</param>
    /// <returns>True if the reaction was successfully posted, false if the IP address has already reacted, or the post could not be found, or if the number of attempts is more than three</returns>
    Task<bool> PostReactionAsync(string year, string slug, string ipAddress, Reaction reaction, int attempts = 0,
        CancellationToken token = default);

    /// <summary>
    /// Gets all the reactions to a journal post
    /// </summary>
    /// <param name="year">The year of the post to get the reactions for</param>
    /// <param name="slug">The slug of the post to get the reactions for</param>
    /// <param name="token">A cancellation token</param>
    /// <returns>The number of reactions for each reaction type, or null if the post could not be found</returns>
    Task<Dictionary<Reaction, int>?> GetReactionsAsync(string year, string slug, CancellationToken token = default);
}
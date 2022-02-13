using System;
using Aoraki.Web.Models;
using Aoraki.Web.Models.Entities;

namespace Aoraki.Web.Extensions;

public static class BlogPostReactionsExtensions
{
    public static BlogPostReactions ApplyReaction(this BlogPostReactions reactions, Reaction reaction)
    {
        switch (reaction)
        {
            case Reaction.Like:
                reactions.ReactLike++;
                break;
            case Reaction.Useful:
                reactions.ReactUseful++;
                break;
            case Reaction.Outdated:
                reactions.ReactOutdated++;
                break;
            case Reaction.Educational:
                reactions.ReactEducational++;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(reaction), reaction, "unsupported reaction");
        }

        return reactions;
    }
}
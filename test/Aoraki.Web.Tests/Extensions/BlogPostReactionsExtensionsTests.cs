using Aoraki.Web.Extensions;
using Aoraki.Web.Models;
using Aoraki.Web.Models.Entities;
using FluentAssertions;
using Xunit;

namespace Aoraki.Web.Tests.Extensions;

public class BlogPostReactionsExtensionsTests
{
    [Fact]
    public void ApplyReaction_ShouldIncrementLikeValue()
    {
        var react = new BlogPostReactions().ApplyReaction(Reaction.Like);
        react.ReactLike.Should().Be(1);
        react.ReactEducational.Should().Be(0);
        react.ReactOutdated.Should().Be(0);
        react.ReactUseful.Should().Be(0);
    }

    [Fact]
    public void ApplyReaction_ShouldIncrementUsefulValue()
    {
        var react = new BlogPostReactions().ApplyReaction(Reaction.Useful);
        react.ReactLike.Should().Be(0);
        react.ReactEducational.Should().Be(0);
        react.ReactOutdated.Should().Be(0);
        react.ReactUseful.Should().Be(1);
    }

    [Fact]
    public void ApplyReaction_ShouldIncrementOutdatedValue()
    {
        var react = new BlogPostReactions().ApplyReaction(Reaction.Outdated);
        react.ReactLike.Should().Be(0);
        react.ReactEducational.Should().Be(0);
        react.ReactOutdated.Should().Be(1);
        react.ReactUseful.Should().Be(0);
    }

    [Fact]
    public void ApplyReaction_ShouldIncrementEducationalValue()
    {
        var react = new BlogPostReactions().ApplyReaction(Reaction.Educational);
        react.ReactLike.Should().Be(0);
        react.ReactEducational.Should().Be(1);
        react.ReactOutdated.Should().Be(0);
        react.ReactUseful.Should().Be(0);
    }
}
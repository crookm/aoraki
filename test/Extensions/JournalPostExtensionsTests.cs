using Aoraki.Web.Extensions;
using Aoraki.Web.Models.Entities;
using FluentAssertions;
using Xunit;

namespace Aoraki.Web.Tests.Extensions;

public class JournalPostExtensionsTests
{
    [Fact]
    public void ToPlainText_ShouldReturnValidResult()
        => new BlogPost { Content = "**[testing]()**" }.ToPlainText()
            .Should().Be("testing\n");

    [Fact]
    public void ToHtml_ShouldReturnValidResult()
        => new BlogPost { Content = "**[testing]()**" }.ToHtml()
            .Should().Be("<p><strong><a href=\"\">testing</a></strong></p>\n");
}
using Aoraki.Web.Data.Models;
using Aoraki.Web.Extensions;
using FluentAssertions;
using Xunit;

namespace Aoraki.Web.Tests.Extensions
{
    public class JournalPostExtensionsTests
    {
        [Fact]
        public void ToPlainText_ConvertsToPlainTextFromMarkdown()
        {
            var post = new JournalPost
            {
                Content = @"# Title
Some regular content.

[a link](https://example.com)"
            };

            var result = post.ToPlainText();

            result.Should().Be("Title\nSome regular content.\na link\n");
        }

        [Fact]
        public void ToHtml_ConvertsToHtmlFromMarkdown()
        {
            var post = new JournalPost
            {
                Content = @"# Title
Some regular content.

[a link](https://example.com)"
            };

            var result = post.ToHtml();

            result.Should().Be("<h1>Title</h1>\n<p>Some regular content.</p>\n<p><a href=\"https://example.com\">a link</a></p>\n");
        }
    }
}
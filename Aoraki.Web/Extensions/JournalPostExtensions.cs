using Aoraki.Web.Models;
using Markdig;

namespace Aoraki.Web.Extensions
{
    public static class JournalPostExtensions
    {
        public static string ToPlainText(this JournalPost post)
        {
            var pipeline = new MarkdownPipelineBuilder().Build();
            return Markdown.ToPlainText(post.Content, pipeline);
        }

        public static string ToHtml(this JournalPost post)
        {
            var pipeline = new MarkdownPipelineBuilder().Build();
            return Markdown.ToHtml(post.Content, pipeline);
        }
    }
}
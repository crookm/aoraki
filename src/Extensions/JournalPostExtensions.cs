using Aoraki.Web.Models.Entities;
using Markdig;

namespace Aoraki.Web.Extensions
{
    public static class JournalPostExtensions
    {
        public static string ToPlainText(this BlogPost post)
        {
            var pipeline = new MarkdownPipelineBuilder().Build();
            return Markdown.ToPlainText(post.Content, pipeline);
        }

        public static string ToHtml(this BlogPost post)
        {
            var pipeline = new MarkdownPipelineBuilder().Build();
            return Markdown.ToHtml(post.Content, pipeline);
        }
    }
}
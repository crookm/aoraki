using System;
using System.Diagnostics.CodeAnalysis;

namespace Aoraki.Web.Models;

[ExcludeFromCodeCoverage] // No need to cover simple objects
public class BlogPostArchive
{
    public string Slug { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public DateTimeOffset Published { get; set; }
}
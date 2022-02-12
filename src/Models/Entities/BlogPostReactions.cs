using System;
using System.Diagnostics.CodeAnalysis;
using Azure;
using Azure.Data.Tables;

namespace Aoraki.Web.Models.Entities;

[ExcludeFromCodeCoverage] // No need to cover simple objects
public class BlogPostReactions : ITableEntity
{
    public string PartitionKey { get; set; } = string.Empty;
    public string RowKey { get; set; } = string.Empty;
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }

    public int ReactLike { get; set; }
    public int ReactUseful { get; set; }
    public int ReactOutdated { get; set; }
    public int ReactEducational { get; set; }
}
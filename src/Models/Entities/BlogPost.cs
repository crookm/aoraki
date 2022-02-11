using System;
using System.Diagnostics.CodeAnalysis;
using Azure;
using Azure.Data.Tables;

namespace Aoraki.Web.Models.Entities;

[ExcludeFromCodeCoverage] // No need to cover simple objects
public class BlogPost : ITableEntity
{
    public string PartitionKey { get; set; } = string.Empty;
    public string RowKey { get; set; } = string.Empty;
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }

    public DateTimeOffset? Published { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Lead { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
}
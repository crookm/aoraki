using System;
using Azure;
using Azure.Data.Tables;

namespace Aoraki.Web.Models.Entities;

public class BlogPost : ITableEntity
{
    public string PartitionKey { get; set; }
    public string RowKey { get; set; }
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }
        
    public DateTimeOffset? Published { get; set; }
    public string Title { get; set; }
    public string Lead { get; set; }
    public string Content { get; set; }
}
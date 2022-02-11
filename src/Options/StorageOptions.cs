using System.Diagnostics.CodeAnalysis;

namespace Aoraki.Web.Options;

[ExcludeFromCodeCoverage]
public class StorageOptions
{
    public const string HierarchyName = "Storage";
    public string ConnectionString { get; set; } = string.Empty;
}
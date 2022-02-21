using System.Diagnostics.CodeAnalysis;

namespace Aoraki.Web.Options;

[ExcludeFromCodeCoverage]
public class StorageOptions
{
    public const string HierarchyName = "Storage";
    public string AccountName { get; set; } = string.Empty;
}
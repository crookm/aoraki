using System.Diagnostics.CodeAnalysis;

namespace Aoraki.Web.Options;

[ExcludeFromCodeCoverage]
public class ConnectionStrings
{
    public const string HierarchyName = "ConnectionStrings";
    public string Storage { get; set; } = string.Empty;
}
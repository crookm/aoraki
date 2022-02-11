using System.Diagnostics.CodeAnalysis;

namespace Aoraki.Web.Models.ViewModels;

[ExcludeFromCodeCoverage] // No need to cover simple objects
public class ErrorViewModel
{
    public string RequestId { get; set; } = string.Empty;

    public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
}
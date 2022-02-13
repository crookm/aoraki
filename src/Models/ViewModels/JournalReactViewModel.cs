using System.Diagnostics.CodeAnalysis;

namespace Aoraki.Web.Models.ViewModels;

[ExcludeFromCodeCoverage] // No need to cover simple objects
public class JournalReactViewModel
{
    public string Year { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public bool Success { get; set; }
}
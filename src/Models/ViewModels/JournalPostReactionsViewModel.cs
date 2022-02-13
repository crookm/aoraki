using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Aoraki.Web.Models.ViewModels;

[ExcludeFromCodeCoverage] // No need to cover simple objects
public class JournalPostReactionsViewModel
{
    public string PostYear { get; set; } = string.Empty;
    public string PostSlug { get; set; } = string.Empty;
    public Dictionary<Reaction, int>? Reactions { get; set; }
}
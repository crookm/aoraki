using System.Collections.Generic;

namespace Aoraki.Web.Models.ViewModels;

public class JournalPostReactionsViewModel
{
    public string PostYear { get; set; } = string.Empty;
    public string PostSlug { get; set; } = string.Empty;
    public Dictionary<Reaction, int>? Reactions { get; set; }
}
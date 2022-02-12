using Aoraki.Web.Models.Entities;

namespace Aoraki.Web.Models.ViewModels;

public class JournalPostReadViewModel
{
    public BlogPost Post { get; set; } = null!;
    public JournalPostReactionsViewModel Reactions { get; set; } = null!;
}
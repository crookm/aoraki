using System.Diagnostics.CodeAnalysis;
using Aoraki.Web.Models.Entities;

namespace Aoraki.Web.Models.ViewModels;

[ExcludeFromCodeCoverage] // No need to cover simple objects
public class JournalPostReadViewModel
{
    public BlogPost Post { get; set; } = null!;
    public JournalPostReactionsViewModel Reactions { get; set; } = null!;
}
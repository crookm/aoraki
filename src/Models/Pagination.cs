using System.Diagnostics.CodeAnalysis;

namespace Aoraki.Web.Models;

[ExcludeFromCodeCoverage] // No need to cover simple objects
public class Pagination
{
    public int CurrentPage { get; set; }
    public int TotalPages { get; set; }
}
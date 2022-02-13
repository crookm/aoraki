using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Aoraki.Web.Models.Entities;

namespace Aoraki.Web.Models.ViewModels;

[ExcludeFromCodeCoverage] // No need to cover simple objects
public class BlogrollIndexViewModel
{
    public Pagination Pagination { get; set; } = null!;
    public IEnumerable<BlogrollBlog> Blogs { get; set; } = Array.Empty<BlogrollBlog>();
}
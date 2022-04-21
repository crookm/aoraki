using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Aoraki.Contracts.Projects;
using Aoraki.Web.Models.Entities;

namespace Aoraki.Web.Models.ViewModels;

[ExcludeFromCodeCoverage] // No need to cover simple objects
public class HomeIndexViewModel
{
    public IEnumerable<BlogPost> RecentPosts { get; set; } = Array.Empty<BlogPost>();
    public IEnumerable<IProjectDefinition> RecentProjects { get; set; } = Array.Empty<IProjectDefinition>();
}
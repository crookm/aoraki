using System;
using System.Collections.Generic;
using System.Linq;
using Aoraki.Contracts.Projects;
using Microsoft.AspNetCore.Mvc;

namespace Aoraki.Web.Controllers;

[Route("[controller]")]
public class ProjectsController : Controller
{
    private readonly IEnumerable<IProjectDefinition> _projectDefinitions;

    public ProjectsController(IEnumerable<IProjectDefinition> projectDefinitions)
    {
        _projectDefinitions = projectDefinitions;
    }

    public IActionResult Index()
        => View(_projectDefinitions.OrderByDescending(p => p.UpdatedProject).ToList());

    [Route("{projectId}")]
    public IActionResult Launch(string projectId)
    {
        var project =
            _projectDefinitions.FirstOrDefault(p => p.Id.Equals(projectId, StringComparison.OrdinalIgnoreCase));
        if (project == null) return NotFound();

        ViewData["title"] = project.DisplayName.ToLower();
        return View(project);
    }
}
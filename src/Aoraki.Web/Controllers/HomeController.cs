using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Aoraki.Contracts.Projects;
using Aoraki.Web.Contracts;
using Aoraki.Web.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace Aoraki.Web.Controllers;

[ExcludeFromCodeCoverage] // Remove exclusion if endpoints with something worth testing are added
public class HomeController : Controller
{
    private const int RecentEntryCount = 3;

    private readonly IJournalService _journalService;
    private readonly IEnumerable<IProjectDefinition> _projectDefinitions;

    public HomeController(IJournalService journalService, IEnumerable<IProjectDefinition> projectDefinitions)
    {
        _journalService = journalService;
        _projectDefinitions = projectDefinitions;
    }

    [ResponseCache(Duration = Constants.CacheDurationHomeIndex)]
    public async Task<IActionResult> Index(CancellationToken token = default)
    {
        var posts = await _journalService.GetPostsAsync(0, RecentEntryCount, token: token);
        var projects = _projectDefinitions.OrderByDescending(p => p.UpdatedProject);

        return View(new HomeIndexViewModel
        {
            RecentPosts = posts,
            RecentProjects = projects
        });
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
        => View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });

    [HttpGet("/Home/Error/404")]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error404() => View("~/Views/Home/Error/Error404.cshtml");
}
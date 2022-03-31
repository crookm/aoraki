using System;
using System.Threading;
using System.Threading.Tasks;
using Aoraki.Web.Contracts;
using Aoraki.Web.Models;
using Aoraki.Web.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace Aoraki.Web.Controllers;

public class BlogrollController : Controller
{
    internal const int EntriesPerPage = 5;

    private readonly IBlogrollService _blogrollService;

    public BlogrollController(IBlogrollService blogrollService)
    {
        _blogrollService = blogrollService;
    }

    [ResponseCache(Duration = Constants.CacheDurationBlogrollIndex, VaryByQueryKeys = new[] { "page" })]
    public async Task<IActionResult> Index(int page = 1, CancellationToken token = default)
    {
        var blogsCount = await _blogrollService.GetTotalEntryCountAsync(token);
        var totalPages = (int)Math.Ceiling((decimal)blogsCount / EntriesPerPage);

        if (page < 1)
            return RedirectToAction(nameof(Index), new { page = 1 });
        if (page > totalPages)
            return RedirectToAction(nameof(Index), new { page = totalPages });

        return View(new BlogrollIndexViewModel
        {
            Pagination = new Pagination { CurrentPage = page, TotalPages = totalPages },
            Blogs = await _blogrollService.GetEntriesAsync(
                skip: (page - 1) * EntriesPerPage, take: EntriesPerPage, token)
        });
    }
}
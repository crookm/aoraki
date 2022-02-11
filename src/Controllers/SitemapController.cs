using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Aoraki.Web.Contracts;
using Microsoft.AspNetCore.Mvc;
using SimpleMvcSitemap;

namespace Aoraki.Web.Controllers;

[ExcludeFromCodeCoverage] // Sitemap builder writes direct to response stream, not testable
public class SitemapController : Controller
{
    private readonly IJournalService _journalService;

    public SitemapController(IJournalService journalService)
    {
        _journalService = journalService;
    }

    [HttpGet("/sitemap.xml")]
    [ResponseCache(Duration = 14400)]
    public IActionResult Index()
        => new SitemapProvider(new BaseUrlProvider())
            .CreateSitemapIndex(new SitemapIndexModel(new List<SitemapIndexNode>
            {
                new(Url.Action("Pages", "Sitemap")),
                new(Url.Action("Posts", "Sitemap")),
            }));

    [HttpGet("/sitemap-pages.xml")]
    [ResponseCache(Duration = 14400)]
    public IActionResult Pages()
        => new SitemapProvider(new BaseUrlProvider())
            .CreateSitemap(new SitemapModel(new List<SitemapNode>
            {
                new(Url.Action("Index", "Journal")) { ChangeFrequency = ChangeFrequency.Daily },
                new(Url.Action("Archive", "Journal")) { ChangeFrequency = ChangeFrequency.Daily },
                new(Url.Action("Index", "Blogroll")) { ChangeFrequency = ChangeFrequency.Weekly },
                new(Url.Action("Colophon", "Pages")) { ChangeFrequency = ChangeFrequency.Yearly },
            }));

    [HttpGet("/sitemap-posts.xml")]
    [ResponseCache(Duration = 14400)]
    public async Task<IActionResult> Posts()
        => new SitemapProvider(new BaseUrlProvider())
            .CreateSitemap(new SitemapModel(
                (await _journalService.GetPostsArchiveAsync())
                .SelectMany(posts => posts.Value)
                .Select(post =>
                    new SitemapNode(
                        Url.Action("Read", "Journal", new { post.Published.Year, post.Slug }))
                    {
                        LastModificationDate = post.Published.UtcDateTime
                    })
                .ToList()));
}
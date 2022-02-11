using System;
using System.ServiceModel.Syndication;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Aoraki.Web.Contracts;
using Aoraki.Web.Extensions;
using Aoraki.Web.Models;
using Aoraki.Web.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace Aoraki.Web.Controllers;

public class JournalController : Controller
{
    internal const int EntriesPerPage = 10;

    private readonly IJournalService _journalService;

    public JournalController(IJournalService journalService)
    {
        _journalService = journalService;
    }

    [ResponseCache(Duration = 14400, VaryByQueryKeys = new[] { "page" })]
    public async Task<IActionResult> Index(int page = 1, CancellationToken token = default)
    {
        var totalPosts = await _journalService.GetTotalPostCountAsync(token: token);
        var totalPages = (int)Math.Ceiling((decimal)totalPosts / EntriesPerPage);

        if (page < 1)
            return RedirectToAction(nameof(Index), new { page = 1 });
        if (page > totalPages)
            return RedirectToAction(nameof(Index), new { page = totalPages });

        return View(new JournalIndexViewModel
        {
            Pagination = new Pagination { CurrentPage = page, TotalPages = totalPages },
            Posts = await _journalService.GetPostsAsync((page - 1) * EntriesPerPage, EntriesPerPage, token: token)
        });
    }

    [ResponseCache(Duration = 14400)]
    public async Task<IActionResult> Archive(CancellationToken token = default)
        => View(await _journalService.GetPostsArchiveAsync(token));

    [ResponseCache(Duration = 604800)]
    public async Task<IActionResult> Read(string year, string slug, CancellationToken token = default)
    {
        var post = await _journalService.GetPostBySlugAsync(year, slug, false, token);
        if (post == null) return NotFound();
        return View(post);
    }

    [HttpGet("journal/{year}/{slug}.txt")]
    [Produces("text/plain")]
    [ResponseCache(Duration = 604800)]
    public async Task<IActionResult> ReadPlaintext(string year, string slug, CancellationToken token = default)
    {
        var post = await _journalService.GetPostBySlugAsync(year, slug, false, token);
        if (post == null) return NotFound();
        return Ok(post.ToPlainText());
    }

    [HttpGet("journal/{year}/{slug}.md")]
    [Produces("text/plain")]
    [ResponseCache(Duration = 604800)]
    public async Task<IActionResult> ReadMarkdown(string year, string slug, CancellationToken token = default)
    {
        var post = await _journalService.GetPostBySlugAsync(year, slug, false, token);
        if (post == null) return NotFound();
        return Ok(post.Content);
    }

    [HttpGet("journal/{year}/{slug}.json")]
    [Produces("application/json")]
    [ResponseCache(Duration = 604800)]
    public async Task<IActionResult> ReadJson(string year, string slug, CancellationToken token = default)
    {
        var post = await _journalService.GetPostBySlugAsync(year, slug, false, token);
        if (post == null) return NotFound();
        return Json(post);
    }

    [HttpGet("/atom.xml")]
    [ResponseCache(Duration = 14400)]
    public async Task<IActionResult> AtomFeed(CancellationToken token = default)
    {
        var feed = await SetupSyndicationFeed(token);
        await using var sw = new StringWriterWithEncoding(Encoding.UTF8);
        await using (var writer = XmlWriter.Create(sw, new XmlWriterSettings { Indent = true, Async = true }))
            feed.GetAtom10Formatter().WriteTo(writer);

        return Content(sw.ToString(), "application/atom+xml", Encoding.UTF8);
    }

    [HttpGet("/rss.xml")]
    [ResponseCache(Duration = 14400)]
    public async Task<IActionResult> RssFeed(CancellationToken token = default)
    {
        var feed = await SetupSyndicationFeed(token);
        await using var sw = new StringWriterWithEncoding(Encoding.UTF8);
        await using (var writer = XmlWriter.Create(sw, new XmlWriterSettings { Indent = true, Async = true }))
            feed.GetRss20Formatter().WriteTo(writer);

        return Content(sw.ToString(), "application/rss+xml", Encoding.UTF8);
    }

    #region Helpers

    internal async Task<SyndicationFeed> SetupSyndicationFeed(CancellationToken token = default)
    {
        var feedItems = await _journalService.GetPostsFeedItemsAsync(Url, Constants.SiteFeedBaseId, token: token);
        return new SyndicationFeed(Constants.SiteTitle, string.Empty, new Uri(Constants.SiteBaseUrl), feedItems)
            { Id = Constants.SiteFeedBaseId };
    }

    #endregion
}
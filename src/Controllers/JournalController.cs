using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
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
    private readonly IJournalReactionService _reactionService;

    public JournalController(IJournalService journalService, IJournalReactionService reactionService)
    {
        _journalService = journalService;
        _reactionService = reactionService;
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

    [ResponseCache(Duration = 0, NoStore = true, Location = ResponseCacheLocation.None)]
    public async Task<IActionResult> React([Required] string year, [Required] string slug, [Required] Reaction reaction,
        CancellationToken token = default)
    {
        if (!ModelState.IsValid)
            return View(new JournalReactViewModel { Success = false });

        var ip = Request.HttpContext.Connection.RemoteIpAddress?.ToString();
        if (Request.Headers.TryGetValue("CF-CONNECTING-IP", out var cloudflareIp))
            ip = cloudflareIp.First();

        return View(new JournalReactViewModel
        {
            Year = year,
            Slug = slug,
            Success = await _reactionService.PostReactionAsync(year, slug, ip ?? "unknown", reaction, token: token)
        });
    }

    [ResponseCache(Duration = 43200)]
    public async Task<IActionResult> Read(string year, string slug, CancellationToken token = default)
    {
        var post = await _journalService.GetPostBySlugAsync(year, slug, false, token);
        if (post == null) return NotFound();

        var reactions = await _reactionService.GetReactionsAsync(post.PartitionKey, post.RowKey, token);

        return View(new JournalPostReadViewModel
        {
            Post = post,
            Reactions = new JournalPostReactionsViewModel
            {
                PostYear = post.PartitionKey,
                PostSlug = post.RowKey,
                Reactions = reactions
            }
        });
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
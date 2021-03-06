using System;
using System.ServiceModel.Syndication;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Aoraki.Web.Contracts;
using Aoraki.Web.Data;
using Aoraki.Web.Extensions;
using Aoraki.Web.Models;
using Aoraki.Web.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace Aoraki.Web.Controllers
{
    public class JournalController : Controller
    {
        private const int EntriesPerPage = 10;

        private readonly ICanonicalService _canonical;
        private readonly IJournalPostService _postService;

        public JournalController(ICanonicalService canonical, IJournalPostService postService)
        {
            _canonical = canonical;
            _postService = postService;
        }

        [ResponseCache(Duration = 14400, VaryByQueryKeys = new[] { "page" })]
        public async Task<IActionResult> Index(int page = 1)
        {
            var totalPosts = await _postService.GetTotalPostCountAsync();
            var totalPages = (int)Math.Ceiling((decimal)totalPosts / (decimal)EntriesPerPage);

            if (page < 1)
                return RedirectToAction(nameof(Index), new { page = 1 });
            if (page > totalPages)
                return RedirectToAction(nameof(Index), new { page = totalPages });

            var posts = await _postService.GetPostsAsync((page - 1) * EntriesPerPage, EntriesPerPage);
            return View(new JournalIndexViewModel
            {
                Pagination = new Pagination { CurrentPage = page, TotalPages = totalPages },
                Posts = posts
            });
        }

        [ResponseCache(Duration = 14400)]
        public async Task<IActionResult> Archive()
        {
            return View(await _postService.GetPostsArchiveAsync());
        }

        [ResponseCache(Duration = 604800)]
        public async Task<IActionResult> Read(int year, string slug)
        {
            var post = await _postService.GetPostBySlugAsync(slug);
            if (post == null) return NotFound();
            if (post.Published?.Year != year) return new NotFoundResult();
            return View(post);
        }

        [HttpGet("journal/{year}/{slug}.txt")]
        [Produces("text/plain")]
        [ResponseCache(Duration = 604800)]
        public async Task<IActionResult> ReadPlaintext(string slug)
        {
            var post = await _postService.GetPostBySlugAsync(slug);
            if (post == null) return NotFound();
            return Ok(post.ToPlainText());
        }

        [HttpGet("journal/{year}/{slug}.md")]
        [Produces("text/plain")]
        [ResponseCache(Duration = 604800)]
        public async Task<IActionResult> ReadMarkdown(string slug)
        {
            var post = await _postService.GetPostBySlugAsync(slug);
            if (post == null) return NotFound();
            return Ok(post.Content);
        }

        [HttpGet("journal/{year}/{slug}.json")]
        [Produces("application/json")]
        [ResponseCache(Duration = 604800)]
        public async Task<IActionResult> ReadJson(string slug)
        {
            var post = await _postService.GetPostBySlugAsync(slug);
            if (post == null) return NotFound();
            return Ok(post);
        }

        [HttpGet("/atom.xml")]
        [ResponseCache(Duration = 14400)]
        public async Task<IActionResult> AtomFeed()
        {
            var feed = await SetupSyndicationFeed();
            await using var sw = new StringWriterWithEncoding(Encoding.UTF8);
            await using (var writer = XmlWriter.Create(sw, new XmlWriterSettings { Indent = true, Async = true}))
                feed.GetAtom10Formatter().WriteTo(writer);

            return Content(sw.ToString(), "application/atom+xml", Encoding.UTF8);
        }

        [HttpGet("/rss.xml")]
        [ResponseCache(Duration = 14400)]
        public async Task<IActionResult> RssFeed()
        {
            var feed = await SetupSyndicationFeed();
            await using var sw = new StringWriterWithEncoding(Encoding.UTF8);
            await using (var writer = XmlWriter.Create(sw, new XmlWriterSettings { Indent = true, Async = true}))
                feed.GetRss20Formatter().WriteTo(writer);

            return Content(sw.ToString(), "application/rss+xml", Encoding.UTF8);
        }

        #region Helpers

        private async Task<SyndicationFeed> SetupSyndicationFeed()
        {
            // This ID must not change
            const string baseId = "uuid:b8787de3-c2eb-41bc-89ab-9c176300d44c";
            var feedItems = await _postService.GetPostsFeedItemsAsync(Url, baseId);
            var siteUri = new Uri(_canonical.CanonicaliseUrl(Url.Action("Index")));
            return new SyndicationFeed("Matts Blog", string.Empty, siteUri, feedItems)
            {
                Id = baseId
            };
        }

        #endregion
    }
}
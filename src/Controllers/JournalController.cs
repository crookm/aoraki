using System;
using System.Threading.Tasks;
using Aoraki.Web.Contracts;
using Aoraki.Web.Extensions;
using Aoraki.Web.Models;
using Aoraki.Web.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace Aoraki.Web.Controllers
{
    public class JournalController : Controller
    {
        private const int PostsPerPage = 10;

        private readonly IJournalPostService _postService;

        public JournalController(IJournalPostService postService)
        {
            _postService = postService;
        }

        [ResponseCache(Duration = 14400, VaryByQueryKeys = new[] { "page" })]
        public async Task<IActionResult> Index(int page = 1)
        {
            var totalPosts = await _postService.GetTotalPostCountAsync();
            var totalPages = (int)Math.Ceiling((decimal)totalPosts / (decimal)PostsPerPage);

            if (page < 1)
                return RedirectToAction(nameof(Index), new { page = 1 });
            if (page > totalPages)
                return RedirectToAction(nameof(Index), new { page = totalPages });

            var posts = await _postService.GetPostsAsync((page - 1) * PostsPerPage, PostsPerPage);
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

        [Produces("text/plain")]
        [ResponseCache(Duration = 604800)]
        [Route("journal/{year}/{slug}.txt")]
        public async Task<IActionResult> ReadPlaintext(string slug)
        {
            var post = await _postService.GetPostBySlugAsync(slug);
            if (post == null) return NotFound();
            return Ok(post.ToPlainText());
        }

        [Produces("text/plain")]
        [ResponseCache(Duration = 604800)]
        [Route("journal/{year}/{slug}.md")]
        public async Task<IActionResult> ReadMarkdown(string slug)
        {
            var post = await _postService.GetPostBySlugAsync(slug);
            if (post == null) return NotFound();
            return Ok(post.Content);
        }

        [Produces("application/json")]
        [ResponseCache(Duration = 604800)]
        [Route("journal/{year}/{slug}.json")]
        public async Task<IActionResult> ReadJson(string slug)
        {
            var post = await _postService.GetPostBySlugAsync(slug);
            if (post == null) return NotFound();
            return Ok(post);
        }
    }
}
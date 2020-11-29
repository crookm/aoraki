using System;
using System.Linq;
using System.Threading.Tasks;
using Aoraki.Web.Contracts;
using Aoraki.Web.Models;
using Aoraki.Web.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Aoraki.Web.Controllers
{
    public class AdminController : Controller
    {
        private readonly ILogger<AdminController> _logger;
        private readonly IJournalPostService _postService;

        public AdminController(ILogger<AdminController> logger, IJournalPostService postService)
        {
            _logger = logger;
            _postService = postService;
        }

        public async Task<IActionResult> Index()
        {
            if (!User.Identity.IsAuthenticated) return RedirectToAction("Login", "Account");
            var posts = await _postService.GetPostsAsync(0, 99, allowUnpublished: true);
            return View(posts);
        }

        [HttpPost]
        public async Task<IActionResult> CreatePost(string title, string slug)
        {
            if (!User.Identity.IsAuthenticated) return RedirectToAction("Login", "Account");
            var postId = await _postService.CreatePostAsync(new JournalPost
            {
                Title = title,
                Slug = slug,
                Tags = Array.Empty<string>(),
                Created = DateTime.UtcNow,
                Published = null,
                Lead = "",
                Content = ""
            });

            _logger.LogInformation("Person created post with id {0}", postId);
            return RedirectToAction(nameof(EditPost), new {id = postId});
        }

        [HttpGet]
        public async Task<IActionResult> EditPost(string id)
        {
            if (!User.Identity.IsAuthenticated) return RedirectToAction("Login", "Account");
            if (string.IsNullOrEmpty(id)) return NotFound();
            var post = await _postService.GetPostByIdAsync(id, allowUnpublished: true);
            if (post == null) return NotFound();

            return View(new AdminEditPostViewModel
            {
                IsPublished = post.Published.HasValue,
                Tags = post.Tags == null ? string.Empty : string.Join(", ", post.Tags),
                Post = post
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPost(
            string id,
            AdminEditPostViewModel model)
        {
            if (!User.Identity.IsAuthenticated) return RedirectToAction("Login", "Account");
            if (string.IsNullOrEmpty(id)) return NotFound();
            if (ModelState.IsValid)
            {
                var oldPost = await _postService.GetPostByIdAsync(id, allowUnpublished: true);
                if (oldPost == null) return NotFound();
                
                // Tags
                model.Post.Tags = model?.Tags
                    ?.Split(',')
                    ?.Select(tag => tag.Trim())
                    ?.Where(tag => !string.IsNullOrEmpty(tag))
                    ?.ToArray();

                // Publishing
                model.Post.Published = oldPost.Published;
                if (model.IsPublished)
                {
                    if (!oldPost.Published.HasValue)
                        model.Post.Published = DateTime.UtcNow;
                }

                await _postService.UpdatePostAsync(id, model.Post);
                _logger.LogInformation("Person updated post with id {0}", id);
            }

            return RedirectToAction(nameof(EditPost), new {id});
        }
    }
}
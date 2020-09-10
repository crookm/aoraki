using System;
using System.Threading.Tasks;
using Aoraki.Web.Contracts;
using Aoraki.Web.Models;
using Aoraki.Web.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using TimeZoneConverter;

namespace Aoraki.Web.Controllers
{
    public class AdminController : Controller
    {
        private readonly IJournalPostService _postService;

        public AdminController(IJournalPostService postService)
        {
            _postService = postService;
        }

        public async Task<IActionResult> Index()
        {
            var posts = await _postService.GetPostsAsync(0, 99, allowUnpublished: true);
            return View(posts);
        }

        [HttpPost]
        public async Task<IActionResult> CreatePost(string title, string slug)
        {
            var postId = await _postService.CreatePostAsync(new JournalPost
            {
                Title = title,
                Slug = slug,
                Tags = new string[] { },
                Created = DateTime.UtcNow,
                Published = null,
                Content = ""
            });

            return RedirectToAction(nameof(EditPost), new {id = postId});
        }

        [HttpGet]
        public async Task<IActionResult> EditPost(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();
            var post = await _postService.GetPostByIdAsync(id, allowUnpublished: true);
            if (post == null) return NotFound();

            return View(new AdminEditPostViewModel
            {
                IsPublished = post.Published.HasValue,
                Post = post
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPost(
            string id,
            AdminEditPostViewModel model)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();
            if (ModelState.IsValid)
            {
                var oldPost = await _postService.GetPostByIdAsync(id, allowUnpublished: true);
                if (oldPost == null) return NotFound();

                model.Post.Published = oldPost.Published;
                if (model.IsPublished)
                {
                    if (!oldPost.Published.HasValue)
                        model.Post.Published = DateTime.UtcNow;
                }

                await _postService.UpdatePostAsync(id, model.Post);
            }

            return RedirectToAction(nameof(EditPost), new {id});
        }
    }
}
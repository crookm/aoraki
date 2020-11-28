using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Aoraki.Web.Contracts;
using Microsoft.AspNetCore.Mvc;
using SimpleMvcSitemap;
using SimpleMvcSitemap.Routing;

namespace Aoraki.Web.Controllers
{
    public class SitemapController : Controller
    {
        private readonly IJournalPostService _postService;

        public SitemapController(IJournalPostService postService)
        {
            _postService = postService;
        }

        [HttpGet("/sitemap.xml")]
        public IActionResult Index()
        {
            return new SitemapProvider(new BaseUrlProvider())
                .CreateSitemapIndex(new SitemapIndexModel(new List<SitemapIndexNode>
                {
                    new SitemapIndexNode(Url.Action("Pages", "Sitemap")),
                }));
        }

        [HttpGet("/sitemap-pages.xml")]
        public IActionResult Pages()
        {
            return new SitemapProvider(new BaseUrlProvider())
                .CreateSitemap(new SitemapModel(new List<SitemapNode>
                {
                    new SitemapNode(Url.Action("Index", "Journal")) { ChangeFrequency = ChangeFrequency.Daily },
                    new SitemapNode(Url.Action("Archive", "Journal")) { ChangeFrequency = ChangeFrequency.Daily },
                }));
        }

        [HttpGet("/sitemap-posts.xml")]
        public async Task<IActionResult> Posts()
        {
            return new SitemapProvider(new BaseUrlProvider())
                .CreateSitemap(new SitemapModel(
                    (await _postService.GetPostsArchiveAsync())
                        .SelectMany(posts => posts.Value)
                        .Select(post =>
                            new SitemapNode(
                                Url.Action("Read", "Journal", new { post.Published.Year, post.Slug }))
                            {
                                LastModificationDate = post.Published
                            })
                        .ToList()));
        }
    }

    internal class BaseUrlProvider : IBaseUrlProvider
    {
        public Uri BaseUrl => new Uri("https://crookm.com");
    }
}
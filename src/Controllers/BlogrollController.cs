using System;
using System.Linq;
using System.Threading.Tasks;
using Aoraki.Web.Data.Context;
using Aoraki.Web.Models;
using Aoraki.Web.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Aoraki.Web.Controllers
{
    public class BlogrollController : Controller
    {
        private const int EntriesPerPage = 5;

        private readonly ILogger<BlogrollController> _logger;
        private readonly AorakiDbContext _db;

        public BlogrollController(ILogger<BlogrollController> logger, AorakiDbContext db)
        {
            _logger = logger;
            _db = db;
        }

        [ResponseCache(Duration = 86400, VaryByQueryKeys = new[] { "page" })]
        public async Task<IActionResult> Index(int page = 1)
        {
            var totalEntries = await _db.BlogrollBlogs.CountAsync();
            var totalPages = (int)Math.Ceiling((decimal)totalEntries / (decimal)EntriesPerPage);

            if (page < 1)
                return RedirectToAction(nameof(Index), new { page = 1 });
            if (page > totalPages)
                return RedirectToAction(nameof(Index), new { page = totalPages });
            
            var blogs = await _db.BlogrollBlogs
                .OrderByDescending(blog => blog.Created)
                .Skip((page - 1) * EntriesPerPage).Take(EntriesPerPage)
                .ToListAsync();
            
            return View(new BlogrollIndexViewModel
            {
                Pagination = new Pagination { CurrentPage = page, TotalPages = totalPages },
                Blogs = blogs
            });
        }
    }
}
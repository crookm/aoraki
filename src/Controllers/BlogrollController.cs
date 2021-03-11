using System.Linq;
using System.Threading.Tasks;
using Aoraki.Web.Data.Context;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Aoraki.Web.Controllers
{
    public class BlogrollController : Controller
    {
        private readonly ILogger<BlogrollController> _logger;
        private readonly AorakiDbContext _db;

        public BlogrollController(ILogger<BlogrollController> logger, AorakiDbContext db)
        {
            _logger = logger;
            _db = db;
        }

        [ResponseCache(Duration = 86400)]
        public async Task<IActionResult> Index()
        {
            var blogs = await _db.BlogrollBlogs
                .OrderByDescending(blog => blog.Created)
                .ToListAsync();
            return View(blogs);
        }
    }
}
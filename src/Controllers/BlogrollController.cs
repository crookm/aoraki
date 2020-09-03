using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Aoraki.Web.Models;
using Aoraki.Web.Models.Entities;
using Aoraki.Web.Models.ViewModels;
using Aoraki.Web.Options;
using Azure.Data.Tables;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Aoraki.Web.Controllers
{
    public class BlogrollController : Controller
    {
        private const int EntriesPerPage = 5;

        private readonly ILogger<BlogrollController> _logger;
        private readonly TableClient _tableClient;

        public BlogrollController(ILogger<BlogrollController> logger, IOptions<StorageOptions> storageOptions)
        {
            _logger = logger;
            _tableClient = new TableClient(storageOptions.Value.AzureStorageConnectionString, "blogroll");
        }

        [ResponseCache(Duration = 86400, VaryByQueryKeys = new[] { "page" })]
        public async Task<IActionResult> Index(int page = 1, CancellationToken token = default)
        {
            var query = _tableClient.QueryAsync<BlogrollBlog>(cancellationToken: token);
            var blogs = await query.ToListAsync(token);
            var totalPages = (int)Math.Ceiling((decimal)blogs.Count / EntriesPerPage);

            if (page < 1)
                return RedirectToAction(nameof(Index), new { page = 1 });
            if (page > totalPages)
                return RedirectToAction(nameof(Index), new { page = totalPages });

            return View(new BlogrollIndexViewModel
            {
                Pagination = new Pagination { CurrentPage = page, TotalPages = totalPages },
                Blogs = blogs
                    .OrderByDescending(blog => blog.Timestamp)
                    .Skip((page - 1) * EntriesPerPage).Take(EntriesPerPage)
            });
        }
    }
}
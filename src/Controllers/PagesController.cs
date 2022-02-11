using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Mvc;

namespace Aoraki.Web.Controllers
{
    [ExcludeFromCodeCoverage] // Remove exclusion if endpoints with something worth testing are added
    public class PagesController : Controller
    {
        [HttpGet("/colophon")]
        [ResponseCache(Duration = 2629800)]
        public IActionResult Colophon() => View();
    }
}
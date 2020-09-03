using Microsoft.AspNetCore.Mvc;

namespace Aoraki.Web.Controllers
{
    public class PagesController : Controller
    {
        [HttpGet("/colophon")]
        [ResponseCache(Duration = 2629800)]
        public IActionResult Colophon() => View();
    }
}
using Microsoft.AspNetCore.Mvc;

namespace Aoraki.Web.Controllers
{
    public class PagesController : Controller
    {
        [HttpGet("/colophon")]
        public IActionResult Colophon() => View();
    }
}
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Aoraki.Web.Models.ViewModels;

namespace Aoraki.Web.Controllers
{
    public class HomeController : Controller
    {
        [HttpGet("/humans.txt")]
        public IActionResult HumansTxt()
        {
            return RedirectPermanent("/.well-known/humans.txt");
        }

        [HttpGet("/security.txt")]
        public IActionResult SecurityTxt()
        {
            return RedirectPermanent("/.well-known/security.txt");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel {RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier});
        }

        [HttpGet("/Home/Error/404")]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error404()
        {
            return View("~/Views/Home/Error/Error404.cshtml");
        }
    }
}
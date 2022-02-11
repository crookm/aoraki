using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Aoraki.Web.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace Aoraki.Web.Controllers;

[ExcludeFromCodeCoverage] // Remove exclusion if endpoints with something worth testing are added
public class HomeController : Controller
{
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
        => View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });

    [HttpGet("/Home/Error/404")]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error404() => View("~/Views/Home/Error/Error404.cshtml");
}
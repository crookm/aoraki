using Microsoft.AspNetCore.Mvc;

namespace Aoraki.Web.Controllers;

// Disable hardcoded URI warning
#pragma warning disable S1075

public class RedirectController : Controller
{
    // Still active, but different url
    // ---
    [HttpGet("/2018/05/sharing-wifi-connection-over-ethernet.html")]
    public IActionResult OldSharingWifiConnectionUrl() => RedirectToActionPermanent("Read", "Journal", new { Year = 2018, Slug = "sharing-wifi-connection-over-ethernet" });

    // Retired, archived, forgotten
    // ---
    [HttpGet("/journal/2018/running-skyrim-se-on-linux-mint-19")]
    public IActionResult OldRunningSkyrimOnLinuxUrl() => RedirectPermanent("https://web.archive.org/web/20200921062453/https://crookm.com/journal/2018/running-skyrim-se-on-linux-mint-19/");
    [HttpGet("/journal/2018/one-page-app-routing-on-netlify")]
    [HttpGet("/2018/02/one-page-app-routing-on-netlify.html")]
    public IActionResult OldOnePageAppRoutingNetlifyUrl() => RedirectPermanent("https://web.archive.org/web/20200921064416/https://crookm.com/journal/2018/one-page-app-routing-on-netlify/");
}
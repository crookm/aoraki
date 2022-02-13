using Aoraki.Web.Controllers;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace Aoraki.Web.Tests.Controllers;

public class RedirectControllerTests
{
    [Fact]
    public void OldSharingWifiConnectionUrl_ShouldDirectToCorrectUrl()
    {
        var controller = new RedirectController();
        var result = controller.OldSharingWifiConnectionUrl();
        var redirectToActionResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirectToActionResult.ControllerName.Should().Be("Journal");
        redirectToActionResult.ActionName.Should().Be(nameof(JournalController.Read));
        redirectToActionResult.RouteValues.Should()
            .Contain("Year", 2018)
            .And.Contain("Slug", "sharing-wifi-connection-over-ethernet");
    }

    [Fact]
    public void OldRunningSkyrimOnLinuxUrl_ShouldDirectToCorrectUrl()
    {
        var controller = new RedirectController();
        var result = controller.OldRunningSkyrimOnLinuxUrl();
        var redirectResult = result.Should().BeOfType<RedirectResult>().Subject;
        redirectResult.Url.Should()
            .Be("https://web.archive.org/web/20200921062453/https://crookm.com/journal/2018/running-skyrim-se-on-linux-mint-19/");
    }
    
    [Fact]
    public void OldOnePageAppRoutingNetlifyUrl_ShouldDirectToCorrectUrl()
    {
        var controller = new RedirectController();
        var result = controller.OldOnePageAppRoutingNetlifyUrl();
        var redirectResult = result.Should().BeOfType<RedirectResult>().Subject;
        redirectResult.Url.Should()
            .Be("https://web.archive.org/web/20200921064416/https://crookm.com/journal/2018/one-page-app-routing-on-netlify/");
    }
}
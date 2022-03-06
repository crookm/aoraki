using System.Threading.Tasks;
using Aoraki.Web.Middleware;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Moq;
using Xunit;

namespace Aoraki.Web.Tests.Middleware;

public class SecurityHeaderMiddlewareTests
{
    [Fact]
    public async Task InvokeAsync_ShouldAddHstsHeader_OnlyInProduction()
    {
        Task Next(HttpContext _) => Task.CompletedTask;
        var middleware = new SecurityHeaderMiddleware(Next);

        var productionContext = new DefaultHttpContext();
        var productionEnv = new Mock<IWebHostEnvironment>();
        productionEnv.SetupGet(x => x.EnvironmentName).Returns("production");
        await middleware.InvokeAsync(productionContext, productionEnv.Object);

        var devContext = new DefaultHttpContext();
        var devEnv = new Mock<IWebHostEnvironment>();
        devEnv.SetupGet(x => x.EnvironmentName).Returns("development");
        await middleware.InvokeAsync(devContext, devEnv.Object);

        productionContext.Response.Headers.Should().ContainKey("strict-transport-security");
        devContext.Response.Headers.Should().NotContainKey("strict-transport-security");
    }
}
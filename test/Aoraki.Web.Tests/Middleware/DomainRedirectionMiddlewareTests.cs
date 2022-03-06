using System.Linq;
using System.Threading.Tasks;
using Aoraki.Web.Middleware;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace Aoraki.Web.Tests.Middleware;

public class DomainRedirectionMiddlewareTests
{
    [Fact]
    public async Task InvokeAsync_ShouldRedirect_WhenUsingOldHost()
    {
        Task Next(HttpContext _) => Task.CompletedTask;
        var middleware = new DomainRedirectionMiddleware(Next);

        var context = new DefaultHttpContext
            { Request = { Scheme = "https", Host = new HostString("www.crookm.com"), Path = new PathString("/aaa") } };
        await middleware.InvokeAsync(context);

        context.Response.StatusCode.Should().Be(StatusCodes.Status301MovedPermanently);
        context.Response.Headers.Location.Should().HaveCount(1)
            .And.Subject.First().Should().Be($"https://{Constants.SiteHostName}:443/aaa");
    }

    [Fact]
    public async Task InvokeAsync_ShouldNotRedirect_WhenUsingNewHost()
    {
        Task Next(HttpContext _) => Task.CompletedTask;
        var middleware = new DomainRedirectionMiddleware(Next);

        var context = new DefaultHttpContext
            { Request = { Scheme = "https", Host = new HostString("mattcrook.io"), Path = new PathString("/aaa") } };
        await middleware.InvokeAsync(context);

        context.Response.StatusCode.Should().Be(StatusCodes.Status200OK);
    }
}
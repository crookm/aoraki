using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;

namespace Aoraki.Web.Middleware;

/// <summary>
/// Injects sensible default security headers into the response of the request
/// </summary>
/// <remarks>
/// Things like HSTS are defined here instead of using the correct middleware, because Azure App Service manages SSL
/// termination itself on a layer above our application, sending requests to the running app using plain HTTP. The HSTS
/// middleware will not inject the header into plain HTTP requests.
/// </remarks>
public class SecurityHeaderMiddleware
{
    private readonly RequestDelegate _next;

    public SecurityHeaderMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, IWebHostEnvironment env)
    {
        context.Response.Headers.Add("x-frame-options", "SAMEORIGIN");
        context.Response.Headers.Add("x-content-type-options", "nosniff");
        context.Response.Headers.Add("referrer-policy", "strict-origin-when-cross-origin");
        context.Response.Headers.Add("permissions-policy",
            "accelerometer=(), camera=(), geolocation=(), gyroscope=(), magnetometer=(), microphone=(), payment=(), usb=()");

        if (env.IsProduction())
            context.Response.Headers.Add("strict-transport-security",
                "max-age=31536000; includeSubDomains; preload");

        await _next.Invoke(context);
    }
}
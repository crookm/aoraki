using System;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights.AspNetCore.Extensions;
using Microsoft.AspNetCore.Http;

namespace Aoraki.Web.Middleware;

/// <summary>
/// Redirects the old domain to the new, keeping the request URI the same other than the hostname.
/// </summary>
public class DomainRedirectionMiddleware
{
    private readonly RequestDelegate _next;

    public DomainRedirectionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var requestUri = context.Request.GetUri();
        if (requestUri.Host.EndsWith("crookm.com"))
        {
            var newUri = new UriBuilder(requestUri) { Host = Constants.SiteHostName };
            context.Response.Redirect(newUri.ToString(), permanent: true);
            return;
        }

        await _next.Invoke(context);
    }
}
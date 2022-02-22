using System.Diagnostics.CodeAnalysis;
using Aoraki.Projects.FridgeMagnet.Integration;
using Aoraki.Web.Contracts;
using Aoraki.Web.Options;
using Aoraki.Web.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Aoraki.Web;

[ExcludeFromCodeCoverage]
public class Startup
{
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    private IConfiguration Configuration { get; }

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddApplicationInsightsTelemetry();
        services
            .AddSingleton<ICanonicalService>(new CanonicalService
            {
                HostName = Constants.SiteHostName,
                EnableTrailingSlash = false,
                EnableLowerCase = true,
                EnableHttps = true,
            });

        services
            .Configure<StorageOptions>(Configuration.GetSection(StorageOptions.HierarchyName));

        services
            .AddSingleton<IJournalService, JournalService>()
            .AddSingleton<IJournalReactionService, JournalReactionService>()
            .AddSingleton<IBlogrollService, BlogrollService>()
            .AddSingleton<IStorageFactory, StorageFactory>();

        services.SetupFridgeMagnetProject();

        services.AddAntiforgery();
        services.AddResponseCaching();
        services.AddControllersWithViews();
        services.AddRouting(options =>
        {
            options.LowercaseUrls = true;
            options.LowercaseQueryStrings = true;
            options.AppendTrailingSlash = false;
        });
    }

    public static void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        // Default headers
        // - Things like HSTS are defined here instead of using the correct middleware because the prod execution
        //      environment requires HTTP, which ASP.NET will not inject the HSTS header into.
        app.Use(async (ctx, next) =>
        {
            ctx.Response.Headers.Add("x-frame-options", "SAMEORIGIN");
            ctx.Response.Headers.Add("x-content-type-options", "nosniff");
            ctx.Response.Headers.Add("referrer-policy", "strict-origin-when-cross-origin");
            ctx.Response.Headers.Add("permissions-policy",
                "accelerometer=(), camera=(), geolocation=(), gyroscope=(), magnetometer=(), microphone=(), payment=(), usb=()");

            if (env.IsProduction())
                ctx.Response.Headers.Add("strict-transport-security",
                    "max-age=31536000; includeSubDomains; preload");

            await next.Invoke();
        });

        if (env.IsDevelopment())
            app.UseDeveloperExceptionPage();
        else
            app.UseExceptionHandler("/Home/Error");

        app.UseBlazorFrameworkFiles();
        app.UseStatusCodePagesWithReExecute("/Home/Error/{0}");
        app.UseStaticFiles(new StaticFileOptions
        {
            OnPrepareResponse = ctx =>
            {
                ctx.Context.Response.Headers.Add(
                    "Cache-Control", $"public, max-age=604800");
            }
        });

        app.UseRouting();
        app.UseResponseCaching();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllerRoute(
                name: "journal",
                pattern: "journal/{year}/{slug}",
                defaults: new { controller = "Journal", action = "Read" });
            endpoints.MapControllerRoute(
                name: "default",
                pattern: "{controller=Journal}/{action=Index}/{id?}");
        });
    }
}
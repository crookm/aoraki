using System.Diagnostics.CodeAnalysis;
using Aoraki.Projects.FridgeMagnet.Integration;
using Aoraki.Web.Contracts;
using Aoraki.Web.Middleware;
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
        app
            .UseMiddleware<SecurityHeaderMiddleware>()
            .UseMiddleware<DomainRedirectionMiddleware>();

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
                pattern: "{controller=Home}/{action=Index}/{id?}");
        });
    }
}
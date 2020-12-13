using System;
using Aoraki.Web.Contracts;
using Aoraki.Web.Data.Context;
using Aoraki.Web.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Aoraki.Web
{
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
            services.AddDbContext<AorakiDbContext>();
            services.AddDefaultIdentity<IdentityUser>()
                .AddEntityFrameworkStores<AorakiDbContext>();
            services.ConfigureApplicationCookie(options =>
            {
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                options.Cookie.SameSite = SameSiteMode.Strict;
                options.Cookie.HttpOnly = true;
                options.ExpireTimeSpan = TimeSpan.FromHours(6);
                options.LoginPath = "/account/login";
                options.ReturnUrlParameter = CookieAuthenticationDefaults.ReturnUrlParameter;
                options.SlidingExpiration = true;
            });

            services
                .AddSingleton<ICanonicalService>(new CanonicalService
                {
                    HostName = "crookm.com",
                    EnableTrailingSlash = false,
                    EnableLowerCase = true,
                    EnableHttps = true,
                });

            services.AddScoped<IJournalPostService, JournalPostService>();

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
            if (env.IsDevelopment())
                app.UseDeveloperExceptionPage();
            else
                app.UseExceptionHandler("/Home/Error");

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
            app.UseAuthorization();
            app.UseAuthentication();
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
}
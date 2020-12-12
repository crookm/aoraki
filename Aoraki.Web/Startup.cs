using Aoraki.Web.Contracts;
using Aoraki.Web.Data.Context;
using Aoraki.Web.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
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

            services
                .AddSingleton<ICanonicalService>(new CanonicalService
                {
                    HostName = "crookm.com",
                    EnableTrailingSlash = false,
                    EnableLowerCase = true,
                    EnableHttps = true,
                });

            services.AddScoped<IJournalPostService, JournalPostService>();

            // services.AddIdentityMongoDbProvider<MongoUser>(options =>
            // {
            //     options.ConnectionString = Configuration.GetSection(nameof(JournalSettings))["DbConnection"];
            //     options.MigrationCollection = "_identity-migrations";
            //     options.UsersCollection = "identity-users";
            //     options.RolesCollection = "identity-roles";
            // });

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
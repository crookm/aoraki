using Aoraki.Web.Contracts;
using Aoraki.Web.Models;
using Aoraki.Web.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

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
            services.Configure<JournalSettings>(Configuration.GetSection(nameof(JournalSettings)));
            services.AddSingleton<IJournalSettings>(provider =>
                provider.GetRequiredService<IOptions<JournalSettings>>().Value);

            services.AddSingleton<IJournalPostService, JournalPostService>();

            services.AddAntiforgery();
            services.AddResponseCaching();
            services.AddControllersWithViews();
            services.AddRouting(options =>
            {
                options.LowercaseUrls = true;
                options.LowercaseQueryStrings = true;
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
                app.UseDeveloperExceptionPage();
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();
            app.UseAuthorization();
            app.UseResponseCaching();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "journal",
                    pattern: "journal/{year}/{slug}",
                    defaults: new {controller = "Journal", action = "Read"});
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Journal}/{action=Index}/{id?}");
            });
        }
    }
}
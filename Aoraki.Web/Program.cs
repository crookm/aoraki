using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace Aoraki.Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder => {
                    webBuilder.UseSentry("https://f62151a26a34404aafb1668adef94ebe@o483303.ingest.sentry.io/5534756");
                    webBuilder.UseStartup<Startup>();
                });
    }
}
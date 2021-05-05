using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using TheArchives.Server.Extensions;

namespace TheArchives.Server
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.AddLogging();
                    webBuilder.UseStartup<Startup>();
                });
    }
}

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.IO.Compression;
using TheArchives.Server.Extensions;

namespace TheArchives.Server
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IWebHostEnvironment environment)
        {
            Configuration = configuration;
            Environment = environment;
        }

        public IConfiguration Configuration { get; }

        public IWebHostEnvironment Environment { get; }


        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            // Add authentication
            ConfigureAuthentication(services);

            // Enable Entity Framework
            services.AddDbContext<Data.ApplicationDbContext>(options =>
                options.UseSqlite(Configuration.GetConnectionString("DefaultConnection")));
            services.AddDatabaseDeveloperPageExceptionFilter();

            // Enable default memory cache
            services.AddMemoryCache();

            // Enable Options
            services.AddOptions();

            // Enable compression
            services.AddResponseCompression(o =>
            {
                o.EnableForHttps = true;
            });

            // Enable AutoMapper
            services.AddAutoMapper(typeof(Startup));

            // Add Elastic Search
            services.AddElasticsearch(Configuration, Environment);

            // Add Hosted Service(s)
            services.AddHostedService<HostedServices.ElasticIndexerHostedService>();

            // Add .NET
            services.AddControllersWithViews();
            services.AddRazorPages();

            // Configure Dependency Injection
            ConfigureDependencyInjection(services);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment()) {
                app.UseDeveloperExceptionPage();
                app.UseMigrationsEndPoint();
                app.UseWebAssemblyDebugging();
            }
            else {
                app.UseForwardedHeaders();
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseResponseCompression();

            app.UseHttpsRedirection();
            app.UseBlazorFrameworkFiles();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
                endpoints.MapControllers();
                endpoints.MapFallbackToFile("index.html");
            });
        }

        protected virtual void ConfigureDependencyInjection(IServiceCollection services)
        {
            // Configuration for Forwarded Headers.
            // Handle headers set by our load balancer.
            // This allows authentication to work even though the load balancer removes the https scheme from the request
            services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders = Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedFor | Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedProto;

                // Only loopback proxies are allowed by default.
                // Clear that restriction because forwarders are enabled by explicit configuration.
                options.KnownNetworks.Clear();
                options.KnownProxies.Clear();
            });

            // Configure compression
            services.Configure<BrotliCompressionProviderOptions>(options =>
            {
                options.Level = CompressionLevel.Fastest;
            });

            // Options
            services.Configure<Models.ElasticOptions>(Configuration.GetSection("Elastic"));
            services.Configure<Models.ContentOptions>(Configuration.GetSection("Content"));

            // Repositories
            services.AddScoped<Repositories.IContentRepository, Repositories.ContentRepository>();
            services.AddScoped<Repositories.ISearchRepository, Repositories.SearchRepository>();
            services.AddScoped<Repositories.ITextRepository, Repositories.TextRepository>();

            // Other
            services.AddScoped<HostedServices.IElasticIndexer, HostedServices.ElasticIndexer>();
        }

        protected virtual void ConfigureAuthentication(IServiceCollection services)
        {
            // Add Authentication Services
            if (!Environment.IsEnvironment("Testing")) {
                services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                }).AddJwtBearer(options =>
                {
                    options.Authority = Configuration["Auth0:Authority"];
                    options.Audience = Configuration["Auth0:ApiIdentifier"];
                });
            }
        }
    }
}

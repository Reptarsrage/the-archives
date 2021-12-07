using System.IO.Compression;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;

using Serilog;
using Serilog.Exceptions;

using TheArchives.Server.Data;
using TheArchives.Server.Extensions;
using TheArchives.Server.HostedServices;
using TheArchives.Server.Models;
using TheArchives.Server.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add Logging
builder.WebHost.UseSerilog((hostingContext, loggingConfiguration) =>
{
    var logDirectory = hostingContext.Configuration["LogDirectory"];
    if (!string.IsNullOrEmpty(logDirectory))
    {
        var logPath = Path.Combine(logDirectory, "log.txt");
        loggingConfiguration.WriteTo.File(logPath, rollingInterval: RollingInterval.Day);
    }

    loggingConfiguration
            .ReadFrom.Configuration(hostingContext.Configuration)
            .Enrich.FromLogContext()
            .Enrich.WithExceptionDetails()
            .WriteTo.Console();
});

// Enable Entity Framework
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// Enable default memory cache
builder.Services.AddMemoryCache();

// Enable Options
builder.Services.AddOptions();

// Enable compression
builder.Services.AddResponseCompression(o =>
{
    o.EnableForHttps = true;
});

// Enable AutoMapper
builder.Services.AddAutoMapper(typeof(Program));

// Add Elastic Search
builder.Services.AddElasticsearch(builder.Configuration, builder.Environment);

// Add Hosted Service(s)
builder.Services.AddHostedService<ElasticIndexerHostedService>();

// Add Authentication
if (!builder.Environment.IsEnvironment("Testing"))
    builder.Services
        .AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.Authority = builder.Configuration["Auth0:Authority"];
            options.Audience = builder.Configuration["Auth0:ApiIdentifier"];
        });

// Add .NET Core
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

// DI: Response compression
builder.Services.Configure<BrotliCompressionProviderOptions>(options =>
{
    options.Level = CompressionLevel.Fastest;
});

// DI: Options
builder.Services.Configure<ElasticOptions>(builder.Configuration.GetSection("Elastic"));
builder.Services.Configure<ContentOptions>(builder.Configuration.GetSection("Content"));

// DI: Repositories
builder.Services.AddScoped<IContentRepository, ContentRepository>();
builder.Services.AddScoped<ISearchRepository, SearchRepository>();
builder.Services.AddScoped<ITextRepository, TextRepository>();

// DI: Other
builder.Services.AddScoped<IElasticIndexer, ElasticIndexer>();

// Configuration for Forwarded Headers.
// Handle headers set by our load balancer.
// This allows authentication to work even though the load balancer removes the https scheme from the request
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedFor | Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedProto;

    // Only loopback proxies are allowed by default.
    // Clear that restriction because forwarders are enabled by explicit configuration.
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseForwardedHeaders();
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseResponseCompression();

app.UseBlazorFrameworkFiles();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();
app.MapControllers();
app.MapFallbackToFile("index.html");

app.Run();
public partial class Program { } // so we can reference it from integration tests
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Blazor.Tools.Components;
using Blazor.Tools.Components.Account;
using Blazor.Tools.Data;

//using Blazor.Tools.BlazorBundler.SessionManagement;
//using Blazor.Tools.BlazorBundler.Interfaces;
//using Blazor.Tools.BlazorBundler.SessionManagement.Interfaces;

namespace Blazor.Tools;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        // Configure logging to add console output
        builder.Logging.ClearProviders(); // Clear default logging providers
        builder.Logging.AddConsole();     // Add console logging
        builder.Logging.AddDebug();     // Add console logging

        // Set minimum level to Debug to capture all messages from Debug and above
        builder.Logging.SetMinimumLevel(LogLevel.Debug);

        // Manually check for the environment variable
        var environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";

        // Load configuration
        builder.Configuration
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile($"appsettings.{environmentName}.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables();

        var _baseAPIUrl = builder.Configuration.GetSection("APIBaseURL").Value ?? string.Empty;
        var _baseAPIPort = int.Parse(builder.Configuration.GetSection("APIBasePort").Value ?? string.Empty);
        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

        builder.AddServiceDefaults();
        // Configure Blazorise with existing registrations
        //builder.Services.AddBlazorBootstrap();

        // Add services to the container.
        builder.Services.AddRazorComponents()
            .AddInteractiveServerComponents();

        builder.Services.AddCascadingAuthenticationState();
        builder.Services.AddScoped<IdentityUserAccessor>();
        builder.Services.AddScoped<IdentityRedirectManager>();
        builder.Services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();
        //builder.Services.AddScoped<SessionTable>();

        builder.Services.AddAuthentication(options =>
            {
                options.DefaultScheme = IdentityConstants.ApplicationScheme;
                options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
            })
            .AddIdentityCookies();

        //RegisterHttpClientService<ICommonService<SessionTable, ISessionTable, IReportItem>, SessionTableService>(builder, _baseAPIUrl);
        //RegisterHttpClientService<ISessionTableService, SessionTableService>(builder, _baseAPIUrl);

        builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(connectionString));
        builder.Services.AddDatabaseDeveloperPageExceptionFilter();

        builder.Services.AddIdentityCore<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true)
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddSignInManager()
            .AddDefaultTokenProviders();

        builder.Services.AddSingleton<IEmailSender<ApplicationUser>, IdentityNoOpEmailSender>();

        // Configure logging
        builder.Services.AddLogging(config =>
        {
            config.AddConsole();
            config.SetMinimumLevel(LogLevel.Information);
        });

        var app = builder.Build();

        // Use DI to get the logger
        var logger = app.Services.GetRequiredService<ILogger<Program>>();

        logger.LogInformation("Start processing Blazor.Tools");

        app.MapDefaultEndpoints();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseMigrationsEndPoint();
        }
        else
        {
            app.UseExceptionHandler("/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

        app.UseHttpsRedirection();

        app.UseStaticFiles();
        app.UseAntiforgery();

        app.MapRazorComponents<App>()
            .AddInteractiveServerRenderMode();

        // Add additional endpoints required by the Identity /Account Razor components.
        app.MapAdditionalIdentityEndpoints();

        app.Run();
    }

    private static void RegisterHttpClientService<TInterface, TImplementation>(WebApplicationBuilder builder, string baseAPIUrl)
            where TInterface : class
            where TImplementation : class, TInterface
    {
        builder.Services.AddHttpClient<TInterface, TImplementation>(client =>
        {
            client.BaseAddress = !string.IsNullOrEmpty(baseAPIUrl) ? new Uri(baseAPIUrl) : null;
            var environment = builder.Environment;
            client.Timeout = environment.IsDevelopment() ? TimeSpan.FromMinutes(30) : TimeSpan.FromSeconds(30);
        });
    }

    private static void RegisterHttpClientReportService<TInterface, TImplementation>(WebApplicationBuilder builder, string baseAPIUrl)
        where TInterface : class
        where TImplementation : class, TInterface
    {
        builder.Services.AddHttpClient<TInterface, TImplementation>(client =>
        {
            client.BaseAddress = !string.IsNullOrEmpty(baseAPIUrl) ? new Uri(baseAPIUrl) : null;
            var environment = builder.Environment;
            client.Timeout = environment.IsDevelopment() ? TimeSpan.FromMinutes(30) : TimeSpan.FromSeconds(30);
        });
    }
}

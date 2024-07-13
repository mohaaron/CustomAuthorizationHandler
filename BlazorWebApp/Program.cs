using BlazorWebApp;
using BlazorWebApp.Client.Pages;
using BlazorWebApp.Components;
using Microsoft.AspNetCore.Authentication;
using Serilog;
using Serilog.Events;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

Log.Information("Starting up");

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((ctx, lc) => lc
        .MinimumLevel.Debug()
        .MinimumLevel.Override("Duende", LogEventLevel.Verbose)
        .MinimumLevel.Override("System", LogEventLevel.Error)
        .MinimumLevel.Override("Microsoft", LogEventLevel.Error)
        .MinimumLevel.Override("System.Net.Http", LogEventLevel.Information)
        .MinimumLevel.Override("Microsoft.AspNetCore.Authentication", LogEventLevel.Information)
        .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level}] {SourceContext}{NewLine}{Message:lj}{NewLine}{Exception}{NewLine}")
        .Enrich.FromLogContext());

    // Add services to the container.
    builder.Services.AddRazorComponents()
        .AddInteractiveServerComponents()
        .AddInteractiveWebAssemblyComponents();

    var app = builder
            .ConfigureAuthServices();

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.UseWebAssemblyDebugging();
    }
    else
    {
        app.UseExceptionHandler("/Error", createScopeForErrors: true);
        // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
        app.UseHsts();
    }

    app.UseSerilogRequestLogging();

    app.UseHttpsRedirection();

    app.UseStaticFiles();
    app.UseAntiforgery();

    app.MapDefaultControllerRoute();

    app.MapRazorComponents<App>()
        .AddInteractiveServerRenderMode()
        .AddInteractiveWebAssemblyRenderMode()
        .AddAdditionalAssemblies(typeof(Counter).Assembly);

    app.MapGet("authentication/login", (string? returnUrl) => TypedResults.Challenge(GetAuthProperties(returnUrl)))
            .AllowAnonymous();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Unhandled exception");
}
finally
{
    Log.Information("Shut down complete");
    Log.CloseAndFlush();
}

static AuthenticationProperties GetAuthProperties(string? returnUrl)
{
    // TODO: Use HttpContext.Request.PathBase instead.
    const string pathBase = "/";

    // Prevent open redirects.
    if (string.IsNullOrEmpty(returnUrl))
    {
        returnUrl = pathBase;
    }
    else if (!Uri.IsWellFormedUriString(returnUrl, UriKind.Relative))
    {
        returnUrl = new Uri(returnUrl, UriKind.Absolute).PathAndQuery;
    }
    else if (returnUrl[0] != '/')
    {
        returnUrl = $"{pathBase}{returnUrl}";
    }

    return new AuthenticationProperties { RedirectUri = returnUrl };
}

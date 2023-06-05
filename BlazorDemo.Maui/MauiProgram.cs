using System;
using System.Net.Http;
using System.Reflection;
using System.Security.Claims;
using BlazorDemo.Masl;
using BlazorDemo.Models.DownstreamApi;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls.Hosting;
using Microsoft.Maui.Hosting;

namespace BlazorDemo.Maui;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts => { fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular"); });

        builder.Services.AddMauiBlazorWebView();

#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
        builder.Logging.AddDebug();
#endif
        builder.Services.AddScoped<CustomAuthorizationMessageHandler>();
        
        var executingAssembly = Assembly.GetExecutingAssembly();

        using var stream = executingAssembly.GetManifestResourceStream("BlazorDemo.Maui.appsettings.json");

        var configuration = new ConfigurationBuilder()
            .AddJsonStream(stream)
            .Build();
        
        builder.Services.AddSingleton<IExternalAuthService, ExternalAuthService>();
        builder.Configuration.AddConfiguration(configuration);
        
        builder.Services.AddAuthorizationCore();
        builder.Services.TryAddScoped<AuthenticationStateProvider, ExternalAuthStateProvider>();
        builder.Services.AddSingleton<ExternalAuthService>();
        
        builder.Services.AddScoped<IDownstreamTokenProvider, DownstreamTokenProvider>();

        return builder.Build();
    }
}

public class CustomAuthorizationMessageHandler : AuthorizationMessageHandler
{
    public CustomAuthorizationMessageHandler(
        IConfiguration configuration,
        IAccessTokenProvider provider,
        NavigationManager navigationManager)
        : base(provider, navigationManager)
    {
        var downstreamApi = configuration.GetRequiredSection("DownstreamApi")
            .Get<DownstreamApiConfiguration>()!;
        
        ConfigureHandler(
            authorizedUrls: new[] { downstreamApi.BaseUrl },
            scopes: downstreamApi.Scopes
        );
    }
}
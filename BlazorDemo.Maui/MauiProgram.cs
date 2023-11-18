using System.Reflection;
using BlazorDemo.Masl;
using BlazorDemo.Masl.MSALClient;
using BlazorDemo.Models;
using BlazorDemo.Shared;
using BlazorDemo.Shared.Downstream;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

namespace BlazorDemo.Maui;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts => { fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular"); });
        
        var executingAssembly = Assembly.GetExecutingAssembly();

        using var stream = executingAssembly.GetManifestResourceStream("BlazorDemo.Maui.appsettings.json");

        var configuration = new ConfigurationBuilder()
            .AddJsonStream(stream!)
            .Build();

        builder.Services.AddMauiBlazorWebView();

#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
        builder.Logging.AddDebug();
#endif
        
        builder.Services.AddScoped<IDownstreamTokenProvider, DownstreamTokenProvider>();
        builder.Services.AddSingleton<IExternalAuthService, ExternalAuthService>();
        builder.Configuration.AddConfiguration(configuration);
        
        builder.Services.AddAuthorizationCore();
        builder.Services.TryAddScoped<AuthenticationStateProvider, ExternalAuthStateProvider>();
        
        builder.Services.AddScoped<IAccessTokenProvider, AccessTokenProvider>();
            
        SharedConfigurationBuilder.Service(builder.Services, configuration);

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
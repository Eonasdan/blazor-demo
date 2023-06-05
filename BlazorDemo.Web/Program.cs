using BlazorDemo.Models.DownstreamApi;
using BlazorDemo.Web.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

namespace BlazorDemo.Web;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebAssemblyHostBuilder.CreateDefault(args);
        builder.RootComponents.Add<App>("#app");
        builder.RootComponents.Add<HeadOutlet>("head::after");

        builder.Services.AddScoped<CustomAuthorizationMessageHandler>();

        var downstreamApiConfiguration = builder.Configuration.GetSection("DownstreamApi")
            .Get<DownstreamApiConfiguration>()!;
        
        builder.Services.AddHttpClient("WebAPI",
                client => client.BaseAddress = new Uri(downstreamApiConfiguration.BaseUrl!))
            .AddHttpMessageHandler<CustomAuthorizationMessageHandler>();

        builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>()
            .CreateClient("WebAPI"));

        builder.Services.AddMsalAuthentication(options =>

        {
            builder.Configuration.Bind("AzureAd", options.ProviderOptions.Authentication);
            options.ProviderOptions.DefaultAccessTokenScopes.Add("https://graph.microsoft.com/User.Read");
            options.ProviderOptions.DefaultAccessTokenScopes.Add("openid");
            options.ProviderOptions.DefaultAccessTokenScopes.Add("offline_access");
            options.ProviderOptions.AdditionalScopesToConsent.Add(downstreamApiConfiguration.ClientId!);
            options.ProviderOptions.LoginMode = "Redirect";
        });

        builder.Services.AddScoped<IDownstreamTokenProvider, DownstreamTokenProvider>();

        await builder.Build().RunAsync();
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
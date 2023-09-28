using BlazorDemo.Models.DownstreamApi;
using BlazorDemo.Shared;
using BlazorDemo.Web.Services;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

namespace BlazorDemo.Web;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebAssemblyHostBuilder.CreateDefault(args);
        builder.RootComponents.Add<App>("#app");
        builder.RootComponents.Add<HeadOutlet>("head::after");
        
        builder.Services.AddScoped<IDownstreamTokenProvider, DownstreamTokenProvider>();
        
        SharedConfigurationBuilder.Service(builder.Services, builder.Configuration);
        
        var downstreamApiConfiguration = builder.Configuration.GetSection("DownstreamApi")
            .Get<DownstreamApiConfiguration>()!;

        builder.Services.AddMsalAuthentication(options =>

        {
            builder.Configuration.Bind("AzureAd", options.ProviderOptions.Authentication);
            options.ProviderOptions.DefaultAccessTokenScopes.Add("https://graph.microsoft.com/User.Read");
            options.ProviderOptions.DefaultAccessTokenScopes.Add("openid");
            options.ProviderOptions.DefaultAccessTokenScopes.Add("offline_access");
            options.ProviderOptions.AdditionalScopesToConsent.Add(downstreamApiConfiguration.ClientId!);
            options.ProviderOptions.LoginMode = "Redirect";
        });

        await builder.Build().RunAsync();
    }
}
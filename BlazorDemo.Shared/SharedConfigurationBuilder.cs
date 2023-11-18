using BlazorDemo.Models;
using BlazorDemo.Shared.Data;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BlazorDemo.Shared;

public static class SharedConfigurationBuilder
{
    public static void Service(IServiceCollection builderServices, IConfiguration configuration)
    {
        var downstreamApiConfiguration = configuration.GetSection("DownstreamApi")
            .Get<DownstreamApiConfiguration>()!;
        
        builderServices.AddScoped<CustomAuthorizationMessageHandler>();
        
        builderServices.AddHttpClient(JsonClient.ClientConfiguration.WebApi.ToString(),
                client => client.BaseAddress = new Uri(downstreamApiConfiguration.BaseUrl!))
            .AddHttpMessageHandler<CustomAuthorizationMessageHandler>();

        builderServices.AddHttpClient(JsonClient.ClientConfiguration.Unauthenticated.ToString(),
            client => client.BaseAddress = new Uri(downstreamApiConfiguration.BaseUrl!));
        
        builderServices.AddScoped<IAuthenticatedClient, AuthenticatedClient>();
        builderServices.AddScoped<IUnauthenticatedClient, UnauthenticatedClient>();
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
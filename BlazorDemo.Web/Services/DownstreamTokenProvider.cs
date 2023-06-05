using BlazorDemo.Models.DownstreamApi;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.JSInterop;

namespace BlazorDemo.Web.Services;

public class DownstreamTokenProvider : IDownstreamTokenProvider
{
    private readonly IAccessTokenProvider _tokenProvider;
    private readonly IJSRuntime _js;
    private readonly DownstreamApiConfiguration _downstreamApiConfiguration;

    public DownstreamTokenProvider(IConfiguration configuration,
        IAccessTokenProvider tokenProvider, IJSRuntime js)
    {
        _tokenProvider = tokenProvider;
        _js = js;
        _downstreamApiConfiguration = configuration.GetRequiredSection("DownstreamApi")
            .Get<DownstreamApiConfiguration>()!;
    }

    public async Task<string?> TryGetToken()
    {
        var accessTokenResult = await _tokenProvider.RequestAccessToken(new AccessTokenRequestOptions
        {
            Scopes = _downstreamApiConfiguration.Scopes
        });
        Console.WriteLine(accessTokenResult.Status);
        if (!accessTokenResult.TryGetToken(out var token))
        {
            Console.WriteLine($"No token");
            return null;
        }

        Console.WriteLine($"Got token");
        var accessToken = token.Value;
        return accessToken;
    }
}
using BlazorDemo.Models.DownstreamApi;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;

namespace BlazorDemo.Web.Services;

public class DownstreamTokenProvider : IDownstreamTokenProvider
{
    private readonly IAccessTokenProvider _tokenProvider;
    private readonly DownstreamApiConfiguration _downstreamApiConfiguration;

    public DownstreamTokenProvider(IConfiguration configuration,
        IAccessTokenProvider tokenProvider)
    {
        _tokenProvider = tokenProvider;
        _downstreamApiConfiguration = configuration.GetRequiredSection("DownstreamApi")
            .Get<DownstreamApiConfiguration>()!;
    }

    public async Task<string?> TryGetToken()
    {
        var accessTokenResult = await _tokenProvider.RequestAccessToken(new AccessTokenRequestOptions
        {
            Scopes = _downstreamApiConfiguration.Scopes
        });
        if (!accessTokenResult.TryGetToken(out var token))
        {
            return null;
        }

        var accessToken = token.Value;
        return accessToken;
    }
}
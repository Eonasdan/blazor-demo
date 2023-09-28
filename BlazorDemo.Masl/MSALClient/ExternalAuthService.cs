using System.Security.Claims;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Identity.Client;

namespace BlazorDemo.Masl.MSALClient;

public class ExternalAuthService : IExternalAuthService
{
    public string[]? Scopes { get; private set; }
    public string? Token { get; private set; }


    private ClaimsPrincipal? _currentUser;
    private AuthenticationResult? _authResult;

    public ClaimsPrincipal CurrentUser
    {
        get => _currentUser ?? new ClaimsPrincipal();
        set
        {
            _currentUser = value;

            UserChanged?.Invoke(_currentUser);
        }
    }

    public event Action<ClaimsPrincipal>? UserChanged;

    public ExternalAuthService(IConfiguration appConfiguration)
    {
        var azureADConfig = appConfiguration.GetSection("AzureAdB2C").Get<AzureADB2CConfig>();
        MSALClientHelper = new MSALClientHelper(azureADConfig!);

        Task.Run(async () => await MSALClientHelper.InitializePublicClientAppAsync()).Wait();

        var downStreamApiConfig = appConfiguration.GetSection("DownstreamApi").Get<DownStreamApiConfig>();
        DownstreamApiHelper = new DownstreamApiHelper(downStreamApiConfig!, MSALClientHelper);

        PlatformConfig.Instance.ClientId = "";
    }

    public async Task LoginAsync()
    {
        CurrentUser = new ClaimsPrincipal();
        Token = null;
        try
        {
            _authResult = await AcquireTokenSilentAsync();

            if (_authResult == null) return;

            var claimsIdentity = new ClaimsIdentity(_authResult.ClaimsPrincipal.Claims, "Basic");

            CurrentUser = new ClaimsPrincipal(claimsIdentity);
            Token = _authResult.AccessToken;
        }
        catch
        {
            // ignored
        }
    }

    public AccessToken? GetAccessToken()
    {
        if (_authResult == null) return null;
        return new AccessToken
        {
            GrantedScopes = _authResult.Scopes.ToList(),
            Expires = _authResult.ExpiresOn, 
            Value = _authResult.AccessToken
        };
    }

    /// <summary>
    /// Gets the instance of MSALClientHelper.
    /// </summary>
    public DownstreamApiHelper DownstreamApiHelper { get; }

    /// <summary>
    /// Gets the instance of MSALClientHelper.
    /// </summary>
    public MSALClientHelper MSALClientHelper { get; }

    /// <summary>
    /// This will determine if the Interactive Authentication should be Embedded or System view
    /// </summary>
    public bool UseEmbedded { get; set; } = false;

    //// Custom logger for sample
    //private readonly IdentityLogger _logger = new IdentityLogger();

    /// <summary>
    /// Acquire the token silently
    /// </summary>
    /// <returns>An access token</returns>
    public async Task<AuthenticationResult?> AcquireTokenSilentAsync()
    {
        // Get accounts by policy
        return await AcquireTokenSilentAsync(GetScopes()).ConfigureAwait(false);
    }

    /// <summary>
    /// Acquire the token silently
    /// </summary>
    /// <param name="scopes">desired scopes</param>
    /// <returns>An access token</returns>
    public async Task<AuthenticationResult?> AcquireTokenSilentAsync(string[]? scopes)
    {
        return await MSALClientHelper.SignInUserAndAcquireAccessToken(scopes).ConfigureAwait(false);
    }

    /// <summary>
    /// Perform the interactive acquisition of the token for the given scope
    /// </summary>
    /// <param name="scopes">desired scopes</param>
    /// <returns></returns>
    public async Task<AuthenticationResult?> AcquireTokenInteractiveAsync(string[] scopes)
    {
        if (scopes == null) throw new ArgumentNullException(nameof(scopes));
        MSALClientHelper.UseEmbedded = UseEmbedded;
        return await MSALClientHelper.SignInUserInteractivelyAsync(scopes).ConfigureAwait(false);
    }

    /// <summary>
    /// It will sign out the user.
    /// </summary>
    /// <returns></returns>
    public async Task SignOutAsync()
    {
        await MSALClientHelper.SignOutUserAsync().ConfigureAwait(false);
        Token = null;
        CurrentUser = new ClaimsPrincipal();
    }

    /// <summary>
    /// Gets scopes for the application
    /// </summary>
    /// <returns>An array of all scopes</returns>
    internal string[]? GetScopes()
    {
        return DownstreamApiHelper.DownstreamApiConfig.ScopesArray;
    }
}
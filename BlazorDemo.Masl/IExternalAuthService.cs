using System.Security.Claims;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.Identity.Client;

namespace BlazorDemo.Masl
{
    public interface IExternalAuthService
    {
        string[]? Scopes { get; }
        string? Token { get; }
        ClaimsPrincipal CurrentUser { get; }
        public event Action<ClaimsPrincipal>? UserChanged;
        Task<AuthenticationResult?> AcquireTokenInteractiveAsync(string[] scopes);
        Task<AuthenticationResult?> AcquireTokenSilentAsync(string[]? scopes);
        Task SignOutAsync();
        Task LoginAsync();
        AccessToken? GetAccessToken();
    }
}
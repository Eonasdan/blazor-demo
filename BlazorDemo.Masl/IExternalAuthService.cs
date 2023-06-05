using System.Security.Claims;

namespace BlazorDemo.Masl
{
    public interface IExternalAuthService
    {
        string[] Scopes { get; }
        string? Token { get; }
        ClaimsPrincipal CurrentUser { get; }
        public event Action<ClaimsPrincipal>? UserChanged;
        Task AcquireTokenInteractiveAsync(string[] scopes);
        Task AcquireTokenSilentAsync(string[] scopes);
        Task SignOutAsync();
        Task LoginAsync();
    }
}
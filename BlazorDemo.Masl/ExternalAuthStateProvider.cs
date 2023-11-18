using BlazorDemo.Masl.MSALClient;
using Microsoft.AspNetCore.Components.Authorization;

namespace BlazorDemo.Masl;

public class ExternalAuthStateProvider : AuthenticationStateProvider
{
    private AuthenticationState _currentUser;

    public ExternalAuthStateProvider(IExternalAuthService service)
    {
        _currentUser = new AuthenticationState(service.CurrentUser);

        service.UserChanged += newUser =>
        {
            _currentUser = new AuthenticationState(newUser);
            NotifyAuthenticationStateChanged(Task.FromResult(_currentUser));
        };
    }

    public override Task<AuthenticationState> GetAuthenticationStateAsync() =>
        Task.FromResult(_currentUser);
}
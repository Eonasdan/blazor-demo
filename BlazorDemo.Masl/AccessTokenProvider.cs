using BlazorDemo.Masl.MSALClient;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;

namespace BlazorDemo.Masl;

public class AccessTokenProvider : IAccessTokenProvider
{
    private readonly IExternalAuthService _service;

    public AccessTokenProvider(IExternalAuthService service)
    {
        _service = service;
    }

    public ValueTask<AccessTokenResult> RequestAccessToken()
    {
        return GetToken();
    }

    public ValueTask<AccessTokenResult> RequestAccessToken(AccessTokenRequestOptions options)
    {
        return GetToken();
    }

    private async ValueTask<AccessTokenResult> GetToken()
    {
        var interactiveRequestOptions = new InteractiveRequestOptions
            { ReturnUrl = "", Interaction = InteractionType.SignIn };

        if (_service.Token == null)
        {
            await _service.LoginAsync();
        }

        return new AccessTokenResult(AccessTokenResultStatus.Success,
            _service.GetAccessToken(),
            "",
            interactiveRequestOptions);
    }
}
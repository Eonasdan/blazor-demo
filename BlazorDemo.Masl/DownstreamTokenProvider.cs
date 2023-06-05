using BlazorDemo.Models.DownstreamApi;

namespace BlazorDemo.Masl;

public class DownstreamTokenProvider : IDownstreamTokenProvider
{
    private readonly IExternalAuthService _service;

    public DownstreamTokenProvider(IExternalAuthService service)
    {
        _service = service;
    }
    
    public Task<string?> TryGetToken()
    {
        return Task.FromResult(_service.Token);
    }
}
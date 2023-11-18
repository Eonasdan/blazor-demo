namespace BlazorDemo.Shared.Data;

public interface IAuthenticatedClient
{
    Task<string> GetAsync();
}

public class AuthenticatedClient : JsonClient, IAuthenticatedClient
{
    public AuthenticatedClient(IHttpClientFactory clientFactory) : base(clientFactory)
    {
        HttpClient.BaseAddress = new Uri(HttpClient.BaseAddress!, "/api/color/");
    }
    
    public async Task<string> GetAsync()
    {
        var result = await GetAsync("");

        return result;
    }
}
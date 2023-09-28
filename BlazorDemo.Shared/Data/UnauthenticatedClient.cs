using BlazorDemo.Models;

namespace BlazorDemo.Shared.Data;

public interface IUnauthenticatedClient
{
    Task<List<WeatherForecast>> GetAsync();
}

public class UnauthenticatedClient : JsonClient, IUnauthenticatedClient
{
    public UnauthenticatedClient(IHttpClientFactory clientFactory) :
        base(clientFactory, ClientConfiguration.Unauthenticated)
    {
        HttpClient.BaseAddress = new Uri(HttpClient.BaseAddress!, "/api/WeatherForecast/");
    }
    
    public async Task<List<WeatherForecast>> GetAsync()
    {
        var result = await GetAsync<List<WeatherForecast>>("");

        return result ?? new List<WeatherForecast>();
    }
}
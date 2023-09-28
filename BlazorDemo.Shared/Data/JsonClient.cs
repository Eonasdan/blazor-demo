using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;

namespace BlazorDemo.Shared.Data;

public class JsonClient : IDisposable
{
    public enum ClientConfiguration
    {
        WebApi,
        Unauthenticated
    }
    
    protected readonly HttpClient HttpClient;

    // ReSharper disable once MemberCanBeProtected.Global
    public JsonClient(IHttpClientFactory factory, 
        ClientConfiguration configuration = ClientConfiguration.WebApi)
    {
        HttpClient = factory.CreateClient(configuration.ToString());
        HttpClient.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json"));
    }
    
    public async Task<string> PostAsync(string url, HttpContent content)
    {
        var responseMessage = await HttpClient.PostAsync(url, content);
        return await VerifySuccessAsync(responseMessage);
    }

    public async Task<string> GetAsync(string url)
    {
        var responseMessage = await HttpClient.GetAsync(url);
        return await VerifySuccessAsync(responseMessage);
    }
    
    public async Task<string> PutAsync(string url, HttpContent content)
    {
        var responseMessage = await HttpClient.PutAsync(url, content);
        return await VerifySuccessAsync(responseMessage);
    }
    
    protected async Task<T?> PostAsync<T>(string url, HttpContent content)
    {
        return JsonSerializer.Deserialize<T>(await PostAsync(url, content));
    }
    
    protected async Task<T?> PutAsync<T>(string url, HttpContent content)
    {
        return JsonSerializer.Deserialize<T>(await PutAsync(url, content));
    }

    protected async Task<T?> GetAsync<T>(string url)
    {
        return JsonSerializer.Deserialize<T>(await GetAsync(url));
    }
    
    protected async Task<bool> DeleteAsync(string url)
    {
        var responseMessage = await HttpClient.DeleteAsync(url);
        return responseMessage.IsSuccessStatusCode;
    }

    private static async Task<string> VerifySuccessAsync(HttpResponseMessage responseMessage)
    {
        if (responseMessage.Content == null) throw new SimpleHttpResponseException(responseMessage.StatusCode, "No content");

        var content = await responseMessage.Content.ReadAsStringAsync();
        if (responseMessage.IsSuccessStatusCode) return content;

        responseMessage.Content.Dispose();

        throw new SimpleHttpResponseException(responseMessage.StatusCode, content);
    }

    public void Dispose()
    {
        HttpClient.Dispose();
        GC.SuppressFinalize(this);
    }
}

public class SimpleHttpResponseException : Exception
{
    public HttpStatusCode StatusCode { get; }

    public SimpleHttpResponseException(HttpStatusCode statusCode, string content) : base(content)
    {
        StatusCode = statusCode;
    }
}

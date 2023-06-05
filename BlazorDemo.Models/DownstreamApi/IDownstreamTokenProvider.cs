namespace BlazorDemo.Models.DownstreamApi;

public interface IDownstreamTokenProvider
{
   Task<string?> TryGetToken();
}
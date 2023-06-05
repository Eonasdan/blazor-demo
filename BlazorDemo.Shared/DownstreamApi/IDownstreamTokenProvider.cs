namespace BlazorDemo.Shared.DownstreamApi;

public interface IDownstreamTokenProvider
{
   Task<string?> TryGetToken();
}
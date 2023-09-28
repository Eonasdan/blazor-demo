namespace BlazorDemo.API.ApiModules;

public class ColorModule : IApiModule
{
    private readonly string[] _colors = { "red", "blue", "green", "yellow", "orange" };
    private static readonly Random Random = new();
    
    public void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        const string path = "/api/color";
        var colorGroup = endpoints.MapGroup(path);
        colorGroup.MapGet("/", () => _colors[Random.Next(_colors.Length)]);
    }
}
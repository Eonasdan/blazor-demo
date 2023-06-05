namespace BlazorDemo.API.ApiModules;

public interface IApiModule
{
    void MapEndpoints(IEndpointRouteBuilder endpoints);
}

public static class AppExtensions
{
    public static void MapApiModules(this IEndpointRouteBuilder app)
    {
        var root = app.MapGroup("");
        
        typeof(Program).Assembly
            .GetTypes()
            .Where(p => p.IsClass && p.IsAssignableTo(typeof(IApiModule)))
            .Select(Activator.CreateInstance)
            .Cast<IApiModule>()
            .ToList()
            .ForEach(module => module.MapEndpoints(root));
    }
}

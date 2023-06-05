using BlazorDemo.Models;
using Microsoft.AspNetCore.Mvc;

namespace BlazorDemo.API.ApiModules;

public class WeatherModule : IApiModule
{
    public void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        const string path = "/api/weather";
        var weightGroup = endpoints.MapGroup(path);
        weightGroup.MapGet("/{id:int?}", async ([FromRoute]int? id) =>
        {
            return WeatherForecast.GetRandom();
        });
    }
}
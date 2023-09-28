using System.Text.Json.Serialization;

namespace BlazorDemo.Models;

public class WeatherForecast
{
    [JsonPropertyName("date")]
    public DateOnly Date { get; set; }

    [JsonPropertyName("temperatureC")]
    public int TemperatureC { get; set; }

    [JsonPropertyName("temperatureF")]
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);

    [JsonPropertyName("summary")]
    public string Summary { get; set; }
    
    [JsonIgnore]
    private static readonly string[] Summaries = {
        "cloud", "sun", "snowflake", "wind", "cloud-sun-rain", "cloud-sun", "cloud-rain"
    };

    public static IEnumerable<WeatherForecast> GetRandom()
    {
        return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })
            .ToArray();
    }
}
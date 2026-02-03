using System.Net.Http.Json;
using Weather.API.Data;      // <-- gives access to WeatherDbContext
using Weather.API.Entities;


public interface IWeatherStore
{
    WeatherResult? Latest { get; set; }
}

public class WeatherStore : IWeatherStore
{
    public WeatherResult? Latest { get; set; }
}

public class WeatherJob
{
    private readonly HttpClient _http;
    private readonly IWeatherStore _store;
    private readonly IServiceProvider _services;

    public WeatherJob(HttpClient http, IWeatherStore store, IServiceProvider services)
    {
        _http = http;
        _store = store;
        _services = services;
    }


    public async Task FetchWeatherAsync()
    {
        
        using var scope = _services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<WeatherDbContext>();
        
        var url = "https://api.met.no/weatherapi/locationforecast/2.0/compact?lat=60.10&lon=9.58";
        Console.WriteLine($"\n[{DateTime.Now:HH:mm:ss}] Fetching weather data...");

        var response = await _http.GetAsync(url);
        response.EnsureSuccessStatusCode();

        var weather = await response.Content.ReadFromJsonAsync<WeatherResponse>();

        if (weather?.properties?.timeseries is { Count: > 0 } list)
        {
            var first = list[0];
            var d = first.data?.instant?.details;

            var result = new WeatherResult(
                Time: first.time,
                Temperature: d?.air_temperature,
                Pressure: d?.air_pressure_at_sea_level,
                Humidity: d?.relative_humidity,
                CloudCover: d?.cloud_area_fraction,
                WindSpeed: d?.wind_speed,
                WindDirection: d?.wind_from_direction,
                PrecipNextHour: first.data?.next_1_hours?.details?.precipitation_amount
            );

                        // Save latest to shared store
            _store.Latest = result;
            Console.WriteLine($"Stored latest weather: {result.Temperature} °C");

            // Save to database
            var entity = new WeatherEntity
            {
                Time = result.Time,
                Temperature = result.Temperature,
                Pressure = result.Pressure,
                Humidity = result.Humidity,
                CloudCover = result.CloudCover,
                WindSpeed = result.WindSpeed,
                WindDirection = result.WindDirection,
                PrecipNextHour = result.PrecipNextHour
            };

            db.WeatherResults.Add(entity);
            await db.SaveChangesAsync();

            Console.WriteLine($"Saved to DB: {result.Temperature} °C");
        }
        else
        {
            Console.WriteLine("❌ No timeseries data received.");
        }
    }
}

public record WeatherResult(
    DateTime Time,
    double? Temperature,
    double? Pressure,
    double? Humidity,
    double? CloudCover,
    double? WindSpeed,
    double? WindDirection,
    double? PrecipNextHour
);

public class WeatherResponse
{
    public string? type { get; set; }
    public Geometry? geometry { get; set; }
    public Properties? properties { get; set; }
}

public class Geometry
{
    public string? type { get; set; }
    public List<double>? coordinates { get; set; }
}

public class Properties
{
    public Meta? meta { get; set; }
    public List<TimeSeries>? timeseries { get; set; }
}

public class Meta
{
    public DateTime updated_at { get; set; }
    public Units? units { get; set; }
}

public class Units
{
    public string? air_temperature { get; set; }
    public string? air_pressure_at_sea_level { get; set; }
    public string? cloud_area_fraction { get; set; }
    public string? precipitation_amount { get; set; }
    public string? relative_humidity { get; set; }
    public string? wind_from_direction { get; set; }
    public string? wind_speed { get; set; }
}

public class TimeSeries
{
    public DateTime time { get; set; }
    public TimeSeriesData? data { get; set; }
}

public class TimeSeriesData
{
    public Instant? instant { get; set; }
    public NextHours? next_1_hours { get; set; }
    public NextHours? next_6_hours { get; set; }
    public NextHours? next_12_hours { get; set; }
}

public class Instant
{
    public Details? details { get; set; }
}

public class Details
{
    public double air_temperature { get; set; }
    public double air_pressure_at_sea_level { get; set; }
    public double cloud_area_fraction { get; set; }
    public double relative_humidity { get; set; }
    public double wind_from_direction { get; set; }
    public double wind_speed { get; set; }
}

public class NextHours
{
    public Summary? summary { get; set; }
    public NextHoursDetails? details { get; set; }
}

public class Summary
{
    public string? symbol_code { get; set; }
}

public class NextHoursDetails
{
    public double precipitation_amount { get; set; }
}
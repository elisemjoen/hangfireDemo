using Hangfire;
using Hangfire.SqlServer;
using Microsoft.EntityFrameworkCore;
using Weather.API.Data;
using Weather.API.Entities;

var builder = WebApplication.CreateBuilder(args);

// Hangfire storage
builder.Services.AddHangfire(config =>
    config.UseSqlServerStorage(
        builder.Configuration.GetConnectionString("HangfireDb"),
        new SqlServerStorageOptions { PrepareSchemaIfNecessary = true }
    )
);
builder.Services.AddHangfireServer();

builder.Services.AddDbContext<WeatherDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("WeatherDb")));


// Shared store for latest weather (singleton)
builder.Services.AddSingleton<IWeatherStore, WeatherStore>();

// Typed HttpClient for WeatherJob with proper User-Agent
builder.Services.AddHttpClient<WeatherJob>(client =>
{
    client.DefaultRequestHeaders.UserAgent.Clear();
    client.DefaultRequestHeaders.UserAgent.ParseAdd(
        "HangfireWeatherDemo/1.0 (contact: elise.mjoen@capgemini.com)"
    );
});

var app = builder.Build();

app.UseHangfireDashboard();

// Schedule the recurring job (every minute)
var manager     = app.Services.GetRequiredService<IRecurringJobManager>();
var weatherJob  = app.Services.GetRequiredService<WeatherJob>();
var jobClient   = app.Services.GetRequiredService<IBackgroundJobClient>();

manager.AddOrUpdate(
    "weather-job",
    () => weatherJob.FetchWeatherAsync(),
    "* * * * *"
);

// Optional: trigger one immediate run at startup so the page has data right away.
jobClient.Enqueue(() => weatherJob.FetchWeatherAsync());

// Root page showing the latest data from the shared store
app.MapGet("/", (IWeatherStore store) =>
{
    var latest = store.Latest;
    if (latest is null)
    {
        return Results.Content("No weather data fetched yet.", "text/plain");
    }

    var html = $@"
<html>
<head>
    <meta http-equiv=""refresh"" content=""30"" />
    <style>
        body {{ font-family: system-ui, Segoe UI, Arial, sans-serif; margin: 2rem; }}
        h1 {{ margin-top: 0; }}
        ul {{ line-height: 1.8; }}
        .muted {{ color: #666; }}
    </style>
</head>
<body>
    <h1>Latest Weather</h1>
    <div class=""muted"">Auto-refreshes every 30s</div>
    <ul>
        <li><b>Time:</b> {latest.Time:yyyy-MM-dd HH:mm}</li>
        <li><b>Temperature:</b> {latest.Temperature} °C</li>
        <li><b>Pressure:</b> {latest.Pressure} hPa</li>
        <li><b>Humidity:</b> {latest.Humidity} %</li>
        <li><b>Cloud cover:</b> {latest.CloudCover} %</li>
        <li><b>Wind:</b> {latest.WindSpeed} m/s from {latest.WindDirection}°</li>
        <li><b>Precip next hour:</b> {latest.PrecipNextHour} mm</li>
    </ul>

    <div class=""muted"">
        <a href=""/hangfire"">Hangfire dashboard</a> |
        <a href=""/weather/latest"">Latest JSON</a> |
        <a href=""/testweather"">Trigger fetch now</a>
    </div>
</body>
</html>";

    return Results.Content(html, "text/html");
});

// JSON endpoint (useful for debugging/Frontend later)
app.MapGet("/weather/latest", (IWeatherStore store) =>
    store.Latest is null ? Results.NoContent() : Results.Json(store.Latest));

// Manual trigger (still handy)
app.MapGet("/testweather", async (WeatherJob job) =>
{
    Console.WriteLine("Manual weather fetch triggered.");
    await job.FetchWeatherAsync();
    return Results.Text("Triggered — see console.");
});

app.Run();
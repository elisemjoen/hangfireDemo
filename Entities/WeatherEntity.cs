namespace Weather.API.Entities;

public class WeatherEntity
{
    public int Id { get; set; }
    public DateTime Time { get; set; }

    public double? Temperature { get; set; }
    public double? Pressure { get; set; }
    public double? Humidity { get; set; }
    public double? CloudCover { get; set; }
    public double? WindSpeed { get; set; }
    public double? WindDirection { get; set; }
    public double? PrecipNextHour { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}

namespace AuroraFix.Models;

public class Weather
{
    public double CloudCoverage { get; set; }
    public DateTime ForecastTime { get; set; }
    public string WeatherDescription { get; set; } = string.Empty;
    public bool IsDay { get; set; }
    public DateTime? Sunrise { get; set; }
    public DateTime? Sunset { get; set; }
}

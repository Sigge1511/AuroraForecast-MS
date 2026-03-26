namespace AuroraFix.Models;

public class AuroraForecast
{
    public DateTime ForecastTime { get; set; }
    public double KpIndex { get; set; }
    public string Location { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public int Probability { get; set; } // 0-100%
    public string ActivityLevel { get; set; } = string.Empty;
}

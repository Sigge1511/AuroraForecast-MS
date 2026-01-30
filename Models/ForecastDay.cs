namespace AuroraForecast.Models;

public class ForecastDay
{
    public string Date { get; set; } = string.Empty;
    public double KpIndex { get; set; }
    public string ActivityLevel { get; set; } = string.Empty;
    public string IconEmoji { get; set; } = string.Empty;
    
    public string DisplayText => $"{Date}: Kp {KpIndex:F1} ({ActivityLevel})";
}

namespace AuroraForecast.Models;

public class AuroraForecast
{
    public DateTime ForecastTime { get; set; }
    public double KpIndex { get; set; }
    public string Location { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public int Probability { get; set; } // 0-100%
    public string ActivityLevel { get; set; } = string.Empty;
    
    public string GetActivityDescription(double probability)
    {
        return probability switch
        {
            <= 10 => "Quiet skies. Very low chance of aurora activity right now. Perfect time for some sleep.",
            <= 30 => "Low activity. You might see a faint glow on the horizon with a camera, but it's hard for the naked eye.",
            <= 50 => "Unsettled. Aurora might be visible in dark areas away from city lights. Keep a lookout!",
            <= 75 => "High chance! Active aurora likely. Find a dark spot, look north, and let your eyes adjust.",
            _ => "Strong activity! Major aurora possible even with light pollution. Look up and enjoy the show!"
        };
    }
}

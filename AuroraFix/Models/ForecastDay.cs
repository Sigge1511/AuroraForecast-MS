namespace AuroraFix.Models;

public class ForecastDay
{
    public string Date { get; set; } = string.Empty;
    public DateTime ForecastDate { get; set; }  // Parsed calendar date for reliable weather matching
    public double KpIndex { get; set; }
    public double Probability { get; set; } //for better UX
    public string ActivityLevel { get; set; } = string.Empty;
    public string IconEmoji { get; set; } = string.Empty;
    public double? CloudCoverage { get; set; }
    public int? ActualProbability { get; set; }  // Adjusted probability (base - cloud penalty)

    public string DisplayText => $"{Date}: Kp {KpIndex:F1} ({ActivityLevel})";
}

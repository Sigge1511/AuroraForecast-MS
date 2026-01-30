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
    
    public string GetActivityDescription()
    {
        return ActivityLevel switch
        {
            "Low" => "Låg aktivitet - Svag chans att se norrsken",
            "Medium" => "Måttlig aktivitet - God chans vid mörk himmel",
            "Active" => "Hög aktivitet - Mycket goda chanser!",
            "Storm" => "Storm! - Utmärkta chanser även på lägre breddgrader",
            _ => "Okänd aktivitet"
        };
    }
}

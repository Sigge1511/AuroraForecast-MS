using System.Net.Http.Json;
using System.Text.Json;
using AuroraForecast.Models;

namespace AuroraForecast.Services;

public class AuroraService
{
    private readonly HttpClient _httpClient;
    private const string KpIndexUrl = "https://services.swpc.noaa.gov/json/planetary_k_index_1m.json";
    private const string KpForecastUrl = "https://services.swpc.noaa.gov/products/noaa-planetary-k-index-forecast.json";

    public AuroraService()
    {
        _httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(30)
        };
    }

    public async Task<double> GetCurrentKpIndexAsync()
    {
        try
        {
            var response = await _httpClient.GetStringAsync(KpIndexUrl);
            var jsonArray = JsonSerializer.Deserialize<JsonElement[]>(response);
            
            if (jsonArray != null && jsonArray.Length > 0)
            {
                var latest = jsonArray[^1];
                
                if (latest.TryGetProperty("kp_index", out var kpValue))
                {
                    return kpValue.GetDouble();
                }
                
                if (latest.TryGetProperty("Kp", out var kpAlt))
                {
                    return kpAlt.GetDouble();
                }
            }
            
            return 0;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error fetching Kp index: {ex.Message}");
            return 2.5; // Fallback värde
        }
    }

    public async Task<List<ForecastDay>> GetThreeDayForecastAsync()
    {
        try
        {
            var response = await _httpClient.GetStringAsync(KpForecastUrl);
            var lines = response.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            
            var forecasts = new List<ForecastDay>();
            var today = DateTime.UtcNow.Date;
            
            foreach (var line in lines.Skip(1))
            {
                if (line.StartsWith("#") || string.IsNullOrWhiteSpace(line))
                    continue;
                
                var parts = line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                
                if (parts.Length >= 3 && double.TryParse(parts[2], out var kp))
                {
                    if (forecasts.Count < 3)
                    {
                        var forecastDate = today.AddDays(forecasts.Count);
                        forecasts.Add(new ForecastDay
                        {
                            Date = forecastDate.ToString("ddd dd MMM"),
                            KpIndex = kp,
                            ActivityLevel = GetActivityLevel(kp),
                            IconEmoji = GetIconEmoji(kp)
                        });
                    }
                }
            }
            
            if (forecasts.Count == 0)
            {
                for (int i = 0; i < 3; i++)
                {
                    forecasts.Add(new ForecastDay
                    {
                        Date = today.AddDays(i).ToString("ddd dd MMM"),
                        KpIndex = 2.5,
                        ActivityLevel = "Low",
                        IconEmoji = "🌙"
                    });
                }
            }
            
            return forecasts;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error fetching forecast: {ex.Message}");
            
            var forecasts = new List<ForecastDay>();
            var today = DateTime.UtcNow.Date;
            
            for (int i = 0; i < 3; i++)
            {
                forecasts.Add(new ForecastDay
                {
                    Date = today.AddDays(i).ToString("ddd dd MMM"),
                    KpIndex = 2.5,
                    ActivityLevel = "Low",
                    IconEmoji = "🌙"
                });
            }
            
            return forecasts;
        }
    }

    public static string GetActivityLevel(double kp)
    {
        return kp switch
        {
            >= 7 => "Storm",
            >= 5 => "Active",
            >= 3 => "Medium",
            _ => "Low"
        };
    }

    private static string GetIconEmoji(double kp)
    {
        return kp switch
        {
            >= 7 => "⚡",
            >= 5 => "✨",
            >= 3 => "🌟",
            _ => "🌙"
        };
    }

    public async Task<Models.AuroraForecast> GetForecastForLocationAsync(string cityName, double latitude, double longitude)
    {
        var kpIndex = await GetCurrentKpIndexAsync();
        
        return new Models.AuroraForecast
        {
            ForecastTime = DateTime.UtcNow,
            KpIndex = kpIndex,
            Location = cityName,
            Latitude = latitude,
            Longitude = longitude,
            Probability = CalculateProbability(kpIndex, latitude),
            ActivityLevel = GetActivityLevel(kpIndex)
        };
    }

    private int CalculateProbability(double kp, double latitude)
    {
        var absLat = Math.Abs(latitude);
        
        int baseProbability = kp switch
        {
            >= 7 => 90,
            >= 5 => 70,
            >= 3 => 40,
            _ => 15
        };
        
        if (absLat > 65) baseProbability = Math.Min(100, baseProbability + 20);
        else if (absLat > 55) baseProbability = Math.Min(100, baseProbability + 10);
        else if (absLat < 45) baseProbability = Math.Max(0, baseProbability - 20);
        
        return baseProbability;
    }
}

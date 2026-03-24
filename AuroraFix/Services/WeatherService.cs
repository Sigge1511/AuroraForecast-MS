using System.Text.Json;
using AuroraFix.Models;

namespace AuroraFix.Services;

public class WeatherService
{
    private static WeatherService? _instance;
    private static readonly object _lock = new();
    private readonly HttpClient _httpClient;

    // Open-Metero API, free and no key needed
    private const string BaseUrl = "https://api.open-meteo.com/v1/forecast";

    public WeatherService()
    {
        _httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(30)
        };
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "AuroraFix-MAUI/1.0");
    }

    public static WeatherService Instance
    {
        get
        {
            if (_instance == null)
            {
                lock (_lock)
                {
                    _instance ??= new WeatherService();
                }
            }
            return _instance;
        }
    }

    public async Task<Weather?> GetCurrentWeatherAsync(double latitude, double longitude)
    {
        try
        {
            var url = $"{BaseUrl}?" +
                      $"latitude={latitude.ToString(System.Globalization.CultureInfo.InvariantCulture)}&" +
                      $"longitude={longitude.ToString(System.Globalization.CultureInfo.InvariantCulture)}&" +
                      $"current=cloud_cover,is_day&" +
                      $"daily=sunrise,sunset&" +
                      $"timezone=auto";

            System.Diagnostics.Debug.WriteLine($"=== Weather API Request: {url} ===");

            var response = await _httpClient.GetStringAsync(url);
            System.Diagnostics.Debug.WriteLine($"=== Weather Response: {response.Substring(0, Math.Min(300, response.Length))}... ===");

            var json = JsonSerializer.Deserialize<JsonElement>(response);

            if (json.TryGetProperty("current", out var current))
            {
                var cloudCoverage = GetDoubleValue(current, "cloud_cover");

                var conditions = new Weather
                {
                    CloudCoverage = cloudCoverage,
                    ForecastTime = DateTime.UtcNow,
                    IsDay = (int)GetDoubleValue(current, "is_day") == 1,
                    WeatherDescription = GetWeatherDescription(cloudCoverage)
                };

                if (json.TryGetProperty("daily", out var todayDaily))
                {
                    if (todayDaily.TryGetProperty("sunrise", out var srArr) && srArr.GetArrayLength() > 0)
                    {
                        if (DateTime.TryParse(srArr[0].GetString(), out var sr))
                            conditions.Sunrise = sr;
                    }
                    if (todayDaily.TryGetProperty("sunset", out var ssArr) && ssArr.GetArrayLength() > 0)
                    {
                        if (DateTime.TryParse(ssArr[0].GetString(), out var ss))
                            conditions.Sunset = ss;
                    }
                }

                System.Diagnostics.Debug.WriteLine($"=== Weather Parsed: {conditions.CloudCoverage}% clouds, IsDay={conditions.IsDay}, Sunrise={conditions.Sunrise:HH:mm}, Sunset={conditions.Sunset:HH:mm} ===");
                return conditions;
            }

            System.Diagnostics.Debug.WriteLine("=== Weather: No 'current' property in response ===");
            return null;
        }
        catch (HttpRequestException ex)
        {
            System.Diagnostics.Debug.WriteLine($"=== Weather HTTP Error: {ex.Message} ===");
            return null;
        }
        catch (JsonException ex)
        {
            System.Diagnostics.Debug.WriteLine($"=== Weather JSON Error: {ex.Message} ===");
            return null;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"=== Weather Unknown Error: {ex.Message} ===");
            return null;
        }
    }

    // Fetches 3-day cloud coverage forecast
    public async Task<List<Weather>> GetThreeDayForecastAsync(double latitude, double longitude)
    {
        try
        {
            var url = $"{BaseUrl}?" +
                      $"latitude={latitude.ToString(System.Globalization.CultureInfo.InvariantCulture)}&" +
                      $"longitude={longitude.ToString(System.Globalization.CultureInfo.InvariantCulture)}&" +
                      $"daily=cloud_cover_mean,sunrise,sunset&" +
                      $"forecast_days=4&" +
                      $"timezone=auto";

            var response = await _httpClient.GetStringAsync(url);
            var json = JsonSerializer.Deserialize<JsonElement>(response);

            var forecasts = new List<Weather>();

            if (json.TryGetProperty("daily", out var daily))
            {
                var times = daily.GetProperty("time").EnumerateArray().ToList();
                var cloudCovers = daily.GetProperty("cloud_cover_mean").EnumerateArray().ToList();
                daily.TryGetProperty("sunrise", out var sunriseArr);
                daily.TryGetProperty("sunset", out var sunsetArr);

                for (int i = 0; i < Math.Min(4, cloudCovers.Count); i++)
                {
                    var cloudCover = cloudCovers[i].GetDouble();

                    DateTime? sunrise = null, sunset = null;
                    if (sunriseArr.ValueKind != System.Text.Json.JsonValueKind.Undefined && i < sunriseArr.GetArrayLength())
                    {
                        if (DateTime.TryParse(sunriseArr[i].GetString(), out var sr)) sunrise = sr;
                    }
                    if (sunsetArr.ValueKind != System.Text.Json.JsonValueKind.Undefined && i < sunsetArr.GetArrayLength())
                    {
                        if (DateTime.TryParse(sunsetArr[i].GetString(), out var ss)) sunset = ss;
                    }

                    forecasts.Add(new Weather
                    {
                        ForecastTime = DateTime.Parse(times[i].GetString()!),
                        CloudCoverage = cloudCover,
                        WeatherDescription = GetWeatherDescription(cloudCover),
                        Sunrise = sunrise,
                        Sunset = sunset
                    });
                }
            }

            System.Diagnostics.Debug.WriteLine($"=== Fetched {forecasts.Count} days of weather forecast ===");
            return forecasts;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"=== 3-day forecast error: {ex.Message} ===");
            return new List<Weather>();
        }
    }
    
    public static bool IsClearEnoughForAurora(double cloudCoverage)
    {
        return cloudCoverage < 30;
    }

    public static bool IsPerfectForAurora(double cloudCoverage)
    {
        return cloudCoverage < 5;
    }

    private double GetDoubleValue(JsonElement element, string propertyName)
    {
        if (element.TryGetProperty(propertyName, out var prop))
        {
            if (prop.ValueKind == JsonValueKind.Number)
                return prop.GetDouble();
            if (prop.ValueKind == JsonValueKind.String)
                if (double.TryParse(prop.GetString(), out var val))
                    return val;
        }
        return 0;
    }

    private string GetWeatherDescription(double cloudCover)
    {
        return cloudCover switch
        {
            < 5 => "Clear skies",
            < 20 => "Partly cloudy",
            < 50 => "Mostly cloudy",
            _ => "Overcast"
        };
    }

    public async Task<bool> IsApiAvailableAsync()
    {
        try
        {
            var result = await GetCurrentWeatherAsync(59.3293, 18.0686); // Stockholm as test location
            return result != null;
        }
        catch
        {
            return false;
        }
    }



    //public static string GetCloudCondition(double cloudCoverage)
    //{
    //    return cloudCoverage switch
    //    {
    //        < 5 => "Clear skies - perfect!",
    //        < 20 => "Partly cloudy - Okay",
    //        < 50 => "Mostly cloudy - Difficult",
    //        _ => "Overcast - Impossible"
    //    };
    //}
}
using System.Text.Json;
using AuroraFix.Models;

namespace AuroraFix.Services;

public class WeatherService
{
    private readonly HttpClient _httpClient;

    // Open-Meteo API — free, no key required
    private const string BaseUrl = "https://api.open-meteo.com/v1/forecast";
    private const double TestLatitude = 59.3293;   // Stockholm — used for health checks
    private const double TestLongitude = 18.0686;

    public WeatherService()
    {
        _httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(30) };
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "AuroraFix-MAUI/1.0");
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

            var response = await _httpClient.GetStringAsync(url);
            var json = JsonSerializer.Deserialize<JsonElement>(response);

            if (!json.TryGetProperty("current", out var current))
                return null;

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
                    if (DateTime.TryParse(srArr[0].GetString(), out var sr)) conditions.Sunrise = sr;
                if (todayDaily.TryGetProperty("sunset", out var ssArr) && ssArr.GetArrayLength() > 0)
                    if (DateTime.TryParse(ssArr[0].GetString(), out var ss)) conditions.Sunset = ss;
            }

            return conditions;
        }
        catch (HttpRequestException ex)
        {
            System.Diagnostics.Debug.WriteLine($"WeatherService: HTTP error in current weather: {ex.Message}");
            return null;
        }
        catch (JsonException ex)
        {
            System.Diagnostics.Debug.WriteLine($"WeatherService: JSON parse error in current weather: {ex.Message}");
            return null;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"WeatherService: Unexpected error in current weather: {ex.Message}");
            return null;
        }
    }

    // Fetches cloud coverage, sunrise and sunset for today + 3 days ahead (4 total).
    // 4 days are needed because the NOAA 3-day forecast covers tomorrow through day+3,
    // so index 0 here (today) is only used for weather matching on the current-day display.
    public async Task<List<Weather>> GetFourDayForecastAsync(double latitude, double longitude)
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

            if (!json.TryGetProperty("daily", out var daily))
                return forecasts;

            var times = new List<JsonElement>();
            var cloudCovers = new List<JsonElement>();
            if (daily.TryGetProperty("time", out var timeArr))
                times = timeArr.EnumerateArray().ToList();
            if (daily.TryGetProperty("cloud_cover_mean", out var cloudArr))
                cloudCovers = cloudArr.EnumerateArray().ToList();
            daily.TryGetProperty("sunrise", out var sunriseArr);
            daily.TryGetProperty("sunset", out var sunsetArr);

            for (int i = 0; i < Math.Min(4, Math.Min(cloudCovers.Count, times.Count)); i++)
            {
                double cloudCover = 0;
                try
                {
                    cloudCover = cloudCovers[i].GetDouble();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"WeatherService: error parsing cloud_cover_mean: {ex.Message}");
                }
                DateTime? sunrise = null, sunset = null;

                if (sunriseArr.ValueKind != JsonValueKind.Undefined && i < sunriseArr.GetArrayLength())
                    if (DateTime.TryParse(sunriseArr[i].GetString(), out var sr)) sunrise = sr;
                if (sunsetArr.ValueKind != JsonValueKind.Undefined && i < sunsetArr.GetArrayLength())
                    if (DateTime.TryParse(sunsetArr[i].GetString(), out var ss)) sunset = ss;

                DateTime forecastTime = DateTime.MinValue;
                if (times[i].ValueKind == JsonValueKind.String && DateTime.TryParse(times[i].GetString(), out var ft))
                    forecastTime = ft;
                else
                    System.Diagnostics.Debug.WriteLine($"WeatherService: error parsing forecast time at index {i}");

                forecasts.Add(new Weather
                {
                    ForecastTime = forecastTime,
                    CloudCoverage = cloudCover,
                    WeatherDescription = GetWeatherDescription(cloudCover),
                    Sunrise = sunrise,
                    Sunset = sunset
                });
            }

            return forecasts;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"WeatherService: Error fetching 4-day forecast: {ex.Message}");
            return new List<Weather>();
        }
    }

    public async Task<bool> IsApiAvailableAsync()
    {
        try
        {
            return await GetCurrentWeatherAsync(TestLatitude, TestLongitude) != null;
        }
        catch
        {
            return false;
        }
    }

    // Maps cloud coverage percentage to a human-readable sky condition label.
    public static string GetWeatherDescription(double cloudCover) => cloudCover switch
    {
        < 5  => "Clear skies",
        < 20 => "Partly cloudy",
        < 50 => "Mostly cloudy",
        _    => "Overcast"
    };

    private double GetDoubleValue(JsonElement element, string propertyName)
    {
        if (element.TryGetProperty(propertyName, out var prop))
        {
            if (prop.ValueKind == JsonValueKind.Number) return prop.GetDouble();
            if (prop.ValueKind == JsonValueKind.String && double.TryParse(prop.GetString(), out var val)) return val;
        }
        return 0;
    }
}

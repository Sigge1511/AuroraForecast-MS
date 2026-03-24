using AuroraFix.Models;
using AuroraFix.Helpers;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Text.Json;

namespace AuroraFix.Services;

public class AuroraService
{
    private readonly HttpClient _httpClient;
    private const string KpIndexUrl = "https://services.swpc.noaa.gov/json/planetary_k_index_1m.json";
    private const string ForecastUrl = "https://services.swpc.noaa.gov/text/3-day-geomag-forecast.txt";

    public AuroraService()
    {
        _httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(30) };
    }

    public async Task<double> GetCurrentKpIndexAsync()
    {
        try
        {
            var response = await _httpClient.GetStringAsync(KpIndexUrl);
            var jsonArray = JsonSerializer.Deserialize<JsonElement[]>(response);

            if (jsonArray != null && jsonArray.Length > 0)
            {
                // Walk backwards and return the first reading with a valid estimated_kp
                for (int i = jsonArray.Length - 1; i >= 0; i--)
                {
                    if (jsonArray[i].TryGetProperty("estimated_kp", out var est))
                    {
                        var val = est.GetDouble();
                        if (val > 0) return val;
                    }
                }
            }
            return 0;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"AuroraService: error fetching Kp index: {ex.Message}");
            return 0;
        }
    }

    public async Task<IReadOnlyList<ForecastDay>> GetThreeDayForecastAsync(double latitude)
    {
        try
        {
            var response = await _httpClient.GetStringAsync(ForecastUrl);
            var lines = response.Split('\n');

            // Locate the "NOAA Kp index forecast" section header
            var kpSectionStart = -1;
            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i].Contains("NOAA Kp index forecast"))
                {
                    kpSectionStart = i;
                    break;
                }
            }

            if (kpSectionStart == -1) return GetFallbackForecast(latitude);

            // The date line is one row after the section header
            var dateLineIndex = kpSectionStart + 1;
            if (dateLineIndex >= lines.Length) return GetFallbackForecast(latitude);

            var dateLine = lines[dateLineIndex].Trim();
            var dateParts = dateLine.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            if (dateParts.Length < 6) return GetFallbackForecast(latitude);

            // Collect Kp values for each of the three forecast days across the 8 three-hour bands
            var day1Values = new List<double>();
            var day2Values = new List<double>();
            var day3Values = new List<double>();

            for (int i = kpSectionStart + 2; i < Math.Min(kpSectionStart + 10, lines.Length); i++)
            {
                var line = lines[i].Trim();
                if (string.IsNullOrEmpty(line)) continue;

                var parts = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                if (parts.Length >= 4)
                {
                    TryAddKp(parts[1], day1Values);
                    TryAddKp(parts[2], day2Values);
                    TryAddKp(parts[3], day3Values);
                }
            }

            var forecasts = new List<ForecastDay>(3);
            AddForecastDay(forecasts, day1Values, dateParts[0], dateParts[1], latitude);
            AddForecastDay(forecasts, day2Values, dateParts[2], dateParts[3], latitude);
            AddForecastDay(forecasts, day3Values, dateParts[4], dateParts[5], latitude);

            return forecasts.Count == 3 ? forecasts : GetFallbackForecast(latitude);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"AuroraService: error fetching 3-day forecast: {ex.Message}");
            return GetFallbackForecast(latitude);
        }
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
            Probability = (int)ProbabilityDisplayHelper.CalculateAuroraProbability(kpIndex, latitude),
            ActivityLevel = GetActivityLevel(kpIndex)
        };
    }

    public static string GetActivityLevel(double kp) => kp switch
    {
        >= 7 => "Storm",
        >= 5 => "Active",
        >= 3 => "Medium",
        _ => "Low"
    };

    public static string GetIconEmoji(double prob) => prob switch
    {
        >= 85 => "\U0001F7E2",
        >= 50 => "\U0001F7E1",
        >= 20 => "\U0001F7E0",
        _ => "\U0001F534"
    };

    // ── Private helpers ───────────────────────────────────────────────────────

    private static void TryAddKp(string raw, List<double> target)
    {
        if (double.TryParse(raw, NumberStyles.Any, CultureInfo.InvariantCulture, out var val))
            target.Add(val);
    }

    private static void AddForecastDay(
        List<ForecastDay> forecasts,
        List<double> kpValues,
        string monthPart,
        string dayPart,
        double latitude)
    {
        if (!kpValues.Any()) return;

        var avgKp = kpValues.Average();
        var prob = ProbabilityDisplayHelper.CalculateAuroraProbability(avgKp, latitude);

        forecasts.Add(new ForecastDay
        {
            Date = $"{monthPart} {dayPart}",
            ForecastDate = ParseNoaaDate(monthPart, dayPart),
            KpIndex = Math.Round(avgKp, 1),
            Probability = prob,
            ActivityLevel = GetActivityLevel(avgKp),
            IconEmoji = GetIconEmoji(prob)
        });
    }

    private IReadOnlyList<ForecastDay> GetFallbackForecast(double latitude)
    {
        var today = DateTime.UtcNow.Date;
        var forecasts = new List<ForecastDay>(3);

        for (int i = 0; i < 3; i++)
        {
            var date = today.AddDays(i);
            forecasts.Add(new ForecastDay
            {
                Date = date.ToString("ddd dd MMM"),
                ForecastDate = date,
                KpIndex = 0,
                Probability = 0,
                ActivityLevel = "Low",
                IconEmoji = GetIconEmoji(0)
            });
        }

        return forecasts;
    }

    // Parses NOAA abbreviated month + day strings (e.g. "Mar 24") into a UTC DateTime.
    // Handles year-boundary: if the parsed date is more than 60 days in the past, rolls to the next year.
    private static DateTime ParseNoaaDate(string month, string day)
    {
        var year = DateTime.UtcNow.Year;
        if (DateTime.TryParseExact($"{month} {day} {year}", "MMM d yyyy",
            CultureInfo.InvariantCulture, DateTimeStyles.None, out var result))
        {
            if (result.Date < DateTime.UtcNow.Date.AddDays(-60))
                result = result.AddYears(1);
            return result.Date;
        }
        return DateTime.UtcNow.Date;
    }
}

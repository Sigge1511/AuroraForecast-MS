using AuroraForecast.Models;
using System.Collections.ObjectModel;
using System.Text.Json;

namespace AuroraForecast.Services;

public class AuroraService
{
    private readonly HttpClient _httpClient;
    private const string KpIndexUrl = "https://services.swpc.noaa.gov/json/planetary_k_index_1m.json";

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
                // Loopa bakåt och hitta första med estimated_kp > 0
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
            System.Diagnostics.Debug.WriteLine($"Error fetching Kp: {ex.Message}");
            return 0;
        }
    }

    public async Task<ObservableCollection<ForecastDay>> GetThreeDayForecastAsync(double latitude)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine($"=== GetThreeDayForecast called with latitude: {latitude} ===");

            var url = "https://services.swpc.noaa.gov/text/3-day-geomag-forecast.txt";
            var response = await _httpClient.GetStringAsync(url);
            var lines = response.Split('\n');

            // Hitta Kp-prognos sektionen
            var kpSectionStart = -1;
            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i].Contains("NOAA Kp index forecast"))
                {
                    kpSectionStart = i;
                    break;
                }
            }

            if (kpSectionStart == -1)
            {
                System.Diagnostics.Debug.WriteLine("=== KP SECTION NOT FOUND ===");
                return GetFallbackForecast(latitude);
            }

            // Datumraden är 1 rad efter "NOAA Kp index forecast"
            var dateLineIndex = kpSectionStart + 1;
            if (dateLineIndex >= lines.Length)
            {
                System.Diagnostics.Debug.WriteLine("=== DATE LINE NOT FOUND ===");
                return GetFallbackForecast(latitude);
            }

            var dateLine = lines[dateLineIndex].Trim();
            var dateParts = dateLine.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            System.Diagnostics.Debug.WriteLine($"=== Date parts count: {dateParts.Length} ===");

            if (dateParts.Length < 6)
            {
                System.Diagnostics.Debug.WriteLine("=== NOT ENOUGH DATE PARTS ===");
                return GetFallbackForecast(latitude);
            }

            // Samla alla Kp-värden per dag
            var day1Values = new List<double>();
            var day2Values = new List<double>();
            var day3Values = new List<double>();

            // Läs 8 timmars-rader (00-03UT till 21-00UT)
            for (int i = kpSectionStart + 2; i < Math.Min(kpSectionStart + 10, lines.Length); i++)
            {
                var line = lines[i].Trim();
                if (string.IsNullOrEmpty(line)) continue;

                var parts = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                if (parts.Length >= 4)
                {
                    if (double.TryParse(parts[1], System.Globalization.NumberStyles.Any,
                        System.Globalization.CultureInfo.InvariantCulture, out var kp1))
                        day1Values.Add(kp1);

                    if (double.TryParse(parts[2], System.Globalization.NumberStyles.Any,
                        System.Globalization.CultureInfo.InvariantCulture, out var kp2))
                        day2Values.Add(kp2);

                    if (double.TryParse(parts[3], System.Globalization.NumberStyles.Any,
                        System.Globalization.CultureInfo.InvariantCulture, out var kp3))
                        day3Values.Add(kp3);
                }
            }

            System.Diagnostics.Debug.WriteLine($"=== Day1: {day1Values.Count} values, Day2: {day2Values.Count}, Day3: {day3Values.Count} ===");

            // forecasts
            var forecasts = new ObservableCollection<ForecastDay>();

            if (day1Values.Any())
            {
                var avgKp = day1Values.Average();
                var prob = CalculateProbability(avgKp, latitude);
                System.Diagnostics.Debug.WriteLine($"=== Day 1: Kp={avgKp:F1}, Prob={prob}% ===");

                forecasts.Add(new ForecastDay
                {
                    Date = $"{dateParts[0]} {dateParts[1]}",
                    KpIndex = Math.Round(avgKp, 1),
                    Probability = prob,
                    ActivityLevel = GetActivityLevel(avgKp),
                    IconEmoji = GetIconEmoji(avgKp)
                });
            }

            if (day2Values.Any())
            {
                var avgKp = day2Values.Average();
                var prob = CalculateProbability(avgKp, latitude);
                System.Diagnostics.Debug.WriteLine($"=== Day 2: Kp={avgKp:F1}, Prob={prob}% ===");

                forecasts.Add(new ForecastDay
                {
                    Date = $"{dateParts[2]} {dateParts[3]}",
                    KpIndex = Math.Round(avgKp, 1),
                    Probability = prob,
                    ActivityLevel = GetActivityLevel(avgKp),
                    IconEmoji = GetIconEmoji(avgKp)
                });
            }

            if (day3Values.Any())
            {
                var avgKp = day3Values.Average();
                var prob = CalculateProbability(avgKp, latitude);
                System.Diagnostics.Debug.WriteLine($"=== Day 3: Kp={avgKp:F1}, Prob={prob}% ===");

                forecasts.Add(new ForecastDay
                {
                    Date = $"{dateParts[4]} {dateParts[5]}",
                    KpIndex = Math.Round(avgKp, 1),
                    Probability = prob,
                    ActivityLevel = GetActivityLevel(avgKp),
                    IconEmoji = GetIconEmoji(avgKp)
                });
            }

            System.Diagnostics.Debug.WriteLine($"=== Returning {forecasts.Count} forecasts ===");
            return forecasts.Count == 3 ? forecasts : GetFallbackForecast(latitude);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"=== EXCEPTION: {ex.Message} ===");
            System.Diagnostics.Debug.WriteLine($"=== STACK: {ex.StackTrace} ===");
            return GetFallbackForecast(latitude);
        }
    }

    private ObservableCollection<ForecastDay> GetFallbackForecast(double latitude)
    {
        var forecasts = new ObservableCollection<ForecastDay>();
        var today = DateTime.UtcNow.Date;
        for (int i = 0; i < 3; i++)
        {
            forecasts.Add(new ForecastDay
            {
                Date = today.AddDays(i).ToString("ddd dd MMM"),
                KpIndex = 0,
                Probability = 0,
                ActivityLevel = "Low",
                IconEmoji = "🌙"
            });
        }
        return forecasts;
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
            >= 5 => "\U0001F7E2", // Green for high (codes for colored circle emojis)
            >= 3 => "\U0001F7E1", //yellow for medium
            _ => "\U0001F534"  // red for high
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
using AuroraFix.Models;
using System.Globalization;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace AuroraFix.Services;

public class GeocodingService
{
    private readonly HttpClient _httpClient;

    private static readonly IReadOnlyList<SelectedLocation> PredefinedLocations = new[]
    {
        new SelectedLocation { CityName = "Östersund",  Latitude = 63.8267, Longitude = 16.0534 },
        new SelectedLocation { CityName = "Kiruna",     Latitude = 67.8558, Longitude = 20.2253 },
        new SelectedLocation { CityName = "Tromsø",     Latitude = 69.6492, Longitude = 18.9553 },
        new SelectedLocation { CityName = "Reykjavik",  Latitude = 64.1466, Longitude = -21.9426 },
        new SelectedLocation { CityName = "Stockholm",  Latitude = 59.3293, Longitude = 18.0686 },
        new SelectedLocation { CityName = "Oslo",       Latitude = 59.9139, Longitude = 10.7522 },
        new SelectedLocation { CityName = "Göteborg",   Latitude = 57.7089, Longitude = 11.9746 },
        new SelectedLocation { CityName = "Malmö",      Latitude = 55.6050, Longitude = 13.0038 }
    };

    public GeocodingService()
    {
        _httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "AuroraFixApp/1.0");
    }

    /// <summary>
    /// Resolves a city name to coordinates. Checks the predefined Nordic list first,
    /// then falls back to OpenStreetMap Nominatim. Returns null if not found.
    /// </summary>
    public async Task<SelectedLocation?> GetLocationFromCityAsync(string cityName)
    {
        if (string.IsNullOrWhiteSpace(cityName))
            return null;

        var sanitized = SanitizeCityName(cityName);
        if (string.IsNullOrWhiteSpace(sanitized))
        {
            System.Diagnostics.Debug.WriteLine("GeocodingService: Input rejected after sanitization.");
            return null;
        }

        var predefined = PredefinedLocations
            .FirstOrDefault(l => l.CityName.Equals(sanitized, StringComparison.InvariantCultureIgnoreCase));
        if (predefined != null)
            return predefined;

        try
        {
            var url = $"https://nominatim.openstreetmap.org/search?q={Uri.EscapeDataString(sanitized)}&format=json&limit=1";
            var response = await _httpClient.GetFromJsonAsync<List<NominatimResult>>(url);

            if (response == null || response.Count == 0)
                return null;

            var result = response[0];
            // Nominatim returns lat/lon as JSON strings, not numbers — parse explicitly.
            if (!double.TryParse(result.Lat, NumberStyles.Float, CultureInfo.InvariantCulture, out var lat) ||
                !double.TryParse(result.Lon, NumberStyles.Float, CultureInfo.InvariantCulture, out var lon))
            {
                System.Diagnostics.Debug.WriteLine($"GeocodingService: Failed to parse coordinates for '{cityName}'");
                return null;
            }

            return new SelectedLocation { CityName = sanitized, Latitude = lat, Longitude = lon };
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"GeocodingService: Nominatim request failed for '{cityName}': {ex.Message}");
            return null;
        }
    }

    public IReadOnlyList<SelectedLocation> GetPopularNordicLocations() => PredefinedLocations;

    /// <summary>
    /// Sanitizes city name input before any list lookup or network call.
    /// Allows Unicode letters and digits (for Ö, Ø, Å, etc.), spaces,
    /// hyphens, apostrophes, dots and commas — all legitimate in place names.
    /// Strips control characters, null bytes, and anything outside that set.
    /// Caps length at 100 characters to prevent abuse.
    /// </summary>
    private static string SanitizeCityName(string input)
    {
        const int maxLength = 100;

        // Keep only characters that can appear in a real place name.
        // char.IsLetter covers full Unicode, so Ä, Ø, é etc. all pass.
        var cleaned = new string(input
            .Where(c => char.IsLetter(c) || char.IsDigit(c) ||
                        c == ' ' || c == '-' || c == '\'' || c == '.' || c == ',')
            .ToArray());

        // Collapse multiple spaces and trim edges
        cleaned = System.Text.RegularExpressions.Regex.Replace(cleaned.Trim(), @"\s+", " ");

        return cleaned.Length > maxLength ? cleaned[..maxLength] : cleaned;
    }

    private class NominatimResult
    {
        [JsonPropertyName("lat")] public string Lat { get; set; } = string.Empty;
        [JsonPropertyName("lon")] public string Lon { get; set; } = string.Empty;
        [JsonPropertyName("display_name")] public string DisplayName { get; set; } = string.Empty;
    }
}

using System.Text.Json;
using AuroraFix.Models;

namespace AuroraFix.Services;

public class IpGeolocationService
{
    private readonly HttpClient _httpClient;
    private const string IpApiUrl = "https://ipapi.co/json/";
    private const int TimeoutSeconds = 8;
    private const string PropCity = "city";
    private const string PropLat = "latitude";
    private const string PropLon = "longitude";
    private const string PropCountry = "country_name";
    private const string PropRegion = "region";
    private const string PropError = "error";

    public IpGeolocationService()
    {
        _httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };
    }

    public async Task<IpGeolocationResult?> GetLocationFromIpAsync()
    {
        try
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(TimeoutSeconds));
            var response = await _httpClient.GetStringAsync(IpApiUrl, cts.Token);
            using var doc = JsonDocument.Parse(response);
            var root = doc.RootElement;

            if (root.TryGetProperty(PropError, out var error) && error.ValueKind == JsonValueKind.True)
            {
                System.Diagnostics.Debug.WriteLine("[IpGeolocation] API returned error response");
                return null;
            }

            var city = root.TryGetProperty(PropCity, out var cityProp) ? cityProp.GetString() : null;
            if (!string.IsNullOrWhiteSpace(city))
            {
                return new IpGeolocationResult
                {
                    City      = city,
                    Latitude  = root.TryGetProperty(PropLat,     out var lat)     ? lat.GetDouble()     : 0,
                    Longitude = root.TryGetProperty(PropLon,     out var lon)     ? lon.GetDouble()     : 0,
                    Country   = root.TryGetProperty(PropCountry, out var country) ? country.GetString() ?? string.Empty : string.Empty,
                    Region    = root.TryGetProperty(PropRegion,  out var region)  ? region.GetString()  ?? string.Empty : string.Empty,
                };
            }

            System.Diagnostics.Debug.WriteLine("[IpGeolocation] API returned empty city");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[IpGeolocation] Failed: {ex.Message}");
        }
        return null;
    }
}

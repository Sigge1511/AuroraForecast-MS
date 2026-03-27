using System.Text.Json;
using AuroraFix.Models;

namespace AuroraFix.Services;

public class IpGeolocationService
{
    private readonly HttpClient _httpClient;
    private const string IpApiUrl = "https://ip-api.com/json/";
    private const string StatusSuccess = "success";
    private const int TimeoutSeconds = 3;
    private const string PropStatus = "status";
    private const string PropCity = "city";
    private const string PropLat = "lat";
    private const string PropLon = "lon";
    private const string PropCountry = "country";
    private const string PropRegion = "regionName";

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

            if (root.TryGetProperty(PropStatus, out var status) && status.GetString() == StatusSuccess)
            {
                return new IpGeolocationResult
                {
                    City      = root.TryGetProperty(PropCity,    out var city)    ? city.GetString()    ?? string.Empty : string.Empty,
                    Latitude  = root.TryGetProperty(PropLat,     out var lat)     ? lat.GetDouble()     : 0,
                    Longitude = root.TryGetProperty(PropLon,     out var lon)     ? lon.GetDouble()     : 0,
                    Country   = root.TryGetProperty(PropCountry, out var country) ? country.GetString() ?? string.Empty : string.Empty,
                    Region    = root.TryGetProperty(PropRegion,  out var region)  ? region.GetString()  ?? string.Empty : string.Empty,
                };
            }

            System.Diagnostics.Debug.WriteLine("[IpGeolocation] API returned non-success status");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[IpGeolocation] Failed: {ex.Message}");
        }
        return null;
    }
}

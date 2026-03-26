using System.Text.Json;
using AuroraFix.Models;

namespace AuroraFix.Services;

public class IpGeolocationService
{
    private readonly HttpClient _httpClient;
    private const string IpApiUrl = "https://ip-api.com/json/";
    private const string StatusSuccess = "success";
    private const int TimeoutSeconds = 3;

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

            if (root.TryGetProperty("status", out var status) && status.GetString() == StatusSuccess)
            {
                return new IpGeolocationResult
                {
                    City    = root.TryGetProperty("city",       out var city)   ? city.GetString()   ?? string.Empty : string.Empty,
                    Latitude  = root.TryGetProperty("lat",      out var lat)    ? lat.GetDouble()    : 0,
                    Longitude = root.TryGetProperty("lon",      out var lon)    ? lon.GetDouble()    : 0,
                    Country = root.TryGetProperty("country",    out var country)? country.GetString() ?? string.Empty : string.Empty,
                    Region  = root.TryGetProperty("regionName", out var region) ? region.GetString() ?? string.Empty : string.Empty,
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

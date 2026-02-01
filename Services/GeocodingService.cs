using AuroraForecast.Models;
using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace AuroraForecast.Services;

public class GeocodingService
{
    private readonly HttpClient _httpClient;

    public GeocodingService()
    {
        _httpClient = new HttpClient();
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "AuroraForecastApp/1.0");
    }

    public async Task<SelectedLocation?> GetLocationFromCityAsync(string cityName)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(cityName))
                return null;

            // look for popular preentered cities
            var predefined = GetPopularNordicLocations()
                .FirstOrDefault(l => l.CityName.Equals(cityName, StringComparison.OrdinalIgnoreCase));

            if (predefined != null)
                return predefined;

            // use OpenStreetMap Nominatim (free)
            // to find other locations
            var url = $"https://nominatim.openstreetmap.org/search?q={Uri.EscapeDataString(cityName)}&format=json&limit=1";

            var response = await _httpClient.GetFromJsonAsync<List<NominatimResult>>(url);

            if (response != null && response.Count > 0)
            {
                var result = response[0];
                return new SelectedLocation
                {
                    CityName = cityName,
                    Latitude = result.Lat,
                    Longitude = result.Lon
                };
            }
            Console.WriteLine("error finding town");
            return null;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Geocoding error: {ex.Message}");

            // Fallback - Östersund
            return new SelectedLocation
            {
                CityName = cityName,
                Latitude = 63.8267,
                Longitude = 16.0534
            };
        }
    }

    public List<SelectedLocation> GetPopularNordicLocations()
    {
        return new List<SelectedLocation>
        {
            new SelectedLocation { CityName = "Östersund", Latitude = 63.8267, Longitude = 16.0534 },
            new SelectedLocation { CityName = "Kiruna", Latitude = 67.8558, Longitude = 20.2253 },
            new SelectedLocation { CityName = "Tromsø", Latitude = 69.6492, Longitude = 18.9553 },
            new SelectedLocation { CityName = "Reykjavik", Latitude = 64.1466, Longitude = -21.9426 },
            new SelectedLocation { CityName = "Stockholm", Latitude = 59.3293, Longitude = 18.0686 },
            new SelectedLocation { CityName = "Oslo", Latitude = 59.9139, Longitude = 10.7522 },
            new SelectedLocation { CityName = "Göteborg", Latitude = 57.7089, Longitude = 11.9746 },
            new SelectedLocation { CityName = "Malmö", Latitude = 55.6050, Longitude = 13.0038 }
        };
    }

    // to help with JSON deserialization
    private class NominatimResult
    {
        [JsonPropertyName("lat")]
        public double Lat { get; set; }

        [JsonPropertyName("lon")]
        public double Lon { get; set; }

        [JsonPropertyName("display_name")]
        public string DisplayName { get; set; } = string.Empty;
    }
}
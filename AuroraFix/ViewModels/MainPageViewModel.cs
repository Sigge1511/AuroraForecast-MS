using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AuroraFix.Models;
using AuroraFix.Services;
using AuroraFix.Helpers;
using Microsoft.Maui.Controls.Shapes;

namespace AuroraFix.ViewModels;

public partial class MainPageViewModel : BaseViewModel
{
    private readonly AuroraService _auroraService;
    private readonly GeocodingService _geocodingService;
    private readonly ProbabilityDisplayHelper _helper;

    // ========================================
    // OBSERVABLE PROPERTIES
    // ========================================

    [ObservableProperty] private string cityName = string.Empty;
    [ObservableProperty] private double currentKpIndex;
    [ObservableProperty] private string activityLevel = string.Empty;
    [ObservableProperty] private string activityDescription = string.Empty;

    // Weather properties
    [ObservableProperty] private double cloudCoverage;
    [ObservableProperty] private string cloudCondition = string.Empty;

    // Aurora probability
    [ObservableProperty] private double probability;
    [ObservableProperty] private string locationInfo = string.Empty;
    [ObservableProperty] private bool isDataLoaded;
    [ObservableProperty] private ObservableCollection<ForecastDay> threeDayForecast;
    [ObservableProperty] private DoubleCollection strokeDashValues; // For circular probability display

    [RelayCommand]
    private async Task RefreshAsync() => await SearchCityAsync();

    // ========================================
    // CONSTRUCTOR
    // ========================================

    public MainPageViewModel()
    {
        _auroraService = new AuroraService();
        _geocodingService = new GeocodingService();
        _helper = new ProbabilityDisplayHelper();

        Title = "Aurora Forecast";
        ThreeDayForecast = new ObservableCollection<ForecastDay>();

        _ = LoadDefaultLocationAsync();
    }

    // ========================================
    // SEARCH CITY COMMAND - MAIN FLOW
    // ========================================

    [RelayCommand]
    private async Task SearchCityAsync()
    {
        if (string.IsNullOrWhiteSpace(CityName))
        {
            SetError("Please enter a city name");
            return;
        }
        if (IsBusy) return;

        try
        {
            IsBusy = true;
            ClearError();
            IsDataLoaded = false;

            // 1. Get location coordinates from city name
            var location = await FetchLocationAsync(CityName);
            if (location == null) return;

            // 2. Update UI display for location
            UpdateLocationDisplay(location);

            // 3. Update current aurora data AND weather (THIS IS THE KEY!)
            await UpdateCurrentAuroraAndWeatherAsync(location);

            // 4. Update 3-day forecast with weather
            await UpdateThreeDayForecastWithWeatherAsync(location.Latitude, location.Longitude);

            IsDataLoaded = true;
        }
        catch (Exception ex)
        {
            SetError($"Error fetching data: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"Error: {ex}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    // ========================================
    // HELPER METHODS
    // ========================================

    private async Task<SelectedLocation> FetchLocationAsync(string city)
    {
        var location = await _geocodingService.GetLocationFromCityAsync(city);
        if (location == null)
        {
            SetError($"Could not find city: {city}");
        }
        return location;
    }

    private void UpdateLocationDisplay(SelectedLocation location)
    {
        LocationInfo = $"{location.CityName} ({location.Latitude:F2}°, {location.Longitude:F2}°)";
    }

    /// <summary>
    /// Updates current aurora forecast AND weather conditions
    /// This is where we combine Kp-index with cloud coverage!
    /// </summary>
    private async Task UpdateCurrentAuroraAndWeatherAsync(SelectedLocation location)
    {       
        var forecast = await _auroraService.GetForecastForLocationAsync(
            location.CityName,
            location.Latitude,
            location.Longitude);
        CurrentKpIndex = forecast.KpIndex;

        var weather = await WeatherService.Instance.GetCurrentWeatherAsync(
            location.Latitude,
            location.Longitude);
        var cloudCoverage = weather?.CloudCoverage ?? 0;
               
        Probability = _helper.CalculateAuroraProbability(
            CurrentKpIndex,         
            location.Latitude,   
            cloudCoverage         
        );

        CloudCoverage = cloudCoverage;
        CloudCondition = _helper.GetCloudImpactLabel(cloudCoverage);

        // Time-of-day darkness awareness
        bool isDark = !(weather?.IsDay ?? false);
        bool isMidnightSun = IsMidnightSun(weather?.Sunrise, weather?.Sunset, location.Latitude);
        DateTime? darkFrom = (!isDark && !isMidnightSun) ? weather?.Sunset : null;

        // Update aurora UI properties
        var baseProbability = _helper.CalculateAuroraProbability(CurrentKpIndex, location.Latitude, 0);
        ActivityLevel = _helper.GetActivityLevelText(Probability);
        ActivityDescription = forecast.GetActivityDescription(Probability, CurrentKpIndex, cloudCoverage, baseProbability, isDark, darkFrom, isMidnightSun);
        StrokeDashValues = _helper.UpdateCircle(Probability);

        // Debug logging
        System.Diagnostics.Debug.WriteLine($"=== CURRENT FORECAST ===");
        System.Diagnostics.Debug.WriteLine($"Kp: {CurrentKpIndex}, Lat: {location.Latitude:F1}");
        System.Diagnostics.Debug.WriteLine($"Clouds: {cloudCoverage:F0}%");
        System.Diagnostics.Debug.WriteLine($"Base Prob: {_helper.CalculateAuroraProbability(CurrentKpIndex, location.Latitude, 0):F0}%");
        System.Diagnostics.Debug.WriteLine($"Actual Prob (with clouds): {Probability:F0}%");
    }

    private async Task UpdateThreeDayForecastWithWeatherAsync(double latitude, double longitude)
    {
        var auroraForecasts = await _auroraService.GetThreeDayForecastAsync(latitude);
        var weatherForecasts = await WeatherService.Instance.GetThreeDayForecastAsync(latitude, longitude);

        ThreeDayForecast.Clear();

        foreach (var day in auroraForecasts)
        {
            // Calculate BASE probability (without clouds)
            var baseProbability = _helper.CalculateAuroraProbability(day.KpIndex, latitude, 0);

            // Match weather to this specific aurora forecast day by calendar date
            var weather = weatherForecasts.FirstOrDefault(w => w.ForecastTime.Date == day.ForecastDate.Date);

            if (weather != null)
            {
                var actualProbability = _helper.AdjustForCloudCoverage(baseProbability, weather.CloudCoverage);

                day.Probability = actualProbability;
                day.CloudCoverage = weather.CloudCoverage;
                day.ActualProbability = (int)actualProbability;
                day.IconEmoji = AuroraService.GetIconEmoji(actualProbability);
                day.Sunrise = weather.Sunrise;
                day.Sunset = weather.Sunset;
                day.DarknessWindow = GetDarknessWindowText(weather.Sunrise, weather.Sunset, latitude);

                System.Diagnostics.Debug.WriteLine(
                    $"Day {day.ForecastDate:yyyy-MM-dd}: KP={day.KpIndex:F1}, " +
                    $"Base={baseProbability:F0}%, Clouds={weather.CloudCoverage:F0}%, Actual={actualProbability:F0}%, Dark={day.DarknessWindow}");
            }
            else
            {
                // No matching weather day — show base probability and log the gap
                day.Probability = baseProbability;
                day.ActualProbability = (int)baseProbability;
                day.IconEmoji = AuroraService.GetIconEmoji(baseProbability);

                System.Diagnostics.Debug.WriteLine(
                    $"Day {day.ForecastDate:yyyy-MM-dd}: KP={day.KpIndex:F1}, " +
                    $"Base={baseProbability:F0}% — no weather match (available: {string.Join(", ", weatherForecasts.Select(w => w.ForecastTime.Date.ToString("yyyy-MM-dd")))})");
            }

            ThreeDayForecast.Add(day);
        }
    }

    private async Task LoadDefaultLocationAsync()
    {
        CityName = "Östersund";
        await SearchCityAsync();
    }

    private static string GetDarknessWindowText(DateTime? sunrise, DateTime? sunset, double latitude)
    {
        if (sunrise == null || sunset == null)
            return Math.Abs(latitude) > 60 ? "Darkness unknown at this latitude" : string.Empty;

        var darkHours = (sunrise.Value - sunset.Value).TotalHours;
        if (darkHours < 0) darkHours += 24;

        if (darkHours < 2)
            return "Midnight sun — no darkness";

        return $"Dark {sunset.Value:HH:mm} – {sunrise.Value:HH:mm}";
    }

    private static bool IsMidnightSun(DateTime? sunrise, DateTime? sunset, double latitude)
    {
        if (sunrise == null || sunset == null)
            return Math.Abs(latitude) > 65;

        var darkHours = (sunrise.Value - sunset.Value).TotalHours;
        if (darkHours < 0) darkHours += 24;
        return darkHours < 2;
    }
}
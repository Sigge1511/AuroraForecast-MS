using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AuroraFix.Models;
using AuroraFix.Services;
using AuroraFix.Helpers;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Devices.Sensors;

namespace AuroraFix.ViewModels;

public partial class MainPageViewModel : BaseViewModel
{
    private readonly AuroraService _auroraService;
    private readonly GeocodingService _geocodingService;
    private readonly WeatherService _weatherService;
    private readonly IpGeolocationService _ipGeolocationService;

    [ObservableProperty] private string cityName = string.Empty;
    [ObservableProperty] private double currentKpIndex;
    [ObservableProperty] private string activityLevel = string.Empty;
    [ObservableProperty] private string activityDescription = string.Empty;
    [ObservableProperty] private double cloudCoverage;
    [ObservableProperty] private string cloudCondition = string.Empty;
    [ObservableProperty] private double probability;
    [ObservableProperty] private string locationInfo = string.Empty;
    [ObservableProperty] private bool isDataLoaded;
    [ObservableProperty] private ObservableCollection<ForecastDay> threeDayForecast = [];
    [ObservableProperty] private DoubleCollection strokeDashValues = [];

    [RelayCommand]
    private async Task RefreshAsync() => await SearchCityAsync();

    public MainPageViewModel(
        AuroraService auroraService,
        GeocodingService geocodingService,
        WeatherService weatherService,
        IpGeolocationService ipGeolocationService)
    {
        _auroraService = auroraService;
        _geocodingService = geocodingService;
        _weatherService = weatherService;
        _ipGeolocationService = ipGeolocationService;

        Title = "Aurora Forecast";
        ThreeDayForecast = new ObservableCollection<ForecastDay>();
        ActivityDescription = "Enter a location above to check aurora conditions.";
        StrokeDashValues = ProbabilityDisplayHelper.UpdateCircle(0);
        IsDataLoaded = true;
    }

    // Called from MainPage.OnAppearing (main thread) — do NOT wrap in Task.Run,
    // as Permissions.RequestAsync must be called from the main thread on Android.
    public async Task InitializeAsync()
    {
        try
        {
            string? city = await TryGetCityFromGpsAsync();
            city ??= await TryGetCityFromIpAsync();

            if (!string.IsNullOrWhiteSpace(city) && string.IsNullOrWhiteSpace(CityName))
            {
                CityName = city;
                await SearchCityAsync();
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[LocationDetection] Startup lookup failed: {ex.Message}");
        }
    }

    private static async Task<string?> TryGetCityFromGpsAsync()
    {
        try
        {
            var status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
            if (status != PermissionStatus.Granted)
                return null;

            var cachedLocation = await Geolocation.GetLastKnownLocationAsync();
            var gpsLocation = cachedLocation
                ?? await Geolocation.GetLocationAsync(new GeolocationRequest(GeolocationAccuracy.Low, TimeSpan.FromSeconds(8)));

            if (gpsLocation == null) return null;

            var placemarks = await Geocoding.GetPlacemarksAsync(gpsLocation.Latitude, gpsLocation.Longitude);
            var city = placemarks?.FirstOrDefault()?.Locality;
            System.Diagnostics.Debug.WriteLine($"[LocationDetection] GPS city: {city}");
            return string.IsNullOrWhiteSpace(city) ? null : city;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[LocationDetection] GPS failed: {ex.Message}");
            return null;
        }
    }

    private async Task<string?> TryGetCityFromIpAsync()
    {
        try
        {
            var result = await _ipGeolocationService.GetLocationFromIpAsync();
            System.Diagnostics.Debug.WriteLine($"[LocationDetection] IP city: {result?.City}");
            return string.IsNullOrWhiteSpace(result?.City) ? null : result.City;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[LocationDetection] IP fallback failed: {ex.Message}");
            return null;
        }
    }

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

            var location = await FetchLocationAsync(CityName);
            if (location == null) return;

            UpdateLocationDisplay(location);

            // Fire all 4 HTTP calls simultaneously — none depends on another at issue time.
            var forecastTask = _auroraService.GetForecastForLocationAsync(location.CityName, location.Latitude, location.Longitude);
            var weatherTask  = _weatherService.GetCurrentWeatherAsync(location.Latitude, location.Longitude);
            var threeDayTask = _auroraService.GetThreeDayForecastAsync(location.Latitude);
            var fourDayTask  = _weatherService.GetFourDayForecastAsync(location.Latitude, location.Longitude);

            await Task.WhenAll(forecastTask, weatherTask, threeDayTask, fourDayTask);

            ApplyCurrentAuroraAndWeather(location, forecastTask.Result, weatherTask.Result);
            ApplyThreeDayForecastWithWeather(location.Latitude, threeDayTask.Result, fourDayTask.Result);

            IsDataLoaded = true;
        }
        catch (Exception ex)
        {
            SetError($"Error fetching data: {ex.Message}");
            IsDataLoaded = true;
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task<SelectedLocation?> FetchLocationAsync(string city)
    {
        var location = await _geocodingService.GetLocationFromCityAsync(city);
        if (location == null)
            SetError($"Could not find city: {city}");
        return location;
    }

    private void UpdateLocationDisplay(SelectedLocation location)
    {
        LocationInfo = $"{location.CityName} ({location.Latitude:F2}°, {location.Longitude:F2}°)";
    }

    private void ApplyCurrentAuroraAndWeather(SelectedLocation location, AuroraForecast forecast, Weather? weather)
    {
        CurrentKpIndex = forecast.KpIndex;
        var clouds = weather?.CloudCoverage ?? 0;
        var baseProbability = ProbabilityDisplayHelper.CalculateAuroraProbability(CurrentKpIndex, location.Latitude);
        Probability = ProbabilityDisplayHelper.AdjustForCloudCoverage(baseProbability, clouds);
        CloudCoverage = clouds;
        CloudCondition = ProbabilityDisplayHelper.GetCloudImpactLabel(clouds);
        ActivityLevel = ProbabilityDisplayHelper.GetActivityLevelText(Probability);
        StrokeDashValues = ProbabilityDisplayHelper.UpdateCircle(Probability);
        bool isDark = !(weather?.IsDay ?? false);
        bool isMidnightSun = GuiMessageHelper.IsMidnightSun(weather?.Sunrise, weather?.Sunset, location.Latitude);
        DateTime? darkFrom = (!isDark && !isMidnightSun) ? weather?.Sunset : null;
        ActivityDescription = GuiMessageHelper.GetActivityDescription(
            Probability, CurrentKpIndex, clouds, baseProbability, isDark, darkFrom, isMidnightSun);
    }

    private void ApplyThreeDayForecastWithWeather(double latitude, IReadOnlyList<ForecastDay> auroraForecasts, List<Weather> weatherForecasts)
    {
        var newItems = new List<ForecastDay>(auroraForecasts.Count);
        foreach (var day in auroraForecasts)
        {
            var baseProbability = ProbabilityDisplayHelper.CalculateAuroraProbability(day.KpIndex, latitude);
            var weather = weatherForecasts.FirstOrDefault(w => w.ForecastTime.Date == day.ForecastDate.Date);

            if (weather != null)
            {
                day.Probability = ProbabilityDisplayHelper.AdjustForCloudCoverage(baseProbability, weather.CloudCoverage);
                day.CloudCoverage = weather.CloudCoverage;
                day.ActualProbability = (int)day.Probability;
                day.IconEmoji = ProbabilityDisplayHelper.GetIconEmoji(day.Probability);
                day.Sunrise = weather.Sunrise;
                day.Sunset = weather.Sunset;
                day.DarknessWindow = GuiMessageHelper.GetDarknessWindowText(weather.Sunrise, weather.Sunset, latitude);
            }
            else
            {
                day.Probability = baseProbability;
                day.ActualProbability = (int)baseProbability;
                day.IconEmoji = ProbabilityDisplayHelper.GetIconEmoji(baseProbability);
            }

            newItems.Add(day);
        }
        ThreeDayForecast = new ObservableCollection<ForecastDay>(newItems);
    }
}

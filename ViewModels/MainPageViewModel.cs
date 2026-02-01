using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AuroraForecast.Models;
using AuroraForecast.Services;
using AuroraForecast.Helpers;
using Microsoft.Maui.Controls.Shapes;

namespace AuroraForecast.ViewModels;

public partial class MainPageViewModel : BaseViewModel
{
    private readonly AuroraService _auroraService;
    private readonly GeocodingService _geocodingService;
    private readonly ProbabilityDisplayHelper _helper;

    [ObservableProperty] private string cityName = string.Empty;
    [ObservableProperty] private double currentKpIndex;
    [ObservableProperty] private string activityLevel = string.Empty;
    [ObservableProperty] private string activityDescription = string.Empty;

    [ObservableProperty]
    private DoubleCollection strokeDashValues;
    [ObservableProperty] private double probability;

    //[ObservableProperty] private string currentVideoSource = string.Empty;
    [ObservableProperty] private string locationInfo = string.Empty;
    [ObservableProperty] private bool isDataLoaded;
    [ObservableProperty] private ObservableCollection<ForecastDay> threeDayForecast;
    
    [RelayCommand]
    private async Task RefreshAsync() => await SearchCityAsync();

    public MainPageViewModel()
    {
        _auroraService = new AuroraService();
        _geocodingService = new GeocodingService();
        _helper = new ProbabilityDisplayHelper();

        Title = "Aurora Forecast";
        ThreeDayForecast = new ObservableCollection<ForecastDay>();
        //CurrentVideoSource = "aurora_low.mp4";

        _ = LoadDefaultLocationAsync();
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

            // 2. Update UI display for location
            UpdateLocationDisplay(location);

            // Update current aurora data and probability
            await UpdateCurrentWeatherAsync(location);

            // Update forecast list
            await UpdateForecastAsync(location.Latitude);

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

    private async Task UpdateCurrentWeatherAsync(SelectedLocation location)
    {
        var forecast = await _auroraService.GetForecastForLocationAsync(
            location.CityName, location.Latitude, location.Longitude);

        CurrentKpIndex = forecast.KpIndex;
        ActivityLevel = forecast.ActivityLevel;
        Probability = _helper.CalculateAuroraProbability(CurrentKpIndex, location.Latitude);
        ActivityDescription = forecast.GetActivityDescription(Probability); //FOR THE DESCRIPTION BELOW

        StrokeDashValues = _helper.UpdateCircle(Probability); //FILLS THE CIRCLE IN %

    }

    private async Task UpdateForecastAsync(double latitude)
    {
        var forecastDays = await _auroraService.GetThreeDayForecastAsync(latitude);

        ThreeDayForecast.Clear();
        foreach (var day in forecastDays)
        {
            // You can also calculate probability per forecast day here if needed
            ThreeDayForecast.Add(day);
        }
    }
            
    private async Task LoadDefaultLocationAsync()
    {
        CityName = "Östersund";
        await SearchCityAsync();
    }
}



using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AuroraForecast.Models;
using AuroraForecast.Services;

namespace AuroraForecast.ViewModels;

public partial class MainPageViewModel : BaseViewModel
{
    private readonly AuroraService _auroraService;
    private readonly GeocodingService _geocodingService;
    private readonly VideoService _videoService;

    [ObservableProperty]
    private string cityName = string.Empty;

    [ObservableProperty]
    private double currentKpIndex;

    [ObservableProperty]
    private string activityLevel = string.Empty;

    [ObservableProperty]
    private string activityDescription = string.Empty;

    [ObservableProperty]
    private int probability;

    [ObservableProperty]
    private string currentVideoSource = string.Empty;

    [ObservableProperty]
    private string locationInfo = string.Empty;

    [ObservableProperty]
    private ObservableCollection<ForecastDay> forecastItems;

    [ObservableProperty]
    private bool isDataLoaded;

    public MainPageViewModel()
    {
        _auroraService = new AuroraService();
        _geocodingService = new GeocodingService();
        _videoService = new VideoService();
        
        Title = "Aurora Forecast";
        ForecastItems = new ObservableCollection<ForecastDay>();
        
        CurrentVideoSource = "aurora_low.mp4";
        
        _ = LoadDefaultLocationAsync();
    }

    private async Task LoadDefaultLocationAsync()
    {
        CityName = "Uppsala";
        await SearchCityAsync();
    }

    [RelayCommand]
    private async Task SearchCityAsync()
    {
        if (string.IsNullOrWhiteSpace(CityName))
        {
            SetError("Ange ett stadsnamn");
            return;
        }

        if (IsBusy)
            return;

        try
        {
            IsBusy = true;
            ClearError();
            IsDataLoaded = false;

            var location = await _geocodingService.GetLocationFromCityAsync(CityName);
            
            if (location == null)
            {
                SetError($"Kunde inte hitta stad: {CityName}");
                return;
            }

            LocationInfo = $"{location.CityName} ({location.Latitude:F2}°, {location.Longitude:F2}°)";

            var forecast = await _auroraService.GetForecastForLocationAsync(
                location.CityName,
                location.Latitude,
                location.Longitude
            );

            CurrentKpIndex = forecast.KpIndex;
            ActivityLevel = forecast.ActivityLevel;
            ActivityDescription = forecast.GetActivityDescription();
            Probability = forecast.Probability;

            CurrentVideoSource = _videoService.GetVideoSourceUri(forecast.KpIndex);

            var threeDayForecast = await _auroraService.GetThreeDayForecastAsync();
            
            ForecastItems.Clear();
            foreach (var day in threeDayForecast)
            {
                ForecastItems.Add(day);
            }

            IsDataLoaded = true;
        }
        catch (Exception ex)
        {
            SetError($"Fel vid hämtning av data: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"Error: {ex}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
        await SearchCityAsync();
    }
}

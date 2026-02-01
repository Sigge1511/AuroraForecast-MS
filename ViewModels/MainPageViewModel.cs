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
    private readonly VideoService _videoService;
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
        _videoService = new VideoService();
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

            // 1. Get coordinates for the city
            var location = await FetchLocationAsync(CityName);
            if (location == null) return;

            // 2. Update UI display for location
            UpdateLocationDisplay(location);

            // 3. Update current aurora data and probability
            await UpdateCurrentWeatherAsync(location);

            // 4. Update the 3-day forecast list
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
        ActivityDescription = forecast.GetActivityDescription();

        Probability = _helper.CalculateAuroraProbability(CurrentKpIndex, location.Latitude);
        UpdateCircle(Probability);

        //CurrentVideoSource = _videoService.GetVideoSourceUri(CurrentKpIndex);
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

    private void UpdateCircle(double prob)
    {        
        double totalUnits = 816.0 / 12.0;
        double filledUnits = (prob / 100.0) * totalUnits;

        StrokeDashValues = new DoubleCollection { filledUnits, 100 };
    }

    //private void UpdateCircleDashArray(double prob)
    //{
    //    // Math: Circumference (816.8) / Thickness (12) = ~68 units total
    //    double totalUnits = 68.07;
    //    double filledUnits = (prob / 100.0) * totalUnits;

    //    // Direct update to the property the View binds to
    //    StrokeDashValues = new DoubleCollection { filledUnits, 100 };
    //}

    
    
    private async Task LoadDefaultLocationAsync()
    {
        CityName = "Östersund";
        await SearchCityAsync();
    }
}


//using System.Collections.ObjectModel;
//using CommunityToolkit.Mvvm.ComponentModel;
//using CommunityToolkit.Mvvm.Input;
//using AuroraForecast.Models;
//using AuroraForecast.Services;
//using AuroraForecast.Helpers;

//namespace AuroraForecast.ViewModels;

//public partial class MainPageViewModel : BaseViewModel
//{
//    private readonly AuroraService _auroraService;
//    private readonly GeocodingService _geocodingService;
//    private readonly VideoService _videoService;
//    private readonly ProbabilityDisplayHelper _helper;


//    [ObservableProperty]
//    private string cityName = string.Empty;

//    [ObservableProperty]
//    private double currentKpIndex;

//    [ObservableProperty]
//    private string activityLevel = string.Empty;

//    [ObservableProperty]
//    private string activityDescription = string.Empty;

//    [ObservableProperty]
//    private double _probability;


//    [ObservableProperty]
//    private string currentVideoSource = string.Empty;

//    [ObservableProperty]
//    private string locationInfo = string.Empty;

//    [ObservableProperty]
//    private bool isDataLoaded;

//    [ObservableProperty]
//    private ObservableCollection<ForecastDay> threeDayForecast; 

//    public MainPageViewModel()
//    {
//        _auroraService = new AuroraService();
//        _geocodingService = new GeocodingService();
//        _videoService = new VideoService();
//        _helper = new ProbabilityDisplayHelper();

//        Title = "Aurora Forecast";
//        ThreeDayForecast = new ObservableCollection<ForecastDay>(); // Ändrat från ForecastItems

//        CurrentVideoSource = "aurora_low.mp4";

//        _ = LoadDefaultLocationAsync();
//    }

//    private async Task LoadDefaultLocationAsync()
//    {
//        CityName = "Östersund";
//        await SearchCityAsync();
//    }

//    [RelayCommand]
//    private async Task SearchCityAsync()
//    {
//        if (string.IsNullOrWhiteSpace(CityName))
//        {
//            SetError("Ange ett stadsnamn");
//            return;
//        }

//        if (IsBusy)
//            return;

//        try
//        {
//            IsBusy = true;
//            ClearError();
//            IsDataLoaded = false;

//            var location = await _geocodingService.GetLocationFromCityAsync(CityName);

//            if (location == null)
//            {
//                SetError($"Kunde inte hitta stad: {CityName}");
//                return;
//            }

//            LocationInfo = $"{location.CityName} ({location.Latitude:F2}°, {location.Longitude:F2}°)";

//            var forecast = await _auroraService.GetForecastForLocationAsync(
//                location.CityName,
//                location.Latitude,
//                location.Longitude
//            );

//            CurrentKpIndex = forecast.KpIndex;
//            ActivityLevel = forecast.ActivityLevel;
//            ActivityDescription = forecast.GetActivityDescription();
//            Probability = _helper.CalculateAuroraProbability(CurrentKpIndex, forecast.Latitude);
//            CurrentVideoSource = _videoService.GetVideoSourceUri(forecast.KpIndex);

//            // Hämta 3-dagars prognos
//            var forecastDays = await _auroraService.GetThreeDayForecastAsync(location.Latitude);

//            // Uppdatera ObservableCollection
//            ThreeDayForecast.Clear();
//            foreach (var day in forecastDays)
//            {
//                ThreeDayForecast.Add(day);
//            }

//            IsDataLoaded = true;
//        }
//        catch (Exception ex)
//        {
//            SetError($"Fel vid hämtning av data: {ex.Message}");
//            System.Diagnostics.Debug.WriteLine($"Error: {ex}");
//        }
//        finally
//        {
//            IsBusy = false;
//        }
//    }

//    [RelayCommand]
//    private async Task RefreshAsync()
//    {
//        await SearchCityAsync();
//    }
//}
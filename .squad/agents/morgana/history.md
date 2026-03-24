# Morgana — Lead Architect

## Project: AuroraFix (seeded 2026-03-21)

**What it is:** Aurora borealis forecast app for .NET MAUI (net10.0).
Targets: Android, iOS, macCatalyst, Windows.

**Stack:** C# · .NET MAUI · CommunityToolkit.Mvvm 8.4.0
**Solution:** AuroraForecast.slnx → AuroraFix/AuroraForecast.csproj
**Namespace:** AuroraFix

**Architecture:** Single-page MVVM app.
- `BaseViewModel : ObservableObject` (CommunityToolkit.Mvvm)
- `MainPageViewModel` uses `[ObservableProperty]` and `[RelayCommand]` source generators
- Services registered as singletons in `MauiProgram.cs`
- `WeatherService` uses a manually-implemented singleton (`Instance`) — note inconsistency with DI pattern

**Key files:**
- `Views/MainPage.xaml` — only page, all UI
- `ViewModels/MainPageViewModel.cs` — search flow, data orchestration
- `Helpers/ProbabilityDisplayHelper.cs` — probability formula + cloud penalty
- `Services/AuroraService.cs` — NOAA Kp-index + 3-day geomag forecast
- `Services/WeatherService.cs` — Open-Meteo cloud coverage (singleton)
- `Services/GeocodingService.cs` — OpenStreetMap Nominatim + Nordic city presets
- `Models/`: AuroraForecast, ForecastDay, SelectedLocation, Weather

**APIs (all free, no key):**
- NOAA SWPC — Kp-index + 3-day forecast text
- Open-Meteo — cloud coverage (current + 3-day daily)
- OpenStreetMap Nominatim — geocoding

**Known architectural issues:**
- `AuroraService.CalculateProbability` (private, simple switch) and `ProbabilityDisplayHelper.CalculateAuroraProbability` (latitude-diff formula) are different — ViewModel uses Helper, not service method
- `WeatherService` bypasses DI with manual singleton
- No test project

**Default location:** Östersund, Sweden

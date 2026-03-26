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

## Learnings

### 2026-03-22: Helper layer audit

1. **Three-layer presentation leak identified.** Display logic was spread across AuroraForecast model (60-line `GetActivityDescription`), AuroraService (static `GetActivityLevel`/`GetIconEmoji`), and MainPageViewModel (static `GetDarknessWindowText`/`IsMidnightSun`). All pure functions, none belonged where they lived.

2. **Helper taxonomy crystallized.** Two helpers with clear responsibilities:
   - `ProbabilityDisplayHelper` — calculations + short display labels (probability, Kp classification, emoji, midnight sun bool, cloud adjustment, circle arc)
   - `GuiMessageHelper` (new) — multi-sentence UI copy-text and formatted display strings (activity description, darkness window text)
   - The dividing line: if it returns a number, bool, or one-word label → ProbabilityDisplayHelper. If it returns user-facing prose or formatted strings → GuiMessageHelper.

3. **Naming conflict found.** `AuroraService.GetActivityLevel(kp)` and `ProbabilityDisplayHelper.GetActivityLevelText(probability)` do similar things with different inputs. Renaming the Kp-based one to `GetKpActivityLevel` makes the distinction explicit.

4. **AuroraForecast model had behavior.** `GetActivityDescription()` was the only method on the model — 60+ lines of conditional UI text with 7 parameters, none of which were model properties. Classic misplaced logic. After extraction, model becomes a pure data class (7 auto-properties, zero methods).

5. **VM orchestration pattern confirmed.** The ViewModel's `UpdateCurrentAuroraAndWeatherAsync` and `UpdateThreeDayForecastWithWeatherAsync` are legitimate orchestration — they call services, assemble parameters, call helpers, and set observable properties. This is correct MVVM. The only things that needed to move out were the two static pure-function methods.

### 2026-03-26: Refactor executed (commit 3ed5598, branch Refactor-and-codereview)

All planned moves from the 2026-03-22 audit have been implemented:
- `GuiMessageHelper.cs` created — `GetActivityDescription`, `GetDarknessWindowText`, `IsMidnightSun` live here
- `AuroraForecast.cs` is now a pure data class (zero methods)
- `GetKpActivityLevel` + `GetIconEmoji` moved from `AuroraService` → `ProbabilityDisplayHelper`
- Named constants added to `ProbabilityDisplayHelper` (KpLatitudeOffset, KpLatitudeDivisor, CircleArcUnits)
- `double baseProbability = -1` sentinel fixed to `double? baseProbability = null`
- Build verified clean. Vespera post-build review: security-neutral, all clear.

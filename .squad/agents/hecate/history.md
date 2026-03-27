# Hecate — Backend Engineer

## Project: AuroraFix (seeded 2026-03-21)

**Namespace:** AuroraFix · .NET MAUI net10.0

**Services I own:**

### AuroraService (`Services/AuroraService.cs`)
- `GetCurrentKpIndexAsync()` — NOAA `planetary_k_index_1m.json`, walks backwards for last nonzero `estimated_kp`
- `GetThreeDayForecastAsync(latitude)` — parses NOAA `3-day-geomag-forecast.txt` (brittle text parsing)
- `GetForecastForLocationAsync(city, lat, lon)` — combines Kp + location into `AuroraForecast`
- `CalculateProbability(kp, latitude)` — private switch-based formula (DIFFERENT from Helper — ViewModel uses Helper)
- `GetActivityLevel(kp)` — static: Low/Medium/Active/Storm
- `GetIconEmoji(prob)` — static: colored circle emojis (red/orange/yellow/green)

### WeatherService (`Services/WeatherService.cs`)
- Open-Meteo API, no key
- `GetCurrentWeatherAsync(lat, lon)` — fetches current `cloud_cover`
- `GetThreeDayForecastAsync(lat, lon)` — fetches daily `cloud_cover_mean` (3 days)
- `GetWeatherIcon(cc)` — static: star/cloud emojis
- **Note:** Manual singleton pattern (`Instance`) — bypasses DI

### GeocodingService (`Services/GeocodingService.cs`)
- `GetLocationFromCityAsync(cityName)` — predefined list first, then Nominatim
- Fallback on error: returns Östersund coords (63.8267, 16.0534)
- User-Agent: `"AuroraFixApp/1.0"` (OSM requirement)

**Known issues:**
- Duplicate probability formulas (AuroraService private vs Helper)
- WeatherService singleton bypasses DI
- NOAA text parser is fragile

### 2026-03-27: IP Geolocation auto-start for Issue #9

**Files created:**
- `AuroraFix/Models/IpGeolocationResult.cs`
- `AuroraFix/Services/IpGeolocationService.cs`

**Files modified:**
- `AuroraFix/MauiProgram.cs`
- `AuroraFix/ViewModels/MainPageViewModel.cs`

**Findings and changes:**
- Added `IpGeolocationService` calling `ip-api.com/json/` (free, no API key, 45 req/min rate limit).
- Used `CancellationTokenSource(3s)` per-request inside a singleton `HttpClient` to enforce 3-second timeout without affecting other calls.
- Fire-and-forget pattern (`Task.Run` + `MainThread.BeginInvokeOnMainThread`) keeps app startup non-blocking — welcome state shows immediately.
- Race condition guard added: IP result only applied if `CityName` is still empty and `!IsBusy` at callback time.
- DI registration: `AddSingleton<IpGeolocationService>()` — consistent with `AuroraService`, `GeocodingService`, `WeatherService` (all parameterless, each owns their own `HttpClient`).
- All JSON property names extracted as `private const string` to eliminate magic strings.

**Key architectural decisions:**
- **API choice:** `ip-api.com` — free, no key, JSON, returns city/lat/lon/country/regionName. Status field ("success"/"fail") is explicit.
- **Timeout:** 3 seconds via `CancellationTokenSource` on the request (not `HttpClient.Timeout`) — allows reuse of the `HttpClient` for retry scenarios.
- **Threading:** Fire-and-forget in `InitializeAsync()` + `MainThread.BeginInvokeOnMainThread` for UI update — standard MAUI pattern for background-to-UI-thread marshalling.
- **Fallback:** Any failure (network error, timeout, "fail" status, empty city) → show welcome message. No error surfaced to user for IP lookup failures.

**Files reviewed:**
- AuroraService.cs
- WeatherService.cs

**Findings and changes:**
- Added try/catch around JsonElement.GetDouble() in AuroraService to prevent unhandled exceptions during Kp parsing, with Debug.WriteLine logging.
- Guarded AddForecastDay against empty kpValues and log if empty.
- In WeatherService, replaced direct property access for time/cloud_cover_mean with TryGetProperty and null checks to avoid null dereference and out-of-range errors.
- Wrapped cloud_cover_mean and forecast time parsing in try/catch with Debug.WriteLine logging for parse errors.
- Ensured all LINQ .Average() and collection accesses are guarded against empty collections or nulls.
- No silent swallows found; all catch blocks now log errors.

**Outcome:**
Both services now handle all I/O, parsing, and LINQ operations robustly, logging errors and returning safe fallbacks as required by conventions.
### 2026-03-21: Unicode/Swedish city search bug investigation & fix

**What was broken:**
- `NominatimResult.Lat` and `.Lon` were typed as `double`, but the Nominatim API returns these values as **JSON strings** (quoted, e.g. `"lat":"63.82"`). System.Text.Json threw a `JsonException` when trying to deserialize a string value into a `double` property. This exception was silently caught and the code fell back to Östersund coordinates for every non-predefined city search, making all custom city searches return wrong data without any visible error.
- `GetPopularNordicLocations()` lookup used `StringComparison.OrdinalIgnoreCase`, which only guarantees case-folding for ASCII (a-z/A-Z). For Unicode characters like Ö/ö, this can be unreliable across platforms.

**No issues found in:**
- `MainPage.xaml` — Entry has no Keyboard type restriction, no Behaviors, no MaxLength, no character filtering. Clean.
- `MainPageViewModel.cs` — No regex, no character whitelist, no input validation that strips or blocks characters. `Uri.EscapeDataString` was already correctly used in GeocodingService for the Nominatim URL.

**What was fixed in `GeocodingService.cs`:**
1. Changed `NominatimResult.Lat` and `.Lon` from `double` to `string`, and parse with `double.Parse(..., CultureInfo.InvariantCulture)` at the point of use. This correctly handles Nominatim's string-quoted coordinates.
2. Changed predefined city lookup from `OrdinalIgnoreCase` to `InvariantCultureIgnoreCase` for robust, culture-safe Unicode case-insensitive matching.
3. Added `using System.Globalization;` import.

**Outcome:** All city names — including those with Swedish/European special characters (Å, Ä, Ö, ü, é, etc.) and hyphens — now correctly resolve via Nominatim. Predefined cities (including "Östersund" as default) continue to work as before.

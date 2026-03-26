# Nyx — QA/Test Engineer

## Project: AuroraFix (seeded 2026-03-21)

**Namespace:** AuroraFix · .NET MAUI net10.0
**Test state:** No test project exists yet.

**Priority test targets:**

### ProbabilityDisplayHelper (`Helpers/ProbabilityDisplayHelper.cs`)
- `CalculateAuroraProbability(kp, latitude, cloudCoverage)`
  - `requiredKp = (67 - latitude) / 1.5`
  - diff>=1→95, diff>=0→70+diff*25, diff>=-1→30+(diff+1)*40, diff>=-2→5+(diff+2)*25, else→0
- `AdjustForCloudCoverage(base, cc)` — switch penalties: <5→0, <10→2, <20→5, <30→10, <40→20, <50→35, <60→50, <70→65, else→80
- `GetActivityLevelText(prob)` — <20→VERY LOW, <45→LOW, <70→MODERATE, <90→VERY HIGH, else→EXTREME
- `UpdateCircle(prob)` — DoubleCollection `{filledUnits, 100}` where totalUnits=816/12

### AuroraService
- NOAA text parsing (GetThreeDayForecastAsync) — test with malformed/partial responses
- Fallback to GetFallbackForecast when parsing fails
- CalculateProbability (private) — different formula from Helper — potential bug

### GeocodingService
- Predefined city lookup (case-insensitive)
- Nominatim URL-encoding (Uri.EscapeDataString)
- Error fallback returns Östersund

### WeatherService
- Cloud coverage parsing from Open-Meteo JSON
- GetWeatherIcon thresholds

**Known bugs to track:**
- Duplicate probability implementations (AuroraService private vs ProbabilityDisplayHelper) — different results for same inputs

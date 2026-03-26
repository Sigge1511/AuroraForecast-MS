# Vespera — Security Specialist

## Project: AuroraFix (seeded 2026-03-21)

**What it is:** Consumer aurora forecast app for .NET MAUI (net10.0).
Targets Android, iOS, macCatalyst, Windows.

**Security profile:**
- No user authentication or accounts
- No user data collected or stored
- No backend/server component — purely client-side
- No API keys (all 3 APIs are free/public)
- All network requests: HTTPS only

**External network calls:**
1. `https://services.swpc.noaa.gov/json/planetary_k_index_1m.json` (NOAA)
2. `https://services.swpc.noaa.gov/text/3-day-geomag-forecast.txt` (NOAA)
3. `https://api.open-meteo.com/v1/forecast?...` (Open-Meteo)
4. `https://nominatim.openstreetmap.org/search?...` (OSM Nominatim)

**Input sanitization:**
- City name → `Uri.EscapeDataString()` before URL construction ✓
- No SQL, no eval, no shell exec — not applicable

**Observations:**
- `HttpClient` instances: AuroraService and GeocodingService create their own instances. WeatherService uses singleton. Not a security issue, but connection pooling could be improved.
- No certificate pinning — acceptable for public weather APIs
- User-Agent header `"AuroraFixApp/1.0"` set on GeocodingService ✓ (OSM policy requirement)
- No sensitive data in transit or at rest

## Learnings
- **Date:** 2024-05-23
- **Topic:** Error Handling & UI Safety
- **Context:** Review of `AuroraFix` post-build error hardening.
- **Observation:** 
  - The `CancellationTokenSource` pattern in `BaseViewModel` relies on GC rather than explicit `Dispose()`. In high-throughput scenarios, this could be a resource leak, but for a user-triggered error toast (4s duration), it is acceptable.
  - MAUI `Label` text binding is safe from XSS by default (renders as plain text).
  - Explicit parsing of external NOAA text files using `double.TryParse` provides good defense against malformed data causing crashes.
- **Action:** Approved the changes. Added note about CTS disposal for future refactoring if error rate increases.

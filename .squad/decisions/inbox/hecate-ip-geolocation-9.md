### 2026-03-27: IP Geolocation decisions for Issue #9
**By:** Hecate + Fern (requested by Sigge)
**What:** Key decisions made implementing IP-based auto-start geolocation

**API choice:** `ip-api.com/json/` — free, no API key, no account required. Returns JSON with city, lat, lon, country, regionName. Explicit status field ("success"/"fail"). Rate limit 45 req/min (safe for mobile). Chosen over alternatives (ipinfo.io, ipgeolocation.io) because it requires zero setup.

**Timeout:** 3 seconds via `CancellationTokenSource` on the request itself, not `HttpClient.Timeout`. This allows the singleton `HttpClient` to be reused without carrying a restrictive timeout, and the per-request CTS is properly disposed with `using`.

**Threading pattern:** Fire-and-forget in `InitializeAsync()` using `Task.Run` (background thread) + `MainThread.BeginInvokeOnMainThread` (UI update). Welcome state shows immediately; city auto-loads asynchronously when lookup resolves. Non-blocking by design.

**Fallback behavior:** Any failure path returns null → app shows normal welcome message. No user-visible error for IP lookup failures (silent degradation). Network error, timeout, non-success status, empty city — all handled identically: do nothing.

**Race condition guard:** IP result only applied if `string.IsNullOrWhiteSpace(CityName) && !IsBusy` at callback time. Prevents IP lookup from overwriting a city the user typed during the 3-second window.

**DI registration:** `AddSingleton<IpGeolocationService>()` — parameterless, consistent with all other services (AuroraService, GeocodingService, WeatherService each own their own `HttpClient`).

**Why:** Issue #9 resolution

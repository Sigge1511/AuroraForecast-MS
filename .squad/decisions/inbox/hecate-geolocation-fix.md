# Decision: GPS-first location detection with ipapi.co HTTPS fallback

**Author:** Hecate (Backend Engineer)  
**Date:** 2026 (Issue #9 follow-up — PR #14)  
**Status:** Implemented

## Context

The original `IpGeolocationService` called `https://ip-api.com/json/`. The `ip-api.com` free tier does not support HTTPS. On real Android devices, this silently failed — the HTTP redirect was blocked by the platform's network security policy (cleartext traffic disallowed by default), the exception was swallowed, and geolocation never triggered.

Additionally, the 3-second request timeout was too aggressive for mobile networks and frequently caused false failures.

## Decision

1. **API switch:** Replace `ip-api.com` with `ipapi.co` — free tier, HTTPS supported, no API key, returns city/latitude/longitude/country_name/region. No `status` field; success is indicated by a non-empty `city`.

2. **GPS-first flow:** `MainPageViewModel.InitializeAsync` now tries device GPS first via `Geolocation.GetLastKnownLocationAsync()` / `Geolocation.GetLocationAsync()` + `Geocoding.GetPlacemarksAsync()`. Falls back to IP geolocation only if GPS fails or permission is denied.

3. **Android permissions:** Added `ACCESS_FINE_LOCATION` and `ACCESS_COARSE_LOCATION` to `AndroidManifest.xml`. Runtime permission request (`Permissions.RequestAsync<LocationWhenInUse>`) added to the GPS path.

4. **Timeout increase:** Raised from 3s to 8s to accommodate real mobile network latency.

## Consequences

- GPS provides higher-quality city data and does not depend on any external API.
- IP fallback remains as a reliable safety net for when GPS is unavailable or permission denied.
- Failure at both GPS and IP levels silently degrades to the welcome state — user can still enter a city manually. No error is surfaced for auto-detection failures.
- All location-detection log lines now use `[LocationDetection]` prefix for consistent filtering in logcat.

## Affected Files

- `AuroraFix/Platforms/Android/AndroidManifest.xml`
- `AuroraFix/Services/IpGeolocationService.cs`
- `AuroraFix/ViewModels/MainPageViewModel.cs`

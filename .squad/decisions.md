# Squad Decisions

Canonical team decision log. Scribe appends merged decisions from `.squad/decisions/inbox/`.

---

### 2026-03-02: Squad initialized
**By:** Squad (Coordinator)
**What:** Initialized a witchy all-female squad roster with roles for lead architecture, product/delivery management, backend, frontend, QA, and DevOps.
**Why:** User requested a squad ready to plan, build, test, and manage work.

---

# Decision: Fix Unicode city search in GeocodingService

**By:** Hecate (Backend Engineer)  
**Date:** 2026-03-21  
**Status:** Proposed

## What

Fixed two bugs in `GeocodingService.cs` that caused all non-predefined city searches to silently fail, returning Östersund coordinates instead of the requested location.

## Root Cause

`NominatimResult.Lat` and `.Lon` were typed as `double`. The Nominatim OpenStreetMap API returns latitude/longitude as **JSON strings** (e.g. `"lat":"63.8267"`), not JSON numbers. System.Text.Json cannot deserialize a quoted string into a `double` and throws a `JsonException`. This exception was caught by the generic `catch` block, triggering the hardcoded Östersund fallback — silently returning wrong coordinates for every city not in the predefined list.

## Changes Made

1. **`NominatimResult.Lat` / `.Lon` type**: Changed from `double` to `string`, parsed with `double.Parse(value, CultureInfo.InvariantCulture)` at the call site. This correctly handles Nominatim's string-quoted coordinate format.
2. **Predefined city lookup**: Changed `StringComparison.OrdinalIgnoreCase` → `StringComparison.InvariantCultureIgnoreCase` for reliable Unicode case-insensitive city name matching (e.g. "östersund" matches "Östersund").
3. **Added** `using System.Globalization;` import.

## What Was Not Changed

- No character filtering existed in XAML or ViewModel — nothing to remove.
- `Uri.EscapeDataString` was already correctly used for the Nominatim URL — handles all Unicode characters as UTF-8 percent-encoded. No change needed.
- No regex or character whitelist existed anywhere — the `\p{L}` requirement did not apply.

## Why

This ensures city searches for any name — including those with Swedish/European special characters (Å, Ä, Ö, ü, é, ø, ñ, etc.) and hyphens — are correctly resolved via the Nominatim geocoding API.
# Decision: Fix Unicode keyboard input on Windows (UTF-8 activeCodePage)

**By:** Selene (Frontend Engineer)
**Date:** 2026-03-21
**Status:** Proposed

## What

Added `<activeCodePage>UTF-8</activeCodePage>` to `Platforms/Windows/app.manifest` so that Swedish and other European special characters (Å, Ä, Ö, ü, é, ø, ñ, etc.) can be typed into the city search Entry on Windows.

## Root Cause

`Platforms/Windows/app.manifest` was missing the `<activeCodePage>` declaration. Without it, Windows uses the system ANSI code page (e.g. CP932 on Japanese systems, CP936 on Chinese systems) for Win32 keyboard input processing (WM_CHAR messages). Characters outside the active code page are **silently dropped** at the Win32 layer before they ever reach the WinUI TextBox — so pressing Å on a non-CP1252 system produces nothing at all.

The MAUI/WinUI text control layer is fully Unicode-capable; the problem is one layer below in the Win32 keyboard message subsystem.

## What Was Ruled Out

- **XAML Entry attributes**: No `Keyboard` attribute, no Behaviors, no Effects, no MaxLength, no `<OnPlatform>` blocks. Entry was clean.
- **ViewModel coercion**: `[ObservableProperty] private string cityName` — no `OnCityNameChanging` / `OnCityNameChanged` override methods.
- **Platform handler** (`MainPage.xaml.cs`): The `EntryHandler.Mapper` override only sets `BorderThickness` and a focus border theme resource — purely visual, zero effect on input.
- **GeocodingService.cs**: Already in correct post-fix state (Lat/Lon as `string`, `InvariantCultureIgnoreCase`). No change needed.

## Change Made

**File:** `AuroraFix/Platforms/Windows/app.manifest`

Added inside `<windowsSettings>`:
```xml
<activeCodePage xmlns="http://schemas.microsoft.com/SMI/2019/WindowsSettings">UTF-8</activeCodePage>
```

## Why

This is the Microsoft-documented solution for full Unicode support in Win32/WinUI Windows apps. It instructs Windows to treat the process code page as UTF-8, ensuring all Unicode characters — including every Latin-extended character used in European city names — are delivered correctly through the WM_CHAR message path.

Reference: https://learn.microsoft.com/en-us/windows/apps/design/globalizing/use-utf8-code-page

## Impact

- All Unicode characters can now be typed into the search field on all Windows systems regardless of system locale.
- No visual changes, no behavior changes, no API changes.
- No other platforms affected (manifest is Windows-only).

## Session directives — 2026-03-24

### 2026-03-24T22-47: User directive — Vespera consultation requirement
**By:** Sigge (via Copilot)
**What:** Vespera MUST be consulted both before and after building anything new. She may cooperate with Morgana on security architecture and SHALL always collaborate with Freya to ensure security documentation is complete.
**Why:** User request — security must be baked in from the start AND documented every time.

### 2026-03-24T22-47: User directive — Hecate + Fern parallel work
**By:** Sigge (via Copilot)
**What:** Hecate and Fern should always be spawned in parallel when backend work is assigned. No reason to serialize them — they are more efficient working simultaneously.
**Why:** User request — maximize backend throughput by running both witches at once.

### 2026-03-24T22-47: User directive — River's lockout policy softened
**By:** Sigge (via Copilot)
**What:** River should NOT enforce strict author lockouts every time she flags issues. Lockouts are reserved for serious rejections. For minor alterations and small fixes, the original author may address River's feedback directly. River's role is to guide and improve, not to block.
**Why:** User request — the coven works with love, not bureaucracy.

### 2026-03-24T22-47: User directive — Always address owner as Sigge
**By:** Sigge (via Copilot)
**What:** The project owner is always to be called "Sigge". Never "Maja", never any other name. This applies to all agents and the coordinator in every session.
**Why:** User preference — persistent identity rule.

# Team Roster

> Witchy all-female AI squad for planning, building, testing, and delivery.

## Coordinator

| Name | Role | Notes |
|------|------|-------|
| Squad | Coordinator | Routes work, enforces handoffs and reviewer gates. Does not generate domain artifacts. |

## Members

| Name | Role | Charter | Status |
|------|------|---------|--------|
| Morgana | Lead Architect | `.squad/agents/morgana/charter.md` | ✅ Active |
| River | Code Reviewer | `.squad/agents/river/charter.md` | ✅ Active |
| Circe | Product/Delivery Lead | `.squad/agents/circe/charter.md` | ✅ Active |
| Hecate | Backend Engineer | `.squad/agents/hecate/charter.md` | ✅ Active |
| Fern | Backend Engineer | `.squad/agents/fern/charter.md` | ✅ Active |
| Selene | Frontend Engineer | `.squad/agents/selene/charter.md` | ✅ Active |
| Nyx | QA/Test Engineer | `.squad/agents/nyx/charter.md` | ✅ Active |
| Rowan | DevOps Engineer | `.squad/agents/rowan/charter.md` | ✅ Active |
| Freya | Technical Writer | `.squad/agents/freya/charter.md` | ✅ Active |
| Vespera | Security Specialist | `.squad/agents/vespera/charter.md` | ✅ Active |
| Calista | UX Specialist | `.squad/agents/calista/charter.md` | ✅ Active |
| Scribe | Session Logger | `.squad/agents/scribe/charter.md` | 📋 Silent |
| Ralph | Work Monitor | — | 🔄 Monitor |

## Coding Agent

<!-- copilot-auto-assign: false -->

| Name | Role | Charter | Status |
|------|------|---------|--------|
| @copilot | Coding Agent | — | 🤖 Coding Agent |

## Project Context

- **Owner:** MajaSigfeldt (always address as **Sigge**)
- **Stack:** C# · .NET MAUI (net10.0) · CommunityToolkit.Mvvm 8.4.0 · Android / iOS / macCatalyst / Windows
- **Description:** AuroraFix — a northern lights (aurora borealis) forecast mobile/desktop app. Fetches real-time Kp-index from NOAA, 3-day geomagnetic forecast, and cloud coverage from Open-Meteo. Combines Kp + latitude + cloud cover to calculate aurora visibility probability (0–100%). Single-page MVVM app with a dark space theme.
- **Solution:** AuroraForecast.slnx → AuroraFix/AuroraForecast.csproj
- **Namespace:** AuroraFix
- **UI:** Dark theme #050810, aurora green accent #2DCCAA, Montserrat fonts, animated GIF background, circular stroke-dash probability ring
- **Default location:** Östersund, Sweden
- **Key files:**
  - `AuroraFix/Views/MainPage.xaml` — only page
  - `AuroraFix/ViewModels/MainPageViewModel.cs` — main logic
  - `AuroraFix/Helpers/ProbabilityDisplayHelper.cs` — probability math
  - `AuroraFix/Services/AuroraService.cs` — NOAA Kp data
  - `AuroraFix/Services/WeatherService.cs` — Open-Meteo cloud coverage (singleton)
  - `AuroraFix/Services/GeocodingService.cs` — OpenStreetMap Nominatim + Nordic presets
  - `AuroraFix/Models/` — AuroraForecast, ForecastDay, SelectedLocation, Weather
- **APIs used (all free/no key):** NOAA SWPC, Open-Meteo, OpenStreetMap Nominatim
- **Updated:** 2026-03-21T12:44:09Z

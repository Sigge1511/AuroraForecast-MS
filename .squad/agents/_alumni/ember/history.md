# Ember — History & Learnings

## Core Context

**Project:** AuroraFix — .NET MAUI aurora borealis forecast app
**Owner:** Sigge (MajaSigfeldt)
**Stack:** C# · .NET MAUI net10.0 · CommunityToolkit.Mvvm 8.4.0 · Android / iOS / macCatalyst / Windows
**UI:** Single-page dark space theme. `#050810` background, `#2DCCAA` aurora teal accent, Montserrat fonts, `giphy.gif` animated background at 0.5 opacity.

## Team

- **Morgana** — Lead Architect
- **River** — Code Reviewer
- **Circe** — Product/Delivery Lead
- **Hecate / Fern** — Backend Engineers (Hecate + Fern always run in parallel)
- **Selene** — Frontend Engineer (implements animations — coordinate on perf)
- **Lyra** — Motion & Visual Experience Designer (owns animation design — perf must coexist with beauty)
- **Nyx** — QA/Test Engineer
- **Rowan** — DevOps Engineer
- **Freya** — Technical Writer
- **Vespera** — Security Specialist (mandatory pre/post-build review)
- **Calista** — UX Specialist
- **Scribe** — Session Logger (silent)
- **Sigge** — Owner (always call by this name)

## Key Architecture (as of joining)

- `MainPageViewModel.cs` — MVVM orchestration; calls services, assembles params, calls helpers, sets observable properties
- `AuroraService.cs` — NOAA API: Kp index + 3-day geomagnetic forecast
- `WeatherService.cs` — Open-Meteo: cloud cover, sunrise/sunset, is_day (singleton, HTTP client reused)
- `GeocodingService.cs` — OpenStreetMap Nominatim + Nordic presets (predefined list checked first)
- `ProbabilityDisplayHelper.cs` — All probability calculations + display labels
- `GuiMessageHelper.cs` — All user-facing prose strings
- `AuroraForecast.cs` — Pure data class (7 properties, zero methods)
- `BaseViewModel.cs` — HasError, ErrorMessage, SetError (4s auto-dismiss), DismissErrorCommand

## Recent Team Work

- Full refactor: GuiMessageHelper created, AuroraForecast cleaned to pure data, display methods moved out of AuroraService, named constants added to ProbabilityDisplayHelper, sentinel pattern fixed (double? baseProbability = null)
- Error hardening: AuroraService + WeatherService try/catch hardened by Hecate; GeocodingService confirmed robust; MainPageViewModel null guard added
- Error toast UI: Selene implemented dark scrim overlay + rounded teal card + 4s auto-dismiss + OK button in MainPage.xaml + BaseViewModel
- Lyra joined as Motion & Visual Experience Designer (2026-03-26)
- Vespera approved all recent changes

## Learnings

*(Ember joined the coven on 2026-03-26 — learnings will accumulate here)*

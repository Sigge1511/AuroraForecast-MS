# Lyra — History & Learnings

## Core Context

**Project:** AuroraFix — .NET MAUI aurora borealis forecast app
**Owner:** Sigge (MajaSigfeldt)
**Stack:** C# · .NET MAUI net10.0 · CommunityToolkit.Mvvm 8.4.0 · Android / iOS / macCatalyst / Windows
**Role:** Performance Engineer — startup time, API parallelism, memory, 60fps animations on Android

## Team

- **Selene** — Frontend Engineer (implements animations; Lyra advises on perf constraints)
- **Calista** — UX Specialist (interaction design, accessibility)
- **Morgana** — Lead Architect
- **River** — Code Reviewer
- **Hecate / Fern** — Backend Engineers
- **Nyx** — QA/Test Engineer
- **Vespera** — Security Specialist (mandatory pre/post-build review)
- **Scribe** — Session Logger (silent)
- **Sigge** — Owner (always call by this name)

## Learnings

*(Lyra joined the coven on 2026-03-26 as Motion & Visual Experience Designer; role updated to Performance Engineer on 2026-03-26)*

---

### 2026-03-26 — Audit: AuroraService / MainPageViewModel / ProbabilityDisplayHelper

**Files audited:** `AuroraService.cs`, `MainPageViewModel.cs`, `ProbabilityDisplayHelper.cs`

#### [CRITICAL] Sequential HTTP pipeline — 4 awaits in series
`SearchCityAsync()` chains 4 HTTP calls end-to-end:
1. `GetForecastForLocationAsync` → internally awaits `GetCurrentKpIndexAsync` (NOAA KP JSON)
2. `GetCurrentWeatherAsync` (Open-Meteo current)
3. `GetThreeDayForecastAsync` (NOAA 3-day text)
4. `GetFourDayForecastAsync` (Open-Meteo 4-day)

Calls 1+2 are sequential inside `UpdateCurrentAuroraAndWeatherAsync`; calls 3+4 are sequential inside `UpdateThreeDayForecastWithWeatherAsync`; both methods are themselves awaited sequentially. All 4 are fully independent — no result depends on another before being issued. On a 200ms-latency mobile connection this wastes ~600ms. `Task.WhenAll` across all four would cut the wait to the slowest single call.

#### [MODERATE] ObservableCollection batch-add fires 3 UI re-renders
`UpdateThreeDayForecastWithWeatherAsync` calls `ThreeDayForecast.Clear()` then `ThreeDayForecast.Add()` in a loop — each `Add` raises `CollectionChanged`, triggering a layout/measure pass. For 3 items on a low-end Android device this means 3 render frames instead of 1. Replacing with a fresh `ObservableCollection<ForecastDay>` assignment (or wrapping in a suppress-notify pattern) collapses it to one notification.

#### [MODERATE] Full NOAA KP JSON deserialised into `JsonElement[]`
`GetCurrentKpIndexAsync` downloads and fully deserialises the entire `planetary_k_index_1m.json` array — up to ~1440 entries per day — just to read the last 30. On Android the JSON string sits in managed heap for the full round-trip before GC can reclaim it. Consider reading the raw UTF-8 stream directly and stopping after the last 30 records, or at minimum slicing `jsonArray` before iterating.

#### [MINOR] Redundant `CalculateAuroraProbability` call
`UpdateCurrentAuroraAndWeatherAsync` calls `CalculateAuroraProbability(kp, lat, 0)` for `baseProbability` and then `CalculateAuroraProbability(kp, lat, clouds)` for `Probability`. The second call already invokes `AdjustForCloudCoverage` internally. The math is cheap but the pattern is confusing; calling the helper once and calling `AdjustForCloudCoverage` manually for the adjusted copy is cleaner and slightly faster.

#### [MINOR] LINQ allocation inside `AddForecastDay`
`kpValues.Any()` allocates an enumerator on every call; `kpValues.Average()` does the same. Replace with `kpValues.Count > 0` and a manual sum-loop. Small, but called on every refresh on the parse thread.

#### [MINOR] `DoubleCollection` allocation per refresh
`ProbabilityDisplayHelper.UpdateCircle` constructs `new DoubleCollection { ... }` on every invocation. Called on each search/refresh. Not hot-path critical, but it's a heap allocation that could be avoided with a small cache keyed on (int)prob.

#### [GOOD] No `async void` found
All async paths return `Task`. `[RelayCommand]` on `RefreshAsync` and `SearchCityAsync` are properly awaited by the toolkit. No fire-and-forget concerns.

#### [GOOD] HttpClient lifetime correct
Both `AuroraService` and `WeatherService` are registered as singletons; their `HttpClient` instances live for the app lifetime. No socket exhaustion, no per-request allocation.

#### [GOOD] ProbabilityDisplayHelper — zero boxing, zero allocations
All switch expressions operate on `double` value types and return string literals. Fully allocation-free. The recalibrated probability tiers (`diff` branches at ≥3, ≥2, ≥1, ≥0, ≥−1) are clean and read correctly.

---

### River (Code Reviewer) — Findings Relevant to Performance (2026-03-26)

River reviewed the same files and flagged issues that have performance or safety implications Lyra should note:

- **Cloud coverage threshold duplication [repeat]:** Thresholds `15/40/75` appear verbatim in both `AdjustForCloudCoverage` and `GetCloudImpactLabel`. Any future calibration touching one will silently diverge from the other. Lyra note: if thresholds ever change, this is a silent bug risk that could affect displayed probability.
- **Probability curve magic numbers:** Bases (90, 60, 35, 10, 2) and multipliers (30, 25, 25, 8) in `CalculateAuroraProbability` are unnamed. Lyra note: these are the inputs to the performance-relevant hot path; naming them also aids profiling annotations.
- **Bare `catch { }` in `ReadKpFromEntry`:** River recommends `JsonElement.TryGetDouble()` to eliminate silent swallowing. Lyra note: Lyra's audit marked this as acceptable for defensive intent — River's preferred fix (`TryGetDouble`) would still be zero-alloc and equally safe.
- **What-comments in `UpdateCurrentAuroraAndWeatherAsync`:** Dead narrative comments. Lyra note: removing them reduces cognitive noise when tracing the async dispatch sequence.
- **Dead null guard in `UpdateLocationDisplay` (line 102):** Unreachable. Lyra note: no performance impact; safe to remove.
- **`Debug.WriteLine` at 3 locations in `AuroraService.cs`:** Pre-production logging debt. Lyra note: these do not affect performance benchmarks but should be stripped before profiling baselines are established.

---

#### [GOOD] `ReadKpFromEntry` helper
Clean extraction, static, no heap pressure. The `catch {}` swallowing on `GetDouble()` is acceptable given the defensive intent — NOAA field types are not guaranteed.

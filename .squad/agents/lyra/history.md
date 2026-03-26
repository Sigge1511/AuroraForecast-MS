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

### 2026-03-26 — Implementation: All Audit Findings Applied

**Files changed:** `AuroraService.cs`, `MainPageViewModel.cs`, `ProbabilityDisplayHelper.cs`
**Commit:** `889a4a9` — `perf: parallelise HTTP pipeline, batch ObservableCollection, remove allocations`

#### What changed and before/after:

**[CRITICAL] HTTP parallelism** (`MainPageViewModel.cs`)
- Before: `UpdateCurrentAuroraAndWeatherAsync` awaited `GetForecastForLocationAsync`, then `GetCurrentWeatherAsync`; `UpdateThreeDayForecastWithWeatherAsync` awaited `GetThreeDayForecastAsync`, then `GetFourDayForecastAsync`. Both methods were themselves awaited sequentially — 4 serial HTTP round-trips.
- After: `SearchCityAsync` fires all 4 tasks simultaneously (`forecastTask`, `weatherTask`, `threeDayTask`, `fourDayTask`) and awaits `Task.WhenAll`. On a 200ms mobile connection this collapses ~800ms of wait to ~200ms (the slowest single call).
- The two old async methods were replaced by sync helpers `ApplyCurrentAuroraAndWeather` and `ApplyThreeDayForecastWithWeather` that accept pre-fetched results.

**[MODERATE] ObservableCollection single assignment** (`MainPageViewModel.cs`)
- Before: `ThreeDayForecast.Clear()` + `ThreeDayForecast.Add(day)` in a loop — 4 `CollectionChanged` events (1 clear + 3 adds), 3 layout/measure passes on Android.
- After: build `List<ForecastDay>` locally, then `ThreeDayForecast = new ObservableCollection<ForecastDay>(newItems)` — fires exactly 1 `PropertyChanged`, 1 layout pass.

**[MODERATE] NOAA JSON slice** (`AuroraService.cs`)
- Before: `int windowSize = Math.Min(30, jsonArray.Length)` with loop bound `jsonArray.Length - windowSize`.
- After: `int windowStart = Math.Max(0, jsonArray.Length - 30)` with loop bound `>= windowStart`. Semantically identical but matches the canonical audit pattern.

**[MINOR] Redundant probability call** (`MainPageViewModel.cs`)
- Before: `CalculateAuroraProbability(kp, lat, 0)` + `CalculateAuroraProbability(kp, lat, clouds)` — double computation, confusing intent.
- After: `CalculateAuroraProbability(kp, lat)` once for `baseProbability`, then `Probability = AdjustForCloudCoverage(baseProbability, clouds)` — one call, explicit intent. Mathematically equivalent because `baseProbability ∈ [0,90]` so clamping in the second call is always a no-op.

**[MINOR] LINQ removal in AddForecastDay** (`AuroraService.cs`)
- Before: `kpValues.Any()` (enumerator alloc), `kpValues.Average()` (enumerator alloc).
- After: `kpValues.Count == 0`, manual `foreach` sum + division. Zero allocations on the parse thread.

**[MINOR] DoubleCollection cache in UpdateCircle** (`ProbabilityDisplayHelper.cs`)
- Before: `new DoubleCollection { filledUnits, 100 }` on every call — heap alloc per search/refresh.
- After: `ConcurrentDictionary<int, DoubleCollection>` keyed on `(int)prob` (0–100), lazy-populated via `GetOrAdd`. Max 101 instances ever created. `ConcurrentDictionary` chosen for thread safety.

**[BONUS] ReadKpFromEntry TryGetDouble** (`AuroraService.cs`)
- Before: `try { return est.GetDouble(); } catch { }` — exception-based control flow, silently swallows unexpected types.
- After: `est.TryGetDouble(out double kp)` — branch on bool, zero-alloc, no exception machinery.

**[BONUS] Remove Debug.WriteLines** (`AuroraService.cs`, `MainPageViewModel.cs`)
- Removed all `System.Diagnostics.Debug.WriteLine` calls from `AuroraService` (3 sites) and `SearchCityAsync` (1 site). Also removed unused `using System.Collections.ObjectModel` from `AuroraService.cs`. Catch clauses rewritten as `catch (Exception)` where variable was only used for logging.

**[BONUS] Dead null guard removed** (`MainPageViewModel.cs`)
- `UpdateLocationDisplay` had `if (location == null) return;` — unreachable because the caller only calls it after a non-null check. Removed.

**[BONUS] Cloud threshold constants** (`ProbabilityDisplayHelper.cs`)
- Extracted `CloudClearThreshold = 15.0`, `CloudPartlyThreshold = 40.0`, `CloudMostlyThreshold = 75.0`. Applied in both `AdjustForCloudCoverage` and `GetCloudImpactLabel` — eliminates silent divergence risk flagged by River.

**[BONUS] Probability curve constants** (`ProbabilityDisplayHelper.cs`)
- Extracted `ProbAtPlusWith3`, `ProbAtPlusWith2`, `ProbAtPlusWith1`, `ProbAtThreshold`, `ProbAtMinusWith1`, `ProbSlopeHigh`, `ProbSlopeMid`, `ProbSlopeLow`. Named constants now document the curve intent.

#### Gotchas:
- `DoubleCollection` is a MAUI/WPF mutable list type — cached instances are returned directly. Safe because values are set once at construction and never mutated.
- `catch (Exception)` (no variable) is valid C# — cleaner than `catch (Exception ex)` when the variable is only needed for logging that we're removing.
- The `System.Collections.ObjectModel` using in `AuroraService.cs` was dead (leftover from earlier code) — removed safely.

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

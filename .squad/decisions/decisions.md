# Decisions — AuroraFix

---

## Decision: Refactor — Extract GuiMessageHelper and clean up responsibility boundaries

**By:** Morgana (Lead Architect)  
**Date:** 2026-03-22  
**Status:** Executed (commit 3ed5598)  
**Requested by:** Sigge

Display/presentation logic is scattered across AuroraForecast, AuroraService, and MainPageViewModel. Move GetActivityDescription to GuiMessageHelper, GetActivityLevel/GetIconEmoji to ProbabilityDisplayHelper, and IsMidnightSun to ProbabilityDisplayHelper. AuroraForecast becomes a pure data class.

---

## Decision: River Code Review — VM & Model Refactor

**By:** River (Code Reviewer)  
**Date:** 2026-03-24  
**Status:** Reviewed  
**Requested by:** Sigge

Confirms Morgana's moves. Adds: extract duplicated darkHours formula, cloud coverage thresholds, and probability formula. Use named constants for magic numbers. Remove Debug.WriteLine from production. Consider splitting UpdateCurrentAuroraAndWeatherAsync for testability.

---

## Decision: Harden error handling in AuroraService and WeatherService

**By:** Hecate (Backend Engineer)  
**Date:** 2026-03-24  
**Status:** Proposed

Audited and improved error handling in AuroraService.cs and WeatherService.cs. Added targeted try/catch blocks for all risky operations (JSON parsing, HTTP, LINQ on collections), replaced direct Parse calls with TryParse where needed, and ensured all exceptions are logged. All collection operations are now guarded against empty/null, and no silent swallows remain. Both services return safe fallbacks and never throw to ViewModel.

---

## Decision: Error toast/overlay implementation

**By:** Selene (Frontend Engineer)  
**Date:** 2026-03-24  
**Status:** Proposed

Implemented a styled error overlay/toast for error feedback. The overlay uses a dark scrim, a rounded card, Montserrat font, and teal accent, matching the aurora design language. It auto-dismisses after 4 seconds or can be dismissed via an OK button. Fully ViewModel-driven — timer logic in BaseViewModel, no code-behind. DismissErrorCommand added. Overlay Grid added to MainPage.xaml.

---

## Decision: Security Review — Post-Build Error Hardening

**By:** Vespera (Security Specialist)  
**Date:** 2026-03-24  
**Status:** APPROVED

Error toast, service hardening, and BaseViewModel concurrency pattern reviewed. No new security vulnerabilities identified. Error messages rendered via MAUI Label (plain text — no injection risk). CancellationTokenSource pattern is correct. TryParse and JsonSerializer properly reject malformed external input. Low-priority note: consider disposing `_errorCts` if ViewModel lifecycle grows complex.

---

## Decision: Security Review — Post-Refactor (branch: Refactor-and-codereview)

**By:** Vespera (Security Specialist)  
**Date:** 2026-03-26  
**Status:** APPROVED

All refactored/added files reviewed for injection risk, data exposure, new attack surface, and null safety for double?. GuiMessageHelper, ProbabilityDisplayHelper, AuroraForecast, AuroraService, MainPageViewModel — no security risks introduced. No action required.

---

## Decision: River Code Review — KP Overhaul + VM Constructor + ProbabilityDisplayHelper Recalibration

**By:** River (Code Reviewer)  
**Date:** 2026-03-26  
**Status:** Reviewed

KP peak window logic and ReadKpFromEntry extraction cleared. VM constructor order correct. Flagged for action before production:
1. `Debug.WriteLine` still live at 3 locations in AuroraService.cs — pre-production debt
2. Cloud coverage thresholds 15/40/75 still duplicated in AdjustForCloudCoverage + GetCloudImpactLabel — **repeat from 2026-03-24**; extract to named constants
3. Probability curve magic numbers (bases: 90, 60, 35, 10, 2; multipliers: 30, 25, 25, 8) — charter rule requires named constants

Clarity debt (should fix):
4. Bare `catch { }` in ReadKpFromEntry — use JsonElement.TryGetDouble()
5. What-comments in UpdateCurrentAuroraAndWeatherAsync — remove/replace with why-comments
6. Dead null guard in UpdateLocationDisplay line 102 — remove

---

## Decision: Performance Audit — AuroraService / MainPageViewModel / ProbabilityDisplayHelper

**By:** Lyra (Performance Engineer)  
**Date:** 2026-03-26  
**Status:** Proposed

**[CRITICAL]** 4 HTTP calls in SearchCityAsync run sequentially; all are fully independent. Refactor with Task.WhenAll — expected ~60–70% fetch latency reduction on mobile.

**[MODERATE]**
- ObservableCollection batch-add fires 3 CollectionChanged events / layout passes; fix with single fresh-collection assignment.
- Full NOAA KP JSON (~1440 entries) deserialised into JsonElement[] but only last 30 used; stream tail only or use JsonDocument to slice before materialising.

**[MINOR]**
- Redundant CalculateAuroraProbability call; call AdjustForCloudCoverage directly on base result.
- LINQ allocations in AddForecastDay (Any/Average); replace with Count > 0 and manual sum-loop.
- DoubleCollection allocated on every refresh in UpdateCircle; cache by integer probability bucket.

---

## Directive: Always address owner as Sigge

**By:** Sigge  
**Date:** 2026-03-24  
**Status:** Active

The project owner is always to be called "Sigge". Never "Maja", never any other name. This applies to all agents and the coordinator in every session.

---

## Directive: River's lockout policy softened

**By:** Sigge  
**Date:** 2026-03-24  
**Status:** Active

River should NOT enforce strict author lockouts every time she flags issues. Lockouts are reserved for serious rejections. For minor alterations and small fixes, the original author may address River's feedback directly. River's role is to guide and improve, not to block.

---

## Directive: Vespera consultation requirement

**By:** Sigge  
**Date:** 2026-03-24  
**Status:** Active

Vespera MUST be consulted both before and after building anything new. She may cooperate with Morgana on security architecture and SHALL always collaborate with Freya to ensure security documentation is complete.

---


# Decision: Rename App Display Name to "Aurora Catcher"

**Date:** 2026-03-21  
**Author:** Rowan (DevOps Engineer)  
**Requested by:** Sigge

## Decision

The app's user-visible display name has been changed from **"AuroraFix"** to **"Aurora Catcher"** across all surfaces where the name appears on a device. The GitHub repository name (`AuroraFix`) is unchanged.

## Files Changed

| File | Change |
|------|--------|
| `AuroraFix/AuroraForecast.csproj` | `<ApplicationTitle>` updated to `Aurora Catcher` |
| `AuroraFix/AppShell.xaml` | `Title` attribute updated to `Aurora Catcher` |

## Rationale

- `<ApplicationTitle>` in `.csproj` is the single source of truth for the device display name in .NET MAUI; it feeds Android, iOS, macOS, and Windows manifests automatically.
- `AppShell.xaml Title` is independently visible on platforms that render a navigation bar title.
- `MainPage.xaml` already displayed `"AURORA CATCHER"` (Selene's work); this change aligns the OS-level label and shell title with that existing UI string.

## Scope Not Changed

- Namespace: `AuroraFix`
- Class names, file names, variable names, code comments
- GitHub repo name
- App ID: `com.companyname.aurorafix`

---

### 2026-03-27T00:21: GUI fix decisions for Issue #10
**By:** Selene (requested by Sigge)
**What:** Three GUI fixes in `AuroraFix/Views/MainPage.xaml` for Issue #10.

#### Circle geometry
Chose `StartPoint="160,25"` / `Size="135,135"` / `Point="159.9,25"` for both Path elements in a 320×320 Grid. Arc centre = (160, 25+135) = (160,160) which equals (Width/2, Height/2). The tiny X-offset on the endpoint (159.9 vs 160) prevents MAUI from collapsing a zero-length arc. Grid was grown from 300×300 to 320×320 rather than shrinking the arc, to give text more room inside the ring. Both paths are geometrically identical so the background halo and teal arc track perfectly.

#### Inner text layout
Removed `Margin="-10,3,0,0"` from the inner `VerticalStackLayout`. The negative left margin was a legacy pixel-nudge that compensated (badly) for the mis-centred arc. With correct geometry it is no longer needed — `HorizontalOptions="Center"` and `VerticalOptions="Center"` alone are sufficient.

#### Bottom margin recalibration
Changed ring Grid `Margin` from `0,0,0,-30` to `0,0,0,-25`. In the old 300×300 grid, the circle path bottom was at y=270 and `300-270=30`, so `-30` perfectly aligned the MORE card with the circle bottom. New circle bottom is at y=295 in the 320px grid; correct overlap value is `-(320-295) = -25`.

#### Forecast dimming
Root cause was renderer-level: `CollectionView` had no explicit `SelectionMode` or background colour. Platform renderers can apply default selection/hover fills to item containers. Fix: `SelectionMode="None"` + `BackgroundColor="Transparent"` on `CollectionView`; `BackgroundColor="Transparent"` on the DataTemplate `Grid`. The intentional `Opacity="0.7"` on the secondary KP INDEX label was deliberately preserved (it is a visual hierarchy signal).

**Why:** Issue #10 resolution — three separate visual defects in the probability ring and forecast section.
**PR:** https://github.com/Sigge1511/AuroraForecast-MS/pull/12

---

# Decision: Bump KP INDEX font size and top margin in circle center

**By:** Selene (Frontend Engineer)
**Date:** 2026-03-28
**Status:** Proposed

## What

Increased font size and top margin on the two KP INDEX labels inside the probability ring's `VerticalStackLayout` in `AuroraFix/Views/MainPage.xaml`.

| Label | Property | Before | After |
|---|---|---|---|
| `Text="KP INDEX: "` | FontSize | `{StaticResource FontSizeLabel}` (18) | `21` |
| `Text="KP INDEX: "` | Margin | `0,18,0,0` | `0,26,0,0` |
| `Text="{Binding CurrentKpIndex, ...}"` | FontSize | `{StaticResource FontSizeLabel}` (18) | `21` |

## Why

Sigge requested more visual breathing room between the large probability % number and the circle stroke ring at the bottom. The KP INDEX block also needed to feel slightly more elevated and present. Bumping font size by +3pt and top margin by +8pt achieves both goals in a targeted way.

## Why Inline Values (Not Resource Change)

`FontSizeLabel` (18) is shared with the "PROBABILITY" header label above the big % number. Changing the resource would inadvertently enlarge that label too. Inline overrides are the surgical choice here.

## Impact

- Circle center visual balance improved on all platforms.
- No structural XAML changes, no ViewModel changes, no binding changes.
- The `FontSizeLabel` resource remains 18 and is unaffected for all other usages.

---

# Decision: Fix animated GIF layout thrashing (micro-shaking) on Android

**By:** Selene (Frontend Engineer)
**Date:** 2026-03-28
**Status:** Proposed

## What

Wrapped the `giphy.gif` background `Image` in an `AbsoluteLayout` with proportional layout bounds to prevent animation-frame `InvalidateMeasure` calls from cascading up the layout tree, eliminating the micro-shaking on Android.

## Root Cause

`<Image Source="giphy.gif" IsAnimationPlaying="True" />` was placed directly in the root `Grid`. On Android, MAUI's `Image` renderer fires `InvalidateMeasure` on **every GIF animation frame** (via `AnimatedDrawable.invalidateSelf()`). Since `Grid` must measure its children to determine its own size, this triggers a full re-measure/re-layout of the entire view tree at animation speed → all UI elements visibly shake.

## Fix Applied

**File:** `AuroraFix/Views/MainPage.xaml`

Replaced bare `<Image>` with an `AbsoluteLayout`-wrapped version:

```xml
<AbsoluteLayout HorizontalOptions="Fill" VerticalOptions="Fill">
    <Image Source="giphy.gif"
           Aspect="AspectFill"
           Opacity="0.5"
           IsAnimationPlaying="True"
           AbsoluteLayout.LayoutBounds="0,0,1,1"
           AbsoluteLayout.LayoutFlags="All" />
</AbsoluteLayout>
```

An `AbsoluteLayout` with `LayoutFlags="All"` (proportional) sizes children from its own already-known dimensions — it does NOT measure children to determine its own size. Therefore, frame-driven `InvalidateMeasure` from the Image is absorbed inside the `AbsoluteLayout` and never reaches the parent Grid. The GIF animation and visuals are unchanged.

## What Was Ruled Out

- **ViewModel/BaseViewModel**: No timers, no continuous property updates. All bindings are one-shot on user search.
- **Shadow effect**: Only repaints when `Probability` changes (on search), not continuously.
- **ScrollView**: `VerticalScrollBarVisibility="Never"` is safe — not a cause.
- **Bindings**: No binding-driven sizes/positions in AbsoluteLayout children.

## Team Convention Going Forward

Never place animated GIF `Image` controls directly in `Grid`, `StackLayout`, or `FlexLayout` containers. Always isolate in an `AbsoluteLayout` with proportional bounds. If `CommunityToolkit.Maui` is added to the project in the future, consider migrating to `MediaElement` (plays as a looping video with no layout side-effects).

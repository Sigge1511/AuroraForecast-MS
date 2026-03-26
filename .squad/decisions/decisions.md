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

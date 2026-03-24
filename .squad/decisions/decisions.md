# Refactor Decisions — VM/Helper/Model

## Decision: Refactor — Extract GuiMessageHelper and clean up responsibility boundaries

**By:** Morgana (Lead Architect)  
**Date:** 2026-03-22  
**Status:** Proposed  
**Requested by:** Sigge1511

Display/presentation logic is scattered across AuroraForecast, AuroraService, and MainPageViewModel. Move GetActivityDescription to GuiMessageHelper, GetActivityLevel/GetIconEmoji to ProbabilityDisplayHelper, and IsMidnightSun to ProbabilityDisplayHelper. AuroraForecast becomes a pure data class.

## Decision: River Code Review — VM & Model Refactor

**By:** River (Code Reviewer)  
**Date:** 2026-03-24  
**Status:** Reviewed  
**Requested by:** Sigge1511

Confirms Morgana's moves. Adds: extract duplicated darkHours formula, cloud coverage thresholds, and probability formula. Use named constants for magic numbers. Remove Debug.WriteLine from production. Consider splitting UpdateCurrentAuroraAndWeatherAsync for testability.

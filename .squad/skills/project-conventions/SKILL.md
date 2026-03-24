---
name: "project-conventions"
description: "Core conventions and patterns for AuroraFix (.NET MAUI)"
domain: "project-conventions"
confidence: "medium"
source: "codebase-scan-2026-03-21"
---

## Context

AuroraFix is a .NET MAUI (net10.0) aurora forecast app. C# with CommunityToolkit.Mvvm source generators. Single-page MVVM. No test project yet.

## Patterns

### MVVM with Source Generators
Use `[ObservableProperty]` for bindable properties (generates `OnXxxChanged` partials) and `[RelayCommand]` for commands in ViewModels. Never write `INotifyPropertyChanged` boilerplate manually.

```csharp
[ObservableProperty] private string cityName = string.Empty;

[RelayCommand]
private async Task SearchCityAsync() { ... }
```

### ViewModels inherit BaseViewModel
All ViewModels extend `BaseViewModel : ObservableObject`. Use `IsBusy`, `SetError()`, `ClearError()` from base. Set `Title` in constructor.

### Service Registration
Register services as `AddSingleton<T>()` in `MauiProgram.cs`. Exception: `WeatherService` uses its own `Instance` singleton — do not add a second registration for it.

### Error Handling
Services use `try/catch` returning null or a fallback value (never throw to ViewModel). ViewModels call `SetError(message)` on catch. Debug logging via `System.Diagnostics.Debug.WriteLine`.

```csharp
catch (Exception ex)
{
    System.Diagnostics.Debug.WriteLine($"Error: {ex.Message}");
    return null; // or fallback value
}
```

### UI Design Tokens
- Background: `#050810`
- Accent / primary: `#2DCCAA` (aurora green)
- Muted text: `#60FFFFFF`, `#20FFFFFF`
- Fonts: `Montserrat`, `MontserratBold`
- Rounded cards: `StrokeShape="RoundRectangle 25"`, `BackgroundColor="#0AFFFFFF"`

### Probability Calculation
Always use `ProbabilityDisplayHelper.CalculateAuroraProbability(kp, latitude, cloudCoverage)`. Do NOT use `AuroraService.CalculateProbability` (private, different formula, kept for internal use only).

### File Structure
```
AuroraFix/
  Models/         — Plain data classes (no logic beyond simple helpers)
  Services/       — External API calls, HttpClient usage
  ViewModels/     — MVVM ViewModels (CommunityToolkit.Mvvm)
  Views/          — XAML pages and controls
  Helpers/        — Reusable calculation/display helpers
  Platforms/      — Platform-specific code
  Resources/      — Fonts, images, raw assets
```

## Anti-Patterns

- **Don't add logic to XAML code-behind** — keep `*.xaml.cs` minimal (only UI event wiring like `Completed="OnSearchClicked"`)
- **Don't create new HttpClient instances per-request** — reuse service-level instances
- **Don't write to UI from background threads** — use `MainThread.InvokeOnMainThreadAsync` if needed
- **Don't use AuroraService's private CalculateProbability** — use ProbabilityDisplayHelper instead

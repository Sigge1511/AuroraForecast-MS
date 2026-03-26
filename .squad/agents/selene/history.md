# Selene — Frontend Engineer

## 2026-03-24: Error overlay/toast implemented
- Added a styled error overlay to MainPage.xaml, matching aurora design tokens (dark scrim, card, Montserrat, teal accent, rounded corners)
- Overlay auto-dismisses after 4s (timer/cancellation logic in BaseViewModel)
- OK button (DismissErrorCommand) for immediate dismiss
- All logic ViewModel-driven, no code-behind


## Project: AuroraFix (seeded 2026-03-21)

**Namespace:** AuroraFix · .NET MAUI net10.0
**UI file:** `Views/MainPage.xaml` (single page)

**Design tokens:**
- Background: `#050810` (dark space)
- Accent: `#2DCCAA` (aurora green)
- Text: White, `#60FFFFFF` (muted), `#20FFFFFF` (very muted)
- Fonts: `Montserrat` (regular), `MontserratBold`
- Animated GIF background: `giphy.gif` at Opacity=0.5 + gradient overlay
- Loading overlay: `#D9050810` with `#2DCCAA` ActivityIndicator

**UI sections (top to bottom):**
1. Header — "AURORA FORECAST", CharacterSpacing=6
2. Search bar — Entry (CityName) + "GO" Button (SearchCityCommand)
3. Probability circle — 300×300 SVG arc, StrokeDashArray=StrokeDashValues, teal on dark base
4. Circle center — "PROBABILITY", large `{Probability:F0}%` (FontSize=90 with green glow shadow), ActivityLevel, KP INDEX
5. More info card — RoundRectangle border `#0AFFFFFF`, ActivityDescription text
6. 3-day forecast — CollectionView, each row: emoji icon + date + Kp + probability%
7. Footer — "SOURCE: NOAA SPACE WEATHER"
8. Loading overlay — full screen when IsBusy=true

**Key bindings:** CityName, Probability, ActivityLevel, CurrentKpIndex, ActivityDescription, StrokeDashValues, IsDataLoaded, IsBusy, ThreeDayForecast

**BindingContext:** Set directly in XAML `<viewmodel:MainPageViewModel />` (not via DI)
**Shell.NavBarIsVisible="False"** — no navigation bar

## Learnings

### 2026-03-21: Å/Ä/Ö cannot be typed — root cause was missing UTF-8 activeCodePage in app.manifest

**Symptom:** Swedish special characters (Å, Ä, Ö) and other non-ASCII characters could not be entered in the city search Entry on Windows.

**What I checked and ruled out:**
- XAML Entry: no `Keyboard` attribute, no Behaviors, no Effects, no MaxLength, no `<OnPlatform>` blocks — clean.
- ViewModel: `[ObservableProperty] private string cityName` with no `OnCityNameChanging`/`OnCityNameChanged` methods — no coercion.
- No SearchBar (it's an Entry) — handler is standard.
- `MainPage.xaml.cs` platform handler: only sets `BorderThickness` and a theme resource (purely visual, no input effect).
- No `<windows:TextBox.InputScope>` override.
- `GeocodingService.cs`: already in correct state from Hecate's fix (Lat/Lon as string, InvariantCultureIgnoreCase).

**Actual root cause:** `Platforms/Windows/app.manifest` was missing the `<activeCodePage>UTF-8</activeCodePage>` declaration. Without it, Windows uses the system ANSI code page for Win32 keyboard input (WM_CHAR messages). On systems where the ANSI code page is not Windows-1252 (e.g. Japanese CP932, Chinese CP936), characters like Å (U+00C5) are silently dropped at the Win32 message layer before they ever reach the WinUI TextBox — so the character simply never appears. Even on CP1252 systems, omitting this is fragile for users with non-Western locales.

**Fix applied:** Added `<activeCodePage xmlns="http://schemas.microsoft.com/SMI/2019/WindowsSettings">UTF-8</activeCodePage>` inside `<windowsSettings>` in `app.manifest`. This instructs Windows to use UTF-8 as the process code page, so all Unicode characters flow through WM_CHAR correctly regardless of system locale.

**Key lesson:** For any .NET MAUI Windows app that accepts user text input, the UTF-8 activeCodePage manifest entry is mandatory — not optional. It is the Windows-documented solution for full Unicode input support. The MAUI/WinUI layer is Unicode-capable, but the Win32 keyboard input layer beneath it is gated by the process code page.

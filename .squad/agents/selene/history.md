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

### 2026-03-29: Circle content collision fix

**Problem:** KP INDEX, KP value, and probability helper labels were colliding with the ring arc strokes.

**Root cause (calculated):** `FontSizeProbabilityHuge` had been bumped to 96 (previously noted as ~90 — actually 96). Stack content height ≈ 24 + 120 + 8 + 53 + 27 = 232px inside a ring with inner diameter 288px. With TranslationY=15 the stack bottom sat at y≈291 — only ~13px clearance before the inner ring stroke edge at y=304. Android's font rendering pads lines slightly, consuming that margin and causing visible overflow/collision.

**Fix applied:**
- Grid 320×320 → 360×360; arc center updated from (160,160) to (180,180): `StartPoint="180,30"`, `Point="179.9,30"`, `Size="150,150"` unchanged (same 150px radius)
- `FontSizeProbabilityHuge` 96 → 78 in Colors.xaml (still visually dominant; reduces stack height by ~23px)
- KP INDEX `Margin` top: 26 → 14 (saves 12px)
- Grid bottom `Margin` -25 → -45 (compensates for 40px taller grid so MORE card stays same visual distance)

**Post-fix clearance:** Stack height ≈ 197px, inner diameter still 288px, center at (180, 195 with TranslationY). Bottom clearance ≈ 30px, top ≈ 54px — comfortable breathing room on both sides.

**Key lesson:** When the ring grid size is increased without updating arc coordinates, the arc goes off-center (grid center ≠ arc center). Always update `StartPoint` and `Point` together: new top = (cx, cy − radius). Increase the negative bottom grid margin by the same amount the grid height grows to keep downstream visual spacing constant.


### 2026-03-28: GIF shake fix v2 — native Android window background

**Problem:** The AbsoluteLayout wrapper from v1 was insufficient — shake persisted because even though layout cascading was reduced, MAUI still processed invalidate() calls from the GIF's animated drawable.

**Root cause (deeper):** Even inside an AbsoluteLayout with proportional bounds, MAUI's `AnimatedDrawable` on Android calls `invalidateSelf()` which propagates through MAUI's internal `InvalidateDrawable` callback chain. The AbsoluteLayout absorbs `InvalidateMeasure` but not `InvalidateDraw` — so repaint cycles still run on the MAUI compositor at GIF framerate, causing jitter.

**Real fix:** Move the GIF completely out of MAUI. Render it as the Android Activity `Window.SetBackgroundDrawable()`. Animation frames call `invalidate()` only on the Window's decor-view background — a native Android layer that is entirely below MAUI's SurfaceView. MAUI layout and draw are never touched.

**Implementation:**
- Moved `giphy.gif` from `Resources/Images/` (MauiImage) → `Resources/Raw/` (MauiAsset) so it's in Android assets and accessible via `Assets.Open("giphy.gif")`
- `MainActivity.OnCreate` reads the GIF bytes, wraps in `Java.Nio.ByteBuffer`, decodes via `Android.Graphics.ImageDecoder.CreateSource(ByteBuffer)` + `DecodeDrawable()` → casts to `AnimatedImageDrawable` (all API 28+)
- Sets `Alpha = 127` (50% opacity) and calls `Start()`
- Sets as window background via `Window.SetBackgroundDrawable()`
- API < 28 guard: `OperatingSystem.IsAndroidVersionAtLeast(28)` + try/catch → graceful dark fallback
- Removed `<AbsoluteLayout>` + `<Image Source="giphy.gif">` from MainPage.xaml
- Set `ContentPage BackgroundColor="Transparent"` and root `Grid BackgroundColor="Transparent"` so the native window GIF shows through
- Kept the `BoxView` `LinearGradientBrush` overlay intact — it still provides the top/bottom dark fade

**`ImageDecoder` namespace:** It's `Android.Graphics.ImageDecoder`, NOT `Android.Graphics.Drawables.ImageDecoder`. `AnimatedImageDrawable` is in `Android.Graphics.Drawables`.

**Key lesson:** `AbsoluteLayout` absorbs `InvalidateMeasure` propagation but NOT `InvalidateDraw`. For a true zero-shake animated background on Android, the drawable must live outside MAUI's view hierarchy entirely — at the Window background level. Use `Activity.Window.SetBackgroundDrawable(animatedImageDrawable)` from `OnCreate`.

### 2026-03-28: UI fixes and rebrand

- Fixed GIF-induced micro-shaking by wrapping giphy.gif in AbsoluteLayout (absorbs per-frame invalidations locally)
- Bumped KP INDEX label FontSize 18→21, top margin 18→26; Kp value label FontSize 18→21
- Renamed GUI header "AURORA FORECAST" → "AURORA CATCHER" in MainPage.xaml
- All work committed and pushed to squad/10-gui-improvements


### 2026-03-28: App renamed from "Aurora Forecast" to "Aurora Catcher" in UI

**Task:** Rename the visible app title in the GUI.

**Changes made:**
- `Views/MainPage.xaml` line 38: `Text="AURORA FORECAST"` → `Text="AURORA CATCHER"` (the header Label with `CharacterSpacing=6`, `FontFamily="MontserratBold"`, teal colour).
- No other visible UI text occurrences of "Aurora Forecast" / "AURORA FORECAST" existed in any XAML file (confirmed by grep across all `.xaml` files).

**What was NOT changed:** Code comments, variable names, namespace names, `history.md` prose references to the old name, or the GitHub repo name — only the rendered UI string.

**Key lesson:** Before renaming any visible string, grep all XAML files for both the title-case and all-caps variants. In this project, the header uses all-caps (`AURORA FORECAST`) while narrative documentation uses title-case — a single grep with `-i` catches both in one pass.

### 2026-03-28: KP INDEX font bump + spacing tweak in circle center

**Task:** Increase visual air between the large probability % number and the circle stroke ring, by elevating the KP INDEX block (larger font + more top margin).

**Changes made:**
- `Text="KP INDEX: "` label: `FontSize` changed from `{StaticResource FontSizeLabel}` (18) → inline `21`. `Margin` changed from `0,18,0,0` → `0,26,0,0` (top margin +8pt).
- `Text="{Binding CurrentKpIndex, ...}"` label: `FontSize` changed from `{StaticResource FontSizeLabel}` (18) → inline `21`.

**Why inline instead of changing the resource?** `FontSizeLabel` (18) is also used by the "PROBABILITY" header label — changing the resource would unintentionally enlarge that label too. Surgical inline override was the right call.

**Key lesson:** When multiple labels share a font-size token but only *some* need resizing, prefer inline overrides over changing the shared resource. Use the resource only if the change should apply everywhere the token is used.

### 2026-03-28: Micro-shaking on Android — animated GIF layout thrashing

**Symptom:** Sigge reported visual "micro-shaking" / trembling of all UI elements on the Android emulator at all times.

**What was ruled out:**
- ViewModel: no timers, no periodic property updates — all updates are one-shot on user search action.
- BaseViewModel: `AutoClearErrorAsync` fires once on error and uses `Task.Delay(4000)` — no polling loop.
- Shadow on Probability Label: only repaints when `Probability` changes (on search), not continuously.
- ScrollView: `VerticalScrollBarVisibility="Never"` is safe.
- Bindings: no binding-driven sizes/positions in AbsoluteLayout.

**Actual root cause:** `<Image Source="giphy.gif" IsAnimationPlaying="True" />` placed directly in the root `Grid`. On Android, MAUI's `Image` control with animated GIFs calls `InvalidateMeasure` on **every animation frame** (driven by Android's `AnimatedDrawable.invalidateSelf()`). Because `Grid` measures its children to determine its own size, this cascades up the full layout tree on every frame — the `Grid`, `ScrollView`, `VerticalStackLayout`, and all content children are re-measured/re-laid-out at animation speed → micro-shaking.

**Fix:** Wrapped the GIF `Image` in an `AbsoluteLayout` with `AbsoluteLayout.LayoutFlags="All"` and `AbsoluteLayout.LayoutBounds="0,0,1,1"`. An `AbsoluteLayout` using proportional bounds determines child positions from its *own* already-known size rather than measuring children. This means child `InvalidateMeasure` calls are absorbed locally inside the AbsoluteLayout and do **not** propagate up to the parent Grid — breaking the layout cascade and eliminating the shaking. The GIF remains animated and visually identical.

**Key lesson:** Never place an animated GIF `Image` directly in a layout container that measures children (`Grid`, `StackLayout`, `FlexLayout`). Always isolate it in an `AbsoluteLayout` with proportional bounds, or use a `MediaElement` if `CommunityToolkit.Maui` is available.

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

### 2026-03-27: Issue #10 — Circle centering, forecast dimming, text fit

**Circle off-center (Issue #10.1)**
Root cause: Both `PathFigure` elements used `StartPoint="140,10"` with `Size="130,130"`. The arc centre is at `(StartPoint.X, StartPoint.Y + Size.Height)` = (140, 140). The Grid was 300×300 with true centre at (150,150) — 10px left in both X and Y. Additionally, the inner `VerticalStackLayout` had `Margin="-10,3,0,0"` which further shifted text left by 10px.

Fix applied: Grid resized to 320×320. Both paths updated to `StartPoint="160,25"`, `Size="135,135"`, `Point="159.9,25"`. Arc centre = (160, 25+135) = (160,160) = exact grid centre. `Margin="-10,3,0,0"` removed; replaced with clean `HorizontalOptions="Center"` / `VerticalOptions="Center"`.

**Forecast greyed out (Issue #10.2)**
Root cause: `CollectionView` had no `SelectionMode` or `BackgroundColor` set. On some renderers the CollectionView container and per-item DataTemplate Grid inherit platform-default selection-state backgrounds or fills that sit over the content as a semi-opaque overlay, making emoji icons and labels appear dimmer. There was no explicit opacity on any parent element — the dimming was purely renderer-level.

Fix applied: Added `SelectionMode="None"` and `BackgroundColor="Transparent"` to the `CollectionView`. Added `BackgroundColor="Transparent"` to the DataTemplate `Grid` as well (River R1 catch — item containers can still fill even when the collection view is transparent). The intentional `Opacity="0.7"` on the secondary KP INDEX label was preserved.

**Text too tight in ring (Issue #10.3)**
Root cause: Old 300×300 grid with radius 130 gave inner diameter 2×(130-6)=248px. The text stack (PROBABILITY, 90-size %, ActivityLevel, KP INDEX) was cramped inside that diameter.

Fix applied: Grid 320×320 with radius 135 → inner diameter 2×(135-6)=258px. Bottom margin corrected from -30 to -25 to realign MORE card with new circle bottom position (y=295 in new geometry vs y=270 in old geometry).

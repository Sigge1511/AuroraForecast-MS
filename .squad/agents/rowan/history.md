# Rowan — DevOps Engineer

## Project: AuroraFix (seeded 2026-03-21)

**Solution:** AuroraForecast.slnx → AuroraFix/AuroraForecast.csproj
**Framework:** .NET MAUI net10.0

**Build targets:**
- `net10.0-android` (always, min API 21)
- `net10.0-ios` + `net10.0-maccatalyst` (non-Linux, min 15.0)
- `net10.0-windows10.0.19041.0` (Windows only, min 10.0.17763.0)

**App config:**
- App ID: `com.companyname.aurorafix`
- Version: 1.0 / build 1
- `WindowsPackageType=None` (unpackaged)
- `MauiXamlInflator=SourceGen` (compile-time XAML for faster builds)

**NuGet packages:**
- `CommunityToolkit.Mvvm 8.4.0`
- `Microsoft.Maui.Controls $(MauiVersion)` — net10.0
- `Microsoft.Extensions.Logging.Debug 10.0.0`

**npm packages:**
- `quill ^2.0.3` (purpose unclear — may be unused or for a planned WebView feature)

**Resources:**
- App icon: `Resources/AppIcon/appicon.svg` (color #512BD4)
- Splash: `Resources/Splash/splash.svg` (#512BD4, 128×128)
- Fonts: Montserrat-Regular.ttf, Montserrat-Bold.ttf
- Animated GIF: `Resources/Images/giphy.gif`

**CI/CD:** None configured. No .github/workflows for build or deploy.

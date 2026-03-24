# 🌌 AuroraFix

*Real-time aurora forecasts, powered by science and open data*

<p align="left">
  <img src="https://img.shields.io/badge/.NET-10.0-blue"/>
  <img src="https://img.shields.io/badge/MAUI-Cross--Platform-brightgreen"/>
  <img src="https://img.shields.io/badge/License-MIT-yellow"/>
  <img src="https://img.shields.io/badge/Platforms-Windows%20·%20Android%20·%20iOS%20·%20macOS-lightgrey"/>
</p>

<table>
  <tr>
    <td><img src="https://github.com/user-attachments/assets/29fcc039-1bd2-4d64-8da4-57e3acbe75e3" width="300"/></td>
    <td><img src="https://github.com/user-attachments/assets/d0e6f8b6-61e4-4da1-981c-2cca4f8b48b5" width="300"/></td>
    <td><img src="https://github.com/user-attachments/assets/3dcb6be9-ebf4-401c-b1bb-21c5a1272824" width="300"/></td>
  </tr>
</table>

## ✨ Features

- 🌍 Search any city worldwide for aurora forecasts
- 📊 Real-time Kp-index from NOAA
- ☁️ Weather integration (Open-Meteo)
- 📅 3-day combined aurora + weather forecast
- 🎯 Probability calculation (latitude + weather)
- 🧮 Animated probability display
- 🗺️ Nordic city presets (Östersund, Kiruna, Tromsø, Reykjavik, Stockholm, Oslo, Göteborg, Malmö)
- 🖥️ Cross-platform: Windows, Android, iOS, macOS

## How It Works
### The Probability Formula
```
Actual Viewing Probability = Base Aurora Probability - Cloud Penalty
```
- **Base Aurora Probability**: Calculated from current Kp-index and your latitude
- **Cloud Penalty**: Calculated from real-time cloud coverage

#### Example Scenarios
**Perfect Conditions**
```
Location: Kiruna, Sweden (67.86°N)
Kp-index: 3.0
Cloud coverage: 5%
→ Base probability: 70%
→ Cloud penalty: 0%
→ Actual: 70% ⭐
```
**High Aurora, Bad Weather**
```
Location: Kiruna, Sweden (67.86°N)
Kp-index: 5.0
Cloud coverage: 80%
→ Base probability: 90%
→ Cloud penalty: -80%
→ Actual: 10% ☁️☁️
```


### The Magic Formula

```
Actual Viewing Probability = Base Aurora Probability - Cloud Penalty
```

**Base Aurora Probability** is calculated from:
- Current Kp-index (geomagnetic activity)
- Your latitude (further north = better chances)

**Cloud Penalty** is calculated from:
- Current cloud coverage percentage
- Clear skies (< 5% clouds) = No penalty ⭐
- Partly cloudy (< 20%) = Small penalty 🌟
- Mostly cloudy (< 50%) = Medium penalty ☁️
- Overcast (≥ 50%) = Large penalty ☁️☁️

### Example Scenarios

#### ✅ Perfect Conditions
```
Location: Kiruna, Sweden (67.86°N)
Kp-index: 3.0
Cloud coverage: 5%

→ Base probability: 70%
→ Cloud penalty: 0%
→ Actual: 70% ⭐
```

#### ⚠️ High Aurora, Bad Weather
```
Location: Kiruna, Sweden (67.86°N)
Kp-index: 5.0 (Storm!)
Cloud coverage: 80%

→ Base probability: 90%
→ Cloud penalty: -80%
→ Actual: 10% ☁️☁️
```
*Aurora is likely happening, but you won't see it through the clouds!*

## Architecture

### Clean Separation of Concerns

```
┌──────────────────────────────────────┐
│  VIEW (XAML + ViewModel)             │
│  - Displays data to user             │
│  - Handles user interactions         │
└──────────────┬───────────────────────┘
               │
┌──────────────▼───────────────────────┐
│  SERVICES                            │
│  - AuroraService (NOAA API)          │
│  - WeatherService (Open-Meteo API)   │
│  - GeocodingService (OSM)            │
└──────────────┬───────────────────────┘
               │
┌──────────────▼───────────────────────┐
│  HELPERS                             │
│  - ProbabilityDisplayHelper          │
│    • Calculates aurora probability   │
│    • Adjusts for cloud coverage      │
└──────────────┬───────────────────────┘
               │
┌──────────────▼───────────────────────┐
│  MODELS (Pure Data)                  │
│  - AuroraForecast                    │
│  - Weather                           │
│  - ForecastDay                       │
└──────────────────────────────────────┘
```

## Data Flow

```mermaid
flowchart LR
    A[User Input] --> B[Geocoding]
    B --> C[Get Coordinates]
    C --> D{Parallel Fetch}
    D --> E[NOAA: Kp Data]
    D --> F[Open-Meteo: Weather]
    E --> G[Calculate Probability]
    F --> G
    G --> H[Display Results]
```

**Detailed Flow:**
1. User searches for a city (e.g., "Tromsø")
2. Geocoding service converts city → coordinates
3. **Parallel API calls:**
   - Fetch Kp-index from NOAA Space Weather API
   - Fetch cloud coverage from Open-Meteo API
4. Calculate actual viewing probability (Kp + latitude - clouds)
5. Display results with weather context
6. Fetch 3-day forecasts (aurora + weather combined)

## APIs Used

| Service | API | Purpose | Cost |
|---------|-----|---------|------|
| Aurora Data | [NOAA Space Weather](https://www.swpc.noaa.gov/) | Real-time Kp-index & forecasts | Free ✅ |
| Weather Data | [Open-Meteo](https://open-meteo.com/) | Cloud coverage & forecasts | Free ✅ |
| Geocoding | [OpenStreetMap Nominatim](https://nominatim.org/) | City name → coordinates | Free ✅ |

*All APIs are free and require no API keys!*

## 📊 Probability Calculation Details

### Cloud Coverage Impact

Cloud coverage directly reduces your viewing probability:

| Cloud Coverage | Penalty | Viewing Condition |
|----------------|---------|-------------------|
| 0-5% | 0% | ⭐ Perfect - Clear skies |
| 5-20% | -2 to -5% | 🌟 Good - Partly cloudy |
| 20-50% | -10 to -35% | ☁️ Difficult - Mostly cloudy |
| 50-70% | -50 to -65% | ☁️ Very difficult |
| 70-100% | -65 to -80% | ☁️☁️ Nearly impossible |

### Latitude Adjustment

Your latitude affects base probability:

| Latitude | Adjustment |
|----------|------------|
| Above 65°N | +20% bonus (Arctic) |
| 55-65°N | +10% bonus (Northern Europe) |
| 45-55°N | No adjustment |
| Below 45°N | -20% penalty (Rare sightings) |

## 🛠️ Tech Stack

- **.NET MAUI** - Cross-platform app framework
- **MVVM Pattern** - Clean architecture with CommunityToolkit.Mvvm
- **HttpClient** - RESTful API calls
- **ObservableCollections** - Reactive UI updates
- **Singleton Pattern** - Efficient service management

## 📱 Platforms

- ✅ Windows
- ✅ Android
- ✅ iOS (untested)
- ✅ macOS (untested)

## 🛠️ Getting Started

```bash
# Clone the repository
git clone https://github.com/Sigge1511/AuroraForecast-MS

# Open in Visual Studio 2022 or 2026
# Select target platform (Windows/Android)
# Build and run!
```

**Requirements:**
- .NET 10 SDK
- Visual Studio 2022 or 2026 with MAUI workload

## Contributing
We welcome contributions! The active development branch is `Refactor-and-codereview`. Please submit pull requests against this branch. All improvements, bug fixes, and feature suggestions are appreciated.

Contributions are welcome! Please open an issue or submit a pull request.

## Acknowledgments

- **NOAA Space Weather Prediction Center** - Aurora data
- **Open-Meteo** - Weather data
- **OpenStreetMap** - Geocoding services

---

**Made with ❤️ for aurora chasers worldwide** 🌌⭐

*Never miss the Northern Lights again!*

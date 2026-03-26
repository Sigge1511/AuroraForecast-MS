# 🌌 AuroraFix

*Chase the Northern Lights with confidence — real-time aurora forecasts powered by live space weather data*

<p align="left">
  <img src="https://img.shields.io/badge/.NET-10.0-blue"/>
  <img src="https://img.shields.io/badge/MAUI-Cross--Platform-brightgreen"/>
  <img src="https://img.shields.io/badge/License-All%20Rights%20Reserved-red"/>
  <img src="https://img.shields.io/badge/Platforms-Windows%20·%20Android%20·%20iOS%20·%20macOS-lightgrey"/>
</p>

<table>
  <tr>
    <td align="center">
      <img src="https://github.com/user-attachments/assets/29fcc039-1bd2-4d64-8da4-57e3acbe75e3" width="300"/><br/>
      <sub><b>🤖 AI-generated UI — Claude's first draft</b></sub>
    </td>
    <td align="center">
      <img src="https://github.com/user-attachments/assets/d0e6f8b6-61e4-4da1-981c-2cca4f8b48b5" width="300"/><br/>
      <sub><b>✨ Developer redesign — polished by hand</b></sub>
    </td>
    <td align="center">
      <img src="https://github.com/user-attachments/assets/3dcb6be9-ebf4-401c-b1bb-21c5a1272824" width="300"/><br/>
      <sub><b>✨ Developer redesign — forecast view</b></sub>
    </td>
  </tr>
</table>

---

## 🤖 Born from an Experiment

AuroraFix started as a bold experiment: *what happens when you let an AI build an entire app from scratch?*

The backend — the data pipeline, probability engine, and API integrations — was architected by Claude (Anthropic's AI assistant) as part of a school assignment. The result was surprisingly solid. The logic worked. The math was right. The architecture was clean.

But the UI? That needed a human touch.

The entire frontend was then torn down and rebuilt by the developer — every screen, every animation, every detail crafted to feel as magical as the aurora itself. What started as an AI experiment became a proper app, and it's still growing.

The best of both worlds: machine precision meets human design. 🌌

---

## ✨ Features

- 🌍 Search any city worldwide for real-time aurora forecasts
- 📊 Live Kp-index data straight from NOAA's Space Weather Prediction Center
- ☁️ Cloud coverage integration via Open-Meteo
- 📅 3-day combined aurora + weather outlook
- 🎯 Smart probability engine (latitude × Kp × weather)
- 🧮 Animated probability display
- 🗺️ Nordic city quick-picks: Östersund, Kiruna, Tromsø, Reykjavik, Stockholm, Oslo, Göteborg, Malmö
- 🖥️ Cross-platform: Windows, Android, iOS, macOS

---

## 📡 Powered by NOAA

The aurora data in AuroraFix comes directly from **NOAA** — the National Oceanic and Atmospheric Administration, a U.S. federal science agency. Their **Space Weather Prediction Center (SWPC)** in Boulder, Colorado operates 24/7, monitoring the Sun and Earth's magnetic environment in real time.

The key metric AuroraFix uses is the **Kp-index** — the *planetary geomagnetic disturbance index* — a 0–9 scale measuring how disturbed Earth's magnetic field is. A high Kp means solar wind is actively interacting with the magnetosphere. A Kp of 5 or above typically signals aurora visible at mid-latitudes. At 8–9, the lights can reach southern Europe and even parts of the US.

Every forecast in AuroraFix is built on this live NOAA stream. No guessing. No delays. Just space weather, straight from the source.

---

## 🧮 The Magic Formula

```
Actual Viewing Probability = Base Aurora Probability − Cloud Penalty
```

**Base Aurora Probability** is calculated from:
- Current Kp-index (geomagnetic activity level, 0–9)
- Your latitude (further north = better chances)

**Cloud Penalty** reduces your probability based on real-time sky conditions:

| Cloud Coverage | Penalty | Viewing Condition |
|----------------|---------|-------------------|
| 0–5% | 0% | ⭐ Perfect — clear skies |
| 5–20% | −2 to −5% | 🌟 Good — partly cloudy |
| 20–50% | −10 to −35% | ☁️ Difficult — mostly cloudy |
| 50–70% | −50 to −65% | ☁️ Very difficult |
| 70–100% | −65 to −80% | ☁️☁️ Nearly impossible |

### Example Scenarios

#### ✅ Perfect Night
```
Location: Kiruna, Sweden (67.86°N)
Kp-index: 3.0 | Cloud coverage: 5%
→ Base: 70%  → Penalty: 0%  → Actual: 70% ⭐
```

#### ⚠️ Aurora Active, Sky Closed
```
Location: Kiruna, Sweden (67.86°N)
Kp-index: 5.0 (Storm!) | Cloud coverage: 80%
→ Base: 90%  → Penalty: −80%  → Actual: 10% ☁️☁️
```
*The lights are dancing — you just can't see them through the clouds.*

---

## 🏗️ Architecture

AuroraFix is built with a clean MVVM separation of concerns. Each layer does exactly one thing.

```
┌──────────────────────────────────────┐
│  VIEW  (XAML + ViewModel)            │
│  Displays data · Handles interaction │
└──────────────┬───────────────────────┘
               │
┌──────────────▼───────────────────────┐
│  SERVICES                            │
│  AuroraService   → NOAA API          │
│  WeatherService  → Open-Meteo API    │
│  GeocodingService → OSM Nominatim    │
└──────────────┬───────────────────────┘
               │
┌──────────────▼───────────────────────┐
│  HELPERS                             │
│  ProbabilityDisplayHelper            │
│    · Aurora probability calc         │
│    · Cloud coverage adjustment       │
└──────────────┬───────────────────────┘
               │
┌──────────────▼───────────────────────┐
│  MODELS  (Pure Data)                 │
│  AuroraForecast · Weather            │
│  ForecastDay                         │
└──────────────────────────────────────┘
```

---

## 🔄 Data Flow

```mermaid
flowchart TD
    A["🌍 User searches city"] --> B["📍 Geocoding\n(OSM Nominatim)"]
    B --> C["🗺️ Coordinates resolved"]
    C --> D{{"⚡ Parallel API fetch"}}
    D --> E["📡 NOAA SWPC\nKp-index + forecast"]
    D --> F["☁️ Open-Meteo\nCloud coverage + weather"]
    E --> G["🧮 Probability engine\nKp × latitude − clouds"]
    F --> G
    G --> H["📊 Display results\n+ 3-day outlook"]
```

**Step by step:**
1. User searches for a city (e.g. "Tromsø")
2. Geocoding service resolves it to GPS coordinates
3. Two API calls fire in parallel:
   - NOAA SWPC → current Kp-index + 3-day forecast
   - Open-Meteo → cloud coverage + weather data
4. Probability engine combines Kp, latitude, and cloud penalty
5. Results are displayed with animated probability and forecast cards

---

## 🌐 APIs

All three APIs are completely free, open, and require no API keys.

| Service | Provider | Purpose |
|---------|----------|---------|
| ☀️ Aurora & Kp data | [NOAA SWPC](https://www.swpc.noaa.gov/) | Real-time space weather, Kp-index, 3-day forecasts |
| ☁️ Weather & clouds | [Open-Meteo](https://open-meteo.com/) | Cloud coverage, hourly + daily weather |
| 📍 Geocoding | [OpenStreetMap Nominatim](https://nominatim.org/) | City name → GPS coordinates |

---

## 📐 Probability Details

### Latitude Bonus/Penalty

| Latitude | Adjustment |
|----------|------------|
| Above 65°N | +20% (Arctic prime zone) |
| 55–65°N | +10% (Northern Europe) |
| 45–55°N | ±0% (baseline) |
| Below 45°N | −20% (rare sightings only) |

---

## 🛠️ Tech Stack

- **.NET MAUI** — Cross-platform framework (Windows, Android, iOS, macOS)
- **MVVM + CommunityToolkit.Mvvm** — Clean, reactive architecture
- **HttpClient** — Lightweight RESTful API communication
- **ObservableCollections** — Live UI updates without boilerplate
- **Singleton services** — Efficient, shared API clients

---

## 📱 Platform Support

| Platform | Status |
|----------|--------|
| Windows | ✅ Fully supported |
| Android | ✅ Fully supported |
| iOS | ✅ Builds (untested on device) |
| macOS | ✅ Builds (untested on device) |

---

## 🚀 Getting Started

```bash
# Clone the repo
git clone https://github.com/Sigge1511/AuroraForecast-MS

# Open AuroraForecast.slnx in Visual Studio 2022 or 2026
# Select your target platform (Windows / Android)
# Hit Run — no API keys needed
```

**Requirements:**
- .NET 10 SDK
- Visual Studio 2022+ with the .NET MAUI workload installed

---

## 🙏 Acknowledgments

- **[NOAA Space Weather Prediction Center](https://www.swpc.noaa.gov/)** — The backbone of every forecast
- **[Open-Meteo](https://open-meteo.com/)** — Beautiful open weather API
- **[OpenStreetMap / Nominatim](https://nominatim.org/)** — Open geocoding for the world

---

<p align="center">
  <b>© 2025 Sigge1511. All Rights Reserved.</b><br/>
  <i>This software is proprietary. No license is granted for use, copying, modification, or distribution.</i>
</p>

<p align="center">
  Made with ❤️ for aurora chasers everywhere 🌌⭐<br/>
  <i>Never miss the Northern Lights again.</i>
</p>

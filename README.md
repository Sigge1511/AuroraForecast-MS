# 🌌 Aurora Forecast

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
      <sub><b>✨ My redesign — polished by hand</b></sub>
    </td>
    <td align="center">
      <img src="https://github.com/user-attachments/assets/3dcb6be9-ebf4-401c-b1bb-21c5a1272824" width="300"/><br/>
      <sub><b>✨ My redesign — forecast view</b></sub>
    </td>
  </tr>
</table>

---

## 🤖 From Experiment to App

Aurora Forecast started as an experiment: *could an AI build a functional weather app?* While the initial data logic was solid, the user experience required a human touch. I rebuilt the entire frontend from scratch—crafting every animation and detail to ensure the app feels as magical as the aurora itself.

---

## ✨ Key Features

- 🌍 **Global Search:** Find real-time aurora forecasts for any city worldwide.
- 📊 **Live NOAA Data:** Direct integration with the Space Weather Prediction Center.
- ☁️ **Weather Awareness:** Cloud coverage data via Open-Meteo.
- 🎯 **Smart Probability Engine:** Custom algorithm (Latitude × Kp × Cloud Penalty).
- 🛡️ **Reliability:** Graceful error handling with one-tap retry functionality.

---

## 📡 Powered by NOAA

The aurora data in this app comes directly from **NOAA** (National Oceanic and Atmospheric Administration). Their **Space Weather Prediction Center (SWPC)** monitors the Sun-Earth environment 24/7.

The app tracks the **Kp-index** (0–9 scale). A higher Kp indicates stronger geomagnetic activity. 
> **Technical Note:** To ensure accuracy during 3-hour boundary resets, the app reports the **peak Kp recorded over the last 30 minutes**.

---

## 🏗️ Architecture

The app follows a clean MVVM pattern to ensure maintainability and performance.

```mermaid
flowchart TD
    A["⚛️ VIEW\nXAML + ViewModel\nDisplays data · Interaction"]
    B["⚙️ SERVICES\nAuroraService → NOAA API\nWeatherService → Open-Meteo\nGeocodingService → OSM"]
    C["🧮 HELPERS\nProbabilityDisplayHelper\nCalculations · Cloud adjustment"]
    D["📦 MODELS\nAuroraForecast · Weather\nPure data containers"]

    A --> B
    B --> C
    C --> D

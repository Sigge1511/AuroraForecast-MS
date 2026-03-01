# Aurora Forecast App 

## AI-First Approach

This is my first .NET MAUI application and my first project ever developed using an "AI-First" workflow. It is meant to be a cross-platform app that provides real-time aurora forecasts based on the NOAA Space Weather API, with calculations of probability for sightings based on the locations latitude.

### Technical Stack
.NET 9.0 ⚡

MAUI (Multi-platform App UI)

MVVM Pattern using CommunityToolkit.Mvvm

NOAA Space Weather API

CommunityToolkit.Maui.MediaElement for video playback


### Project Structure

Plaintext

AuroraForecast/

├── Models/             # Data models

├── ViewModels/         # MVVM ViewModels logic

├── Views/              # UI Layouts (XAML)

├── Services/           # API Integration & Business logic

├── Resources/

│   ├── Raw/            # Place files here

│   └── Styles/         # Global styles and colors

└── AuroraForecast.csproj

### Features
[x] MVVM Architecture – Clean separation of concerns.

[x] NOAA API Integration – Real-time space weather data.

[x] Dynamic Video Background – Visuals react to solar activity.

[x] 3-Day Forecast – Plan your aurora hunting in advance.

[x] City Search – Location-based results via geocoding.

[x] Async/Await – Smooth, non-blocking UI.

[x] Robust Error Handling – Gracious failures for API or network issues.

### Testing & Logic
Test with different cities:
Kiruna, SE → High probability often

Uppsala, SE → Medium probability often

Paris, FR → Lower probability often

Moving Background:
The background gif moves slowly in the background to add the right feeling and has a slight transparency to be more discret.


### Grading Criteria Checklist

[x] API Integration (NOAA)

[x] MVVM Pattern

[x] Async/Await implementation

[x] Data Binding

[x] Commands (ICommand/RelayCommand)

[x] Error Handling

[x] Professional UI/UX

[x] Innovation (Dynamic Video logic)

For educational purposes. NOAA data is in the public domain.

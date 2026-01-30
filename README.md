# Aurora Forecast App 🌌 (.NET 9)

En .NET MAUI-app som visar norrskensprognos baserat på NOAA Space Weather API med dynamisk videobakgrund.

## Teknisk Stack
- **.NET 9.0** ⚡
- **MAUI (Multi-platform App UI)**
- **MVVM Pattern** med CommunityToolkit.Mvvm
- **NOAA Space Weather API**
- **CommunityToolkit.Maui.MediaElement**

## Snabbstart

### 1. Öppna projektet
Dubbelklicka på `AuroraForecast.csproj` i Visual Studio 2022

### 2. Lägg till videor (valfritt)
Gå till `Resources/Raw/` och lägg till:
- aurora_low.mp4
- aurora_medium.mp4
- aurora_active.mp4
- aurora_storm.mp4

**Tips:** Hitta gratis videor på Pexels.com

### 3. Välj plattform
- **Windows** (snabbast för testning!)
- Android
- iOS (Mac only)

### 4. Tryck F5

Appen startar med Uppsala som default och hämtar realtidsdata från NOAA!

## NuGet Packages (.NET 9)
```xml
<PackageReference Include="CommunityToolkit.Maui" Version="9.1.1" />
<PackageReference Include="CommunityToolkit.Maui.MediaElement" Version="4.1.1" />
<PackageReference Include="CommunityToolkit.Mvvm" Version="8.3.2" />
```

## Projektstruktur
```
AuroraForecast/
├── Models/                  # Datamodeller
├── ViewModels/              # MVVM ViewModels
├── Views/                   # UI (XAML)
├── Services/                # API integration
├── Resources/
│   ├── Raw/                 # Videofiler här!
│   └── Styles/
└── AuroraForecast.csproj    # .NET 9 projekt
```

## Funktioner
✅ MVVM-arkitektur  
✅ NOAA API-integration  
✅ Dynamisk videobakgrund  
✅ 3-dagars prognos  
✅ Stadsökning med geocoding  
✅ Async/await  
✅ Error handling  

## Testning

### Test olika städer:
- **Kiruna** → Hög sannolikhet
- **Uppsala** → Medium sannolikhet  
- **Stockholm** → Lägre sannolikhet

### Dynamisk video:
Videon byter automatiskt baserat på Kp-index från NOAA!

## Felsökning

### "Cannot resolve CommunityToolkit"
```bash
dotnet restore
```

### Videon visas inte
- Kontrollera att videofilerna finns i `Resources/Raw/`
- Rätt filnamn: aurora_low.mp4, etc.
- Appen fungerar även utan videor!

## Bedömningskriterier

✅ API-integration (NOAA)  
✅ MVVM Pattern  
✅ Async/await  
✅ Databinding  
✅ Commands (ICommand)  
✅ Error handling  
✅ Professionell UI  
✅ Innovation (dynamisk video)  

## Licens
Educational purpose - NOAA data är public domain

**Lycka till med inlämningen på söndag! 🚀**

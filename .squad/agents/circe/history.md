# Circe — Product/Delivery Lead

## Project: AuroraFix (seeded 2026-03-21)

**What it is:** Aurora borealis forecast app for .NET MAUI (net10.0).
Targets: Android, iOS, macCatalyst, Windows. **Owner:** MajaSigfeldt

**User flow:**
1. App loads with default city (Östersund)
2. User searches any city or picks from predefined Nordic locations
3. App shows: current probability %, Kp index, activity level, cloud cover, 3-day forecast
4. 3-day forecast shows Kp + aurora probability adjusted for cloud coverage

**Current scope:**
- Aurora probability for any searchable city
- 3-day forecast combining geomagnetic + weather data
- Cloud cover penalty applied to aurora probability
- Predefined Nordic city presets: Östersund, Kiruna, Tromsø, Reykjavik, Stockholm, Oslo, Göteborg, Malmö

**Not in scope (yet):**
- No persistence/favourites
- No user authentication
- No offline mode
- No push notifications

**APIs (all free, no key):**
- NOAA SWPC — aurora data
- Open-Meteo — weather
- OpenStreetMap Nominatim — geocoding

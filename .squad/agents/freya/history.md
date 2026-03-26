# Freya — Technical Writer

## Project: AuroraFix (seeded 2026-03-21)

**What it is:** Aurora borealis (northern lights) forecast app for .NET MAUI (net10.0).
Targets Android, iOS, macCatalyst, Windows. Single-page app.

**Documentation state:** No README. No API docs. Code comments mix English and Swedish.

**User-facing description:**
AuroraFix helps you find the best chances of seeing the northern lights. Enter your city to get:
- Current aurora probability (0–100%)
- Kp index and activity level (VERY LOW → EXTREME)
- Cloud cover conditions
- 3-day forecast with cloud-adjusted probability

**Data sources:**
- **NOAA Space Weather Prediction Center** — real-time Kp index + 3-day geomagnetic forecast (free, no key)
- **Open-Meteo** — cloud coverage forecast (free, no key)
- **OpenStreetMap Nominatim** — city geocoding (free, no key)

**Predefined Nordic cities:** Östersund, Kiruna, Tromsø, Reykjavik, Stockholm, Oslo, Göteborg, Malmö

**Probability explained (for docs):**
Calculated from Kp index and your latitude. Higher northern latitudes see aurora more easily. Cloud coverage reduces the effective probability by a penalty (up to 80 points). Activity levels: VERY LOW / LOW / MODERATE / VERY HIGH / EXTREME.

namespace AuroraFix.Models;

public class AuroraForecast
{
    public DateTime ForecastTime { get; set; }
    public double KpIndex { get; set; }
    public string Location { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public int Probability { get; set; } // 0-100%
    public string ActivityLevel { get; set; } = string.Empty;
    
    public string GetActivityDescription(double probability, double kpIndex = 0, double cloudCoverage = 0, double baseProbability = -1)
    {
        // High solar activity but clouds blocking the view (KP >= 5)
        if (kpIndex >= 5 && cloudCoverage > 75)
            return "Strong aurora activity out there — but the sky is completely overcast tonight. Not your night, but the northern lights will be back. Come back another time!";

        if (kpIndex >= 5 && cloudCoverage > 50)
            return "High solar activity tonight! The aurora is active up there, but thick clouds are blocking the view. Keep checking back — it might clear up!";

        // Zero probability — explain the actual reason
        if (probability <= 0)
        {
            if (cloudCoverage > 75)
                return "The sky is fully overcast tonight — even if the aurora were dancing, you wouldn't see it. Try again on a clearer night!";

            if (baseProbability <= 0)
                return "The solar activity isn't strong enough to reach this far south right now. Keep an eye on the KP index — when it climbs, the aurora follows!";

            return "Cloudy skies are winning tonight. There's some solar activity out there, but the clouds are blocking everything. Check back later or try another night!";
        }

        // Clouds significantly reducing an otherwise good forecast
        bool cloudsSignificant = baseProbability > 0 && cloudCoverage > 40 && (baseProbability - probability) >= 20;
        if (cloudsSignificant)
        {
            return baseProbability switch
            {
                >= 75 => $"The aurora is active tonight — but thick clouds are in the way. Without them, you'd have a {baseProbability:F0}% chance! Check for gaps in the cloud cover.",
                >= 55 => $"Decent aurora potential up there, but clouds are holding you back. Clear skies would give you a {baseProbability:F0}% chance — keep an eye on the forecast!",
                _ => $"Some aurora activity tonight, but clouds are cutting your chances. Clear skies would give you a {baseProbability:F0}% shot — worth checking back!"
            };
        }

        // Standard messages for clear or lightly cloudy conditions
        return probability switch
        {
            <= 15 => "Very quiet up there. Low solar activity — don't expect much tonight. Good night for sleep!",
            <= 35 => "Low activity. You might catch a faint glow with a camera in very dark areas, but it's a tough night for the naked eye.",
            <= 55 => "Moderate chance! Worth heading somewhere dark and keeping a lookout — the aurora might show up!",
            <= 75 => "Good chance tonight! Active aurora likely. Find a dark spot, look north, and give your eyes time to adjust.",
            _ => "Excellent conditions! Strong aurora activity — even some light pollution won't stop the show. Look up!"
        };
    }
}

namespace AuroraFix.Helpers;

/// <summary>
/// Produces user-facing prose and formatted display strings for the aurora UI.
/// All methods are pure functions — no state.
/// </summary>
public static class GuiMessageHelper
{
    /// <summary>
    /// Returns a human-readable aurora activity description based on current conditions.
    /// Considers visibility (darkness/midnight sun), cloud cover, and probability.
    /// </summary>
    public static string GetActivityDescription(
        double probability,
        double kpIndex = 0,
        double cloudCoverage = 0,
        double? baseProbability = null,
        bool isDark = true,
        DateTime? darkFrom = null,
        bool isMidnightSun = false)
    {
        // Time-of-day check: aurora is only visible in darkness
        if (!isDark)
        {
            if (isMidnightSun)
                return "No darkness at your location this time of year — the midnight sun rules the sky. Come back in autumn when the nights return!";

            string darkTime;
            try
            {
                darkTime = darkFrom.HasValue ? $" around {darkFrom.Value:HH:mm}" : " later tonight";
            }
            catch (Exception ex)
            {
                darkTime = " later tonight";
                System.Diagnostics.Debug.WriteLine($"GuiMessageHelper: Failed to format darkFrom: {ex.Message}");
            }
            return probability switch
            {
                > 50 => $"The aurora is looking active — but it is still daylight at your location. Darkness falls{darkTime}. Set a reminder and check back then!",
                > 20 => $"Some aurora potential tonight, but it is still daytime at your location. It gets dark{darkTime} — worth keeping an eye out!",
                _ => $"Currently daytime at your location. Solar activity is low right now, but darkness falls{darkTime} — check back then for the latest forecast."
            };
        }

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

            if (!baseProbability.HasValue || baseProbability.Value <= 0)
                return "The solar activity isn't strong enough to reach this far south right now. Keep an eye on the KP index — when it climbs, the aurora follows!";

            return "Cloudy skies are winning tonight. There's some solar activity out there, but the clouds are blocking everything. Check back later or try another night!";
        }

        // Clouds significantly reducing an otherwise good forecast
        bool cloudsSignificant = baseProbability.HasValue
            && baseProbability.Value > 0
            && cloudCoverage > 40
            && (baseProbability.Value - probability) >= 20;

        if (cloudsSignificant)
        {
            return baseProbability!.Value switch
            {
                >= 75 => $"The aurora is active tonight — but thick clouds are in the way. Without them, you'd have a {baseProbability.Value:F0}% chance! Check for gaps in the cloud cover.",
                >= 55 => $"Decent aurora potential up there, but clouds are holding you back. Clear skies would give you a {baseProbability.Value:F0}% chance — keep an eye on the forecast!",
                _ => $"Some aurora activity tonight, but clouds are cutting your chances. Clear skies would give you a {baseProbability.Value:F0}% shot — worth checking back!"
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

    /// <summary>
    /// Formats the darkness window for a forecast card, e.g. "Dark 21:30 – 03:45".
    /// Returns an empty string for mid-latitude locations where darkness is not relevant.
    /// </summary>
    public static string GetDarknessWindowText(DateTime? sunrise, DateTime? sunset, double latitude)
    {
        if (sunrise == null || sunset == null)
            return Math.Abs(latitude) > 60 ? "Darkness unknown at this latitude" : string.Empty;

        var darkHours = (sunrise.Value - sunset.Value).TotalHours;
        if (darkHours < 0) darkHours += 24;

        return darkHours < 2
            ? "Midnight sun — no darkness"
            : $"Dark {sunset.Value:HH:mm} – {sunrise.Value:HH:mm}";
    }

    /// <summary>Returns true if there is less than 2 hours of darkness — midnight sun scenario.</summary>
    public static bool IsMidnightSun(DateTime? sunrise, DateTime? sunset, double latitude)
    {
        if (sunrise == null || sunset == null)
            return Math.Abs(latitude) > 65;

        var darkHours = (sunrise.Value - sunset.Value).TotalHours;
        if (darkHours < 0) darkHours += 24;
        return darkHours < 2;
    }
}

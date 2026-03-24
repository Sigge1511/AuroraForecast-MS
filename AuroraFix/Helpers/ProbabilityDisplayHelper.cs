using System.Globalization;

namespace AuroraFix.Helpers;

// All methods are pure functions — no state. Kept as static for direct call-site clarity.
public static class ProbabilityDisplayHelper
{
    /// <summary>
    /// Returns an aurora sighting probability (0–100) based on the current Kp index,
    /// the observer's latitude, and optional cloud cover percentage.
    /// </summary>
    public static double CalculateAuroraProbability(double kp, double userLatitude, double cloudCoverage = 0)
    {
        double requiredKp = Math.Max(0, (67 - userLatitude) / 1.5);
        double diff = kp - requiredKp;

        double baseProbability = diff switch
        {
            >= 1 => 95,
            >= 0 => 70 + (diff * 25),
            >= -1 => 30 + (diff + 1) * 40,
            >= -2 => 5 + (diff + 2) * 25,
            _ => 0
        };

        if (cloudCoverage > 0)
            baseProbability = AdjustForCloudCoverage(baseProbability, cloudCoverage);

        return Math.Max(0, Math.Min(100, baseProbability));
    }

    /// <summary>Returns the StrokeDashArray values for the probability ring arc.</summary>
    public static DoubleCollection UpdateCircle(double prob)
    {
        double totalUnits = 816.0 / 12.0;
        double filledUnits = (prob / 100.0) * totalUnits;
        return new DoubleCollection { filledUnits, 100 };
    }

    public static string GetActivityLevelText(double probability) => probability switch
    {
        >= 90 => "EXTREME",
        >= 70 => "VERY HIGH",
        >= 45 => "MODERATE",
        >= 20 => "LOW",
        _ => "VERY LOW"
    };

    /// <summary>Scales a base probability down based on cloud cover.</summary>
    public static double AdjustForCloudCoverage(double baseProbability, double cloudCoverage)
    {
        double factor = cloudCoverage switch
        {
            <= 15 => 1.0,   // Clear — no impact
            <= 40 => 0.65,  // Partly cloudy — slight impact
            <= 75 => 0.25,  // Mostly cloudy — significant impact
            _ => 0.0        // Overcast — aurora fully blocked
        };
        return baseProbability * factor;
    }

    public static string GetCloudImpactLabel(double cloudCoverage) => cloudCoverage switch
    {
        <= 15 => "Clear skies",
        <= 40 => "Partly cloudy",
        <= 75 => "Mostly cloudy",
        _ => "Overcast"
    };
}

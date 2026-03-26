using System.Globalization;

namespace AuroraFix.Helpers;

// All methods are pure functions — no state. Kept as static for direct call-site clarity.
public static class ProbabilityDisplayHelper
{
    // Kp threshold formula constants: requiredKp = (KpLatitudeOffset - latitude) / KpLatitudeDivisor
    private const double KpLatitudeOffset = 67.0;
    private const double KpLatitudeDivisor = 1.5;

    // Arc length of the full probability ring (816px circumference / 12 stroke-width units)
    private const double CircleArcUnits = 816.0 / 12.0;

    /// <summary>
    /// Returns an aurora sighting probability (0–100) based on the current Kp index,
    /// the observer's latitude, and optional cloud cover percentage.
    /// </summary>
    public static double CalculateAuroraProbability(double kp, double userLatitude, double cloudCoverage = 0)
    {
        double requiredKp = Math.Max(0, (KpLatitudeOffset - userLatitude) / KpLatitudeDivisor);
        double diff = kp - requiredKp;

        double baseProbability = diff switch
        {
            >= 3.0 => 90,
            >= 2.0 => 60 + (diff - 2.0) * 30,
            >= 1.0 => 35 + (diff - 1.0) * 25,
            >= 0.0 => 10 + diff * 25,
            >= -1.0 => 2 + (diff + 1.0) * 8,
            _ => 0
        };

        if (cloudCoverage > 0)
            baseProbability = AdjustForCloudCoverage(baseProbability, cloudCoverage);

        return Math.Max(0, Math.Min(100, baseProbability));
    }

    /// <summary>Returns the StrokeDashArray values for the probability ring arc.</summary>
    public static DoubleCollection UpdateCircle(double prob)
    {
        double safeProb = Math.Max(0, Math.Min(100, prob));
        double filledUnits = (safeProb / 100.0) * CircleArcUnits;
        return new DoubleCollection { filledUnits, 100 };
    }

    /// <summary>Returns a short activity label based on Kp index value.</summary>
    public static string GetKpActivityLevel(double kp) => kp switch
    {
        >= 7 => "Storm",
        >= 5 => "Active",
        >= 3 => "Medium",
        _ => "Low"
    };

    /// <summary>Returns a colour-dot emoji reflecting aurora probability.</summary>
    public static string GetIconEmoji(double prob) => prob switch
    {
        >= 85 => "\U0001F7E2",
        >= 50 => "\U0001F7E1",
        >= 20 => "\U0001F7E0",
        _ => "\U0001F534"
    };

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

using System.Globalization;

namespace AuroraFix.Helpers;

public class ProbabilityDisplayHelper 
{   
    public double CalculateAuroraProbability(double kp, double userLatitude, double cloudCoverage)
    {
        // calc that likens often used probability calcs
        double requiredKp = (67 - userLatitude) / 1.5;
        if (requiredKp < 0) requiredKp = 0;

        double diff = kp - requiredKp;
        double baseProbability;

        // Base probability calculation (from Kp and latitude)
        if (diff >= 1) baseProbability = 95;
        else if (diff >= 0) baseProbability = 70 + (diff * 25);
        else if (diff >= -1) baseProbability = 30 + (diff + 1) * 40;
        else if (diff >= -2) baseProbability = 5 + (diff + 2) * 25;
        else baseProbability = 0; // Too far south for current Kp

        // Step 2: Adjust for cloud coverage if provided
        if (cloudCoverage > 0)
        {
            baseProbability = AdjustForCloudCoverage(baseProbability, cloudCoverage);
        }

        return Math.Max(0, Math.Min(100, baseProbability));
    }
    public DoubleCollection UpdateCircle(double prob)
    {
        double totalUnits = 816.0 / 12.0;
        double filledUnits = (prob / 100.0) * totalUnits;

        var StrokeDashValues = new DoubleCollection { filledUnits, 100 };
        return StrokeDashValues;
    }
    public string GetActivityLevelText(double probability)
    {
        return probability switch
        {
            >= 90 => "EXTREME",
            >= 70 => "VERY HIGH",  
            >= 45 => "MODERATE",  
            >= 20 => "LOW",      
            _ => "VERY LOW"    
        };
    }
    public double AdjustForCloudCoverage(double baseProbability, double cloudCoverage)
    {
        // 4 stages of cloud impact
        double factor = cloudCoverage switch
        {
            <= 15 => 1.0,   // Clear — no impact
            <= 40 => 0.65,  // Partly cloudy — slight impact
            <= 75 => 0.25,  // Mostly cloudy — big impact
            _ => 0.0        // Overcast — aurora completely blocked
        };

        return baseProbability * factor;
    }

    public string GetCloudImpactLabel(double cloudCoverage)
    {
        return cloudCoverage switch
        {
            <= 15 => "Clear skies ☀️",
            <= 40 => "Partly cloudy 🌤️",
            <= 75 => "Mostly cloudy ☁️",
            _ => "Overcast 🌫️"
        };
    }
}
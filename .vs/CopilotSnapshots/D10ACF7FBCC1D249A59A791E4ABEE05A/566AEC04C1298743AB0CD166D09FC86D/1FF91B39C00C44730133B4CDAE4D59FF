using System.Globalization;

namespace AuroraForecast.Helpers;

public class ProbabilityDisplayHelper 
{   
    public double CalculateAuroraProbability(double kp, double userLatitude)
    {
        // calc that likens often used probability calcs
        double requiredKp = (67 - userLatitude) / 1.5;
        if (requiredKp < 0) requiredKp = 0;

        double diff = kp - requiredKp;

        // a more forgiving estimation on what KP is needed
        if (diff >= 1) return 95;      
        if (diff >= 0) return 70 + (diff * 25); 
        if (diff >= -1) return 30 + (diff + 1) * 40; 
        if (diff >= -2) return 5 + (diff + 2) * 25; 

        return 0; // to far south for current Kp
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
}
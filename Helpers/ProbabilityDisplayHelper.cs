using System.Globalization;

namespace AuroraForecast.Helpers;

public class ProbabilityDisplayHelper 
{   
    public double CalculateAuroraProbability(double kp, double userLatitude)
    {
        // calc that likens often used probability calcs
        double kpThreshold = 10.0 - (userLatitude - 45.0) * 0.4;
        kpThreshold = Math.Max(0, kpThreshold);

        if (kp < kpThreshold)
        {
            return (kpThreshold - kp < 0.5) ? 5.0 : 0.0;
        }

        double probability = (kp - kpThreshold) * 33.0;
        return Math.Clamp(probability, 0, 100);
    }
    public DoubleCollection UpdateCircle(double prob)
    {
        double totalUnits = 816.0 / 12.0;
        double filledUnits = (prob / 100.0) * totalUnits;

        var StrokeDashValues = new DoubleCollection { filledUnits, 100 };
        return StrokeDashValues;
    }
    
}
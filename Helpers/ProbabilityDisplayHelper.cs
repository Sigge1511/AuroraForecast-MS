using System.Globalization;

namespace AuroraForecast.Helpers;

public class ProbabilityDisplayHelper /*: IValueConverter*/
{
    // Omkretsen på vår cirkel (pi * diameter)
    // Med Width=280 och StrokeThickness=12 blir radien ca 134 -> omkrets ca 840
    //private const double Circumference = 840;

    //public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    //{
    //    if (value is double prob)
    //    {
    //        // För en cirkel med radie 130 och StrokeThickness 12:
    //        // Omkretsen är 816.8 pixlar. DashArray räknar i "thickness-enheter".
    //        double totalDashUnits = 816.8 / 12.0;
    //        double filledUnits = (prob / 100.0) * totalDashUnits;

    //        // Vi returnerar [fylld del, ett jättestort gap]
    //        return new DoubleCollection { filledUnits, 100 };
    //    }
    //    return new DoubleCollection { 0, 100 };
    //}
    //public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => null;


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
}
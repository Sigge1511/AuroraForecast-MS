using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuroraFix.Models
{
    public class Weather
    {
        public double CloudCoverage { get; set; }
        public DateTime ForecastTime { get; set; }
        public string WeatherDescription { get; set; } = string.Empty;
        public bool IsDay { get; set; }
        public DateTime? Sunrise { get; set; }
        public DateTime? Sunset { get; set; }


        public string GetCloudCondition()
        {
            return CloudCoverage switch
            {
                < 5 => "Clear skies - perfect!",
                < 20 => "Partly cloudy - Okay",
                < 50 => "Mostly cloudy - Difficult",
                _ => "Overcast - Impossible"
            };
        }

        public string GetWeatherIcon()
        {
            return CloudCoverage switch
            {
                < 10 => "☀️",
                < 30 => "🌤️",
                < 50 => "⛅",
                < 70 => "☁️",
                _ => "🌧️"
            };
        }

        public int AdjustAuroraProbability(int baseProbability)
        {
            // Reduce the aurora probability based on cloud coverage
            var cloudPenalty = CloudCoverage switch
            {
                < 10 => 0,      // No reduction - perfect!
                < 20 => 2,      // Minimal reduction
                < 30 => 5,      // Small reduction
                < 40 => 10,     // Moderate reduction
                < 50 => 20,     // Medium reduction
                < 60 => 35,     // Large reduction
                < 70 => 50,     // Very large reduction
                _ => 80         // Almost impossible
            };

            return Math.Max(0, baseProbability - cloudPenalty);
        }
    }
}

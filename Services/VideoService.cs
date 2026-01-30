namespace AuroraForecast.Services;

public class VideoService
{
    public string GetVideoForKpIndex(double kpIndex)
    {
        var videoName = kpIndex switch
        {
            >= 7 => "aurora_storm.mp4",
            >= 5 => "aurora_active.mp4",
            >= 3 => "aurora_medium.mp4",
            _ => "aurora_low.mp4"
        };

        return videoName;
    }

    public string GetVideoSourceUri(double kpIndex)
    {
        var videoName = GetVideoForKpIndex(kpIndex);
        return videoName;
    }

    public string GetVideoDescription(double kpIndex)
    {
        return kpIndex switch
        {
            >= 7 => "Intensiv nordskensaktivitet",
            >= 5 => "Hög nordskensaktivitet",
            >= 3 => "Måttlig nordskensaktivitet",
            _ => "Lugn nordskensaktivitet"
        };
    }
}

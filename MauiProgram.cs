using Microsoft.Extensions.Logging;
using AuroraForecast.Views;
using AuroraForecast.ViewModels;
using AuroraForecast.Services;
using CommunityToolkit.Maui;

namespace AuroraForecast;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .UseMauiCommunityToolkitMediaElement()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        // Register Services
        builder.Services.AddSingleton<AuroraService>();
        builder.Services.AddSingleton<GeocodingService>();
        builder.Services.AddSingleton<VideoService>();

        // Register ViewModels
        builder.Services.AddTransient<MainPageViewModel>();

        // Register Views
        builder.Services.AddTransient<MainPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}

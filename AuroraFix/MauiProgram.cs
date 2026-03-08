using AuroraFix.Services;
using AuroraFix.ViewModels;
using Microsoft.Extensions.Logging;
using AuroraFix.Views;

namespace AuroraFix
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("Montserrat-Regular.ttf", "Montserrat");
                    fonts.AddFont("Montserrat-Bold.ttf", "MontserratBold");
                });


            // Register Services
            builder.Services.AddSingleton<AuroraService>();
            builder.Services.AddSingleton<GeocodingService>();

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
}

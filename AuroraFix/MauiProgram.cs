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
                })
                .ConfigureMauiHandlers(handlers =>
                {
                    Microsoft.Maui.Handlers.EntryHandler.Mapper.AppendToMapping("NoUnderline", (handler, view) =>
                    {
#if WINDOWS
                        handler.PlatformView.BorderThickness = new Microsoft.UI.Xaml.Thickness(0);
                        handler.PlatformView.Resources["TextControlBorderThemeThicknessFocused"] = new Microsoft.UI.Xaml.Thickness(0);
#endif
                    });
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

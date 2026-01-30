using AuroraForecast.Views;

namespace AuroraForecast;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();

        MainPage = new MainPage();
    }
}

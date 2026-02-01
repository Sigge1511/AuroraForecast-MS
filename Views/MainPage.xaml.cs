namespace AuroraForecast.Views;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();
        Microsoft.Maui.Handlers.EntryHandler.Mapper.AppendToMapping("NoUnderline", (handler, view) =>
        {
            #if WINDOWS
            // Tar bort den inbyggda linjen och fokus-markeringen på Windows
            handler.PlatformView.BorderThickness = new Microsoft.UI.Xaml.Thickness(0);
            handler.PlatformView.Resources["TextControlBorderThemeThicknessFocused"] = new Microsoft.UI.Xaml.Thickness(0);
            #endif
        });
    }

    private async void OnSearchClicked(object? sender, EventArgs e)
    {
        var viewModel = BindingContext as ViewModels.MainPageViewModel;
        if (viewModel?.SearchCityCommand.CanExecute(null) == true)
        {
            await viewModel.SearchCityCommand.ExecuteAsync(null);
        }
    }
}

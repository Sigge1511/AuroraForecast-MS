namespace AuroraForecast.Views;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();
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

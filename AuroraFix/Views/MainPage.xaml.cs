namespace AuroraFix.Views;

public partial class MainPage : ContentPage
{
    public MainPage(ViewModels.MainPageViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is ViewModels.MainPageViewModel vm && !vm.IsDataLoaded)
        {
            try
            {
                await vm.InitializeAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"MainPage.OnAppearing error: {ex.Message}");
            }
        }
    }

    private async void OnSearchClicked(object? sender, EventArgs e)
    {
        try
        {
            if (BindingContext is ViewModels.MainPageViewModel vm &&
                vm.SearchCityCommand.CanExecute(null))
            {
                await vm.SearchCityCommand.ExecuteAsync(null);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"MainPage.OnSearchClicked error: {ex.Message}");
        }
    }
}

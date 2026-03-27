namespace AuroraFix.Views;

public partial class MainPage : ContentPage
{
    private bool _locationInitialized;

    public MainPage(ViewModels.MainPageViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (!_locationInitialized && BindingContext is ViewModels.MainPageViewModel vm)
        {
            _locationInitialized = true;
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

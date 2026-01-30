using CommunityToolkit.Mvvm.ComponentModel;

namespace AuroraForecast.ViewModels;

public partial class BaseViewModel : ObservableObject
{
    [ObservableProperty]
    private bool isBusy;

    [ObservableProperty]
    private string title = string.Empty;

    [ObservableProperty]
    private string errorMessage = string.Empty;

    [ObservableProperty]
    private bool hasError;

    public void ClearError()
    {
        HasError = false;
        ErrorMessage = string.Empty;
    }

    public void SetError(string message)
    {
        HasError = true;
        ErrorMessage = message;
    }
}

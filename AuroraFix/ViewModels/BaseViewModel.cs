using CommunityToolkit.Mvvm.ComponentModel;

namespace AuroraFix.ViewModels;

public partial class BaseViewModel : ObservableObject
{
    private System.Threading.CancellationTokenSource? _errorCts;
    private CommunityToolkit.Mvvm.Input.IRelayCommand? _dismissErrorCommand;
    public CommunityToolkit.Mvvm.Input.IRelayCommand DismissErrorCommand =>
        _dismissErrorCommand ??= new CommunityToolkit.Mvvm.Input.RelayCommand(ClearError);
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
        _errorCts?.Cancel();
        _errorCts = null;
        HasError = false;
        ErrorMessage = string.Empty;
    }

    public void SetError(string message)
    {
        _errorCts?.Cancel();
        _errorCts = new System.Threading.CancellationTokenSource();
        HasError = true;
        ErrorMessage = message;
        AutoClearErrorAsync(_errorCts.Token);
    }

    private async void AutoClearErrorAsync(System.Threading.CancellationToken token)
    {
        try
        {
            await System.Threading.Tasks.Task.Delay(4000, token);
            if (!token.IsCancellationRequested)
            {
                ClearError();
            }
        }
        catch (System.OperationCanceledException) { }
    }
}

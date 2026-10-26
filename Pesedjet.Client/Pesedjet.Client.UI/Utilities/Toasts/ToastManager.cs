using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Pesedjet.Client.UI.Utilities.Toasts;

public sealed partial class ToastManager : ObservableObject, IToastManager
{
    private static readonly TimeSpan ToastDuration = TimeSpan.FromSeconds(4);

    private readonly TimeProvider _timeProvider;

    private int _lastToastNumber;

    [ObservableProperty]
    private string _message = string.Empty;

    [ObservableProperty]
    private bool _isVisible;

    public ToastManager(TimeProvider timeProvider)
    {
        _timeProvider = timeProvider;
    }

    public void Show(string message)
    {
        _lastToastNumber++;
        Message = message;
        IsVisible = true;

        _ = HideAfterDelayAsync(_lastToastNumber);
    }

    // A newer toast restarts the time: only the last one may hide the toast.
    private async Task HideAfterDelayAsync(int toastNumber)
    {
        await Task.Delay(ToastDuration, _timeProvider);

        if (toastNumber == _lastToastNumber)
        {
            IsVisible = false;
        }
    }
}

using System;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Pesedjet.Client.UI.Utilities.Timing;

namespace Pesedjet.Client.UI.ViewModels;

// MOD-006 (CU-034): the turn time ran out; the player has one minute to answer before others can vote his expulsion.
public partial class AreYouThereViewModel : DialogViewModelBase, IDisposable
{
    private const int AnswerSeconds = 60;
    private const int SecondsPerMinute = 60;
    private const int WarningSeconds = 15;

    private readonly Countdown _countdown;

    [ObservableProperty]
    private string _remainingText = string.Empty;

    [ObservableProperty]
    private bool _isWarning;

    public AreYouThereViewModel(TimeProvider timeProvider)
    {
        _countdown = new Countdown(timeProvider, HandleTick, HandleFinished);
        _countdown.Start(AnswerSeconds);
    }

    public void Dispose()
    {
        _countdown.Dispose();
    }

    // The player cannot dismiss the question: only answering or running out of time closes it.
    public override void Dismiss()
    {
    }

    [RelayCommand]
    private void StillHere()
    {
        IsConfirmed = true;
        RequestClose();
    }

    private void HandleTick(int remainingSeconds)
    {
        RemainingText = string.Format(
            CultureInfo.InvariantCulture,
            "{0}:{1:00}",
            remainingSeconds / SecondsPerMinute,
            remainingSeconds % SecondsPerMinute);
        IsWarning = remainingSeconds <= WarningSeconds;
    }

    private void HandleFinished()
    {
        // TODO: Tell the server the player did not answer so the others can vote his expulsion (CU-034, CU-016).
        RequestClose();
    }
}

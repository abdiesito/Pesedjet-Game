using System;
using System.Collections.Generic;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Pesedjet.Client.UI.Models;
using Pesedjet.Client.UI.Utilities.Dialogs;
using Pesedjet.Client.UI.Utilities.Localization;
using Pesedjet.Client.UI.Utilities.Navigation;
using Pesedjet.Client.UI.Utilities.Session;
using Pesedjet.Client.UI.Utilities.Timing;

namespace Pesedjet.Client.UI.ViewModels;

// MOD-010: the final standings. A registered player can go to the main menu, back to the room or to the scoreboard;
// a guest has a single exit that continues by itself after ten seconds and leads to the game menu (CU-029 FA-03).
public partial class MatchResultViewModel : DialogViewModelBase, IDialogParameter<MatchResult>, IDisposable
{
    private const int ContinueSeconds = 10;
    private const string TitleKey = "MatchResult.Title";
    private const string SubtitleKey = "MatchResult.Subtitle";
    private const string ContinueKey = "MatchResult.Continue";

    private readonly INavigator _navigator;
    private readonly IPlayerSession _playerSession;
    private readonly Countdown _countdown;

    private MatchResult? _result;

    [ObservableProperty]
    private string _title = string.Empty;

    [ObservableProperty]
    private string _subtitle = string.Empty;

    [ObservableProperty]
    private string _continueText = string.Empty;

    [ObservableProperty]
    private IReadOnlyList<MatchResultRow> _rows = [];

    public bool IsGuest => _playerSession.IsGuest;

    public bool IsRegistered => !_playerSession.IsGuest;

    public MatchResultViewModel(INavigator navigator, IPlayerSession playerSession, TimeProvider timeProvider)
    {
        _navigator = navigator;
        _playerSession = playerSession;
        _countdown = new Countdown(timeProvider, HandleTick, HandleFinished);
    }

    public void Initialize(MatchResult parameter)
    {
        _result = parameter;
        Rows = parameter.Rows;
        FillTexts();

        if (_playerSession.IsGuest)
        {
            _countdown.Start(ContinueSeconds);
        }
    }

    public void Dispose()
    {
        _countdown.Dispose();
    }

    // The result cannot be dismissed with Esc: the player leaves through one of its exits.
    public override void Dismiss()
    {
    }

    [RelayCommand]
    private void GoToMainMenu()
    {
        RequestClose();
        _navigator.NavigateAndClear<MainMenuViewModel>();
    }

    [RelayCommand]
    private void GoBackToRoom()
    {
        RequestClose();
        _navigator.NavigateAndClear<MainMenuViewModel>();
        _navigator.NavigateTo<WaitingRoomViewModel>();
    }

    [RelayCommand]
    private void ViewScoreboard()
    {
        RequestClose();
        _navigator.NavigateAndClear<MainMenuViewModel>();
        _navigator.NavigateTo<ScoreboardViewModel>();
    }

    [RelayCommand]
    private void ContinueAsGuest()
    {
        RequestClose();
        _navigator.NavigateAndClear<GameMenuViewModel>();
    }

    protected override void OnLanguageChanged()
    {
        FillTexts();
        Rows = new List<MatchResultRow>(Rows);
        HandleTick(_countdown.RemainingSeconds);
        base.OnLanguageChanged();
    }

    private void FillTexts()
    {
        if (_result is not null)
        {
            Title = string.Format(
                CultureInfo.CurrentCulture,
                LocalizationManager.Instance[TitleKey],
                _result.WinnerName);
            Subtitle = string.Format(
                CultureInfo.CurrentCulture,
                LocalizationManager.Instance[SubtitleKey],
                _result.AmuletGoal,
                _result.WinningRound);
        }
    }

    private void HandleTick(int remainingSeconds)
    {
        ContinueText = string.Format(
            CultureInfo.CurrentCulture,
            LocalizationManager.Instance[ContinueKey],
            remainingSeconds);
    }

    private void HandleFinished()
    {
        ContinueAsGuest();
    }
}

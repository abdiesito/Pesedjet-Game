using System;
using System.Collections.Generic;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Pesedjet.Client.UI.Models;
using Pesedjet.Client.UI.Utilities.Dialogs;
using Pesedjet.Client.UI.Utilities.Localization;
using Pesedjet.Client.UI.Utilities.Timing;

namespace Pesedjet.Client.UI.ViewModels;

// MOD-009: the round summary drawn over the table; it continues by itself after ten seconds.
public partial class RoundSummaryViewModel : DialogViewModelBase, IDialogParameter<RoundSummary>, IDisposable
{
    private const int ContinueSeconds = 10;
    private const string TitleKey = "RoundSummary.Title";
    private const string ContinueKey = "RoundSummary.Continue";
    private const string ReadyKey = "RoundSummary.Ready";
    private const string ScoreKey = "RoundSummary.ColScore";

    private readonly Countdown _countdown;

    private RoundSummary? _summary;

    [ObservableProperty]
    private string _scoreboardHeader = string.Empty;

    [ObservableProperty]
    private string _title = string.Empty;

    [ObservableProperty]
    private string _cause = string.Empty;

    [ObservableProperty]
    private string _nextRoundNote = string.Empty;

    [ObservableProperty]
    private string _readyText = string.Empty;

    [ObservableProperty]
    private string _continueText = string.Empty;

    [ObservableProperty]
    private IReadOnlyList<RoundSummaryRow> _rows = [];

    public RoundSummaryViewModel(TimeProvider timeProvider)
    {
        _countdown = new Countdown(timeProvider, HandleTick, HandleFinished);
    }

    public void Initialize(RoundSummary parameter)
    {
        _summary = parameter;
        Rows = parameter.Rows;
        FillTexts();
        _countdown.Start(ContinueSeconds);
    }

    public void Dispose()
    {
        _countdown.Dispose();
    }

    [RelayCommand]
    private void Continue()
    {
        RequestClose();
    }

    protected override void OnLanguageChanged()
    {
        FillTexts();
        Rows = new List<RoundSummaryRow>(Rows);
        HandleTick(_countdown.RemainingSeconds);
        base.OnLanguageChanged();
    }

    private void FillTexts()
    {
        if (_summary is not null)
        {
            Cause = _summary.Cause.Resolve();
            NextRoundNote = _summary.NextRoundNote.Resolve();
            Title = string.Format(
                CultureInfo.CurrentCulture,
                LocalizationManager.Instance[TitleKey],
                _summary.Round);
            ScoreboardHeader = string.Format(
                CultureInfo.CurrentCulture,
                LocalizationManager.Instance[ScoreKey],
                Rows.Count > 0 ? Rows[0].AmuletGoal : 0);
            ReadyText = string.Format(
                CultureInfo.CurrentCulture,
                LocalizationManager.Instance[ReadyKey],
                1,
                Rows.Count);
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
        RequestClose();
    }
}

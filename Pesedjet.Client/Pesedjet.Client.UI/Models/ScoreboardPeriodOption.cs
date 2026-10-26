using CommunityToolkit.Mvvm.ComponentModel;
using Pesedjet.Client.UI.Utilities.Localization;

namespace Pesedjet.Client.UI.Models;

public sealed class ScoreboardPeriodOption : ObservableObject
{
    private readonly string _key;

    public ScoreboardPeriodOption(ScoreboardPeriod period, string key)
    {
        Period = period;
        _key = key;
    }

    public ScoreboardPeriod Period { get; }

    public string DisplayName => LocalizationManager.Instance[_key];

    public void Refresh()
    {
        OnPropertyChanged(nameof(DisplayName));
    }
}

using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using Pesedjet.Client.UI.Utilities.Localization;

namespace Pesedjet.Client.UI.Models;

public partial class ObjectiveOption : ObservableObject
{
    [ObservableProperty]
    private bool _isSelected;

    public required string Username { get; init; }

    public required bool IsEligible { get; init; }
    
    public string ReasonKey { get; init; } = string.Empty;

    public string ReasonText => ReasonKey.Length == 0 ? string.Empty : LocalizationManager.Instance[ReasonKey];

    public bool HasReason => ReasonKey.Length > 0;

    public ICommand? SelectCommand { get; set; }

    public void Refresh()
    {
        OnPropertyChanged(nameof(ReasonText));
    }
}

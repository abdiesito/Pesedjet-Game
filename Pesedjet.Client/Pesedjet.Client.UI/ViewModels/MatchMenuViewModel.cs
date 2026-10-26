using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Pesedjet.Client.UI.Models;

namespace Pesedjet.Client.UI.ViewModels;

public partial class MatchMenuViewModel : DialogViewModelBase
{
    [ObservableProperty]
    private MatchChoice _choice = MatchChoice.None;

    [RelayCommand]
    private void Close()
    {
        RequestClose();
    }

    [RelayCommand]
    private void OpenSettings()
    {
        Choice = MatchChoice.Settings;
        RequestClose();
    }

    [RelayCommand]
    private void Abandon()
    {
        Choice = MatchChoice.Abandon;
        RequestClose();
    }
}

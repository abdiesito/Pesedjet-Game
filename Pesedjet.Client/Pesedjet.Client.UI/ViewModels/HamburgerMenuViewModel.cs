using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using Pesedjet.Client.UI.Utilities.Dialogs;

namespace Pesedjet.Client.UI.ViewModels;

// The menu of the three horizontal lines: the guest has no main menu, so the language and the settings live here.
public partial class HamburgerMenuViewModel : DialogViewModelBase
{
    private readonly IDialogManager _dialogManager;

    public LanguageSettingsViewModel Language { get; }

    public HamburgerMenuViewModel(IDialogManager dialogManager, LanguageSettingsViewModel language)
    {
        _dialogManager = dialogManager;
        Language = language;
    }

    [RelayCommand]
    private void Close()
    {
        RequestClose();
    }

    [RelayCommand]
    private async Task OpenSettingsAsync()
    {
        RequestClose();

        await _dialogManager.ShowDialogAsync<SettingsViewModel>();
    }
}

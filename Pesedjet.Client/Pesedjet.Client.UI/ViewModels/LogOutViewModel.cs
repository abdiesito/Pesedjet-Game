using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Pesedjet.Client.Networking.Interfaces;
using Pesedjet.Client.Networking.Results;
using Pesedjet.Client.UI.Models;
using Pesedjet.Client.UI.Utilities.Localization;
using Pesedjet.Client.UI.Utilities.Navigation;
using Pesedjet.Client.UI.Utilities.Session;

namespace Pesedjet.Client.UI.ViewModels;

public partial class LogOutViewModel : DialogViewModelBase
{
    private readonly INavigator _navigator;
    private readonly IPlayerSession _playerSession;
    private readonly IAuthClient _authClient;
    private readonly UserSettingsStore _userSettingsStore;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(CancelCommand), nameof(ConfirmLogOutCommand))]
    private bool _isBusy;

    public LogOutViewModel(
        INavigator navigator,
        IPlayerSession playerSession,
        IAuthClient authClient,
        UserSettingsStore userSettingsStore)
    {
        _navigator = navigator;
        _playerSession = playerSession;
        _authClient = authClient;
        _userSettingsStore = userSettingsStore;
    }

    public override void Dismiss()
    {
        if (!IsBusy)
        {
            RequestClose();
        }
    }

    [RelayCommand(CanExecute = nameof(CanRespond))]
    private void Cancel()
    {
        RequestClose();
    }

    [RelayCommand(CanExecute = nameof(CanRespond))]
    private async Task ConfirmLogOutAsync()
    {
        IsBusy = true;

        try
        {
            await RequestServerLogOutAsync();
        }
        finally
        {
            IsBusy = false;
        }

        UserSettings currentSettings = _userSettingsStore.Load();
        UserSettings updatedSettings = currentSettings with { RememberToken = string.Empty };
        _userSettingsStore.Save(updatedSettings);

        _playerSession.CurrentPlayer = null;
        _playerSession.PlayerId = Guid.Empty;
        RequestClose();
        _navigator.NavigateAndClear<AccessMenuViewModel>();
    }

    private bool CanRespond()
    {
        return !IsBusy;
    }

    private async Task RequestServerLogOutAsync()
    {
        UserSettings settings = _userSettingsStore.Load();

        if (!string.IsNullOrWhiteSpace(settings.RememberToken))
        {
            AuthResult result = await _authClient.LogoutAsync(settings.RememberToken);
            _ = result;
        }
    }
}
using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Pesedjet.Client.Networking.Interfaces;
using Pesedjet.Client.Networking.Results;
using Pesedjet.Client.UI.Utilities.Localization;
using Pesedjet.Client.UI.Utilities.Navigation;
using Pesedjet.Client.UI.Utilities.Session;
using Pesedjet.Client.UI.Utilities.Threading;
using Pesedjet.Client.UI.ViewModels.Dialogs.Dependencies;

namespace Pesedjet.Client.UI.ViewModels.Dialogs;

public partial class LogOutViewModel : DialogViewModelBase
{
    private readonly INavigator _navigator;
    private readonly IPlayerSession _playerSession;
    private readonly IAuthClient _authClient;
    private readonly UserSettingsStore _userSettingsStore;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(CancelCommand), nameof(ConfirmLogOutCommand))]
    private bool _isBusy;

    public LogOutViewModel(LogOutDependencies dependencies)
    {
        _navigator = dependencies.Navigator;
        _playerSession = dependencies.PlayerSession;
        _authClient = dependencies.AuthClient;
        _userSettingsStore = dependencies.UserSettingsStore;
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
    private void ConfirmLogOut()
    {
        IsBusy = true;

        UserSettings settings = _userSettingsStore.Load();
        string rememberToken = settings.RememberToken;

        if (string.IsNullOrWhiteSpace(rememberToken))
        {
            FinishLogOut(true);
        }
        else
        {
            BackgroundJob<string, ClientResult<bool>> job = new()
            {
                Work = _authClient.Logout,
                State = rememberToken,
                OnCompleted = OnLogOutCompleted,
                OnFailed = OnLogOutFailed
            };

            BackgroundWork.Start(job);
        }
    }

    private void OnLogOutCompleted(ClientResult<bool> result, string rememberToken)
    {
        FinishLogOut(result.IsSuccess);
    }

    private void OnLogOutFailed(Exception error, string rememberToken)
    {
        FinishLogOut(false);
    }

    // When the server could not be reached the token stays on this machine: the server still holds the session for
    // it, and the token is what lets this device log in again.
    private void FinishLogOut(bool isTokenReleased)
    {
        IsBusy = false;

        UserSettings currentSettings = _userSettingsStore.Load();
        string keptToken = isTokenReleased ? string.Empty : currentSettings.RememberToken;
        UserSettings updatedSettings = currentSettings with { RememberToken = keptToken, AutoResume = false };
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
}

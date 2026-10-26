using System;
using System.Globalization;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using Pesedjet.Client.UI.Models;
using Pesedjet.Client.UI.Utilities.Dialogs;
using Pesedjet.Client.UI.Utilities.Localization;
using Pesedjet.Client.UI.Utilities.Navigation;
using Pesedjet.Client.UI.Utilities.Session;
using Pesedjet.Client.UI.Utilities.Toasts;

namespace Pesedjet.Client.UI.ViewModels;

public partial class MainMenuViewModel : ViewModelBase, INavigable
{
    private const int WelcomeNameMaximumLength = 20;
    private const string Ellipsis = "…";
    private const string WelcomeBackKey = "MainMenu.WelcomeBack";

    private readonly INavigator _navigator;
    private readonly IDialogManager _dialogManager;
    private readonly IPlayerSession _playerSession;
    private readonly IToastManager _toastManager;

    public MainMenuViewModel(
        INavigator navigator,
        IDialogManager dialogManager,
        IPlayerSession playerSession,
        IToastManager toastManager)
    {
        _navigator = navigator;
        _dialogManager = dialogManager;
        _playerSession = playerSession;
        _toastManager = toastManager;
    }

    public bool ShowsTopBar => true;

    // Coming back from another screen is not a new sign-in: the welcome is shown only once.
    public void OnNavigatedTo()
    {
        if (!_playerSession.HasWelcomed)
        {
            PlayerProfile player = _playerSession.CurrentPlayer ?? new PlayerProfile(string.Empty, 0, 0);

            _playerSession.HasWelcomed = true;
            _toastManager.Show(string.Format(
                CultureInfo.CurrentCulture,
                LocalizationManager.Instance[WelcomeBackKey],
                ShortenForWelcome(player.Username)));
        }
    }

    [RelayCommand]
    private void Play()
    {
        _navigator.NavigateTo<GameMenuViewModel>();
    }

    [RelayCommand]
    private void OpenScoreboard()
    {
        _navigator.NavigateTo<ScoreboardViewModel>();
    }

    [RelayCommand]
    private void OpenFriends()
    {
        _navigator.NavigateTo<FriendsViewModel>();
    }

    [RelayCommand]
    private async Task OpenSettingsAsync()
    {
        await _dialogManager.ShowDialogAsync<SettingsViewModel>();
    }

    [RelayCommand]
    private async Task LogOutAsync()
    {
        await _dialogManager.ShowDialogAsync<LogOutViewModel>();
    }

    private static string ShortenForWelcome(string username)
    {
        string shortened = username;

        if (username.Length > WelcomeNameMaximumLength)
        {
            shortened = username[..WelcomeNameMaximumLength] + Ellipsis;
        }

        return shortened;
    }
}

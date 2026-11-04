using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Pesedjet.Client.Networking.Interfaces;
using Pesedjet.Client.Networking.Results;
using Pesedjet.Client.UI.Models;
using Pesedjet.Client.UI.Utilities.Dialogs;
using Pesedjet.Client.UI.Utilities.Localization;
using Pesedjet.Client.UI.Utilities.Navigation;
using Pesedjet.Client.UI.Utilities.Session;
using Pesedjet.Client.UI.Utilities.Validation;
using Pesedjet.Server.Contracts.DTOs;

namespace Pesedjet.Client.UI.ViewModels;

public partial class AccessMenuViewModel : ViewModelBase, INavigable
{
    private const int MaxFailedAttemptsThreshold = 5;

    private readonly INavigator _navigator;
    private readonly IDialogManager _dialogManager;
    private readonly IAuthClient _authClient;
    private readonly IPlayerSession _playerSession;
    private readonly UserSettingsStore _userSettingsStore;

    private int _failedAttemptsCount;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string _username = string.Empty;

    [ObservableProperty]
    private string _password = string.Empty;

    [ObservableProperty]
    private bool _rememberSession;

    [ObservableProperty]
    private bool _isPasswordVisible;

    [ObservableProperty]
    [NotifyPropertyChangedFor(
        nameof(HasUsernameError),
        nameof(IsUsernameRequired),
        nameof(IsUsernameFormatInvalid))]
    private FieldValidationError _usernameValidationError;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasPasswordError), nameof(IsPasswordRequired))]
    private FieldValidationError _passwordValidationError;

    [ObservableProperty]
    [NotifyPropertyChangedFor(
        nameof(IsInvalidCredentials),
        nameof(IsTooManyAttempts),
        nameof(IsServerUnreachable))]
    private LoginStatus _loginStatus;

    public bool HasUsernameError => UsernameValidationError != FieldValidationError.None;

    public bool IsUsernameRequired => UsernameValidationError == FieldValidationError.Required;

    public bool IsUsernameFormatInvalid => UsernameValidationError == FieldValidationError.InvalidFormat;

    public bool HasPasswordError => PasswordValidationError != FieldValidationError.None;

    public bool IsPasswordRequired => PasswordValidationError == FieldValidationError.Required;

    public bool IsInvalidCredentials => LoginStatus == LoginStatus.InvalidCredentials;

    public bool IsTooManyAttempts => LoginStatus == LoginStatus.TooManyAttempts;

    public bool IsServerUnreachable => LoginStatus == LoginStatus.ServerUnreachable;

    public AccessMenuViewModel(
        INavigator navigator,
        IDialogManager dialogManager,
        IAuthClient authClient,
        IPlayerSession playerSession,
        UserSettingsStore userSettingsStore)
    {
        _navigator = navigator;
        _dialogManager = dialogManager;
        _authClient = authClient;
        _playerSession = playerSession;
        _userSettingsStore = userSettingsStore;

        _ = ResumeSessionIfSavedAsync();
    }

    public void OnNavigatedFrom()
    {
        Password = string.Empty;
        IsPasswordVisible = false;
    }

    partial void OnUsernameChanged(string value)
    {
        UsernameValidationError = FieldValidationError.None;
        ClearStatus();
    }

    partial void OnPasswordChanged(string value)
    {
        PasswordValidationError = FieldValidationError.None;
        ClearStatus();
    }

    [RelayCommand]
    private async Task LoginAsync()
    {
        Username = Username.Trim();

        if (ValidateFields() && !IsBusy)
        {
            IsBusy = true;
            ClearStatus();

            try
            {
                UserLoginRequest request = new UserLoginRequest
                {
                    Username = Username,
                    Password = Password,
                    RememberSession = RememberSession
                };

                (AuthResult result, UserSession? session) = await _authClient.LoginAsync(request);

                if (result == AuthResult.Success && session is not null)
                {
                    _failedAttemptsCount = 0;

                    string rememberToken = RememberSession ? (session.RememberToken ?? string.Empty) : string.Empty;

                    UserSettings currentSettings = _userSettingsStore.Load();
                    UserSettings updatedSettings = currentSettings with
                    {
                        RememberToken = rememberToken
                    };

                    _userSettingsStore.Save(updatedSettings);

                    PlayerProfile profile = new PlayerProfile(
                        Username: session.Username ?? string.Empty,
                        Points: 0,
                        Victories: 0
                    );

                    _playerSession.CurrentPlayer = profile;
                    _navigator.NavigateTo<MainMenuViewModel>();
                }
                else
                {
                    HandleLoginFailure(result);
                }
            }
            finally
            {
                IsBusy = false;
            }
        }
    }

    [RelayCommand]
    private void NavigateToForgotPassword()
    {
        // TODO: Navigate to ForgotPasswordViewModel when implemented.
    }

    [RelayCommand]
    private void NavigateToRegisterAccount()
    {
        _navigator.NavigateTo<RegisterAccountViewModel>();
    }

    [RelayCommand]
    private async Task PlayAsGuestAsync()
    {
        await _dialogManager.ShowDialogAsync<GuestNoticeViewModel, GuestNoticeMode>(GuestNoticeMode.Entry);
    }

    private async Task ResumeSessionIfSavedAsync()
    {
        UserSettings settings = _userSettingsStore.Load();

        if (!string.IsNullOrWhiteSpace(settings.RememberToken))
        {
            IsBusy = true;

            try
            {
                (AuthResult result, UserSession? session) = await _authClient.ResumeSessionAsync(settings.RememberToken);

                if (result == AuthResult.Success && session is not null)
                {
                    PlayerProfile profile = new PlayerProfile(
                        Username: session.Username ?? string.Empty,
                        Points: 0,
                        Victories: 0
                    );

                    _playerSession.CurrentPlayer = profile;
                    _navigator.NavigateTo<MainMenuViewModel>();
                }
                else
                {
                    UserSettings resetSettings = settings with { RememberToken = string.Empty };
                    _userSettingsStore.Save(resetSettings);
                }
            }
            finally
            {
                IsBusy = false;
            }
        }
    }

    private bool ValidateFields()
    {
        UsernameValidationError = UsernameRules.Validate(Username);
        PasswordValidationError = ValidatePassword();

        bool isValid = !HasUsernameError && !HasPasswordError;

        return isValid;
    }

    private FieldValidationError ValidatePassword()
    {
        FieldValidationError validationError = FieldValidationError.None;

        if (string.IsNullOrEmpty(Password))
        {
            validationError = FieldValidationError.Required;
        }

        return validationError;
    }

    private void HandleLoginFailure(AuthResult result)
    {
        if (result == AuthResult.ServerUnreachable)
        {
            LoginStatus = LoginStatus.ServerUnreachable;
        }
        else if (result == AuthResult.InvalidCredentials || result == AuthResult.UserNotFound)
        {
            _failedAttemptsCount++;

            if (_failedAttemptsCount >= MaxFailedAttemptsThreshold)
            {
                LoginStatus = LoginStatus.TooManyAttempts;
            }
            else
            {
                LoginStatus = LoginStatus.InvalidCredentials;
            }
        }
        else
        {
            LoginStatus = LoginStatus.InvalidCredentials;
        }
    }

    private void ClearStatus()
    {
        LoginStatus = LoginStatus.None;
    }
}
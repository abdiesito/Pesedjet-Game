using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Pesedjet.Client.Networking.Interfaces;
using Pesedjet.Client.Networking.Results;
using Pesedjet.Client.UI.Utilities.Dialogs;
using Pesedjet.Client.UI.Utilities.Localization;
using Pesedjet.Client.UI.Utilities.Navigation;
using Pesedjet.Client.UI.Utilities.Session;
using Pesedjet.Client.UI.Utilities.Validation;
using Pesedjet.Server.Contracts.DTOs;
using Pesedjet.Client.UI.Utilities.Threading;
using Pesedjet.Client.UI.ViewModels.Dependencies;
using Pesedjet.Server.Contracts.Faults;
using Pesedjet.Client.UI.ViewModels.Dialogs;
using Pesedjet.Client.UI.Models.Account;
using Pesedjet.Client.UI.Models.DialogParameters;

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

    public AccessMenuViewModel(AccessMenuDependencies dependencies)
    {
        _navigator = dependencies.Navigator;
        _dialogManager = dependencies.DialogManager;
        _authClient = dependencies.AuthClient;
        _playerSession = dependencies.PlayerSession;
        _userSettingsStore = dependencies.UserSettingsStore;

        ResumeSessionIfSaved();
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
    private void Login()
    {
        Username = Username.Trim();

        if (ValidateFields() && !IsBusy)
        {
            IsBusy = true;
            ClearStatus();

            UserLoginRequest request = new UserLoginRequest
            {
                Username = Username,
                Password = Password,
                RememberSession = RememberSession
            };

            BackgroundJob<UserLoginRequest, ClientResult<UserSession>> job = new()
            {
                Work = _authClient.Login,
                State = request,
                OnCompleted = OnLoginCompleted,
                OnFailed = OnLoginFailed
            };

            BackgroundWork.Start(job);
        }
    }

    private void OnLoginCompleted(ClientResult<UserSession> result, UserLoginRequest request)
    {
        ApplyLoginResult(result);
    }

    private void OnLoginFailed(Exception error, UserLoginRequest request)
    {
        ApplyLoginResult(ClientResult<UserSession>.TechnicalFailure(FailureKind.Unexpected));
    }

    private void ApplyLoginResult(ClientResult<UserSession> result)
    {
        IsBusy = false;

        if (result.IsSuccess)
        {
            UserSession session = result.Value!;

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

            _playerSession.PlayerId = session.PlayerId;
            _playerSession.CurrentPlayer = profile;
            _navigator.NavigateTo<MainMenuViewModel>();
        }
        else
        {
            HandleLoginFailure(result);
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

    private void ResumeSessionIfSaved()
    {
        UserSettings settings = _userSettingsStore.Load();

        if (!string.IsNullOrWhiteSpace(settings.RememberToken))
        {
            IsBusy = true;

            BackgroundJob<UserSettings, ClientResult<UserSession>> job = new()
            {
                Work = ResumeSessionOnWorker,
                State = settings,
                OnCompleted = OnResumeCompleted,
                OnFailed = OnResumeFailed
            };

            BackgroundWork.Start(job);
        }
    }

    private ClientResult<UserSession> ResumeSessionOnWorker(UserSettings settings)
    {
        return _authClient.ResumeSession(settings.RememberToken);
    }

    private void OnResumeCompleted(ClientResult<UserSession> result, UserSettings settings)
    {
        ApplyResumeResult(result, settings);
    }

    private void OnResumeFailed(Exception error, UserSettings settings)
    {
        ApplyResumeResult(ClientResult<UserSession>.TechnicalFailure(FailureKind.Unexpected), settings);
    }

    private void ApplyResumeResult(ClientResult<UserSession> result, UserSettings settings)
    {
        IsBusy = false;

        if (result.IsSuccess)
        {
            UserSession session = result.Value!;

            PlayerProfile profile = new PlayerProfile(
                Username: session.Username ?? string.Empty,
                Points: 0,
                Victories: 0
            );

            _playerSession.PlayerId = session.PlayerId;
            _playerSession.CurrentPlayer = profile;
            _navigator.NavigateTo<MainMenuViewModel>();
        }
        else
        {
            UserSettings resetSettings = settings with { RememberToken = string.Empty };
            _userSettingsStore.Save(resetSettings);
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

    private void HandleLoginFailure(ClientResult<UserSession> result)
    {
        bool isRejectedCredentials = result.Failure == FailureKind.Business
                                     && (result.ErrorCode == ServiceErrorCodes.InvalidCredentials
                                         || result.ErrorCode == ServiceErrorCodes.UserNotFound);

        if (result.IsConnectionFailure)
        {
            LoginStatus = LoginStatus.ServerUnreachable;
        }
        else if (isRejectedCredentials)
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
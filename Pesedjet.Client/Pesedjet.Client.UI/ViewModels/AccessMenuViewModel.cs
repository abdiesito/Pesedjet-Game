using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Pesedjet.Client.UI.Models;
using Pesedjet.Client.UI.Utilities.Dialogs;
using Pesedjet.Client.UI.Utilities.Navigation;
using Pesedjet.Client.UI.Utilities.Validation;

namespace Pesedjet.Client.UI.ViewModels;

public partial class AccessMenuViewModel : ViewModelBase, INavigable
{
    private readonly INavigator _navigator;
    private readonly IDialogManager _dialogManager;

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

    public AccessMenuViewModel(INavigator navigator, IDialogManager dialogManager)
    {
        _navigator = navigator;
        _dialogManager = dialogManager;
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

        if (ValidateFields())
        {
            ClearStatus();
            // TODO: Authenticate through the player client with a 15 second timeout, show ServerUnreachable on
            // communication failures (EX-01) and redirect to forgot password after 5 failed attempts.
        }
    }

    [RelayCommand]
    private void NavigateToForgotPassword()
    {
        // TODO: Navigate to ForgotPasswordViewModel when it exists.
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

    private bool ValidateFields()
    {
        UsernameValidationError = UsernameRules.Validate(Username);
        PasswordValidationError = ValidatePassword();

        return !HasUsernameError && !HasPasswordError;
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

    private void ClearStatus()
    {
        LoginStatus = LoginStatus.None;
    }
}

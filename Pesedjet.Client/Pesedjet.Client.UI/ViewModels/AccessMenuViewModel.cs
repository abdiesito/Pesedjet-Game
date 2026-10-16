using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Pesedjet.Client.UI.Models;
using Pesedjet.Client.UI.Utilities.Navigation;

namespace Pesedjet.Client.UI.ViewModels;

public partial class AccessMenuViewModel : ViewModelBase, INavigable
{
    private const int MaxFailedAttempts = 5;                                        
    private static readonly TimeSpan LoginTimeout = TimeSpan.FromSeconds(15);       
    private static readonly TimeSpan RedirectDelay = TimeSpan.FromSeconds(1.5);     
    private static readonly Regex UsernamePattern = new(@"^[\p{L}\p{N}]{3,50}$", RegexOptions.Compiled); 

    private readonly INavigator _navigator;
    //private readonly IPlayerService _playerService;
    private int _failedAttempts;

    [ObservableProperty] private bool _isBusy;
    [ObservableProperty] private string _username = string.Empty;
    [ObservableProperty] private string _password = string.Empty;
    [ObservableProperty] private bool _rememberSession;
    [ObservableProperty] private bool _isPasswordVisible;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasUsernameError), nameof(IsUsernameRequired), nameof(IsUsernameFormatInvalid))]
    private FieldValidationError _usernameValidationError;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasPasswordError), nameof(IsPasswordRequired))]
    private FieldValidationError _passwordValidationError;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsInvalidCredentials), nameof(IsTooManyAttempts), nameof(IsServerUnreachable))]
    private LoginStatus _loginStatus;

    public AccessMenuViewModel(INavigator navigator/*, IPlayerService playerService*/)
    {
        _navigator = navigator;
        //_playerService = playerService;
    }
    
    public void OnNavigatedFrom()
    {
        Password = string.Empty;
        IsPasswordVisible = false;
    }

    public bool HasUsernameError => UsernameValidationError != FieldValidationError.None;
    public bool IsUsernameRequired => UsernameValidationError == FieldValidationError.Required;
    public bool IsUsernameFormatInvalid => UsernameValidationError == FieldValidationError.InvalidFormat;

    public bool HasPasswordError => PasswordValidationError != FieldValidationError.None;
    public bool IsPasswordRequired => PasswordValidationError == FieldValidationError.Required;

    public bool IsInvalidCredentials => LoginStatus == LoginStatus.InvalidCredentials;
    public bool IsTooManyAttempts => LoginStatus == LoginStatus.TooManyAttempts;
    public bool IsServerUnreachable => LoginStatus == LoginStatus.ServerUnreachable;

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
        if (!ValidateFields())                      
        {
            return;
        }

        IsBusy = true;
        ClearStatus();
        try
        {
            
            /*var player = await _playerService
                .AuthenticatePlayerAsync(Username, Password)
                .WaitAsync(LoginTimeout);

            if (player is null)
            {
                await HandleFailedAttemptAsync();
                return;
            }*/

            _failedAttempts = 0;
            //_navigator.NavigateAndClear<MenuViewModel>();
        }
        catch (Exception)
        {
            LoginStatus = LoginStatus.ServerUnreachable; // TODO: Manage Use Case EX-01
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private void NavigateToForgotPassword()
    {
        // _navigator.NavigateTo<ForgotPasswordViewModel>();
    }

    [RelayCommand]
    private void NavigateToRegisterAccount()
    {
        _navigator.NavigateTo<RegisterAccountViewModel>();
    }

    [RelayCommand]
    private void PlayAsGuest()
    {
        // await _dialogManager.ShowAsync<GuestModeNoticeViewModel>();
    }

    private bool ValidateFields()
    {
        UsernameValidationError = string.IsNullOrEmpty(Username) ? FieldValidationError.Required 
            : UsernamePattern.IsMatch(Username) ? FieldValidationError.None 
            : FieldValidationError.InvalidFormat;

        PasswordValidationError = string.IsNullOrEmpty(Password) ? FieldValidationError.Required 
            : FieldValidationError.None;

        return !HasUsernameError && !HasPasswordError;
    }

    private async Task HandleFailedAttemptAsync()
    {
        _failedAttempts++;
        Password = string.Empty;

        if (_failedAttempts < MaxFailedAttempts)
        {
            LoginStatus = LoginStatus.InvalidCredentials;
            return;
        }

        _failedAttempts = 0;
        LoginStatus = LoginStatus.TooManyAttempts;
        await Task.Delay(RedirectDelay);
        NavigateToForgotPassword();
    }

    private void ClearStatus() => LoginStatus = LoginStatus.None;
}

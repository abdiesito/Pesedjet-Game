using System;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Pesedjet.Client.Networking.Proxies;
using Pesedjet.Client.Networking.Results;
using Pesedjet.Client.UI.Models;
using Pesedjet.Client.UI.Utilities.Localization;
using Pesedjet.Client.UI.Utilities.Navigation;
using Pesedjet.Client.UI.Utilities.Validation;
using Pesedjet.Server.Contracts.DTOs;

namespace Pesedjet.Client.UI.ViewModels;

public partial class RegisterAccountViewModel : ViewModelBase
{
    private const int MinimumAgeInYears = 12;
    private const int PasswordMinimumLength = 8;
    private const int RegexTimeoutMilliseconds = 250;
    private const string BirthDateFormatKey = "RegisterAccount.BirthDateFormat";

    private static readonly Regex EmailPattern = new(
        @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant,
        TimeSpan.FromMilliseconds(RegexTimeoutMilliseconds));

    private readonly INavigator _navigator;
    private readonly TimeProvider _timeProvider;
    private readonly IAccountClient _accountClient;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string _fullName = string.Empty;

    [ObservableProperty]
    private string _username = string.Empty;

    [ObservableProperty]
    private string _birthDate = string.Empty;

    [ObservableProperty]
    private string _email = string.Empty;

    [ObservableProperty]
    private string _password = string.Empty;

    [ObservableProperty]
    private string _confirmPassword = string.Empty;

    [ObservableProperty]
    private bool _isPasswordVisible;

    [ObservableProperty]
    private bool _isConfirmPasswordVisible;

    [ObservableProperty]
    private bool _hasMinimumLength;

    [ObservableProperty]
    private bool _hasUppercase;

    [ObservableProperty]
    private bool _hasNumber;

    [ObservableProperty]
    private bool _hasSpecialCharacter;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasFullNameError), nameof(IsFullNameRequired), nameof(HasValidationSummary))]
    private FieldValidationError _fullNameValidationError;

    [ObservableProperty]
    [NotifyPropertyChangedFor(
        nameof(HasUsernameError),
        nameof(IsUsernameRequired),
        nameof(IsUsernameFormatInvalid),
        nameof(IsUsernameAlreadyRegistered),
        nameof(HasValidationSummary))]
    private FieldValidationError _usernameValidationError;

    [ObservableProperty]
    [NotifyPropertyChangedFor(
        nameof(HasBirthDateError),
        nameof(IsBirthDateRequired),
        nameof(IsBirthDateFormatInvalid),
        nameof(IsBirthDateUnderage),
        nameof(HasValidationSummary))]
    private FieldValidationError _birthDateValidationError;

    [ObservableProperty]
    [NotifyPropertyChangedFor(
        nameof(HasEmailError),
        nameof(IsEmailRequired),
        nameof(IsEmailFormatInvalid),
        nameof(IsEmailAlreadyRegistered),
        nameof(HasValidationSummary))]
    private FieldValidationError _emailValidationError;

    [ObservableProperty]
    [NotifyPropertyChangedFor(
        nameof(HasPasswordError),
        nameof(IsPasswordRequired),
        nameof(ArePasswordRequirementsUnmet),
        nameof(HasValidationSummary))]
    private FieldValidationError _passwordValidationError;

    [ObservableProperty]
    [NotifyPropertyChangedFor(
        nameof(HasConfirmPasswordError),
        nameof(IsConfirmPasswordRequired),
        nameof(IsConfirmPasswordMismatch),
        nameof(HasValidationSummary))]
    private FieldValidationError _confirmPasswordValidationError;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsServerUnreachable), nameof(IsRegistrationFailed))]
    private RegistrationStatus _registrationStatus;

    public bool HasFullNameError => FullNameValidationError != FieldValidationError.None;

    public bool IsFullNameRequired => FullNameValidationError == FieldValidationError.Required;

    public bool HasUsernameError => UsernameValidationError != FieldValidationError.None;

    public bool IsUsernameRequired => UsernameValidationError == FieldValidationError.Required;

    public bool IsUsernameFormatInvalid => UsernameValidationError == FieldValidationError.InvalidFormat;

    public bool IsUsernameAlreadyRegistered => UsernameValidationError == FieldValidationError.AlreadyRegistered;

    public bool HasBirthDateError => BirthDateValidationError != FieldValidationError.None;

    public bool IsBirthDateRequired => BirthDateValidationError == FieldValidationError.Required;

    public bool IsBirthDateFormatInvalid => BirthDateValidationError == FieldValidationError.InvalidFormat;

    public bool IsBirthDateUnderage => BirthDateValidationError == FieldValidationError.Underage;

    public bool HasEmailError => EmailValidationError != FieldValidationError.None;

    public bool IsEmailRequired => EmailValidationError == FieldValidationError.Required;

    public bool IsEmailFormatInvalid => EmailValidationError == FieldValidationError.InvalidFormat;

    public bool IsEmailAlreadyRegistered => EmailValidationError == FieldValidationError.AlreadyRegistered;

    public bool HasPasswordError => PasswordValidationError != FieldValidationError.None;

    public bool IsPasswordRequired => PasswordValidationError == FieldValidationError.Required;

    public bool ArePasswordRequirementsUnmet =>
        PasswordValidationError == FieldValidationError.PasswordRequirementsNotMet;

    public bool HasConfirmPasswordError => ConfirmPasswordValidationError != FieldValidationError.None;

    public bool IsConfirmPasswordRequired => ConfirmPasswordValidationError == FieldValidationError.Required;

    public bool IsConfirmPasswordMismatch =>
        ConfirmPasswordValidationError == FieldValidationError.PasswordsDoNotMatch;

    public bool HasValidationSummary =>
        HasFullNameError || HasUsernameError || HasBirthDateError ||
        HasEmailError || HasPasswordError || HasConfirmPasswordError;

    public bool IsServerUnreachable => RegistrationStatus == RegistrationStatus.ServerUnreachable;

    public bool IsRegistrationFailed => RegistrationStatus == RegistrationStatus.Failed;

    public RegisterAccountViewModel(
        INavigator navigator,
        TimeProvider timeProvider,
        IAccountClient accountClient)
    {
        _navigator = navigator;
        _timeProvider = timeProvider;
        _accountClient = accountClient;
    }

    partial void OnFullNameChanged(string value)
    {
        FullNameValidationError = FieldValidationError.None;
    }

    partial void OnUsernameChanged(string value)
    {
        UsernameValidationError = FieldValidationError.None;
    }

    partial void OnBirthDateChanged(string value)
    {
        BirthDateValidationError = FieldValidationError.None;
    }

    partial void OnEmailChanged(string value)
    {
        EmailValidationError = FieldValidationError.None;
    }

    partial void OnPasswordChanged(string value)
    {
        PasswordValidationError = FieldValidationError.None;
        ConfirmPasswordValidationError = FieldValidationError.None;
        HasMinimumLength = value.Length >= PasswordMinimumLength;
        HasUppercase = value.Any(char.IsUpper);
        HasNumber = value.Any(char.IsDigit);
        HasSpecialCharacter = value.Any(character => !char.IsLetterOrDigit(character));
    }

    partial void OnConfirmPasswordChanged(string value)
    {
        ConfirmPasswordValidationError = FieldValidationError.None;
    }

    [RelayCommand]
    private async Task RegisterAsync()
    {
        TrimTextFields();
        RegistrationStatus = RegistrationStatus.None;

        if (ValidateFields())
        {
            IsBusy = true;

            try
            {
                AccountRegistrationResult result = await _accountClient.RegisterAccountAsync(CreateRegisterRequest());
                ApplyRegistrationResult(result);
            }
            finally
            {
                IsBusy = false;
            }
        }
    }

    [RelayCommand]
    private void Cancel()
    {
        // TODO: CU-002 FA-07: Ask for confirmation through IDialogManager before leaving when data was typed.
        _navigator.GoBack();
    }

    private UserRegisterRequest CreateRegisterRequest()
    {
        TryParseBirthDate(out DateOnly birthDate);

        return new UserRegisterRequest
        {
            FullName = FullName,
            Username = Username,
            Email = Email,
            Password = Password,
            BirthDate = birthDate.ToDateTime(TimeOnly.MinValue)
        };
    }

    private void ApplyRegistrationResult(AccountRegistrationResult result)
    {
        switch (result)
        {
            case AccountRegistrationResult.Registered:
                // TODO: Navigate to the verification code screen when it exists.
                _navigator.NavigateAndClear<AccessMenuViewModel>();
                break;
            case AccountRegistrationResult.UsernameAlreadyRegistered:
                UsernameValidationError = FieldValidationError.AlreadyRegistered;
                break;
            case AccountRegistrationResult.EmailAlreadyRegistered:
                EmailValidationError = FieldValidationError.AlreadyRegistered;
                break;
            case AccountRegistrationResult.ServerUnreachable:
                RegistrationStatus = RegistrationStatus.ServerUnreachable;
                break;
            default:
                RegistrationStatus = RegistrationStatus.Failed;
                break;
        }
    }

    private void TrimTextFields()
    {
        FullName = FullName.Trim();
        Username = Username.Trim();
        BirthDate = BirthDate.Trim();
        Email = Email.Trim();
    }

    private bool ValidateFields()
    {
        FullNameValidationError = ValidateFullName();
        UsernameValidationError = UsernameRules.Validate(Username);
        BirthDateValidationError = ValidateBirthDate();
        EmailValidationError = ValidateEmail();
        PasswordValidationError = ValidatePassword();
        ConfirmPasswordValidationError = ValidateConfirmPassword();

        return !HasValidationSummary;
    }

    private FieldValidationError ValidateFullName()
    {
        FieldValidationError validationError = FieldValidationError.None;

        if (string.IsNullOrEmpty(FullName))
        {
            validationError = FieldValidationError.Required;
        }

        return validationError;
    }

    private FieldValidationError ValidateEmail()
    {
        FieldValidationError validationError = FieldValidationError.None;

        if (string.IsNullOrEmpty(Email))
        {
            validationError = FieldValidationError.Required;
        }
        else if (!EmailPattern.IsMatch(Email))
        {
            validationError = FieldValidationError.InvalidFormat;
        }

        return validationError;
    }

    private FieldValidationError ValidatePassword()
    {
        FieldValidationError validationError = FieldValidationError.None;

        if (string.IsNullOrEmpty(Password))
        {
            validationError = FieldValidationError.Required;
        }
        else if (!AreAllPasswordRequirementsMet())
        {
            validationError = FieldValidationError.PasswordRequirementsNotMet;
        }

        return validationError;
    }

    private FieldValidationError ValidateConfirmPassword()
    {
        FieldValidationError validationError = FieldValidationError.None;

        if (string.IsNullOrEmpty(ConfirmPassword))
        {
            validationError = FieldValidationError.Required;
        }
        else if (!string.Equals(Password, ConfirmPassword, StringComparison.Ordinal))
        {
            validationError = FieldValidationError.PasswordsDoNotMatch;
        }

        return validationError;
    }

    private FieldValidationError ValidateBirthDate()
    {
        FieldValidationError validationError = FieldValidationError.None;

        if (string.IsNullOrEmpty(BirthDate))
        {
            validationError = FieldValidationError.Required;
        }
        else if (!TryParseBirthDate(out DateOnly birthDate))
        {
            validationError = FieldValidationError.InvalidFormat;
        }
        else
        {
            validationError = ClassifyBirthDate(birthDate);
        }

        return validationError;
    }

    private bool TryParseBirthDate(out DateOnly birthDate)
    {
        string format = LocalizationManager.Instance[BirthDateFormatKey];

        return DateOnly.TryParseExact(
            BirthDate, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out birthDate);
    }

    private FieldValidationError ClassifyBirthDate(DateOnly birthDate)
    {
        DateOnly today = DateOnly.FromDateTime(_timeProvider.GetLocalNow().DateTime);
        FieldValidationError validationError = FieldValidationError.None;

        if (birthDate > today)
        {
            validationError = FieldValidationError.InvalidFormat;
        }
        else if (birthDate.AddYears(MinimumAgeInYears) > today)
        {
            validationError = FieldValidationError.Underage;
        }

        return validationError;
    }

    private bool AreAllPasswordRequirementsMet()
    {
        return HasMinimumLength && HasUppercase && HasNumber && HasSpecialCharacter;
    }
}

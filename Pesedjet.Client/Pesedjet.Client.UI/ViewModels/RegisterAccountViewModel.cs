using System;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Pesedjet.Client.UI.Models;
using Pesedjet.Client.UI.Utilities.Localization;
using Pesedjet.Client.UI.Utilities.Navigation;

namespace Pesedjet.Client.UI.ViewModels;

public partial class RegisterAccountViewModel : ViewModelBase
{
    private const int MinimumAgeInYears = 12;
    private const int PasswordMinimumLength = 8;
    private const int UsernameMinimumLength = 3;
    private const int UsernameMaximumLength = 50;
    private const string BirthDateFormatKey = "RegisterAccount.BirthDateFormat";

    private static readonly Regex UsernamePattern =
        new($@"^[\p{{L}}\p{{N}}]{{{UsernameMinimumLength},{UsernameMaximumLength}}}$", RegexOptions.Compiled);

    private static readonly Regex EmailPattern = new(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled);

    private readonly INavigator _navigator;
    private readonly TimeProvider _timeProvider;

    [ObservableProperty] private bool _isBusy;
    [ObservableProperty] private string _fullName = string.Empty;
    [ObservableProperty] private string _username = string.Empty;
    [ObservableProperty] private string _birthDate = string.Empty;
    [ObservableProperty] private string _email = string.Empty;
    [ObservableProperty] private string _password = string.Empty;
    [ObservableProperty] private string _confirmPassword = string.Empty;
    [ObservableProperty] private bool _isPasswordVisible;
    [ObservableProperty] private bool _isConfirmPasswordVisible;
    [ObservableProperty] private bool _hasMinimumLength;
    [ObservableProperty] private bool _hasUppercase;
    [ObservableProperty] private bool _hasNumber;
    [ObservableProperty] private bool _hasSpecialCharacter;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasFullNameError), nameof(IsFullNameRequired), nameof(HasValidationSummary))]
    private FieldValidationError _fullNameValidationError;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasUsernameError), nameof(IsUsernameRequired), nameof(IsUsernameFormatInvalid), nameof(HasValidationSummary))]
    private FieldValidationError _usernameValidationError;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasBirthDateError), nameof(IsBirthDateRequired), nameof(IsBirthDateInvalid), nameof(IsBirthDateUnderage), nameof(HasValidationSummary))]
    private FieldValidationError _birthDateValidationError;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasEmailError), nameof(IsEmailRequired), nameof(IsEmailInvalid), nameof(HasValidationSummary))]
    private FieldValidationError _emailValidationError;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasPasswordError), nameof(IsPasswordRequired), nameof(ArePasswordRequirementsUnmet), nameof(HasValidationSummary))]
    private FieldValidationError _passwordValidationError;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasConfirmPasswordError), nameof(IsConfirmPasswordRequired), nameof(IsConfirmPasswordMismatch), nameof(HasValidationSummary))]
    private FieldValidationError _confirmPasswordValidationError;

    public RegisterAccountViewModel(INavigator navigator, TimeProvider timeProvider)
    {
        _navigator = navigator;
        _timeProvider = timeProvider;
    }
    
    public bool HasFullNameError => FullNameValidationError != FieldValidationError.None;
    public bool IsFullNameRequired => FullNameValidationError == FieldValidationError.Required;

    public bool HasUsernameError => UsernameValidationError != FieldValidationError.None;
    public bool IsUsernameRequired => UsernameValidationError == FieldValidationError.Required;
    public bool IsUsernameFormatInvalid => UsernameValidationError == FieldValidationError.InvalidFormat;

    public bool HasBirthDateError => BirthDateValidationError != FieldValidationError.None;
    public bool IsBirthDateRequired => BirthDateValidationError == FieldValidationError.Required;
    public bool IsBirthDateInvalid => BirthDateValidationError == FieldValidationError.InvalidFormat;
    public bool IsBirthDateUnderage => BirthDateValidationError == FieldValidationError.Underage;

    public bool HasEmailError => EmailValidationError != FieldValidationError.None;
    public bool IsEmailRequired => EmailValidationError == FieldValidationError.Required;
    public bool IsEmailInvalid => EmailValidationError == FieldValidationError.InvalidFormat;

    public bool HasPasswordError => PasswordValidationError != FieldValidationError.None;
    public bool IsPasswordRequired => PasswordValidationError == FieldValidationError.Required;
    public bool ArePasswordRequirementsUnmet => PasswordValidationError == FieldValidationError.PasswordRequirementsNotMet;

    public bool HasConfirmPasswordError => ConfirmPasswordValidationError != FieldValidationError.None;
    public bool IsConfirmPasswordRequired => ConfirmPasswordValidationError == FieldValidationError.Required;
    public bool IsConfirmPasswordMismatch => ConfirmPasswordValidationError == FieldValidationError.PasswordsDoNotMatch;

    public bool HasValidationSummary =>
        HasFullNameError || HasUsernameError || HasBirthDateError ||
        HasEmailError || HasPasswordError || HasConfirmPasswordError;
    
    partial void OnFullNameChanged(string value) => FullNameValidationError = FieldValidationError.None;

    partial void OnUsernameChanged(string value) => UsernameValidationError = FieldValidationError.None;

    partial void OnBirthDateChanged(string value) => BirthDateValidationError = FieldValidationError.None;

    partial void OnEmailChanged(string value) => EmailValidationError = FieldValidationError.None;

    partial void OnPasswordChanged(string value)
    {
        PasswordValidationError = FieldValidationError.None;
        ConfirmPasswordValidationError = FieldValidationError.None;
        HasMinimumLength = value.Length >= PasswordMinimumLength;
        HasUppercase = value.Any(char.IsUpper);
        HasNumber = value.Any(char.IsDigit);
        HasSpecialCharacter = value.Any(character => !char.IsLetterOrDigit(character));
    }

    partial void OnConfirmPasswordChanged(string value) => ConfirmPasswordValidationError = FieldValidationError.None;

    [RelayCommand]
    private Task RegisterAsync()
    {
        TrimTextFields();
        if (!ValidateFields())
        {
            return Task.CompletedTask;
        }

        // Pendiente (CU-002): REGISTER_REQUEST comprueba correo y usuario (MSG-REG-002, MSG-REG-003) con IsBusy activo
        // y, si todo está libre, abre MOD-003 (verificar identidad) con IDialogManager; al verificar, GUI-002.
        return Task.CompletedTask;
    }

    [RelayCommand]
    private void Cancel()
    {
        // Pendiente (CU-002 FA-07): si ya hay datos escritos, pedir confirmación con IDialogManager antes de salir.
        _navigator.GoBack();
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
        FullNameValidationError = string.IsNullOrEmpty(FullName) ? FieldValidationError.Required : FieldValidationError.None;

        UsernameValidationError = string.IsNullOrEmpty(Username) ? FieldValidationError.Required
                                : UsernamePattern.IsMatch(Username) ? FieldValidationError.None
                                : FieldValidationError.InvalidFormat;

        BirthDateValidationError = ValidateBirthDate();

        EmailValidationError = string.IsNullOrEmpty(Email) ? FieldValidationError.Required
                             : EmailPattern.IsMatch(Email) ? FieldValidationError.None
                             : FieldValidationError.InvalidFormat;

        PasswordValidationError = string.IsNullOrEmpty(Password) ? FieldValidationError.Required
                                : AllPasswordRequirementsMet() ? FieldValidationError.None
                                : FieldValidationError.PasswordRequirementsNotMet;

        ConfirmPasswordValidationError = string.IsNullOrEmpty(ConfirmPassword) ? FieldValidationError.Required
                                       : string.Equals(Password, ConfirmPassword, StringComparison.Ordinal) ? FieldValidationError.None
                                       : FieldValidationError.PasswordsDoNotMatch;

        return !HasValidationSummary;
    }

    private FieldValidationError ValidateBirthDate()
    {
        if (string.IsNullOrEmpty(BirthDate))
        {
            return FieldValidationError.Required;
        }
        
        var format = LocalizationManager.Instance[BirthDateFormatKey];
        if (!DateOnly.TryParseExact(BirthDate, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out var birthDate))
        {
            return FieldValidationError.InvalidFormat;
        }

        var today = DateOnly.FromDateTime(_timeProvider.GetLocalNow().DateTime);
        if (birthDate > today)
        {
            return FieldValidationError.InvalidFormat;
        }

        return birthDate.AddYears(MinimumAgeInYears) > today ? FieldValidationError.Underage : FieldValidationError.None;
    }

    private bool AllPasswordRequirementsMet() =>
        HasMinimumLength && HasUppercase && HasNumber && HasSpecialCharacter;
}

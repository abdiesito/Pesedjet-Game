using System;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Pesedjet.Client.UI.Models;

namespace Pesedjet.Client.UI.ViewModels;

public partial class AccountSettingsViewModel : ViewModelBase
{
    private const int PasswordMinimumLength = 8;

    [ObservableProperty]
    private string _currentPassword = string.Empty;

    [ObservableProperty]
    private string _newPassword = string.Empty;

    [ObservableProperty]
    private string _confirmPassword = string.Empty;

    [ObservableProperty]
    private bool _isCurrentPasswordVisible;

    [ObservableProperty]
    private bool _isNewPasswordVisible;

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
    [NotifyPropertyChangedFor(nameof(HasCurrentPasswordError), nameof(IsCurrentPasswordRequired))]
    private FieldValidationError _currentPasswordValidationError;

    [ObservableProperty]
    [NotifyPropertyChangedFor(
        nameof(HasNewPasswordError),
        nameof(IsNewPasswordRequired),
        nameof(ArePasswordRequirementsUnmet),
        nameof(IsNewPasswordUnchanged))]
    private FieldValidationError _newPasswordValidationError;

    [ObservableProperty]
    [NotifyPropertyChangedFor(
        nameof(HasConfirmPasswordError),
        nameof(IsConfirmPasswordRequired),
        nameof(IsConfirmPasswordMismatch))]
    private FieldValidationError _confirmPasswordValidationError;

    public bool HasCurrentPasswordError => CurrentPasswordValidationError != FieldValidationError.None;

    public bool IsCurrentPasswordRequired => CurrentPasswordValidationError == FieldValidationError.Required;

    public bool HasNewPasswordError => NewPasswordValidationError != FieldValidationError.None;

    public bool IsNewPasswordRequired => NewPasswordValidationError == FieldValidationError.Required;

    public bool ArePasswordRequirementsUnmet =>
        NewPasswordValidationError == FieldValidationError.PasswordRequirementsNotMet;

    public bool IsNewPasswordUnchanged => NewPasswordValidationError == FieldValidationError.PasswordUnchanged;

    public bool HasConfirmPasswordError => ConfirmPasswordValidationError != FieldValidationError.None;

    public bool IsConfirmPasswordRequired => ConfirmPasswordValidationError == FieldValidationError.Required;

    public bool IsConfirmPasswordMismatch =>
        ConfirmPasswordValidationError == FieldValidationError.PasswordsDoNotMatch;

    partial void OnCurrentPasswordChanged(string value)
    {
        CurrentPasswordValidationError = FieldValidationError.None;
    }

    partial void OnNewPasswordChanged(string value)
    {
        NewPasswordValidationError = FieldValidationError.None;
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
    private void ChangePassword()
    {
        CurrentPasswordValidationError = ValidateCurrentPassword();
        NewPasswordValidationError = ValidateNewPassword();
        ConfirmPasswordValidationError = ValidateConfirmPassword();

        if (!HasCurrentPasswordError && !HasNewPasswordError && !HasConfirmPasswordError)
        {
            // TODO: Send CHANGE_PASSWORD_REQUEST, verify identity through MOD-003 and show MSG-REC-003 (CU-018).
        }
    }

    [RelayCommand]
    private void CancelPasswordChange()
    {
        CurrentPassword = string.Empty;
        NewPassword = string.Empty;
        ConfirmPassword = string.Empty;
        IsCurrentPasswordVisible = false;
        IsNewPasswordVisible = false;
        IsConfirmPasswordVisible = false;
        CurrentPasswordValidationError = FieldValidationError.None;
    }

    [RelayCommand]
    private void DeleteAccount()
    {
        // TODO: Confirm by typing the username through IDialogManager, then verify identity through MOD-003 (CU-015).
    }

    private FieldValidationError ValidateCurrentPassword()
    {
        FieldValidationError validationError = FieldValidationError.None;

        if (string.IsNullOrEmpty(CurrentPassword))
        {
            validationError = FieldValidationError.Required;
        }

        return validationError;
    }

    private FieldValidationError ValidateNewPassword()
    {
        FieldValidationError validationError = FieldValidationError.None;

        if (string.IsNullOrEmpty(NewPassword))
        {
            validationError = FieldValidationError.Required;
        }
        else if (!AreAllPasswordRequirementsMet())
        {
            validationError = FieldValidationError.PasswordRequirementsNotMet;
        }
        else if (string.Equals(NewPassword, CurrentPassword, StringComparison.Ordinal))
        {
            validationError = FieldValidationError.PasswordUnchanged;
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
        else if (!string.Equals(NewPassword, ConfirmPassword, StringComparison.Ordinal))
        {
            validationError = FieldValidationError.PasswordsDoNotMatch;
        }

        return validationError;
    }

    private bool AreAllPasswordRequirementsMet()
    {
        return HasMinimumLength && HasUppercase && HasNumber && HasSpecialCharacter;
    }
}

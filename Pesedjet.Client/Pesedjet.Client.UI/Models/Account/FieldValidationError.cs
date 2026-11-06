namespace Pesedjet.Client.UI.Models.Account;

public enum FieldValidationError
{
    None,
    Required,
    InvalidFormat,
    Underage,
    PasswordRequirementsNotMet,
    PasswordsDoNotMatch,
    AlreadyRegistered,
    PasswordUnchanged
}

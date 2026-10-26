namespace Pesedjet.Client.UI.Models;

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

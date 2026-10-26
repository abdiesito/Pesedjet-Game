using System;
using System.Text.RegularExpressions;
using Pesedjet.Client.UI.Models;

namespace Pesedjet.Client.UI.Utilities.Validation;

public static class UsernameRules
{
    public const int MinimumLength = 3;
    public const int MaximumLength = 50;

    private const int RegexTimeoutMilliseconds = 250;

    private static readonly TimeSpan RegexTimeout = TimeSpan.FromMilliseconds(RegexTimeoutMilliseconds);

    private static readonly Regex UsernamePattern = new(
        $@"^[\p{{L}}\p{{N}}]{{{MinimumLength},{MaximumLength}}}$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant,
        RegexTimeout);

    public static FieldValidationError Validate(string username)
    {
        FieldValidationError validationError = FieldValidationError.None;

        if (string.IsNullOrEmpty(username))
        {
            validationError = FieldValidationError.Required;
        }
        else if (!UsernamePattern.IsMatch(username))
        {
            validationError = FieldValidationError.InvalidFormat;
        }

        return validationError;
    }
}

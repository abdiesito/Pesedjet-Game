namespace Pesedjet.Client.UI.Utilities.Localization;

public sealed record UserSettings(
    string Language, 
    string RememberToken = "",
    bool AutoResume = false)
{
    // What a machine without a settings file, or with an unreadable one, starts with.
    public static UserSettings Default { get; } = new(string.Empty);
}
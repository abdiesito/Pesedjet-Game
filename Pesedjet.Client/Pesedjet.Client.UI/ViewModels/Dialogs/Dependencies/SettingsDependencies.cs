using Pesedjet.Client.UI.Utilities.Session;

namespace Pesedjet.Client.UI.ViewModels.Dialogs.Dependencies;

public sealed record SettingsDependencies(
    AudioSettingsViewModel AudioSettings,
    DisplaySettingsViewModel DisplaySettings,
    LanguageSettingsViewModel LanguageSettings,
    AccountSettingsViewModel AccountSettings,
    IPlayerSession PlayerSession);

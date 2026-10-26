using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Pesedjet.Client.UI.Resources;

namespace Pesedjet.Client.UI.Utilities.Localization;

public sealed class LocalizationManager : INotifyPropertyChanged
{
    private const string DefaultCultureCode = "es-MX";
    private const string AllPropertiesChanged = "";

    private static readonly string[] SupportedCultureCodes = [DefaultCultureCode, "en-US"];

    private readonly UserSettingsStore _settingsStore;
    private readonly object _cultureChangeLock = new();
    private readonly object _settingsWriteLock = new();
    private volatile CultureInfo _activeCulture = CultureInfo.InvariantCulture;

    public static IReadOnlyList<LanguageOption> AvailableLanguages { get; } =
        SupportedCultureCodes.Select(LanguageOption.FromCultureCode).ToList();

    public static LocalizationManager Instance { get; } =
        new(new UserSettingsStore(new AesGcmSettingsProtector()));

    public event PropertyChangedEventHandler? PropertyChanged;

    public event EventHandler? LanguageChanged;

    public string this[string key] =>
        Strings.ResourceManager.GetString(key, _activeCulture) ?? $"[{key}]";

    public LanguageOption CurrentLanguage
    {
        get
        {
            return AvailableLanguages.First(language => language.CultureCode == _activeCulture.Name);
        }
        set
        {
            if (value is not null)
            {
                ChangeLanguage(value.CultureCode);
            }
        }
    }

    private LocalizationManager(UserSettingsStore settingsStore)
    {
        _settingsStore = settingsStore;
        string? savedCultureCode = FindSupportedCultureCode(_settingsStore.Load()?.Language);
        ApplyCulture(savedCultureCode ?? DefaultCultureCode);
    }

    public bool ChangeLanguage(string cultureCode)
    {
        string? supportedCultureCode = FindSupportedCultureCode(cultureCode);
        bool isLanguageSupported = supportedCultureCode is not null;
        bool hasLanguageChanged = false;

        if (supportedCultureCode is not null)
        {
            lock (_cultureChangeLock)
            {
                hasLanguageChanged = supportedCultureCode != _activeCulture.Name;

                if (hasLanguageChanged)
                {
                    ApplyCulture(supportedCultureCode);
                }
            }

            if (hasLanguageChanged)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(AllPropertiesChanged));
                LanguageChanged?.Invoke(this, EventArgs.Empty);

                Task.Run(SaveActiveLanguagePreference);
            }
        }

        return isLanguageSupported;
    }

    // Every localized binding subscribes (weakly) to this singleton, one subscription per bound text. Avalonia only
    // drops the subscriptions of screens that are already gone when the singleton raises the event they listen to,
    // so without this they pile up with every navigation. Re-reading the few dozen texts of the visible screen is
    // cheap.
    public void ReleaseDetachedBindings()
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(AllPropertiesChanged));
    }

    private void SaveActiveLanguagePreference()
    {
        lock (_settingsWriteLock)
        {
            try
            {
                _settingsStore.Save(new UserSettings(_activeCulture.Name));
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                // TODO: Replace the trace with a logger when the language applies to this session but is not saved.
                Trace.TraceWarning($"The language preference could not be saved: {ex.Message}");
            }
        }
    }

    private void ApplyCulture(string cultureCode)
    {
        _activeCulture = CultureInfo.GetCultureInfo(cultureCode);
        CultureInfo.DefaultThreadCurrentCulture = _activeCulture;
        CultureInfo.DefaultThreadCurrentUICulture = _activeCulture;
        CultureInfo.CurrentCulture = _activeCulture;
        CultureInfo.CurrentUICulture = _activeCulture;
    }

    private static string? FindSupportedCultureCode(string? cultureCode)
    {
        return SupportedCultureCodes.FirstOrDefault(
            supportedCode => string.Equals(supportedCode, cultureCode, StringComparison.OrdinalIgnoreCase));
    }
}
